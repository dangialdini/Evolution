using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.LookupService;
using Evolution.CommonService;
using Evolution.Models.Models;

namespace Evolution.LookupServiceTests {
    public partial class LookupServiceTests {
        [TestMethod]
        public void FindCountriesModelTest() {
            var user = GetTestUser();
            var model = LookupService.FindCountriesModel();
            var dbData = db.FindCountries();

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
            var newItem = createCountry();
            var error = LookupService.InsertOrUpdateCountry(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindCountriesModel();
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteCountry(newItem.Id);

            model = LookupService.FindCountriesModel();
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindCountriesListItemModelTest() {
            var user = GetTestUser();
            var model = LookupService.FindCountriesListItemModel();
            var dbData = db.FindCountries();

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach(var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.CountryName, $"Error: Model Text is {item.Text} when {dbItem.CountryName} was expected");
            }

            // Add another item a make sure it is found
            var newItem = createCountry();
            var error = LookupService.InsertOrUpdateCountry(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindCountriesListItemModel();
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteCountry(newItem.Id);

            model = LookupService.FindCountriesListItemModel();
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindCountriesListModelTest() {
            var user = GetTestUser();
            var model = LookupService.FindCountriesListModel(0, 1, PageSize, "");
            var dbData = db.FindCountries();

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
            var newItem = createCountry();
            var error = LookupService.InsertOrUpdateCountry(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindCountriesListModel(0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteCountry(newItem.Id);

            model = LookupService.FindCountriesListModel(0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindCountryTest() {
            var user = GetTestUser();

            var model = createCountry();
            var error = LookupService.InsertOrUpdateCountry(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = db.FindCountry(model.Id);
            var result = LookupService.MapToModel(test);
            AreEqual(model, result);
        }

        [TestMethod]
        public void FindCountryModelTest() {
            var user = GetTestUser();

            var model = createCountry();
            var error = LookupService.InsertOrUpdateCountry(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = LookupService.FindCountryModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateCountryTest() {
            // Tested in DeleteCountryTest, but additional tests here
            var testUser = GetTestUser();

            var testCountry1 = createCountry();
            var error = LookupService.InsertOrUpdateCountry(testCountry1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindCountry(testCountry1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testCountry1, testModel);

            var testCountry2 = createCountry();
            error = LookupService.InsertOrUpdateCountry(testCountry2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindCountry(testCountry2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testCountry2, testModel);


            // Try to create a Country with the same name
            var dupItem = LookupService.Clone(testCountry1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateCountry(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate Country returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockCountry(testCountry1);

            testCountry1.CountryName = RandomString();
            error = LookupService.InsertOrUpdateCountry(testCountry1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockCountry(testCountry1);

            testCountry1.CountryName = testCountry2.CountryName;
            error = LookupService.InsertOrUpdateCountry(testCountry1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a country to the same name as an existing Country returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteCountryTest() {
            // Get a test user
            var user = GetTestUser();

            // Create a record
            var model = createCountry();

            var error = LookupService.InsertOrUpdateCountry(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindCountry(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteCountry(model.Id);

            // And check that is was deleted
            result = db.FindCountry(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockCountryTest() {
            var testUser = GetTestUser();

            // Create a record
            var model = createCountry();

            var error = LookupService.InsertOrUpdateCountry(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockCountry(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateCountry(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateCountry(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockCountry(model);
            error = LookupService.InsertOrUpdateCountry(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        CountryModel createCountry() {
            CountryModel model = new CountryModel {
                CountryName = RandomString(),
                ISO2Code = "TC",
                ISO3Code = "TCC",
                UNCode = RandomInt(1, 99),
                Enabled = true
            };
            return model;
        }
    }
}
