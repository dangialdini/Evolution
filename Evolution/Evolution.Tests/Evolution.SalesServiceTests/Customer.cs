using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.Enumerations;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests : BaseTest {
        [TestMethod]
        public void FindCustomerTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var soh = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 1);
            var soht = SalesService.CopySaleToTemp(testCompany, soh, testUser, false);

            var check1 = SalesService.FindCustomer(soht, testCompany);
            AreEqual(check1, testCustomer);

            var sodt = SalesService.FindSalesOrderDetailTempsListModel(testCompany.Id, soht.Id)
                                   .Items
                                   .FirstOrDefault();

            var check2 = SalesService.FindCustomer(sodt, testCompany);
            AreEqual(check2, testCustomer);
        }
    }
}
