using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.ProductService;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.ProductServiceTests {
    public partial class ProductServiceTests {
        [TestMethod]
        public void FindProductsForBrandCategoryListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a new BrandCatgeory
            var brandCategory = createBrandCategory(testCompany);
            var error = ProductService.InsertOrUpdateBrandCategory(brandCategory, null, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var model = ProductService.FindProductsForBrandCategoryListItemModel(brandCategory.Id, 0, 0, 1, PageSize, "");

            int expected = 0,
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Add some brands to the BrandCategory
            int rnd,
                numBrands = 16,
                numProducts = 0;
            var brands = ProductService.FindBrandListModel(0, 1, PageSize, "");
            for (int i = 0; i < numBrands; i++) {
                rnd = RandomInt(0, brands.Items.Count());

                var tempBrand = brands.Items[rnd];

                ProductService.AddBrandToBrandCategory(testCompany, tempBrand, brandCategory);
                numProducts += ProductService.FindProductsForBrandModel(tempBrand.Id, "", Enumerations.SortOrder.Asc, true).Count();

                brands.Items.RemoveAt(rnd);     // So we don't add the same brand again
            }

            model = ProductService.FindProductsForBrandCategoryListItemModel(brandCategory.Id, 0, 0, 1, 100000, "");

            expected = numProducts;
            actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Remove some brands from the BrandCategory
            var categoryBrands = db.FindBrandCategory(brandCategory.Id)
                                   .BrandBrandCategories
                                   .ToList();
            for (int i = 0; i < categoryBrands.Count(); i++) {
                var tempCatBrand = categoryBrands[i];

                var bm = ProductService.FindBrandModel(tempCatBrand.BrandId);
                ProductService.DeleteBrandFromBrandCategory(bm, brandCategory);

                expected -= ProductService.FindProductsForBrandModel(tempCatBrand.BrandId, "", Enumerations.SortOrder.Asc, true).Count();

                model = ProductService.FindProductsForBrandCategoryListItemModel(brandCategory.Id, 0, 0, 1, 100000, "");
                actual = model.Count();
                Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");
            }
        }

        [TestMethod]
        public void FindBrandCategoriesModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = ProductService.FindBrandCategoriesModel(testCompany);
            var dbData = db.FindBrandCategories(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = ProductService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createBrandCategory(testCompany);
            var error = ProductService.InsertOrUpdateBrandCategory(newItem, null, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = ProductService.FindBrandCategoriesModel(testCompany);
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            ProductService.DeleteBrandCategory(newItem.Id);

            model = ProductService.FindBrandCategoriesModel(testCompany);
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindBrandCategoryListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = ProductService.FindBrandCategoryListItemModel(testCompany);
            var dbData = db.FindBrandCategories(testCompany.Id);

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
            var newItem = createBrandCategory(testCompany);
            var error = ProductService.InsertOrUpdateBrandCategory(newItem, null, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = ProductService.FindBrandCategoryListItemModel(testCompany);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            ProductService.DeleteBrandCategory(newItem.Id);

            model = ProductService.FindBrandCategoryListItemModel(testCompany);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindBrandCategoriesListModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = ProductService.FindBrandCategoriesListModel(testCompany.Id, 0, 1, PageSize, "");
            var dbData = db.FindBrandCategories(testCompany.Id);

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
            var newItem = createBrandCategory(testCompany);
            var error = ProductService.InsertOrUpdateBrandCategory(newItem, null, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = ProductService.FindBrandCategoriesListModel(testCompany.Id, 0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            ProductService.DeleteBrandCategory(newItem.Id);

            model = ProductService.FindBrandCategoriesListModel(testCompany.Id, 0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindBrandCategoryModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var model = createBrandCategory(testCompany);
            var error = ProductService.InsertOrUpdateBrandCategory(model, null, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = ProductService.FindBrandCategoryModel(model.Id, testCompany, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void FindProductBrandCategoryModelTest() {
            // Tests finding the Brand Category of a product
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // 1 Create a brand category
            var bc = createBrandCategory(testCompany);
            var error = ProductService.InsertOrUpdateBrandCategory(bc, null, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // 2 Check it has no brands
            var bbcList = ProductService.FindBrandBrandCategoriesListItemModel(bc);
            int expected = 0,
                actual = bbcList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} categories were found when {expected} were expected");

            // 3 Add a brand
            var brand = createBrand();
            error = ProductService.InsertOrUpdateBrand(brand, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // 4 Add the brand to the category
            var bbcModel = ProductService.AddBrandToBrandCategory(testCompany, brand, bc);
            Assert.IsTrue(bbcModel != null, "Error: A NULL value was returned when a BrandBrandCategoryModel object was expected");

            // 5 Add some products to the brand and choose one.
            int numProds = 10;
            int rnd = RandomInt(0, numProds - 1);
            ProductModel testProd = null;

            for (int i = 0; i < numProds; i++) {
                var prod = createProduct(testCompany, testUser, brand);
                error = ProductService.InsertOrUpdateProduct(prod, testUser, "");
                Assert.IsTrue(!error.IsError, error.Message);
                if (i == rnd) testProd = prod;
            }

            // 6 Get its brand category
            var checkBc = ProductService.FindProductBrandCategoryModel(testCompany.Id, testProd.Id);
            Assert.IsTrue(checkBc != null, "Error: A NULL value was returned when a BrandCategoryModel object was expected");

            // 7 Compare the category with (1)
            bc = ProductService.FindBrandCategoryModel(bc.Id, testCompany, false);
            AreEqual(bc, checkBc);
        }

        [TestMethod]
        public void InsertOrUpdateBrandCategoryTest() {
            // Tested in DeleteBrandCategoryTest, but additional tests here
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var testBrandCategory1 = createBrandCategory(testCompany);
            var error = ProductService.InsertOrUpdateBrandCategory(testBrandCategory1, new List<int>(), testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindBrandCategory(testBrandCategory1.Id);

            var testModel = ProductService.MapToModel(test);

            AreEqual(testBrandCategory1, testModel);

            var testBrandCategory2 = createBrandCategory(testCompany);
            error = ProductService.InsertOrUpdateBrandCategory(testBrandCategory2, new List<int>(), testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindBrandCategory(testBrandCategory2.Id);

            testModel = ProductService.MapToModel(test);

            AreEqual(testBrandCategory2, testModel);


            // Try to create a BrandCategory with the same name
            var dupItem = ProductService.MapToModel(testBrandCategory1);
            dupItem.Id = 0;
            error = ProductService.InsertOrUpdateBrandCategory(dupItem, new List<int>(), testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate BrandCategory returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = ProductService.LockBrandCategory(testBrandCategory1);

            testBrandCategory1.CategoryName = RandomString();
            error = ProductService.InsertOrUpdateBrandCategory(testBrandCategory1, new List<int>(), testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = ProductService.LockBrandCategory(testBrandCategory1);

            testBrandCategory1.CategoryName = testBrandCategory2.CategoryName;
            error = ProductService.InsertOrUpdateBrandCategory(testBrandCategory1, new List<int>(), testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a BrandCategory to the same name as an existing BrandCategory returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteBrandCategoryTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a price level
            BrandCategoryModel model = createBrandCategory(testCompany);

            var error = ProductService.InsertOrUpdateBrandCategory(model, null,  user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindBrandCategory(model.Id);
            BrandCategoryModel test = ProductService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            ProductService.DeleteBrandCategory(model.Id);

            // And check that is was deleted
            result = db.FindBrandCategory(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockBrandCategoryTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createBrandCategory(testCompany);

            var error = ProductService.InsertOrUpdateBrandCategory(model, null, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = ProductService.LockBrandCategory(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = ProductService.InsertOrUpdateBrandCategory(model, null, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = ProductService.InsertOrUpdateBrandCategory(model, null, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = ProductService.LockBrandCategory(model);
            error = ProductService.InsertOrUpdateBrandCategory(model, null, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void AddBrandToBrandCategoryTest() {
            // Tested in FindBrandBrandCategoriesListItemModelTest
        }

        [TestMethod]
        public void DeleteBrandFromBrandCategoryTest() {
            // Tested in FindBrandBrandCategoriesListItemModelTest
        }

        [TestMethod]
        public void CopyBrandCategoriesTest() {
            // Tested by all tests which create a test company
        }

        private BrandCategoryModel createBrandCategory(CompanyModel company) {
            return new BrandCategoryModel {
                CompanyId = company.Id,
                CategoryName = RandomString(),
                Enabled = true
            };
        }
    }
}
