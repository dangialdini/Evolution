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
        public void FindSaleNextActionListItemModelTest() {
            var actions = LookupService.FindSaleNextActionListItemModel();
            Assert.IsTrue(actions.Count() > 0, "Error: An empty list was returned when items were expected");
        }

        [TestMethod]
        public void FindSaleNextActionIdTest() {
            var nextId = LookupService.FindSaleNextActionId(Enumerations.SaleNextAction.None);
            Assert.IsTrue(nextId != null, "Error: A NULL value was returned where a numeric value was expected");
        }
    }
}
