using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.ProductServiceTests {
    public partial class ProductServiceTests {
        [TestMethod]
        public void FindBrandListModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = ProductService.FindBrandListModel(0, 1, PageSize, "", false);
            var dbData = db.FindBrands();

            int expected = dbData.Count(),
                actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = ProductService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createBrand();
            var error = ProductService.InsertOrUpdateBrand(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = ProductService.FindBrandListModel(0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            ProductService.DeleteBrand(newItem.Id);

            model = ProductService.FindBrandListModel(0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindBrandListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = ProductService.FindBrandListItemModel();
            var dbData = db.FindBrands();

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");

                var test = ProductService.MapToModel(dbItem);
                AreEqual(item, test);
            }

            // Add another item a make sure it is found
            var newItem = createBrand();
            var error = ProductService.InsertOrUpdateBrand(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = ProductService.FindBrandListItemModel();
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            ProductService.DeleteBrand(newItem.Id);

            model = ProductService.FindBrandListItemModel();
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindBrandModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var model = createBrand();
            var error = ProductService.InsertOrUpdateBrand(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = ProductService.FindBrandModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateBrandTest() {
            // Tested in DeleteBrandTest, but additional tests here
            var testUser = GetTestUser();

            var testBrand1 = createBrand();
            var error = ProductService.InsertOrUpdateBrand(testBrand1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindBrand(testBrand1.Id);

            var testModel = ProductService.MapToModel(test);

            AreEqual(testBrand1, testModel);

            var testBrand2 = createBrand();
            error = ProductService.InsertOrUpdateBrand(testBrand2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindBrand(testBrand2.Id);

            testModel = ProductService.MapToModel(test);

            AreEqual(testBrand2, testModel);


            // Try to create a Brand with the same name
            var dupItem = ProductService.MapToModel(testBrand1);
            dupItem.Id = 0;
            error = ProductService.InsertOrUpdateBrand(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate Brand returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = ProductService.LockBrand(testBrand1);

            testBrand1.BrandName = RandomString();
            error = ProductService.InsertOrUpdateBrand(testBrand1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = ProductService.LockBrand(testBrand1);

            testBrand1.BrandName = testBrand2.BrandName;
            error = ProductService.InsertOrUpdateBrand(testBrand1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a Brand to the same name as an existing Brand returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteBrandTest() {
            // Get a test user
            var user = GetTestUser();

            // Create a price level
            BrandModel model = createBrand();

            var error = ProductService.InsertOrUpdateBrand(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindBrand(model.Id);
            BrandModel test = ProductService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            ProductService.DeleteBrand(model.Id);

            // And check that is was deleted
            result = db.FindBrand(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockBrandTest() {
            var testUser = GetTestUser();

            // Create a record
            var model = createBrand();

            var error = ProductService.InsertOrUpdateBrand(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = ProductService.LockBrand(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = ProductService.InsertOrUpdateBrand(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = ProductService.InsertOrUpdateBrand(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = ProductService.LockBrand(model);
            error = ProductService.InsertOrUpdateBrand(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        private BrandModel createBrand() {
            BrandModel model = new BrandModel {
                BrandName = RandomString(),
                Enabled = true
            };
            return model;
        }
    }
}
