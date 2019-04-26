using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.LookupService;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Models.Models;

namespace Evolution.LookupServiceTests {
    public partial class LookupServiceTests {
        [TestMethod]
        public void FindFTPProtocolListItemModelTest() {
            var ftpProtocols = LookupService.FindFTPProtocolListItemModel();
            Assert.IsTrue(ftpProtocols != null, "Error: NULL was returned when a non-NULL value was expected");
            Assert.IsTrue(ftpProtocols.Count() > 0, "Error: The returned list contained 0 items when a few were expected");
            Assert.IsTrue(ftpProtocols.Count() <= 5, $"Error: The returned list contained {ftpProtocols.Count()} items when 5 or less were expected");
        }
    }
}
