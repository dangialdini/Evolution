using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.PurchasingService;

namespace Evolution.PurchasingServiceTests {
    public partial class PurchasingServiceTests {
        [TestMethod]
        public void FindPurchaseOrderDetailTempsListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testPurchase = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            var pohtModel = PurchasingService.CopyPurchaseOrderToTemp(testCompany, testPurchase, testUser);

            var model = PurchasingService.FindPurchaseOrderDetailTempsListModel(testCompany.Id, pohtModel.Id, 0, 1, PageSize, "");
            var dbData = db.FindPurchaseOrderDetailTemps(testCompany.Id, pohtModel.Id);

            int expected = dbData.Count(),
                actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = PurchasingService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createPurchaseOrderDetailTemp(model);
            var error = PurchasingService.InsertOrUpdatePurchaseOrderDetailTemp(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
            
            model = PurchasingService.FindPurchaseOrderDetailTempsListModel(testCompany.Id, pohtModel.Id, 0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            PurchasingService.DeletePurchaseOrderDetailTemp(newItem.Id);

            model = PurchasingService.FindPurchaseOrderDetailTempsListModel(testCompany.Id, pohtModel.Id, 0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindPurchaseOrderDetailTempModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testPurchase = GetTestPurchaseOrderHeader(testCompany, testUser, 1);

            var pohtModel = PurchasingService.CopyPurchaseOrderToTemp(testCompany, testPurchase, testUser);
            var model = PurchasingService.FindPurchaseOrderDetailTempsListModel(testCompany.Id, pohtModel.Id, 0, 1, PageSize, "");

            var newItem = createPurchaseOrderDetailTemp(model);
            var error = PurchasingService.InsertOrUpdatePurchaseOrderDetailTemp(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = PurchasingService.FindPurchaseOrderDetailTempModel(newItem.Id, null, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdatePurchaseOrderDetailTempTest() {
            // Tested in DeletePurchaseOrderDetailTempTest
        }

        [TestMethod]
        public void DeletePurchaseOrderDetailTempTest() {
            // Get a test user
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testPurchase = GetTestPurchaseOrderHeader(testCompany, testUser, 1);

            var pohtModel = PurchasingService.CopyPurchaseOrderToTemp(testCompany, testPurchase, testUser);
            var model = PurchasingService.FindPurchaseOrderDetailTempsListModel(testCompany.Id, pohtModel.Id, 0, 1, PageSize, "");

            // Create a record
            var newItem = createPurchaseOrderDetailTemp(model);

            var error = PurchasingService.InsertOrUpdatePurchaseOrderDetailTemp(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindPurchaseOrderDetailTemp(newItem.Id);
            var test = PurchasingService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            PurchasingService.DeletePurchaseOrderDetailTemp(newItem.Id);

            // And check that is was deleted
            result = db.FindPurchaseOrderDetailTemp(newItem.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockPurchaseOrderDetailTempTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testPurchase = GetTestPurchaseOrderHeader(testCompany, testUser, 1);

            var pohtModel = PurchasingService.CopyPurchaseOrderToTemp(testCompany, testPurchase, testUser);
            var model = PurchasingService.FindPurchaseOrderDetailTempsListModel(testCompany.Id, pohtModel.Id, 0, 1, PageSize, "");

            // Create a record
            var newItem = createPurchaseOrderDetailTemp(model);

            var error = PurchasingService.InsertOrUpdatePurchaseOrderDetailTemp(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = PurchasingService.LockPurchaseOrderDetailTemp(newItem);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = PurchasingService.InsertOrUpdatePurchaseOrderDetailTemp(newItem, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = PurchasingService.InsertOrUpdatePurchaseOrderDetailTemp(newItem, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = PurchasingService.LockPurchaseOrderDetailTemp(newItem);
            error = PurchasingService.InsertOrUpdatePurchaseOrderDetailTemp(newItem, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void CreateOrderSummaryTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testPurchase = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            var pohtModel = PurchasingService.CopyPurchaseOrderToTemp(testCompany, testPurchase, testUser);

            var pos = PurchasingService.CreateOrderSummary(pohtModel, testCompany.DateFormat);

            // 16/1/2018    TBD: Lines commented out below are because CreateOrderSummary does not 
            //              populate them yet as allocations have not been implemented
            Assert.IsTrue(pos.OrderNumber != 0, $"Error: OrderNumber '{pos.OrderNumber}' was returned when a non-zero value was expected");
            Assert.IsTrue(pos.TotalCbms != 0, $"Error: TotalCbms '{pos.TotalCbms}' was returned when a non-zero value was expected");
            //Assert.IsTrue(pos.AllocValueEx != 0, $"Error: AllocValueEx '{pos.AllocValueEx}' was returned when a non-zero value was expected");
            Assert.IsTrue(pos.OrderValueEx != 0, $"Error: OrderValueEx '{pos.OrderValueEx}' was returned when a non-zero value was expected");
            //Assert.IsTrue(pos.AllocatedPercent != 0, $"Error: AllocatedPercent '{pos.AllocatedPercent}' was returned when a non-zero value was expected");
            
            // Can't check GST because it is permissible to be zero
            //Assert.IsTrue(pos.Tax != 0, $"Error: GST '{pos.Tax}' was returned when a non-zero value was expected");
            Assert.IsTrue(pos.Total != 0, $"Error: Total '{pos.Total}' was returned when a non-zero value was expected");

            Assert.IsTrue(!string.IsNullOrEmpty(pos.POStatusText), $"Error: POStatusText '{pos.POStatusText}' was returned when a non-empty value was expected");

            // Can't test the LandingDate field as it is caluculated from multiple other tables which haven't been set up in this test
            //Assert.IsTrue(!string.IsNullOrEmpty(pos.LandingDate), $"Error: LandingDate '{pos.LandingDate}' was returned when a non-empty value was expected");
            Assert.IsTrue(!string.IsNullOrEmpty(pos.RealisticRequiredDate), $"Error: RealisticRequiredDate '{pos.RealisticRequiredDate}' was returned when a non-empty value was expected");
            Assert.IsTrue(!string.IsNullOrEmpty(pos.RequiredDate), $"Error: RequiredDate '{pos.RequiredDate}' was returned when a non-empty value was expected");
            Assert.IsTrue(!string.IsNullOrEmpty(pos.CompletedDate), $"Error: CompletedDate '{pos.CompletedDate}' was returned when a non-empty value was expected");
        }

        private PurchaseOrderDetailTempModel createPurchaseOrderDetailTemp(PurchaseOrderDetailTempListModel model) {
            // Create a new item by duplicating a randomly selected item already in the list
            // This is an easy way, otherwise we have to go looking for suppliers, products, tax codes etc
            var podt = PurchasingService.MapToModel(model.Items[RandomInt(0, model.Items.Count() - 1)]);
            podt.Id = 0;
            podt.LineNumber = 0;
            podt.OrderQty = RandomInt();

            return podt;
        }
    }
}
