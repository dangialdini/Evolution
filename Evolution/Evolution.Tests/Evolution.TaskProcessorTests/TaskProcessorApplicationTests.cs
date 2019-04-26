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
    public partial class TaskProcessorApplicationTests : BaseTest {
        [TestMethod]
        public void RunTest() {
            // Tested by task tests which run via TaskProcessorApplication
        }

        [TestMethod]
        public void WriteLogTest() {
            // Tested by task tests which run via TaskProcessorApplication
        }
    }
}
