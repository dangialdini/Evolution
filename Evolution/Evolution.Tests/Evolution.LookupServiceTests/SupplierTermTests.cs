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
        public void FindSupplierTermsListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindSupplierTermsListItemModel(testCompany);
            var dbData = db.FindSupplierTerms(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.SupplierTermName, $"Error: Model Text is {item.Text} when {dbItem.SupplierTermName} was expected");
            }

            // Add another item a make sure it is found
            var newItem = createSupplierTerm(testCompany);
            var error = LookupService.InsertOrUpdateSupplierTerm(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindSupplierTermsListItemModel(testCompany);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteSupplierTerm(newItem.Id);

            model = LookupService.FindSupplierTermsListItemModel(testCompany);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindSupplierTermModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var model = createSupplierTerm(testCompany);
            var error = LookupService.InsertOrUpdateSupplierTerm(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = LookupService.FindSupplierTermModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateSupplierTermTest() {
            // Tested in SupplierTermTest, but additional tests here
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var testSupplierTerm1 = createSupplierTerm(testCompany);
            var error = LookupService.InsertOrUpdateSupplierTerm(testSupplierTerm1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindSupplierTerm(testSupplierTerm1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testSupplierTerm1, testModel);

            var testSupplierTerm2 = createSupplierTerm(testCompany);
            error = LookupService.InsertOrUpdateSupplierTerm(testSupplierTerm2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindSupplierTerm(testSupplierTerm2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testSupplierTerm2, testModel);


            // Try to create a SupplierTerm with the same name
            var dupItem = LookupService.Clone(testSupplierTerm1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateSupplierTerm(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate SupplierTerm returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockSupplierTerm(testSupplierTerm1);

            testSupplierTerm1.SupplierTermName = RandomString();
            error = LookupService.InsertOrUpdateSupplierTerm(testSupplierTerm1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockSupplierTerm(testSupplierTerm1);

            testSupplierTerm1.SupplierTermName = testSupplierTerm2.SupplierTermName;
            error = LookupService.InsertOrUpdateSupplierTerm(testSupplierTerm1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a SupplierTerm to the same name as an existing SupplierTerm returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteSupplierTermTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a record
            SupplierTermModel model = createSupplierTerm(testCompany);

            var error = LookupService.InsertOrUpdateSupplierTerm(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindSupplierTerm(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteSupplierTerm(model.Id);

            // And check that is was deleted
            result = db.FindSupplierTerm(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockSupplierTermTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createSupplierTerm(testCompany);

            var error = LookupService.InsertOrUpdateSupplierTerm(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockSupplierTerm(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateSupplierTerm(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateSupplierTerm(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockSupplierTerm(model);
            error = LookupService.InsertOrUpdateSupplierTerm(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        private SupplierTermModel createSupplierTerm(CompanyModel company) {
            var model = new SupplierTermModel { CompanyId = company.Id,
                SupplierTermName = RandomString(),
                Enabled = true };
            return model;
        }
    }
}
