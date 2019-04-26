using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.SupplierService;

namespace Evolution.SupplierServiceTests {
    public partial class SupplierServiceTests {
        [TestMethod]
        public void FindSupplierListItemModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create some suppliers
            List<SupplierModel> supplierList = new List<SupplierModel>();

            for(int i = 0; i < 10; i++) {
                supplierList.Add(GetTestSupplier(testUser, true));
            }

            // Now check them
            List<ListItemModel> model = SupplierService.FindSupplierListItemModel(testCompany);
            var dbData = db.FindSuppliers(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");

                var test = SupplierService.MapToModel(dbItem);
                AreEqual(item, test);
            }

            // Add another item a make sure it is found
            var newItem = GetTestSupplier(testUser, true);

            model = SupplierService.FindSupplierListItemModel(testCompany);
            var testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            SupplierService.DeleteSupplier(newItem.Id);

            model = SupplierService.FindSupplierListItemModel(testCompany);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindSuppliersListModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Perform a global test which should bring back the first page of all suppliers
            var result = SupplierService.FindSuppliersListModel(testCompany, 0, 1, PageSize, "");
            Assert.IsTrue(result.Items.Count > 0, $"Error: {result.Items.Count} records were found when more than 100 were expected");

            // Now create a supplier and try to search for it
            var testSupplier = GetTestSupplier(testUser);

            result = SupplierService.FindSuppliersListModel(testCompany, 0, 1, PageSize, testSupplier.Name);
            Assert.IsTrue(result.Items.Count == 1, $"Error: {result.Items.Count} records were found when 1 was expected");
            Assert.IsTrue(result.Items[0].Id == testSupplier.Id, $"Error: Supplier {result.Items[0].Id} was returned when {testSupplier.Id} was expected");
        }

        [TestMethod]
        public void FindSupplierModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();

            // Now create a supplier and try to retrieve it
            var testSupplier = GetTestSupplier(testUser);

            var result = SupplierService.FindSupplierModel(testSupplier.Id, false);
            Assert.IsTrue(result != null, $"Error: 0 records were returned when 1 was expected");
            Assert.IsTrue(result.Id == testSupplier.Id, $"Error: Supplier {result.Id} was returned when {testSupplier.Id} was expected");
            AreEqual(testSupplier, result);
        }

        [TestMethod]
        public void InsertOrUpdateSupplierTest() {
            // Get a test user and test company
            var testUser = GetTestUser();

            // Create some suppliers
            var testSupplier1 = GetTestSupplier(testUser);
            var testSupplier2 = GetTestSupplier(testUser);

            // Retrieve the supplier and compare
            var test = db.FindSupplier(testSupplier1.Id);

            SupplierModel testModel = SupplierService.MapToModel(test);

            AreEqual(testSupplier1, testModel);

            // Now try to create another Supplier with the same name
            var dupSupplier = SupplierService.MapToModel(testSupplier1);
            dupSupplier.Id = 0;
            var error = SupplierService.InsertOrUpdateSupplier(dupSupplier, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate supplier returned no error when it should have returned a 'duplicate' error");

            // Try to rename the supplier to a non-existing name (should work)
            string lgs = SupplierService.LockSupplier(testSupplier1);

            testSupplier1.Name = RandomString();
            error = SupplierService.InsertOrUpdateSupplier(testSupplier1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename the supplier to an existing supplier name (should fail)
            lgs = SupplierService.LockSupplier(testSupplier1);

            testSupplier1.Name = testSupplier2.Name;
            error = SupplierService.InsertOrUpdateSupplier(testSupplier1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a supplier to the same name as an existing supplier returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void ValidateSupplierModelTest() {
            // Tested by all tests which create a supplier
        }

        [TestMethod]
        public void DeleteSupplierTest() {
            // Get a test user and test company
            var testUser = GetTestUser();

            // Create a supplier
            var testSupplier = GetTestSupplier(testUser);

            // Delete the supplier
            // This test tests all the referencial integrity
            SupplierService.DeleteSupplier(testSupplier.Id);

            // Try to find the supplier
            var test = db.FindSupplier(testSupplier.Id);
            Assert.IsTrue(test == null, "Error: The supplier record was found when it should have been deleted");
        }

        [TestMethod]
        public void LockSupplierTest() {
            // Get a test user and test company
            var testUser = GetTestUser();

            // Create a supplier
            var model = GetTestSupplier(testUser);

            // Get the current Lock
            string lockGuid = SupplierService.LockSupplier(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = SupplierService.InsertOrUpdateSupplier(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = SupplierService.InsertOrUpdateSupplier(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = SupplierService.LockSupplier(model);
            error = SupplierService.InsertOrUpdateSupplier(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }
    }
}
