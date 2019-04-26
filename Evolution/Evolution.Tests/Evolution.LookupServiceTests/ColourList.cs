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
        public void FindColourListItemModelTest() {
            var colourList = LookupService.FindColourListItemModel();
            int actual = colourList.Count(),
                expected = 148;
            Assert.IsTrue(actual == expected, $"Error: {actual} items were expected when {expected} were expected. Has the list been changed in the database ?");
        }
    }
}
