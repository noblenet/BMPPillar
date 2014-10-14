using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using PetaPoco;
using bmpxsd;
using log4net;
using PillarAPI.Interfaces;
using PillarAPI.Models;
using PillarAPI.Utilities;

namespace PillarAPI.RequestResponses
{
    public class ReplaceFile : IReplaceFile
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        public void ProcessRequest(IMessageInfoContainer message)
        {
            var receivedReplaceFileRequest = message.MessageObject as ReplaceFileRequest;
            // call something that handles null messages.
            if (receivedReplaceFileRequest == null) return;

            string basedir =Pillar.GlobalPillarApiSettings.COLLECTION_FILE_DIRECTORY;
            string savedir = basedir + receivedReplaceFileRequest.CollectionID + @"\";
            string filePathNewFile = savedir + receivedReplaceFileRequest.FileID;
            string filePathOldFile = "";
            string collectionId = receivedReplaceFileRequest.CollectionID;
            string fileName = receivedReplaceFileRequest.FileID;
            var responseInfo = new ResponseInfo();
            var f1 = new FileInfoContainer(collectionId, fileName);

            //If there is no file to replace in the repository - The file has never been or no longer is in the repository.
            if (string.IsNullOrEmpty(f1.FileId))
            {
                responseInfo.ResponseCode = ResponseCode.FILE_NOT_FOUND_FAILURE;
                responseInfo.ResponseText = "There is no file to replace in the repository.";
                CollectedUtilities.InsertAudit(FileAction.REPLACE_FILE.ToString(), receivedReplaceFileRequest.From, receivedReplaceFileRequest.AuditTrailInformation, receivedReplaceFileRequest.FileID, responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);
                ReplaceFileResponse(receivedReplaceFileRequest, responseInfo, null, null);
                return;
            }
            filePathOldFile = savedir + f1.FileId + "." + f1.FileSpecId + "." + fileName;

            // Gets checksum data for old file, which is stored in the DB. 
            ChecksumDataForFile_TYPE checksumDataForFileTypeForExistingFile = CollectedUtilities.GetLatestChecksum(collectionId, fileName, receivedReplaceFileRequest.ChecksumDataForExistingFile.ChecksumSpec);

            // If the file is on disk and the checksum isn't on in DB, checksum will be calculated.
            if (File.Exists(filePathOldFile) && checksumDataForFileTypeForExistingFile == null)
            {
                checksumDataForFileTypeForExistingFile = ChecksumUtilities.CalculateChecksumDataForFileType(receivedReplaceFileRequest.ChecksumDataForExistingFile.ChecksumSpec, filePathOldFile);
            }

            // No checksum is available - response will be sent and replace file request terminated.
            if (checksumDataForFileTypeForExistingFile == null)
            {
                Log.Debug("Checksum value isn't kept in DB and cannot be calculated. File is properly kept on optic media");
                responseInfo.ResponseCode = ResponseCode.EXISTING_FILE_CHECKSUM_FAILURE;
                responseInfo.ResponseText = "Checksum value isn't kept in DB and file isn't available. Contact SA and arrange for file to be transfered from storage";
                CollectedUtilities.InsertAudit(FileAction.REPLACE_FILE.ToString(), receivedReplaceFileRequest.From, receivedReplaceFileRequest.AuditTrailInformation, receivedReplaceFileRequest.FileID, responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);
                ReplaceFileResponse(receivedReplaceFileRequest, responseInfo, null, null);
                return;
            }

            // validates whether the checksums are the same or not - 
            if (XmlUtilities.ValidateFileChkSum(receivedReplaceFileRequest.ChecksumDataForExistingFile.ChecksumValue, checksumDataForFileTypeForExistingFile.ChecksumValue))
            {
                // File is being replace by another process
                if (File.Exists(filePathNewFile))
                {
                    Log.Debug("Filen er i gang med at blive udskiftet af en anden proces");
                    responseInfo.ResponseCode = ResponseCode.FAILURE;
                    responseInfo.ResponseText = "File is being replaced by another process";
                    CollectedUtilities.InsertAudit(FileAction.REPLACE_FILE.ToString(), receivedReplaceFileRequest.From, receivedReplaceFileRequest.AuditTrailInformation, receivedReplaceFileRequest.FileID, responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);
                    ReplaceFileResponse(receivedReplaceFileRequest, responseInfo, null, null);
                }
                else
                {
                    Log.Debug("Filen må gerne blive udskiftet!!!");
                    Uri uriStringWebResource = null;
                    var client = new WebClient();
                    try
                    {
                        uriStringWebResource = new Uri(receivedReplaceFileRequest.FileAddress
                            );
                    }
                    catch (Exception e)
                    {
                        Log.Debug("fejl ved replace, 1: " + e);
                        throw;
                    }
                    try
                    {
                        var fileData = new Tuple<string, ReplaceFileRequest>(filePathOldFile, receivedReplaceFileRequest);
                        client.DownloadFileCompleted += CatchReplaceFileEvent;
                        //client.DownloadFileAsync(uriStringWebResource, filePathNewFile, receivedReplaceFileRequest);
                        client.DownloadFileAsync(uriStringWebResource, filePathNewFile, fileData);
                        client.Dispose();
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.ToString());
                        throw;
                    }
                }
            }
            // Checksums are not the same, therefore file wil not be replaced.
            else
            {
                //Log.Debug("Checksumværdierne stemmer ikke overens!");
                responseInfo.ResponseCode = ResponseCode.EXISTING_FILE_CHECKSUM_FAILURE;
                responseInfo.ResponseText = "Checksum for existing file did not match.";
                //skal returnere den nye værdi
                CollectedUtilities.InsertAudit(FileAction.PUT_FILE.ToString(), receivedReplaceFileRequest.From, receivedReplaceFileRequest.AuditTrailInformation, receivedReplaceFileRequest.FileID, responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);
                ReplaceFileResponse(receivedReplaceFileRequest, responseInfo,
                                    receivedReplaceFileRequest.ChecksumRequestForExistingFile != null
                                        ? checksumDataForFileTypeForExistingFile
                                        : null, null);
            }
        }

        private static void CatchReplaceFileEvent(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            ((IDisposable)sender).Dispose();
            var fileData = (Tuple<string, ReplaceFileRequest>)asyncCompletedEventArgs.UserState;
            var receivedReplaceFileRequest = fileData.Item2;
            var oldFilePath = fileData.Item1;
            var savedir =Pillar.GlobalPillarApiSettings.COLLECTION_FILE_DIRECTORY + receivedReplaceFileRequest.CollectionID + @"\";
            var filePath = savedir + receivedReplaceFileRequest.FileID;
            var collectionId = receivedReplaceFileRequest.CollectionID;
            var fileName = receivedReplaceFileRequest.FileID;
            var oldFile = new FileInfoContainer(collectionId, fileName);

            // Checksum data for new file, from client message.
            ChecksumDataForFile_TYPE receivedChecksumDataForNewFile = receivedReplaceFileRequest.ChecksumDataForNewFile;
            // Calculation of checksum data for new file.
            ChecksumDataForFile_TYPE returnChecksumDataForFileType = ChecksumUtilities.CalculateChecksumDataForFileType(receivedChecksumDataForNewFile.ChecksumSpec, filePath);
            var responseInfo = new ResponseInfo();

            try
            {
                if (asyncCompletedEventArgs.Cancelled || asyncCompletedEventArgs.Error != null)
                {
                    throw asyncCompletedEventArgs.Error;
                }

                //Checks checksum for new file against checksum from client
                if (XmlUtilities.ValidateFileChkSum(receivedChecksumDataForNewFile.ChecksumValue, returnChecksumDataForFileType.ChecksumValue))
                {
                    var retval = ExecuteInsertFileSpec(fileName, collectionId, new FileInfo(filePath));
                    if (!retval)
                    {
                        TransactionFailure(filePath, receivedReplaceFileRequest, receivedChecksumDataForNewFile, returnChecksumDataForFileType);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            var f1 = new FileInfoContainer(collectionId, fileName);
            ChecksumType requestedChecksumtype = returnChecksumDataForFileType.ChecksumSpec.ChecksumType;
            var defaultChecksumType = (ChecksumType)Enum.Parse(typeof(ChecksumType),Pillar.GlobalPillarApiSettings.DEFAULT_CHECKSUM_TYPE);

            // Inserts default checksum into DB - if it differs from requested checksumType
            if (requestedChecksumtype != defaultChecksumType)
            {
                var defaultChecksumSpecType = new ChecksumSpec_TYPE { ChecksumType = defaultChecksumType };
                ChecksumDataForFile_TYPE defaultChecksumDataForFileType = ChecksumUtilities.CalculateChecksumDataForFileType(defaultChecksumSpecType, filePath);
                CollectedUtilities.InsertChecksum(int.Parse(f1.FileSpecId), defaultChecksumDataForFileType);
            }

            // Inserts requested checksum into DB
            CollectedUtilities.InsertChecksum(int.Parse(f1.FileSpecId), returnChecksumDataForFileType);
            // Tries to rename file to file_id.file_spec_id.filename
            try
            {
                File.Move(filePath, (savedir + f1.FileId + "." + f1.FileSpecId + "." + f1.FileName));
            }
            catch (IOException ex) // renaming failed - rollback to old file. Clean up DB - delete entry - reinstate old file_spec
            {
                RollbackRenamedFile(f1, oldFile, filePath, receivedReplaceFileRequest, receivedChecksumDataForNewFile, returnChecksumDataForFileType);
                Log.Error("CatcReplaceFileEvent rollback error: ", ex);
                return;
            }
            responseInfo = new ResponseInfo
            {
                ResponseCode = ResponseCode.OPERATION_COMPLETED,
                ResponseText = "replaceFile request has been completed successfully"
            };
            CollectedUtilities.TryToDeleteFile(oldFilePath); // deletes old file
            ReplaceFileResponse(receivedReplaceFileRequest, responseInfo, receivedChecksumDataForNewFile, returnChecksumDataForFileType);



            // The checksum for the replaced file is not the same as the checksum sent by the client. File gets deleted and an error response will be sent to the client.
            responseInfo = new ResponseInfo
                {
                    ResponseCode = ResponseCode.NEW_FILE_CHECKSUM_FAILURE,
                    ResponseText = "Checksum for replaced file does not match"
                };
            CollectedUtilities.TryToDeleteFile(filePath);
            ReplaceFileResponse(receivedReplaceFileRequest, responseInfo, receivedChecksumDataForNewFile, returnChecksumDataForFileType);
        }

        /// <summary>
        /// Rollback the renamed file.
        /// Rollback to old file. Clean up DB - delete entry - reinstate old file_spec
        /// </summary>
        /// <param name="fileInfoContainer">The fileInfoContainer.</param>
        /// <param name="oldFile">The old file.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="receivedReplaceFileRequest">The received replace file request.</param>
        /// <param name="receivedChecksumDataForNewFile">The received checksum data for new file.</param>
        /// <param name="returnChecksumDataForFileType">Type of the return checksum data for file.</param>
        private static void RollbackRenamedFile(FileInfoContainer fileInfoContainer, FileInfoContainer oldFile, string filePath, ReplaceFileRequest receivedReplaceFileRequest, ChecksumDataForFile_TYPE receivedChecksumDataForNewFile, ChecksumDataForFile_TYPE returnChecksumDataForFileType)
        {
            try
            {
                ExecuteDeleteFileSpec(fileInfoContainer);
                ExecuteReactivateFileSpec(oldFile);
                TransactionFailure(filePath, receivedReplaceFileRequest, receivedChecksumDataForNewFile, returnChecksumDataForFileType);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
            
         
        }

        private static void ExecuteReactivateFileSpec(FileInfoContainer oldFile)
        {
            try
            {
                using (var db = DatabaseConnection.GetConnection())
                {
                    using (var trans = db.GetTransaction())
                    {
                        db.Update<FileSpecPoco>("SET active = 1 " + "WHERE file_spec_id = @0 " + "AND file_id = @1 " + "AND active = 0", oldFile.FileSpecId, oldFile.FileId);
                        trans.Complete();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Replace file error. Couldn't update file_specs i DB: ", e);
            }
        }

        private static void ExecuteDeleteFileSpec(FileInfoContainer fileInfoContainer)
        {
            try
            {
                using (var db = DatabaseConnection.GetConnection())
                {
                    using (var trans = db.GetTransaction())
                    {
                        db.Delete<FileSpecPoco>("WHERE file_spec_id = @0 " + "AND file_id = @1 " + "AND active = 1", fileInfoContainer.FileSpecId, fileInfoContainer.FileId);
                        trans.Complete();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Replace file error. Couldn't delete from file_specs in DB: ", e);
                throw;
            }
        }

        private static bool ExecuteInsertFileSpec(string fileName, string collectionId, FileInfo fi)
        {
            var retval = true;
            using (var db = DatabaseConnection.GetConnection())
            {
                var fileSpec = new FileSpecPoco();
                var sql = Sql.Builder
                    .Append("SELECT f.file_id from files f, users u ")
                    .Append("WHERE f.file_name = @0" ,fileName)
                    .Append(" AND f.user_id = u.user_id AND u.collection_id = @0", collectionId);

                try
                {
                    var userPoco = db.SingleOrDefault<FilePoco>(sql);
                    fileSpec.file_size = (int)fi.Length;
                    fileSpec.file_id = userPoco.file_id;
                    fileSpec.filepath = fi.FullName;

                    db.Update(fileSpec);

                }
                catch (Exception e)
                {
                    retval = false;
                    Log.Error(e);
                }
            }
            return retval;
        }


        private static void ReplaceFileResponse(ReplaceFileRequest receivedReplaceFileRequest, ResponseInfo resInf, ChecksumDataForFile_TYPE checksumDataForExistingFile, ChecksumDataForFile_TYPE checksumDataForNewFile)
        {
            var responseObject = new ReplaceFileFinalResponse
                {
                    ResponseInfo = resInf,
                    CorrelationID = receivedReplaceFileRequest.CorrelationID,
                    CollectionID = receivedReplaceFileRequest.CollectionID,
                    Destination = receivedReplaceFileRequest.ReplyTo,
                    FileAddress = receivedReplaceFileRequest.FileAddress,
                    FileID = receivedReplaceFileRequest.FileID,
                    From =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    minVersion = receivedReplaceFileRequest.minVersion,
                    PillarID =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    ReplyTo =Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                    To = receivedReplaceFileRequest.From,
                    version =Pillar.GlobalPillarApiSettings.XSD_VERSION
                };
            if (checksumDataForNewFile != null) responseObject.PillarChecksumSpec = checksumDataForNewFile.ChecksumSpec;
            if (checksumDataForNewFile != null) responseObject.ChecksumDataForNewFile = checksumDataForNewFile;
            if (checksumDataForExistingFile != null) responseObject.ChecksumDataForExistingFile = checksumDataForExistingFile;
            new MessageInfoContainer(responseObject).Send();
        }

        private static void TransactionFailure(string filePath, ReplaceFileRequest receivedReplaceFileRequest, ChecksumDataForFile_TYPE checksumDataForExistingFile, ChecksumDataForFile_TYPE checksumDataForNewFile)
        {
            var responseInfo = new ResponseInfo
                {
                    ResponseCode = ResponseCode.FILE_TRANSFER_FAILURE,
                    ResponseText = "Replace file failed - please try again"
                };
            CollectedUtilities.TryToDeleteFile(filePath);
            CollectedUtilities.InsertAudit(FileAction.REPLACE_FILE.ToString(), receivedReplaceFileRequest.From, receivedReplaceFileRequest.AuditTrailInformation, receivedReplaceFileRequest.FileID, responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);
            ReplaceFileResponse(receivedReplaceFileRequest, responseInfo, checksumDataForExistingFile, checksumDataForNewFile);
        }
    }
}