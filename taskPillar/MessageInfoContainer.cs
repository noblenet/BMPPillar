using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Apache.NMS;
using PillarAPI.ActiveMQ;
using PillarAPI.CustomExceptions;
using PillarAPI.Interfaces;
using PillarAPI.Utilities;
using bmpxsd;
using log4net;

namespace PillarAPI
{
    public class MessageInfoContainer : IMessageInfoContainer, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public MessageInfoContainer(IdentifyPillarsForPutFileResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(IdentifyPillarsForGetFileResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(IdentifyPillarsForDeleteFileResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(IdentifyPillarsForReplaceFileResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(IdentifyPillarsForGetFileIDsResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(IdentifyPillarsForGetChecksumsResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(IdentifyContributorsForGetAuditTrailsResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(IdentifyContributorsForGetStatusResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(GetStatusFinalResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(GetAuditTrailsFinalResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(ReplaceFileFinalResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(GetFileIDsFinalResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(DeleteFileFinalResponse message)
        {
            InitWithObject(message);
        }


        public MessageInfoContainer(ITextMessage message)
        {
            InitWithMessage(message);
        }

        public MessageInfoContainer(GetChecksumsRequest message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(GetChecksumsFinalResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(PutFileFinalResponse message)
        {
            InitWithObject(message);
        }

        public MessageInfoContainer(GetFileFinalResponse message)
        {
            InitWithObject(message);
        }

        #endregion

        private ResponseInfo ResponseInfoField { get; set; }

        #region IMessageInfoContainer Members

        public bool IsSerializedMessageValid { get; private set; }
        public string CorrelationId { get; private set; }
        public string Destination { get; private set; }
        public Type MessageType { get; private set; }
        public string MessageTypeName { get; private set; }
        public string SerializedMessage { get; private set; }
        public object MessageObject { get; private set; }
        public bool IsFromValidCollection { get; private set; }
        public string SignedMessage { get; private set; }

        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private void InitWithObject(object message)
        {
            var generalMessage = message as Message;
            if (generalMessage != null) CorrelationId = generalMessage.CorrelationID;
            if (generalMessage != null) Destination = generalMessage.Destination;
            MessageType = message.GetType();
            SerializedMessage = SerializationUtilities.Serialize(message);
            IsSerializedMessageValid = XmlUtilities.ValidateXmlMessage(SerializedMessage);
            //IsSerializedMessageValid = SerializationUtilities.ValidateXmlMessage(SerializedMessage);
            MessageObject = message;
            SignedMessage = signMessage();
            SetResponseInfo(message);
            if (generalMessage != null)
                IsFromValidCollection = generalMessage.CollectionID.Equals(Pillar.GlobalPillarApiSettings.COLLECTION_ID);
            Log.Debug("Message: " + message);
        }


        private void InitWithMessage(ITextMessage message)
        {
            const string namespace4Types = "bmpxsd";
            MessageTypeName = FindMsgtype(message);
            string typeName = namespace4Types + "." + MessageTypeName;
            MessageType = Type.GetType(typeName);
            MessageObject = SerializationUtilities.DeserializeObject(message.Text, MessageType);
            var generalMessage = MessageObject as Message;
            if (generalMessage != null) CorrelationId = generalMessage.CorrelationID;
            if (generalMessage != null) Destination = generalMessage.Destination;
            SerializedMessage = message.Text;
            IsSerializedMessageValid = SerializationUtilities.ValidateXmlMessage(SerializedMessage);
            SignedMessage = signMessage();
            SetResponseInfo(message);
            if (generalMessage != null)
                IsFromValidCollection = generalMessage.CollectionID.Equals(Pillar.GlobalPillarApiSettings.COLLECTION_ID);
            Log.Debug("Message: " + message);
        }

        private string FindMsgtype(ITextMessage message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message.Properties["org.bitrepository.messages.type"].ToString()))
                {
                    return message.Properties["org.bitrepository.messages.type"].ToString();
                }
                if (!string.IsNullOrEmpty(message.NMSType))
                {
                    return message.NMSType;
                }
                // Encode the XML string in a UTF-8 byte array
                byte[] encodedString = Encoding.UTF8.GetBytes(message.Text);
                // Put the byte array into a stream and rewind it to the beginning
                using (var ms = new MemoryStream(encodedString))
                {
                    ms.Flush();
                    ms.Position = 0;
                    // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(ms);
                    XmlElement elm = xmlDoc.DocumentElement;
                    if (elm == null) throw new ArgumentNullException("");
                    return elm.Name.Replace("ns2:", "").Trim();
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat(e.ToString());
                throw;
            }
        }


        private void SetResponseInfo(object message)
        {
            PropertyInfo[] propertyInfos = message.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (propertyInfo.Name == "ResponseInfo")
                {
                    ResponseInfoField = (ResponseInfo) propertyInfo.GetValue(message);
                }
            }
        }

        internal void Send()
        {
            if (!IsSerializedMessageValid) return;
            using (var messageSender = new ActiveMqITextMessage(this))
            {
                messageSender.Send();
            }
        }

        private string signMessage()
        {
            try
            {
                X509Certificate2 privateCertificate =
                    CmsMessageUtilities.GetCertificate(Pillar.GlobalPillarApiSettings.USER_CERTIFICATES_STORE,
                                                       Pillar.GlobalPillarApiSettings.PRIVATE_CERTIFICATE_THUMBPRINT);
                return CmsMessageUtilities.CmsMessageSigner(privateCertificate, SerializedMessage);
            }
            catch (Exception e)
            {
                Log.ErrorFormat(
                    "Error signing message. There might be something wrong with the PRIVATE_CERTIFICATES_STORE name or the certificate is missing.");
                throw new SigningVerifyingMessageException(
                    "Error signing message. Check the certificate and/or the trust store.", e);
            }
        }
    }
}