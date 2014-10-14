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

namespace PillarAPI.RequestResponses
{
    /// <summary>
    /// </summary>
    public class ChecksumPutFile : IPutFile
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
                    // Stops filetransfer if file already exists
                    if (putFileFileInfoContainer.FileStates == FileStatesEnum.ExistingInDB)
                    {
                        DuplicateFileFailureResponse(receivedPutFileRequest);
                        return;
                    }
                    // File is in db, but is marked as deleted - files table is updated.
                    var updateCmd = UpdateFile(putFileFileInfoContainer.FileName, putFileFileInfoContainer.CollectionId);
                    // if DB update fails, filetransfer is aborted. - do we wanna try once more before quitting?
                    if (!updateCmd) ExecuteNonQuerySqlTransactionFailed(receivedPutFileRequest);
                }
                else
                {
                    // Insert file in files table in DB
                    var insertFileCmd = InsertFileIntoDb(fileName, collectionId);
                    // If DB insert fails, filetransfer is aborted. - do we wanna try once more before quitting?
                    if (!insertFileCmd) ExecuteNonQuerySqlTransactionFailed(receivedPutFileRequest);
                }
                InsertFileInfoIntoDB(receivedPutFileRequest);
            }
            catch (Exception e)
            {
                // DB insert fails, filetransfer is aborted.
                Log.Error("PutFiles. DB transaction failed: ", e);
                ExecuteNonQuerySqlTransactionFailed(receivedPutFileRequest);
            }
        }

        private static void ExecuteNonQuerySqlTransactionFailed(PutFileRequest receivedPutFileRequest)
        {
            var responseInfo = new ResponseInfo
            {
                ResponseCode = ResponseCode.FAILURE,
                ResponseText = "PutFile failed"
            };
            CollectedUtilities.InsertAudit(FileAction.PUT_FILE.ToString(), receivedPutFileRequest.From,
                                           receivedPutFileRequest.AuditTrailInformation, receivedPutFileRequest.FileID,
                                           responseInfo.ResponseText, Pillar.GlobalPillarApiSettings.PILLAR_ID);
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
                                           responseInfo.ResponseText, Pillar.GlobalPillarApiSettings.PILLAR_ID);
            PutFileReply(receivedPutFileRequest, responseInfo, null);
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

        private static void InsertFileInfoIntoDB(PutFileRequest receivedPutFileRequest)
        {
            ////////////////////////////////////////////////////////////////
            // Tilføj funktionalitet der håndterer e.error og e.cancelled //
            ////////////////////////////////////////////////////////////////
            string fileName = receivedPutFileRequest.FileID;
            string collectionId = receivedPutFileRequest.CollectionID;
            var responseInfo = new ResponseInfo();

            try
            {
                    var newFileSpecId = InsertNewFileSpec(fileName, collectionId);
                    // Inserts requested checksum into DB
                    CollectedUtilities.InsertChecksum(newFileSpecId, receivedPutFileRequest.ChecksumDataForNewFile);
                    responseInfo.ResponseCode = ResponseCode.OPERATION_COMPLETED;
                    responseInfo.ResponseText = "Putfile request has been completed successfully";
                    PutFileReply(receivedPutFileRequest, responseInfo, receivedPutFileRequest.ChecksumDataForNewFile);
            }
            catch (Exception fileTransferError)
            {
                Log.Error(fileTransferError);
                // Cleans up after attempted filetransfer. DB - file_spec and files gets updated.
                using (var db = DatabaseConnection.GetConnection())
                {
                    db.Delete<FilePoco>("DELETE FROM files WHERE file_name = @0 and user_id = (SELECT user_id FROM users WHERE collection_id = @1)", fileName, collectionId);
                }
                responseInfo.ResponseCode = ResponseCode.FILE_TRANSFER_FAILURE;
                responseInfo.ResponseText = "PutFile failed";
                PutFileReply(receivedPutFileRequest, responseInfo, null);
                Log.Error("CatchPutFileEvent cleanup, file transfer error: ", fileTransferError);
            }
            CollectedUtilities.InsertAudit(FileAction.PUT_FILE.ToString(), receivedPutFileRequest.From, receivedPutFileRequest.AuditTrailInformation, receivedPutFileRequest.FileID, responseInfo.ResponseText, Pillar.GlobalPillarApiSettings.PILLAR_ID);
        }

        private static int InsertNewFileSpec(string fileName, string collectionId)
        {
            using (var db = DatabaseConnection.GetConnection())
            {
                using (var trans = db.GetTransaction())
                {
                    // insert new file_spec in database and renames file - Remember to check filesize against receivedFileChkData filesize
                    var fi = new FileInfo(fileName);

                    var sqlstring = Sql.Builder
                        .Append("SELECT f.file_id from files f ")
                        .Append("LEFT JOIN users u ")
                        .Append("WHERE f.file_name = @0 ", fileName)
                        .Append("AND f.user_id = u.user_id ")
                        .Append("AND u.collection_id = @0", collectionId);

                    var filePoco = db.SingleOrDefault<FilePoco>(sqlstring);
                    var fileSpec = new FileSpecPoco
                    {
                        active = true,
                        file_id = filePoco.file_id,
                        file_size = 0,
                        filepath = string.Empty,
                    };
                    var result = db.Insert(fileSpec);
                    trans.Complete();
                    return Convert.ToInt32(result);
                }
            }
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
                From = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                PillarChecksumSpec = putFileRequestMessage.ChecksumRequestForNewFile,
                PillarID = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                ReplyTo = Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                ResponseInfo = responseInfo,
                To = putFileRequestMessage.From,
                minVersion = Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                version = Pillar.GlobalPillarApiSettings.XSD_VERSION
            };
            if (ChecksumDataForNewFile != null) responseObject.ChecksumDataForNewFile = ChecksumDataForNewFile;
            new MessageInfoContainer(responseObject).Send();
        }
    }
}