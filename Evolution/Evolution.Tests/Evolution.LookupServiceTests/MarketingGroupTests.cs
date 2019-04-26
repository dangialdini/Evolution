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
        public void FindMarketingGroupsModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindMarketingGroupsModel(testCompany.Id);
            var dbData = db.FindMarketingGroups(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = LookupService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createMarketingGroup(testCompany);
            var error = LookupService.InsertOrUpdateMarketingGroup(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindMarketingGroupsModel(testCompany.Id);
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteMarketingGroup(newItem.Id);

            model = LookupService.FindMarketingGroupsModel(testCompany.Id);
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindMarketingGroupsListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindMarketingGroupsListItemModel(testCompany);
            var dbData = db.FindMarketingGroups(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.MarketingGroupName, $"Error: Model Text is {item.Text} when {dbItem.MarketingGroupName} was expected");
            }

            // Add another item a make sure it is found
            var newItem = createMarketingGroup(testCompany);
            var error = LookupService.InsertOrUpdateMarketingGroup(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindMarketingGroupsListItemModel(testCompany);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteMarketingGroup(newItem.Id);

            model = LookupService.FindMarketingGroupsListItemModel(testCompany);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindMarketingGroupsListModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindMarketingGroupsListModel(testCompany.Id, 0, 1, PageSize, "");
            var dbData = db.FindMarketingGroups(testCompany.Id);

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
            var newItem = createMarketingGroup(testCompany);
            var error = LookupService.InsertOrUpdateMarketingGroup(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindMarketingGroupsListModel(testCompany.Id, 0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteMarketingGroup(newItem.Id);

            model = LookupService.FindMarketingGroupsListModel(testCompany.Id, 0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindMarketingGroupModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var model = createMarketingGroup(testCompany);
            var error = LookupService.InsertOrUpdateMarketingGroup(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = LookupService.FindMarketingGroupModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateMarketingGroupTest() {
            // Tested in DeleteMarketingGroupTest, but additional tests here
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var testMarketingGroup1 = createMarketingGroup(testCompany);
            var error = LookupService.InsertOrUpdateMarketingGroup(testMarketingGroup1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindMarketingGroup(testMarketingGroup1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testMarketingGroup1, testModel);

            var testMarketingGroup2 = createMarketingGroup(testCompany);
            error = LookupService.InsertOrUpdateMarketingGroup(testMarketingGroup2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindMarketingGroup(testMarketingGroup2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testMarketingGroup2, testModel);


            // Try to create a MarketingGroup with the same name
            var dupItem = LookupService.Clone(testMarketingGroup1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateMarketingGroup(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate MarketingGroup returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockMarketingGroup(testMarketingGroup1);

            testMarketingGroup1.MarketingGroupName = RandomString().Left(30);
            error = LookupService.InsertOrUpdateMarketingGroup(testMarketingGroup1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockMarketingGroup(testMarketingGroup1);

            testMarketingGroup1.MarketingGroupName = testMarketingGroup2.MarketingGroupName;
            error = LookupService.InsertOrUpdateMarketingGroup(testMarketingGroup1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a MarketingGroup Id to the same name as an existing MarketingGroup returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteMarketingGroupTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a marketing group
            MarketingGroupModel model = createMarketingGroup(testCompany);

            var error = LookupService.InsertOrUpdateMarketingGroup(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindMarketingGroup(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteMarketingGroup(model.Id);

            // And check that is was deleted
            result = db.FindMarketingGroup(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockMarketingGroupTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createMarketingGroup(testCompany);

            var error = LookupService.InsertOrUpdateMarketingGroup(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockMarketingGroup(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateMarketingGroup(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateMarketingGroup(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockMarketingGroup(model);
            error = LookupService.InsertOrUpdateMarketingGroup(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void CopyMarketingGroupsTest() {
            // Tested by all tests which create a test company
        }

        MarketingGroupModel createMarketingGroup(CompanyModel company) {
            MarketingGroupModel model = new MarketingGroupModel {
                CompanyId = company.Id,
                MarketingGroupName = RandomString().Left(30),
                Enabled = true
            };
            return model;
        }
    }
}
