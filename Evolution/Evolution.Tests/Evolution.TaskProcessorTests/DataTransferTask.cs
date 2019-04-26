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
    public partial class DataTransferTaskTests : BaseTest {
        [TestMethod]
        public void GetTaskNameTest() {
            var task = new DataTransferTask(db);
            string expected = TaskName.DataTransferTask,
                   actual = task.GetTaskName();
            Assert.IsTrue(actual == expected, $"Error: GetTaskName() returned {actual} when {expected} was expected. Check that the derived task class overrides the GetTaskName() method");
        }

        [TestMethod]
        public void DoProcessingTest() {
            // Get a test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            //CreateTestTransfers(testCompany, testUser);
            var testLocation = LookupService.FindLocationModel(testCompany.DefaultLocationID.Value);

            // Create some test transfers
            var testFolder = Path.GetTempPath() + RandomString();
            LogTestFolder(testFolder);
            Directory.CreateDirectory(testFolder);

            TestFileTransfer config = new TestFileTransfer(FileTransferDataType.WarehousePick, "Warehouse_Picks.xml", "{PICKNO}.{EXTN}");
            var sendConfig = GetTestTransfer(testCompany, testUser, testLocation, FileTransferType.Send, FTPProtocol.FTP, testFolder, "/test/Evolution/", @"\Development\Evolution\DataTransfers\Test\Archive", @"\Development\Evolution\DataTransfers\Test\Error", config);
            var receiveConfig = GetTestTransfer(testCompany, testUser, testLocation, FileTransferType.Receive, FTPProtocol.FTP, "/test/Evolution/", testFolder, "", "", config);

            // Create some test files
            int numFiles = 6;
            List<string> fileList = new List<string>();

            for(int i = 0; i < numFiles; i++) {
                var fileName = sendConfig.SourceFolder + "\\" + RandomString() + ".txt";
                File.WriteAllText(fileName, LorumIpsum());
                fileList.Add(fileName);
            }

            // Upload the files
            var task = new DataTransferTask(db);
            string[] args = { TaskName.DataTransferTask,
                              sendConfig.TransferName };
            task.Run(args);

            // Check that the local files have been deleted
            foreach(var fileName in fileList) {
                Assert.IsTrue(!File.Exists(fileName), $"Error: File {fileName} was found in {testFolder} when it was expected to be deleted after sending");
            }

            // Download the files
            args = new string[] { TaskName.DataTransferTask,
                                  receiveConfig.TransferName };
            task.Run(args);

            // Check that the files exist
            foreach (var fileName in fileList) {
                Assert.IsTrue(File.Exists(fileName), $"Error: File {fileName} was not found in {testFolder} when it was expected to be");
            }

            // Check that the files have been deleted on the FTP site
            FTPService.FTPService ftpService = FTPService(FTPProtocol.FTP);

            List<string> foundList = new List<string>();
            var errorMsg = "";

            bool bError = ftpService.GetFTPFileList(receiveConfig.SourceFolder, ref foundList, ref errorMsg);
            if (bError) {
                foreach (string fileName in foundList) {
                    LogTestFile(fileName);
                    ftpService.DeleteFile(receiveConfig.SourceFolder + "/" + fileName.FileName(), ref errorMsg);
                }
            }
            Assert.IsTrue(!bError, errorMsg);
            Assert.IsTrue(foundList.Count() == 0, $"Error: {foundList.Count()} files were found on the remote server when 0 were expected");
        }

        private FTPService.FTPService FTPService(FTPProtocol protocol) {
            FTPService.FTPService ftpService = new FTPService.FTPService(GetAppSetting("Host", "", protocol),
                                                                         GetAppSetting("Login", "", protocol),
                                                                         GetAppSetting("Password", "", protocol),
                                                                         protocol);
            return ftpService;
        }
    }
}
