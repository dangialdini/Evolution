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
        public void FindSalesOrderHeaderTempModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testSale = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 10);

            var sohtModel = SalesService.CopySaleToTemp(testCompany, testSale, testUser, false);

            var excludes = new List<string>();
            excludes.Add("Id");
            excludes.Add("OriginalRowIdId");
            excludes.Add("SalesOrderDetails");

            AreEqual(testSale, sohtModel, excludes);
        }

        [TestMethod]
        public void CreateOrderSummaryTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Create a record
            var temp = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 10);
            var soht = SalesService.CopySaleToTemp(testCompany, temp, testUser, false);

            var summary = SalesService.CreateOrderSummary(soht);

            Assert.IsTrue(summary.SubTotal > 0, $"Error: SubTotal {summary.SubTotal} was returned when a value greater than 0 was expected");
            Assert.IsTrue(!string.IsNullOrEmpty(summary.TaxName), $"Error: TaxName {summary.TaxName} was returned when a no-empty string was expected");
            Assert.IsTrue(summary.TaxTotal > 0, $"Error: TaxTotal {summary.TaxTotal} was returned when a value greater than 0 was expected");
            Assert.IsTrue(summary.Total > 0, $"Error: Total {summary.Total} was returned when a value greater than 0 was expected");
            Assert.IsTrue(summary.TotalCbms > 0, $"Error: TotalCbms {summary.TotalCbms} was returned when a value greater than 0 was expected");
            Assert.IsTrue(!string.IsNullOrEmpty(summary.CurrencySymbol), $"Error: CurrencySymbol {summary.CurrencySymbol} was returned when a no-empty string was expected");
        }

        [TestMethod]
        public void InsertOrUpdateSalesOrderHeaderTempTest() {
            // Tested by all tests which copy a SalesOrderHeader to temp
        }

        [TestMethod]
        public void LockSalesOrderHeaderTempTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Create a record
            var temp = GetTestSalesOrderHeader(testCompany, testCustomer, testUser);
            var model = SalesService.CopySaleToTemp(testCompany, temp, testUser, false);

            // Get the current Lock
            string lockGuid = SalesService.LockSalesOrderHeaderTemp(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = SalesService.InsertOrUpdateSalesOrderHeaderTemp(model, otherUser, lockGuid, false);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = SalesService.InsertOrUpdateSalesOrderHeaderTemp(model, testUser, lockGuid, false);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = SalesService.LockSalesOrderHeaderTemp(model);
            error = SalesService.InsertOrUpdateSalesOrderHeaderTemp(model, testUser, lockGuid, false);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }
    }
}
