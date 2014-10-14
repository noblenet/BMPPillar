using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Apache.NMS;
using PillarAPI.ActiveMQ;
using PillarAPI.Enums;
using PillarAPI.Interfaces;
using PillarAPI.Utilities;
using log4net;
using pillarAPI;

namespace PillarAPI
{
    public class Pillar : IPillar
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static IConnection GlobalConnection;
        internal static PillarApiSettings GlobalPillarApiSettings;
        private readonly IOC _ioc;
        private bool _keepRunning;

        public Pillar(PillarTypeEnum pillarType)
        {
            _ioc = new IOC(pillarType);
            Log.InfoFormat("Pillar created of type: {0}", pillarType);
            _keepRunning = true;
        }


        public void Initialize(PillarApiSettings pillarApiSettings)
        {
            //   throw new NotImplementedException("test"); // To force an exception
            GlobalPillarApiSettings = pillarApiSettings;
            VerifyDBIsReady();
            VerifyStoreIsReady();
            DBUtilities.CleanFilesFromDBIfLeftHanging();

            var cts = new CancellationTokenSource();
            GlobalConnection = ActiveMQSetup.Connection;
            GlobalConnection.Start();

            // Pass the same token source to the delegate and to the task instance.
            Log.Debug("Destination: " + GlobalPillarApiSettings.COLLECTION_DESTINATION);

            IdentifyPillarsGeneralTopicListener ipgtl = _ioc.GetIdentifyPillarsGeneralTopicListener();
            Task.Factory.StartNew(() => ipgtl.DoWorkGeneralTopic(cts.Token), cts.Token);

            PillarQueueListener pillarQueueListener = _ioc.GetPillarQueueListener();

            Task.Factory.StartNew(() => pillarQueueListener.DoWorkSAPillarQueue(cts.Token), cts.Token);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            while (_keepRunning)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }
            Log.Debug("er forbi _keepRunning");
            cts.Cancel();
            Log.Debug("er forbi cancel token");
            GlobalConnection.Close();
        }

        private void VerifyDBIsReady()
        {
            Log.InfoFormat("Connected to the database with '{0}'", Pillar.GlobalPillarApiSettings.SQLITE_CONNECTION_STRING);
            Log.Info(DBUtilities.PrintInfo());
        }

        public void KillPillar()
        {
            _keepRunning = false;
        }

        private void VerifyStoreIsReady()
        {
            if (StorageUtility.IsAccesable())
            {
                Log.Info("Storage accesable");
                if (StorageUtility.IsWritable())
                {
                    Log.DebugFormat("Storage has {0} directories", StorageUtility.DirCount());
                    Log.DebugFormat("Storage contains {0} files", StorageUtility.FileCount());
                }
                else
                {
                    throw new StorageNotWritableException();
                }
            }
            else
            {
                throw new StorageNotAccessableException();
            }
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = (Exception) e.ExceptionObject;
                Log.Error("Unhandled exception", ex);
                MessageBox.Show("Whoops! Please contact the developers with the following"
                                + " information:\n\n" + ex.Message + ex.StackTrace,
                                "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            finally
            {
                Application.Exit();
            }
        }
    }

    internal class StorageNotWritableException : Exception
    {
    }

    internal class StorageNotAccessableException : Exception
    {
    }
}