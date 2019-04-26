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
        public void FindSalesOrderHeaderSubStatusListItemModelTest() {
            var statusList = LookupService.FindSalesOrderHeaderSubStatusListItemModel();
            Assert.IsTrue(statusList.Count() > 0, "Error: No items were returned when some were expected");
        }
    }
}
