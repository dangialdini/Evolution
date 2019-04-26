using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.SalesService;
using Evolution.Enumerations;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests {
        [TestMethod]
        public void FindSalesOrderDetailTempsListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testSale = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 10);

            var sohtModel = SalesService.CopySaleToTemp(testCompany, testSale, testUser, false);

            var model = SalesService.FindSalesOrderDetailTempsListModel(testCompany.Id, sohtModel.Id, 0, 1, PageSize, "");
            var dbData = db.FindSalesOrderDetailTemps(testCompany.Id, sohtModel.Id);

            int expected = dbData.Count(),
                actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = SalesService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createSalesOrderDetailTemp(sohtModel, testCompany, testCustomer, testUser, model.Items[0].ProductId.Value);
            var error = SalesService.InsertOrUpdateSalesOrderDetailTemp(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = SalesService.FindSalesOrderDetailTempsListModel(testCompany.Id, sohtModel.Id, 0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            SalesService.DeleteSalesOrderDetailTemp(newItem.Id);

            model = SalesService.FindSalesOrderDetailTempsListModel(testCompany.Id, sohtModel.Id, 0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindSalesOrderDetailTempModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testSale = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 1);

            var sohtModel = SalesService.CopySaleToTemp(testCompany, testSale, testUser, false);
            var model = SalesService.FindSalesOrderDetailTempsListModel(testCompany.Id, sohtModel.Id);

            var newItem = createSalesOrderDetailTemp(sohtModel, testCompany, testCustomer, testUser, model.Items[0].ProductId.Value);
            var error = SalesService.InsertOrUpdateSalesOrderDetailTemp(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = SalesService.FindSalesOrderDetailTempModel(newItem.Id, null, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateSalesOrderDetailTempTest() {
            // Tested by all tests which copy a sale to temp
        }

        [TestMethod]
        public void DeleteSalesOrderDetailTempTest() {
            // Get a test user
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testSale = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 1);

            var sohtModel = SalesService.CopySaleToTemp(testCompany, testSale, testUser, false);
            var model = SalesService.FindSalesOrderDetailTempsListModel(testCompany.Id, sohtModel.Id, 0, 1, PageSize, "");

            // Create a record
            var newItem = createSalesOrderDetailTemp(sohtModel, testCompany, testCustomer, testUser, model.Items[0].ProductId.Value);

            var error = SalesService.InsertOrUpdateSalesOrderDetailTemp(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindSalesOrderDetailTemp(newItem.Id);
            var test = SalesService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            SalesService.DeleteSalesOrderDetailTemp(newItem.Id);

            // And check that is was deleted
            result = db.FindSalesOrderDetailTemp(newItem.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockSalesOrderDetailTempTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Create a record
            var temp1 = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 1);
            var soht = SalesService.CopySaleToTemp(testCompany, temp1, testUser, false);
            var sodtl = SalesService.FindSalesOrderDetailTempsListModel(testCompany.Id, soht.Id, 0);
            var model = sodtl.Items.First();

            // Get the current Lock
            string lockGuid = SalesService.LockSalesOrderDetailTemp(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = SalesService.InsertOrUpdateSalesOrderDetailTemp(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = SalesService.InsertOrUpdateSalesOrderDetailTemp(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = SalesService.LockSalesOrderDetailTemp(model);
            error = SalesService.InsertOrUpdateSalesOrderDetailTemp(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void GetNextSalesOrderDetailLineNumberTempTest() {
            // Test that next line numbers are correctly incremented in steps of 1000
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testSale = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 1);

            var soht = SalesService.CopySaleToTemp(testCompany, testSale, testUser, false);
            var itemList = SalesService.FindSalesOrderDetailTempsListModel(testCompany.Id, soht.Id, 0, 1, 9999, "");

            int expected = 1,
                actual = itemList.Items.Count();
            Assert.IsTrue(expected == actual, $"Error: {actual} item(s) were found when {expected} were expected");

            // Test array
            int[] tests = { 0,      1000,
                            400,    1000,
                            700,    1000,
                            1000,   2000,
                            1100,   2000,
                            2200,   3000,
                            9900,   10000,
                            10100,  11000
            };

            for (int i = 0; i < tests.Count(); i += 2) {
                itemList.Items[0].LineNumber = tests[i];

                var lgs = SalesService.LockSalesOrderDetailTemp(itemList.Items[0]);
                var error = SalesService.InsertOrUpdateSalesOrderDetailTemp(itemList.Items[0], testUser, lgs);
                Assert.IsTrue(!error.IsError, error.Message);

                expected = tests[i + 1];
                actual = db.GetNextSalesOrderDetailLineNumber(soht.Id, true);
                Assert.IsTrue(expected == actual, $"Error: Line number {actual} was returned when {expected} were expected");
            }
        }

        private SalesOrderDetailTempModel createSalesOrderDetailTemp(SalesOrderHeaderTempModel soht, 
                                                                     CompanyModel testCompany, 
                                                                     CustomerModel testCustomer, 
                                                                     UserModel testUser, 
                                                                     int productId) {
            var prodPrice = ProductService.FindProductPrice(testCompany, productId, testCustomer.Id);

            var sodt = new SalesOrderDetailTempModel {
                CompanyId = testCompany.Id,
                SalesOrderHeaderTempId = soht.Id,
                LineNumber = 1000,
                ProductId = productId,
                OrderQty = RandomInt(),
                UnitPriceExTax = (prodPrice == null ? 0 : prodPrice.SellingPrice)
                //ConflictFlag = false,
                //ConflictApproved = false,
                //ReallocateItem = false
            };

            return sodt;
        }
    }
}
