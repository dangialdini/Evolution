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

namespace Evolution.TaskProcessorTests {
    [TestClass]
    public partial class HouseKeepingTaskTests : BaseTest {
        [TestMethod]
        public void GetTaskNameTest() {
            var task = new HouseKeepingTask(db);
            string expected = TaskName.HouseKeepingTask,
                   actual = task.GetTaskName();
            Assert.IsTrue(actual == expected, $"Error: GetTaskName() returned {actual} when {expected} was expected. Check that the derived task class overrides the GetTaskName() method");
        }

        [TestMethod]
        public void DoProcessingTest() {
            var app = new TaskProcessorApplication();

            string[] args = { TaskName.HouseKeepingTask };
            app.Run(args);
        }
    }
}
