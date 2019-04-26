using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using CommonTest;
using Evolution.TaskManagerService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.TaskManagerServiceTests {
    [TestClass]
    public class TaskManagerServiceTests : BaseTest {
        [TestMethod]
        public void FindTaskListModelTest() {
            // Get a test user and test company
            int tz = 600;
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            TaskManagerService.TaskManagerService taskManagerService = new TaskManagerService.TaskManagerService(db, testCompany);

            // Now create an notification and try to search for it
            string  subject = RandomString(),
                    message = LorumIpsum();

            var result = taskManagerService.SendTask(subject,
                                                     message,
                                                     TaskType.Default,
                                                     LookupService.FindLOVItemsModel(testCompany, LOVName.BusinessUnit).FirstOrDefault(),
                                                     testUser, 
                                                     null,
                                                     null);
            Assert.IsTrue(!result.IsError, result.Message);

            // Perform a global test which should bring back the first page
            var notificationList = taskManagerService.FindTaskListModel(testCompany, testUser, 0, 1, PageSize,
                                                                        0,
                                                                        0,
                                                                        0,
                                                                        0,
                                                                        0,
                                                                        "",
                                                                        tz);
            int actual = notificationList.Items
                                         .Where(n => n.Title == subject)
                                         .Count();
            int expected = 1;
            Assert.IsTrue(actual == expected, $"Error: {actual} records were found when {expected} was expected");

            // Delete and check again
            taskManagerService.DeleteTask(result.Id);

            notificationList = taskManagerService.FindTaskListModel(testCompany, testUser, 0, 1, PageSize,
                                                                    0,
                                                                    0,
                                                                    0,
                                                                    0,
                                                                    0,
                                                                    "",
                                                                    tz);
            actual = notificationList.Items
                                     .Where(n => n.Title == subject)
                                     .Count();
            expected = 0;
            Assert.IsTrue(actual == expected, $"Error: {actual} records were found when {expected} was expected");
        }

        [TestMethod]
        public void FindTaskModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            TaskManagerService.TaskManagerService taskManagerService = new TaskManagerService.TaskManagerService(db, testCompany);

            // Now create a task and try to search for it
            var testTask = createTestTask(testCompany, testUser);
            var error = taskManagerService.InsertOrUpdateTask(testTask, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var result = taskManagerService.FindTaskModel(testTask.Id, testCompany, testUser, false);
            Assert.IsTrue(result != null, $"Error: 0 records were returned when 1 was expected");
            Assert.IsTrue(result.Id == testTask.Id, $"Error: Supplier {result.Id} was returned when {testTask.Id} was expected");
            AreEqual(testTask, result);
        }

        [TestMethod]
        public void MapToModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            TaskManagerService.TaskManagerService taskManagerService = new TaskManagerService.TaskManagerService(db, testCompany);

            // Create a task
            var testTask = new TaskModel {
                CompanyId = testCompany.Id,
                CreatedDate = DateTimeOffset.Now,
                TaskTypeId = db.FindLOVItemByValue1(null, LOVName.TaskType, ((int)TaskType.MSQChangeNotification).ToString()).Id,
                BusinessUnitId = db.FindLOVItems(testCompany.Id, db.FindLOV(LOVName.BusinessUnit).Id)
                               .FirstOrDefault()
                               .Id,
                UserId = testUser.Id,
                Title = RandomString(),
                Description = RandomString(),
                StatusId = db.FindLOVItems(testCompany.Id,
                                           db.FindLOV(LOVName.TaskStatus).Id)
                                             .FirstOrDefault()
                                             .Id,
                Enabled = true
            };

            var model = taskManagerService.MapToModel(testTask);
            AreEqual(testTask, model);
        }

        [TestMethod]
        public void InsertOrUpdateTaskTest() {
            // Tested in FindTaskModelTest()
        }

        [TestMethod]
        public void DeleteTaskTest() {
            // Tested in FindTaskListModelTest() above
        }

        [TestMethod]
        public void LockTaskTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            TaskManagerService.TaskManagerService taskManagerService = new TaskManagerService.TaskManagerService(db, testCompany);

            // Create a task
            var model = createTestTask(testCompany, testUser);
            var error = taskManagerService.InsertOrUpdateTask(model, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Get the current Lock
            string lockGuid = taskManagerService.LockTask(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = taskManagerService.InsertOrUpdateTask(model, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = taskManagerService.InsertOrUpdateTask(model, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = taskManagerService.LockTask(model);
            error = taskManagerService.InsertOrUpdateTask(model, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void SendTaskTest() {
            // Tested in FindTaskListModelTest() above
        }

        [TestMethod]
        public void AddOrganisationDetailsTest() {
            // Tested in all SendNorificationTests as SendTask calls TaskManagerService.AddOrganisationDetails
        }

        [TestMethod]
        public void AddUserDetailsTest() {
            var testUser = GetTestUser(true, false);

            Dictionary<string, string> dict = new Dictionary<string, string>();

            int expected = 0,
                actual = dict.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            TaskManagerService.TaskManagerService taskManagerService = new TaskManagerService.TaskManagerService(db);
            taskManagerService.AddUserDetails(testUser, dict);

            expected = 2;
            actual = dict.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            string keyName = "USERNAME";
            try {
                var test = dict[keyName].ToString();
                Assert.IsTrue(testUser.FullName == test, $"Error: {test} was returned when {testUser.FullName} was expected");
            } catch {
                Assert.Fail($"Error: Item '{keyName}' was not found in the dictionary");
            }

            keyName = "EMAIL";
            try {
                var test = dict[keyName].ToString();
                Assert.IsTrue(testUser.EMail == test, $"Error: {test} was returned when {testUser.EMail} was expected");
            } catch {
                Assert.Fail($"Error: Item '{keyName}' was not found in the dictionary");
            }
        }

        [TestMethod]
        public void GetTaskCountTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            TaskManagerService.TaskManagerService taskManagerService = new TaskManagerService.TaskManagerService(db, testCompany);

            // Now create a task and try to search for it
            int expected = taskManagerService.GetTaskCount();   // Start count

            for (int i = 0; i < 10; i++) {
                int rand = RandomInt(5, 20);
                for(int j = 0; j < rand; j++) {
                    var testTask = createTestTask(testCompany, testUser);
                    var error = taskManagerService.InsertOrUpdateTask(testTask, "");
                    Assert.IsTrue(!error.IsError, error.Message);
                }
                expected += rand;

                int actual = taskManagerService.GetTaskCount();
                Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected ");
            }
        }

        private TaskModel createTestTask(CompanyModel testCompany, UserModel testUser) {
            var businessUnit = db.FindLOVItems(testCompany.Id, db.FindLOV(LOVName.BusinessUnit).Id)
                                 .FirstOrDefault();
            var status = db.FindLOVItems(testCompany.Id,
                                         db.FindLOV(LOVName.TaskStatus).Id)
                           .FirstOrDefault();
            var taskType = db.FindLOVItemByValue1(null, LOVName.TaskType, ((int)TaskType.MSQChangeNotification).ToString());

            var testTask = new TaskModel {
                CompanyId = testCompany.Id,
                CreatedDate = DateTimeOffset.Now,
                TaskTypeId = taskType.Id,
                TaskTypeText = taskType.ItemText,
                BusinessUnitId = businessUnit.Id,
                BusinessUnit = businessUnit.ItemText,
                UserId = testUser.Id,
                AssigneeName = testUser.FullName,
                Title = RandomString(),
                Description = RandomString(),
                StatusId = status.Id,
                StatusText = status.ItemText,
                StatusColour = status.Colour,
                Enabled = true
            };
            return testTask;
        }
    }
}
