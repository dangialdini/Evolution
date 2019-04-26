using System;
using System.Collections.Generic;
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
        public void FindPaymentTermsModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindPaymentTermsModel(testCompany.Id);
            var dbData = db.FindPaymentTerms(testCompany.Id);

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
            var newItem = createPaymentTerm(testCompany);
            var error = LookupService.InsertOrUpdatePaymentTerm(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindPaymentTermsModel(testCompany.Id);
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeletePaymentTerm(newItem.Id);

            model = LookupService.FindPaymentTermsModel(testCompany.Id);
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindPaymentTermsListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindPaymentTermsListItemModel(testCompany);
            var dbData = db.FindPaymentTerms(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
            }

            // Add another item a make sure it is found
            var newItem = createPaymentTerm(testCompany);
            var error = LookupService.InsertOrUpdatePaymentTerm(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindPaymentTermsListItemModel(testCompany);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeletePaymentTerm(newItem.Id);

            model = LookupService.FindPaymentTermsListItemModel(testCompany);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindPaymentTermsListModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindPaymentTermsListModel(testCompany.Id, 0, 1, PageSize);
            var dbData = db.FindPaymentTerms(testCompany.Id);

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
            var newItem = createPaymentTerm(testCompany);
            var error = LookupService.InsertOrUpdatePaymentTerm(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindPaymentTermsListModel(testCompany.Id, 0, 1, PageSize);
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeletePaymentTerm(newItem.Id);

            model = LookupService.FindPaymentTermsListModel(testCompany.Id, 0, 1, PageSize);
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindPaymentTermModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var model = createPaymentTerm(testCompany);
            var error = LookupService.InsertOrUpdatePaymentTerm(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = LookupService.FindPaymentTermModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdatePaymentTermTest() {
            // Tested in DeletePaymentTermTest
        }

        [TestMethod]
        public void DeletePaymentTermTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a template
            PaymentTermModel model = createPaymentTerm(testCompany);

            var error = LookupService.InsertOrUpdatePaymentTerm(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindPaymentTerm(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeletePaymentTerm(model.Id);

            // And check that is was deleted
            result = db.FindPaymentTerm(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockPaymentTermTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createPaymentTerm(testCompany);

            var error = LookupService.InsertOrUpdatePaymentTerm(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockPaymentTerm(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdatePaymentTerm(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdatePaymentTerm(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockPaymentTerm(model);
            error = LookupService.InsertOrUpdatePaymentTerm(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void CopyPaymentTermsTest() {
            // Tested by all tests which create a test company
        }

        PaymentTermModel createPaymentTerm(CompanyModel company) {
            PaymentTermModel model = new PaymentTermModel {
                CompanyId = company.Id,
                LatePaymentChargePercent = (decimal)10.5,
                EarlyPaymentDiscountPercent = (decimal)5.5,
                TermsOfPaymentId = "COD",
                TermText = "Cash on Delivery",
                ImportPaymentIsDue = 20,
                DiscountDays = 10,
                BalanceDueDays = 14,
                DiscountDate = RandomString().Left(3),
                BalanceDueDate = RandomString().Left(3),
                Enabled = true
            };
            return model;
        }
    }
}
