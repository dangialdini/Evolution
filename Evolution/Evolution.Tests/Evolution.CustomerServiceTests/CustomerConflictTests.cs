using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;

namespace Evolution.CustomerServiceTests {
    public partial class CustomerServiceTests {
        [TestMethod]
        public void FindCustomerConflictsListModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var cust1 = GetTestCustomer(testCompany, testUser);
            var cust2 = GetTestCustomer(testCompany, testUser);

            // Add conflicts
            var conf1 = createCustomerConflict(testCompany.Id, testCustomer.Id, testUser, cust1.Id);
            var conf2 = createCustomerConflict(testCompany.Id, testCustomer.Id, testUser, cust2.Id);

            // Search for the conflicts
            var result = CustomerService.FindCustomerConflictsListModel(testCustomer.Id, 0, 1, PageSize)
                                        .Items
                                        .OrderBy(c => c.Id)
                                        .ToList();
            int expectedResult = 2;
            Assert.IsTrue(result.Count == expectedResult, $"Error: {result.Count} records were found when {expectedResult} were expected");
            Assert.IsTrue(result[0].Id == conf1.Id || result[0].Id == conf2.Id, $"Error: Conflict {result[0].Id} was returned when {conf1.Id} was expected");
            Assert.IsTrue(result[1].Id == conf1.Id || result[1].Id == conf2.Id, $"Error: Conflict {result[1].Id} was returned when {conf2.Id} was expected");
        }

        [TestMethod]
        public void FindCustomerConflictModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var cust1 = GetTestCustomer(testCompany, testUser);

            // Add conflict and try to retrieve it
            var conf1 = createCustomerConflict(testCompany.Id, testCustomer.Id, testUser, cust1.Id);

            var result = CustomerService.FindCustomerConflictModel(conf1.Id, testCompany, testCustomer);
            Assert.IsTrue(result != null, $"Error: 0 records were returned when 1 was expected");
            Assert.IsTrue(result.Id == conf1.Id, $"Error: Customer {result.Id} was returned when {conf1.Id} was expected");
            AreEqual(conf1, result);

            // Now delete it and try to retrieve it
            CustomerService.DeleteCustomerConflict(conf1.Id);

            result = CustomerService.FindCustomerConflictModel(conf1.Id, testCompany, testCustomer, false);
            Assert.IsTrue(result == null, $"Error: 1 record was returned when 0 were expected");
        }

        [TestMethod]
        public void InsertOrUpdateCustomerConflictTest() {
            // Tested by FindCustomerConflictModelTest
        }

        [TestMethod]
        public void DeleteCustomerConflictTest() {
            // Tested by FindCustomerConflictModelTest
        }

        [TestMethod]
        public void LockCustomerConflictTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var cust1 = GetTestCustomer(testCompany, testUser);

            // Add conflict and try to retrieve it
            var conf1 = createCustomerConflict(testCompany.Id, testCustomer.Id, testUser, cust1.Id);

            // Get the current Lock
            string lockGuid = CustomerService.LockCustomerConflict(conf1);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = CustomerService.InsertOrUpdateCustomerConflict(conf1, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = CustomerService.InsertOrUpdateCustomerConflict(conf1, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = CustomerService.LockCustomerConflict(conf1);
            error = CustomerService.InsertOrUpdateCustomerConflict(conf1, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Delete the record and check that the lock was deleted
            int tempId = conf1.Id;
            CustomerService.DeleteCustomerConflict(tempId);
            var lockRecord = db.FindLock(typeof(CustomerConflictSensitivity).ToString(), tempId);
            // The following is used because Assert.IsNull tried to evaluate the error message when lockrecord was NULL - its shouldn't!
            if (lockRecord != null) Assert.Fail($"Error: Lock record {lockRecord.Id} was returned when none were expected");
        }

        CustomerConflictModel createCustomerConflict(int companyId, int customerId, UserModel user, int conflictWithId) {
            CustomerConflictModel model = new CustomerConflictModel {
                CompanyId = companyId,
                CustomerId = customerId,
                SensitiveWithId = conflictWithId
            };
            var error = CustomerService.InsertOrUpdateCustomerConflict(model, user, "");

            Assert.IsFalse(error.IsError, error.Message);

            return model;
        }
    }
}
