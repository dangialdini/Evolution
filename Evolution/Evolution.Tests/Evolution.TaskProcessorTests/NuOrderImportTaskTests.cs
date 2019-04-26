using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.TaskProcessor;
using System.IO;

namespace Evolution.TaskProcessorTests {
    [TestClass]
    public partial class NuOrderImportTaskTests : BaseTest {
        [TestMethod]
        public void GetTaskNameTest() {
            var task = new PepperiImportTask(db);
            string expected = TaskName.PepperiImportTask;
            string actual = task.GetTaskName();
            Assert.IsTrue(actual == expected, $"Error: GetTaskName() return {actual} when {expected} was expected. Check that the derived task class overrides the GetTaskName() method");
        }

        [TestMethod]
        public void DoProcessingTest() {
            // NOTE: DataTransfer & Scheduled Task both need to be created and setup in order to run this test
            //       Expecting 3 files to be moved to the Archive Folder

            var testFolder = @"C:\Development\Evolution\Evolution\Evolution.TaskProcessor\NuOrderImportTestData";
            var testFiles = Directory.GetFiles(testFolder);

            // Clean up the Root Directory
            if (testFiles.Length > 0) {
                foreach(var file in testFiles) {
                    var fileName = Path.GetFileName(file);
                    var fullFileName = Path.Combine(testFolder, fileName);
                    File.Delete(fullFileName);
                }
            }

            // Clean up the Archive and Error folders
            var sourceDirectory = Directory.GetDirectories(testFolder);
            foreach (var folder in sourceDirectory) {
                var filesInFolder = Directory.GetFiles(folder);
                if(filesInFolder.Length > 0) {
                    foreach(var file in filesInFolder) {
                        var fileName = Path.GetFileName(file);
                        var fullFileName = Path.Combine(folder, fileName);
                        File.Delete(fullFileName);
                    }
                }
            }

            // Copy files from the SOURCE OF TRUTH into the testFolder
            string[] sourceFiles = Directory.GetFiles(@"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.NuOrderImportServiceTests\TestData");
            foreach(var file in sourceFiles) {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(testFolder, fileName);
                File.Copy(file, destFile);
            }

            // Run the task
            var task = new NuOrderImportTask(db);
            task.Run(null);

            // Check all files have been moved out of the testFolder
            testFiles = Directory.GetFiles(testFolder);
            var expected = 0;
            var actual = testFiles.Length;
            Assert.IsTrue(actual == expected, $"Error: {actual} number of files found when {expected} were expected");

            // Check the correct number of files have been moved to the Archive folder
            var archiveTestFiles = Directory.GetFiles(testFolder + "\\Archive");
            expected = 3;
            actual = archiveTestFiles.Length;
            Assert.IsTrue(actual == expected, $"Error: {actual} number of files found when {expected} were expected");
        }

        [TestMethod]
        public void IncorrectProductNumberMoveToErrorFolderTest() {
            // NOTE:    DataTransfer & Scheduled Task both need to be created and setup in order to run this test
            //          Expecting 2 files, both with errors to be moved to the Error Folder

            var testFolder = @"C:\Development\Evolution\Evolution\Evolution.TaskProcessor\NuOrderImportTestData";
            var testFiles = Directory.GetFiles(testFolder);

            // Clean up the Root Directory
            if(testFiles.Length > 0) {
                foreach(var file in testFiles) {
                    var fileName = Path.GetFileName(file);
                    var fullFileName = Path.Combine(testFolder, fileName);
                    File.Delete(fullFileName);
                }
            }

            // Clean up the Archive and Error folders
            var sourceDirectory = Directory.GetDirectories(testFolder);
            foreach (var folder in sourceDirectory) {
                var filesInFolder = Directory.GetFiles(folder);
                if (filesInFolder.Length > 0) {
                    foreach (var file in filesInFolder) {
                        var fileName = Path.GetFileName(file);
                        var fullFileName = Path.Combine(folder, fileName);
                        File.Delete(fullFileName);
                    }
                }
            }

            // Copy files from the SOURCE OF TRUTH into the testFolder
            string[] sourceFiles = Directory.GetFiles(@"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.NuOrderImportServiceTests\TestData\IncorrectProductNumber");
            foreach (var file in sourceFiles) {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(testFolder, fileName);
                File.Copy(file, destFile);
            }

            // Run the task
            var task = new NuOrderImportTask(db);
            task.Run(null);

            // Check all files have been moved out of the testFolder
            testFiles = Directory.GetFiles(testFolder);
            var expected = 0;
            var actual = testFiles.Length;
            Assert.IsTrue(actual == expected, $"Error: {actual} number of files found when {expected} were expected");

            // Check the correct number of files have been moved to the Error folder
            var errorTestFiles = Directory.GetFiles(testFolder + "\\Error");
            expected = 2;
            actual = errorTestFiles.Length;
            Assert.IsTrue(actual == expected, $"Error: {actual} number of files found when {expected} were expected");
        }

    }
}
