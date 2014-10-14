using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using bmpxsd;
using log4net;
using PillarAPI.Interfaces;
using PillarAPI.Utilities;

namespace PillarAPI.RequestResponses
{
    /// <summary>
    /// </summary>
    public  class GetFile : IGetFile
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
                var client = new WebClient();
                var queryStringCollection = new NameValueCollection();
                string fileName = receivedGetFileRequest.FileID;
                string collectionId = receivedGetFileRequest.CollectionID;
                string collectionDir =Pillar.GlobalPillarApiSettings.COLLECTION_FILE_DIRECTORY + receivedGetFileRequest.CollectionID + @"\";

                var f1 = new FileInfoContainer(collectionId, fileName);
                if (string.IsNullOrEmpty(f1.FileId))
                {
                    GetFileExistsNot(receivedGetFileRequest, false); // The file isn't in the Pillar
                }
                else
                {
                    Log.DebugFormat("Er i get og skal hente filen: {0}{1}.{2}.{3}", collectionDir, f1.FileId, f1.FileSpecId, fileName);
                    string filePath = collectionDir + f1.FileId + "." + f1.FileSpecId + "." + fileName;
                    if (!File.Exists(filePath)) // File is on Pillar, but not on local disk
                    {
                        GetFileExistsNot(receivedGetFileRequest, true); // The file isn't in the Pillar
                    }
                    try
                    {
                        queryStringCollection.Add("FileName", fileName);
                        client.QueryString = queryStringCollection;
                        byte[] rawData = File.ReadAllBytes(filePath);
                        client.UploadDataCompleted += CatchGetFileEvent;
                        client.UploadDataAsync(uriString, null, rawData, receivedGetFileRequest);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        private static void CatchGetFileEvent(object sender, UploadDataCompletedEventArgs e)
        {
            var disposable = sender as IDisposable;
            if (disposable != null) disposable.Dispose();
            var receivedGetFileRequest = (GetFileRequest) e.UserState;
            var responseInfo = new ResponseInfo();
            Log.DebugFormat("Fejl eller noget andet: {0} - {1}", e.Error, e.Cancelled);
            if (!e.Cancelled && e.Error == null)
            {
                responseInfo.ResponseCode = ResponseCode.OPERATION_COMPLETED;
                responseInfo.ResponseText = "Get file request completed successfully";
            }
            else
            {
                responseInfo.ResponseCode = ResponseCode.FILE_TRANSFER_FAILURE;
                responseInfo.ResponseText = "Get file request completed unsuccessfully";
            }

            var responseObject = new GetFileFinalResponse
                {
                    CollectionID = receivedGetFileRequest.CollectionID,
                    CorrelationID = receivedGetFileRequest.CorrelationID,
                    Destination = receivedGetFileRequest.ReplyTo,
                    FileAddress = receivedGetFileRequest.FileAddress,
                    FileID = receivedGetFileRequest.FileID,
                    //FilePart = new FilePart(),
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

        private static void GetFileProgressCallBack(object sender, UploadProgressChangedEventArgs e)
        {
            //TODO måske anvendes
            //Console.WriteLine("Progress: " + e.ProgressPercentage);
        }

        private static void GetFileExistsNot(GetFileRequest receivedGetFileRequest, bool archived)
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