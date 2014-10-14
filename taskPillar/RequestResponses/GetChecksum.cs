using System;
using System.IO;
using System.Linq;
using System.Reflection;
using bmpxsd;
using log4net;
using PillarAPI.Interfaces;
using PillarAPI.Utilities;

namespace PillarAPI.RequestResponses
{
    public class GetChecksum : IGetChecksum
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(IMessageInfoContainer message)
        {
            var receivedGetChecksumsRequest = message.MessageObject as GetChecksumsRequest;
            //string fileName = receivedGetChecksumsRequest.FileIDs.Item.ToString();
            string collection_id = receivedGetChecksumsRequest.CollectionID;

            // GetAllChecksums is identified by an empty system.object.
            if (!(receivedGetChecksumsRequest.FileIDs.Item.GetType() == typeof (Object)))
            {
                SingleFile(receivedGetChecksumsRequest);
                return;
            }

            ResultingChecksums resChk = null;
            var resInf = new ResponseInfo();


            string filePath =Pillar.GlobalPillarApiSettings.COLLECTION_FILE_DIRECTORY + collection_id + @"\";
            
            try
            {
                var myDb = DatabaseConnection.GetConnection();
                myDb.OpenSharedConnection();

                var result=myDb.Query<article>(
                    "SELECT fs.archived, fs.file_id, fs.file_spec_id, f.file_name " +
                    "FROM files f, file_specs fs " +
                    "WHERE f.deleted = 0 " +
                    "AND user_id = (SELECT user_id FROM users WHERE collection_id = @0) " +
                    "AND f.file_id = fs.file_id " +
                    "AND fs.active = 1", new object[] {collection_id});

                var enumerable = result as article[] ?? result.ToArray();
                int rowCount = enumerable.Count();
                int rowPointer = 0;
                var resChkTypeArray = new object[rowCount];

                if (rowCount > 0)
                {
                    foreach (article Article in enumerable)
                    {
                        ChecksumDataForFile_TYPE checksumDataForFileTypeForFile = CollectedUtilities.GetLatestChecksum(receivedGetChecksumsRequest.CollectionID,
                                                                                                                       Article.file_name,
                                                                                                                       receivedGetChecksumsRequest.ChecksumRequestForExistingFile);

                        // Checks whether file has been sent to optical disk or not (archived).
                        if (Convert.ToBoolean(Article.archived)) // Archived -  if possible checksum will be fetched from DB.
                        {
                            if (checksumDataForFileTypeForFile != null)
                            {
                                //Log.Debug("henter fra db");
                                var resChkType = new ChecksumDataForChecksumSpec_TYPE
                                    {
                                        FileID = Article.file_name,
                                        CalculationTimestamp = checksumDataForFileTypeForFile.CalculationTimestamp,
                                        ChecksumValue = checksumDataForFileTypeForFile.ChecksumValue
                                    };
                                resChkTypeArray[rowPointer] = resChkType;
                            }
                            else
                            {
                                resChkTypeArray[rowPointer] = null;
                            }
                        }
                        else // Not archived - Checksum will be calculated or fetched from db
                        {
                            if (File.Exists(filePath + Article.file_id + "." + Article.file_spec_id + "." + Article.file_name) && checksumDataForFileTypeForFile == null)
                            {
                                ChecksumDataForFile_TYPE checksumDataForFileTypeCalculated = ChecksumUtilities.CalculateChecksumDataForFileType(receivedGetChecksumsRequest.ChecksumRequestForExistingFile, filePath + Article.file_id + "." + Article.file_spec_id + "." + Article.file_name);
                                var resChkType = new ChecksumDataForChecksumSpec_TYPE
                                    {
                                        FileID = Article.file_name,
                                        CalculationTimestamp = checksumDataForFileTypeCalculated.CalculationTimestamp,
                                        ChecksumValue = checksumDataForFileTypeCalculated.ChecksumValue
                                    };
                                resChkTypeArray[rowPointer] = resChkType;
                                var checksumDataForFileTypeForDb = new ChecksumDataForFile_TYPE
                                    {
                                        CalculationTimestamp = resChkType.CalculationTimestamp,
                                        ChecksumValue = resChkType.ChecksumValue,
                                        ChecksumSpec = receivedGetChecksumsRequest.ChecksumRequestForExistingFile
                                    };
                                //CollectedUtilities.InsertChecksum(Article.file_spec_id, checksumDataForFileTypeForDb);
                                CollectedUtilities.InsertChecksum(Article.file_spec_id, checksumDataForFileTypeForDb);
                            }
                            else if (checksumDataForFileTypeForFile != null) // checksum is kept in DB
                            {
                                var resChkType = new ChecksumDataForChecksumSpec_TYPE
                                    {
                                        FileID = Article.file_name,
                                        CalculationTimestamp = checksumDataForFileTypeForFile.CalculationTimestamp,
                                        ChecksumValue = checksumDataForFileTypeForFile.ChecksumValue
                                    };
                                resChkTypeArray[rowPointer] = resChkType;
                            }
                        }
                        rowPointer++;
                    }
                    try
                    {
                        resChk = new ResultingChecksums {Items = resChkTypeArray};
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e.Message, e);
                    }
                    resInf.ResponseCode = ResponseCode.OPERATION_COMPLETED;
                    resInf.ResponseText = "Data for AllFileIDs has been found for get checksums request. If there are checksums missing contact SA and have them transfer files from disk";
                }
                else
                {
                    resInf.ResponseCode = ResponseCode.FAILURE;
                    resInf.ResponseText = "No files where found for a getAllFileIDs checksums request";
                }
            }
            catch (Exception e)
            {
                Log.Warn("GetChecksums error: " + e);
            }
            ChecksumResponse(receivedGetChecksumsRequest, resInf, resChk);
        }

        public class article
        {
            public bool archived { get; set; }
            public int file_id { get; set; }
            public int file_spec_id { get; set; }
            public string file_name { get; set; }
        }


        /// <summary>
        ///     Processes single file checksum requests.
        /// </summary>
        /// <param name="receivedGetChecksumsRequest"></param>
        private static void SingleFile(GetChecksumsRequest receivedGetChecksumsRequest)
        {
            string fileName = receivedGetChecksumsRequest.FileIDs.Item.ToString();
            string collectionId = receivedGetChecksumsRequest.CollectionID;

            var f1 = new FileInfoContainer(collectionId, fileName);

            //There is no file to calculate the checksum on in the repository.
            if (string.IsNullOrEmpty(f1.FileId))
            {
                var resInf = new ResponseInfo
                    {
                        ResponseCode = ResponseCode.FILE_NOT_FOUND_FAILURE,
                        ResponseText = "There is no file to calculate the checksum on"
                    };
                ChecksumResponse(receivedGetChecksumsRequest, resInf, null);
            }
            string fullFilePath =Pillar.GlobalPillarApiSettings.COLLECTION_FILE_DIRECTORY + collectionId + @"\" + f1.FileId + "." + f1.FileSpecId + "." + fileName;

            ChecksumDataForFile_TYPE checksumDataForFileTypeForFile = CollectedUtilities.GetLatestChecksum(receivedGetChecksumsRequest.CollectionID,
                                                                                                           receivedGetChecksumsRequest.FileIDs.Item.ToString(),     
                                                                                                           receivedGetChecksumsRequest.ChecksumRequestForExistingFile);

        
            // If the file is on disk and the checksum isn't in the DB, checksum will be calculated and saved in DB.
            if (File.Exists(fullFilePath) && checksumDataForFileTypeForFile == null)
            {
                checksumDataForFileTypeForFile = ChecksumUtilities.CalculateChecksumDataForFileType(receivedGetChecksumsRequest.ChecksumRequestForExistingFile, fullFilePath);
                //CollectedUtilities.InsertChecksum(int.Parse(f1.FileSpecId), checksumDataForFileTypeForFile);
                CollectedUtilities.InsertChecksum(int.Parse(f1.FileSpecId), checksumDataForFileTypeForFile);
            }

           

            if (checksumDataForFileTypeForFile.ChecksumValue == null)
            {
                var responsInfo = new ResponseInfo
                    {
                        ResponseCode = ResponseCode.EXISTING_FILE_CHECKSUM_FAILURE,
                        ResponseText = "Checksum value isn't kept in DB and file isn't available. Contact SA and arrange for file to be transfered from storage"
                    };
                ChecksumResponse(receivedGetChecksumsRequest, responsInfo, null);
            }

            var responsInfoFinal = new ResponseInfo
                {
                    ResponseCode = ResponseCode.OPERATION_COMPLETED,
                    ResponseText = "Checksum value for single file returned"
                };


            var check = new ChecksumDataForChecksumSpec_TYPE
                {
                    ChecksumValue = checksumDataForFileTypeForFile.ChecksumValue,
                    CalculationTimestamp = checksumDataForFileTypeForFile.CalculationTimestamp,
                    FileID = receivedGetChecksumsRequest.FileIDs.Item.ToString()
                };
            var resChkTypeArray = new object[] {check};
            var resChk = new ResultingChecksums {Items = resChkTypeArray};
            ChecksumResponse(receivedGetChecksumsRequest, responsInfoFinal, resChk);
        }

        private static void ChecksumResponse(GetChecksumsRequest receivedGetChecksumsRequest, ResponseInfo responseInfo, ResultingChecksums resChk)
        {
            string filename = VarifyIfSingleOrAllFileIDs(receivedGetChecksumsRequest);

            CollectedUtilities.InsertAudit(FileAction.GET_CHECKSUMS.ToString(), receivedGetChecksumsRequest.From, receivedGetChecksumsRequest.AuditTrailInformation, filename, responseInfo.ResponseText,Pillar.GlobalPillarApiSettings.PILLAR_ID);

            var responseObject = new GetChecksumsFinalResponse
                {
                    ChecksumRequestForExistingFile = receivedGetChecksumsRequest.ChecksumRequestForExistingFile,
                    CollectionID = receivedGetChecksumsRequest.CollectionID,
                    CorrelationID = receivedGetChecksumsRequest.CorrelationID,
                    Destination = receivedGetChecksumsRequest.ReplyTo,
                    From =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    PillarID =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                    ReplyTo =Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                    ResponseInfo = responseInfo,
                    ResultingChecksums = resChk,
                    To = receivedGetChecksumsRequest.From,
                    minVersion = receivedGetChecksumsRequest.minVersion,
                    version =Pillar.GlobalPillarApiSettings.XSD_VERSION
                };

            var returnMessage = new MessageInfoContainer(responseObject);
            returnMessage.Send();
        }

        /// <summary>
        ///     Varifies if checksum for single or all file IDs are requested.
        /// </summary>
        /// <param name="receivedGetChecksumsRequest">The received get checksums request.</param>
        /// <returns></returns>
        private static string VarifyIfSingleOrAllFileIDs(GetChecksumsRequest receivedGetChecksumsRequest)
        {
            if (receivedGetChecksumsRequest.FileIDs.Item.GetType() == typeof (object))
            {
                return "AllFileIDs";
            }
            return receivedGetChecksumsRequest.FileIDs.Item.ToString();
        }
    }
}