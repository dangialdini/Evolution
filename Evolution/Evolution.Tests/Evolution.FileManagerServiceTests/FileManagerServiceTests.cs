using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.FileManagerServiceTests {

    [TestClass]
    public class FileManagerServiceTests : BaseTest {
        [TestMethod]
        public void CreateFolderTest() {
            // Tested in MoveFileTest
        }

        [TestMethod]
        public void DeleteFileTest() {
            // Tested in MoveFileTest
        }

        [TestMethod]
        public void MoveFileTest() {
            // Get the temp folder
            string folderName1 = Path.GetTempPath();

            var error = FileManagerService.FileManagerService.CreateFolder(folderName1);
            Assert.IsTrue(!error.IsError, error.Message);

            // Create a file in it
            string fileName1 = folderName1 + "\\" + Guid.NewGuid() + ".txt";
            LogTestFile(fileName1);

            File.WriteAllText(fileName1, Guid.NewGuid().ToString());
            Assert.IsTrue(File.Exists(fileName1), $"Error: File {fileName1} could not be found. Check that the folder and file exists");

            // Create another folder
            string folderName2 = Path.GetTempPath() + "\\Test";
            LogTestFolder(folderName2);
            Directory.CreateDirectory(folderName2);

            string fileName2 = folderName2 + "\\" + fileName1.FileName();
            LogTestFile(fileName2);

            // Move the temp file to it
            error = FileManagerService.FileManagerService.MoveFile(fileName1, folderName2, true);
            Assert.IsTrue(!File.Exists(fileName1), $"Error: File {fileName1} was found when it should have been moved and deleted");
            Assert.IsTrue(File.Exists(fileName2), $"Error: File {fileName2} could not be found. Check that the folder and file exists");

            // Delete it
            FileManagerService.FileManagerService.DeleteFile(fileName2);
            Assert.IsTrue(!File.Exists(fileName2), $"Error: File {fileName2} could not be found. Check that the folder and file exists");

            // Cleanup
            try {
                Directory.Delete(folderName2);
            } catch { }
        }
    }
}
