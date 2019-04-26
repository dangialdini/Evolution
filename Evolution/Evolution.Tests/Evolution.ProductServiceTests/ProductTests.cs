using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.ProductServiceTests {
    public partial class ProductServiceTests {
        [TestMethod]
        public void FindProductsForBrandModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var testBrand = createBrandWithProducts(testCompany, user);
            var model = ProductService.FindProductsForBrandModel(testBrand.Id);
            var dbData = db.FindProductsForBrand(testBrand.Id);

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
            var newItem = createProduct(testCompany, user, testBrand);
            var error = ProductService.InsertOrUpdateProduct(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = ProductService.FindProductsForBrandModel(testBrand.Id);
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            ProductService.DeleteProduct(newItem.Id);

            model = ProductService.FindProductsForBrandModel(testBrand.Id);
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }
        
        [TestMethod]
        public void FindProductListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var testBrand = createBrandWithProducts(testCompany, user);

            int batchSize = 0;
            var model = ProductService.FindProductListItemModel("", batchSize);
            var dbData = db.FindProducts();
            if(batchSize > 0) dbData = dbData.Take(batchSize);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = ProductService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item and make sure it is found
            var newItem = createProduct(testCompany, user, testBrand);
            var error = ProductService.InsertOrUpdateProduct(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = ProductService.FindProductListItemModel("", batchSize);
            var testItem = model.Where(p => p.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            ProductService.DeleteProduct(newItem.Id);

            model = ProductService.FindProductListItemModel("", batchSize);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindProductsListModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var testBrand = createBrandWithProducts(testCompany, user);
            var model = ProductService.FindProductsListModel(testBrand.Id, 0, 0, 1, PageSize, "").Items;
            var dbData = db.FindProductsForBrand(testBrand.Id);

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
            var newItem = createProduct(testCompany, user, testBrand);
            var error = ProductService.InsertOrUpdateProduct(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = ProductService.FindProductsListModel(testBrand.Id, 0, 0, 1, PageSize, "").Items;
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            ProductService.DeleteProduct(newItem.Id);

            model = ProductService.FindProductsListModel(testBrand.Id, 0, 0, 1, PageSize, "").Items;
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindProductModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var model = createProduct(testCompany, user);
            var error = ProductService.InsertOrUpdateProduct(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = ProductService.FindProductModel(model.Id, null, testCompany, false);
            AreEqual(model, test);
        }

        [Ignore]
        [TestMethod]
        public void FindProductPriceTest() {
            Assert.Fail("To be implemented - pricing profiles to be determined");
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }

        [TestMethod]
        public void ApplyUnitConversionsTest() {
            var product = new ProductModel {
                Length = 4,
                Width = 8,
                Height = 2,
                Weight = 0.5,

                PackedLength = 8,
                PackedWidth = 16,
                PackedHeight = 4, 
                PackedWeight = 4,

                InnerLength = 8,
                InnerWidth = 16,
                InnerHeight = 4,
                InnerWeight = 4,

                MasterLength = 1,
                MasterWidth = 0.5,
                MasterHeight = 0.4,
                MasterWeight = 3.4
            };
            var tempProduct = ProductService.MapToModel(product);

            // Try a metric conversion - should be no change
            ProductService.ApplyUnitConversions(product, UnitOfMeasure.Metric);
            AreEqual(product, tempProduct);

            // Try an imperial conversion - values should be converted to imperial
            ProductService.ApplyUnitConversions(tempProduct, UnitOfMeasure.Imperial);

            Assert.IsTrue(tempProduct.Length == product.Length.CmToInches(), "Error: Incorrect conversion (1)");
            Assert.IsTrue(tempProduct.Width == product.Width.CmToInches(), "Error: Incorrect conversion (2)");
            Assert.IsTrue(tempProduct.Height == product.Height.CmToInches(), "Error: Incorrect conversion (3)");
            Assert.IsTrue(tempProduct.Weight == product.Weight.KgToLb(), "Error: Incorrect conversion (4)");

            Assert.IsTrue(tempProduct.PackedLength == product.PackedLength.CmToInches(), "Error: Incorrect conversion (5)");
            Assert.IsTrue(tempProduct.PackedWidth == product.PackedWidth.CmToInches(), "Error: Incorrect conversion (6)");
            Assert.IsTrue(tempProduct.PackedHeight == product.PackedHeight.CmToInches(), "Error: Incorrect conversion (7)");
            Assert.IsTrue(tempProduct.PackedWeight == product.PackedWeight.KgToLb(), "Error: Incorrect conversion (8)");

            Assert.IsTrue(tempProduct.InnerLength == product.InnerLength.CmToInches(), "Error: Incorrect conversion (9)");
            Assert.IsTrue(tempProduct.InnerWidth == product.InnerWidth.CmToInches(), "Error: Incorrect conversion (10)");
            Assert.IsTrue(tempProduct.InnerHeight == product.InnerHeight.CmToInches(), "Error: Incorrect conversion (11)");
            Assert.IsTrue(tempProduct.InnerWeight == product.InnerWeight.KgToLb(), "Error: Incorrect conversion (12)");

            Assert.IsTrue(tempProduct.MasterLength == product.MasterLength.MToFeet(), "Error: Incorrect conversion (13)");
            Assert.IsTrue(tempProduct.MasterWidth == product.MasterWidth.MToFeet(), "Error: Incorrect conversion (14)");
            Assert.IsTrue(tempProduct.MasterHeight == product.MasterHeight.MToFeet(), "Error: Incorrect conversion (15)");
            Assert.IsTrue(tempProduct.MasterWeight == product.MasterWeight.KgToLb(), "Error: Incorrect conversion (16)");
        }

        [TestMethod]
        public void InsertOrUpdateProductTest() {
            // Tested in DeleteProductTest
        }

        [TestMethod]
        public void DeleteProductTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a product
            ProductModel model = createProduct(testCompany, user);

            var error = ProductService.InsertOrUpdateProduct(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindProduct(model.Id);
            ProductModel test = ProductService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            ProductService.DeleteProduct(model.Id);

            // And check that is was deleted
            result = db.FindProduct(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockProductTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createProduct(testCompany, testUser);

            var error = ProductService.InsertOrUpdateProduct(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = ProductService.LockProduct(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = ProductService.InsertOrUpdateProduct(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = ProductService.InsertOrUpdateProduct(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = ProductService.LockProduct(model);
            error = ProductService.InsertOrUpdateProduct(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void ValidateModelTest() {
            // Called by all tests above which use InsertOrUpdateProduct
        }

        private ProductModel createProduct(CompanyModel company, UserModel user, BrandModel brand = null) {
            var supplier = SupplierService.FindSuppliersListModel(company).Items.First();
            ProductModel model = new ProductModel {
                CreatedById = user.Id,
                CreatedByText = (user.FirstName + " " + user.LastName).Trim(),
                CreatedDate = DateTimeOffset.Now,
                BrandId = null,
                ItemName = RandomString().Left(30),
                ItemNumber = RandomString().Left(30),
                Picture = "/Content/default.jpg",
                PrimarySupplierId = supplier.Id,
                SupplierName = supplier.Name,
                TaxCodeId = supplier.TaxCodeId,
                Enabled = true
            };
            if (brand != null) model.BrandId = brand.Id;
            return model;
        }

        private BrandModel createBrandWithProducts(CompanyModel company, UserModel user) {
            BrandModel model = new BrandModel {
                BrandName = RandomString(),
                Enabled = false
            };
            ProductService.InsertOrUpdateBrand(model, user, "");

            // Add some products to the brand
            for (int i = 0; i < 20; i++) {
                var prod = createProduct(company, user, model);
                var error = ProductService.InsertOrUpdateProduct(prod, user, "");
                Assert.IsTrue(!error.IsError, "Error: " + error.Message);
            }

            return model;
        }
    }
}
