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
    public partial class TaskBaseTests : BaseTest {
        [TestMethod]
        public void GetTaskNameTest() {
            // This method is overwridden by all derived tasks
        }

        [TestMethod]
        public void DoProcessingTest() {
            // This method is overwridden by all derived tasks
        }

        [TestMethod]
        public void RunTest() {
            // Tested by all task tests
        }

        [TestMethod]
        public void StartTaskTest() {
            // Tested by all task tests
        }

        [TestMethod]
        public void EndTaskTest() {
            // Tested by all task tests
        }

        [TestMethod]
        public void WriteLogTest() {
            // Tested by all task tests
        }

        [TestMethod]
        public void WriteTaskLogTest() {
            // Tested by all task tests
        }
    }
}
