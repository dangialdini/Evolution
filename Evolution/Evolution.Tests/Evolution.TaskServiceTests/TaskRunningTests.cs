using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.TaskService;
using Evolution.Enumerations;

namespace Evolution.TaskServiceTests {
    public partial class TaskServiceTests {

        #region Task running

        [TestMethod]
        public void StartTaskTest() {

            string taskName = RandomString();

            var newTask = TaskService.StartTask(taskName);

            var testTask = db.FindScheduledTask(taskName);

            Assert.IsTrue(testTask != null, "Error: A NULL value was returned when a non-NULL value was expected");
            AreEqual(newTask, testTask);
            Assert.IsTrue(testTask.CurrentState == TaskState.Running, $"Error: The task state is {testTask.CurrentState} when it is expected to be " + TaskState.Running);
            Assert.IsTrue(testTask.Enabled == true, $"Error: The Enabled state is {testTask.Enabled} when it is expected to be True");
        }

        [TestMethod]
        public void EndTaskTest() {

            string taskName = RandomString();

            var newTask = TaskService.StartTask(taskName);
            TaskService.EndTask(newTask, 0);

            var testTask = db.FindScheduledTask(taskName);

            Assert.IsTrue(testTask != null, "Error: A NULL value was returned when a non-NULL value was expected");
            AreEqual(newTask, testTask);
            Assert.IsTrue(testTask.CurrentState == TaskState.NotRunning, $"Error: The task state is {testTask.CurrentState} when it is expected to be " + TaskState.NotRunning);
            Assert.IsTrue(testTask.Enabled == true, $"Error: The Enabled state is {testTask.Enabled} when it is expected to be True");
        }

        [TestMethod]
        public void EnableTaskTest() {

            string taskName = RandomString();

            var newTask = TaskService.StartTask(taskName);
            TaskService.EnableTask(newTask, false);

            var testTask = db.FindScheduledTask(taskName);

            Assert.IsTrue(testTask != null, "Error: A NULL value was returned when a non-NULL value was expected");
            AreEqual(newTask, testTask);
            Assert.IsTrue(testTask.CurrentState == TaskState.Disabled, $"Error: The task state is {testTask.CurrentState} when it is expected to be " + TaskState.Disabled);
            Assert.IsTrue(testTask.Enabled == false, $"Error: The Enabled state is {testTask.Enabled} when it is expected to be False");

            TaskService.EnableTask(newTask, true);

            testTask = db.FindScheduledTask(taskName);

            Assert.IsTrue(testTask != null, "Error: A NULL value was returned when a non-NULL value was expected");
            AreEqual(newTask, testTask);
            Assert.IsTrue(testTask.CurrentState == TaskState.NotRunning, "Error: The task state is {testTask.CurrentState} when it is expected to be " + TaskState.NotRunning);
            Assert.IsTrue(testTask.Enabled == true, $"Error: The Enabled state is {testTask.Enabled} when it is expected to be True");
        }

        [TestMethod]
        public void SetTaskStatusTest() {

            string taskName = RandomString();

            var newTask = TaskService.StartTask(taskName);
            string expectedValue = "Test";
            TaskService.SetTaskStatus(newTask, expectedValue);

            var testTask = db.FindScheduledTask(taskName);
            Assert.IsTrue(testTask != null, "Error: A NULL value was returned when a non-NULL value was expected");
            AreEqual(newTask, testTask);
            Assert.IsTrue(testTask.CurrentState == expectedValue, $"Error: The task state is {testTask.CurrentState} when it is expected to be '{expectedValue}'");
            Assert.IsTrue(testTask.Enabled == true, $"Error: The Enabled state is {testTask.Enabled} when it is expected to be True");

            TaskService.EndTask(newTask, 0);

            testTask = db.FindScheduledTask(taskName);

            Assert.IsTrue(testTask != null, "Error: A NULL value was returned when a non-NULL value was expected");
            AreEqual(newTask, testTask);
            Assert.IsTrue(testTask.CurrentState == TaskState.NotRunning, $"Error: The task state is {testTask.CurrentState} when it is expected to be " + TaskState.NotRunning);
            Assert.IsTrue(testTask.Enabled == true, $"Error: The Enabled state is {testTask.Enabled} when it is expected to be True");
        }

        [TestMethod]
        public void WriteTaskLogTest() {

            string taskName = RandomString();

            var newTask = TaskService.StartTask(taskName);

            for (int i = 0; i < 50; i++) {
                string message = RandomString();
                var log = TaskService.WriteTaskLog(newTask, message);

                var testLog = TaskService.FindTaskLog(log.Id);

                Assert.IsTrue(log != null, "Error: A NULL value was returned when a non-NULL value was expected");
                AreEqual(log, testLog);
            }
        }

        #endregion
    }
}
