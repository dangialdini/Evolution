using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Extensions;

namespace Evolution.CustomerServiceTests {
    public partial class CustomerServiceTests {
        [TestMethod]
        public void FindCustomerContactsListItemModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add contacts
            var cont1 = createCustomerContact(testCompany.Id, testCustomer.Id, testUser);
            var cont2 = createCustomerContact(testCompany.Id, testCustomer.Id, testUser);

            // Search for the contacts
            var result = CustomerService.FindCustomerContactsListItemModel(testCustomer.Id)
                                        .OrderBy(c => c.Id)
                                        .ToList();
            int expectedResult = 3;     // First contact is created as primary when cust is created
            Assert.IsTrue(result.Count == expectedResult, $"Error: {result.Count} records were found when {expectedResult} were expected");
            Assert.IsTrue(result[1].Id == cont1.Id.ToString(), $"Error: Conflict {result[0].Id} was returned when {cont1.Id} was expected");
            Assert.IsTrue(result[2].Id == cont2.Id.ToString(), $"Error: Conflict {result[1].Id} was returned when {cont2.Id} was expected");
        }

        [TestMethod]
        public void FindCustomerContactsListModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add contacts
            var cont1 = createCustomerContact(testCompany.Id, testCustomer.Id, testUser);
            var cont2 = createCustomerContact(testCompany.Id, testCustomer.Id, testUser);

