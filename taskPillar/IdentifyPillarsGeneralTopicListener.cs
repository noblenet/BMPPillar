using System;
using System.Reflection;
using System.Threading;
using Apache.NMS;
using log4net;
using PillarAPI.ActiveMQ;
using PillarAPI.IdentifyResponses;
using PillarAPI.Interfaces;

namespace PillarAPI
{
    public  class IdentifyPillarsGeneralTopicListener
    {
        /// <summary>
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IResponseBuilderForIdentifyPillarsForGetFileRequest ResponseBuilderForIdentifyPillarsForGetFileRequest { get;  set; }
        public  void DoWorkGeneralTopic(CancellationToken token)
        {
            IConnection connection = Pillar.GlobalConnection;
            IDestination destination = ActiveMQSetup.GetDestination(ActiveMQSetup.GetSession(connection), Pillar.GlobalPillarApiSettings.COLLECTION_DESTINATION);
            var messageTopicSubscriber = new ActiveMqTopicSubscriber(ActiveMQSetup.GetSession(connection), destination);

            Log.Debug("Entered IdentifyPillarsGeneralTopicListener");
            messageTopicSubscriber.Start(Pillar.GlobalPillarApiSettings.GENERAL_TOPIC_SUBSCRIBER);
            messageTopicSubscriber.OnMessageReceived += message =>
                {

                    Log.Debug("Caught a message from General topic/n" + message.Text );
                    var messageInfoContainer = new MessageInfoContainer(message);
                    //MessageBox.Show(messageInfoContainer.IsSerializedMessageValid.ToString() + "\n\n" + messageInfoContainer.SerializedMessage );
                    if (messageInfoContainer.IsFromValidCollection)
                    {
                        Log.DebugFormat("Messagetype: {0}", messageInfoContainer.MessageTypeName);
                        try
                        {
                            MakeIdentifyResponse(messageInfoContainer);
                            message.Acknowledge();
                            Log.DebugFormat("Message '{0}' is acknowledged", message);
                        }
                        catch (Exception e)
                        {
                            Log.Warn(e.Message, e);
                            throw;
                        }
                    }
                };
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }
            Log.Debug("No longer listens for messages in IdentifyPillarsGeneralTopicListener");
            if (token.IsCancellationRequested)
            {
                messageTopicSubscriber.Dispose();
                // Stops the task that runs the method
                token.ThrowIfCancellationRequested();
            }
        }

        private  void MakeIdentifyResponse(MessageInfoContainer messageInfoContainer)
        {
            switch (messageInfoContainer.MessageTypeName)
            {
                case "IdentifyPillarsForPutFileRequest":
                    ResponseBuilderForIdentifyPillarsForPutFileRequest.MakeResponse(messageInfoContainer);
                    break;
                case "IdentifyPillarsForGetFileRequest":
                    ResponseBuilderForIdentifyPillarsForGetFileRequest.SendResponse(messageInfoContainer);
                    break;
                case "IdentifyPillarsForDeleteFileRequest":
                    ResponseBuilderForIdentifyPillarsForDeleteFileRequest.MakeResponse(messageInfoContainer);
                    break;
                case "IdentifyPillarsForGetFileIDsRequest":
                    ResponseBuilderForIdentifyPillarsForGetFileIDsRequest.MakeResponse(messageInfoContainer);
                    break;
                case "IdentifyPillarsForGetChecksumsRequest":
                    ResponseBuilderForIdentifyPillarsForGetCheckSumsRequest.MakeResponse(messageInfoContainer);
                    break;
                case "IdentifyPillarsForReplaceFileRequest":
                    ResponseBuilderForIdentifyPillarsForReplaceFileRequest.MakeResponse(messageInfoContainer);
                    break;
                case "IdentifyContributorsForGetAuditTrailsRequest":
                    ResponseBuilderForIdentifyContributorsForGetAuditTrailsRequest.MakeResponse(messageInfoContainer);
                    break;
                default:
                    Log.ErrorFormat("Unknown Messagetype: '{0}'", messageInfoContainer.MessageTypeName);
                    break;
            }
        }
    }
}