using System;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;

namespace PseudoKlient
{
    public class TSubscriber : IDisposable
    {
        public delegate void MessageReceivedDelegate(ITextMessage message);

        private readonly string _destination;

        private readonly ISession _session;
        private readonly ITopic _topic;
        private bool _disposed;

        public TSubscriber(ISession session, string destination)
        {
            _session = session;
            _destination = destination;
            _topic = new ActiveMQTopic(_destination);
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
            Consumer = _session.CreateDurableConsumer(_topic, consumerId, null, false);
            Consumer.Listener += (message =>
                {
                    var textMessage = message as ITextMessage;
                    if (textMessage == null) throw new InvalidCastException();
                    if (OnMessageReceived != null)
                    {
                        OnMessageReceived(textMessage);
                    }
                });
        }
    }

    public class QSubscriber : IDisposable
    {
        public delegate void MessageReceivedDelegate(ITextMessage message);

        private readonly string _destination;
        private readonly IQueue _queue;
        private readonly ISession _session;
        private bool _disposed;

        public QSubscriber(ISession session, string destination)
        {
            _session = session;
            _destination = destination;
            _queue = new ActiveMQQueue(_destination);
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
            Consumer = _session.CreateConsumer(_queue, null, false);
            Consumer.Listener += (message =>
                {
                    var textMessage = message as ITextMessage;
                    if (textMessage == null) throw new InvalidCastException();
                    if (OnMessageReceived != null)
                    {
                        OnMessageReceived(textMessage);
                    }
                });
        }
    }

    public class Publisher : IDisposable
    {
        private readonly IQueue _queue;
        private readonly ITopic _topic;
        private bool _disposed;

        public Publisher(ISession session, string destinationTopicName, bool isQueue)
        {
            if (isQueue)
            {
                _queue = new ActiveMQQueue(destinationTopicName);
                Producer = session.CreateProducer(_queue);
            }
            else
            {
                _topic = new ActiveMQTopic(destinationTopicName);
                Producer = session.CreateProducer(_topic);
            }
        }

        public IMessageProducer Producer { get; private set; }
        public string DestinationName { get; private set; }

        public void Dispose()
        {
            if (_disposed) return;
            Producer.Close();
            Producer.Dispose();
            _disposed = true;
        }

        public void SendMessage(ITextMessage message)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            ITextMessage textMessage = message;
            Producer.Send(textMessage);
        }
    }

    public class Publisher2 : IDisposable
    {
        private bool _disposed;

        public Publisher2(ISession session, IDestination dest)
        {
            Producer = session.CreateProducer(dest);
        }

        private IMessageProducer Producer { get; set; }

        public void Dispose()
        {
            if (_disposed) return;
            Producer.Close();
            Producer.Dispose();
            _disposed = true;
        }

        public void SendMessage(ITextMessage message)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            Producer.Send(message);
        }
    }
}