using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;
using PillarAPI;
using PillarAPI.Enums;
using PillarAPI.Interfaces;
using PillarAPI.Utilities;
using log4net;
using pillarAPI;

namespace PillarService
{
    public partial class PillarService : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IPillar _piz;

        public PillarService()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            Log.Debug("Starting service");
            try
            {
                var pillarApiSettings =
                    (PillarApiSettings)
                    SerializationUtilities.DeserializeObject(
                        File.ReadAllText(ConfigurationManager.AppSettings["Path2Settingsfile"]),
                        typeof (PillarApiSettings));
                Log.Debug(pillarApiSettings.ToString());
                _piz =
                    new Pillar(
                        (PillarTypeEnum)
                        Enum.Parse(typeof (PillarTypeEnum), ConfigurationManager.AppSettings["PillarType"]));
                Task.Factory.StartNew(() => _piz.Initialize(pillarApiSettings));
            }
            catch (Exception ef)
            {
                Log.Error(ef);
            }
        }

        protected override void OnStop()
        {
            Log.Debug("Stoping service");
            _piz.KillPillar();
            Log.Debug("Service stopped");
        }
    }
}