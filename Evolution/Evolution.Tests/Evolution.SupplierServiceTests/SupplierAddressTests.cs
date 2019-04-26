using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.SupplierService;

namespace Evolution.SupplierServiceTests {
    public partial class SupplierServiceTests {
        [TestMethod]
        public void FindSupplierAddressModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();

            // Now create a supplier with an address and try to retrieve it
            var testSupplier = GetTestSupplier(testUser);

            var result = SupplierService.FindSupplierAddressModel(testSupplier.Id, false);

            var supplierAddrs = db.FindSupplierAddress(testSupplier.Id);
            Assert.IsTrue(supplierAddrs != null, $"Error: 0 records were returned when 1 was expected");

            Assert.IsTrue(result != null, $"Error: 0 records were returned when 1 was expected");
            Assert.IsTrue(result.Id == supplierAddrs.Id, $"Error: Supplier {result.Id} was returned when {supplierAddrs.Id} was expected");
            AreEqual(supplierAddrs, result);
        }

        [TestMethod]
        public void InsertOrUpdateSupplierAddressTest() {
            // Tested in FindSupplierAddressModelTest above (GetTestSupplier calls it)
        }

        [TestMethod]
        public void ValidateAddressModelTest() {
            // Tested by all tests which create a supplier
        }
    }
}
