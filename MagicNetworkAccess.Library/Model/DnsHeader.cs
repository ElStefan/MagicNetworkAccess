using System.IO;
using System.Net;

namespace MagicNetworkAccess.Library.Model
{
    public class DnsHeader
    {
        private readonly ushort usIdentification;
        private readonly ushort usFlags; private readonly ushort usTotalQuestions;
        private readonly ushort usTotalAnswerRRs;
        private readonly ushort usTotalAuthorityRRs;
        private readonly ushort usTotalAdditionalRRs;

        public DnsHeader(byte[] byBuffer, int nReceived)
        {
            var memoryStream = new MemoryStream(byBuffer, 0, nReceived);
            var binaryReader = new BinaryReader(memoryStream);
            usIdentification = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usFlags = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usTotalQuestions = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usTotalAnswerRRs = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usTotalAuthorityRRs = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
            usTotalAdditionalRRs = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
        }

        public string Identification
        {
            get
            {
                return $"0x{usIdentification:x2}";
            }
        }

        public string Flags
        {
            get
            {
                return $"0x{usFlags:x2}";
            }
        }

        public string TotalQuestions
        {
            get
            {
                return usTotalQuestions.ToString();
            }
        }

        public string TotalAnswerRRs
        {
            get
            {
                return usTotalAnswerRRs.ToString();
            }
        }

        public string TotalAuthorityRRs
        {
            get
            {
                return usTotalAuthorityRRs.ToString();
            }
        }

        public string TotalAdditionalRRs
        {
            get
            {
                return usTotalAdditionalRRs.ToString();
            }
        }
    }
}