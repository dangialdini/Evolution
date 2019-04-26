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
        public void FindFreightForwardersListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindFreightForwardersListItemModel(testCompany);
            var dbData = db.FindFreightForwarders(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.Name, $"Error: Model Text is {item.Text} when {dbItem.Name} was expected");
            }

            // Add another item a make sure it is found
            var newItem = createFreightForwarder(testCompany);
            var error = LookupService.InsertOrUpdateFreightForwarder(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindFreightForwardersListItemModel(testCompany);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteFreightForwarder(newItem.Id);

            model = LookupService.FindFreightForwardersListItemModel(testCompany);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindFreightForwardersListModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindFreightForwardersListModel(testCompany.Id, 0, 1, PageSize, "");
            var dbData = db.FindFreightForwarders(testCompany.Id);

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
            var newItem = createFreightForwarder(testCompany);
            var error = LookupService.InsertOrUpdateFreightForwarder(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindFreightForwardersListModel(testCompany.Id, 0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteFreightForwarder(newItem.Id);

            model = LookupService.FindFreightForwardersListModel(testCompany.Id, 0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindFreightForwarderModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var model = createFreightForwarder(testCompany);
            var error = LookupService.InsertOrUpdateFreightForwarder(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = LookupService.FindFreightForwarderModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateFreightForwarderTest() {
            // Tested in DeleteFreightForwarderTest, but additional tests here
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var testFreightForwarder1 = createFreightForwarder(testCompany);
            var error = LookupService.InsertOrUpdateFreightForwarder(testFreightForwarder1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindFreightForwarder(testFreightForwarder1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testFreightForwarder1, testModel);

            var testFreightForwarder2 = createFreightForwarder(testCompany);
            error = LookupService.InsertOrUpdateFreightForwarder(testFreightForwarder2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindFreightForwarder(testFreightForwarder2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testFreightForwarder2, testModel);


            // Try to create a FreightForwarder with the same name
            var dupItem = LookupService.Clone(testFreightForwarder1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateFreightForwarder(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate FreightForwarder returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockFreightForwarder(testFreightForwarder1);

            testFreightForwarder1.Name = RandomString();
            error = LookupService.InsertOrUpdateFreightForwarder(testFreightForwarder1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockFreightForwarder(testFreightForwarder1);

            testFreightForwarder1.Name = testFreightForwarder2.Name;
            error = LookupService.InsertOrUpdateFreightForwarder(testFreightForwarder1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a FreightForwarder to the same name as an existing FreightForwarder returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteFreightForwarderTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a record
            FreightForwarderModel model = createFreightForwarder(testCompany);

            var error = LookupService.InsertOrUpdateFreightForwarder(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindFreightForwarder(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteFreightForwarder(model.Id);

            // And check that is was deleted
            result = db.FindFreightForwarder(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockFreightForwarderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createFreightForwarder(testCompany);

            var error = LookupService.InsertOrUpdateFreightForwarder(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockFreightForwarder(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateFreightForwarder(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateFreightForwarder(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockFreightForwarder(model);
            error = LookupService.InsertOrUpdateFreightForwarder(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        private FreightForwarderModel createFreightForwarder(CompanyModel company) {
            var model = new FreightForwarderModel {
                CompanyId = company.Id,
                Name = RandomString(),
                Enabled = true };
            return model;
        }
    }
}
