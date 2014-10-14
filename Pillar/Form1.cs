using Pillar.Properties;
using pillarAPI;
using PillarAPI.Interfaces;
using PillarAPI.Utilities;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pillar
{
    public partial class Form1 : Form
    {
        private IPillar _piz;

        public Form1()
        {
            InitializeComponent();
            StartPillar();
        }

        private void StartPillarToolStripMenuItemClick(object sender, EventArgs e)
        {
            // l"Sker her noget i start...");
            StartPillar();
        }

        private void StartPillar()
        {
            //TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs eventArgs) =>
            //{
            //    eventArgs.SetObserved();
            //    ((AggregateException)eventArgs.Exception).Handle(ex =>
            //    {
            //        Console.WriteLine("Exception type: {0}", ex.GetType());
            //        return true;
            //    });
            //};

            try
            {
                var pillarApiSettings = (PillarApiSettings)SerializationUtilities.DeserializeObject(File.ReadAllText(Settings.Default.Path2Settingsfile), typeof(PillarApiSettings));
                _piz = new PillarAPI.Pillar(Settings.Default.PillarType);
                label1.Text = Resources.pillar_on;
                ovalShape1.FillColor = Color.Lime;
                
                Task.Factory.StartNew(() => _piz.Initialize(pillarApiSettings));

            }
            catch (Exception ef)
            {
                label1.Text = Resources.pillar_off;
                ovalShape1.FillColor = Color.Red;
                MessageBox.Show(ef.ToString());
            }
        }

        private void StopPillar(object sender, FormClosingEventArgs e)
        {
            // Makes sure that tasks get killed
            _piz.KillPillar();


            //Environment.Exit(0);
            //System.Windows.Forms.Application.Exit();
        }

        private void StopPillarToolStripMenuItemClick(object sender, EventArgs e)
        {
            _piz.KillPillar();
            label1.Text = Resources.pillar_off;
            ovalShape1.FillColor = Color.Red;
        }
    }
}