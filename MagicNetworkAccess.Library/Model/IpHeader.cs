using System;
using System.IO;
using System.Net;

namespace MagicNetworkAccess.Library.Model
{
    public class IpHeader
    {
        private readonly byte byVersionAndHeaderLength;
        private readonly byte byDifferentiatedServices;
        private readonly ushort usTotalLength;
        private readonly ushort usIdentification;
        private readonly ushort usFlagsAndOffset;
        private readonly byte byTTL;
        private readonly byte byProtocol;
        private readonly short sChecksum;
        private readonly uint uiSourceIPAddress;
        private readonly uint uiDestinationIPAddress;
        private readonly byte byHeaderLength;
        private readonly byte[] byIPData = new byte[4096];

        public IpHeader(byte[] byBuffer, int nReceived)
        {
            var memoryStream = new MemoryStream(byBuffer, 0, nReceived);
            var binaryReader = new BinaryReader(memoryStream);

            byVersionAndHeaderLength = binaryReader.ReadByte();
            byDifferentiatedServices = binaryReader.ReadByte();

            usTotalLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usIdentification = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usFlagsAndOffset = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            byTTL = binaryReader.ReadByte();
            byProtocol = binaryReader.ReadByte();
            sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            uiSourceIPAddress = (uint)(binaryReader.ReadInt32());
            uiDestinationIPAddress = (uint)(binaryReader.ReadInt32());

            byHeaderLength = byVersionAndHeaderLength;
            byHeaderLength <<= 4;
            byHeaderLength >>= 4;
            byHeaderLength *= 4;
            Array.Copy(byBuffer,
                       byHeaderLength,
                       byIPData, 0,
                       usTotalLength - byHeaderLength);
        }

        public string Version
        {
            get
            {
                if ((byVersionAndHeaderLength >> 4) == 4)
                {
                    return "IP v4";
                }
                if ((byVersionAndHeaderLength >> 4) == 6)
                {
                    return "IP v6";
                }
                return "Unknown";
            }
        }

        public string HeaderLength
        {
            get
            {
                return byHeaderLength.ToString();
            }
        }

        public ushort MessageLength
        {
            get
            {
                return (ushort)(usTotalLength - byHeaderLength);
            }
        }

        public string DifferentiatedServices
        {
            get
            {
                return $"0x{byDifferentiatedServices:x2} ({byDifferentiatedServices})";
            }
        }

        public string Flags
        {
            get
            {
                var nFlags = usFlagsAndOffset >> 13;
                if (nFlags == 2)
                {
                    return "Don't fragment";
                }
                if (nFlags == 1)
                {
                    return "More fragments to come";
                }
                return nFlags.ToString();
            }
        }

        public string FragmentationOffset
        {
            get
            {
                var nOffset = usFlagsAndOffset << 3;
                nOffset >>= 3;

                return nOffset.ToString();
            }
        }

        public string Ttl
        {
            get
            {
                return byTTL.ToString();
            }
        }

        public Protocol ProtocolType
        {
            get
            {
                if (byProtocol == 6)
                {
                    return Protocol.TCP;
                }
                if (byProtocol == 17)
                {
                    return Protocol.UDP;
                }
                return Protocol.Unknown;
            }
        }

        public string Checksum
        {
            get
            {
                return $"0x{sChecksum:x2}";
            }
        }

        public IPAddress SourceAddress
        {
            get
            {
                return new IPAddress(uiSourceIPAddress);
            }
        }

        public IPAddress DestinationAddress
        {
            get
            {
                return new IPAddress(uiDestinationIPAddress);
            }
        }

        public string TotalLength
        {
            get
            {
                return usTotalLength.ToString();
            }
        }

        public string Identification
        {
            get
            {
                return usIdentification.ToString();
            }
        }

        public byte[] Data
        {
            get
            {
                return byIPData;
            }
        }
    }
}