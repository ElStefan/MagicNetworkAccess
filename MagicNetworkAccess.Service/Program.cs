using System.ServiceProcess;

namespace MagicNetworkAccess.Service
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            using (var service1 = new WindowsService())
            {
                ServiceBase.Run(service1);
            }
        }
    }
}