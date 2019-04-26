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
        public void SetCustomerDefaultsTest() {
            // Called by checkDefaults() in CreateCustomerTest()
        }

        [TestMethod]
        public void CreateCustomerTest() {
            // Tests to ensure that the 'customer defaults' feature works
            var companyList = CompanyService.FindCompaniesListModel(0, 1, Int32.MaxValue, "");

            // Test against the Australian market
            var testCompany = getCompany("Australia");
            checkDefaults(testCompany, testCompany.DefaultCountryID, "");

            // Test for no country
            checkDefaults(testCompany, null, "");

            // Test for a country with a postcode - should drop out to defaults for country
            checkDefaults(testCompany, testCompany.DefaultCountryID, "2155");

            // Test for NZ
            var country = LookupService.FindCountryModel("New Zealand");
            checkDefaults(testCompany, country.Id, "");


            // Test against the UK market
            testCompany = getCompany("United Kingdom");
            checkDefaults(testCompany, testCompany.DefaultCountryID, "");

            // Test for no country
            checkDefaults(testCompany, null, "");

            // Test for a country with a postcode - should drop out to defaults for country
            checkDefaults(testCompany, testCompany.DefaultCountryID, "RH16 1PP");

            // Test for France
            country = LookupService.FindCountryModel("France");
            checkDefaults(testCompany, country.Id, "");


            // Test against the US market
            testCompany = getCompany("United States of America");
            checkDefaults(testCompany, testCompany.DefaultCountryID, "");

            // Test for no country
            checkDefaults(testCompany, null, "");

            // Test for a country with a postcode - should drop out to defaults for country
            checkDefaults(testCompany, testCompany.DefaultCountryID, "90048");
        }

        [TestMethod]
        public void FindCustomerDefaultsListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var defaultList = CustomerService.FindCustomerDefaultsListModel(testCompany.Id, 0, 1, Int32.MaxValue, "")
                                             .Items
                                             .OrderBy(d => d.Id)
                                             .ToList();
            int expected = 0,
                actual = defaultList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Add defaults
            int numDefaults = 3;
            List<CustomerDefaultModel> defaults = new List<CustomerDefaultModel>();
            for (var i = 0; i < numDefaults; i++) {
                defaults.Add(createCustomerDefault(testCompany));
            }

            db.RefreshCustomerDefaults();   // Forces relinking of FK's in EF

            defaultList = CustomerService.FindCustomerDefaultsListModel(testCompany.Id, 0, 1, Int32.MaxValue, "")
                                         .Items
                                         .OrderBy(d => d.Id)
                                         .ToList();
            expected = numDefaults;
            actual = defaultList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Check that all the defaults match
            for (int i = 0; i < numDefaults; i++) {
                AreEqual(defaults[i], defaultList[i]);
            }

            // Delete defaults
            foreach (var defaultItem in defaults) {
                CustomerService.DeleteCustomerDefault(defaultItem.Id);

                defaultList = CustomerService.FindCustomerDefaultsListModel(testCompany.Id, 0, 1, Int32.MaxValue, "")
                                             .Items
                                             .OrderBy(d => d.Id)
                                             .ToList();
                expected--;
                actual = defaultList.Count();
                Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
            }
        }

        [TestMethod]
        public void FindCustomerDefaultModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var custDefault = createCustomerDefault(testCompany);

            db.RefreshCustomerDefaults();   // Forces relinking of FK's in EF

            // Find the default
            var testDefault = CustomerService.FindCustomerDefaultModel(custDefault.Id, testCompany, false);
            Assert.IsTrue(testDefault != null, $"Error: A NULL object was returned when a CustomerDefault object was expected");
            AreEqual(custDefault, testDefault);

            // Delete the default
            CustomerService.DeleteCustomerDefault(custDefault.Id);

            // Find the default again
            testDefault = CustomerService.FindCustomerDefaultModel(custDefault.Id, testCompany, false);
            Assert.IsTrue(testDefault == null, $"Error: A CustomerDefault was returned when NULL was expected. This indicates that the CustomerDefault failed to be deleted");
        }

        [TestMethod]
        public void InsertOrUpdateCustomerDefaultTest() {
            // Tested in methods above
        }

        [TestMethod]
        public void DeleteCustomerDefaultTest() {
            // Tested in methods above
        }

        [TestMethod]
        public void LockCustomerDefaultTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Add default and try to retrieve it
            var default1 = createCustomerDefault(testCompany);

            // Get the current Lock
            string lockGuid = CustomerService.LockCustomerDefault(default1);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = CustomerService.InsertOrUpdateCustomerDefault(default1, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = CustomerService.InsertOrUpdateCustomerDefault(default1, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = CustomerService.LockCustomerDefault(default1);
            error = CustomerService.InsertOrUpdateCustomerDefault(default1, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        CompanyListModel companyList = null; 
        CompanyModel getCompany(string countryName) {
            if(companyList == null) companyList = CompanyService.FindCompaniesListModel(0, 1, Int32.MaxValue, "");
            var country = LookupService.FindCountryModel(countryName);
            return companyList.Items.Where(cl => cl.DefaultCountryID == country.Id).FirstOrDefault();
        }

        void checkDefaults(CompanyModel company, int? countryId, string postCode) {
            var customer = CustomerService.CreateCustomer(company, countryId, postCode);
            Assert.IsTrue(customer.CurrencyId > 0, $"Error: CurrencyId was returned as {customer.CurrencyId} when a value >0 was expected");
            Assert.IsTrue(customer.PaymentTermId > 0, $"Error: PaymentTermId was returned as {customer.CurrencyId} when a value >0 was expected");
            Assert.IsTrue(customer.PriceLevelId >= 0, $"Error: PriceLevelId was returned as {customer.PriceLevelId} when a value >=0 was expected");
            Assert.IsTrue(customer.TaxCodeId > 0, $"Error: TaxCodeId was returned as {customer.TaxCodeId} when a value >0 was expected");
            Assert.IsTrue(customer.CreditLimit >= 0, $"Error: CreditLimit was returned as {customer.CreditLimit} when a value >=0 was expected");
            Assert.IsTrue(customer.VolumeDiscount >= 0, $"Error: VolumeDiscount was returned as {customer.VolumeDiscount} when a value >=0 was expected");
            Assert.IsTrue(customer.FreightCarrierId >= 0, $"Error: FreightCarrierId was returned as {customer.FreightCarrierId} when a value >=0 was expected");
            Assert.IsTrue(customer.CustomerTypeId > 0, $"Error: CustomerTypeId was returned as {customer.CustomerTypeId} when a value >0 was expected");
            Assert.IsTrue(customer.FreightRate >= 0, $"Error: FreightRate was returned as {customer.FreightRate} when a value >=0 was expected");
            Assert.IsTrue(customer.MinFreightPerOrder >= 0, $"Error: MinFreightPerOrder was returned as {customer.MinFreightPerOrder} when a value >=0 was expected");
            Assert.IsTrue(customer.MinFreightThreshold >= 0, $"Error: MinFreightThreshold was returned as {customer.MinFreightThreshold} when a value >=0 was expected");
            Assert.IsTrue(customer.FreightWhenBelowThreshold >= 0, $"Error: FreightWhenBelowThreshold was returned as {customer.FreightWhenBelowThreshold} when a value >=0 was expected");
        }

        CountryListModel countryList = null;
        CustomerDefaultModel createCustomerDefault(CompanyModel company) {
            if (countryList == null) countryList = LookupService.FindCountriesListModel();
            var rnd = RandomInt(0, countryList.Items.Count() - 1);

            var currency = LookupService.FindCurrencyModel(company.DefaultCurrencyID.Value, false);

            var model = new CustomerDefaultModel {
                CompanyId = company.Id,
                CountryId = countryList.Items[rnd].Id,
                CountryNameText = countryList.Items[rnd].CountryName,
                CurrencyId = currency.Id,
                CurrencyCodeText = currency.CurrencyCode
            };
            var error = CustomerService.InsertOrUpdateCustomerDefault(model);
            Assert.IsTrue(!error.IsError, error.Message);

            countryList.Items.RemoveAt(rnd);    // So we don't use the same country again

            return model;
        }
    }
}
