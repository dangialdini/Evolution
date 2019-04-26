using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.TaskService;
using Evolution.Enumerations;

namespace Evolution.TaskServiceTests {
    [TestClass]
    public partial class TaskServiceTests : BaseTest {

        private TaskService.TaskService _taskService = null;
        protected TaskService.TaskService TaskService {
            get {
                if (_taskService == null) _taskService = new TaskService.TaskService(db);
                return _taskService;
            }
        }
    }
}