using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using PillarAPI.Utilities;
using bmpxsd;
using log4net;
using PillarAPI;
using pillarAPI;

namespace PseudoKlient
{
    public class PseudoKlient
    {

        //// MQ stuff
        private const string TOPIC_NAME = "topic://sa-test";
        //public const string BROKER = "ssl://JKM-win7monster:61616";
        private const string CLIENT_ID = "sa_testClient";
        private const string Path2Settings = @"I:\PillarSettingsKNA.xml";
        //public const string CLIENT_TOPIC = "topic://sa_test_client";
        //public const string PILLAR_QUEUE = "queue://sa_pillar_queue";

        //// Filedir
        public const string filePath = @"i:\testFiler";
        //public const string XSDPath = @"C:\Udvikling\Kode\TestTing\xsdFiles\BitRepositoryElements.xsd";
//        private readonly HttpServer _httpServer = new HttpServer();
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
        var pillarApiSettings = (PillarApiSettings)SerializationUtilities.DeserializeObject(File.ReadAllText(Path2Settings), typeof(PillarApiSettings));
            
            Log.Debug("Broker: " + pillarApiSettings.MESSAGE_BUS_CONFIGURATION_URL );

//            IConnectionFactory connectionFactory = new ConnectionFactory(BROKER + "?transport.clientcertfilename=C:/Downloads/Apache/apache-activemq-5.5.1/conf/broker2.cer", CLIENT_ID);
            IConnectionFactory connectionFactory = new ConnectionFactory(pillarApiSettings.MESSAGE_BUS_CONFIGURATION_URL, CLIENT_ID);
            IConnection connection = connectionFactory.CreateConnection();
            connection.Start();

            ISession session = connection.CreateSession();

            //var messageSubscriber = new TSubscriber(session, CLIENT_TOPIC);

            //Console.WriteLine("1");

            //var topicSender = new Publisher(session, TOPIC_NAME_DURABLE, false);
            var topicSender = new Publisher2(session, session.GetDestination(TOPIC_NAME));
            var saSender = new Publisher2(session, session.GetDestination(pillarApiSettings.SA_PILLAR_QUEUE));

            // Messagelist - functions
            var usrLst = new List<string> {"sa-test", "sa-test2", "sa-test3", "sa-test4"};

            // Userlist - CollectionIds
            var flLst = new List<string>
                {
                    "IdentifyPillarsForPutFileRequest",
                    "PutFileRequest",
                    "IdentifyPillarsForGetFileRequest",
                    "GetFileRequest",
                    "IdentifyPillarsForDeleteFileRequest",
                    "DeleteFileRequest",
                    "IdentifyPillarsForGetFileIDsRequest",
                    "IdentifyPillarsForGetAllFileIDsRequest",
                    "GetFileIDsRequest",
                    "GetAllFileIDsRequest",
                    "IdentifyPillarsForGetChecksumsRequest",
                    "IdentifyPillarsForGetAllChecksumsRequest",
                    "GetChecksumsRequest",
                    "GetAllChecksumsRequest",
                    "IdentifyPillarsForReplaceFileRequest",
                    "replaceFileRequest",
                    "IdentifyContributorsForGetAuditTrailsRequest",
                    "GetAuditTrailsRequest",
                    "GetStatusRequest"
                };
            //flLst.Add("IdentifyContributorsForGetAuditTrailsRequest");
            //flLst.Add("IdentifyContributorsForGetStatusRequest");
            // files in dir
            string[] files = Directory.GetFiles(filePath);

