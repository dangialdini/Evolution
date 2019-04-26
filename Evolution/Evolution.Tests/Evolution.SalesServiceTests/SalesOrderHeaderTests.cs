using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.SalesService;
using Evolution.Enumerations;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests {
        [TestMethod]
        public void FindSalesOrderHeadersListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Create a random number of sales
            List<SalesOrderHeaderModel> sohList = new List<SalesOrderHeaderModel>();
            int numSohs = RandomInt(5, 25);
            for (int i = 0; i < numSohs; i++) sohList.Add(GetTestSalesOrderHeader(testCompany, testCustomer, testUser));

            // Check that they are found
            var result = SalesService.FindSalesOrderHeadersListModel(testCompany.Id, 0, 1, PageSize, "", 0, 0, 0, 0, (int)SalesOrderHeaderStatus.Quote, 0);
            int actual = result.Items.Count;
            int expected = numSohs;
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Now delete them
            foreach (var poh in sohList) SalesService.DeleteSalesOrderHeader(poh);

            // Now check that they have disappeared
            result = SalesService.FindSalesOrderHeadersListModel(testCompany.Id, 0, 1, PageSize, "", 0, 0, 0, 0, (int)SalesOrderHeaderStatus.Quote, 0);
            actual = result.Items.Count;
            expected = 0;
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
        }

        [TestMethod]
        public void FindSalesOrderHeaderSummaryListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            int rand = RandomInt(1, 20);
            for (int i = 0; i < rand; i++) {
                var model = GetTestSalesOrderHeader(testCompany, testCustomer, testUser);
                Assert.IsTrue(model != null, "Error: A NULL value was returned when an object was expected");
            }

            var summary = SalesService.FindSalesOrderHeaderSummaryListModel(testCompany, testUser, 0, 1, Int32.MaxValue, "");
            int expected = rand,
                actual = summary.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
        }

        [TestMethod]
        public void FindSalesOrderHeaderModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var model = GetTestSalesOrderHeader(testCompany, testCustomer, testUser);

            var test = SalesService.FindSalesOrderHeaderModel(model.Id, testCompany, false);
            var excludes = new List<string>();
            excludes.Add("SalesOrderDetails");
            excludes.Add("OrderNumberUrl");     // Because it isn't known at test prep

            AreEqual(model, test, excludes);
        }

        [TestMethod]
        public void FindSalesOrderHeaderModelFromTempIdTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var soh = GetTestSalesOrderHeader(testCompany, testCustomer, testUser);
            var soht = SalesService.CopySaleToTemp(testCompany, soh, testUser, false);

            var checkSoh = SalesService.FindSalesOrderHeaderModelFromTempId(soht.Id, testCompany, false);
            Assert.IsTrue(checkSoh != null, "Error: A NULL value was returned when an object was expected");

            var excludes = new List<string>();
            excludes.Add("SalesOrderDetails");
            excludes.Add("OrderNumberUrl");     // Because it isn't known at test prep

            AreEqual(soh, checkSoh, excludes);
        }

        [TestMethod]
        public void FindCreditCardSalesTest() {
            // Finds all sales conducted against a specific credit card
            var testUser = GetTestUser();
            var testCompany = GetTestCompanyAU();

            var dbData = db.FindSalesOrderHeaders(testCompany.Id)
                           .Where(soh => soh.CreditCardId != null)
                           .ToList();
            int rand = RandomInt(1, dbData.Count());

            var actual = SalesService.FindCreditCardSales(testCompany, dbData[rand].CreditCardId.Value).Items.Count();
            var expected = dbData.Where(dd => dd.CreditCardId == dbData[rand].CreditCardId.Value).Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned where {expected} were expected");
        }

        [TestMethod]
        public void InsertOrUpdateSalesOrderHeaderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Check that getting a sale and updating it without changes works
            var testSoh = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 10);

            var error = SalesService.InsertOrUpdateSalesOrderHeader(testSoh, testUser, SalesService.LockSalesOrderHeader(testSoh));
            Assert.IsTrue(!error.IsError, error.Message);
        }

        [TestMethod]
        public void DeleteSalesOrderHeaderTest() {
            // Get a test user
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Create a purchase
            var model = GetTestSalesOrderHeader(testCompany, testCustomer, testUser);

            // Check that it was written
            var result = db.FindSalesOrderHeader(model.Id);
            SalesOrderHeaderModel test = SalesService.MapToModel(result);

            var excludes = new List<string>();
            excludes.Add("SalesOrderDetails");  // SalesOrderDetail is a list of objects
            excludes.Add("OrderNumberUrl");     // Because it isn't known at test prep
            AreEqual(model, test, excludes);

            // Now delete it
            SalesService.DeleteSalesOrderHeader(model.Id);

            // And check that is was deleted
            result = db.FindSalesOrderHeader(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockSalesOrderHeaderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Create a record
            var model = GetTestSalesOrderHeader(testCompany, testCustomer, testUser);

            // Get the current Lock
            string lockGuid = SalesService.LockSalesOrderHeader(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = SalesService.InsertOrUpdateSalesOrderHeader(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = SalesService.InsertOrUpdateSalesOrderHeader(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = SalesService.LockSalesOrderHeader(model);
            error = SalesService.InsertOrUpdateSalesOrderHeader(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }
    }
}
