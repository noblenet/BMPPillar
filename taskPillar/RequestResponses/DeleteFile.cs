using System;
using System.IO;
using System.Reflection;
using PetaPoco;
using PillarAPI.Enums;
using PillarAPI.Interfaces;
using PillarAPI.Models;
using PillarAPI.Utilities;
using bmpxsd;
using log4net;

namespace PillarAPI.RequestResponses
{
    public class DeleteFile : IDeleteFile
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(IMessageInfoContainer message)
        {
            var receivedDeleteFileRequestMessage = message.MessageObject as DeleteFileRequest;
            if (receivedDeleteFileRequestMessage == null)
            {
                // call something that handles null messages.
                return;
            }
            ChecksumDataForFile_TYPE receivedChecksumDataForFile =
                receivedDeleteFileRequestMessage.ChecksumDataForExistingFile;
            var responseInfo = new ResponseInfo();

            string collectionId = receivedDeleteFileRequestMessage.CollectionID;
            string fileName = receivedDeleteFileRequestMessage.FileID;
            string savedir = Pillar.GlobalPillarApiSettings.COLLECTION_FILE_DIRECTORY +
                             receivedDeleteFileRequestMessage.CollectionID + @"\";

            var fileInfoContainerForFileToDelete = new FileInfoContainer(collectionId, fileName);

            // File does not exist in Pillar
            if (fileInfoContainerForFileToDelete.FileStates != FileStatesEnum.ExistingInDB)
            {
                Log.Debug("Den er fanget af det tomme fileid");
                responseInfo.ResponseCode = ResponseCode.FILE_NOT_FOUND_FAILURE;
                responseInfo.ResponseText = "File requested for deletion does not exists in SA pillar";
                CollectedUtilities.InsertAudit(FileAction.DELETE_FILE.ToString(), receivedDeleteFileRequestMessage.From,
                                               receivedDeleteFileRequestMessage.AuditTrailInformation,
                                               receivedDeleteFileRequestMessage.FileID, responseInfo.ResponseText,
                                               Pillar.GlobalPillarApiSettings.PILLAR_ID);
                ReturnMessage(receivedDeleteFileRequestMessage, null, responseInfo);
                return;
            }

            string filePath = savedir + fileInfoContainerForFileToDelete.FileId + "." +
                              fileInfoContainerForFileToDelete.FileSpecId + "." + fileName;
            // checks if file is on disk and either calculate or retrieve checksum, if right checksum is in DB.
            ChecksumDataForFile_TYPE checksumDataForFileTypeForFileToDelete = File.Exists(filePath) &&
                                                                              !fileInfoContainerForFileToDelete.Archived
                                                                                  ? ChecksumUtilities
                                                                                        .CalculateChecksumDataForFileType(
                                                                                            receivedChecksumDataForFile
                                                                                                .ChecksumSpec, filePath)
                                                                                  : CollectedUtilities.GetLatestChecksum(
                                                                                      collectionId, fileName,
                                                                                      receivedChecksumDataForFile
                                                                                          .ChecksumSpec);

            // This is to be used if returnchecksum is to calculated from ChecksumRequestForExistingFile.
            // But which of the results should be returned if there are more than one request, 'otherChecksumTypes'?

            if (checksumDataForFileTypeForFileToDelete != null &&
                XmlUtilities.ValidateFileChkSum(checksumDataForFileTypeForFileToDelete.ChecksumValue,
                                                receivedChecksumDataForFile.ChecksumValue))
            {
                using (Database myDb = DatabaseConnection.GetConnection())
                {
                    int retval =
                        myDb.Update<FilePoco>(
                            "SET deleted = @deleted WHERE file_name = @file_name and user_id = (SELECT user_id FROM users WHERE collection_id = @collection_id)",
                            1, fileName, collectionId);
                    // Verify retval value? Hvad returnerer petapoco update
                    try
                    {
                        if (retval >= 1)
                        {
                            responseInfo.ResponseCode = ResponseCode.OPERATION_COMPLETED;
                            responseInfo.ResponseText =
                                "Delete file request has been received and executed successfully";
                        }
                        else
                        {
                            responseInfo.ResponseCode = ResponseCode.FAILURE;
                            responseInfo.ResponseText =
                                "Delete file request has been received. There is an error in the database, contact SA";
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception);
                        responseInfo.ResponseCode = ResponseCode.FAILURE;
                        responseInfo.ResponseText =
                            "Delete file request has been received. There is an error in the database, contact SA";
                    }
                }

                CollectedUtilities.TryToDeleteFile(filePath);
                ReturnMessage(receivedDeleteFileRequestMessage, checksumDataForFileTypeForFileToDelete, responseInfo);
            }
                // The file did not pass checksum validation - Return an error message to client.
            else
            {
                responseInfo.ResponseCode = ResponseCode.EXISTING_FILE_CHECKSUM_FAILURE;
                if (fileInfoContainerForFileToDelete.Archived && !File.Exists(filePath))
                {
                    responseInfo.ResponseText =
                        "File deletion denied. File has been archived and no checksum for requested checksum type i available.";
                }
                else
                {
                    responseInfo.ResponseText = "File requested for deletion did not pass checksum validation";
                }
                ReturnMessage(receivedDeleteFileRequestMessage, checksumDataForFileTypeForFileToDelete, responseInfo);
            }
            CollectedUtilities.InsertAudit(FileAction.DELETE_FILE.ToString(), receivedDeleteFileRequestMessage.From,
                                           receivedDeleteFileRequestMessage.AuditTrailInformation,
                                           receivedDeleteFileRequestMessage.FileID, responseInfo.ResponseText,
                                           Pillar.GlobalPillarApiSettings.PILLAR_ID);
        }

        private static void ReturnMessage(DeleteFileRequest deleteFileRequest,
                                          ChecksumDataForFile_TYPE responseChecksumDataForFile,
                                          ResponseInfo responseInfo)
        {
            var responseObject = new DeleteFileFinalResponse
                                     {
                                         ChecksumDataForExistingFile = responseChecksumDataForFile,
                                         CollectionID = deleteFileRequest.CollectionID,
                                         CorrelationID = deleteFileRequest.CorrelationID,
                                         FileID = deleteFileRequest.FileID,
                                         From = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                                         PillarID = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                                         ReplyTo = Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                                         ResponseInfo = responseInfo,
                                         To = deleteFileRequest.ReplyTo,
                                         minVersion = Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                                         version = Pillar.GlobalPillarApiSettings.XSD_VERSION
                                     };

            new MessageInfoContainer(responseObject).Send();
        }
    }
}