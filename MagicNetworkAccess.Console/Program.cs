using MagicNetworkAccess.Library.Core;

namespace MagicNetworkAccess.Console
{
    internal static class Program
    {
        private static void Main()
        {
            SystemCore.Instance.Start();
            System.Console.ReadLine();
            SystemCore.Instance.Stop();
        }
    }
}