using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.TaskService;
using Evolution.Models.Models;
using Evolution.Enumerations;

namespace Evolution.TaskServiceTests {
    public partial class TaskServiceTests {

        #region Task maintenence tests

        [TestMethod]
        public void FindScheduledTasksListModelTest() {
            var testUser = GetTestUser();
            var model = TaskService.FindScheduledTasksListModel(0, 1, PageSize, "");
            var dbData = db.FindScheduledTasks();

            int expected = dbData.Count(),
                actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = TaskService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createScheduledTask(testUser);
            var error = TaskService.InsertOrUpdateScheduledTask(newItem, testUser, TaskService.LockScheduledTask(newItem));
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = TaskService.FindScheduledTasksListModel(0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            TaskService.DeleteScheduledTask(newItem);

            model = TaskService.FindScheduledTasksListModel(0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindScheduledTaskModelTest() {
            var testUser = GetTestUser();

            var model = createScheduledTask(testUser);
            var error = TaskService.InsertOrUpdateScheduledTask(model, testUser, TaskService.LockScheduledTask(model));
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = TaskService.FindScheduledTaskModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateScheduledTaskTest() {
            // Tested in DeleteScheduledTaskTest
        }

        [TestMethod]
        public void DeleteScheduledTaskTest() {
            // Get a test user
            var testUser = GetTestUser();

            // Create a task
            ScheduledTaskModel model = createScheduledTask(testUser);

            var error = TaskService.InsertOrUpdateScheduledTask(model, testUser, TaskService.LockScheduledTask(model));
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindScheduledTask(model.Id);
            var test = TaskService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            TaskService.DeleteScheduledTask(model);

            // And check that is was deleted
            result = db.FindScheduledTask(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockScheduledTaskTest() {
            var testUser = GetTestUser();

            // Create a record
            var model = createScheduledTask(testUser);

            var error = TaskService.InsertOrUpdateScheduledTask(model, testUser, TaskService.LockScheduledTask(model));
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = TaskService.LockScheduledTask(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = TaskService.InsertOrUpdateScheduledTask(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = TaskService.InsertOrUpdateScheduledTask(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = TaskService.LockScheduledTask(model);
            error = TaskService.InsertOrUpdateScheduledTask(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void RunTaskTest() {
            var testUser = GetTestUser();

            // Create a disabled task
            var task = createScheduledTask(testUser, false);

            var error = TaskService.RunTask(task.Id);
            Assert.IsTrue(error.IsError, $"Error: An error response was expected as the task is in a disabled state");

            // Now enable it and try to run it
            task.TaskName = "HouseKeeping";
            task.Enabled = true;
            TaskService.InsertOrUpdateScheduledTask(task, testUser, TaskService.LockScheduledTask(task));

            error = TaskService.RunTask(task.Id);
            Assert.IsTrue(!error.IsError, error.Message);
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }

        #endregion

        #region Private methods

        private ScheduledTaskModel createScheduledTask(UserModel user, bool bEnabled = true) {
            var task = TaskService.StartTask(RandomString(), "", false);

            var newTask = TaskService.MapToModel(task);

            return newTask;
        }

        #endregion
    }
}
