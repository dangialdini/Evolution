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
        public void FindSalesOrderHeaderStatusListItemModelTest() {
            var user = GetTestUser();
            var model = LookupService.FindSalesOrderHeaderStatusListItemModel();
            var dbData = db.FindSalesOrderHeaderStatuses();

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.StatusName, $"Error: Model Text is {item.Text} when {dbItem.StatusName} was expected");
            }

            // Add another item a make sure it is found
            var newItem = createSalesOrderHeaderStatus();
            var error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindSalesOrderHeaderStatusListItemModel();
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteSalesOrderHeaderStatus(newItem.Id);

            model = LookupService.FindSalesOrderHeaderStatusListItemModel();
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindSalesOrderHeaderStatusByValueModelTest() {
            var testUser = GetTestUser();

            var testStatus = createSalesOrderHeaderStatus();
            var error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(testStatus, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var check = LookupService.FindSalesOrderHeaderStatusByValueModel((SalesOrderHeaderStatus)testStatus.StatusValue);
            AreEqual(testStatus, check);

            // Now delete it and check it has gone
            LookupService.DeleteSalesOrderHeaderStatus(testStatus.Id);
            check = LookupService.FindSalesOrderHeaderStatusByValueModel((SalesOrderHeaderStatus)testStatus.StatusValue);
            Assert.IsTrue(check == null, "Error: An objetc was returned when a NULL value was expected");
        }

        [TestMethod]
        public void InsertOrUpdateSalesOrderHeaderStatusTest() {
            // Tested in DeleteSalesOrderStatuTest, but additional tests here
            var testUser = GetTestUser();

            var testSalesOrderStatu1 = createSalesOrderHeaderStatus();
            var error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(testSalesOrderStatu1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindSalesOrderHeaderStatus(testSalesOrderStatu1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testSalesOrderStatu1, testModel);

            var testSalesOrderStatu2 = createSalesOrderHeaderStatus();
            error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(testSalesOrderStatu2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindSalesOrderHeaderStatus(testSalesOrderStatu2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testSalesOrderStatu2, testModel);


            // Try to create a SalesOrderStatus with the same name
            var dupItem = LookupService.Clone(testSalesOrderStatu1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate SalesOrderStatus returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockSalesOrderHeaderStatus(testSalesOrderStatu1);

            testSalesOrderStatu1.StatusName = RandomString();
            testSalesOrderStatu1.StatusValue++;
            error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(testSalesOrderStatu1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockSalesOrderHeaderStatus(testSalesOrderStatu1);

            testSalesOrderStatu1.StatusName = testSalesOrderStatu2.StatusName;
            testSalesOrderStatu1.StatusValue = testSalesOrderStatu2.StatusValue;
            error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(testSalesOrderStatu1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a SalesOrderStatus to the same name as an existing SalesOrderStatus returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteSalesOrderHeaderStatusTest() {
            // Get a test user
            var user = GetTestUser();

            var model = createSalesOrderHeaderStatus();

            var error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindSalesOrderHeaderStatus(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteSalesOrderHeaderStatus(model.Id);

            // And check that is was deleted
            result = db.FindSalesOrderHeaderStatus(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockSalesOrderHeaderStatusTest() {
            var testUser = GetTestUser();

            // Create a record
            var model = createSalesOrderHeaderStatus();

            var error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockSalesOrderHeaderStatus(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockSalesOrderHeaderStatus(model);
            error = LookupService.InsertOrUpdateSalesOrderHeaderStatus(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        private SalesOrderHeaderStatusModel createSalesOrderHeaderStatus() {
            return new SalesOrderHeaderStatusModel {
                StatusName = RandomString(),
                StatusValue = RandomInt()
            };
        }
    }
}
