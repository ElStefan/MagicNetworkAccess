using log4net;
using MagicNetworkAccess.Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MagicNetworkAccess.Library.Helper
{
    public static class WolHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WolHelper));

        public static void Wake(IPAddress ip)
        {
            if (ip == null)
            {
                Log.Error("Wake - Invalid ip");
                return;
            }
            string macAddress;
            if (!SystemCore.Instance.ArpTable.TryGetValue(ip, out macAddress))
            {
                Log.ErrorFormat("Wake - Ip '{0}' unknown", ip);
                ArpHelper.Refresh();
                return;
            }

            DateTime lastWake;
            if (SystemCore.Instance.LastWakeTimes.TryGetValue(ip, out lastWake) && lastWake >= DateTime.Now.AddMinutes(-20))
            {
                return;
            }

            Log.DebugFormat("Wake - Waking up {0} (MAC: {1})", ip, macAddress);
            PhysicalAddress.Parse(macAddress.ToUpperInvariant()).SendWol();

            SystemCore.Instance.LastWakeTimes.AddOrUpdate(ip, DateTime.Now, (oldKey, oldValue) => DateTime.Now);
        }
    }
}