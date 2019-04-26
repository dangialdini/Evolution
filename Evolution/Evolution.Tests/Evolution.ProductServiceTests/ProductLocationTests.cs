using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.LookupService;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.ProductServiceTests {
    public partial class ProductServiceTests {
        [TestMethod]
        public void FindProductLocationModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var parentCompany = CompanyService.FindParentCompanyModel();

            var productList = FindProductsForTest(parentCompany, 1);
            Assert.IsTrue(productList.Count() > 0, "Error: No products were returned for the parent company. This could be because the parent company has not been flagged or it has no allocations");

            var temp = db.FindLocations(parentCompany.Id).FirstOrDefault();
            var location = LookupService.FindLocationModel(temp.Id);

            var model = createProductLocation(testCompany, productList[0], location, 10);
            var error = ProductService.InsertOrUpdateProductLocation(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = ProductService.FindProductLocationModel(model.Id);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateProductLocationTest() {
            // Tested in FindProductLocationModelTest
        }

        [TestMethod]
        public void DeleteProductLocationTest() {
            // Get a test user
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var parentCompany = CompanyService.FindParentCompanyModel();

            var productList = FindProductsForTest(parentCompany, 1);
            Assert.IsTrue(productList.Count() > 0, "Error: No products were returned for the parent company. This could be because the parent company has not been flagged or it has no allocations");

            var temp = db.FindLocations(parentCompany.Id).FirstOrDefault();
            var location = LookupService.FindLocationModel(temp.Id);

            // Create a record
            var model = createProductLocation(testCompany, productList[0], location, 10);
            var error = ProductService.InsertOrUpdateProductLocation(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Check that it was written
            var result = db.FindProductLocation(model.Id);
            var test = ProductService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            ProductService.DeleteProductLocation(model.Id);

            // And check that is was deleted
            result = db.FindProductLocation(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockProductLocationTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var parentCompany = CompanyService.FindParentCompanyModel();

            var productList = FindProductsForTest(parentCompany, 1);
            Assert.IsTrue(productList.Count() > 0, "Error: No products were returned for the parent company. This could be because the parent company has not been flagged or it has no allocations");

            var temp = db.FindLocations(parentCompany.Id).FirstOrDefault();
            var location = LookupService.FindLocationModel(temp.Id);

            // Create a record
            var model = createProductLocation(testCompany, productList[0], location, 10);

            var error = ProductService.InsertOrUpdateProductLocation(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = ProductService.LockProductLocation(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = ProductService.InsertOrUpdateProductLocation(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = ProductService.InsertOrUpdateProductLocation(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = ProductService.LockProductLocation(model);
            error = ProductService.InsertOrUpdateProductLocation(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        private List<ProductModel> FindProductsForTest(CompanyModel company, int numProducts) {
            List<ProductModel> model = new List<ProductModel>();
            /*
            var tempProd = db.FindAllocationsForCompany(company.Id)
                             .Where(a => a.Quantity > 0)
                             .OrderByDescending(a => a.Quantity)
                             .Select(a => a.Product)
                             .Take(numProducts)
                             .ToList();
            */
            var tempProd = db.FindProducts()
                             .Where(p => !p.ItemNumber.Contains("\\"))
                             .Take(numProducts)
                             .ToList();
            foreach (var item in tempProd) {
                var product = ProductService.MapToModel(item);
                model.Add(product);
            }
            return model;
        }

        private ProductLocationModel createProductLocation(CompanyModel company, ProductModel product, 
                                                           LocationModel location, int qtyOnHand) {
            var model = new ProductLocationModel {
                CompanyId = company.Id,
                ProductId = product.Id,
                LocationId = location.Id,
                QuantityOnHand = qtyOnHand,
                SellOnOrder = 0,
                PurchaseOnOrder = 0
            };
            return model;
        }
    }
}
