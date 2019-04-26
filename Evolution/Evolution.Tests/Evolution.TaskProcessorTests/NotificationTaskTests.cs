using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.TaskProcessor;
using Evolution.PurchasingService;

namespace Evolution.TaskProcessorTests {
    [TestClass]
    public partial class NotificationTaskTests : BaseTest {
        [TestMethod]
        public void GetTaskNameTest() {
            var task = new NotificationTask(db);
            string expected = TaskName.NotificationTask,
                   actual = task.GetTaskName();
            Assert.IsTrue(actual == expected, $"Error: GetTaskName() returned {actual} when {expected} was expected. Check that the derived task class overrides the GetTaskName() method");
        }

        [TestMethod]
        public void DoProcessingTest() {
            // Tested in CheckCompanyForPassedUnpackDatesTest
        }

        [TestMethod]
        public void CheckForPassedUnpackDatesTest() {
            // Tested in CheckCompanyForPassedUnpackDatesTest
        }

        [TestMethod]
        public void CheckCompanyForPassedUnpackDatesTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            EMailService.EMailService service = new EMailService.EMailService(db);
            service.EmptyEMailQueue();

            var task = new NotificationTask(db);
            
            // Check email queue - should be 0 added
            var expected = 0;
            task.CheckCompanyForPassedUnpackDates(testCompany);
            var actual = service.GetQueueCount();
            Assert.IsTrue(actual == expected, $"Error: Email Count returned {actual} when {expected} was expected");

            // Call email queue = 0 = actual
            // Create POs with Data > now
            var poStatuses = Enum.GetValues(typeof(PurchaseOrderStatus));
            var names = Enum.GetNames(typeof(PurchaseOrderStatus));
            
            foreach(var value in Enum.GetValues(typeof(PurchaseOrderStatus))) {
                var po = GetTestPurchaseOrderHeader(testCompany, testUser);
                po.RealisticRequiredDate = DateTime.Now.AddDays((int)value);
                po.POStatus = (int)value;
                PurchasingService.InsertOrUpdatePurchaseOrderHeader(po, testUser, PurchasingService.LockPurchaseOrderHeader(po));
            }
            task.CheckCompanyForPassedUnpackDates(testCompany);
            actual = service.GetQueueCount();
            Assert.IsTrue(actual == expected, $"Error: Email Count returned {actual} when {expected} was expected");

            // Create POs with Data < now
            foreach (var value in Enum.GetValues(typeof(PurchaseOrderStatus))) {
                var po = GetTestPurchaseOrderHeader(testCompany, testUser);
                po.RealisticRequiredDate = DateTime.Now.AddDays((int)value-100);
                po.POStatus = (int)value;
                PurchasingService.InsertOrUpdatePurchaseOrderHeader(po, testUser, PurchasingService.LockPurchaseOrderHeader(po));
            }
            expected = 9;
            task.CheckCompanyForPassedUnpackDates(testCompany);
            actual = service.GetQueueCount();
            Assert.IsTrue(actual == expected, $"Error: Email Count returned {actual} when {expected} was expected");
        }
    }
}
