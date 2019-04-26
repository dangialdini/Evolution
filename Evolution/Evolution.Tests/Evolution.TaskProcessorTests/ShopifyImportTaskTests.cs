using System;
using System.IO;
using CommonTest;
using Evolution.Enumerations;
using Evolution.TaskProcessor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evolution.TaskProcessorTests {
    [TestClass]
    public class ShopifyImportTaskTests : BaseTest {
        [TestMethod]
        public void GetTaskNameTest() {
            var task = new ShopifyImportTask(db);
            string expected = TaskName.ShopifyImportTask;
            string actual = task.GetTaskName();
            Assert.IsTrue(actual == expected, $"Error: GetTaskName() return {actual} when {expected} was expected. Check that the derived task class overrides the GetTaskName() method");
        }

        [TestMethod]
        public void DoProcessingTest() {
            var testFolder = @"C:\Development\Evolution\Evolution\Evolution.TaskProcessor\ShopifyImportTestData";
            var testFiles = Directory.GetFiles(testFolder);

            // Clean up the Root Directory
            if(testFiles.Length > 0) {
                foreach(var file in testFiles) {
                    var fileName = Path.GetFileName(file);
                    var fullFileName = Path.Combine(testFolder, fileName);
                    File.Delete(fullFileName);
                }
            }

            // Cleaup the Archive and Error folders
            var sourceDirectory = Directory.GetDirectories(testFolder);
            foreach(var folder in sourceDirectory) {
                var filesInFolder = Directory.GetFiles(folder);
                if(filesInFolder.Length > 0) {
                    foreach(var file in filesInFolder) {
                        var fileName = Path.GetFileName(file);
                        var fullFileName = Path.Combine(folder, fileName);
                        File.Delete(fullFileName);
                    }
                }
            }

            // Copy files from SOURCE into the testFolder
            string[] sourceFiles = Directory.GetFiles(@"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.ShopifyImportServiceTests\TestData");
            foreach(var file in sourceFiles) {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(testFolder, fileName);
                File.Copy(file, destFile);
            }

            // Run the task
            var task = new ShopifyImportTask(db);
            task.Run(null);

            // Check all files have been moved out of the testFolder
            testFiles = Directory.GetFiles(testFolder);
            var expected = 0;
            var actual = testFiles.Length;
            Assert.IsTrue(actual == expected, $"Error: {actual} number of files found when {expected} were expected");

            // Check the correct number of files have been moved to the Archive folder
            var archiveTestFiles = Directory.GetFiles(testFolder + "\\Archive");
            expected = 4;
            actual = archiveTestFiles.Length;
            Assert.IsTrue(actual == expected, $"Error: {actual} number of files found when {expected} were expected");


            // ERROR FILES TEST:
            sourceFiles = Directory.GetFiles(@"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.ShopifyImportServiceTests\TestData\IncorrectProductNumber");
            foreach (var file in sourceFiles) {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(testFolder, fileName);
                File.Copy(file, destFile);
            }
            task.Run(null);

            // Check all files have been moved out of the testFolder
            testFiles = Directory.GetFiles(testFolder);
            expected = 0;
            actual = testFiles.Length;
            Assert.IsTrue(actual == expected, $"Error: {actual} number of files found when {expected} were expected");

            // Check the correct number of files have been moved to the Error folder
            var errorTestFiles = Directory.GetFiles(testFolder + "\\Error");
            expected = 1;
            actual = errorTestFiles.Length;
            Assert.IsTrue(actual == expected, $"Error: {actual} number of files found when {expected} were expected");
        }

    }
}
