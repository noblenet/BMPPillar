using System;
using System.Reflection;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using PillarAPI.CustomExceptions;
using log4net;

namespace PillarAPI.ActiveMQ
{
    /// <summary>
    ///     Uses singleton pattern to garantie global connection to ActiveMQ
    /// </summary>
    internal static class ActiveMQSetup
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IConnection _tempvar;

        public static IConnection Connection
        {
            get
            {
                Log.Debug("Trying to get a GlobalConnection");
                Log.Debug("Using: " + Pillar.GlobalPillarApiSettings.MESSAGE_BUS_CONFIGURATION_URL + " " +
                          Pillar.GlobalPillarApiSettings.PILLAR_ID);
                try
                {
                    IConnectionFactory connectionFactory =
                        new ConnectionFactory(Pillar.GlobalPillarApiSettings.MESSAGE_BUS_CONFIGURATION_URL,
                                              Pillar.GlobalPillarApiSettings.PILLAR_ID);
                    connectionFactory.RedeliveryPolicy.MaximumRedeliveries = 0;
                    return _tempvar ?? (_tempvar = connectionFactory.CreateConnection());
                }
                catch (Exception e)
                {
                    Log.Fatal(e);
                    throw new QueueNotAccessibleException(e.Message, e.InnerException);
                }
            }
        }

        public static ISession GetSession(IConnection connection)
        {
            Log.Debug("Trying to get a Session");
            return connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
        }

        public static IDestination GetDestination(ISession session, string destination)
        {
            Log.DebugFormat("Trying to get a Destination to '{0}'", destination);
            return session.GetDestination(destination);
        }
    }
}