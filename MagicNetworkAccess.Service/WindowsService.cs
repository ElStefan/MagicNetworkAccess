using MagicNetworkAccess.Library.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MagicNetworkAccess.Service
{
    public partial class WindowsService : ServiceBase
    {
        public WindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!SystemCore.Instance.Start())
            {
                Stop();
            }
        }

        protected override void OnStop()
        {
            SystemCore.Instance.Stop();
        }
    }
}