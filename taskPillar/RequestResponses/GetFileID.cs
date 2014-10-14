using System;
using System.Linq;
using System.Reflection;
using PetaPoco;
using bmpxsd;
using log4net;
using PillarAPI.Interfaces;
using PillarAPI.Models;
using PillarAPI.Utilities;

namespace PillarAPI.RequestResponses
{
    public class GetFileId : IGetFileId
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(IMessageInfoContainer message)
        {
            var receivedGetFileIDsRequest = message.MessageObject as GetFileIDsRequest;
            if (receivedGetFileIDsRequest == null) return;
            FileIDs fileIDSRequested = receivedGetFileIDsRequest.FileIDs;

            var sql = Sql.Builder;
            sql.Append("SELECT f.file_name, fs.file_size, fs.received");
            sql.Append("FROM file_specs fs, files f, users u ");
            sql.Append("WHERE u.collection_id = @0 ", receivedGetFileIDsRequest.CollectionID);
            sql.Append("AND u.user_id = f.user_id");
            sql.Append("AND fs.file_id = f.file_id ");
            sql.Append("AND fs.active = 1 ");
            sql.Append("AND f.deleted = 0");

            var filename = "AllFileIDs";
            if ((fileIDSRequested.Item.GetType() != typeof (object)))
            {
                filename = fileIDSRequested.Item.ToString();
                sql.Append("AND f.file_name = @0", filename);
            }
            var resultingFileIDsResponse = new ResultingFileIDs();
            var fileIDsDataResponse = new FileIDsData();
            var responseInfo = new ResponseInfo();

            using (var db = DatabaseConnection.GetConnection())
            {

                try
                {
                    var fileIdPocos = db.Query<dynamic>(sql);
                    var rowCount = fileIdPocos.Count();
                    var fileIDsDataItemResponse = new FileIDsDataItem[rowCount];

                    if (fileIdPocos.Any())
                    {
                        if (fileIDSRequested.Item.GetType() == typeof (object)) // get all file ids request
                        {
                            var rowPointer = 0;
                            foreach (var fileId in fileIdPocos)
                            {
                                var fileIDsDataItem1 = new FileIDsDataItem
                                {
                                    FileID = fileId.file_name,
                                    FileSize = fileId.file_size.ToString(),
                                    LastModificationTime = DateTime.Parse(fileId.received.ToString())
                                };
                                fileIDsDataItemResponse[rowPointer] = fileIDsDataItem1;
                                rowPointer++;
                            }
                            responseInfo.ResponseText = "Data for AllFileIDs has been found for Get file ids request";
                        }
                        else
                        {
                            var fileIDsDataItem1 = new FileIDsDataItem
                            {
                                FileID = fileIDSRequested.Item.ToString(),
                                FileSize = fileIdPocos.First().file_size.ToString(),
                                LastModificationTime = DateTime.Parse(fileIdPocos.First().received.ToString())
                            };
                            fileIDsDataItemResponse[0] = fileIDsDataItem1;
                            responseInfo.ResponseText = "Data for a FileID has been found for GetFileIds request";
                        }
                        fileIDsDataResponse.FileIDsDataItems = fileIDsDataItemResponse;
                        resultingFileIDsResponse.Item = fileIDsDataResponse;
                        responseInfo.ResponseCode = ResponseCode.OPERATION_COMPLETED;
                            
                    }
                    else
                    {
                        responseInfo.ResponseCode = ResponseCode.FAILURE;
                        responseInfo.ResponseText = "No files where found for getFileIdsRequest";
                    }
                }
                catch (Exception e)
                {
                    Log.Debug("Exception caught: " + e);
                }
            }
            CollectedUtilities.InsertAudit(FileAction.GET_FILEID.ToString(), receivedGetFileIDsRequest.From, receivedGetFileIDsRequest.AuditTrailInformation, filename, responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);
            
            var responseObject = new GetFileIDsFinalResponse
                {
                    CollectionID = receivedGetFileIDsRequest.CollectionID,
                    CorrelationID = receivedGetFileIDsRequest.CorrelationID,
                    Destination = receivedGetFileIDsRequest.ReplyTo,
                    FileIDs = fileIDSRequested,
                    From =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    minVersion = receivedGetFileIDsRequest.minVersion,
                    PillarID =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    ReplyTo =Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                    ResultingFileIDs = resultingFileIDsResponse,
                    ResponseInfo = responseInfo,
                    To = receivedGetFileIDsRequest.From,
                    version =Pillar.GlobalPillarApiSettings.XSD_VERSION
                };

            new MessageInfoContainer(responseObject).Send();
        }
    }
}