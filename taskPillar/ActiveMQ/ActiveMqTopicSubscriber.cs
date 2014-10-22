using System;
using System.Reflection;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;
using log4net;

namespace PillarAPI.ActiveMQ
{
    public class ActiveMqTopicSubscriber : IDisposable
    {
        public delegate void MessageReceivedDelegate(ITextMessage message);

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDestination _destination;
        private readonly ISession _session;
        private readonly ITopic _topic;
        private bool _disposed;

        public ActiveMqTopicSubscriber(ISession session, IDestination destination)
        {
            _session = session;
            _destination = destination;
            if (destination.IsTopic)
            {
                _topic = (ITopic) _destination;
            }
        }

        public ActiveMqTopicSubscriber(ISession session, string destination)
        {
            _session = session;
            _destination = _session.GetDestination(destination);
            _topic = new ActiveMQTopic(destination);
        }

        public IMessageConsumer Consumer { get; private set; }
        public string ConsumerId { get; private set; }

        public void Dispose()
        {
            if (_disposed) return;
            Consumer.Close();
            Consumer.Dispose();
            _session.Dispose();
            _disposed = true;
        }

        public event MessageReceivedDelegate OnMessageReceived;

        public void Start(string consumerId)
        {
            Log.Debug("Enter start: " + DateTime.Now);

            ConsumerId = consumerId;
            //Consumer = _session.CreateDurableConsumer(_topic, consumerId, null, false);
            Consumer = _session.CreateDurableConsumer(_topic, consumerId, null, false);
            Log.Debug("Setter listener");
            Consumer.Listener += message =>
                                     {
                                         var textMessage = message as ITextMessage;
                                         try
                                         {
                                             if (textMessage == null) throw new InvalidCastException();
                                             if (OnMessageReceived == null) return;
                                             message.Acknowledge();
                                             Log.Debug("Message has been acknowledged");
                                             OnMessageReceived(textMessage);
                                         }
                                         catch (Exception ex)
                                         {
                                             Log.Error(ex.Message, ex);
                                             throw;
                                         }
                                     };
            Log.Debug("Exit");
        }
    }
}