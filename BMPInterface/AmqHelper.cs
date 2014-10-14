using System;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;

namespace BMPInterface
{
    public class TSubscriber : IDisposable
    {
        private readonly ISession _session;
        private readonly ITopic _topic;
        private readonly string _destination;
        private bool _disposed;

        public delegate void MessageReceivedDelegate(ITextMessage message);

        public TSubscriber(ISession session, string destination)
        {
            this._session = session;
            this._destination = destination;
            _topic = new ActiveMQTopic(this._destination);
        }

        public event MessageReceivedDelegate OnMessageReceived;

        public IMessageConsumer Consumer { get; private set; }

        public string ConsumerId { get; private set; }

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

        public void Dispose()
        {
            if (!_disposed)
            {
                Consumer.Close();
                Consumer.Dispose();
                _disposed = true;
            }
        }
    }

    public class QSubscriber : IDisposable
    {
        private readonly ISession _session;
        private readonly IQueue _queue;
        private readonly string _destination;
        private bool _disposed;

        public delegate void MessageReceivedDelegate(ITextMessage message);

        public QSubscriber(ISession session, string destination)
        {
            this._session = session;
            this._destination = destination;
            _queue = new ActiveMQQueue(this._destination);
        }

        public event MessageReceivedDelegate OnMessageReceived;

        public IMessageConsumer Consumer { get; private set; }

        public string ConsumerId { get; private set; }

        public void Start(string consumerId)
        {
            ConsumerId = consumerId;
            Consumer = _session.CreateConsumer(_queue, null, false);
            Consumer.Listener += (message =>
            {
                ITextMessage textMessage = message as ITextMessage;
                if (textMessage == null) throw new InvalidCastException();
                if (OnMessageReceived != null)
                {
                    OnMessageReceived(textMessage);
                }
            });
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Consumer.Close();
                Consumer.Dispose();
                _disposed = true;
            }
        }
    }

    public class Publisher : IDisposable
    {
        private bool _disposed;
        private readonly IQueue _queue;
        private readonly ITopic _topic;

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

        public void SendMessage(ITextMessage message)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            ITextMessage textMessage = message;
            Producer.Send(textMessage);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Producer.Close();
            Producer.Dispose();
            _disposed = true;
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
        
        public void SendMessage(ITextMessage message)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            Producer.Send(message);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Producer.Close();
            Producer.Dispose();
            _disposed = true;
        }
    }
}
