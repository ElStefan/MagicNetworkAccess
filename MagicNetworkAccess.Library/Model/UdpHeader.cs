using System;
using System.IO;
using System.Net;

namespace MagicNetworkAccess.Library.Model
{
    public class UdpHeader
    {
        private readonly ushort usSourcePort;
        private readonly ushort usDestinationPort; private readonly ushort usLength; private readonly short sChecksum;
        private readonly byte[] byUDPData = new byte[4096];

        public UdpHeader(byte[] byBuffer, int nReceived)
        {
            var memoryStream = new MemoryStream(byBuffer, 0, nReceived);
            var binaryReader = new BinaryReader(memoryStream);

            usSourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            usDestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            usLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

            Array.Copy(byBuffer,
           8, byUDPData,
           0,
           nReceived - 8);
        }

        public string SourcePort
        {
            get
            {
                return usSourcePort.ToString();
            }
        }

        public string DestinationPort
        {
            get
            {
                return usDestinationPort.ToString();
            }
        }

        public string Length
        {
            get
            {
                return usLength.ToString();
            }
        }

        public string Checksum
        {
            get
            {
                return $"0x{sChecksum:x2}";
            }
        }

        public byte[] Data
        {
            get
            {
                return byUDPData;
            }
        }
    }
}