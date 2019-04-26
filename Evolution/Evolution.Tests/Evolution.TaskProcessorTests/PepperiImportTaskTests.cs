using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.TaskProcessor;
using System.IO;

namespace Evolution.TaskProcessorTests {
    [TestClass]
    public partial class PepperiImportTaskTests : BaseTest {
        [TestMethod]
        public void GetTaskNameTest() {
            var task = new PepperiImportTask(db);
            string expected = TaskName.PepperiImportTask;
            string actual = task.GetTaskName();
            Assert.IsTrue(actual == expected, $"Error: GetTaskName() return {actual} when {expected} was expected. Check that the derived task class overrides the GetTaskName() method");
        }

        [TestMethod]
        public void DoProcessingTest() {
            var testFolder = @"C:\Development\Evolution\Evolution\Evolution.TaskProcessor\PepperiImportTestData";
            var testFiles = Directory.GetFiles(testFolder);

            // Clean up the Root Directory
            if(testFiles.Length > 0) {
                foreach(var file in testFiles) {
                    var fileName = Path.GetFileName(file);
                    var fullFileName = Path.Combine(testFolder, fileName);
                    File.Delete(fullFileName);
                }
            }

            var sourceDirectory = Directory.GetDirectories(testFolder);

            // Clean up the Archive and Error folders
            foreach (var folder in sourceDirectory) {
                var filesInFoler = Directory.GetFiles(folder);
                if (filesInFoler.Length > 0) {
                    foreach (var file in filesInFoler) {
                        var fileName = Path.GetFileName(file);
                        var fullFileName = Path.Combine(folder, fileName);
                        File.Delete(fullFileName);
                    }
                }
            }

            // Copy files from the SOURCE OF TRUTH into our testFolder
            string[] sourceFiles = Directory.GetFiles(@"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.PepperiImportServiceTests\TestData");
            foreach(var file in sourceFiles) {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(testFolder, fileName);
                File.Copy(file, destFile);
            }

            // Run the task
            var task = new PepperiImportTask(db);
            task.Run(null);

            // Check all files have been moved out of the testFolder
            testFiles = Directory.GetFiles(testFolder);
            var expected = 0;
            var actual = testFiles.Length;
            Assert.IsTrue(actual == expected, $"Error: {actual} number of files found, when {expected} were expected");

            // Check the correct number of files have been moved into the Archive folder
            var archiveTestFile = Directory.GetFiles(testFolder + "\\Archive");
            expected = 3;
            actual = archiveTestFile.Length;
            Assert.IsTrue(actual == expected, $"Error: {actual} number of files found, when {expected} were expected");

            // Check the correct number of files have been moved into the Error folder
            var errorFiles = Directory.GetFiles(testFolder + "\\Error");
            expected = 2;
            actual = errorFiles.Length;
            Assert.IsTrue(actual == expected, $"Error: {actual} number of files found, when {expected} were expected");
        }
    }
}
