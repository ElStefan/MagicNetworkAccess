using log4net;
using MagicNetworkAccess.Library.Helper;
using System.Collections.Concurrent;
using System.Net;

namespace MagicNetworkAccess.Library.Core
{
    public class SystemCore
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SystemCore));
        private PackageWorker packageWorker;
        private CancellationTokenSource _refreshTokenSource;
        private Task _refreshTask;

        public readonly ConcurrentDictionary<IPAddress, string> ArpTable = new ConcurrentDictionary<IPAddress, string>();
        public readonly ConcurrentDictionary<IPAddress, DateTime> LastWakeTimes = new ConcurrentDictionary<IPAddress, DateTime>();

        #region Singleton

        private static readonly Lazy<SystemCore> _instance = new Lazy<SystemCore>(() => new SystemCore());

        private SystemCore()
        {
        }

        public static SystemCore Instance => _instance.Value;

        #endregion Singleton

        public bool Start()
        {
            Log.Debug("MagicNetworkAccess - Start");

            packageWorker = new PackageWorker();
            if (!packageWorker.Start())
            {
                return false;
            }

            _refreshTokenSource = new CancellationTokenSource();
            _refreshTask = Task.Run(() => RefreshArpLoopAsync(_refreshTokenSource.Token));

            Log.Debug("MagicNetworkAccess - Started");
            return true;
        }

        public bool Stop()
        {
            Log.Debug("MagicNetworkAccess - Stop");

            try
            {
                _refreshTokenSource?.Cancel();
                _refreshTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch (Exception exception)
            {
                Log.Error(nameof(Stop), exception);
            }

            if (!packageWorker.Stop())
            {
                return false;
            }
            return true;
        }

        private static async Task RefreshArpLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    ArpHelper.Refresh();
                }
                catch (Exception exception)
                {
                    Log.Error(nameof(RefreshArpLoopAsync), exception);
                }

                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}
