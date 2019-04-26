using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;

namespace Evolution.CustomerServiceTests {
    public partial class CustomerServiceTests {
        [TestMethod]
        public void FindCustomerMarketingListModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var cont1 = createCustomerContact(testCompany.Id, testCustomer.Id, testUser);

            // Add marketing
            var mktg1 = createCustomerMarketing(testCompany.Id, testCustomer.Id, testUser, cont1);
            var mktg2 = createCustomerMarketing(testCompany.Id, testCustomer.Id, testUser, cont1);

            // Search for the marketing subscriptions
            var result = CustomerService.FindCustomerMarketingListModel(testCustomer.Id, 0, 1, PageSize)
                                        .Items
                                        .OrderBy(m => m.Id)
                                        .ToList();
            int expectedResult = 2;
            Assert.IsTrue(result.Count == expectedResult, $"Error: {result.Count} records were found when {expectedResult} were expected");
            Assert.IsTrue(result[0].Id == mktg1.Id, $"Error: Conflict {result[0].Id} was returned when {mktg1.Id} was expected");
            Assert.IsTrue(result[1].Id == mktg2.Id, $"Error: Conflict {result[1].Id} was returned when {mktg2.Id} was expected");
        }

        [TestMethod]
        public void FindCustomerMarketingModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var cont1 = createCustomerContact(testCompany.Id, testCustomer.Id, testUser);

            // Add marketing
            var mktg1 = createCustomerMarketing(testCompany.Id, testCustomer.Id, testUser, cont1);

            var result = CustomerService.FindCustomerMarketingModel(mktg1.Id, testCompany, testCustomer);
            Assert.IsTrue(result != null, $"Error: 0 records were returned when 1 was expected");
            Assert.IsTrue(result.Id == mktg1.Id, $"Error: Customer {result.Id} was returned when {mktg1.Id} was expected");
            AreEqual(mktg1, result);

            // Now delete it and try to retrieve it
            CustomerService.DeleteCustomerMarketing(mktg1.Id);

            result = CustomerService.FindCustomerMarketingModel(mktg1.Id, testCompany, testCustomer, false);
            Assert.IsTrue(result == null, $"Error: 1 record was returned when 0 were expected");
        }

        [TestMethod]
        public void InsertOrUpdateCustomerMarketingTest() {
            // Tested by FindCustomerMarketingModelTest
        }

        [TestMethod]
        public void DeleteCustomerMarketingTest() {
            // Tested by FindCustomerMarketingModelTest
        }

        [TestMethod]
        public void LockCustomerMarketingTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var cont1 = createCustomerContact(testCompany.Id, testCustomer.Id, testUser);

            // Add marketing
            var mktg1 = createCustomerMarketing(testCompany.Id, testCustomer.Id, testUser, cont1);

            // Get the current lock
            string lockGuid = CustomerService.LockCustomerMarketing(mktg1);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = CustomerService.InsertOrUpdateCustomerMarketing(mktg1, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = CustomerService.InsertOrUpdateCustomerMarketing(mktg1, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = CustomerService.LockCustomerMarketing(mktg1);
            error = CustomerService.InsertOrUpdateCustomerMarketing(mktg1, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Delete the record and check that the lock was deleted
            int tempId = mktg1.Id;
            CustomerService.DeleteCustomerMarketing(tempId);
            var lockRecord = db.FindLock(typeof(MarketingGroupSubscription).ToString(), tempId);
            // The following is used because Assert.IsNull tried to evaluate the error message when lockrecord was NULL - its shouldn't!
            if (lockRecord != null) Assert.Fail($"Error: Lock record {lockRecord.Id} was returned when none were expected");
        }

        CustomerMarketingModel createCustomerMarketing(int companyId, int customerId, UserModel user, CustomerContactModel contact) {
            var group = LookupService.FindMarketingGroupsListModel(companyId, 0, 1, 100, "").Items.First();

            CustomerMarketingModel model = new CustomerMarketingModel {
                CompanyId = companyId,
                CustomerId = customerId,
                CustomerContactId = contact.Id,
                ContactName = (contact.ContactFirstname + " " + contact.ContactSurname).Trim(),
                MarketingGroupId = group.Id,
                GroupName = group.MarketingGroupName
            };

            var error = CustomerService.InsertOrUpdateCustomerMarketing(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            return model;
        }
    }
}
