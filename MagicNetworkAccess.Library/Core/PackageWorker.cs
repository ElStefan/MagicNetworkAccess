using log4net;
using MagicNetworkAccess.Library.Helper;
using MagicNetworkAccess.Library.Model;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MagicNetworkAccess.Library.Core
{
    public class PackageWorker : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PackageWorker));
        private bool stopProcess;
        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private Thread worker;
        private readonly ConcurrentQueue<Package> packageQueue = new ConcurrentQueue<Package>();
        private Socket mainSocket;
        private byte[] byteData = new byte[1048576];

        public bool Start()
        {
            try
            {
                if (!PrepareSocket())
                {
                    return false;
                }

                worker = new Thread(Process);
                worker.Start();
            }
            catch (Exception exception)
            {
                Log.Error(nameof(Start), exception);
                return false;
            }
            return true;
        }

        private bool PrepareSocket()
        {
            var HostEntry = Dns.GetHostEntry((Dns.GetHostName()));
            if (HostEntry.AddressList.Length <= 0)
            {
                Log.Error("PrepareSocket - No ipv4 address found");
                return false;
            }
            var ip = HostEntry.AddressList.FirstOrDefault(o => o.AddressFamily == AddressFamily.InterNetwork);

            mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            mainSocket.Bind(new IPEndPoint(ip, 0));
            mainSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            byte[] byTrue = { 1, 0, 0, 0 };
            byte[] byOut = { 1, 0, 0, 0 };
            mainSocket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
            mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, OnReceive, null);

            return true;
        }

        public bool Stop()
        {
            try
            {
                stopProcess = true;
                mainSocket.Close();
                autoResetEvent.Set();

                if (!worker.Join(1000))
                {
                    worker.Abort();
                }
                Dispose();
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(nameof(Stop), exception);
                return false;
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                var nReceived = mainSocket.EndReceive(ar);
                packageQueue.Enqueue(new Package { Data = byteData.ToArray(), DataLength = nReceived });
                byteData = new byte[4096];
            }
            catch (SocketException)
            {
                // ignore
            }
            catch (Exception exception)
            {
                Log.Error(nameof(OnReceive), exception);
            }
            if (!stopProcess)
            {
                mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, OnReceive, null);
            }
        }

        private static ParseResult ParsePackage(Package item)
        {
            var data = item.Data;
            var nReceived = item.DataLength;
            try
            {
                var ipHeader = new IpHeader(data, nReceived);
                if (ipHeader.ProtocolType.HasFlag(Protocol.TCP))
                {
                    var tcpHeader = new TcpHeader(ipHeader.Data, ipHeader.MessageLength);
                    return new ParseResult { IpAddress = ipHeader.DestinationAddress, Port = tcpHeader.DestinationPort };
                }
            }
            catch (Exception)
            {
                // ignore - the wanted packages will not cause crashes
            }
            return null;
        }

        private void Process()
        {
            while (!stopProcess)
            {
                try
                {
                    if (packageQueue.IsEmpty)
                    {
                        autoResetEvent.WaitOne(100);
                        continue;
                    }
                    Package item;
                    if (!packageQueue.TryDequeue(out item))
                    {
                        autoResetEvent.WaitOne(100);
                        continue;
                    }
                    if (item == null)
                    {
                        autoResetEvent.WaitOne(100);
                        continue;
                    }
                    var result = ParsePackage(item);
                    if (result == null)
                    {
                        continue;
                    }
                    if (result.Port.Equals("445", StringComparison.Ordinal)) // Smb
                    {
                        WolHelper.Wake(result.IpAddress);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(nameof(Process), exception);
                    continue;
                }
            }
        }

        public void Dispose()
        {
            autoResetEvent.Dispose();
            mainSocket.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}