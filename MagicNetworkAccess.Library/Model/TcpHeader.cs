using System;
using System.IO;
using System.Net;

namespace MagicNetworkAccess.Library.Model
{
    public class TcpHeader
    {
        private readonly ushort usSourcePort;
        private readonly ushort usDestinationPort;
        private readonly uint uiSequenceNumber = 555;
        private readonly uint uiAcknowledgementNumber = 555;
        private readonly ushort usDataOffsetAndFlags = 555;
        private readonly ushort usWindow = 555;
        private readonly short sChecksum = 555;
        private readonly ushort usUrgentPointer;
        private readonly byte byHeaderLength;
        private readonly ushort usMessageLength;
        private readonly byte[] byTCPData = new byte[4096];

        public TcpHeader(byte[] byBuffer, int nReceived)
        {
            var memoryStream = new MemoryStream(byBuffer, 0, nReceived);
            var binaryReader = new BinaryReader(memoryStream);
            usSourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usDestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            uiSequenceNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
            uiAcknowledgementNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
            usDataOffsetAndFlags = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usWindow = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            sChecksum = (short)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usUrgentPointer = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            byHeaderLength = (byte)(usDataOffsetAndFlags >> 12);
            byHeaderLength *= 4;
            usMessageLength = (ushort)(nReceived - byHeaderLength);
            Array.Copy(byBuffer, byHeaderLength, byTCPData, 0, nReceived - byHeaderLength);
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

        public string SequenceNumber
        {
            get
            {
                return uiSequenceNumber.ToString();
            }
        }

        public string AcknowledgementNumber
        {
            get
            {
                if ((usDataOffsetAndFlags & 0x10) != 0)
                {
                    return uiAcknowledgementNumber.ToString();
                }
                return "";
            }
        }

        public string HeaderLength
        {
            get
            {
                return byHeaderLength.ToString();
            }
        }

        public string WindowSize
        {
            get
            {
                return usWindow.ToString();
            }
        }

        public string UrgentPointer
        {
            get
            {
                if ((usDataOffsetAndFlags & 0x20) != 0)
                {
                    return usUrgentPointer.ToString();
                }
                return "";
            }
        }

        public string Flags
        {
            get
            {
                var nFlags = usDataOffsetAndFlags & 0x3F;

                var strFlags = $"0x{nFlags:x2} (";

                if ((nFlags & 0x01) != 0)
                {
                    strFlags += "FIN, ";
                }
                if ((nFlags & 0x02) != 0)
                {
                    strFlags += "SYN, ";
                }
                if ((nFlags & 0x04) != 0)
                {
                    strFlags += "RST, ";
                }
                if ((nFlags & 0x08) != 0)
                {
                    strFlags += "PSH, ";
                }
                if ((nFlags & 0x10) != 0)
                {
                    strFlags += "ACK, ";
                }
                if ((nFlags & 0x20) != 0)
                {
                    strFlags += "URG";
                }
                strFlags += ")";

                if (strFlags.Contains("()"))
                {
                    strFlags = strFlags.Remove(strFlags.Length - 3);
                }
                else if (strFlags.Contains(", )"))
                {
                    strFlags = strFlags.Remove(strFlags.Length - 3, 2);
                }

                return strFlags;
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
                return byTCPData;
            }
        }

        public ushort MessageLength
        {
            get
            {
                return usMessageLength;
            }
        }
    }
}