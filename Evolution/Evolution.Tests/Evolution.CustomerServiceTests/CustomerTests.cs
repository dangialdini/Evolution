using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CustomerService;
using Evolution.MediaService;
using Evolution.CommonService;
using Evolution.Enumerations;

namespace Evolution.CustomerServiceTests {
    public partial class CustomerServiceTests {

        [TestMethod]
        public void FindCustomersListItemModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompanyAU();

            // Perform a global test which should bring back the first page of all customers
            var result = CustomerService.FindCustomersListItemModel(testCompany, "");
            Assert.IsTrue(result.Count > 0, $"Error: {result.Count} records were found when more than 100 were expected");

            // Now create a customer and try to search for it
            var testCustomer = GetTestCustomer(testCompany, testUser);

            result = CustomerService.FindCustomersListItemModel(testCompany, testCustomer.Name);
            Assert.IsTrue(result.Count == 1, $"Error: {result.Count} records were found when 1 was expected");
            Assert.IsTrue(result[0].Id == testCustomer.Id.ToString(), $"Error: Customer {result[0].Id} was returned when {testCustomer.Id} was expected");
        }

        [TestMethod]
        public void FindCustomersListModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompanyAU();

            // Perform a global test which should bring back the first page of all customers
            var result = CustomerService.FindCustomersListModel(testCompany.Id, 0, 1, PageSize, "");
            Assert.IsTrue(result.Items.Count > 0, $"Error: {result.Items.Count} records were found when more than 100 were expected");

            // Now create a customer and try to search for it
            var testCustomer = GetTestCustomer(testCompany, testUser);

            result = CustomerService.FindCustomersListModel(testCompany.Id, 0, 1, PageSize, testCustomer.Name);
            Assert.IsTrue(result.Items.Count == 1, $"Error: {result.Items.Count} records were found when 1 was expected");
            Assert.IsTrue(result.Items[0].Id == testCustomer.Id, $"Error: Customer {result.Items[0].Id} was returned when {testCustomer.Id} was expected");
        }

        [TestMethod]
        public void FindCustomerModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Now create a customer and try to retrieve it
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var result = CustomerService.FindCustomerModel(testCustomer.Id, testCompany, false);
            Assert.IsTrue(result != null, $"Error: 0 records were returned when 1 was expected");
            Assert.IsTrue(result.Id == testCustomer.Id, $"Error: Customer {result.Id} was returned when {testCustomer.Id} was expected");
            AreEqual(testCustomer, result);
        }

        [TestMethod]
        public void InsertOrUpdateCustomerTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a customer
            var testCustomer1 = GetTestCustomer(testCompany, testUser);
            var testCustomer2 = GetTestCustomer(testCompany, testUser);

            // Retrieve the customer and compare
            var test = db.FindCustomer(testCustomer1.Id);
            var testModel = CustomerService.MapToModel(test);
            AreEqual(testCustomer1, testModel);

            test = db.FindCustomer(testCustomer2.Id);
            testModel = CustomerService.MapToModel(test);
            AreEqual(testCustomer2, testModel);


            // Try to create a Customer with the same name
            var dupItem = CustomerService.MapToModel(testCustomer1);
            dupItem.Id = 0;
            var error = CustomerService.InsertOrUpdateCustomer(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate Customer returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = CustomerService.LockCustomer(testCustomer1);

            testCustomer1.Name = RandomString();
            error = CustomerService.InsertOrUpdateCustomer(testCustomer1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename the customer to an existing customer (should fail)
            lgs = CustomerService.LockCustomer(testCustomer1);

            testCustomer1.Name = testCustomer2.Name;
            error = CustomerService.InsertOrUpdateCustomer(testCustomer1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a customer to the same name as an existing Customer returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteCustomerTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a customer
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add an address to the customer
            createCustomerAddress(testCompany.Id, testCustomer.Id, testUser, AddressType.Billing);

            // Add a conflict sensitivity
            var conflictCustomer = GetTestCustomer(testCompany, testUser);
            createCustomerConflict(testCompany.Id, testCustomer.Id, testUser, conflictCustomer.Id);

            // Add a contact
            var contact = createCustomerContact(testCompany.Id, testCustomer.Id, testUser);

            // No need to create freight as it is part of the Customer record

            // Add some marketing
            createCustomerMarketing(testCompany.Id, testCustomer.Id, testUser, contact);

            // Add a note - second note with an attachment
            CreateCustomerNote(testCompany.Id, testCustomer.Id, testUser, false);
            CreateCustomerNote(testCompany.Id, testCustomer.Id, testUser, true);

            // Delete the customer
            // This test tests all the referencial integrity
            CustomerService.DeleteCustomer(testCompany.Id, testCustomer.Id);

            // Try to find the customer
            var test = db.FindCustomer(testCustomer.Id);
            Assert.IsTrue(test == null, "Error: The customer record was found when it should have been deleted");
        }

        [TestMethod]
        public void LockCustomerTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a customer
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Get the current lock
            string lockGuid = CustomerService.LockCustomer(testCustomer);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = CustomerService.InsertOrUpdateCustomer(testCustomer, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = CustomerService.InsertOrUpdateCustomer(testCustomer, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = CustomerService.LockCustomer(testCustomer);
            error = CustomerService.InsertOrUpdateCustomer(testCustomer, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void GetCustomerCountTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a customer
            int actual = CustomerService.GetCustomerCount(testCompany);
            int expected = 0;
            Assert.IsTrue(expected == actual, $"Error: {actual} was returned when {expected} was expected");

            var testCustomer = GetTestCustomer(testCompany, testUser);
            actual = CustomerService.GetCustomerCount(testCompany);
            expected = 1;
            Assert.IsTrue(expected == actual, $"Error: {actual} was returned when {expected} was expected");

            CustomerService.DeleteCustomer(testCompany.Id, testCustomer.Id);
            actual = CustomerService.GetCustomerCount(testCompany);
            expected = 0;
            Assert.IsTrue(expected == actual, $"Error: {actual} was returned when {expected} was expected");
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }

        [TestMethod]
        public void MapToEntityTest() {
            // Tested by all READ tests in this module
        }

        [TestMethod]
        public void ValidateCustomerModelTest() {
            // Tested by all tests which write a Customer record to the database or get a test Customer
        }
    }
}
