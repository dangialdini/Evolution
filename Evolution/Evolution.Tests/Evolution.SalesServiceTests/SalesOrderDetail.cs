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
        public void FindSalesOrderDetailListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var soh = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 1);

            var model = SalesService.FindSalesOrderDetailListModel(testCompany, soh);
            var dbData = db.FindSalesOrderDetails(testCompany.Id, soh.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = SalesService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item and make sure it is found
            var newItem = createSalesOrderDetail(model);
            var error = SalesService.InsertOrUpdateSalesOrderDetail(newItem, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = SalesService.FindSalesOrderDetailListModel(testCompany, soh);
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            // Detail items are removed from the temp table, not main table, hence there is
            // no service API to delete them from main
            db.DeleteSalesOrderDetail(newItem.Id);

            model = SalesService.FindSalesOrderDetailListModel(testCompany, soh);
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindSalesOrderDetailModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var soh = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 1);

            var model = SalesService.FindSalesOrderDetailModel(soh.Id);
            var dbData = db.FindSalesOrderDetail(soh.Id);

            var temp = SalesService.MapToModel(dbData);
            AreEqual(temp, model);
        }

        [TestMethod]
        public void InsertOrUpdateSalesOrderDetailTest() {
            // Tested in all tests which create a SalesOrderHeader
        }

        private SalesOrderDetailModel createSalesOrderDetail(List<SalesOrderDetailModel> model) {
            // Create a new item by duplicating a randomly selected item already in the list
            // This is an easy way, otherwise we have to go looking for suppliers, products, tax codes etc
            var sod = SalesService.MapToModel(model[RandomInt(0, model.Count() - 1)]);
            sod.Id = 0;
            sod.LineNumber = 0;
            sod.OrderQty = RandomInt();

            return sod;
        }
    }
}
