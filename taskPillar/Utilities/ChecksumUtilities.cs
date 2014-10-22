using System;
using System.IO;
using System.Security.Cryptography;
using bmpxsd;

namespace PillarAPI.Utilities
{
    public class ChecksumUtilities
    {
        private static Object thisLock = new Object();

        public static ChecksumSpec_TYPE ChecksumSpecTypeSet()
        {
            var checksumSpecType = new ChecksumSpec_TYPE
                                       {
                                           ChecksumType =
                                               (ChecksumType)
                                               Enum.Parse(typeof (ChecksumType),
                                                          Pillar.GlobalPillarApiSettings
                                                                .DEFAULT_CHECKSUM_TYPE)
                                       };
            return checksumSpecType;
        }

        public static ChecksumSpec_TYPE ChecksumSpecTypeSet(ChecksumType checksumType)
        {
            var checksumSpecType = new ChecksumSpec_TYPE {ChecksumType = checksumType};
            return checksumSpecType;
        }

        public static ChecksumSpec_TYPE ChecksumSpecTypeSet(byte[] salt, ChecksumType checksumType)
        {
            var checksumSpecType = new ChecksumSpec_TYPE {ChecksumType = checksumType};
            checksumSpecType.ChecksumSalt = salt;
            return checksumSpecType;
        }

        public static ChecksumSpec_TYPE ChecksumSpecTypeSet(byte[] salt, ChecksumType checksumType,
                                                            string otherChecksumType)
        {
            var checksumSpecType = new ChecksumSpec_TYPE {ChecksumType = checksumType};
            if (salt != null) checksumSpecType.ChecksumSalt = salt;
            checksumSpecType.OtherChecksumType = otherChecksumType;
            return checksumSpecType;
        }

        public static ChecksumSpec_TYPE ChecksumSpecTypeSet(ChecksumType checksumType, string otherChecksumType)
        {
            var checksumSpecType = new ChecksumSpec_TYPE {ChecksumType = checksumType};
            checksumSpecType.OtherChecksumType = otherChecksumType;
            return checksumSpecType;
        }

        public static ChecksumDataForFile_TYPE CalculateChecksumDataForFileType(ChecksumSpec_TYPE receivedChkSpkType,
                                                                                string filepath)
        {
            var returnChecksumDataForFileType = new ChecksumDataForFile_TYPE
                                                    {
                                                        CalculationTimestamp =
                                                            DateTime.Parse(DateTime.Now.ToString("s")),
                                                        ChecksumSpec = receivedChkSpkType,
                                                        ChecksumValue = CalculateChecksum(receivedChkSpkType, filepath)
                                                    };
            return returnChecksumDataForFileType;
        }

        private static byte[] CalculateChecksum(ChecksumSpec_TYPE receivedChkSpkType, string filepath)
        {
            using (var file = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                byte[] hashResultArray = null;
                byte[] saltArray = receivedChkSpkType.ChecksumSalt ?? new byte[0];
                ChecksumType chkType = receivedChkSpkType.ChecksumType;
                switch (chkType)
                {
                    case ChecksumType.MD5:
                        using (MD5 md5 = MD5.Create())
                        {
                            hashResultArray = md5.ComputeHash(file);
                        }
                        break;
                    case ChecksumType.SHA1:
                        using (SHA1 sha1 = new SHA1Managed())
                        {
                            hashResultArray = sha1.ComputeHash(file);
                        }
                        break;
                    case ChecksumType.SHA256:
                        using (SHA256 sha256 = new SHA256Managed())
                        {
                            hashResultArray = sha256.ComputeHash(file);
                        }
                        break;
                    case ChecksumType.SHA384:
                        using (SHA384 sha384 = new SHA384Managed())
                        {
                            hashResultArray = sha384.ComputeHash(file);
                        }
                        break;
                    case ChecksumType.SHA512:
                        using (SHA512 sha512 = new SHA512Managed())
                        {
                            hashResultArray = sha512.ComputeHash(file);
                        }
                        break;
                    case ChecksumType.HMAC_MD5:
                        using (var hmacmd5 = new HMACMD5(saltArray))
                        {
                            hashResultArray = hmacmd5.ComputeHash(file);
                        }
                        break;
                    case ChecksumType.HMAC_SHA1:
                        using (var hmacsha1 = new HMACSHA1(saltArray))
                        {
                            hashResultArray = hmacsha1.ComputeHash(file);
                        }
                        break;
                    case ChecksumType.HMAC_SHA256:
                        using (var hmacsha256 = new HMACSHA256(saltArray))
                        {
                            hashResultArray = hmacsha256.ComputeHash(file);
                        }
                        break;
                    case ChecksumType.HMAC_SHA384:
                        using (var hmacsha384 = new HMACSHA384(saltArray))
                        {
                            hashResultArray = hmacsha384.ComputeHash(file);
                        }
                        break;
                    case ChecksumType.HMAC_SHA512:
                        using (var hmacsha512 = new HMACSHA512(saltArray))
                        {
                            hashResultArray = hmacsha512.ComputeHash(file);
                        }
                        break;
                }
                return hashResultArray;
            }
        }
    }
}