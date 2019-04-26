using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using CommonTest;
using Evolution.FileCompressionService;

namespace Evolution.FileCompressionServiceTests {
    [TestClass]
    public class ZipTests : BaseTest {
        [TestMethod]
        public void ZipFolderTest() {
            // Create a temp folder
            string tempRoot = Path.GetTempPath();
            string tempFolder = tempRoot + "ZipTest";
            Directory.CreateDirectory(tempFolder);

            // Create some files in the temp directory
            string[] fileList = new string[10];
            for (int i = 0; i < fileList.Length; i++) {
                string testFile = tempFolder + "\\File" + i.ToString() + ".dat";
                fileList[i] = testFile;

                LogTestFile(testFile);

                using (StreamWriter sw = new StreamWriter(testFile)) {
                    sw.WriteLine(LorumIpsum());
                }
            }

            // Zip up the folder
            string zipFile = tempRoot + "ZippedFiles.zip";
            LogTestFile(zipFile);

            var error = Zip.ZipFolder(tempFolder, zipFile);
            Assert.IsTrue(!error.IsError, "Error: " + error.Message);

            // Temp files are cleaned up by the base class test tare-down process
        }

        [TestMethod]
        public void UnzipFileTest() {
            // Tested in ZipUnzipFileTest below
        }

        [TestMethod]
        public void ZipFileTest() {
            // Tested in ZipUnzipFileTest below
        }

        [TestMethod]
        public void ZipUnzipFileTest() {
            string tempRoot = Path.GetTempPath();
            string tempFile1 = tempRoot + RandomString() + ".txt";
            LogTestFile(tempFile1);

            using (StreamWriter sw = new StreamWriter(tempFile1)) {
                sw.WriteLine(LorumIpsum());
            }
            string file1 = File.ReadAllText(tempFile1);

            string tempFile2 = tempRoot + RandomString() + ".zip";
            LogTestFile(tempFile2);

            var error = Zip.ZipFile(tempFile1, tempFile2);
            Assert.IsTrue(!error.IsError, error.Message);
            Assert.IsTrue(File.Exists(tempFile2), $"Error: ZIP failed '{tempFile2}' was not created");

            File.Delete(tempFile1);

            error = Zip.UnzipFile(tempFile2, tempRoot);
            Assert.IsTrue(!error.IsError, error.Message);
            Assert.IsTrue(File.Exists(tempFile1), $"Error: UNZIP failed '{tempFile1}' was not created");

            string file3 = File.ReadAllText(tempFile1);
            Assert.IsTrue(file1 == file3, "Error: The content of the source file is different to the combined zip/unzip output file - Zip/Unzip failed");
        }

        [TestMethod]
        public void ZipFilesTest() {
            int numFiles = 10;
            string tempRoot = Path.GetTempPath();
            string zipFile = Path.GetTempPath() + RandomString() + ".zip";

            List<string> fileList = new List<string>();
            List<string> fileContent = new List<string>();

            // Create some files to zip up
            for (int i = 0; i < numFiles; i++) {
                string tempFile = tempRoot + RandomString() + ".txt";
                LogTestFile(tempFile);
                fileList.Add(tempFile);

                using (StreamWriter sw = new StreamWriter(tempFile)) {
                    sw.WriteLine(LorumIpsum() + RandomString());
                }
                fileContent.Add(File.ReadAllText(tempFile));
            }

            // Zip them up
            var error = Zip.ZipFiles(fileList, zipFile);
            Assert.IsTrue(!error.IsError, error.Message);
            Assert.IsTrue(File.Exists(zipFile), $"Error: ZIP failed '{zipFile}' was not created");

            // Delete the source files
            for (int i = 0; i < numFiles; i++) {
                File.Delete(fileList[i]);
                Assert.IsTrue(!File.Exists(fileList[i]), $"Error: Failed to delete {fileList[i]}");
            }

            // Now unzip the zip file and check the exctracted files
            error = Zip.UnzipFile(zipFile, tempRoot);
            Assert.IsTrue(!error.IsError, error.Message);

            for(int i = 0; i < numFiles; i++) {
                Assert.IsTrue(File.Exists(fileList[i]), $"Error: {fileList[i]} was not found");
                string temp = File.ReadAllText(fileList[i]);
                Assert.IsTrue(temp == fileContent[i], "Error: The content of {fileList[i]} did not match the original source file - Zip/Unzip failed");
            }
        }
    }
}
