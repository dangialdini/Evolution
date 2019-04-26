using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.Extensions;
using CommonTest;
using Evolution.FTPService;
using Evolution.Enumerations;

namespace Evolution.FTPServiceTests {
    [TestClass]
    public class FTPServiceTests : BaseTest {
        [TestMethod]
        public void DownloadFileTest() {
            // Tested in UploadFileTest
        }

        [TestMethod]
        public void UploadFileTest() {
            // Do a regular FTP test - single file
            uploadFileTest(FTPProtocol.FTP, 1);

            // Now do an SFTP test - single file
            uploadFileTest(FTPProtocol.SFTP, 1);

            // Do a regular FTP test - multiple files
            uploadFileTest(FTPProtocol.FTP, 4);

            // Now do an SFTP test - multiple files
            uploadFileTest(FTPProtocol.SFTP, 4);
        }

        private void uploadFileTest(FTPProtocol protocol, int numFiles) {
            bool    bError;
            string  errorMsg = "",
                    source = "",
                    target,
                    remoteFolder = GetAppSetting("TargetFolder", "", protocol);

            // Create file(s) to upload
            List<string> sourceFiles = new List<string>();
            for (int i = 0; i < numFiles; i++) {
                source = Path.GetTempPath() + "\\" + i.ToString() + ".test";
                db.LogTestFile(source);     // So the cleanup delete it
                sourceFiles.Add(source);

                using (StreamWriter sw = new StreamWriter(source)) {
                    sw.WriteLine(LorumIpsum());
                }
            }

            // Upload the file(s)
            FTPService.FTPService ftpService = FTPService(protocol);
            if (numFiles > 1) {
                source = source.FolderName() + "\\*.test";
                target = remoteFolder;
            } else {
                target = remoteFolder + "/" + source.FileName();
            }
            bError = ftpService.UploadFile(source, target, ref errorMsg);
            Assert.IsTrue(!bError, errorMsg);

            // Delete the original files so that when we download again,
            // we know we have recreated the files
            foreach(var fileName in sourceFiles) {
                File.Delete(fileName);
                Assert.IsTrue(!File.Exists(fileName), $"Error: Failed to delete {fileName}");
            }
            
            // Now try to download the files
            if (numFiles > 1) {
                source = remoteFolder;
                target = Path.GetTempFileName().FolderName();
            } else {
                source = target;
                target = Path.GetTempFileName().FolderName() + "\\" + source.FileName();
                db.LogTestFile(target);     // So the cleanup deletes it
            }
            List<string> downloadedFiles = new List<string>();
            bError = ftpService.DownloadFile(source, target, downloadedFiles, ref errorMsg);
            LogTestFile(downloadedFiles);
            Assert.IsTrue(!bError, errorMsg);

            // Cleanup the host
            foreach (string fileName in sourceFiles) {
                bError = ftpService.DeleteFile(remoteFolder + "/" + fileName.FileName(), ref errorMsg);
                Assert.IsTrue(!bError, errorMsg);
            }

            // Check that the file exists
            foreach (string fileName in sourceFiles) {
                Assert.IsTrue(File.Exists(fileName), $"Error: Failed to find file {fileName}");
            }
        }

        [TestMethod]
        public void GetLocalFileListTest() {
            // Tested in GetFTPFileListTest() below
        }

        [TestMethod]
        public void GetFTPFileListTest() {
            // Do a regular FTP test
            getFTPFileListTest(FTPProtocol.FTP);

            // Now do an SFTP test
            getFTPFileListTest(FTPProtocol.SFTP);
        }

