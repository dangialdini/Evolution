using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.DAL;
using System.Collections.Generic;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests : BaseTest {

        [TestMethod]
        public void FindCreditClaimHeadersTest() {
            // Tested in InsertOrUpdateCreditClaimHeaderTest
        }

        [TestMethod]
        public void InsertOrUpdateCreditClaimHeaderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Check db before test
            SalesService.CleanCreditClaimTables();
            CreditClaimHeaderListModel cch = SalesService.FindCreditClaimHeaders();

            int expected = 0;
            int actual = cch.Items.Count();
            Assert.IsTrue(expected == actual, $"Error: {actual} number of items were found when {expected} were expected");

            // Create and test
            var model = getCreditClaimHeader();
            cch = SalesService.FindCreditClaimHeaders();

            expected = 1;
            actual = cch.Items.Count();
            Assert.IsTrue(expected == actual, $"Error: {actual} number of items were found when {expected} were expected");
            AreEqual(model, cch.Items);

            // Delete and check after test
            SalesService.CleanCreditClaimTables();
            cch = SalesService.FindCreditClaimHeaders();

            expected = 0;
            actual = cch.Items.Count();
            Assert.IsTrue(expected == actual, $"Error: {actual} number of items were found when {expected} were expected");
        }

        [TestMethod]
        public void CleanCreditClaimTablesTest() {
            // Tested in InsertOrUpdateCreditClaimHeaderTest
        }
    }
}
