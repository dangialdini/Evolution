using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.SalesService;
using Evolution.MapperService;

namespace Evolution.SalesServiceTests {
    [TestClass]
    public partial class SalesServiceTests : BaseTest {
        [TestMethod]
        public void CopySaleToTempTest() {
            // Tested in:
            //      LockSalesOrderDetailTempTest
            //      GetNextSalesOrderDetailLineNumberTempTest
        }

        [TestMethod]
        public void CopyTempToSalesOrderHeaderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testSale = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 10);

            var sohtModel = SalesService.CopySaleToTemp(testCompany, testSale, testUser, false);

            var excludes = new List<string>();
            excludes.Add("Id");
            excludes.Add("OriginalRowIdId");
            excludes.Add("SalesOrderDetails");

            AreEqual(testSale, sohtModel, excludes);
        }
    }
}
