using MagicNetworkAccess.Library.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MagicNetworkAccess.UnitTest
{
    [TestClass]
    public class ArpHelperTests
    {
        [TestMethod]
        public void GetArpTable()
        {
            var table = ArpHelper.GetArpTable();
            Assert.IsNotNull(table, "Failed to get arp table of local host");
        }
    }
}