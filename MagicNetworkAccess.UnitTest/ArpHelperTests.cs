using MagicNetworkAccess.Library.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MagicNetworkAccess.UnitTest
{
    [TestClass]
    public class ArpHelperTests
    {
        [TestMethod]
        public void GetArpTable()
        {
            Assert.IsNotNull(ArpHelper.GetArpTable());
        }
    }
}