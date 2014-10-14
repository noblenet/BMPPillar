using System;
using bmpxsd;

namespace PillarAPI.IdentifyResponses
{
    internal static class ResponseBuilderForIdentifyContributorsForGetAuditTrailsRequest
    {
        public static void MakeResponse(MessageInfoContainer message)
        {
            var receivedIdentifyPillarsForDeleteFileRequest = message.MessageObject as IdentifyContributorsForGetAuditTrailsRequest;
            if (receivedIdentifyPillarsForDeleteFileRequest == null) throw new ArgumentNullException("message");
            var resInfo = new ResponseInfo();
            if (!message.IsSerializedMessageValid)
            {
                resInfo.ResponseCode = ResponseCode.REQUEST_NOT_UNDERSTOOD_FAILURE;
                // IdentifyResponseText is a text for logging the type of identify response given.
                // It does not have a semantic meaning, but contributes to a more readable log.
                resInfo.ResponseText = "Recieved message is not valid";
            }
            else
            {
                resInfo.ResponseCode = ResponseCode.IDENTIFICATION_POSITIVE;
                resInfo.ResponseText = "Ready for GetAuditTrailsRequest";
            }
            var responseObject = new IdentifyContributorsForGetAuditTrailsResponse
                    {
                        CollectionID = receivedIdentifyPillarsForDeleteFileRequest.CollectionID,
                        CorrelationID = receivedIdentifyPillarsForDeleteFileRequest.CorrelationID,
                        Destination = receivedIdentifyPillarsForDeleteFileRequest.ReplyTo,
                        From = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                        ReplyTo = Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                        ResponseInfo = resInfo,
                        To = receivedIdentifyPillarsForDeleteFileRequest.From,
                        minVersion = Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                        version = Pillar.GlobalPillarApiSettings.XSD_VERSION
                    };
            new MessageInfoContainer(responseObject).Send();
        }
    }
}