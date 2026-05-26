using log4net;
using MagicNetworkAccess.Library.Helper;
using MagicNetworkAccess.Library.Model;
using System;
using System.Buffers;
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
        private byte[] byteData = new byte[65535];

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
                if (nReceived > 0)
                {
                    var copy = ArrayPool<byte>.Shared.Rent(nReceived);
                    Buffer.BlockCopy(byteData, 0, copy, 0, nReceived);
                    packageQueue.Enqueue(new Package { Data = copy, DataLength = nReceived });
                    autoResetEvent.Set();
                }
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
            if (item?.Data == null)
            {
                return null;
            }

            var data = item.Data;
            var nReceived = item.DataLength;

            try
            {
                // Fast-path parser to avoid allocating full header model objects for every packet.
                // IPv4 header minimum is 20 bytes; TCP header minimum is 20 bytes.
                if (nReceived < 40)
                {
                    return null;
                }

                var version = (data[0] >> 4) & 0x0F;
                if (version != 4)
                {
                    return null;
                }

                var headerLength = (data[0] & 0x0F) * 4;
                if (headerLength < 20 || nReceived < headerLength + 20)
                {
                    return null;
                }

                // Protocol 6 = TCP
                if (data[9] != 6)
                {
                    return null;
                }

                var destinationPort = (ushort)((data[headerLength + 2] << 8) | data[headerLength + 3]);
                if (destinationPort != 445)
                {
                    return null;
                }

                var destinationAddressBytes = new byte[4];
                Buffer.BlockCopy(data, 16, destinationAddressBytes, 0, 4);
                return new ParseResult
                {
                    IpAddress = new IPAddress(destinationAddressBytes),
                    Port = destinationPort.ToString()
                };
            }
            catch (Exception)
            {
                // ignore - the wanted packages will not cause crashes
                return null;
            }
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
                    try
                    {
                        var result = ParsePackage(item);
                        if (result != null)
                        {
                            WolHelper.Wake(result.IpAddress);
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(item.Data);
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