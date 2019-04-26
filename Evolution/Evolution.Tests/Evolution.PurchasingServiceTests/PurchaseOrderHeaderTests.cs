using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.PurchasingService;
using Evolution.TaskProcessor;
using Evolution.Extensions;

namespace Evolution.PurchasingServiceTests {
    public partial class PurchasingServiceTests {
        [TestMethod]
        public void FindPurchaseOrderHeadersListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a random number of purchases
            List<PurchaseOrderHeaderModel> pohList = new List<PurchaseOrderHeaderModel>();
            int numPohs = RandomInt(5, 15);
            for (int i = 0; i < numPohs; i++) pohList.Add(GetTestPurchaseOrderHeader(testCompany, testUser, 10));

            // Check that they are found
            var result = PurchasingService.FindPurchaseOrderHeadersListModel(testCompany.Id, 0, 1, PageSize, "", 0, 0, 0);
            int actual = result.Items.Count;
            int expected = numPohs;
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Now delete them
            foreach (var poh in pohList) PurchasingService.DeletePurchaseOrderHeader(poh);

            // Now check that they have disappeared
            result = PurchasingService.FindPurchaseOrderHeadersListModel(testCompany.Id, 0, 1, PageSize, "", 0, 0, 0);
            actual = result.Items.Count;
            expected = 0;
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
        }

        [TestMethod]
        public void FindPurchaseOrderHeadersStringTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a random number of purchases
            List<PurchaseOrderHeaderModel> pohList = new List<PurchaseOrderHeaderModel>();
            int numPohs = RandomInt(5, 15);
            for (int i = 0; i < numPohs; i++) pohList.Add(GetTestPurchaseOrderHeader(testCompany, testUser, 10));

            // Check that they are found
            var result = PurchasingService.FindPurchaseOrderHeadersString(testCompany);
            int actual = result.CountOf("|") + 1;
            int expected = numPohs;
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Now delete them
            foreach (var poh in pohList) PurchasingService.DeletePurchaseOrderHeader(poh);

