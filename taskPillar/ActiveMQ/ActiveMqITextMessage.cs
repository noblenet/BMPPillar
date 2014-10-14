using System;
using System.Reflection;
using Apache.NMS;
using log4net;
using PillarAPI.Interfaces;

namespace PillarAPI.ActiveMQ
{
    public class ActiveMqITextMessage : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ITextMessage _itMessage;
        private readonly ISession _session;
        //private readonly string _fileName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ActiveMqITextMessage" /> class.
        /// </summary>
        /// <param name="messageInfoContainer">The message info container.</param>
        public ActiveMqITextMessage(IMessageInfoContainer messageInfoContainer)
        {
            _session = ActiveMQSetup.GetSession(Pillar.GlobalConnection);
            _itMessage = _session.CreateTextMessage();
            _itMessage.NMSCorrelationID = messageInfoContainer.CorrelationId;
            _itMessage.NMSDestination = _session.GetDestination(messageInfoContainer.Destination);
            _itMessage.NMSReplyTo = _session.GetDestination(Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE);
            _itMessage.NMSTimestamp = DateTime.Now;
            _itMessage.NMSType = messageInfoContainer.MessageType.Name;
            _itMessage.Properties["org.bitrepository.messages.type"] = messageInfoContainer.MessageType.Name;
            _itMessage.Properties["org.bitrepository.messages.collectionid"] = Pillar.GlobalPillarApiSettings.COLLECTION_ID;
            _itMessage.Properties["org.bitrepository.messages.signature"] = messageInfoContainer.SignedMessage;
            _itMessage.Text = messageInfoContainer.SerializedMessage;

        }

        public string GetCorrelationID
        {
            get { return _itMessage.NMSCorrelationID; }
        }

        /// <summary>
        ///     Sends the specified ITextMessage.
        /// </summary>
        /// <param name="session">The session.</param>
        public void Send()
        {
            using (var publisher = new ActiveMqPublisher(_itMessage.NMSDestination))
            {
                    Log.Debug("Sending: " + _itMessage.Text);
                    publisher.SendMessage(_itMessage);
            }
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}