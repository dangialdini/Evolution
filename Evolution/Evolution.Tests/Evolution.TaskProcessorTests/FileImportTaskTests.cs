using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.TaskProcessor;

namespace Evolution.TaskProcessorTests
{
	[TestClass]
    public class FileImportTaskTests : BaseTest
    {
        [TestMethod]
        public void GetTaskNameTest() {
            var task = new FileImportTask(db);
            string expected = TaskName.FileImportTask,
                   actual = task.GetTaskName();
            Assert.IsTrue(actual == expected, $"Error: GetTaskName() returned {actual} when {expected} was expected. Check that the derived task class ovverides the GetTaskName() method.");
        }

		[TestMethod]
        public void DoProcessingTest()
        {
                        
        }
    }
}
