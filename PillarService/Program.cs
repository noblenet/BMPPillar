using System.ServiceProcess;

namespace PillarService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            var pillarService = new PillarService();
            pillarService.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#else

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new PillarService() 
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
