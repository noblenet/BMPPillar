using System;
using System.Reflection;
using PillarAPI.Enums;
using PillarAPI.Interfaces;
using bmpxsd;
using log4net;

namespace PillarAPI.IdentifyResponses
{
    internal class ResponseBuilderForIdentifyPillarsForGetFileRequest :
        IResponseBuilderForIdentifyPillarsForGetFileRequest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void SendResponse(IMessageInfoContainer message)
        {
            IdentifyPillarsForGetFileResponse identifyPillarsForGetFileResponse = MakeResponse(message);
            new MessageInfoContainer(identifyPillarsForGetFileResponse).Send();
        }

        private IdentifyPillarsForGetFileResponse MakeResponse(IMessageInfoContainer message)
        {
            Log.Debug(message);
            var receivedIdentifyPillarsForGetFileRequest = message.MessageObject as IdentifyPillarsForGetFileRequest;
            if (receivedIdentifyPillarsForGetFileRequest == null) throw new ArgumentNullException("message");
            string collectionId = receivedIdentifyPillarsForGetFileRequest.CollectionID;
            string fileName = receivedIdentifyPillarsForGetFileRequest.FileID;
            var f1 = new FileInfoContainer(collectionId, fileName);
            var timeType = new TimeMeasure_TYPE();

            var resInfo = new ResponseInfo();
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
                if (f1.FileStates != FileStatesEnum.NeverExisted)
                {
                    if (!Convert.ToBoolean(f1.Archived))
                    {
                        resInfo.ResponseCode = ResponseCode.IDENTIFICATION_POSITIVE;
                        resInfo.ResponseText =
                            "Get File request has been received and operation request is expected to be successful";
                        timeType.TimeMeasureUnit = TimeMeasureUnit.MILLISECONDS;
                        timeType.TimeMeasureValue = "3000";
                    }
                    else
                    {
                        resInfo.ResponseCode = ResponseCode.IDENTIFICATION_POSITIVE;
                        resInfo.ResponseText =
                            "Get file request has been received. The file is in storage, contact SA and have them move the file. Then operation request will be successful";
                        timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                        timeType.TimeMeasureValue = "24";
                    }
                }
                else
                {
                    resInfo.ResponseCode = ResponseCode.IDENTIFICATION_NEGATIVE;
                    resInfo.ResponseText = "File with given FileID has NOT been found for a read request";
                    timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                    timeType.TimeMeasureValue = "0";
                }
            }
            return new IdentifyPillarsForGetFileResponse
                       {
                           CollectionID = receivedIdentifyPillarsForGetFileRequest.CollectionID,
                           CorrelationID = receivedIdentifyPillarsForGetFileRequest.CorrelationID,
                           Destination = receivedIdentifyPillarsForGetFileRequest.ReplyTo,
                           FileID = receivedIdentifyPillarsForGetFileRequest.FileID,
                           From = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                           minVersion = Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                           PillarID = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                           ReplyTo = Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                           ResponseInfo = resInfo,
                           TimeToDeliver = timeType,
                           To = receivedIdentifyPillarsForGetFileRequest.From,
                           version = Pillar.GlobalPillarApiSettings.XSD_VERSION
                       };
        }
    }
}