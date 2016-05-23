﻿using log4net;
using MagicNetworkAccess.Library.Core;
using MagicNetworkAccess.Library.Helper;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicNetworkAccess.Library.Jobs
{
    [DisallowConcurrentExecution]
    public class ArpRefreshJob : IJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ArpRefreshJob));

        public void Execute(IJobExecutionContext context)
        {
            Log.Debug("Execute - Start");
            try
            {
                ArpHelper.Refresh();
            }
            catch (Exception exception)
            {
                Log.Error(nameof(Execute), exception);
                return;
            }
            Log.Debug("Execute - Stop");
        }
    }
}