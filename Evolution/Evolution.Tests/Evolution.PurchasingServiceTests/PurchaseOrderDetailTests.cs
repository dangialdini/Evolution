using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.PurchasingService;

namespace Evolution.PurchasingServiceTests {
    public partial class PurchasingServiceTests {
        [TestMethod]
        public void FindPurchaseOrderDetailListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            var poh = GetTestPurchaseOrderHeader(testCompany, testUser, 1);

            var model = PurchasingService.FindPurchaseOrderDetailListModel(poh);
            var dbData = db.FindPurchaseOrderDetails(testCompany.Id, poh.Id);

            int expected = dbData.Count(),
                actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = PurchasingService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createPurchaseOrderDetail(model);
            var error = PurchasingService.InsertOrUpdatePurchaseOrderDetail(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = PurchasingService.FindPurchaseOrderDetailListModel(poh);
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            // Detail items are removed from the temp table, not main table, hence there is
            // no service API to delete them from main
            db.DeletePurchaseOrderDetail(newItem.Id);

            model = PurchasingService.FindPurchaseOrderDetailListModel(poh);
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindPurchaseOrderDetailModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            var poh = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            var podList = PurchasingService.FindPurchaseOrderDetailListModel(poh);
            Assert.IsTrue(podList != null, "Error: NULL was returned when an object was expected");

            var pod = PurchasingService.FindPurchaseOrderDetailModel(podList.Items[0].Id);
            Assert.IsTrue(podList != null, "Error: NULL was returned when an object was expected");

            // Now delete it and check it has gone
            PurchasingService.DeletePurchaseOrderDetail(pod);

            pod = PurchasingService.FindPurchaseOrderDetailModel(podList.Items[0].Id);
            Assert.IsTrue(pod == null, "Error: An object was returned when NULL was expected");
        }

        [TestMethod]
        public void InsertOrUpdatePurchaseOrderDetailTest() {
            // Tested in all tests which create a PurchaseOrderHeader
        }

        [TestMethod]
        public void DeletePurchaseOrderDetailTest() {
            // Tested in:
            //      FindPurchaseOrderDetailListModelTest
            //      DeletePurchaseOrderDetail
        }

        [TestMethod]
        public void LockPurchaseOrderDetailTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a record
            var poh = GetTestPurchaseOrderHeader(testCompany, testUser, 1);

            // Get the first detail record
            var detailList = PurchasingService.FindPurchaseOrderDetailListModel(poh);
            var detail = detailList.Items.First();

            // Get the current Lock
            string lockGuid = PurchasingService.LockPurchaseOrderDetail(detail);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = PurchasingService.InsertOrUpdatePurchaseOrderDetail(detail, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = PurchasingService.InsertOrUpdatePurchaseOrderDetail(detail, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = PurchasingService.LockPurchaseOrderDetail(detail);
            error = PurchasingService.InsertOrUpdatePurchaseOrderDetail(detail, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void FindPurchaseOrderHeaderByPONumberModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a record
            var poh1 = GetTestPurchaseOrderHeader(testCompany, testUser, 1);

            // Try to find it using the PONumber
            var poh2 = PurchasingService.FindPurchaseOrderHeaderByPONumberModel(poh1.OrderNumber.Value, testCompany);
            Assert.IsTrue(poh2 != null, "Error: A non-NULL value was expected");

            // Check thiat it is the same record
            decimal expected = poh1.OrderNumber.Value,
                    actual = poh2.OrderNumber.Value;
            Assert.IsTrue(expected == actual, $"Error: {actual} was returned when {expected} was expected");

            expected = poh1.Id;
            actual = poh2.Id;
            Assert.IsTrue(expected == actual, $"Error: {actual} was returned when {expected} was expected");

            PurchasingService.DeletePurchaseOrderHeader(poh1.Id);

            // Now try to find it again
            poh2 = PurchasingService.FindPurchaseOrderHeaderByPONumberModel(poh1.OrderNumber.Value, testCompany);
            Assert.IsTrue(poh2 == null, "Error: A NULL value was expected but an object was returned");
        }

        private PurchaseOrderDetailModel createPurchaseOrderDetail(PurchaseOrderDetailListModel model) {
            // Create a new item by duplicating a randomly selected item already in the list
            // This is an easy way, otherwise we have to go looking for suppliers, products, tax codes etc
            var pod = PurchasingService.MapToModel(model.Items[RandomInt(0, model.Items.Count() - 1)]);
            pod.Id = 0;
            pod.LineNumber = 0;
            pod.OrderQty = RandomInt();

            return pod;
        }
    }
}
