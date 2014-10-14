using System;
using System.IO;
using System.Reflection;
using bmpxsd;
using log4net;
using PillarAPI.Interfaces;
using PillarAPI.Utilities;
using WebdavTransfer;

namespace PillarAPI.RequestResponses
{
    /// <summary>
    /// </summary>
    public class GetFileWithWebDav : IGetFile
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(IMessageInfoContainer message)
        {
            Log.DebugFormat("Rammer GetFile");
            var receivedGetFileRequest = message.MessageObject as GetFileRequest;

            if (receivedGetFileRequest != null)
            {
                var uriString = new Uri(receivedGetFileRequest.FileAddress);
                Log.DebugFormat("UriStrengen: {0}", uriString);
                string fileName = receivedGetFileRequest.FileID;
                string collectionId = receivedGetFileRequest.CollectionID;
                //TODO Hvad er collectionDir
                string collectionDir =Pillar.GlobalPillarApiSettings.COLLECTION_FILE_DIRECTORY + receivedGetFileRequest.CollectionID + @"\";

                var f1 = new FileInfoContainer(collectionId, fileName);
                if (string.IsNullOrEmpty(f1.FileId))
                {
                    FileNotOnDiskResponse(receivedGetFileRequest, false); // The file isn't in the Pillar
                }
                else
                {
                    Log.DebugFormat("Er i get og skal hente filen: {0}{1}.{2}.{3}", collectionDir, f1.FileId, f1.FileSpecId, fileName);
                    var filePath =  string.Format("{0}.{1}.{2}", f1.FileId, f1.FileSpecId, fileName);
                    filePath = Path.Combine(f1.FilePath, filePath);
                    if (!File.Exists(filePath)) // File is on Pillar, but not on local disk
                    {
                        FileNotOnDiskResponse(receivedGetFileRequest, true); // The file isn't in the Pillar
                    }
                   MoveFileToWebDav(receivedGetFileRequest, filePath);
                }
            }
        }

        private static void MoveFileToWebDav(GetFileRequest receivedGetFileRequest, string filePath)
        {
            var _webdavSettings = new WebdavTransferSettings(Pillar.GlobalPillarApiSettings.WEBDAV_IP_ADDRESS, Pillar.GlobalPillarApiSettings.WEBDAV_BASEFOLDERNAME, Pillar.GlobalPillarApiSettings.WEBDAV_CLIENT_CERTIFICATE_THUMBPRINT, Pillar.GlobalPillarApiSettings.USER_CERTIFICATES_STORE, Pillar.GlobalPillarApiSettings.WEBDAV_URI_SCHEME, Pillar.GlobalPillarApiSettings.WEBDAV_HTTP_PORT);
            IFileOperator fileOperator = new FileOperator(new ExecutorWithWebdav(_webdavSettings));
            var fileTransferResult = fileOperator.TransferFile(receivedGetFileRequest.FileAddress, new FileInfo(filePath), receivedGetFileRequest.FileID);
            var responseInfo = SetResponseInfo(fileTransferResult);

            var responseObject = new GetFileFinalResponse
            {
                CollectionID = receivedGetFileRequest.CollectionID,
                CorrelationID = receivedGetFileRequest.CorrelationID,
                Destination = receivedGetFileRequest.ReplyTo,
                FileAddress = receivedGetFileRequest.FileAddress,
                FileID = receivedGetFileRequest.FileID,
                From =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                minVersion =Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                PillarID =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                ReplyTo =Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                ResponseInfo = responseInfo,
                To = receivedGetFileRequest.From,
                version =Pillar.GlobalPillarApiSettings.XSD_VERSION
            };

            new MessageInfoContainer(responseObject).Send();
            CollectedUtilities.InsertAudit(FileAction.GET_FILE.ToString(), receivedGetFileRequest.From, receivedGetFileRequest.AuditTrailInformation, receivedGetFileRequest.FileID, responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);
        }

            private static ResponseInfo SetResponseInfo(bool fileTransferResult)
            {
                var responseInfo = new ResponseInfo();
                if (fileTransferResult)
                {
                    responseInfo.ResponseCode = ResponseCode.OPERATION_COMPLETED;
                    responseInfo.ResponseText = "Get file request completed successfully";
                }
                else
                {
                    responseInfo.ResponseCode = ResponseCode.FILE_TRANSFER_FAILURE;
                    responseInfo.ResponseText = "Get file request completed unsuccessfully";
                }
                return responseInfo;
            }

            private static void FileNotOnDiskResponse(GetFileRequest receivedGetFileRequest, bool archived)
        {
            var responseInfo = new ResponseInfo();
            if (archived)
            {
                responseInfo.ResponseCode = ResponseCode.FILE_TRANSFER_FAILURE;
                responseInfo.ResponseText = "File with given FileID is not available for download. Contact SA to have file made available";
            }
            else
            {
                responseInfo.ResponseCode = ResponseCode.FILE_NOT_FOUND_FAILURE;
                responseInfo.ResponseText = "File with given FileID has NOT been found for a read request";
            }
            var responseObject = new GetFileFinalResponse
            {
                CorrelationID = receivedGetFileRequest.CorrelationID,
                CollectionID = receivedGetFileRequest.CollectionID,
                Destination = receivedGetFileRequest.ReplyTo,
                FileAddress = receivedGetFileRequest.FileAddress,
                FileID = receivedGetFileRequest.FileID,
                FilePart = new FilePart(),
                From =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                minVersion = receivedGetFileRequest.minVersion,
                PillarID =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                ReplyTo =Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                ResponseInfo = responseInfo,
                To = receivedGetFileRequest.From,
                version =Pillar.GlobalPillarApiSettings.XSD_VERSION
            };
            new MessageInfoContainer(responseObject).Send();
            CollectedUtilities.InsertAudit(FileAction.GET_FILE.ToString(), receivedGetFileRequest.From, receivedGetFileRequest.AuditTrailInformation, receivedGetFileRequest.FileID, responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);
        }
    }
}