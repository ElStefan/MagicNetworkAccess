using log4net;
using MagicNetworkAccess.Library.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace MagicNetworkAccess.Library.Helper
{
    public static class ArpHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ArpHelper));
        private static DateTime LastRefresh;

        public static Dictionary<IPAddress, string> GetArpTable()
        {
            try
            {
                var table = new Dictionary<IPAddress, string>();

                var arpTableString = GetArpProcessResult();
                if (string.IsNullOrEmpty(arpTableString))
                {
                    Log.Error("GetArpTable - Arp -a did not return anything");
                    return null;
                }

                foreach (var arp in arpTableString.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var pieces = arp.Split(new[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    if (pieces.Length == 3)
                    {
                        IPAddress ip;
                        if (!IPAddress.TryParse(pieces[0], out ip))
                        {
                            continue;
                        }
                        if (table.ContainsKey(ip))
                        {
                            continue;
                        }
                        table.Add(ip, pieces[1]);
                    }
                }
                return table;
            }
            catch (Exception exception)
            {
                Log.Error(nameof(GetArpTable), exception);
                return null;
            }
        }

        private static string GetArpProcessResult()
        {
            try
            {
                var startInfo = new ProcessStartInfo("arp", "-a")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using (var p = Process.Start(startInfo))
                {
                    return p.StandardOutput.ReadToEnd();
                }
            }
            catch (Exception exception)
            {
                Log.Error(nameof(GetArpProcessResult), exception);
                return null;
            }
        }

        public static void Refresh()
        {
            if (LastRefresh > DateTime.Now.AddMinutes(-10))
            {
                return;
            }
            LastRefresh = DateTime.Now;
            var arpTable = GetArpTable();
            if (arpTable == null || !arpTable.Any())
            {
                Log.Warn("Refresh - No arp entries found");
                return;
            }
            foreach (var item in arpTable)
            {
                SystemCore.Instance.ArpTable.AddOrUpdate(item.Key, item.Value, (oldKey, oldValue) => item.Value);
            }
        }
    }
}