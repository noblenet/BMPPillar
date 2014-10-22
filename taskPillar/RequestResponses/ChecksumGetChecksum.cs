using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PetaPoco;
using PillarAPI.Interfaces;
using PillarAPI.Utilities;
using bmpxsd;
using log4net;

namespace PillarAPI.RequestResponses
{
    public class ChecksumGetChecksum : IGetChecksum
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(IMessageInfoContainer message)
        {
            var receivedGetChecksumsRequest = (GetChecksumsRequest) message.MessageObject;
            string collectionId = receivedGetChecksumsRequest.CollectionID;

            ResultingChecksums resChk = null;
            var resInf = new ResponseInfo();

            try
            {
                Sql sqlstring = Sql.Builder
                                   .Append("SELECT f.file_name")
                                   .Append("FROM files f, file_specs fs")
                                   .Append("WHERE f.deleted = 0")
                                   .Append("AND f.user_id = (SELECT user_id FROM users WHERE collection_id = @0)",
                                           collectionId)
                                   .Append("AND f.file_id = fs.file_id ")
                                   .Append("AND fs.active = 1");
                if (!(IsGetAllChecksumRequest(receivedGetChecksumsRequest)))
                    sqlstring.Append("AND f.file_name = @0", receivedGetChecksumsRequest.FileIDs.Item.ToString());

                using (Database myDb = DatabaseConnection.GetConnection())
                {
                    var resChkTypeCollection = new List<object>();

                    IEnumerable<string> files = myDb.Query<string>(sqlstring);
                    foreach (string filename in files)
                    {
                        ChecksumDataForFile_TYPE checksumDataForFileTypeForFile =
                            CollectedUtilities.GetLatestChecksum(receivedGetChecksumsRequest.CollectionID, filename,
                                                                 receivedGetChecksumsRequest
                                                                     .ChecksumRequestForExistingFile);

                        var resChkType = new ChecksumDataForChecksumSpec_TYPE
                                             {
                                                 FileID = filename,
                                                 CalculationTimestamp =
                                                     checksumDataForFileTypeForFile.CalculationTimestamp,
                                                 ChecksumValue = checksumDataForFileTypeForFile.ChecksumValue
                                             };
                        resChkTypeCollection.Add(resChkType);
                    }

                    if (files.Any())
                    {
                        try
                        {
                            resChk = new ResultingChecksums {Items = resChkTypeCollection.ToArray()};
                        }
                        catch (Exception e)
                        {
                            Log.Warn(e.Message, e);
                        }
                        resInf.ResponseCode = ResponseCode.OPERATION_COMPLETED;
                        resInf.ResponseText =
                            "Data has been found for get checksums request. If there are checksums missing contact SA and have them transfer files from disk";
                    }
                    else
                    {
                        resInf.ResponseCode = ResponseCode.FAILURE;
                        resInf.ResponseText = "No files where found for a get checksums request";
                    }
                }
            }
            catch (Exception e)
            {
                resInf.ResponseCode = ResponseCode.FAILURE;
                resInf.ResponseText = "Unknow error";
                Log.Warn("GetChecksums error: " + e);
            }
            ChecksumResponse(receivedGetChecksumsRequest, resInf, resChk);
        }

        private static void ChecksumResponse(GetChecksumsRequest receivedGetChecksumsRequest, ResponseInfo responseInfo,
                                             ResultingChecksums resChk)
        {
            var responseObject = new GetChecksumsFinalResponse
                                     {
                                         ChecksumRequestForExistingFile =
                                             receivedGetChecksumsRequest.ChecksumRequestForExistingFile,
                                         CollectionID = receivedGetChecksumsRequest.CollectionID,
                                         CorrelationID = receivedGetChecksumsRequest.CorrelationID,
                                         Destination = receivedGetChecksumsRequest.ReplyTo,
                                         From = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                                         PillarID = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                                         ReplyTo = Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                                         ResponseInfo = responseInfo,
                                         ResultingChecksums = resChk,
                                         To = receivedGetChecksumsRequest.From,
                                         minVersion = receivedGetChecksumsRequest.minVersion,
                                         version = Pillar.GlobalPillarApiSettings.XSD_VERSION
                                     };
            var returnMessage = new MessageInfoContainer(responseObject);
            returnMessage.Send();
        }

        /// <summary>
        ///     Returns the single or all file identifier.
        /// </summary>
        /// <param name="receivedGetChecksumsRequest">The received get checksums request.</param>
        /// <returns></returns>
        private static string ReturnSingleOrAllFileIdentifier(GetChecksumsRequest receivedGetChecksumsRequest)
        {
            return IsGetAllChecksumRequest(receivedGetChecksumsRequest)
                       ? "AllFileIDs"
                       : receivedGetChecksumsRequest.FileIDs.Item.ToString();
        }

        private static bool IsGetAllChecksumRequest(GetChecksumsRequest receivedGetChecksumsRequest)
        {
            return receivedGetChecksumsRequest.FileIDs.Item.GetType() == typeof (Object);
        }
    }
}