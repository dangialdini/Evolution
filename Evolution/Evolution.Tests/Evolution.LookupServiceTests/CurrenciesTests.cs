using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.LookupService;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.LookupServiceTests {
    public partial class LookupServiceTests {
        [TestMethod]
        public void FindCurrenciesModelTest() {
            var user = GetTestUser();
            var model = LookupService.FindCurrenciesModel();
            var dbData = db.FindCurrencies();

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
            var newItem = createCurrency();
            var error = LookupService.InsertOrUpdateCurrency(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindCurrenciesModel();
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteCurrency(newItem.Id);

            model = LookupService.FindCurrenciesModel();
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindCurrenciesListItemModelTest() {
            var user = GetTestUser();
            var model = LookupService.FindCurrenciesListItemModel();
            var dbData = db.FindCurrencies();

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.CurrencyName, $"Error: Model Text is {item.Text} when {dbItem.CurrencyName} was expected");
            }

            // Add another item a make sure it is found
            var newItem = createCurrency();
            var error = LookupService.InsertOrUpdateCurrency(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindCurrenciesListItemModel();
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteCurrency(newItem.Id);

            model = LookupService.FindCurrenciesListItemModel();
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindCurrenciesListModelTest() {
            var user = GetTestUser();
            var model = LookupService.FindCurrenciesListModel(0, 1, PageSize, "");
            var dbData = db.FindCurrencies();

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
            var newItem = createCurrency();
            var error = LookupService.InsertOrUpdateCurrency(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindCurrenciesListModel(0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteCurrency(newItem.Id);

            model = LookupService.FindCurrenciesListModel(0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindCurrencyModelTest() {
            var user = GetTestUser();

            var model = createCurrency();
            var error = LookupService.InsertOrUpdateCurrency(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = db.FindCurrency(model.Id);
            var result = LookupService.MapToModel(test);
            AreEqual(model, result);
        }

        [TestMethod]
        public void FindCurrencySymbolTest() {
            var user = GetTestUser();

            var model = createCurrency();
            model.CurrencySymbol = RandomString().Left(4);
            var error = LookupService.InsertOrUpdateCurrency(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = LookupService.FindCurrencySymbol(model.Id);
            Assert.IsTrue(test == model.CurrencySymbol, $"Error: {test} was returned when {model.CurrencySymbol} was expected");
        }

        [TestMethod]
        public void InsertOrUpdateCurrencyTest() {
            // Tested in DeleteCurrencyTest, but additional tests here
            var testUser = GetTestUser();

            var testCurrency1 = createCurrency();
            var error = LookupService.InsertOrUpdateCurrency(testCurrency1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindCurrency(testCurrency1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testCurrency1, testModel);

            var testCurrency2 = createCurrency();
            error = LookupService.InsertOrUpdateCurrency(testCurrency2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindCurrency(testCurrency2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testCurrency2, testModel);


            // Try to create a Currency with the same name
            var dupItem = LookupService.Clone(testCurrency1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateCurrency(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate Currency returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockCurrency(testCurrency1);

            testCurrency1.CurrencyCode = RandomString().Left(3);
            error = LookupService.InsertOrUpdateCurrency(testCurrency1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockCurrency(testCurrency1);

            testCurrency1.CurrencyCode = testCurrency2.CurrencyCode;
            error = LookupService.InsertOrUpdateCurrency(testCurrency1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a Currency to the same name as an existing Currency returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteCurrencyTest() {
            // Get a test user
            var user = GetTestUser();

            // Create a currency
            CurrencyModel model = createCurrency();

            var error = LookupService.InsertOrUpdateCurrency(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindCurrency(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteCurrency(model.Id);

            // And check that is was deleted
            result = db.FindCurrency(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockCurrencyTest() {
            var testUser = GetTestUser();

            // Create a record
            var model = createCurrency();

            var error = LookupService.InsertOrUpdateCurrency(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockCurrency(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateCurrency(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateCurrency(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockCurrency(model);
            error = LookupService.InsertOrUpdateCurrency(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        CurrencyModel createCurrency() {
            CurrencyModel model = new CurrencyModel {
                CurrencyCode = RandomString().Left(3),
                CurrencyName = RandomString().Left(30),
                ExchangeRate = (decimal)1.25,
                CurrencySymbol = "$",
                FormatTemplate = "$###.##",
                Enabled = true
            };
            return model;
        }
    }
}
