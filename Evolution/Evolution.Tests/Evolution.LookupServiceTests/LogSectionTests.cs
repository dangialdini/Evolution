using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.LookupService;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.LookupServiceTests {
    public partial class LookupServiceTests {
        [TestMethod]
        public void FindLogSectionListItemModelTest() {
            var logSectionList = LookupService.FindLogSectionListItemModel(true);

            int actual = logSectionList.Count(),
                expected = 4;
            Assert.IsTrue(actual >= expected, $"Error: {actual} items were returned when {expected} or more were expected");
            Assert.IsTrue(logSectionList[0].Text == "All", $"Error: The first item returned was '{logSectionList[0].Text}' when 'All' was expected");
        }
    }
}
