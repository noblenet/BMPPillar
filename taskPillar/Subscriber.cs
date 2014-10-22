using System;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;

namespace PillarAPI
{
    public class Subscriber : IDisposable
    {
        public delegate void MessageReceivedDelegate(ITextMessage message);

        private readonly IDestination _destination;
        private readonly ISession _session;
        private readonly ITopic _topic;
        private bool _disposed;


        public Subscriber(ISession session, IDestination destination)
        {
            _session = session;
            _destination = destination;
            if (destination.IsTopic)
            {
                _topic = (ITopic) _destination;
            }
        }

        public Subscriber(ISession session, string dest)
        {
            _session = session;
            _destination = session.GetDestination(dest);
            _topic = new ActiveMQTopic(dest);
        }

        public IMessageConsumer Consumer { get; private set; }

        public string ConsumerId { get; private set; }

        public void Dispose()
        {
            if (!_disposed)
            {
                Consumer.Close();
                Consumer.Dispose();
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
                                         if (textMessage == null) throw new InvalidCastException();
                                         if (OnMessageReceived != null)
                                         {
                                             OnMessageReceived(textMessage);
                                         }
                                     };
        }
    }
}