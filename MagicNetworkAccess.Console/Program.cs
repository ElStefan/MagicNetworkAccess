using MagicNetworkAccess.Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicNetworkAccess.Console
{
    static class Program
    {
        static void Main()
        {
            SystemCore.Instance.Start();
            System.Console.ReadLine();
            SystemCore.Instance.Stop();
        }
    }
}
