using System;
using System.Reflection;
using PetaPoco;
using bmpxsd;
using log4net;

namespace PillarAPI.IdentifyResponses
{
    internal static class ResponseBuilderForIdentifyPillarsForGetFileIDsRequest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void MakeResponse(MessageInfoContainer message)
        {
            Log.Debug("Making Identityresponce for GetFileID.. ");
            var receivedIdentifyPillarsForGetFileIDsRequest =
                message.MessageObject as IdentifyPillarsForGetFileIDsRequest;
            if (receivedIdentifyPillarsForGetFileIDsRequest == null) throw new ArgumentNullException("message");
            string collectionId = receivedIdentifyPillarsForGetFileIDsRequest.CollectionID;
            var resInfo = new ResponseInfo();
            var timeType = new TimeMeasure_TYPE();
            if (!message.IsSerializedMessageValid)
            {
                resInfo.ResponseCode = ResponseCode.REQUEST_NOT_UNDERSTOOD_FAILURE;
                // IdentifyResponseText is a text for logging the type of identify response given.
                // It does not have a semantic meaning, but contributes to a more readable log.
                resInfo.ResponseText = "Recieved message is not valid";
                timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                timeType.TimeMeasureValue = "0";
            }
            else
            {
                if (!(receivedIdentifyPillarsForGetFileIDsRequest.FileIDs.Item.GetType() == typeof (Object)))
                    // single file
                {
                    string fileName = receivedIdentifyPillarsForGetFileIDsRequest.FileIDs.Item.ToString();
                    var f1 = new FileInfoContainer(collectionId, fileName);

                    if (string.IsNullOrEmpty(f1.FileId))
                    {
                        resInfo.ResponseCode = ResponseCode.IDENTIFICATION_NEGATIVE;
                        resInfo.ResponseText =
                            "This Pillar does not contain the file with the requested fileId. File id request is not possible.";
                        timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                        timeType.TimeMeasureValue = "9999";
                    }
                    else
                    {
                        resInfo.ResponseCode = ResponseCode.IDENTIFICATION_POSITIVE;
                        resInfo.ResponseText = "File is available for get file id request";
                        timeType.TimeMeasureUnit = TimeMeasureUnit.MILLISECONDS;
                        timeType.TimeMeasureValue = "2000";
                    }
                }
                else // All files
                {
                    if (IsCollectionEmpty(collectionId))
                    {
                        resInfo.ResponseCode = ResponseCode.IDENTIFICATION_NEGATIVE;
                        resInfo.ResponseText = "There are no files in the repository";
                        timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                        timeType.TimeMeasureValue = "24";
                    }
                    else
                    {
                        resInfo.ResponseCode = ResponseCode.IDENTIFICATION_POSITIVE;
                        resInfo.ResponseText = "Ready for get all file ids request";
                        timeType.TimeMeasureUnit = TimeMeasureUnit.MILLISECONDS;
                        timeType.TimeMeasureValue = "5000";
                    }
                }
                var responseObject = new IdentifyPillarsForGetFileIDsResponse
                                         {
                                             ResponseInfo = resInfo,
                                             CollectionID = receivedIdentifyPillarsForGetFileIDsRequest.CollectionID,
                                             CorrelationID = receivedIdentifyPillarsForGetFileIDsRequest.CorrelationID,
                                             Destination = receivedIdentifyPillarsForGetFileIDsRequest.ReplyTo,
                                             FileIDs = receivedIdentifyPillarsForGetFileIDsRequest.FileIDs,
                                             From = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                                             minVersion = Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                                             PillarID = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                                             ReplyTo = Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                                             TimeToDeliver = timeType,
                                             To = receivedIdentifyPillarsForGetFileIDsRequest.From,
                                             version = Pillar.GlobalPillarApiSettings.XSD_VERSION
                                         };
                var mic = new MessageInfoContainer(responseObject);
                Log.DebugFormat("MessageInfoContainer to send: \n{0}", mic.SerializedMessage);
                mic.Send();
            }
        }

        private static bool IsCollectionEmpty(string collectionId)
        {
            Sql sqlstring = Sql.Builder
                               .Append("SELECT COUNT(0) fs.archived, fs.file_id, fs.file_spec_id, f.file_name ")
                               .Append("FROM file_specs fs, files f , users u ")
                               .Append("WHERE u.collection_id = @0 ", collectionId)
                               .Append("AND f.user_id = u.user_id ")
                               .Append("AND fs.file_id = f.file_id ")
                               .Append("AND f.deleted = 0 ")
                               .Append("AND fs.active = 1");

            try
            {
                using (Database db = DatabaseConnection.GetConnection())
                {
                    return db.Single<int>(sqlstring) == 0;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
        }
    }
}