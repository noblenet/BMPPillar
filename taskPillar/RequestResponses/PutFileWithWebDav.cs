using System;
using System.IO;
using System.Reflection;
using PetaPoco;
using bmpxsd;
using log4net;
using PillarAPI.Enums;
using PillarAPI.Interfaces;
using PillarAPI.Models;
using PillarAPI.Utilities;
using StorageHandler;
using WebdavTransfer;

namespace PillarAPI.RequestResponses
{
    public class PutFileWithWebDav : IPutFile
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// ++++++++++++ Tjekkes der for om en anden proces er ved at lægge den samme fil op? +++++++++++++++
        public void ProcessRequest(IMessageInfoContainer message)
        {
            var receivedPutFileRequest = message.MessageObject as PutFileRequest;
            string fileName = receivedPutFileRequest.FileID;
            string collectionId = receivedPutFileRequest.CollectionID;

            try
            {
                var putFileFileInfoContainer = new FileInfoContainer(collectionId, fileName);

                if (putFileFileInfoContainer.FileStates == FileStatesEnum.ExistingInDB || putFileFileInfoContainer.FileStates == FileStatesEnum.DeletedFromDB)
                {
                    UpdateFileInDB(putFileFileInfoContainer, receivedPutFileRequest);
                }
                else
                {
                    // If DB insert fails, filetransfer is aborted. - do we wanna try once more before quitting?
                    if (InsertFileIntoDb(fileName, collectionId))
                    {
                        MoveFileFromWebDavToLocalDirectoryNew(receivedPutFileRequest);
                        return;
                    }

                    ExecuteNonQuerySqlTransactionFailed(receivedPutFileRequest);
                }
            }
            catch (Exception e)
            {
                // DB insert fails, filetransfer is aborted.
                Log.Error("PutFiles. DB transaction failed: ", e);
                ExecuteNonQuerySqlTransactionFailed(receivedPutFileRequest);
                Utilities.DBUtilities.CleanFilesFromDBIfLeftHanging();
            }
        }

        private static void UpdateFileInDB(FileInfoContainer putFileFileInfoContainer, PutFileRequest receivedPutFileRequest)
        {
            // Stops filetransfer if file already exists
            if (putFileFileInfoContainer.FileStates != FileStatesEnum.ExistingInDB)
            {
                // if DB update fails, filetransfer is aborted. - do we wanna try once more before quitting?
                if (!UpdateFile(putFileFileInfoContainer.FileName, putFileFileInfoContainer.CollectionId)) ExecuteNonQuerySqlTransactionFailed(receivedPutFileRequest);
            }
            else
            {
                DuplicateFileFailureResponse(receivedPutFileRequest);
            }
        }

        private static void ExecuteNonQuerySqlTransactionFailed(PutFileRequest receivedPutFileRequest)
        {
            Utilities.DBUtilities.CleanFilesFromDBIfLeftHanging();
              
            var responseInfo = new ResponseInfo
                {
                    ResponseCode = ResponseCode.FAILURE,
                    ResponseText = "PutFile failed"
                };
            CollectedUtilities.InsertAudit(FileAction.PUT_FILE.ToString(), receivedPutFileRequest.From,
                                           receivedPutFileRequest.AuditTrailInformation, receivedPutFileRequest.FileID,
                                           responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);
            PutFileReply(receivedPutFileRequest, responseInfo, null);
        }

        private static bool InsertFileIntoDb(string fileName, string collectionId)
        {
            bool inserted = true;

            using (var db = DatabaseConnection.GetConnection())
            {
                var file = new FilePoco();
                var userPoco = db.SingleOrDefault<FilePoco>(string.Format("SELECT user_id FROM users WHERE collection_id = '{0}'", collectionId));

                file.file_name = fileName;
                file.user_id = userPoco.user_id;

                try
                {
                    using (var trans = db.GetTransaction())
                    {
                        db.Insert(file);
                        trans.Complete();
                    }
                }
                catch (Exception e)
                {
                    inserted = false;
                    Log.Error("InsertFile into DB error: ", e);
                }
            }
            return inserted;
        }

        private static void DuplicateFileFailureResponse(PutFileRequest receivedPutFileRequest)
        {
            var responseInfo = new ResponseInfo
                {
                    ResponseCode = ResponseCode.DUPLICATE_FILE_FAILURE,
                    ResponseText = "Duplicate FileID in put file request"
                };
            CollectedUtilities.InsertAudit(FileAction.PUT_FILE.ToString(), receivedPutFileRequest.From,
                                           receivedPutFileRequest.AuditTrailInformation, receivedPutFileRequest.FileID,
                                           responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);
            PutFileReply(receivedPutFileRequest, responseInfo, null);
        }

