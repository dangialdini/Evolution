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
        public void CreateShipmentTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var shipment = ShipmentService.CreateShipment(testCompany, testUser);

            // Retrieve it
            var checkShipment = db.FindShipment(shipment.Id);
            Assert.IsTrue(checkShipment != null, "Error: A NULL value was returned when an object was expected");

            var checkModel = ShipmentService.MapToModel(checkShipment);
            AreEqual(shipment, checkModel);
        }

        [TestMethod]
        public void FindShipmentsListItemModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompanyAU();
            var model = ShipmentService.FindShipmentsListItemModel(testCompany);
            var dbData = db.FindShipments(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");

                var test = ShipmentService.MapToModel(dbItem);
                AreEqual(item, test);
            }

            // Add another item a make sure it is found
            testCompany = GetTestCompany(testUser);
            var newItem = new ShipmentModel {
                CompanyId = testCompany.Id
            };
            var error = ShipmentService.InsertOrUpdateShipment(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = ShipmentService.FindShipmentsListItemModel(testCompany);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            ShipmentService.DeleteShipment(newItem.Id);

            model = ShipmentService.FindShipmentsListItemModel(testCompany);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindShipmentsListModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Perform a global test which should bring back the first page of all shipments
            var result = ShipmentService.FindShipmentsListModel(testCompany.Id, 0, 1, PageSize, "");
            Assert.IsTrue(result.Items.Count == 0, $"Error: {result.Items.Count} records were found when 0 were expected");

            // Now create a shipment and try to search for it
            var testShipment = GetTestShipment(testCompany, testUser, 1);

            result = ShipmentService.FindShipmentsListModel(testCompany.Id, 0, 1, PageSize, "");
            Assert.IsTrue(result.Items.Count == 1, $"Error: {result.Items.Count} records were found when 1 was expected");
            Assert.IsTrue(result.Items[0].ShipmentId == testShipment.Id, $"Error: Shipment {result.Items[0].ShipmentId} was returned when {testShipment.Id} was expected");

            // Delete the record
            ShipmentService.DeleteShipment(testShipment.Id);

            // Try to find it again
            result = ShipmentService.FindShipmentsListModel(testCompany.Id, 0, 1, PageSize, "");
            Assert.IsTrue(result.Items.Count == 0, $"Error: {result.Items.Count} records were found when 0 was expected");
        }

        [TestMethod]
        public void FindShipmentModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Now create a shipment and try to retrieve it
            var testShipment = GetTestShipment(testCompany, testUser);

            var result = ShipmentService.FindShipmentModel(testShipment.Id, testCompany, false);
            Assert.IsTrue(result != null, $"Error: 0 records were returned when 1 was expected");
            Assert.IsTrue(result.Id == testShipment.Id, $"Error: Shipment {result.Id} was returned when {testShipment.Id} was expected");
            AreEqual(testShipment, result);
        }

        [TestMethod]
        public void InsertOrUpdateShipmentTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a shipment
            var testShipment = GetTestShipment(testCompany, testUser);

            // Retrieve the shipment and compare
            var test = db.FindShipment(testShipment.Id);

            ShipmentModel testModel = ShipmentService.MapToModel(test);

            AreEqual(testShipment, testModel);
        }

        [TestMethod]
        public void DeleteShipmentTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a shipment
            var testShipment = GetTestShipment(testCompany, testUser);

            // Delete the shipment
            // This test tests all the referencial integrity
            ShipmentService.DeleteShipment(testShipment.Id);

            // Try to find the shipment
            var test = db.FindShipment(testShipment.Id);
            Assert.IsTrue(test == null, "Error: The shipment record was found when it should have been deleted");
        }

        [TestMethod]
        public void LockShipmentTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a shipment
            var model = GetTestShipment(testCompany, testUser);

            // Get the current Lock
            string lockGuid = ShipmentService.LockShipment(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = ShipmentService.InsertOrUpdateShipment(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = ShipmentService.InsertOrUpdateShipment(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = ShipmentService.LockShipment(model);
            error = ShipmentService.InsertOrUpdateShipment(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }
    }
}
