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
        public void FindTaxCodesModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindTaxCodesModel(testCompany.Id);
            var dbData = db.FindTaxCodes(testCompany.Id);

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
            var newItem = createTaxCode(testCompany);
            var error = LookupService.InsertOrUpdateTaxCode(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindTaxCodesModel(testCompany.Id);
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteTaxCode(newItem.Id);

            model = LookupService.FindTaxCodesModel(testCompany.Id);
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindTaxCodesListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindTaxCodesListItemModel(testCompany);
            var dbData = db.FindTaxCodes(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.TaxCode1, $"Error: Model Text is {item.Text} when {dbItem.TaxCode1} was expected");
            }

            // Add another item a make sure it is found
            var newItem = createTaxCode(testCompany);
            var error = LookupService.InsertOrUpdateTaxCode(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindTaxCodesListItemModel(testCompany);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteTaxCode(newItem.Id);

            model = LookupService.FindTaxCodesListItemModel(testCompany);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindTaxCodesListModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindTaxCodesListModel(testCompany.Id, 0, 1, PageSize, "");
            var dbData = db.FindTaxCodes(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Items.Where(i => i.Enabled == true).Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items.Where(i => i.Enabled == true)) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = LookupService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createTaxCode(testCompany);
            var error = LookupService.InsertOrUpdateTaxCode(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindTaxCodesListModel(testCompany.Id, 0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteTaxCode(newItem.Id);

            model = LookupService.FindTaxCodesListModel(testCompany.Id, 0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindTaxCodeModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var model = createTaxCode(testCompany);
            var error = LookupService.InsertOrUpdateTaxCode(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = LookupService.FindTaxCodeModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateTaxCodeTest() {
            // Tested in DeleteTaxCodeTest, but additional tests here
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var testTaxCode1 = createTaxCode(testCompany);
            var error = LookupService.InsertOrUpdateTaxCode(testTaxCode1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindTaxCode(testTaxCode1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testTaxCode1, testModel);

            var testTaxCode2 = createTaxCode(testCompany);
            error = LookupService.InsertOrUpdateTaxCode(testTaxCode2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindTaxCode(testTaxCode2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testTaxCode2, testModel);


            // Try to create a TaxCode with the same name
            var dupItem = LookupService.Clone(testTaxCode1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateTaxCode(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate TaxCode returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockTaxCode(testTaxCode1);

            testTaxCode1.TaxCode = RandomString().Left(3);
            error = LookupService.InsertOrUpdateTaxCode(testTaxCode1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockTaxCode(testTaxCode1);

            testTaxCode1.TaxCode = testTaxCode2.TaxCode;
            error = LookupService.InsertOrUpdateTaxCode(testTaxCode1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a TaxCode to the same name as an existing TaxCode returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteTaxCodeTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a tax code
            TaxCodeModel model = createTaxCode(testCompany);

            var error = LookupService.InsertOrUpdateTaxCode(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindTaxCode(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteTaxCode(model.Id);

            // And check that is was deleted
            result = db.FindTaxCode(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockTaxCodeTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createTaxCode(testCompany);

            var error = LookupService.InsertOrUpdateTaxCode(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockTaxCode(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateTaxCode(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateTaxCode(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockTaxCode(model);
            error = LookupService.InsertOrUpdateTaxCode(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void CopyTaxCodesTest() {
            // Tested by all tests which create a test company
        }

        TaxCodeModel createTaxCode(CompanyModel company) {
            TaxCodeModel model = new TaxCodeModel {
                CompanyId = company.Id,
                TaxCode = RandomString().Left(3),
                TaxCodeDescription = RandomString().Left(30),
                TaxPercentageRate = (decimal)20.5,
                TaxCodeTypeId = RandomString().Left(3),
                Enabled = true
            };
            return model;
        }
    }
}
