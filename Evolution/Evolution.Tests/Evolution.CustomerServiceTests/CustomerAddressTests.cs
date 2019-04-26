using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.LookupService;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.CustomerServiceTests {
    public partial class CustomerServiceTests {
        [TestMethod]
        public void FindCustomerAddressesListModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add addresses
            var addrs1 = createCustomerAddress(testCompany.Id, testCustomer.Id, testUser, AddressType.Billing);
            var addrs2 = createCustomerAddress(testCompany.Id, testCustomer.Id, testUser, AddressType.Billing);

            // Search for the addresses
            var result = CustomerService.FindCustomerAddressesListModel(testCustomer.Id, 0, 1, PageSize, "")
                                        .Items
                                        .OrderBy(c => c.Id)
                                        .ToList();
            int expectedResult = 2;
            Assert.IsTrue(result.Count == expectedResult, $"Error: {result.Count} records were found when {expectedResult} were expected");
            Assert.IsTrue(result[0].Id == addrs1.Id || result[0].Id == addrs2.Id, $"Error: Address {result[0].Id} was returned when {addrs1.Id} was expected");
            Assert.IsTrue(result[1].Id == addrs1.Id || result[1].Id == addrs2.Id, $"Error: Address {result[1].Id} was returned when {addrs2.Id} was expected");
        }

        [TestMethod]
        public void FindCustomerAddressModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add address and try to retrieve it
            var addrs1 = createCustomerAddress(testCompany.Id, testCustomer.Id, testUser, AddressType.Billing);

            //db.RefreshCustomerAddresses();

            var result = CustomerService.FindCustomerAddressModel(addrs1.Id, testCompany, testCustomer);
            Assert.IsTrue(result != null, $"Error: 0 records were returned when 1 was expected");
            Assert.IsTrue(result.Id == addrs1.Id, $"Error: Customer {result.Id} was returned when {addrs1.Id} was expected");
            AreEqual(addrs1, result);

            // Now delete it and try to retrieve it
            CustomerService.DeleteCustomerAddress(addrs1.Id);

            result = CustomerService.FindCustomerAddressModel(addrs1.Id, testCompany, testCustomer, false);
            Assert.IsTrue(result == null, $"Error: 1 record was returned when 0 were expected");
        }

        [TestMethod]
        public void FindCurrentCustomerAddressesTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add addresses to the customer
            var addrs1 = createCustomerAddress(testCompany.Id, testCustomer.Id, testUser, AddressType.Billing);
            var addrsList = CustomerService.FindCurrentCustomerAddresses(testCustomer, AddressType.Billing);
            // Should be one address found with null start and end
            int expected = 1,
                actual = addrsList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} was expected");

            // Set a date
            addrs1.DateStart = DateTimeOffset.Now.AddMonths(-1);
            var error = CustomerService.InsertOrUpdateCustomerAddress(addrs1, CustomerService.LockCustomerAddress(addrs1));
            Assert.IsTrue(!error.IsError, error.Message);

            addrsList = CustomerService.FindCurrentCustomerAddresses(testCustomer, AddressType.Billing);
            // Should be one address as none have dates on them
            expected = 1;
            actual = addrsList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} was expected");

            // Add another address
            var addrs2 = createCustomerAddress(testCompany.Id, testCustomer.Id, testUser, AddressType.Billing);
            addrsList = CustomerService.FindCurrentCustomerAddresses(testCustomer, AddressType.Billing);
            expected = 2;
            actual = addrsList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} was expected");

            // Now set dates
            addrs1.DateEnd = DateTimeOffset.Now.AddDays(-1);
            error = CustomerService.InsertOrUpdateCustomerAddress(addrs1, CustomerService.LockCustomerAddress(addrs1));
            Assert.IsTrue(!error.IsError, error.Message);

            addrs2.DateStart = addrs1.DateEnd.Value.AddSeconds(1);
            error = CustomerService.InsertOrUpdateCustomerAddress(addrs1, CustomerService.LockCustomerAddress(addrs1));
            Assert.IsTrue(!error.IsError, error.Message);

            addrsList = CustomerService.FindCurrentCustomerAddresses(testCustomer, AddressType.Billing);
            expected = 1;
            actual = addrsList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} was expected");

            // Add a different address type
            var addrs3 = createCustomerAddress(testCompany.Id, testCustomer.Id, testUser, AddressType.Delivery);
            addrsList = CustomerService.FindCurrentCustomerAddresses(testCustomer, AddressType.Billing);
            expected = 1;
            actual = addrsList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} was expected");
        }

        [TestMethod]
        public void InsertOrUpdateCustomerAddressTest() {
            // Tested by FindCustomerAddressModelTest
        }

        [TestMethod]
        public void DeleteCustomerAddressTest() {
            // Tested by FindCustomerAddressModelTest
        }

        [TestMethod]
        public void LockCustomerAddressTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add addresses and try to retrieve it
            var addrs1 = createCustomerAddress(testCompany.Id, testCustomer.Id, testUser, AddressType.Billing);

            // Get the current lock
            string lockGuid = CustomerService.LockCustomerAddress(addrs1);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = CustomerService.InsertOrUpdateCustomerAddress(addrs1, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = CustomerService.InsertOrUpdateCustomerAddress(addrs1, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = CustomerService.LockCustomerAddress(addrs1);
            error = CustomerService.InsertOrUpdateCustomerAddress(addrs1, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        CustomerAddressModel createCustomerAddress(int companyId, int customerId, UserModel user, AddressType addressType) {
            var lov = db.FindLOV(LOVName.AddressType);
            var addrsType = lov.LOVItems
                               .Where(lovi => lovi.ItemValue1.ToString() == ((int)addressType).ToString())
                               .FirstOrDefault();

            if (countryList == null) countryList = LookupService.FindCountriesListModel();
            var rnd = RandomInt(0, countryList.Items.Count() - 1);

            CustomerAddressModel model = new CustomerAddressModel {
                CompanyId = companyId,
                CustomerId = customerId,
                AddressTypeId = addrsType.Id,
                AddressType = (AddressType)Convert.ToInt32(addrsType.ItemValue1),
                AddressTypeText = addrsType.ItemText,
                Street = RandomString(),
                City = RandomString(),
                State = RandomString().Left(20),
                CountryId = countryList.Items[rnd].Id,
                CountryName = countryList.Items[rnd].CountryName,
                Postcode = RandomString().Left(10),
                DateStart = DateTimeOffset.Now,
                DateEnd = DateTimeOffset.Now.AddDays(1)
            };
            var error = CustomerService.InsertOrUpdateCustomerAddress(model, "");

            Assert.IsFalse(error.IsError, error.Message);

            countryList.Items.RemoveAt(rnd);    // So we don't use the same country again

            return model;
        }

        [TestMethod]
        public void ValidateAddressModelTest() {
            // Tested by all tests which write a CustomerAddress record to the database
        }
    }
}
