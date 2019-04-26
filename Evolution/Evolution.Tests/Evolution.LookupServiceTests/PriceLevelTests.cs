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
        public void FindPriceLevelsModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var model = LookupService.FindPriceLevelsModel(testCompany.Id);
            var dbData = db.FindPriceLevels(testCompany.Id);

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
            var newItem = createPriceLevel(testCompany);
            var error = LookupService.InsertOrUpdatePriceLevel(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindPriceLevelsModel(testCompany.Id);
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeletePriceLevel(newItem.Id);

            model = LookupService.FindPriceLevelsModel(testCompany.Id);
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindPriceLevelsListItemModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var model = LookupService.FindPriceLevelsListItemModel(testCompany);
            var dbData = db.FindPriceLevels(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");

                var test = LookupService.MapToModel(dbItem);
                AreEqual(item, test);
            }

            // Add another item a make sure it is found
            var newItem = createPriceLevel(testCompany);
            var error = LookupService.InsertOrUpdatePriceLevel(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindPriceLevelsListItemModel(testCompany);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeletePriceLevel(newItem.Id);

            model = LookupService.FindPriceLevelsListItemModel(testCompany);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindPriceLevelsListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var model = LookupService.FindPriceLevelsListModel(testCompany.Id, 0, 1, PageSize, "");
            var dbData = db.FindPriceLevels(testCompany.Id);

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
            var newItem = createPriceLevel(testCompany);
            var error = LookupService.InsertOrUpdatePriceLevel(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindPriceLevelsListModel(testCompany.Id, 0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeletePriceLevel(newItem.Id);

            model = LookupService.FindPriceLevelsListModel(testCompany.Id, 0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindPriceLevelModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var model = createPriceLevel(testCompany);
            var error = LookupService.InsertOrUpdatePriceLevel(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = LookupService.FindPriceLevelModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdatePriceLevelTest() {
            // Tested in DeletePriceLevelTest, but additional tests here
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var testPriceLevel1 = createPriceLevel(testCompany);
            var error = LookupService.InsertOrUpdatePriceLevel(testPriceLevel1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindPriceLevel(testPriceLevel1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testPriceLevel1, testModel);

            var testPriceLevel2 = createPriceLevel(testCompany);
            error = LookupService.InsertOrUpdatePriceLevel(testPriceLevel2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindPriceLevel(testPriceLevel2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testPriceLevel2, testModel);


            // Try to create a PriceLevel with the same name
            var dupItem = LookupService.Clone(testPriceLevel1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdatePriceLevel(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate PriceLevel returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockPriceLevel(testPriceLevel1);

            testPriceLevel1.Mneumonic = RandomString().Left(3);
            error = LookupService.InsertOrUpdatePriceLevel(testPriceLevel1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockPriceLevel(testPriceLevel1);

            testPriceLevel1.Mneumonic = testPriceLevel2.Mneumonic;
            error = LookupService.InsertOrUpdatePriceLevel(testPriceLevel1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a PriceLevel to the same name as an existing PriceLevel returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeletePriceLevelTest() {
            // Get a test user
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a price level
            PriceLevelModel model = createPriceLevel(testCompany);

            var error = LookupService.InsertOrUpdatePriceLevel(model, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindPriceLevel(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeletePriceLevel(model.Id);

            // And check that is was deleted
            result = db.FindPriceLevel(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockPriceLevelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createPriceLevel(testCompany);

            var error = LookupService.InsertOrUpdatePriceLevel(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockPriceLevel(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdatePriceLevel(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdatePriceLevel(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockPriceLevel(model);
            error = LookupService.InsertOrUpdatePriceLevel(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void CopyPriceLevelsTest() {
            // Tested by all tests which create a test company
        }

        PriceLevelModel createPriceLevel(CompanyModel company) {
            PriceLevelModel model = new PriceLevelModel {
                CompanyId = company.Id,
                Mneumonic = RandomString().Left(3),
                Description = RandomString().Left(30),
                ImportPriceLevel = RandomString().Left(1),
                ImportSalesTaxCalcMethod = RandomString().Left(1),
                Enabled = true
            };
            return model;
        }
    }
}
