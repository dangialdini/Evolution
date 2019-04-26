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
        public void FindPurchaseOrderHeaderStatusListItemModelTest() {
            var user = GetTestUser();
            var model = LookupService.FindPurchaseOrderHeaderStatusListItemModel();
            var dbData = db.FindPurchaseOrderHeaderStatuses();

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
            }

            // Add another item a make sure it is found
            var newItem = createPurchaseOrderHeaderStatus();
            var error = LookupService.InsertOrUpdatePurchaseOrderHeaderStatus(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindPurchaseOrderHeaderStatusListItemModel();
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeletePurchaseOrderHeaderStatus(newItem.Id);

            model = LookupService.FindPurchaseOrderHeaderStatusListItemModel();
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindPurchaseOrderHeaderStatusByValueModelTest() {
            var poStatusList = db.FindPurchaseOrderHeaderStatuses()
                                 .ToList();

            foreach (PurchaseOrderStatus poStatus in Enum.GetValues(typeof(PurchaseOrderStatus))) {
                // Find each status in the enum
                var test1 = LookupService.FindPurchaseOrderHeaderStatusByValueModel(poStatus);
                Assert.IsTrue(test1 != null, "Error: A NULL value was returned when an object was expected");

                // Check it exists in the db list
                var test2 = poStatusList.Where(posl => posl.Id.ToString() == test1.Id.ToString())
                                        .FirstOrDefault();
                Assert.IsTrue(test2 != null, $"Error: Status {test1.Id} / {test1.StatusName} could not be found in the database list - this suggests that the Enum values are different to those in the PurchaseOrderHeaderStatus database table");
            }
        }

        [TestMethod]
        public void InsertOrUpdatePurchaseOrderHeaderStatusTest() {
            // Tested in DeletePurchaseOrderHeaderStatusTest, but additional tests here
            var testUser = GetTestUser();

            var testPurchaseOrderHeaderStatus1 = createPurchaseOrderHeaderStatus();
            var error = LookupService.InsertOrUpdatePurchaseOrderHeaderStatus(testPurchaseOrderHeaderStatus1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindPurchaseOrderHeaderStatus(testPurchaseOrderHeaderStatus1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testPurchaseOrderHeaderStatus1, testModel);

            var testPurchaseOrderHeaderStatus2 = createPurchaseOrderHeaderStatus();
            error = LookupService.InsertOrUpdatePurchaseOrderHeaderStatus(testPurchaseOrderHeaderStatus2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindPurchaseOrderHeaderStatus(testPurchaseOrderHeaderStatus2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testPurchaseOrderHeaderStatus2, testModel);


            // Try to create a PurchaseOrderHeaderStatus with the same name
            var dupItem = LookupService.Clone(testPurchaseOrderHeaderStatus1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdatePurchaseOrderHeaderStatus(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate PurchaseOrderHeaderStatus returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockPurchaseOrderHeaderStatus(testPurchaseOrderHeaderStatus1);

            testPurchaseOrderHeaderStatus1.StatusName = RandomString().Left(3);
            error = LookupService.InsertOrUpdatePurchaseOrderHeaderStatus(testPurchaseOrderHeaderStatus1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockPurchaseOrderHeaderStatus(testPurchaseOrderHeaderStatus1);

            testPurchaseOrderHeaderStatus1.StatusName = testPurchaseOrderHeaderStatus2.StatusName;
            error = LookupService.InsertOrUpdatePurchaseOrderHeaderStatus(testPurchaseOrderHeaderStatus1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a PurchaseOrderHeaderStatus to the same name as an existing PurchaseOrderHeaderStatus returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeletePurchaseOrderHeaderStatusTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a price level
            PurchaseOrderHeaderStatusModel model = createPurchaseOrderHeaderStatus();

            var error = LookupService.InsertOrUpdatePurchaseOrderHeaderStatus(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindPurchaseOrderHeaderStatus(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeletePurchaseOrderHeaderStatus(model.Id);

            // And check that is was deleted
            result = db.FindPurchaseOrderHeaderStatus(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockPurchaseOrderHeaderStatusTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createPurchaseOrderHeaderStatus();

            var error = LookupService.InsertOrUpdatePurchaseOrderHeaderStatus(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockPurchaseOrderHeaderStatus(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdatePurchaseOrderHeaderStatus(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdatePurchaseOrderHeaderStatus(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockPurchaseOrderHeaderStatus(model);
            error = LookupService.InsertOrUpdatePurchaseOrderHeaderStatus(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        PurchaseOrderHeaderStatusModel createPurchaseOrderHeaderStatus() {
            var model = new PurchaseOrderHeaderStatusModel {
                StatusName = RandomString(),
                StatusValue = RandomInt(1000, 1100)
            };
            return model;
        }
    }
}
