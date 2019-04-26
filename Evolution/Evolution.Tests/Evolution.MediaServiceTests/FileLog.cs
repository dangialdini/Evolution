using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.MediaService;
using Evolution.NoteService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using System.Drawing;

namespace Evolution.MediaServiceTests {
    public partial class MediaServiceTests : BaseTest {

        [TestMethod]
        public void AddFileToLogTest() {
            int folderDeletes = 0,
                fileDeletes = 0;

            // Clean the log to a known state
            MediaServices.CleanFileLog(ref folderDeletes, ref fileDeletes, true);

            // Create a temp folder
            string tempFolder = Path.GetTempPath() + Guid.NewGuid().ToString();
            MediaServices.AddFolderToLog(tempFolder, 30);
            Directory.CreateDirectory(tempFolder);

            string tempFile = tempFolder + "\\" + Guid.NewGuid().ToString() + ".txt";
            MediaServices.AddFileToLog(tempFile, 30);
            File.WriteAllText(tempFile, LorumIpsum());

            Assert.IsTrue(File.Exists(tempFile), "Error: File {tempFile} could not be found");

            // Clean the log - files/folders are to be kept for 30 mins, so shouldn't be deleted
            MediaServices.CleanFileLog(ref folderDeletes, ref fileDeletes, false);
            int expected = 0;
            Assert.IsTrue(folderDeletes == expected, $"Error: {folderDeletes} folders were deleted when {expected} were expected");
            Assert.IsTrue(fileDeletes == expected, $"Error: {fileDeletes} files were deleted when {expected} were expected");
            Assert.IsTrue(File.Exists(tempFile), "Error: File {tempFile} could not be found");

            // Change the log times
            foreach(var log in db.FindFileLogs(true)) {
                log.DeleteAfterDate = log.DeleteAfterDate.AddMinutes(-31);
                db.InsertOrUpdateFileLog(log);
            }

            // Clean the log, taking advantage of the timeout period
            MediaServices.CleanFileLog(ref folderDeletes, ref fileDeletes, false);
            expected = 1;
            Assert.IsTrue(folderDeletes == expected, $"Error: {folderDeletes} folders were deleted when {expected} were expected");
            Assert.IsTrue(fileDeletes == expected, $"Error: {fileDeletes} files were deleted when {expected} were expected");
            Assert.IsTrue(!File.Exists(tempFile), $"Error: File {tempFile} was found when it should have been deleted");
            Assert.IsTrue(!File.Exists(tempFolder), $"Error: Folder {tempFolder} was found when it should have been deleted");
        }

        [TestMethod]
        public void AddFolderToLogTest() {
            // Tested in AddFileToLogTest
        }

        [TestMethod]
        public void CleanFileLogTest() {
            // Tested in AddFileToLogTest
        }
    }
}
