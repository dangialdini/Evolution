using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.TaskService;
using Evolution.Enumerations;

namespace Evolution.TaskServiceTests {
    public partial class TaskServiceTests {

        #region Task log

        [TestMethod]
        public void FindScheduledTaskLogListModelTest() {
            var testUser = GetTestUser();

            // Create a task
            var task = createScheduledTask(testUser);

            // Check its log
            var model = TaskService.FindScheduledTaskLogListModel(0, task.Id, 1, PageSize, "", (int)LogSeverity.All, null, null);
            Assert.IsTrue(model.Items.Count() == 0, $"Error: {model.Items.Count()} items were returned when 0 were expected");

            // Write some log records
            int expected = 50;
            var dbTask = db.FindScheduledTask(task.Id);
            for (int i = 0; i < expected; i++) {
                TaskService.WriteTaskLog(dbTask, LorumIpsum());
            }

            model = TaskService.FindScheduledTaskLogListModel(0, task.Id, 1, PageSize, "", (int)LogSeverity.All, null, null);
            Assert.IsTrue(model.Items.Count() == expected, $"Error: {model.Items.Count()} items were returned when {expected} were expected");
        }

        [TestMethod]
        public void FindTaskLogTest() {
            // Tested in WriteTaskLogTest
        }

        #endregion
    }
}