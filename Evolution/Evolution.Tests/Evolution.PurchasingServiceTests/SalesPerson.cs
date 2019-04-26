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
        public void FindOrderPurchasersTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a purchase
            var poh = GetTestPurchaseOrderHeader(testCompany, testUser, 10);
            var brandCategory = ProductService.FindBrandCategoryModel(poh.BrandCategoryId.Value, testCompany, false);

            string userGroup = brandCategory.CategoryName + " purchasing";
            MembershipManagementService.AddUserToGroup(userGroup, testUser);

            // Get the users
            string errorMsg = "";
            var users = PurchasingService.FindOrderPurchasers(poh, 
                                                              testCompany, 
                                                              poh.OrderNumber.Value,
                                                              ref errorMsg);
            Assert.IsTrue(users != null, errorMsg);
            int expected = 1,
                actual = users.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} user(s) were returned when {expected} were expected");

            // Add some more users
            var testUser2 = GetTestUser();
            MembershipManagementService.AddUserToGroup(userGroup, testUser2);
            var testUser3 = GetTestUser();
            MembershipManagementService.AddUserToGroup(userGroup, testUser3);

            users = PurchasingService.FindOrderPurchasers(poh,
                                                          testCompany,
                                                          poh.OrderNumber.Value,
                                                          ref errorMsg);
            Assert.IsTrue(users != null, errorMsg);
            expected = 3;
            actual = users.Count();
            Assert.IsTrue(actual >= expected, $"Error: {actual} user(s) were returned when {expected} were expected");

            // Now remove one and try again
            MembershipManagementService.DeleteUser(testUser2);

            users = PurchasingService.FindOrderPurchasers(poh,
                                                          testCompany,
                                                          poh.OrderNumber.Value,
                                                          ref errorMsg);
            Assert.IsTrue(users != null, errorMsg);
            expected = 2;
            actual = users.Count();
            Assert.IsTrue(actual >= expected, $"Error: {actual} user(s) were returned when {expected} were expected");

            // Now delete another and try again
            MembershipManagementService.DeleteUser(testUser3);

            users = PurchasingService.FindOrderPurchasers(poh,
                                                          testCompany,
                                                          poh.OrderNumber.Value,
                                                          ref errorMsg);
            Assert.IsTrue(users != null, errorMsg);
            expected = 1;
            actual = users.Count();
            Assert.IsTrue(actual >= expected, $"Error: {actual} user(s) were returned when {expected} were expected");
        }
    }
}
