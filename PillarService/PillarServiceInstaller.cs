using System.ComponentModel;

namespace PillarService
{
    [RunInstaller(true)]
    public partial class PillarServiceInstaller : System.Configuration.Install.Installer
    {
        public PillarServiceInstaller()
        {
            InitializeComponent();
        }

        private void serviceInstaller1_AfterInstall(object sender, System.Configuration.Install.InstallEventArgs e)
        {

        }
    }
}
