using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.CustomerService;
using Evolution.Models.Models;
using Evolution.Enumerations;

namespace Evolution.AuditServiceTests {
    [TestClass]
    public class AuditServiceTests : BaseTest {
        [TestMethod]
        public void LogChangesTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Check the log
            var auditList = AuditService.FindAuditListModel(typeof(Company).ToString(), testCompany.Id);
            int expected = 2,
                actual = auditList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} were expected");
            Assert.IsTrue(auditList.Items[0].AfterValue == "Record created", $"Error: The log message is '{auditList.Items[0].AfterValue}' when 'Record created' was expected");

            // Create a new customer
            var testCustomer = GetTestCustomer(testCompany, testUser);
            auditList = AuditService.FindAuditListModel(typeof(Customer).ToString(), testCustomer.Id);
            expected = 1;
            actual = auditList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} were expected");
            Assert.IsTrue(auditList.Items[0].AfterValue == "Record created", $"Error: The log message is '{auditList.Items[0].AfterValue}' when 'Record created' was expected");

            // Change the customer name
            string newName = "Changed Name";
            testCustomer.Name = newName;
            string lgs = CustomerService.LockCustomer(testCustomer);
            CustomerService.InsertOrUpdateCustomer(testCustomer, testUser, lgs);
            auditList = AuditService.FindAuditListModel(typeof(Customer).ToString(), testCustomer.Id);
            expected = 2;
            actual = auditList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} were expected");
            Assert.IsTrue(auditList.Items[0].AfterValue == "Record created", $"Error: The log message is '{auditList.Items[0].AfterValue}' when 'Record created' was expected");
            Assert.IsTrue(auditList.Items[1].AfterValue == "Changed Name", $"Error: The log message is '{auditList.Items[1].AfterValue}' when '{newName}' was expected");
        }

        [TestMethod]
        public void FindAuditListModelTest() {
            // Tested in LogChangesTest above
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }

        private void logChanges(Customer before, Customer after, UserModel user) {
            AuditService.LogChanges(typeof(Customer).ToString(), BusinessArea.ProductDetails, user, before, after);
        }
    }
}
