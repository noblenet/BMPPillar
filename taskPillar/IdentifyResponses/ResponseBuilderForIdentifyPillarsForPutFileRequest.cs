using System;
using bmpxsd;

namespace PillarAPI.IdentifyResponses
{
    internal static class ResponseBuilderForIdentifyPillarsForPutFileRequest
    {
        internal static void MakeResponse(MessageInfoContainer message)
        {
            // DB understøttelse

            var receivedIdentifyPillarsForPutFileRequest = message.MessageObject as IdentifyPillarsForPutFileRequest;
            if (receivedIdentifyPillarsForPutFileRequest == null) throw new ArgumentNullException("message");
            string collectionId = receivedIdentifyPillarsForPutFileRequest.CollectionID;
            string fileName = receivedIdentifyPillarsForPutFileRequest.FileID;
            var f1 = new FileInfoContainer(collectionId, fileName);
            var timeType = new TimeMeasure_TYPE();
            // Response information telling that the Identify request has been received and
            // if an operation request can be met. This response info can be used to send 'negative' Identify responses.
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
                if (string.IsNullOrEmpty(f1.FileId))
                {
                    resInfo.ResponseCode = ResponseCode.IDENTIFICATION_POSITIVE;
                    // IdentifyResponseText is a text for logging the type of identify response given.
                    // It does not have a semantic meaning, but contributes to a more readable log.
                    resInfo.ResponseText = "Message request has been received and operation request is expected to be successful";
                    timeType.TimeMeasureUnit = TimeMeasureUnit.MILLISECONDS;
                    timeType.TimeMeasureValue = "3000";
                }
                else
                {
                    resInfo.ResponseCode = ResponseCode.DUPLICATE_FILE_FAILURE;
                    resInfo.ResponseText = "Duplicate FileID in put file request";
                    timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                    timeType.TimeMeasureValue = "0";
                }
            }

            // Can be places in a global Pillar object at some time
            var chkSpk = new ChecksumSpec_TYPE();
            var chkType = (ChecksumType)Enum.Parse(typeof(ChecksumType),Pillar.GlobalPillarApiSettings.DEFAULT_CHECKSUM_TYPE);
            chkSpk.ChecksumType = chkType;

            var responseObjectV24 = new IdentifyPillarsForPutFileResponse
                {
                    CollectionID = receivedIdentifyPillarsForPutFileRequest.CollectionID,
                    CorrelationID = receivedIdentifyPillarsForPutFileRequest.CorrelationID,
                    Destination = receivedIdentifyPillarsForPutFileRequest.ReplyTo,
                    FileID = receivedIdentifyPillarsForPutFileRequest.FileID,
                    From =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    PillarChecksumSpec = chkSpk,
                    PillarID =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    ReplyTo =Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                    ResponseInfo = resInfo,
                    TimeToDeliver = timeType,
                    To = receivedIdentifyPillarsForPutFileRequest.From,
                    minVersion =Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                    version =Pillar.GlobalPillarApiSettings.XSD_VERSION
                };
            //responseObjectV24.ChecksumDataForExistingFile = ; // Checksum for the file in case a file with the supplied fileID already exists in the pillar.

            var a = new MessageInfoContainer(responseObjectV24);
            a.Send();
        }
    }
}