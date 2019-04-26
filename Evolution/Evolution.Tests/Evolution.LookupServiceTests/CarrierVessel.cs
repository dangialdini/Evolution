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
        public void FindCarrierVesselListItemModelTest() {
            var user = GetTestUser();
            var model = LookupService.FindCarrierVesselListItemModel();
            var dbData = db.FindCarrierVessels();

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.CarrierVesselName, $"Error: Model Text is {item.Text} when {dbItem.CarrierVesselName} was expected");
            }

            // TBD: Add code to test adding and deleting carrier vessels
        }
    }
}