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
        public void FindFreightCarriersListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindFreightCarriersListItemModel(testCompany.Id);
            var dbData = db.FindFreightCarriers(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.FreightCarrier1, $"Error: Model Text is {item.Text} when {dbItem.FreightCarrier1} was expected");
            }

            // Add another item a make sure it is found
            var newItem = createFreightCarrier(testCompany);
            var error = LookupService.InsertOrUpdateFreightCarrier(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindFreightCarriersListItemModel(testCompany.Id);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteFreightCarrier(newItem.Id);

            model = LookupService.FindFreightCarriersListItemModel(testCompany.Id);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindFreightCarriersListModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindFreightCarriersListModel(testCompany.Id, 0, 1, PageSize, "");
            var dbData = db.FindFreightCarriers(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = LookupService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createFreightCarrier(testCompany);
            var error = LookupService.InsertOrUpdateFreightCarrier(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindFreightCarriersListModel(testCompany.Id, 0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteFreightCarrier(newItem.Id);

            model = LookupService.FindFreightCarriersListModel(testCompany.Id, 0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindFreightCarrierModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var model = createFreightCarrier(testCompany);
            var error = LookupService.InsertOrUpdateFreightCarrier(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = LookupService.FindFreightCarrierModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateFreightCarrierTest() {
            // Tested in DeleteFreightCarrierTest, but additional tests here
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var testFreightCarrier1 = createFreightCarrier(testCompany);
            var error = LookupService.InsertOrUpdateFreightCarrier(testFreightCarrier1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindFreightCarrier(testFreightCarrier1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testFreightCarrier1, testModel);

            var testFreightCarrier2 = createFreightCarrier(testCompany);
            error = LookupService.InsertOrUpdateFreightCarrier(testFreightCarrier2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindFreightCarrier(testFreightCarrier2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testFreightCarrier2, testModel);


            // Try to create a FreightCarrier with the same name
            var dupItem = LookupService.Clone(testFreightCarrier1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateFreightCarrier(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate Freight Carrier returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockFreightCarrier(testFreightCarrier1);

            testFreightCarrier1.FreightCarrier = RandomString();
            error = LookupService.InsertOrUpdateFreightCarrier(testFreightCarrier1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockFreightCarrier(testFreightCarrier1);

            testFreightCarrier1.FreightCarrier = testFreightCarrier2.FreightCarrier;
            error = LookupService.InsertOrUpdateFreightCarrier(testFreightCarrier1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a Freight Carrier to the same name as an existing Freight Carrier returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteFreightCarrierTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a freight carrier
            FreightCarrierModel model = createFreightCarrier(testCompany);

            var error = LookupService.InsertOrUpdateFreightCarrier(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindFreightCarrier(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteFreightCarrier(model.Id);

            // And check that is was deleted
            result = db.FindFreightCarrier(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockFreightCarrierTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createFreightCarrier(testCompany);

            var error = LookupService.InsertOrUpdateFreightCarrier(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockFreightCarrier(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateFreightCarrier(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateFreightCarrier(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockFreightCarrier(model);
            error = LookupService.InsertOrUpdateFreightCarrier(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        FreightCarrierModel createFreightCarrier(CompanyModel company) {
            FreightCarrierModel model = new FreightCarrierModel {
                CompanyId = company.Id,
                FreightCarrier = RandomString(),
                AutoBuildTrackingLink = true,
                HTTPPrefix = RandomString(),
                Enabled = true
            };
            return model;
        }
    }
}