            // Search for the contacts
            var result = CustomerService.FindCustomerContactsListModel(testCustomer.Id, 0, 1, PageSize, "")
                                        .Items
                                        .OrderBy(c => c.Id)
                                        .ToList();
            int expectedResult = 3;     // First contact is created as primary when cust is created
            Assert.IsTrue(result.Count == expectedResult, $"Error: {result.Count} records were found when {expectedResult} were expected");
            Assert.IsTrue(result[1].Id == cont1.Id, $"Error: Conflict {result[0].Id} was returned when {cont1.Id} was expected");
            Assert.IsTrue(result[2].Id == cont2.Id, $"Error: Conflict {result[1].Id} was returned when {cont2.Id} was expected");
        }

        [TestMethod]
        public void FindCustomerContactModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add contact and try to retrieve it
            var cont1 = createCustomerContact(testCompany.Id, testCustomer.Id, testUser);

            var result = CustomerService.FindCustomerContactModel(cont1.Id, testCompany, testCustomer);
            Assert.IsTrue(result != null, $"Error: 0 records were returned when 1 was expected");
            Assert.IsTrue(result.Id == cont1.Id, $"Error: Customer {result.Id} was returned when {cont1.Id} was expected");
            AreEqual(cont1, result);

            // Now delete it and try to retrieve it
            CustomerService.DeleteCustomerContact(cont1.Id);

            result = CustomerService.FindCustomerContactModel(cont1.Id, testCompany, testCustomer, false);
            Assert.IsTrue(result == null, $"Error: 1 record was returned when 0 were expected");
        }

        [TestMethod]
        public void FindPrimaryCustomerContactsModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add contacts
            var contacts = new List<CustomerContactModel>();
            for (int i = 0; i < 10; i++) {
                contacts.Add(createCustomerContact(testCompany.Id, testCustomer.Id, testUser));
            }

            // Find the primary contact
            var primaryContacts = CustomerService.FindPrimaryCustomerContactsModel(testCustomer);
            int expected = 1,                           // Created with the customer
                actual = primaryContacts.Count();       // Should be one
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} were expected");

            // Set one of the contacts to primary
            int rand = RandomInt(0, 9);
            contacts[rand].PrimaryContact = true;
            CustomerService.InsertOrUpdateCustomerContact(contacts[rand], testUser, CustomerService.LockCustomerContact(contacts[rand]));

            primaryContacts = CustomerService.FindPrimaryCustomerContactsModel(testCustomer);
            expected = 2;
            actual = primaryContacts.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} were expected");

            if (rand + 1 >= contacts.Count) {
                rand--;
            } else {
                rand++;
            }
            contacts[rand].PrimaryContact = true;
            CustomerService.InsertOrUpdateCustomerContact(contacts[rand], testUser, CustomerService.LockCustomerContact(contacts[rand]));

            primaryContacts = CustomerService.FindPrimaryCustomerContactsModel(testCustomer);
            expected = 3;
            actual = primaryContacts.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} were expected");
        }

        [TestMethod]
        public void FindCustomerRecipientsTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            int expected = 1;   // Include the primary contact created with the customer

            // Add contacts
            var contacts = new List<CustomerContactModel>();
            var rand = RandomInt(2, 10);
            for (int i = 0; i < rand; i++) {
                contacts.Add(createCustomerContact(testCompany.Id, testCustomer.Id, testUser));
                expected++;
            }

            // Add account manager
            var brandCategory = ProductService.FindBrandCategoriesModel(testCompany).First();

            var acctMgrs = new List<BrandCategorySalesPersonModel>();
            rand = RandomInt(2, 10);
            for (int i = 0; i < rand; i++) {
                var accMgr = createBrandCategorySalesPerson(testCompany, brandCategory, testCustomer, 
                                                            GetTestUser(), 
                                                            getSalesPersonType());
                var error = CustomerService.InsertOrUpdateBrandCategorySalesPerson(accMgr, testUser, "");
                Assert.IsTrue(!error.IsError, error.Message);
                acctMgrs.Add(accMgr);
                expected++;
            }

            // Add account admins
            var acctAdmins = new List<BrandCategorySalesPersonModel>();
            rand = RandomInt(2, 10);
            for (int i = 0; i < rand; i++) {
                var accAdmin = createBrandCategorySalesPerson(testCompany, brandCategory, testCustomer,
                                                              GetTestUser(), 
                                                              getAdminPersonType());
                var error = CustomerService.InsertOrUpdateBrandCategorySalesPerson(accAdmin, testUser, "");
                Assert.IsTrue(!error.IsError, error.Message);
                acctAdmins.Add(accAdmin);
                expected++;
            }

            // Create a sale in the temp tables
            var soht = GetTestSalesOrderHeaderTemp(testCompany, testCustomer, testUser, 10);

            // Get the recipients
            var recipients = CustomerService.FindCustomerRecipients(soht, testCompany, testUser);
            expected++;     // Include Current user
            int actual = recipients.Count();
            Assert.IsTrue(expected == actual, $"Error: {actual} recipients were returned when {expected} were expected");
        }

        [TestMethod]
        public void InsertOrUpdateCustomerContactTest() {
            // Tested by FindCustomerContactModelTest
        }

        [TestMethod]
        public void ValidateContactModelTest() {
            // Tested by all tests which call InsertOrUpdateCustomerContactTest
        }

        [TestMethod]
        public void DeleteCustomerContactTest() {
            // Tested by FindCustomerContactModelTest
        }

        [TestMethod]
        public void LockCustomerContactTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var cust1 = GetTestCustomer(testCompany, testUser);

            // Add contact
            var cont1 = createCustomerContact(testCompany.Id, testCustomer.Id, testUser);

            // Get the current Lock
            string lockGuid = CustomerService.LockCustomerContact(cont1);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = CustomerService.InsertOrUpdateCustomerContact(cont1, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = CustomerService.InsertOrUpdateCustomerContact(cont1, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = CustomerService.LockCustomerContact(cont1);
            error = CustomerService.InsertOrUpdateCustomerContact(cont1, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Delete the record and check that the lock was deleted
            int tempId = cont1.Id;
            CustomerService.DeleteCustomerContact(tempId);
            var lockRecord = db.FindLock(typeof(CustomerContact).ToString(), tempId);
            // The following is used because Assert.IsNull tried to evaluate the error message when lockrecord was NULL - its shouldn't!
            if (lockRecord != null) Assert.Fail($"Error: Lock record {lockRecord.Id} was returned when none were expected");
        }

        CustomerContactModel createCustomerContact(int companyId, int customerId, UserModel user) {
            CustomerContactModel model = new CustomerContactModel {
                CompanyId = companyId,
                CustomerId = customerId,
                ContactFirstname = RandomString().Left(50),
                ContactSurname = RandomString(),
                ContactEmail = RandomEMail(),
                Enabled = true
            };

            var error = CustomerService.InsertOrUpdateCustomerContact(model, user, "");

            Assert.IsFalse(error.IsError, error.Message);

            return model;
        }
    }
}
