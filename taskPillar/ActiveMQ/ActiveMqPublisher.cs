using System;
using Apache.NMS;

namespace PillarAPI.ActiveMQ
{
    public class ActiveMqPublisher : IDisposable
    {
        private readonly ISession _session;
        private bool _disposed;

        public ActiveMqPublisher(IDestination destinationName)
        {
            _session = ActiveMQSetup.GetSession(Pillar.GlobalConnection);
            Producer = _session.CreateProducer(destinationName);
        }

        private IMessageProducer Producer { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            if (_disposed) return;
            Producer.Close();
            Producer.Dispose();
            _session.Dispose();
            _disposed = true;
        }

        #endregion

        public void SendMessage(ITextMessage message)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            Producer.Send(message);
        }
    }
}