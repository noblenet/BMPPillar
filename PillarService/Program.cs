using System.Threading;

namespace PillarService
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main()
        {
#if DEBUG
            var pillarService = new PillarService();
            pillarService.OnDebug();
            Thread.Sleep(Timeout.Infinite);
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