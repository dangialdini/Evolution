using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.SalesService;
using Evolution.MediaService;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests {
        [TestMethod]
        public void FindOrderActioningTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var brandCategory = ProductService.FindBrandCategoriesModel(testCompany).FirstOrDefault();
            var additionalInfo = CustomerService.FindCustomerAdditionalInfoModel(testCustomer.Id, testCompany);

            OrderActionListModel orderList = SalesService.FindOrderActioning(testCompany.Id, testCompany.DefaultLocationID.Value, additionalInfo.RegionId.Value, 0, brandCategory.Id, 0);
            var expected = 0;
            var actual = orderList.Items.Count();
            Assert.IsTrue(expected == actual, $"Error: {expected} numeber of records should exist in the db, when {actual} was found");

            // Create an X amount of Sales
            List<SalesOrderHeaderModel> sohList = new List<SalesOrderHeaderModel>();
            for(var i = 0; i <= RandomInt(5, 20); i++) {
                var soh = GetTestSalesOrderHeader(testCompany, testCustomer, testUser);
                var soStatus = LookupService.FindSalesOrderHeaderStatusByValueModel(SalesOrderHeaderStatus.ConfirmedOrder);
                soh.SOStatus = soStatus.Id;
                soh.SOStatusValue = (SalesOrderHeaderStatus)soStatus.StatusValue;
                soh.SOStatusText = soStatus.StatusName;
                soh.NextActionId = (int)Enumerations.SaleNextAction.AwaitingPaymentAccounts;

                var error = SalesService.InsertOrUpdateSalesOrderHeader(soh, testUser, SalesService.LockSalesOrderHeader(soh));
                Assert.IsTrue(!error.IsError, error.Message);

                sohList.Add(soh);
            }

            expected = sohList.Count();
            orderList = SalesService.FindOrderActioning(testCompany.Id, testCompany.DefaultLocationID.Value, 0, 0, brandCategory.Id, 0);
            actual = orderList.Items.Count();
            Assert.IsTrue(expected == actual, $"Error: {expected} numeber of records should exist in the db, when {actual} was found");

            // Delete them
            foreach (var order in sohList) {
                SalesService.DeleteSalesOrderHeader(order);
            }
            orderList = SalesService.FindOrderActioning(testCompany.Id, testCompany.DefaultLocationID.Value, 0, 0, brandCategory.Id, 0);
            expected = 0;
            actual = orderList.Items.Count();
            Assert.IsTrue(expected == actual, $"Error: {expected} numeber of records should exist in the db, when {actual} was found");
        }

    }
}
