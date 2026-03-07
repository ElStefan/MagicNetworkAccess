using log4net;
using MagicNetworkAccess.Library.Core;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;

namespace MagicNetworkAccess.Library.Helper
{
    public static class WolHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WolHelper));
        private static readonly ConcurrentDictionary<IPAddress, DateTime> LastUnknownIpLogTimes = new ConcurrentDictionary<IPAddress, DateTime>();
        private static DateTime _lastArpRefreshAttemptUtc = DateTime.MinValue;

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
                var nowUtc = DateTime.UtcNow;
                var shouldLog = !LastUnknownIpLogTimes.TryGetValue(ip, out var lastLogUtc) || lastLogUtc < nowUtc.AddMinutes(-5);
                if (shouldLog)
                {
                    LastUnknownIpLogTimes.AddOrUpdate(ip, nowUtc, (_, __) => nowUtc);
                    Log.WarnFormat("Wake - Ip '{0}' unknown (throttled)", ip);
                }

                if (_lastArpRefreshAttemptUtc < nowUtc.AddMinutes(-1))
                {
                    _lastArpRefreshAttemptUtc = nowUtc;
                    ArpHelper.Refresh();
                }

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