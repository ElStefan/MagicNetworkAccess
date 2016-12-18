using MagicNetworkAccess.Library.Core;
using System.ServiceProcess;

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