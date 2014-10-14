using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using PetaPoco;
using bmpxsd;
using log4net;
using PillarAPI.Models;

namespace PillarAPI.Utilities
{
    public class CollectedUtilities
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static Object DeserializeObject(string serializedObject, Type objectType)
        {
            var xs = new XmlSerializer(objectType);
            var encoding = new UTF8Encoding();
            var memoryStream = new MemoryStream(encoding.GetBytes(serializedObject));
            return xs.Deserialize(memoryStream);
        }

        public static string Serialize(object msgObject)
        {
            var xmls = new XmlSerializer(msgObject.GetType());
            using (var ms = new MemoryStream())
            {
                var utf = new UTF8Encoding(false);
                var settings = new XmlWriterSettings
                    {
                        Encoding = utf,
                        Indent = true,
                        IndentChars = "  ",
                        NewLineChars = Environment.NewLine,
                        ConformanceLevel = ConformanceLevel.Document,
                        OmitXmlDeclaration = false
                    };

                using (XmlWriter writer = XmlWriter.Create(ms, settings))
                {
                    xmls.Serialize(writer, msgObject);
                }
                string serializedObject = Encoding.UTF8.GetString(ms.ToArray());
                return serializedObject;
            }
        }

        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Log.Debug(string.Format("\tWarning: Matching schema not found. No validation occurred.{0}", args.Message));
            else
                Log.Debug(string.Format("\tValidation error: {0}", args.Message));
        }

        public static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return sbinary;
        }

        public static Byte[] StringToByteArray(string buff)
        {
            var encoding = new ASCIIEncoding();
            byte[] byteArray = encoding.GetBytes(buff);
            return byteArray;
        }

        public static IEnumerable<ChecksumsTypePoco> GetPillarChecksumTypes()
        {
            IEnumerable<ChecksumsTypePoco> algorithms = null;
            using (var db = DatabaseConnection.GetConnection())
            {
                try
                {
                    algorithms = db.Query<ChecksumsTypePoco>("SELECT algorithm FROM checksum_types");
                }
                catch (Exception e)
                {
                    Log.Error("Not an error, no checksum for given file: ", e);
                    return algorithms;
                }
            }
            return algorithms;
        }

        public static ChecksumDataForFile_TYPE GetLatestChecksum(string collectionId, string fileName, ChecksumSpec_TYPE chkSpkTypeForFile)
        {
            var pocoString = chkSpkTypeForFile.ChecksumSalt == null ? PocoStringWithoutSalt(collectionId, fileName, chkSpkTypeForFile) : PocoStringWithSalt(collectionId, fileName, chkSpkTypeForFile);
            ChecksumDataForFile_TYPE returnChecksumDataForFileType = null;
            using (var db = DatabaseConnection.GetConnection())
            {
                try
                {
                    var latestChecksum = db.SingleOrDefault<dynamic>(pocoString);
                    returnChecksumDataForFileType = new ChecksumDataForFile_TYPE
                    {
                        CalculationTimestamp = DateTime.Parse(latestChecksum.date.ToString()),
                        ChecksumSpec = chkSpkTypeForFile,
                        ChecksumValue = (byte[])latestChecksum.checksum
                    };
                    return returnChecksumDataForFileType;
                }
                catch (Exception e)
                {
                    Log.Error("GetLatestChecksum DB error: ", e);
                    throw;
                }
            }
        }

        private static Sql PocoStringWithoutSalt(string collectionId, string fileName, ChecksumSpec_TYPE chkSpkTypeForFile)
        {
            Sql pocoStringWithoutSalt = Sql.Builder
                .Append("SELECT MAX(c.date) as date, c.checksum")
                .Append("FROM files f")
                .Append("LEFT JOIN file_specs fs ON fs.active = 1 AND  f.file_id = fs.file_id")
                .Append("LEFT JOIN checksum_types ct ON ct.algorithm = @0", chkSpkTypeForFile.ChecksumType.ToString())
                .Append("LEFT JOIN checksums c ON ct.algorithm_id = c.algorithm_id AND fs.file_spec_id = c.file_spec_id")
                .Append("LEFT JOIN users u ON u.user_id = f.user_id AND u.collection_id = @0", collectionId)
                .Append("WHERE f.file_name = @0", fileName);
            return pocoStringWithoutSalt;
        }

        private static Sql PocoStringWithSalt(string collectionId, string fileName, ChecksumSpec_TYPE chkSpkTypeForFile)
        {
            Sql pocoStringWithSalt = Sql.Builder
                .Append("SELECT MAX(c.date) as date, c.checksum")
                .Append("FROM files f")
                .Append("LEFT JOIN file_specs fs ON fs.active = 1 AND  f.file_id = fs.file_id")
                .Append("LEFT JOIN checksum_types ct ON ct.algorithm = @0", chkSpkTypeForFile.ChecksumType.ToString())
                .Append("LEFT JOIN checksums c ON ct.algorithm_id = c.algorithm_id AND fs.file_spec_id = c.file_spec_id AND c.salt IS @0", chkSpkTypeForFile.ChecksumSalt)
                .Append("LEFT JOIN users u ON u.user_id = f.user_id AND u.collection_id = @0", collectionId)
                .Append("WHERE f.file_name = @0", fileName);
            return pocoStringWithSalt;
        }

        public static bool InsertChecksum(int fileSpecId, ChecksumDataForFile_TYPE checksumDataForFileTypeForFile)
        {
            bool inserted = true;
            try
            {
                using (var db = DatabaseConnection.GetConnection())
                {
                    var checksum = new ChecksumPoco();
                    Log.Debug(checksumDataForFileTypeForFile.ChecksumSpec.ChecksumType.ToString());
                    var checksumsTypePoco = db.SingleOrDefault<ChecksumsTypePoco>(string.Format("Select algorithm_id FROM checksum_types WHERE algorithm = '{0}'", checksumDataForFileTypeForFile.ChecksumSpec.ChecksumType));
                    if (checksumDataForFileTypeForFile.ChecksumSpec.ChecksumSalt != null) checksum.salt = checksumDataForFileTypeForFile.ChecksumSpec.ChecksumSalt;
                    checksum.algorithm_id = checksumsTypePoco.algorithm_id;
                    checksum.file_spec_id = fileSpecId;
                    checksum.checksum = checksumDataForFileTypeForFile.ChecksumValue;
                    checksum.date = checksumDataForFileTypeForFile.CalculationTimestamp;

                    try
                    {
                        using (var trans = db.GetTransaction())
                        {
                            db.Insert(checksum);
                            trans.Complete();
                        }
                    }
                    catch (Exception e)
                    {
                        inserted = false;
                        Log.Error("InsertChecksum insert into DB error: ", e);
                    }
                    finally
                    {
                        db.CompleteTransaction();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("InsertChecksum failed: ", e);
            }
            return inserted;
        }


        public static bool TryToDeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch (IOException ioException)
            {
                Log.Error("File couldn't be deleted.", ioException);
                return false;
            }
        }

        public static bool InsertAudit(string actionOnFile, string actorOnFile, string auditTrailInformation, string fileName, string info, string reportingComponent)
        {
            var inserted = true;

            try
            {
                using (var db = DatabaseConnection.GetConnection())
                {
                    var audit = new AuditPoco
                    {
                        actionOnFile = actionOnFile,
                        actorOnFile = actorOnFile,
                        auditTrailInformation = auditTrailInformation,
                        fileName = fileName,
                        file_id = 0,
                        info = info,
                        reportingComponent = reportingComponent
                    };

                    try
                    {
                        using (var trans = db.GetTransaction())
                        {
                            db.Insert(audit);
                            trans.Complete();
                        }
                    }
                    catch (Exception e)
                    {
                        inserted = false;
                        Log.Error("InsertAudit into DB error: ", e);
                    }
                    finally
                    {
                        db.CompleteTransaction();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("InsertAudit failed: ", e);
            }
            return inserted;
        }
    }
}