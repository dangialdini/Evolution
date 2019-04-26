using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.NuOrderImportService;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Evolution.NuOrderImportServiceTests {
    [TestClass]
    public partial class NuOrderImportServiceTests : BaseTest {

        [TestMethod]
        public void ProcessFileTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // File location for Test Data
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.NuOrderImportServiceTest\TestData";
            var files = Directory.GetFiles(testLocation);

            NuOrderImportService.NuOrderImportService service = new NuOrderImportService.NuOrderImportService(db);
            foreach (var file in files) {
                var orderLines = service.ProcessFile(file, testCompany.AccountName);
                Assert.IsTrue(orderLines != null, $"Error: There are no orders in the file '{file}', when there should be at least one");
            }
        }

        [TestMethod]
        public void MapFileToTempTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var taskUser = GetTaskUser();

            // File location for Test Data
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.NuOrderImportServiceTest\TestData";
            var files = Directory.GetFiles(testLocation);

            NuOrderImportService.NuOrderImportService nuOrderImportService = new NuOrderImportService.NuOrderImportService(db);
            int expected;
            int actual;

            foreach (var fileName in files) {
                var orderLines = nuOrderImportService.ProcessFile(fileName, testCompany.AccountName);
                expected = orderLines.Count;

                List<NuOrderImportTemp> nuOrderImportTemp = nuOrderImportService.MapFileToTemp(testCompany.AccountName, orderLines, taskUser);
                actual = nuOrderImportTemp.Count;

                Assert.AreEqual(expected, actual, $"Error: {expected} number of lines in the .csv file, when {actual} were mapped.");
            }
        }

        [TestMethod]
        public void GetTempTableDataTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var taskUser = GetTaskUser();

            // File location for Test Data
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.NuOrderImportServiceTest\TestData";
            var files = Directory.GetFiles(testLocation);

            NuOrderImportService.NuOrderImportService nuOrderImportService = new NuOrderImportService.NuOrderImportService(db);
            int expected;
            int actual;

            foreach (var fileName in files) {
                var orderLines = nuOrderImportService.ProcessFile(fileName, testCompany.AccountName);
                List<NuOrderImportTemp> nuOrderImportTemp = nuOrderImportService.MapFileToTemp(testCompany.AccountName, orderLines, taskUser);
                expected = nuOrderImportTemp.Count;

                List<NuOrderImportTemp> nuOrderImportTempTableData = nuOrderImportService.GetTempTableData();
                actual = nuOrderImportTempTableData.Count;

                Assert.AreEqual(expected, actual, $"Error: {expected} number of lines in the .csv file, when {actual} were retrieved from the database.");
            }
        }

        [TestMethod]
        public void CopyTempDataToProductionTest() {
            // Tested in NoOrderImportTaskTest
        }

        [TestMethod]
        public void NoHeaderInFileTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.NuOrderImportServiceTests\TestData\NoHeader";
            var file = Directory.GetFiles(testLocation);
            NuOrderImportService.NuOrderImportService service = new NuOrderImportService.NuOrderImportService(db);

            // It is expected that this test will fail in code as there is no header in the CSV file.
            // This test will catch the failed attempt at processing the file and return a true value.
            bool errored = false;
            List<Dictionary<string, string>> orderLines = null;
            try {
                orderLines = service.ProcessFile(file[0], testCompany.AccountName);
            } catch {
                errored = true;                
            }
            Assert.IsTrue(errored == true, "Error: Call should have caused an exception but none was thrown");
        }
        
        [TestMethod]
        public void NoDetailsInFileTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var taskUser = GetTaskUser();

            // File location for Test Data
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.NuOrderImportServiceTests\TestData\NoLines";
            var file = Directory.GetFiles(testLocation);

            NuOrderImportService.NuOrderImportService service = new NuOrderImportService.NuOrderImportService(db);
            int expected = 0;
            int actual;

            var orderLines = service.ProcessFile(file[0], testCompany.AccountName);
            List<NuOrderImportTemp> nuOrderImportTempTableData = service.MapFileToTemp(testCompany.AccountName, orderLines, taskUser);
            actual = (nuOrderImportTempTableData == null || nuOrderImportTempTableData.Count == 0) ? 0 : nuOrderImportTempTableData.Count;
            Assert.IsTrue(expected == actual, $"Error: {expected} lines were expected when {actual} were found");
        }

        [TestMethod]
        public void EmptyFileTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.NuOrderImportServiceTests\TestData\Empty";
            var file = Directory.GetFiles(testLocation);
            NuOrderImportService.NuOrderImportService service = new NuOrderImportService.NuOrderImportService(db);

            // It is expected that this test will fail in code as the CSV file is empty (contains no header and no line items).
            // This test will catch the failed attempt at processing the file and return a true value.
            bool errored = false;
            List<Dictionary<string, string>> orderLines = null;
            try {
                orderLines = service.ProcessFile(file[0], testCompany.AccountName);
            } catch {
                errored = true;
            }
            Assert.IsTrue(errored == true, "Error: Call should have caused an exception but none was thrown");
        }
    }
}
