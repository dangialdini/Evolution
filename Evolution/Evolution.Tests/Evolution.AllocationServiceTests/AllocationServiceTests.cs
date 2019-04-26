using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.AllocationService;
using Evolution.Models.Models;

namespace Evolution.AllocationServiceTests {
    [TestClass]
    public partial class AllocationServiceTests : BaseTest {
        private List<ProductModel> FindProductsForTest(Company company, int numProducts) {
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
                             .Take(numProducts)
                             .ToList();
            foreach (var item in tempProd) {
                var product = ProductService.MapToModel(item);
                model.Add(product);
            }
            return model;
        }
        
        private AllocationModel createAllocation(CompanyModel company, ProductModel product) {
            AllocationModel model = new AllocationModel {
                CompanyId = company.Id,
                ProductId = product.Id
            };
            return model;
        }
        /*
        [TestMethod]
        public void MapToModelTest() {
            Assert.Fail("Test to be implemented!");
        }
        */
    }
}