            Task.Factory.StartNew(() => DoWork("sa_test_client", connection, "Client subscriber"));
            try
            {
                ConsoleKeyInfo esckey;
                int flNum = -1;
                do
                {
                    do
                    {
                        // User/collection is choosen
                        Console.WriteLine("\nHvilken af følgende users/collections vil du anvende?\n");
                        usrLst.ForEach(name => Console.WriteLine(usrLst.IndexOf(name) + 1 + ". " + name));
                        do
                        {
                            try
                            {
                                flNum = Convert.ToInt16(Console.ReadLine());
                                if (flNum > usrLst.Count || flNum < 1)
                                {
                                    throw new FormatException();
                                }
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Den indtastede værdi skal være et tal mellem 1 og " +
                                                  (flLst.Count - 1));
                            }
                        } while (flNum < 0);
                        string colId = usrLst[flNum - 1];
                        Console.WriteLine("Du vil anvende: " + flNum + ". " + colId + "\n");
                        flNum = -1;

                        // File i choosen
                        Console.WriteLine("Hvilken af følgende filer vil du arbejde med?");
                        int fileCount = 1;
                        foreach (string name in files)
                        {
                            var fInfo = new FileInfo(name);
                            Console.WriteLine(fileCount + ". " + fInfo.Name + ", størrelse i bytes: " + fInfo.Length);
                            fileCount++;
                        }
                        do
                        {
                            try
                            {
                                flNum = Convert.ToInt16(Console.ReadLine());
                                if (flNum > usrLst.Count || flNum < 1)
                                {
                                    throw new FormatException();
                                }
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Den indtastede værdi skal være et tal mellem 1 og " +
                                                  (flLst.Count - 1));
                            }
                        } while (flNum < 0);
                        var fInfo2 = new FileInfo(files[flNum - 1]);
                        string file = fInfo2.Name;
                        Console.WriteLine("Du vil anvende følgende fil: " + flNum + ". " + file + "\n");
                        flNum = -1;


                        // Messagetype is choosen
                        Console.WriteLine("Hvilken af følgende beskeder vil du sende?\n");
                        flLst.ForEach(name => Console.WriteLine(flLst.IndexOf(name) + 1 + ". " + name));
                        do
                        {
                            try
                            {
                                flNum = Convert.ToInt16(Console.ReadLine());
                                if (flNum > flLst.Count || flNum < 1)
                                {
                                    throw new FormatException();
                                }
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Den indtastede værdi skal være et tal mellem 1 og " + flLst.Count);
                                flNum = -1;
                            }
                        } while (flNum < 0);
                        Console.WriteLine("Du vil sende: " + flNum + "\n");

                        switch (flNum)
                        {
                            case 1:
                                topicSender.SendMessage(MakeIdentifyPillarsForPutFileRequest(session, colId, file,pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.SA_PILLAR_QUEUE));
                                break;
                            case 2:
                                // ---------  Har fjernet httpsTingen
                                saSender.SendMessage(MakePutFileRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.PILLAR_ID));
                                Console.WriteLine("HTTP er overstået: " + HttpServer.HttpStart(@"http://127.0.0.1:443/", file));
                                break;
                            case 3:
                                topicSender.SendMessage(MakeIdentifyPillarsForGetFileRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.SA_PILLAR_QUEUE));
                                break;
                            case 4:
                                // ---------  Har fjernet httpsTingen
                                saSender.SendMessage(MakeGetFileRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.PILLAR_ID));
                                Console.WriteLine("HTTP er overstået: " + HttpServer.HttpStart(@"http://127.0.0.1:443/", file));
                                break;
                            case 5:
                                topicSender.SendMessage(MakeIdentifyPillarsForDeleteFileRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.SA_PILLAR_QUEUE));
                                break;
                            case 6:
                                saSender.SendMessage(MakeDeleteFileRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.PILLAR_ID));
                                break;
                            case 7:
                                topicSender.SendMessage(MakeIdentifyPillarsForGetFileIDsRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION));
                                break;
                            case 8:
                                topicSender.SendMessage(MakeIdentifyPillarsForGetFileIDsRequest(session, colId, "AllFileIDs", pillarApiSettings.COLLECTION_DESTINATION));
                                break;
                            case 9:
                                saSender.SendMessage(MakeGetFileIDsRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.PILLAR_ID));
                                break;
                            case 10:
                                saSender.SendMessage(MakeGetFileIDsRequest(session, colId, "AllFileIDs", pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.PILLAR_ID));
                                break;
                            case 11:
                                topicSender.SendMessage(MakeIdentifyPillarsForGetChecksumsRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION));
                                break;
                            case 12:
                                topicSender.SendMessage(MakeIdentifyPillarsForGetChecksumsRequest(session, colId, "AllFileIDs", pillarApiSettings.COLLECTION_DESTINATION));
                                break;
                            case 13:
                                saSender.SendMessage(GetChecksumsRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION));
                                break;
                            case 14:
                                saSender.SendMessage(GetChecksumsRequest(session, colId, "AllFileIDs", pillarApiSettings.COLLECTION_DESTINATION));
                                break;
                            case 15:
                                topicSender.SendMessage(MakeIdentifyPillarsForReplaceFileRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.SA_PILLAR_QUEUE));
                                break;
                            case 16:
                                saSender.SendMessage(MakeReplaceFileRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.PILLAR_ID));
                                Console.WriteLine("HTTP er overstået: " + HttpServer.HttpStart(@"http://127.0.0.1:443/", file));
                                break;
                            case 17:
                                topicSender.SendMessage(MakeIdentifyContributorsForGetAuditTrailsRequest(session, colId, pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.SA_PILLAR_QUEUE));
                                break;
                            case 18:
                                saSender.SendMessage(MakeGetAuditTrailsRequest(session, colId, file, pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.SA_PILLAR_QUEUE));
                                break;
                            case 19:
                                saSender.SendMessage(MakeGetStatusRequest(session, colId, pillarApiSettings.COLLECTION_DESTINATION,pillarApiSettings.SA_PILLAR_QUEUE));
                                break;
                            default:
                                Console.WriteLine("Beskedtypen er ikke oprettet if systemet endnu");
                                break;
                        }
                        Console.WriteLine("Tryk på en vilkårlig tast for at fortsætte, tryk på 'Esc' for at afsluttet.");
                        esckey = Console.ReadKey();
                        Console.WriteLine("\n");
                    } while (esckey.Key != ConsoleKey.Escape);
                } while (esckey.Key != ConsoleKey.Escape);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            ////cleanup
            Thread.Sleep(1000);
            topicSender.Dispose();
            session.Dispose();
            connection.Dispose();
            Console.ReadLine();
        }

        public static void DoWork(string queue, IConnection connection, string Subscription_ID)
        {
            ISession session;
            session = connection.CreateSession();

            var messageSubscriber = new TSubscriber(session, queue);
            messageSubscriber.Start(Subscription_ID);

            messageSubscriber.OnMessageReceived += (message =>
                {
                    string msgtype = message.NMSType;

                    Console.WriteLine("msgType fra listener: " + msgtype);
                    //Console.WriteLine("messageTimeStamp: " + message.NMSTimestamp + "\n" +
                    //    "corId: " + message.NMSCorrelationID + "\n" +
                    //    "delMode: " + message.NMSDeliveryMode + "\n" +
                    //    "dest: " + message.NMSDestination + "\n" +
                    //    "mesId: " + message.NMSMessageId + "\n" +
                    //    "prio: " + message.NMSPriority + "\n" +
                    //    "redelivered: " + message.NMSRedelivered + "\n" +
                    //    "repTo: " + message.NMSReplyTo + "\n" +
                    //    "ttl: " + message.NMSTimeToLive + "\n" +
                    //    "props: " + message.Properties
                    //    );
                    switch (msgtype)
                    {
                        case "IdentifyPillarsForPutFileResponse":
                            Console.WriteLine("\nIdentifyPillarsForPutFileResponse " + message.Text);
                            break;
                        case "IdentifyPillarsForGetFileResponse":
                            Console.WriteLine("IdentifyPillarsForGetFileResponse " + message.Text);
                            break;
                        case "IdentifyPillarsForGetFileIDsResponse":
                            Console.WriteLine("\nIdentifyPillarsForGetFileIDsFileResponse " + message.Text);
                            break;
                        case "IdentifyPillarsForDeleteFileResponse":
                            Console.WriteLine("IdentifyPillarsForDeleteFileResponse " + message.Text);
                            break;
                        case "IdentifyPillarsForGetChecksumsResponse":
                            Console.WriteLine("IdentifyPillarsForGetChecksumsResponse " + message.Text);
                            break;
                        case "IdentifyPillarsForReplaceFileResponse":
                            Console.WriteLine("IdentifyPillarsForReplaceFileResponse " + message.Text);
                            break;
                        case "IdentifyContributorsForGetAuditTrailsResponse":
                            Console.WriteLine("IdentifyContributorsForGetAuditTrailsResponse " + message.Text);
                            break;
                        case "PutFileFinalResponse":
                            Console.WriteLine("PutFileFinalResponse " + message.Text);
                            break;
                        case "GetFileFinalResponse":
                            Console.WriteLine("GetFileFinalResponse " + message.Text);
                            break;
                        case "GetChecksumsFinalResponse":
                            Console.WriteLine("GetChecksumsFinalResponse " + message.Text);
                            break;
                        case "ReplaceFileFinalResponse":
                            Console.WriteLine("ReplaceFileFinalResponse " + message.Text);
                            break;
                        case "DeleteFileFinalResponse":
                            Console.WriteLine("DeleteFileFinalResponse " + message.Text);
                            break;
                        case "GetAuditTrailsFinalResponse":
                            Console.WriteLine("GetAuditTrailsFinalResponse " + message.Text);
                            break;
                        case "GetStatusFinalResponse":
                            Console.WriteLine("GetStatusFinalResponse " + message.Text);
                            break;
                        default:
                            break;
                    }
                    msgtype = "";
                });
        }

        private static ITextMessage MakeGetStatusRequest(ISession session, string colId, string client_topic, string pillar_queue)
        {
            var messageObject = new GetStatusRequest
                {
                    AuditTrailInformation = "Audit fra GetStatus",
                    CollectionID = colId,
                    Contributor = "Den lidt for seje PseudoKlient",
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    From = CLIENT_ID,
                    ReplyTo = client_topic,
                    To = pillar_queue,
                    minVersion = "1",
                    version = "9"
                };
            return MakeITMessage(session, colId, messageObject, client_topic);
        }

        private static ITextMessage MakeGetAuditTrailsRequest(ISession session, string colId, string fileName, string client_topic, string pillar_queue)
        {
            var messageObject = new GetAuditTrailsRequest
                {
                    AuditTrailInformation = "Informationer der kan have betydning for klienten når audit skal hentes frem igen",
                    CollectionID = colId,
                    Contributor = "Den lidt for seje PseudoKlient",
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    FileID = fileName,
                    From = CLIENT_ID,
                    MaxSequenceNumber = "99999",
                    MaxTimestamp = DateTime.Now,
                    MaxTimestampSpecified = true,
                    MinSequenceNumber = "0",
                    MinTimestamp = new DateTime(2020, 5, 1, 0, 0, 1),
                    MinTimestampSpecified = true,
                    ReplyTo = client_topic,
                    ResultAddress = "ResultAddress",
                    To = pillar_queue,
                    minVersion = "1",
                    version = "9"
                };

            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        private static ITextMessage MakeIdentifyContributorsForGetAuditTrailsRequest(ISession session, string colId, string client_topic, string pillar_queue)
        {
            var messageObject = new IdentifyContributorsForGetAuditTrailsRequest
                {
                    AuditTrailInformation = "AudittrailInfo hvad er det så lige det er?",
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    From = CLIENT_ID,
                    ReplyTo = client_topic,
                    To = pillar_queue,
                    minVersion = "1",
                    version = "9"
                };

            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        private static ITextMessage MakeIdentifyPillarsForReplaceFileRequest(ISession session, string colId, string fileName, string client_topic, string pillar_queue)
        {
            var fInfo = new FileInfo(filePath + @"\" + fileName);
            var messageObject = new IdentifyPillarsForReplaceFileRequest
                {
                    AuditTrailInformation = "Replace AuditTrailInformation",
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    FileID = fileName,
                    FileSize = fInfo.Length.ToString(CultureInfo.InvariantCulture),
                    From = CLIENT_ID,
                    ReplyTo = client_topic,
                    To = pillar_queue,
                    minVersion = "1",
                    version = "9"
                };
            return MakeITMessage(session, colId, messageObject,client_topic);
        }


        private static ITextMessage MakeIdentifyPillarsForPutFileRequest(ISession session, string colId, string fileName, string client_topic, string pillar_queue)
        {
            var fInfo = new FileInfo(filePath + @"\" + fileName);
            var messageObject = new IdentifyPillarsForPutFileRequest
                {
                    AuditTrailInformation = "PUT AuditTrailInformation",
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    FileID = fileName,
                    FileSize = fInfo.Length.ToString(CultureInfo.InvariantCulture),
                    From = CLIENT_ID,
                    minVersion = "1",
                    ReplyTo = client_topic,
                    To = pillar_queue,
                    version = "9"
                };

            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        private static ITextMessage MakeIdentifyPillarsForGetFileRequest(ISession session, string colId, string fileName, string client_topic, string pillar_queue)
        {
            var messageObject = new IdentifyPillarsForGetFileRequest
                {
                    AuditTrailInformation = "GET AuditTrailInformation",
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    FileID = fileName,
                    From = CLIENT_ID,
                    minVersion = "1",
                    ReplyTo = client_topic,
                    To = TOPIC_NAME,
                    version = "9"
                };
            return MakeITMessage(session, colId, messageObject,client_topic);
        }

       

        private static ITextMessage MakeIdentifyPillarsForDeleteFileRequest(ISession session, string colId,
                                                                           string fileName, string client_topic, string pillar_queue)
        {
            ITextMessage itMessage = session.CreateTextMessage();
            // Opretter og fylder IdentifyPillarsForDeleteFileRequest
            var messageObject = new IdentifyPillarsForDeleteFileRequest
                {
                    AuditTrailInformation = "DELETE AuditTrailInformation",
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    FileID = fileName,
                    From = CLIENT_ID,
                    minVersion = "1",
                    ReplyTo = client_topic,
                    To = TOPIC_NAME,
                    version = "9"
                };

            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        public static ITextMessage MakeIdentifyPillarsForGetChecksumsRequest(ISession session, string colId, string fileName, string client_topic)
        {
            var encoding = new ASCIIEncoding();
            byte[] hexBytes = encoding.GetBytes("sjopremfvxvfdasdjsdapoerwqw");

            ITextMessage itMessage = session.CreateTextMessage();

            // Opretter og fylder IdentifyPillarsForPutFileRequest
            var chkSpk = new ChecksumSpec_TYPE {ChecksumSalt = hexBytes, ChecksumType = ChecksumType.MD5};
            var fIDs = new FileIDs();
            if (fileName == "AllFileIDs")
            {
                var getAll = new object();
                fIDs.Item = getAll;
            }
            else
            {
                fIDs.Item = fileName;
            }
            var messageObject = new IdentifyPillarsForGetChecksumsRequest
                {
                    AuditTrailInformation = "idCHECKSUMS AuditTrailInformation",
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    ChecksumRequestForExistingFile = chkSpk,
                    FileIDs = fIDs,
                    From = CLIENT_ID,
                    minVersion = "1",
                    ReplyTo = client_topic,
                    To = TOPIC_NAME,
                    version = "9"
                };

            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        private static ITextMessage MakeIdentifyPillarsForGetFileIDsRequest(ISession session, string colId, string fileName, string client_topic)
        {
            string[] files = Directory.GetFiles(filePath);
            //int fileCount = 0;
            ITextMessage itMessage = session.CreateTextMessage();
            // Opretter og fylder IdentifyPillarsForPutFileRequest

            var fIDs = new FileIDs();
            if (fileName == "AllFileIDs")
            {
                var getAll = new object();
                fIDs.Item = getAll;
            }
            else
            {
                fIDs.Item = fileName;
            }
            var messageObject = new IdentifyPillarsForGetFileIDsRequest
                {
                    AuditTrailInformation = "idCHECKSUMS AuditTrailInformation",
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    FileIDs = fIDs,
                    From = CLIENT_ID,
                    minVersion = "1",
                    ReplyTo = client_topic,
                    To = TOPIC_NAME,
                    version = "9"
                };

            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        private static ITextMessage GetChecksumsRequest(ISession session, string colId, string fileName, string client_topic)
        {
            var encoding = new ASCIIEncoding();
            byte[] hexBytes = encoding.GetBytes("sjopremfvxvfdasdjsdapoerwqw");

            ITextMessage itMessage = session.CreateTextMessage();
            var chkSpk = new ChecksumSpec_TYPE
                {
                    ChecksumSalt = hexBytes,
                    //ChecksumType = ChecksumType.MD5
                    ChecksumType = ChecksumType.HMAC_SHA512
                };

            var fIDs = new FileIDs();
            if (fileName == "AllFileIDs")
            {
                var getAll = new object();
                fIDs.Item = getAll;
            }
            else
            {
                fIDs.Item = fileName;
            }

            var messageObject = new GetChecksumsRequest
                {
                    AuditTrailInformation = "AuditTing fra",
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    ChecksumRequestForExistingFile = chkSpk,
                    FileIDs = fIDs,
                    From = CLIENT_ID,
                    minVersion = "1",
                    PillarID = "SA_testPillar",
                    ReplyTo = client_topic,
                    To = TOPIC_NAME,
                    version = "9"
                };
            //messageObject.ResultAddress = "";

            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        private static ITextMessage MakeGetFileIDsRequest(ISession session, string colId, string fileName, string client_topic,string pillarID)
        {
            string[] files = Directory.GetFiles(filePath);
            //int fileCount = 0;

            ITextMessage ITMessage = session.CreateTextMessage();


            var fIDs = new FileIDs();
            if (fileName == "AllFileIDs")
            {
                var getAll = new object();
                fIDs.Item = getAll;
            }
            else
            {
                fIDs.Item = fileName;
            }
            var messageObject = new GetFileIDsRequest
                {
                    AuditTrailInformation = "AuditTing fra",
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    FileIDs = fIDs,
                    From = CLIENT_ID,
                    minVersion = "1",
                    PillarID = pillarID,
                    ReplyTo = client_topic,
                    To = TOPIC_NAME,
                    version = "9"
                };
            //messageObject.ResultAddress = "";


            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        // ikke færdig
        private static ITextMessage MakeDeleteFileRequest(ISession session, string colId, string fileName, string client_topic, string pillarID)
        {
            ITextMessage itMessage = session.CreateTextMessage();


            var file = new FileStream(filePath + @"\" + fileName, FileMode.Open);
            var chkSpk3 = new ChecksumSpec_TYPE {ChecksumType = ChecksumType.MD5};
            MD5 md5 = MD5.Create();
            byte[] resultArray3 = md5.ComputeHash(file);
            md5.Dispose();
            file.Dispose();

            // Hvorfor er der to checksumting her?
            var chkDataType = new ChecksumDataForFile_TYPE
                {
                    CalculationTimestamp = DateTime.Now,
                    ChecksumSpec = chkSpk3,
                    ChecksumValue = resultArray3
                };
            var messageObject = new DeleteFileRequest
                {
                    AuditTrailInformation = "AuditTing fra",
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    FileID = fileName,
                    From = CLIENT_ID,
                    minVersion = "1",
                    PillarID = pillarID,
                    ReplyTo = client_topic,
                    To = TOPIC_NAME,
                    version = "9",
                    ChecksumDataForExistingFile = chkDataType,
                    ChecksumRequestForExistingFile = chkSpk3
                };
            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        private static ITextMessage MakeGetFileRequest(ISession session, string colId, string fileName, string client_topic, string pillarID)
        {
            //const string filePath = @"D:\testFiler\";
            const string filePath = @"C:\testFiler\downloaded\";
            var fInfo = new FileInfo(filePath + fileName);

            ITextMessage itMessage = session.CreateTextMessage();
            var FP = new FilePart {PartLength = "", PartOffSet = ""};
            var messageObject = new GetFileRequest
                {
                    AuditTrailInformation = "AuditTing fra",
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    FileAddress = @"http://localhost:443/",
                    FileID = fileName,
                    FilePart = FP,
                    From = CLIENT_ID,
                    minVersion = "1",
                    PillarID = pillarID,
                    ReplyTo = client_topic,
                    To = TOPIC_NAME,
                    version = "9"
                };
            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        private static ITextMessage MakePutFileRequest(ISession session, string colId, string fileName, string client_topic, string pillarID)
        {
            var fInfo = new FileInfo(filePath + @"\" + fileName);
            ITextMessage ITMessage = session.CreateTextMessage();
            


            var file = new FileStream(filePath + @"\" + fileName, FileMode.Open);
            var chkSpk3 = new ChecksumSpec_TYPE {ChecksumType = ChecksumType.MD5};
            MD5 md5 = MD5.Create();
            byte[] resultArray3 = md5.ComputeHash(file);
            md5.Dispose();
            file.Dispose();

            var checksumDataForFileType = new ChecksumDataForFile_TYPE
                {
                    CalculationTimestamp = DateTime.Now,
                    ChecksumSpec = chkSpk3,
                    ChecksumValue = resultArray3
                };
            var messageObject = new PutFileRequest
                {
                    ChecksumRequestForNewFile = chkSpk3,
                    ChecksumDataForNewFile = checksumDataForFileType,
                    AuditTrailInformation = "Audittrail fra Putfile", 
                    CollectionID = colId, 
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    FileAddress = @"http://localhost:443/",
                    FileID = fileName, 
                    FileSize = fInfo.Length.ToString(CultureInfo.InvariantCulture),
                    From = CLIENT_ID,
                    minVersion = "1", 
                    PillarID = pillarID,
                    ReplyTo = client_topic, 
                    To = TOPIC_NAME,
                    version = "9"
                };
            // Serialiserer message ned i en memorystream.
            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        private static ITextMessage MakeReplaceFileRequest(ISession session, string colId, string fileName, string client_topic, string pillarID)
        {
            var fInfo = new FileInfo(filePath + @"\" + fileName);
            var file = new FileStream(filePath + @"\" + fileName, FileMode.Open);
            var chkSpk3 = new ChecksumSpec_TYPE {ChecksumType = ChecksumType.MD5};
            MD5 md5 = MD5.Create();
            byte[] resultArray3 = md5.ComputeHash(file);
            md5.Dispose();
            file.Dispose();

            //var chkSpk = new ChecksumSpec_TYPE { ChecksumSalt = hexBytes, ChecksumType = ChecksumType.HMAC_MD5 };
            var checksumDataForExistingFile = new ChecksumDataForFile_TYPE
                {
                    CalculationTimestamp = DateTime.Now,
                    ChecksumSpec = chkSpk3,
                    ChecksumValue = resultArray3
                };
            var checksumDataForNewFile = new ChecksumDataForFile_TYPE
                {
                    CalculationTimestamp = DateTime.Now,
                    ChecksumSpec = chkSpk3,
                    ChecksumValue = resultArray3
                };

            var messageObject = new ReplaceFileRequest
                {
                    AuditTrailInformation = "AuditShit",
                    ChecksumDataForExistingFile = checksumDataForExistingFile,
                    ChecksumDataForNewFile = checksumDataForNewFile,
                    ChecksumRequestForExistingFile = chkSpk3,
                    ChecksumRequestForNewFile = chkSpk3,
                    CollectionID = colId,
                    CorrelationID = Guid.NewGuid().ToString("N"),
                    FileAddress = @"http://localhost:443/",
                    FileID = fileName,
                    FileSize = fInfo.Length.ToString(CultureInfo.InvariantCulture),
                    From = CLIENT_ID,
                    PillarID = pillarID,
                    ReplyTo = client_topic,
                    To = TOPIC_NAME,
                    minVersion = "1",
                    version = "9"
                };
            return MakeITMessage(session, colId, messageObject,client_topic);
        }

        private static ITextMessage MakeITMessage(ISession session, string colId, Object messageObject, string client_topic)
        {
            ITextMessage itMessage = session.CreateTextMessage();
            itMessage.NMSType = messageObject.GetType().Name.ToString(CultureInfo.InvariantCulture);
            itMessage.Properties["org.bitrepository.messages.type"] = messageObject.GetType().Name.ToString(CultureInfo.InvariantCulture);
            itMessage.Properties["org.bitrepository.messages.collectionid"] = colId;
            itMessage.NMSReplyTo = session.GetDestination(client_topic);
            itMessage.Text = NNSUtilities.Serialize(messageObject);
            Console.WriteLine("----------- " + session.GetDestination(client_topic) + " ---------------");
            Console.WriteLine(itMessage.Text);
            return itMessage;
        }

        public static bool ValidateXmlMessage(string message, string xsdPath)
        {
            bool isValid = true;
            try
            {
                var settings = new XmlReaderSettings();
                settings.Schemas.Add(null, xsdPath);
                settings.ValidationType = ValidationType.Schema;
                var document = new XmlDocument();
                document.LoadXml(message);
                XmlReader rdr = XmlReader.Create(new StringReader(document.InnerXml), settings);
                while (rdr.Read())
                {
                }
            }
            catch (Exception e)
            {
                isValid = false;
                Console.WriteLine(e);
            }

            return isValid;
        }
    }
}