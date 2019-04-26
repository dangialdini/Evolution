using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.TaskProcessor;
using Evolution.NuOrderImportService;
using System.IO;

namespace Evolution.NuOrderImportServiceTest {
    [TestClass]
    public partial class NuOrderImportServiceTest : BaseTest {

        [TestMethod]
        public void ProcessFileTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Setup a Task
            var fit = new FileImportTask(db);
            var testTask = fit.StartTask();

            // File location for Test Data
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.NuOrderImportServiceTest\TestData";
            var files = Directory.GetFiles(testLocation);

            NuOrderImportService.NuOrderImportService service = new NuOrderImportService.NuOrderImportService(db);
            foreach(var file in files) {
                var orderLines = service.ProcessFile(file, testCompany.AccountName, testTask);
                Assert.IsTrue(orderLines != null, "Error: ");
            }
        }

        [TestMethod]
        public void MapFileToTempTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Setup a Task
            var fit = new FileImportTask(db);
            var testTask = fit.StartTask();

            // File location for Test Data
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.NuOrderImportServiceTest\TestData";
            var files = Directory.GetFiles(testLocation);

            NuOrderImportService.NuOrderImportService service = new NuOrderImportService.NuOrderImportService(db);
            foreach (var file in files) {
                var orderLines = service.ProcessFile(file, testCompany.AccountName, testTask);
                foreach(var order in orderLines) {
                    NuOrderImportTemp nuOrderImportTemp = service.MapFileToTemp(testCompany.AccountName, order, testTask);

                }
                
            }
        }

    }
}
