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
        public void FindRegionsModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindRegionsModel(testCompany.Id);
            var dbData = db.FindRegions(testCompany.Id);

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
            var newItem = createRegion(testCompany);
            var error = LookupService.InsertOrUpdateRegion(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindRegionsModel(testCompany.Id);
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteRegion(newItem.Id);

            model = LookupService.FindRegionsModel(testCompany.Id);
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindRegionsListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindRegionsListItemModel(testCompany.Id);
            var dbData = db.FindRegions(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.RegionName, $"Error: Model Text is {item.Text} when {dbItem.RegionName} was expected");
            }

            // Add another item a make sure it is found
            var newItem = createRegion(testCompany);
            var error = LookupService.InsertOrUpdateRegion(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindRegionsListItemModel(testCompany.Id);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteRegion(newItem.Id);

            model = LookupService.FindRegionsListItemModel(testCompany.Id);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindRegionsListModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindRegionsListModel(testCompany.Id, 0, 1, PageSize, "");
            var dbData = db.FindRegions(testCompany.Id);

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
            var newItem = createRegion(testCompany);
            var error = LookupService.InsertOrUpdateRegion(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindRegionsListModel(testCompany.Id, 0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteRegion(newItem.Id);

            model = LookupService.FindRegionsListModel(testCompany.Id, 0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindRegionModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var model = createRegion(testCompany);
            var error = LookupService.InsertOrUpdateRegion(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = LookupService.FindRegionModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateRegionTest() {
            // Tested in DeleteRegionTest, but additional tests here
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var testRegion1 = createRegion(testCompany);
            var error = LookupService.InsertOrUpdateRegion(testRegion1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindRegion(testRegion1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testRegion1, testModel);

            var testRegion2 = createRegion(testCompany);
            error = LookupService.InsertOrUpdateRegion(testRegion2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindRegion(testRegion2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testRegion2, testModel);


            // Try to create a Region with the same name
            var dupItem = LookupService.Clone(testRegion1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateRegion(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate Region returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockRegion(testRegion1);

            testRegion1.RegionName = RandomString();
            error = LookupService.InsertOrUpdateRegion(testRegion1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockRegion(testRegion1);

            testRegion1.RegionName = testRegion2.RegionName;
            error = LookupService.InsertOrUpdateRegion(testRegion1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a Region to the same name as an existing Region returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteRegionTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a region
            RegionModel model = createRegion(testCompany);

            var error = LookupService.InsertOrUpdateRegion(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindRegion(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteRegion(model.Id);

            // And check that is was deleted
            result = db.FindRegion(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockRegionTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createRegion(testCompany);

            var error = LookupService.InsertOrUpdateRegion(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockRegion(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateRegion(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateRegion(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockRegion(model);
            error = LookupService.InsertOrUpdateRegion(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void CopyRegionsTest() {
            // Tested by all tests which create a test company
        }

        RegionModel createRegion(CompanyModel company) {
            RegionModel model = new RegionModel {
                CompanyId = company.Id,
                RegionName = RandomString(),
                CountryCode = RandomString().Left(3),
                PostCodeFrom = RandomString().Left(10),
                PostCodeTo = RandomString().Left(10),
                FreightRate = (decimal)50,
                Enabled = true
            };
            return model;
        }
    }
}