            // Now check that they have disappeared
            result = PurchasingService.FindPurchaseOrderHeadersString(testCompany);
            Assert.IsTrue(result == "", $"Error: '{result}' was returned when and empty string was expected");
        }

        [TestMethod]
        public void FindPurchaseOrderHeaderSummaryListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a random number of purchases
            List<PurchaseOrderHeaderModel> pohList = new List<PurchaseOrderHeaderModel>();
            int numPohs = RandomInt(5, 25);
            for (int i = 0; i < numPohs; i++) pohList.Add(GetTestPurchaseOrderHeader(testCompany, testUser, 10));

            // Check that they are found
            var result = PurchasingService.FindPurchaseOrderHeaderSummaryListModel(testCompany, testUser, 0, 1, PageSize, "");
            int actual = result.Items.Count;
            int expected = numPohs;
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Now delete them
            foreach (var poh in pohList) PurchasingService.DeletePurchaseOrderHeader(poh);

            // Now check that they have disappeared
            result = PurchasingService.FindPurchaseOrderHeadersListModel(testCompany.Id, 0, 1, PageSize, "", 0, 0, 0);
            actual = result.Items.Count;
            expected = 0;
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
        }

        [TestMethod]
        public void FindPurchaseOrderHeaderModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            var model = GetTestPurchaseOrderHeader(testCompany, testUser);

            var test = PurchasingService.FindPurchaseOrderHeaderModel(model.Id, testCompany, false);

            var excludes = new List<string>();
            excludes.Add("OrderNumberUrl");         // Because it isn't known in data prep
            AreEqual(model, test, excludes);
        }

        [TestMethod]
        public void FindPurchaseOrderTotalTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            var poh = GetTestPurchaseOrderHeader(testCompany, testUser, 1);
            var model = PurchasingService.FindPurchaseOrderDetailListModel(poh);

            var pod = model.Items.First();
            decimal totalValue = pod.OrderQty.Value * pod.UnitPriceExTax.Value;

            for (int i = 0; i < 16; i++) {
                pod = model.Items.First();
                pod.Id = 0;
                pod.OrderQty = RandomInt();
                totalValue += pod.OrderQty.Value * pod.UnitPriceExTax.Value;

                var error = PurchasingService.InsertOrUpdatePurchaseOrderDetail(pod, testUser, "");
                Assert.IsTrue(!error.IsError, error.Message);
            }

            var result = PurchasingService.FindPurchaseOrderTotal(poh);
            Assert.IsTrue(result == totalValue, $"Error: {result} was returned when {totalValue} was expected");
        }

        [TestMethod]
        public void InsertOrUpdatePurchaseOrderHeaderTest() {
            // Tested in DeletePurchaseOrderHeaderTest
        }

        [TestMethod]
        public void DeletePurchaseOrderHeaderTest() {
            // Get a test user
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a purchase
            var model = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            // Check that it was written
            var result = db.FindPurchaseOrderHeader(model.Id);
            var test = PurchasingService.MapToModel(result);

            var excludes = new List<string>();
            excludes.Add("OrderNumberUrl");         // Because it isn't known yet
            AreEqual(model, test, excludes);

            // Now delete it
            PurchasingService.DeletePurchaseOrderHeader(model.Id);

            // And check that is was deleted
            result = db.FindPurchaseOrderHeader(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockPurchaseOrderHeaderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a record
            var model = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            // Get the current Lock
            string lockGuid = PurchasingService.LockPurchaseOrderHeader(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = PurchasingService.InsertOrUpdatePurchaseOrderHeader(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = PurchasingService.InsertOrUpdatePurchaseOrderHeader(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = PurchasingService.LockPurchaseOrderHeader(model);
            error = PurchasingService.InsertOrUpdatePurchaseOrderHeader(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void CopyPurchaseOrderToTempTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testPurchase = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            var pohtModel = PurchasingService.CopyPurchaseOrderToTemp(testCompany, testPurchase, testUser);

            var excludes = new List<string>();
            excludes.Add("Id");
            excludes.Add("OriginalRowIdId");

            AreEqual(testPurchase, pohtModel, excludes);
        }

        [TestMethod]
        public void SendPurchaseOrderToSupplierTest() {
            // Tested in FilePackagerService tests
        }

        [TestMethod]
        public void SendPurchaseOrderToFreightForwarderTest() {
            // Tested in FilePackagerService tests
        }

        [TestMethod]
        public void SendPurchaseOrderToWarehouseTest() {
            // Tested in FilePackagerService tests
        }

        [TestMethod]
        public void GetPurchaseCountTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a purchase
            int actual = PurchasingService.GetPurchaseCount(testCompany);
            int expected = 0;
            Assert.IsTrue(expected == actual, $"Error: {actual} was returned when {expected} was expected");

            var testPurchase = GetTestPurchaseOrderHeader(testCompany, testUser);
            actual = PurchasingService.GetPurchaseCount(testCompany);
            expected = 1;
            Assert.IsTrue(expected == actual, $"Error: {actual} was returned when {expected} was expected");

            PurchasingService.DeletePurchaseOrderHeader(testPurchase.Id);
            actual = PurchasingService.GetPurchaseCount(testCompany);
            expected = 0;
            Assert.IsTrue(expected == actual, $"Error: {actual} was returned when {expected} was expected");
        }

        [TestMethod]
        public void FindUndeliveredPurchaseOrdersTest() {
            // Tests the service
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Should be no orders to start with
            var undeliveredOrders = PurchasingService.FindUndeliveredPurchaseOrders(testCompany);
            int expected = 0,
                actual = undeliveredOrders.Items.Count;
            Assert.IsTrue(actual == expected, $"Error: {actual} orders were returned when {expected} were expected (1)");


            // Create some orders
            var order1 = GetTestPurchaseOrderHeader(testCompany, testUser, 10);
            var order2 = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            undeliveredOrders = PurchasingService.FindUndeliveredPurchaseOrders(testCompany);
            expected = 0;
            actual = undeliveredOrders.Items.Count;
            Assert.IsTrue(actual == expected, $"Error: {actual} orders were returned when {expected} were expected (2)");

            // Now move the RealisticRequiredDate back so that the orders are overdue
            order1.RealisticRequiredDate = DateTimeOffset.Now.AddDays(RandomInt(1, 10) * -1);

            string lockGuid = PurchasingService.LockPurchaseOrderHeader(order1);
            PurchasingService.InsertOrUpdatePurchaseOrderHeader(order1, testUser, lockGuid);

            undeliveredOrders = PurchasingService.FindUndeliveredPurchaseOrders(testCompany);
            expected = 1;
            actual = undeliveredOrders.Items.Count;
            Assert.IsTrue(actual == expected, $"Error: {actual} orders were returned when {expected} were expected (3)");

            // Now move the other order back
            order2.RealisticRequiredDate = DateTimeOffset.Now.AddDays(RandomInt(1, 10) * -1);

            lockGuid = PurchasingService.LockPurchaseOrderHeader(order2);
            PurchasingService.InsertOrUpdatePurchaseOrderHeader(order2, testUser, lockGuid);

            undeliveredOrders = PurchasingService.FindUndeliveredPurchaseOrders(testCompany);
            expected = 2;
            actual = undeliveredOrders.Items.Count;
            Assert.IsTrue(actual == expected, $"Error: {actual} orders were returned when {expected} were expected (4)");

            // Put them forward
            order1.RealisticRequiredDate = DateTimeOffset.Now.AddDays(RandomInt(1, 10));

            lockGuid = PurchasingService.LockPurchaseOrderHeader(order1);
            PurchasingService.InsertOrUpdatePurchaseOrderHeader(order1, testUser, lockGuid);

            order2.RealisticRequiredDate = DateTimeOffset.Now.AddDays(RandomInt(1, 10));

            lockGuid = PurchasingService.LockPurchaseOrderHeader(order2);
            PurchasingService.InsertOrUpdatePurchaseOrderHeader(order2, testUser, lockGuid);

            undeliveredOrders = PurchasingService.FindUndeliveredPurchaseOrders(testCompany);
            expected = 0;
            actual = undeliveredOrders.Items.Count;
            Assert.IsTrue(actual == expected, $"Error: {actual} orders were returned when {expected} were expected (5)");
        }

        [TestMethod]
        public void FindUndeliveredPurchaseOrders2Test() {
            // Tests the TaskProcessor/emails
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Get the current email queue count
            db.EmptyEMailQueue();
            int beforeCount = db.FindEMailQueues().Count();

            var task = new NotificationTask(db);
            task.StartTask();

            int afterCount = db.FindEMailQueues().Count();
            Assert.IsTrue(afterCount == beforeCount, $"Error: The EMail queue contains {afterCount} item(s) when {beforeCount} were expected (1)");

            // Create some orders
            var order1 = GetTestPurchaseOrderHeader(testCompany, testUser, 10);
            var order2 = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            task.CheckCompanyForPassedUnpackDates(testCompany);

            // The orders are not overdue, so we should not have added any messages to the email queue
            afterCount = db.FindEMailQueues().Count();
            Assert.IsTrue(afterCount == beforeCount, $"Error: The EMail queue contains {afterCount} item(s) when {beforeCount} were expected (2)");

            // Now move the RealisticRequiredDate back so that the orders are overdue
            order1.RealisticRequiredDate = DateTimeOffset.Now.AddDays(RandomInt(1, 10) * -1);

            string lockGuid = PurchasingService.LockPurchaseOrderHeader(order1);
            PurchasingService.InsertOrUpdatePurchaseOrderHeader(order1, testUser, lockGuid);

            beforeCount = db.FindEMailQueues().Count();

            var error = task.CheckCompanyForPassedUnpackDates(testCompany);
            Assert.IsTrue(!error.IsError, error.Message);

            // We should have added a message to the email queue
            afterCount = db.FindEMailQueues().Count();
            int expected = 1 + beforeCount;
            Assert.IsTrue(afterCount == expected, $"Error: The EMail queue contains {afterCount} item(s) when {expected} were expected (3)");

            // Now move the other order back
            order2.RealisticRequiredDate = DateTimeOffset.Now.AddDays(RandomInt(1, 10) * -1);

            lockGuid = PurchasingService.LockPurchaseOrderHeader(order2);
            PurchasingService.InsertOrUpdatePurchaseOrderHeader(order2, testUser, lockGuid);

            beforeCount = db.FindEMailQueues().Count() - beforeCount;

            task.CheckCompanyForPassedUnpackDates(testCompany);

            // We should have added two messages
            afterCount = db.FindEMailQueues().Count();
            expected = 2 + beforeCount;
            Assert.IsTrue(afterCount == expected, $"Error: The EMail queue contains {afterCount} item(s) when {expected} were expected (4)");

            // Now add another sales person to the user groups of the orders.
            // Note that each order could be a different brand category/group
            var testUser2 = GetTestUser();
            var brandCategory = ProductService.FindBrandCategoryModel(order1.BrandCategoryId.Value, testCompany, false);
            MembershipManagementService.AddUserToGroup(brandCategory.CategoryName + " Purchasing", testUser2);

            brandCategory = ProductService.FindBrandCategoryModel(order2.BrandCategoryId.Value, testCompany, false);
            MembershipManagementService.AddUserToGroup(brandCategory.CategoryName + " Purchasing", testUser2);

            beforeCount = db.FindEMailQueues().Count();

            task.CheckCompanyForPassedUnpackDates(testCompany);

            // We should have added another 4 messages
            afterCount = db.FindEMailQueues().Count();
            expected = 2 + beforeCount;
            Assert.IsTrue(afterCount == expected, $"Error: The EMail queue contains {afterCount} item(s) when {expected} were expected (5)");

            // Put the orders forward
            order1.RealisticRequiredDate = DateTimeOffset.Now.AddDays(RandomInt(1, 10));

            lockGuid = PurchasingService.LockPurchaseOrderHeader(order1);
            PurchasingService.InsertOrUpdatePurchaseOrderHeader(order1, testUser, lockGuid);

            order2.RealisticRequiredDate = DateTimeOffset.Now.AddDays(RandomInt(1, 10));

            lockGuid = PurchasingService.LockPurchaseOrderHeader(order2);
            PurchasingService.InsertOrUpdatePurchaseOrderHeader(order2, testUser, lockGuid);

            beforeCount = db.FindEMailQueues().Count();

            task.CheckCompanyForPassedUnpackDates(testCompany);

            // We should not have added any more messages
            afterCount = db.FindEMailQueues().Count();
            expected = 0 + beforeCount;
            Assert.IsTrue(afterCount == expected, $"Error: The EMail queue contains {afterCount} item(s) when {expected} were expected (6)");

            task.EndTask(0);
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }
    }
}
