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
        public void FindSalesPersonListItemModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var model = SalesService.FindSalesPersonListItemModel(testCompany);
            var dbData = db.FindSalesOrderHeaders(testCompany.Id)
                           .Select(soh => soh.User_SalesPerson)
                           .Distinct(); ;

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");
        }
    }
}
