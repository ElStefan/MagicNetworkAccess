using log4net;
using MagicNetworkAccess.Library.Jobs;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace MagicNetworkAccess.Library.Core
{
    public class SystemCore
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SystemCore));
        private PackageWorker packageWorker;
        private IScheduler scheduler;

        public readonly ConcurrentDictionary<IPAddress, string> ArpTable = new ConcurrentDictionary<IPAddress, string>();
        public readonly ConcurrentDictionary<IPAddress, DateTime> LastWakeTimes = new ConcurrentDictionary<IPAddress, DateTime>();

        #region Singleton

        private static readonly Lazy<SystemCore> _instance = new Lazy<SystemCore>(() => new SystemCore());

        private SystemCore()
        {
        }

        public static SystemCore Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        #endregion Singleton

        public bool Start()
        {
            scheduler = StdSchedulerFactory.GetDefaultScheduler();

            var arpRefreshJob = JobBuilder.Create<ArpRefreshJob>()
                .WithIdentity(nameof(ArpRefreshJob))
                .Build();
            var arpRefreshTrigger = TriggerBuilder.Create()
                .ForJob(arpRefreshJob)
                .StartNow()
                .WithIdentity($"trigger{nameof(ArpRefreshJob)}")
                .WithSimpleSchedule(o => o.WithIntervalInHours(1))
                .Build();

            scheduler.ScheduleJob(arpRefreshJob, arpRefreshTrigger);
            scheduler.Start();

            Log.Debug("MagicNetworkAccess - Start");
            packageWorker = new PackageWorker();
            if (!packageWorker.Start())
            {
                return false;
            }
            Log.Debug("MagicNetworkAccess - Started");
            return true;
        }

        public bool Stop()
        {
            Log.Debug("MagicNetworkAccess - Stop");
            scheduler.Shutdown();
            if (!packageWorker.Stop())
            {
                return false;
            }
            return true;
        }
    }
}