        private static void MoveFileFromWebDavToLocalDirectoryNew(PutFileRequest receivedPutFileRequest)
        {
            Log.DebugFormat("MoveFileFromWebDavToLocalDirectoryNew called with '{0}'", receivedPutFileRequest);

            string fileName = receivedPutFileRequest.FileID;
            string collectionId = receivedPutFileRequest.CollectionID;
            IHandler storageHandler = new Handler(Pillar.GlobalPillarApiSettings.DRIVES4STORAGE);
            var saveDir = storageHandler.GetNewLocation();
            var filePath = Path.Combine(saveDir, receivedPutFileRequest.FileID);
            var filePathInfo = new FileInfo(filePath);
            var webdavSettings = new WebdavTransferSettings(Pillar.GlobalPillarApiSettings.WEBDAV_IP_ADDRESS, Pillar.GlobalPillarApiSettings.WEBDAV_BASEFOLDERNAME, Pillar.GlobalPillarApiSettings.WEBDAV_CLIENT_CERTIFICATE_THUMBPRINT, Pillar.GlobalPillarApiSettings.USER_CERTIFICATES_STORE, Pillar.GlobalPillarApiSettings.WEBDAV_URI_SCHEME, Pillar.GlobalPillarApiSettings.WEBDAV_HTTP_PORT);
           
            IFileOperator fileOperator = new FileOperator(new ExecutorWithWebdav(webdavSettings));
            var fileTranferResult = fileOperator.GetFile(receivedPutFileRequest.FileAddress, filePathInfo);
            var folderDeleteResult = fileOperator.DeleteFolder(new UriBuilder(receivedPutFileRequest.FileAddress));
            if (!fileTranferResult)
            {
                throw new WTFException();
            }
            ChecksumDataForFile_TYPE receivedFileChkData = receivedPutFileRequest.ChecksumDataForNewFile;
            ChecksumDataForFile_TYPE returnChecksumDataForFileType = ChecksumUtilities.CalculateChecksumDataForFileType(receivedFileChkData.ChecksumSpec, filePath);
            var responseInfo = new ResponseInfo();
            try
            {
         
                // TODO - Validation should be done from MoveFileFromWebDavToLocalDirectory
                if (XmlUtilities.ValidateFileChkSum(receivedFileChkData.ChecksumValue, returnChecksumDataForFileType.ChecksumValue))
                {
                    SaveFileSpecInDb(fileName, collectionId, filePathInfo);

                    var f1 = new FileInfoContainer(collectionId, fileName);
                    SaveChecksumsIntoDb(filePath, f1, returnChecksumDataForFileType);
                    // Tries to rename file to file_id.file_spec_id.filename
                    try
                    {
                        var newFileName = Path.Combine(saveDir,string.Format("{0}.{1}.{2}", f1.FileId, f1.FileSpecId, receivedPutFileRequest.FileID));
                        storageHandler.RenameFile(filePathInfo, newFileName);
                    }
                    catch (IOException ex)
                    {
                        // Rename failed, reaction?
                        Log.Error(ex); // Write error
                    }
                    responseInfo.ResponseCode = ResponseCode.OPERATION_COMPLETED;
                    responseInfo.ResponseText = "Putfile request has been completed successfully";
                    PutFileReply(receivedPutFileRequest, responseInfo, returnChecksumDataForFileType);
                }
                else // File is not valid, delete file and set deleted in files to true.
                {
                    using (var db = DatabaseConnection.GetConnection())
                    {
                        using (var trans = db.GetTransaction())
                        {
                            try
                            {
                                var user = db.SingleOrDefault<dynamic>(Sql.Builder.Append("SELECT user_id FROM users WHERE collection_id = @0", collectionId));
                                db.Delete<FilePoco>("WHERE file_name = @0 and user_id = @1", fileName, user.user_id);
                                trans.Complete();
                            }
                            catch (Exception e)
                            {
                                Log.Error("Fejler her 2");
                                Log.Error(e);
                            }

                            responseInfo.ResponseCode = ResponseCode.NEW_FILE_CHECKSUM_FAILURE;
                            responseInfo.ResponseText = "Oploaded file did not pass checksum validation";
                            PutFileReply(receivedPutFileRequest, responseInfo, null);

                            CollectedUtilities.TryToDeleteFile(filePath);
                        }
                    }
                }
            }
            // TODO - Move to method
            catch (Exception fileTransferError)
            {
                Log.Error(fileTransferError);
                // Cleans up after attempted filetransfer. DB - file_spec and files gets updated.
                using (var db = DatabaseConnection.GetConnection())
                {
                    db.Execute("DELETE FROM files WHERE file_name = @0 and user_id = (SELECT user_id FROM users WHERE collection_id = @1)", fileName, collectionId);
                }
                responseInfo.ResponseCode = ResponseCode.FILE_TRANSFER_FAILURE;
                responseInfo.ResponseText = "PutFile failed";
                PutFileReply(receivedPutFileRequest, responseInfo, null);
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception fileDeleteError)
                {
                    Log.Error("File delete error: ", fileDeleteError);
                }
                Log.Error("File transfer error: ", fileTransferError);
            }
            // TODO - Individual methods needs to call this.
            CollectedUtilities.InsertAudit(FileAction.PUT_FILE.ToString(), receivedPutFileRequest.From, receivedPutFileRequest.AuditTrailInformation, receivedPutFileRequest.FileID, responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);
        }

        private static void SaveChecksumsIntoDb(string filePath, FileInfoContainer f1, ChecksumDataForFile_TYPE returnChecksumDataForFileType)
        {
            ChecksumType requestedChecksumtype = returnChecksumDataForFileType.ChecksumSpec.ChecksumType;
            var defaultChecksumType = (ChecksumType)Enum.Parse(typeof(ChecksumType),Pillar.GlobalPillarApiSettings.DEFAULT_CHECKSUM_TYPE);
            if (requestedChecksumtype != defaultChecksumType)
            {
                var defaultChecksumSpecType = new ChecksumSpec_TYPE { ChecksumType = defaultChecksumType };
                ChecksumDataForFile_TYPE defaultChecksumDataForFileType = ChecksumUtilities.CalculateChecksumDataForFileType(defaultChecksumSpecType, filePath);
                CollectedUtilities.InsertChecksum(int.Parse(f1.FileSpecId), defaultChecksumDataForFileType);
            }
            CollectedUtilities.InsertChecksum(int.Parse(f1.FileSpecId), returnChecksumDataForFileType);
        }

        private static bool UpdateFile(string fileName, string collectionId)
        {
            var retval = true;
            using (var db = DatabaseConnection.GetConnection())
            {
                var file = new FilePoco();

                var userPoco = db.SingleOrDefault<FilePoco>(Sql.Builder.Append("SELECT user_id FROM users WHERE collection_id = @0", collectionId));
                file.file_id = userPoco.file_id;
                file.file_name = fileName;
                file.deleted = Convert.ToBoolean(0);
                using (var trans = db.GetTransaction())
                {
                    try
                    {
                        db.Update(file);
                        trans.Complete();
                    }

                    catch (Exception e)
                    {
                        db.AbortTransaction();
                        retval = false;
                        Log.Error("UpdateFile into DB error: ", e);
                    }
                }
            }
            return retval;
        }

        private static void SaveFileSpecInDb(string fileName, string collectionId, FileInfo filePath)
        {
            using (var db = DatabaseConnection.GetConnection())
            {
                using (var trans = db.GetTransaction())
                {
                    try
                    {
                        // insert new file_spec in database and renames file - Remember to check filesize against receivedFileChkData filesize
                        MakeFileSpecPoco(db, fileName, collectionId, filePath);
                        trans.Complete();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        throw;
                    }
                }
            }
        }

        private static void MakeFileSpecPoco(Database db, string fileName, string collectionId, FileInfo filePath2Sourcefile)
        {
            var filePoco = db.SingleOrDefault<FilePoco>(GetFileId(fileName, collectionId));

           
            var fileSpec = new FileSpecPoco
            {
                active = true,
                file_id = filePoco.file_id,
                file_size = (int)filePath2Sourcefile.Length,
                filepath = filePath2Sourcefile.DirectoryName
            };
            db.Insert(fileSpec);
        }

        private static Sql GetFileId(string fileName, string collectionId)
        {
            var sqlstring = Sql.Builder
                .Append("SELECT f.file_id from files f ")
                .Append("LEFT JOIN users u ")
                .Append("WHERE f.file_name = @0 ", fileName)
                .Append("AND f.user_id = u.user_id ")
                .Append("AND u.collection_id = @0", collectionId);
            return sqlstring;
        }

        /// <summary>
        ///     bla
        /// </summary>
        /// <param name="putFileRequestMessage"></param>
        /// <param name="responseInfo"> </param>
        private static void PutFileReply(PutFileRequest putFileRequestMessage, ResponseInfo responseInfo, ChecksumDataForFile_TYPE ChecksumDataForNewFile)
        {
            var responseObject = new PutFileFinalResponse
                {
                    //ChecksumDataForExistingFile // If file already exists in repository - why god, WHY. We already have the integrityservice to take care of this.
                    ChecksumDataForNewFile = ChecksumDataForNewFile,
                    CollectionID = putFileRequestMessage.CollectionID,
                    CorrelationID = putFileRequestMessage.CorrelationID,
                    Destination = putFileRequestMessage.ReplyTo,
                    FileAddress = putFileRequestMessage.FileAddress,
                    FileID = putFileRequestMessage.FileID,
                    From =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    PillarChecksumSpec = putFileRequestMessage.ChecksumRequestForNewFile,
                    PillarID =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    ReplyTo =Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                    ResponseInfo = responseInfo,
                    To = putFileRequestMessage.From,
                    minVersion =Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                    version =Pillar.GlobalPillarApiSettings.XSD_VERSION
                };
            if (ChecksumDataForNewFile != null) responseObject.ChecksumDataForNewFile = ChecksumDataForNewFile;
            new MessageInfoContainer(responseObject).Send();
        }
    }

    internal class WTFException : Exception
    {
    }
}