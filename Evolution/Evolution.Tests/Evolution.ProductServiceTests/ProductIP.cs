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
        public void FindProductIPListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var product = createProduct(testCompany, testUser);
            ProductService.InsertOrUpdateProduct(product, testUser, "");

            // Test Model.Items against Marketing\Llocation - count
            var model = ProductService.FindProductIPListModel(product.Id);
            var marketingLocation = LookupService.FindLOVItemsModel(testCompany, LOVName.MarketingLocation);
            int expected = marketingLocation.Count();
            int actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // check all items in model are Selected = false
            foreach(var item in model.Items) {
                Assert.IsTrue(item.Selected == false, "Error: 'False' was expected when 'True' was returned");
            }

            // Add a new item and make sure it is found
            foreach(var item in model.Items) {
                item.Selected = true;
            }
            var error = ProductService.InsertOrUpdateProductIP(product, model, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // check all items in model are Selected = true
            model = ProductService.FindProductIPListModel(product.Id);
            foreach (var item in model.Items) {
                Assert.IsTrue(item.Selected == true, "Error: 'True' was expected when 'False' was returned");
            }

            // Delete it and make sure it dissapears
            foreach(var item in model.Items) {
                ProductService.DeleteProductIP(item.Id);
            }

            // check all items in model are Selected = false
            model = ProductService.FindProductIPListModel(product.Id);
            foreach (var item in model.Items) {
                Assert.IsTrue(item.Selected == false, "Error: 'False' was expected when 'True' was returned");
            }
        }

        [TestMethod]
        public void InsertOrUpdateProductIPTest() {
            // Tested in FindProductIPListModelTest
        }

        [TestMethod]
        public void DeleteProductIPTest() {
            // Tested in FindProductIPListModelTest
        }

        private ProductIPModel createProductIP(int productId, int marketId) {
            ProductIPModel model = new ProductIPModel();
            model.ProductId = productId;
            model.MarketId = marketId;
            return model;
        }
    }
}