        void getFTPFileListTest(FTPProtocol protocol) {
            int numFiles = 10;
            string errorMsg = "";
            List<string> sourceList = new List<string>();
            List<string> foundList = new List<string>();

            FTPService.FTPService ftpService = FTPService(protocol);

            for(int i = 0; i < numFiles; i++) {
                var tempFile = GetTempFile(".ftptest2");
                using (var sw = new StreamWriter(tempFile)) {
                    sw.WriteLine(LorumIpsum());
                }
                sourceList.Add(tempFile);
            }
            sourceList = sourceList.OrderBy(sl => sl.Substring(0))
                                   .ToList();

            // Upload the test files - this uses GetLocalFileList()
            string remoteFolder = GetAppSetting("TargetFolder", "", protocol);
            bool bError = ftpService.UploadFile(sourceList[0].FolderName() + "\\*.ftptest2",
                                                remoteFolder, 
                                                ref errorMsg);
            if(bError) {
                foreach (string fileName in sourceList) {
                    ftpService.DeleteFile(remoteFolder + "/" + fileName.FileName(), ref errorMsg);
                }
            }
            Assert.IsTrue(!bError, errorMsg);

            // Now get the FTP file list
            bError = ftpService.GetFTPFileList(remoteFolder, ref foundList, ref errorMsg);
            if (bError) {
                foreach (string fileName in sourceList) {
                    ftpService.DeleteFile(remoteFolder + "/" + fileName.FileName(), ref errorMsg);
                }
            }
            Assert.IsTrue(!bError, errorMsg);
            Assert.IsTrue(foundList.Count() == numFiles, $"Error: (Protocol: {protocol}) {foundList.Count()} files were returned when {numFiles} were expected");

            foundList = foundList.OrderBy(sl => sl.Substring(0))
                                 .ToList();
            string expected = "",
                   actual = "";

            for(int i = 0; i < numFiles; i++) {
                expected = sourceList[i].FileName();
                actual = foundList[i].FileName();

                if (expected != actual) i = numFiles;
            }

            // Cleanup before the assert so we don't leave files on the server
            foreach (string fileName in sourceList) {
                try {
                    File.Delete(fileName);
                } catch { }
                ftpService.DeleteFile(remoteFolder + "/" + fileName.FileName(), ref errorMsg);
            }

            Assert.IsTrue(expected == actual, $"Error: '{actual}' was returned when '{expected}' was expected");
        }

        [TestMethod]
        public void DeleteFileTest() {
            // Tested in UploadFileTest
        }

        [TestMethod]
        public void MoveFileTest() {
            // Do a regular FTP test
            moveFileTest(FTPProtocol.FTP);

            // Now do an SFTP test
            moveFileTest(FTPProtocol.SFTP);
        }

        private void moveFileTest(FTPProtocol protocol) { 
            bool    bError;
            string  errorMsg = "",
                    sourceFile = "",
                    targetFile,
                    remoteFolder = GetAppSetting("TargetFolder", "", protocol);

            // Create file(s) to upload
            sourceFile = Path.GetTempPath() + "\\MoveFile.test";
            db.LogTestFile(sourceFile);     // So the cleanup delete it

            using (StreamWriter sw = new StreamWriter(sourceFile)) {
                sw.WriteLine(LorumIpsum());
            }

            // Upload the file
            FTPService.FTPService ftpService = FTPService(protocol);
            targetFile = remoteFolder + "/" + sourceFile.FileName();

            bError = ftpService.UploadFile(sourceFile, targetFile, ref errorMsg);
            Assert.IsTrue(!bError, errorMsg);
            File.Delete(sourceFile);

            // Now download it to check that it got uploaded
            string temp = targetFile;
            targetFile = sourceFile;
            sourceFile = temp;

            List<string> downloadedFiles = new List<string>();
            bError = ftpService.DownloadFile(sourceFile, targetFile, downloadedFiles, ref errorMsg);
            LogTestFile(downloadedFiles);
            Assert.IsTrue(!bError, errorMsg);
            Assert.IsTrue(downloadedFiles.Count == 1, $"Error: {downloadedFiles.Count} file(s) were downloaded when 1 was expected");

            // Check that the file exists
            Assert.IsTrue(File.Exists(targetFile), $"Error: Failed to find file {targetFile}");

            // Now move it on the host
            targetFile = GetAppSetting("ArchiveFolder", "", protocol) + "/" + sourceFile.FileName();
            bError = ftpService.MoveFile(sourceFile, targetFile, ref errorMsg, true);
            Assert.IsTrue(!bError, errorMsg);

            // Try to download it from original location (should fail because it has been moved)
            string tempFile = Path.GetTempFileName();
            db.LogTestFile(tempFile);     // So the cleanup delete it

            bError = ftpService.DownloadFile(sourceFile, tempFile, downloadedFiles, ref errorMsg);
            Assert.IsTrue(bError, "Error: Download should have failed because the source file has been moved. This error means that the move failed");

            // Now try to download it from the archive location
            bError = ftpService.DownloadFile(targetFile, tempFile, downloadedFiles, ref errorMsg);
            Assert.IsTrue(!bError, errorMsg);

            // Now delete it in the archive
            bError = ftpService.DeleteFile(targetFile, ref errorMsg);
            Assert.IsTrue(!bError, errorMsg);

            // Finally, try to download the archived file again (should fail)
            bError = ftpService.DownloadFile(targetFile, tempFile, downloadedFiles, ref errorMsg);
            Assert.IsTrue(bError, "Error: Download should have failed because the previous delete should have removed the file. This error means that the delete failed");
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
