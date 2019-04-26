using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.SalesService;
using Evolution.Enumerations;
using Evolution.EMailService;
using Evolution.TaskManagerService;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests {
        [TestMethod]
        public void SendMSQOverrideEMailTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var soh = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, RandomInt(5, 50));

            EMailService.EMailService EMailService = GetEMailService(testCompany);
            TaskManagerService.TaskManagerService TaskManagerService = GetTaskManagerService(testCompany);

            int emailQBefore = EMailService.GetQueueCount(),
                notificationsBefore = TaskManagerService.GetTaskCount();

            var testUser2 = GetTestUser();
            var error = SalesService.SendMSQOverrideEMail(testCompany, testUser, testUser2, soh);

            int emailQAfter = EMailService.GetQueueCount(),
                notificationsAfter = TaskManagerService.GetTaskCount();

            int actual = emailQAfter - emailQBefore,
                expected = 1;
            Assert.IsTrue(actual == expected, $"Error: {actual} EMail items were returned when {expected} were expected");

            actual = notificationsAfter - notificationsBefore;
            expected = 1;
            Assert.IsTrue(actual == expected, $"Error: {actual} Notifications were returned when {expected} were expected");
        }
    }
}
