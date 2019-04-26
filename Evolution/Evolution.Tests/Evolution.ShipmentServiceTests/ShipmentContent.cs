using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.ShipmentService;

namespace Evolution.ShipmentServiceTests {
    public partial class ShipmentServiceTests {
        [TestMethod]
        public void FindShipmentContentByPONoModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testShipment = GetTestShipment(testCompany, testUser, 10);

            var newPoh = GetTestPurchaseOrderHeader(testCompany, testUser, RandomInt(10, 20));
            var newItem = ShipmentService.AddPurchaseOrder(testCompany, testUser, testShipment, newPoh);

            var checkContent = ShipmentService.FindShipmentContentByPONoModel(testCompany, newPoh.OrderNumber.Value, false);
            Assert.IsTrue(checkContent != null, "Error: A NULL value was returned when an object was expected");

            var checkPoh = PurchasingService.FindPurchaseOrderHeaderModel(checkContent.PurchaseOrderHeaderId.Value, testCompany, false);

            var excludes = new List<string>();
            excludes.Add("OrderNumberUrl");         // Because it isn't known at test prep
            AreEqual(newPoh, checkPoh, excludes);

            db.DeletePurchaseOrderHeader(newPoh.Id);

            // Check that it has gone
            checkContent = ShipmentService.FindShipmentContentByPONoModel(testCompany, newPoh.OrderNumber.Value, false);
            Assert.IsTrue(checkContent == null, "Error: A non-NULL value was returned when a NULL value was expected");

            checkPoh = PurchasingService.FindPurchaseOrderHeaderModel(newPoh.Id, testCompany, false);
            Assert.IsTrue(checkPoh == null, "Error: An object was returned when NULL was expected");
        }

        [TestMethod]
        public void FindShipmentContentListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testShipment = GetTestShipment(testCompany, testUser, 10);
            var model = ShipmentService.FindShipmentContentListModel(testCompany, testShipment.Id, 0);
            var dbData = db.FindShipmentContents(testCompany.Id, testShipment.Id);

            int expected = dbData.Count(),
                actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = ShipmentService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newPoh = GetTestPurchaseOrderHeader(testCompany, testUser, RandomInt(10, 20));
            var newItem = ShipmentService.AddPurchaseOrder(testCompany, testUser, testShipment, newPoh);

            model = ShipmentService.FindShipmentContentListModel(testCompany, testShipment.Id, 0);
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            ShipmentService.DeleteShipmentContent(newItem.Id, true);

            model = ShipmentService.FindShipmentContentListModel(testCompany, testShipment.Id, 0);
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindShipmentContentModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testShipment = GetTestShipment(testCompany, testUser, 5);

            var newItem = GetTestPurchaseOrderHeader(testCompany, testUser, RandomInt(10, 20));
            var model = ShipmentService.AddPurchaseOrder(testCompany, testUser, testShipment, newItem);

            var test = ShipmentService.FindShipmentContentModel(model.Id);
            AreEqual(model, test);
        }

        [TestMethod]
        public void AddPurchaseOrdersTest() {
            // Tested by all tests in this module which call GetTestShipment
        }

        [TestMethod]
        public void AddPurchaseOrderTest() {
            // Tested in:
            //      FindShipmentContentByPONoModelTest
            //      FindShipmentContentListModelTest
            //      FindShipmentContentModelTest
        }

        [TestMethod]
        public void InsertOrUpdateShipmentContentTest() {
            // Tested in all tests in this module
        }

        [TestMethod]
        public void DeleteShipmentContentTest() {
            // Tested in:
            //      FindShipmentContentListModelTest
            //      FindShipmentContentByPONoModelTest
        }
    }
}
