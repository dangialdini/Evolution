using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.PurchasingService;

namespace Evolution.PurchasingServiceTests {
    public partial class PurchasingServiceTests {
        [TestMethod]
        public void FindPurchaseOrderHeaderTempModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testPurchase = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            var pohtModel = PurchasingService.CopyPurchaseOrderToTemp(testCompany, testPurchase, testUser);

            List<string> excludes = new List<string>();
            excludes.Add("Id");
            excludes.Add("OriginalRowIdId");

            AreEqual(testPurchase, pohtModel, excludes);
        }

        [TestMethod]
        public void InsertOrUpdatePurchaseOrderHeaderTempTest() {
            // Tested by all tests which copy a PurchaseOrderHeader to temp
        }

        [TestMethod]
        public void LockPurchaseOrderHeaderTempTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testPurchase = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            var model = PurchasingService.CopyPurchaseOrderToTemp(testCompany, testPurchase, testUser);
            Assert.IsTrue(model != null, "Error: A NULL value was returned when an object was expected");

            // Get the current Lock
            string lockGuid = PurchasingService.LockPurchaseOrderHeaderTemp(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = PurchasingService.InsertOrUpdatePurchaseOrderHeaderTemp(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = PurchasingService.InsertOrUpdatePurchaseOrderHeaderTemp(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = PurchasingService.LockPurchaseOrderHeaderTemp(model);
            error = PurchasingService.InsertOrUpdatePurchaseOrderHeaderTemp(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void CopyTempToPurchaseOrderHeaderTest() {
            // Tested in FindPurchaseOrderHeaderTempModelTest() above
            // and all tests which check the PurchaseOrder[Header|Detail]Temp tables
        }
    }
}
