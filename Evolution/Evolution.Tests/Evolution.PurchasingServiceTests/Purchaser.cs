using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.PurchasingService;

namespace Evolution.PurchasingServiceTests {
    public partial class PurchasingServiceTests {
        [TestMethod]
        public void FindPurchasersListItemModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            int itemCount = RandomInt(5, 15);
            var testOrder = GetTestPurchaseOrderHeader(testCompany, testUser, itemCount);
            var brandCat = ProductService.FindBrandCategoryModel(testOrder.BrandCategoryId.Value, testCompany, false);

            string errorMsg = "";
            var purchasers = PurchasingService.FindOrderPurchasers(testOrder,
                                                                   testCompany,
                                                                   testOrder.OrderNumber.Value,
                                                                   ref errorMsg);
            int expected = 1,
                actual = purchasers.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} Purchaser(s) were found when {expected} were expected");

            // No add some more users to the same user group as the purchaser
            var testPurchaser1 = GetTestUser();
            MembershipManagementService.AddUserToGroup(brandCat.CategoryName + " Purchasing", testPurchaser1);

            var testPurchaser2 = GetTestUser();
            MembershipManagementService.AddUserToGroup(brandCat.CategoryName + " Purchasing", testPurchaser2);

            purchasers = PurchasingService.FindOrderPurchasers(testOrder,
                                                               testCompany,
                                                               testOrder.OrderNumber.Value,
                                                               ref errorMsg);
            expected = 3;
            actual = purchasers.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} Purchaser(s) were found when {expected} were expected");
        }
    }
}
