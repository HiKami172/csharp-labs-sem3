using System.ServiceProcess;
using System.ComponentModel;
using System.Configuration.Install;

namespace FTPFileWatcher
{
    [RunInstaller(true)]
    public partial class FTPInstaller : Installer
    {
        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;
        public FTPInstaller()
        {
            InitializeComponent();
            serviceInstaller = new ServiceInstaller();
            processInstaller = new ServiceProcessInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Manual;
            serviceInstaller.ServiceName = "FTPFileWatcher";
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
