using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.AllocationService;
using Evolution.Models.Models;

namespace Evolution.AllocationServiceTests {
    public partial class AllocationServiceTests : BaseTest {
        [Ignore]
        [TestMethod]
        public void FindAllocationsListModelTest() {
            // TBD:
            // db.FindAllocationsListModel is a stored-proc call
            Assert.Fail("Test to be implemented!");
        }

        [TestMethod]
        public void FindAllocationListModelTest() {
            // Gets a list of allocations
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var productList = FindProductsForTest(db.FindParentCompany(), 2);
            Assert.IsTrue(productList.Count() > 0, "Error: No products were returned for the parent company. This could be because the parent company has not been flagged or it has no allocations");

            var product = productList[0];
            var model = AllocationService.FindAllocationListModel(testCompany, product);
            var dbData = db.FindAllocationsForCompany(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = AllocationService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            product = productList[1];
            var newItem = createAllocation(testCompany, product);
            var error = AllocationService.InsertOrUpdateAllocation(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = AllocationService.FindAllocationListModel(testCompany, product);
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            AllocationService.DeleteAllocation(newItem.Id);

            model = AllocationService.FindAllocationListModel(testCompany, product);
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [Ignore]
        [TestMethod]
        public void FindAllocationsToPurchaseOrderTest() {
            Assert.Fail("Test to be implemented!");
        }

        [Ignore]
        [TestMethod]
        public void FindAllocationsForSaleTest() {
            Assert.Fail("Test to be implemented!");
        }

        [Ignore]
        [TestMethod]
        public void FindAvailabilityDetailsTest() {
            Assert.Fail("Test to be implemented!");
        }

        [Ignore]
        [TestMethod]
        public void FindSaleDetailsTest() {
            Assert.Fail("Test to be implemented!");
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all Find tests
        }

        [TestMethod]
        public void InsertOrUpdateAllocationTest() {
            // Tested in FindAllocationListModelTest above
        }

        [TestMethod]
        public void DeleteAllocationTest() {
            // Get a test user
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var productList = FindProductsForTest(db.FindParentCompany(), 1);
            Assert.IsTrue(productList.Count() > 0, "Error: No products were returned for the parent company. This could be because the parent company has not been flagged or it has no allocations");

            AllocationModel model = createAllocation(testCompany, productList[0]);

            var error = AllocationService.InsertOrUpdateAllocation(model, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindAllocation(model.Id);
            var test = AllocationService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            AllocationService.DeleteAllocation(model.Id);

            // And check that is was deleted
            result = db.FindAllocation(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [Ignore]
        [TestMethod]
        public void DeleteAllocationsForSaleLineTest() {
            Assert.Fail("Test to be implemented");
        }

        [Ignore]
        [TestMethod]
        public void DeleteAllocationsForPurchaseLineTest() {
            Assert.Fail("Test to be implemented");
        }

        [TestMethod]
        public void LockAllocationTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var productList = FindProductsForTest(db.FindParentCompany(), 1);
            Assert.IsTrue(productList.Count() > 0, "Error: No products were returned for the parent company. This could be because the parent company has not been flagged or it has no allocations");

            var model = createAllocation(testCompany, productList[0]);

            var error = AllocationService.InsertOrUpdateAllocation(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = AllocationService.LockAllocation(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = AllocationService.InsertOrUpdateAllocation(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = AllocationService.InsertOrUpdateAllocation(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = AllocationService.LockAllocation(model);
            error = AllocationService.InsertOrUpdateAllocation(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }
    }
}
