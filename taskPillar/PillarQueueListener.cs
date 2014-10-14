using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Apache.NMS;
using log4net;
using PillarAPI.ActiveMQ;
using PillarAPI.Interfaces;
using PillarAPI.Utilities;

namespace PillarAPI
{
    public  class PillarQueueListener
    {
        private readonly IPillarWrapper _pillarWrapper;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public PillarQueueListener(IPillarWrapper pillarWrapper)
        {
            _pillarWrapper = pillarWrapper;
        }

        public void DoWorkSAPillarQueue(CancellationToken token)
        {
            IConnection connection = Pillar.GlobalConnection;
            IDestination destination = ActiveMQSetup.GetDestination(ActiveMQSetup.GetSession(connection),Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE);
            var messagequeueSubscriber = new ActiveMqQueueSubscriber(ActiveMQSetup.GetSession(connection), destination);
            X509Certificate2 publicCertificate = CmsMessageUtilities.GetCertificate(Pillar.GlobalPillarApiSettings.USER_CERTIFICATES_STORE, Pillar.GlobalPillarApiSettings.PUBLIC_CERTIFICATE_THUMBPRINT);
            messagequeueSubscriber.Start(Pillar.GlobalPillarApiSettings.SA_QUEUE_SUBSCRIBER);
            MessageInfoContainer messageInfoContainer;
            messagequeueSubscriber.OnMessageReceived += message =>
                {
                    Log.Debug(message.Text);

                    messageInfoContainer = new MessageInfoContainer(message);
                    if (!messageInfoContainer.IsFromValidCollection) return;
                    if (!string.IsNullOrEmpty(message.Properties["org.bitrepository.messages.signature"].ToString()) &&
                        CmsMessageUtilities.CmsSignedMessageVerifier(publicCertificate, message.Properties["org.bitrepository.messages.signature"].ToString()))
                    {
                        Log.DebugFormat("Messagetype: {0}", messageInfoContainer.MessageTypeName);
                        try
                        {
                            ExecuteRequest(messageInfoContainer);
                            message.Acknowledge();
                            Log.DebugFormat("Message '{0}' is acknowledged", message);
                            GC.Collect();
                        }
                        catch (Exception e)
                        {
                            Log.Error(e.Message, e);
                            throw;
                        }
                    }
                };
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(500);
            }
            if (token.IsCancellationRequested)
            {
                Log.InfoFormat("Cancellation requested");
                messagequeueSubscriber.Dispose();
                token.ThrowIfCancellationRequested();
            }
            Log.Debug("Listener terminated");
        }

        private  void ExecuteRequest(IMessageInfoContainer message)
        {
            switch (message.MessageTypeName)
            {
                case "GetFileRequest":
                    _pillarWrapper.GetFile(message);
                    break;
                case "PutFileRequest":
                    _pillarWrapper.PutFile(message);
                    break;
                case "GetFileIDsRequest":
                    _pillarWrapper.GetFileId(message);
                    break;
                case "GetChecksumsRequest":
                    _pillarWrapper.GetChecksum(message);
                    break;
                case "DeleteFileRequest":
                    _pillarWrapper.DeleteFile(message);
                    break;
                case "ReplaceFileRequest":
                    _pillarWrapper.ReplaceFile(message);
                    break;
                case "GetAuditTrailsRequest":
                    _pillarWrapper.GetAuditTrail(message);
                    break;
                case "GetStatusRequest":
                    _pillarWrapper.GetStatus(message);
                    break;
                default:
                    Log.ErrorFormat("Unknown Messagetype: '{0}'", message.MessageTypeName);
                    break;
            }
        }
    }
}