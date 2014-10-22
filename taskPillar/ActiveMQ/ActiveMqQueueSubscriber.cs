using System;
using System.Reflection;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;
using log4net;

namespace PillarAPI.ActiveMQ
{
    public class ActiveMqQueueSubscriber : IDisposable
    {
        public delegate void MessageReceivedDelegate(ITextMessage message);

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDestination _destination;
        private readonly ISession _session;
        private readonly ITopic _topic;
        private bool _disposed;

        public ActiveMqQueueSubscriber(ISession session, IDestination destination)
        {
            _session = session;
            _destination = destination;
            if (destination.IsTopic)
            {
                _topic = (ITopic) _destination;
            }
        }

        public ActiveMqQueueSubscriber(ISession session, string dest)
        {
            _session = session;
            _destination = session.GetDestination(dest);
            _topic = new ActiveMQTopic(dest);
        }

        private IMessageConsumer Consumer { get; set; }

        private string ConsumerId { get; set; }

        public void Dispose()
        {
            if (!_disposed)
            {
                Consumer.Close();
                Consumer.Dispose();
                _session.Dispose();
                _disposed = true;
            }
        }

        public event MessageReceivedDelegate OnMessageReceived;

        public void Start(string consumerId)
        {
            ConsumerId = consumerId;
            Consumer = _topic != null
                           ? _session.CreateDurableConsumer(_topic, consumerId, "2 > 1", false)
                           : _session.CreateConsumer(_destination, null, false);
            Consumer.Listener += message =>
                                     {
                                         var textMessage = message as ITextMessage;
                                         try
                                         {
                                             if (textMessage == null) throw new InvalidCastException();
                                             if (OnMessageReceived == null) return;
                                             message.Acknowledge();
                                             OnMessageReceived(textMessage);
                                         }
                                         catch (Exception ex)
                                         {
                                             Log.Error(ex.Message, ex);
                                             throw;
                                         }
                                     };
        }
    }
}