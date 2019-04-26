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
        public void FindCreditCardProvidersTest() {
            var ccProviderList = LookupService.FindCreditCardProviders();
            Assert.IsTrue(ccProviderList.Count() > 0, "Error: No items were returned when some were expected");
        }
    }
}
