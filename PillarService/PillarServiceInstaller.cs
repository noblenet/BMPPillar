using System.ComponentModel;
using System.Configuration.Install;

namespace PillarService
{
    [RunInstaller(true)]
    public partial class PillarServiceInstaller : Installer
    {
        public PillarServiceInstaller()
        {
            InitializeComponent();
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
        }
    }
}