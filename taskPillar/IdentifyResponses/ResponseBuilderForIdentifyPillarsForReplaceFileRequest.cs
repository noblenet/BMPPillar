using System;
using System.Collections.Generic;
using System.Linq;
using bmpxsd;
using PillarAPI.Models;
using PillarAPI.Utilities;

namespace PillarAPI.IdentifyResponses
{
    internal static class ResponseBuilderForIdentifyPillarsForReplaceFileRequest
    {
        public static void MakeResponse(MessageInfoContainer message)
        {
            var receivedIdentifyPillarsForReplaceFileRequest = message.MessageObject as IdentifyPillarsForReplaceFileRequest;
            if (receivedIdentifyPillarsForReplaceFileRequest == null) throw new ArgumentNullException("message");
            string collectionId = receivedIdentifyPillarsForReplaceFileRequest.CollectionID;
            string fileName = receivedIdentifyPillarsForReplaceFileRequest.FileID;
            var resInfo = new ResponseInfo();
            //filePathOldFile = savedir + f1.FileId + "." + f1.FileSpecId + "." + fileName;

            // findes filen? er den på disk? Er tjeksum rigtig?
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
                var f1 = new FileInfoContainer(collectionId, fileName);
                //If there is no file to replace in the repository - The file has never been or no longer is in the repository.
                if (string.IsNullOrEmpty(f1.FileId))
                {
                    resInfo.ResponseCode = ResponseCode.FILE_NOT_FOUND_FAILURE;
                    resInfo.ResponseText = "There is no file to replace in the repository";
                    timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                    timeType.TimeMeasureValue = "0";
                }
                else
                {
                    resInfo.ResponseCode = ResponseCode.IDENTIFICATION_POSITIVE;
                    resInfo.ResponseText = "The file can be replaced";
                    timeType.TimeMeasureUnit = TimeMeasureUnit.MILLISECONDS;
                    timeType.TimeMeasureValue = "3000";
                }
            }
            IEnumerable<ChecksumsTypePoco> algorithms = CollectedUtilities.GetPillarChecksumTypes();
            var checksumType = new ChecksumType();
            string otherChecksumTypes = null;
            int start = 1;
            if (algorithms != null)
            {
                int end = algorithms.Count();
                foreach (ChecksumsTypePoco checksumsType in algorithms)
                {
                    if (start > 1)
                    {
                        otherChecksumTypes = otherChecksumTypes + checksumsType.algorithm;
                        otherChecksumTypes += (start < end) ? "," : string.Empty;
                    }
                    else
                    {
                        checksumType = (ChecksumType)Enum.Parse(typeof(ChecksumType), checksumsType.algorithm);
                    }
                    start++;
                }
            }
            var chkSpcType = new ChecksumSpec_TYPE { ChecksumType = checksumType, OtherChecksumType = otherChecksumTypes };

            var responseObject = new IdentifyPillarsForReplaceFileResponse
                {
                    CollectionID = receivedIdentifyPillarsForReplaceFileRequest.CollectionID,
                    CorrelationID = receivedIdentifyPillarsForReplaceFileRequest.CorrelationID,
                    Destination = receivedIdentifyPillarsForReplaceFileRequest.ReplyTo,
                    FileID = receivedIdentifyPillarsForReplaceFileRequest.FileID,
                    From =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    PillarChecksumSpec = chkSpcType,
                    PillarID =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    ReplyTo =Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                    ResponseInfo = resInfo,
                    TimeToDeliver = timeType,
                    To = receivedIdentifyPillarsForReplaceFileRequest.From,
                    minVersion =Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                    version =Pillar.GlobalPillarApiSettings.XSD_VERSION
                };

            new MessageInfoContainer(responseObject).Send();
        }
    }
}