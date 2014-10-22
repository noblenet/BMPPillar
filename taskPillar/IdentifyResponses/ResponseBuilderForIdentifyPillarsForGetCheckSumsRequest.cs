using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PetaPoco;
using PillarAPI.Utilities;
using bmpxsd;
using log4net;

namespace PillarAPI.IdentifyResponses
{
    internal static class ResponseBuilderForIdentifyPillarsForGetCheckSumsRequest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void MakeResponse(MessageInfoContainer message)
        {
            var receivedIdentifyPillarsForGetCheckSumsRequest =
                message.MessageObject as IdentifyPillarsForGetChecksumsRequest;
            if (receivedIdentifyPillarsForGetCheckSumsRequest == null) throw new ArgumentNullException("message");


            var resInfo = new ResponseInfo();
            var timeType = new TimeMeasure_TYPE();
            string collectionId = receivedIdentifyPillarsForGetCheckSumsRequest.CollectionID;

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
                if (!(receivedIdentifyPillarsForGetCheckSumsRequest.FileIDs.Item.GetType() == typeof (Object)))
                {
                    string fileName = receivedIdentifyPillarsForGetCheckSumsRequest.FileIDs.Item.ToString();
                    var f1 = new FileInfoContainer(collectionId, fileName);
                    // There is no file in the repository - The file has never been or no longer is in the repository.
                    if (string.IsNullOrEmpty(f1.FileId))
                    {
                        resInfo.ResponseCode = ResponseCode.FILE_NOT_FOUND_FAILURE;
                        resInfo.ResponseText =
                            "There is no file with the requested fileId in the repository. No checksum calculation is possible.";
                        timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                        timeType.TimeMeasureValue = "0";
                    }
                    else
                    {
                        if (f1.Archived)
                        {
                            ChecksumDataForFile_TYPE checksumDataForFileTypeForFile =
                                CollectedUtilities.GetLatestChecksum(collectionId, fileName,
                                                                     receivedIdentifyPillarsForGetCheckSumsRequest.
                                                                         ChecksumRequestForExistingFile);

                            if (checksumDataForFileTypeForFile == null)
                            {
                                resInfo.ResponseCode = ResponseCode.IDENTIFICATION_NEGATIVE;
                                resInfo.ResponseText =
                                    "File has been archived and is not available for checksum calculation. Contact SA and arrange for file to be transfered from storage";
                                timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                                timeType.TimeMeasureValue = "24";
                            }
                            else
                            {
                                resInfo.ResponseCode = ResponseCode.IDENTIFICATION_POSITIVE;
                                resInfo.ResponseText = "Checksum is kept in db and is available";
                                timeType.TimeMeasureUnit = TimeMeasureUnit.MILLISECONDS;
                                timeType.TimeMeasureValue = "2000";
                            }
                        }
                        else
                        {
                            resInfo.ResponseCode = ResponseCode.IDENTIFICATION_POSITIVE;
                            resInfo.ResponseText = "File is available for checksum calculation";
                            timeType.TimeMeasureUnit = TimeMeasureUnit.MILLISECONDS;
                            timeType.TimeMeasureValue = "2000";
                        }
                    }
                }
                else // getAll request
                {
                    GetInfoAboutCalculateChecksum(collectionId, resInfo, timeType);
                }
            }
            var responseObject = new IdentifyPillarsForGetChecksumsResponse
                                     {
                                         CollectionID = receivedIdentifyPillarsForGetCheckSumsRequest.CollectionID,
                                         CorrelationID = receivedIdentifyPillarsForGetCheckSumsRequest.CorrelationID,
                                         ChecksumRequestForExistingFile =
                                             receivedIdentifyPillarsForGetCheckSumsRequest
                                             .ChecksumRequestForExistingFile,
                                         Destination = receivedIdentifyPillarsForGetCheckSumsRequest.ReplyTo,
                                         FileIDs = receivedIdentifyPillarsForGetCheckSumsRequest.FileIDs,
                                         From = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                                         minVersion = Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                                         PillarChecksumSpec =
                                             receivedIdentifyPillarsForGetCheckSumsRequest
                                             .ChecksumRequestForExistingFile,
                                         PillarID = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                                         ReplyTo = Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                                         ResponseInfo = resInfo,
                                         TimeToDeliver = timeType,
                                         To = receivedIdentifyPillarsForGetCheckSumsRequest.From,
                                         version = Pillar.GlobalPillarApiSettings.XSD_VERSION
                                     };

            new MessageInfoContainer(responseObject).Send();
        }

        private static void GetInfoAboutCalculateChecksum(string collectionId, ResponseInfo resInfo,
                                                          TimeMeasure_TYPE timeType)
        {
            using (Database db = DatabaseConnection.GetConnection())
            {
                Sql sqlString = Sql.Builder
                                   .Append("SELECT fs.archived, fs.file_id, fs.file_spec_id, f.file_name ")
                                   .Append("FROM file_specs fs, files f , users u ")
                                   .Append("WHERE u.collection_id = @0", collectionId)
                                   .Append("AND f.user_id = u.user_id")
                                   .Append("AND fs.file_id = f.file_id ")
                                   .Append("AND f.deleted = 0 ")
                                   .Append("AND fs.active = 1 ")
                                   .Append("ORDER BY fs.archived DESC");

                try
                {
                    IEnumerable<dynamic> result = db.Query<dynamic>(sqlString);

                    if (result.Any())
                    {
                        if (!Convert.ToBoolean(result.First().archived))
                        {
                            resInfo.ResponseText = "All files are ready for checksum calculation";
                        }
                        else
                        {
                            resInfo.ResponseText =
                                "One or more files has been archived and are not available for new checksum calculations. " +
                                "If you want new checksum calculations on all files, contact SA and have then transfer the files. " +
                                "If new checksum calculations are not needed, the pillar has stored checksums for all files. " +
                                "Stored check does not require files to be transfered";
                        }

                        resInfo.ResponseCode = ResponseCode.IDENTIFICATION_POSITIVE;
                        timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                        timeType.TimeMeasureValue = "1";
                    }
                    else
                    {
                        resInfo.ResponseCode = ResponseCode.IDENTIFICATION_NEGATIVE;
                        resInfo.ResponseText = "There are no files to calculatate checksums on in the repository";
                        timeType.TimeMeasureUnit = TimeMeasureUnit.HOURS;
                        timeType.TimeMeasureValue = "0";
                    }
                }
                catch (Exception e)
                {
                    Log.Debug(e.ToString());
                }
            }
        }
    }
}