using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using bmpxsd;
using log4net;
using PillarAPI.Models;
using PillarAPI.Utilities;

namespace PillarAPI.IdentifyResponses
{
    internal static class ResponseBuilderForIdentifyPillarsForDeleteFileRequest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void MakeResponse(MessageInfoContainer message)
        {
            var receivedIdentifyPillarsForDeleteFileRequest = message.MessageObject as IdentifyPillarsForDeleteFileRequest;
            if (receivedIdentifyPillarsForDeleteFileRequest == null) throw new ArgumentNullException("message");
            string collectionId = receivedIdentifyPillarsForDeleteFileRequest.CollectionID;
            string fileName = receivedIdentifyPillarsForDeleteFileRequest.FileID;
            var f1 = new FileInfoContainer(collectionId, fileName);
            IEnumerable<ChecksumsTypePoco> algorithms = CollectedUtilities.GetPillarChecksumTypes();
            var checksumType = new ChecksumType();
            string result = null;
            int start = 1;
            if (algorithms != null)
            {
                int end = algorithms.Count();
                foreach (ChecksumsTypePoco checksumsType in algorithms)
                {
                    if (start > 1)
                    {
                        result = result + checksumsType.algorithm;
                        result += (start < end) ? "," : string.Empty;
                    }
                    else
                    {
                        checksumType = (ChecksumType)Enum.Parse(typeof(ChecksumType), checksumsType.algorithm);
                    }
                    start++;
                }
            }
            Log.Debug(result);
            var checkType = new ChecksumSpec_TYPE
                {
                    ChecksumType = checksumType,
                    OtherChecksumType = result
                };
            var resInfo = new ResponseInfo();
            var timeType = new TimeMeasure_TYPE();
            if (!message.IsSerializedMessageValid)
            {
                resInfo.ResponseCode = ResponseCode.REQUEST_NOT_UNDERSTOOD_FAILURE;
                resInfo.ResponseText = "Recieved message is not valid";
                timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                timeType.TimeMeasureValue = "0";
            }
            else
            {

                if (!string.IsNullOrEmpty(f1.FileName))
                {
                    resInfo.ResponseCode = ResponseCode.IDENTIFICATION_POSITIVE;
                    resInfo.ResponseText = "Message request has been received and operation request is expected to be successful";
                    timeType.TimeMeasureUnit = TimeMeasureUnit.MILLISECONDS;
                    timeType.TimeMeasureValue = "3000";
                }
                else
                {
                    resInfo.ResponseCode = ResponseCode.IDENTIFICATION_NEGATIVE;
                    resInfo.ResponseText = "File with given FileID has NOT been found for a delete file request";
                    timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                    timeType.TimeMeasureValue = "0";
                }
            }
            var responseObject = new IdentifyPillarsForDeleteFileResponse
                {
                    CollectionID = receivedIdentifyPillarsForDeleteFileRequest.CorrelationID,
                    CorrelationID = receivedIdentifyPillarsForDeleteFileRequest.CorrelationID,
                    Destination = receivedIdentifyPillarsForDeleteFileRequest.ReplyTo,
                    FileID = receivedIdentifyPillarsForDeleteFileRequest.FileID,
                    From = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    minVersion = Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                    PillarChecksumSpec = checkType,
                    PillarID = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    ReplyTo = Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                    ResponseInfo = resInfo,
                    TimeToDeliver = timeType,
                    To = receivedIdentifyPillarsForDeleteFileRequest.From,
                    version = Pillar.GlobalPillarApiSettings.XSD_VERSION
                };

            new MessageInfoContainer(responseObject).Send();
        }
    }
}