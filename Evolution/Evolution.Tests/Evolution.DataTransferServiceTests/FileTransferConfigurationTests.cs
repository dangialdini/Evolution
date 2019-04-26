using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using CommonTest;
using Evolution.DataTransferService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.FTPService;
using Evolution.Enumerations;
using AutoMapper;

namespace Evolution.DataTransferServiceTests {
    public partial class DataTransferServiceTests : BaseTest {

        [TestMethod]
        public void FindDataTransferConfigurationsModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var model = DataTransferService.FindDataTransferConfigurationsModel(true);
            var dbData = db.FindFileTransferConfigurations(true);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = DataTransferService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createDataTransfer(testCompany, testUser);
            var error = DataTransferService.InsertOrUpdateDataTransferConfiguration(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = DataTransferService.FindDataTransferConfigurationsModel(true);
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            DataTransferService.DeleteDataTransferConfiguration(newItem.Id);

            model = DataTransferService.FindDataTransferConfigurationsModel(true);
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindDataTransferTemplatesListItemModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var model = DataTransferService.FindDataTransferTemplatesListItemModel();

            int expected = 1,
                actual = model.Count();
            Assert.IsTrue(actual > expected, $"Error: {actual} items were found when more than {expected} were expected");

            // Add another item a make sure it is found
            string templateFolder = GetAppSetting("SiteFolder", "") + "\\App_Data\\DataTransferTemplates";
            string newFile = RandomInt().ToString() + ".xml";
            string newTemplate = templateFolder + "\\" + newFile;

            LogTestFile(newTemplate);
            using (var sw = new StreamWriter(newTemplate)) {
                sw.WriteLine(LorumIpsum());
            }

            model = DataTransferService.FindDataTransferTemplatesListItemModel();
            var testItem = model.Where(i => i.Text == newFile).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            File.Delete(newTemplate);

            model = DataTransferService.FindDataTransferTemplatesListItemModel();
            testItem = model.Where(i => i.Text == newFile).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindDataTransferConfigurationsListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var model = DataTransferService.FindDataTransferConfigurationsListModel(0, 1, PageSize, "");
            var dbData = db.FindFileTransferConfigurations(true);

            int expected = dbData.Count(),
                actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = DataTransferService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createDataTransfer(testCompany, testUser);
            var error = DataTransferService.InsertOrUpdateDataTransferConfiguration(newItem, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = DataTransferService.FindDataTransferConfigurationsListModel(0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            DataTransferService.DeleteDataTransferConfiguration(newItem.Id);

            model = DataTransferService.FindDataTransferConfigurationsListModel(0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindDataTransferConfigurationModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var model = createDataTransfer(testCompany, testUser);
            var error = DataTransferService.InsertOrUpdateDataTransferConfiguration(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = DataTransferService.FindDataTransferConfigurationModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateDataTransferConfigurationTest() {
            // Tested in DeleteDataTransferConfigurationTest
        }

        [TestMethod]
        public void DeleteDataTransferConfigurationTest() {
            // Get a test user
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a price level
            var model = createDataTransfer(testCompany, testUser);

            var error = DataTransferService.InsertOrUpdateDataTransferConfiguration(model, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindFileTransferConfiguration(model.Id);
            var test = DataTransferService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            DataTransferService.DeleteDataTransferConfiguration(model.Id);

            // And check that is was deleted
            result = db.FindFileTransferConfiguration(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockDataTransferConfigurationTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createDataTransfer(testCompany, testUser);

            var error = DataTransferService.InsertOrUpdateDataTransferConfiguration(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = DataTransferService.LockDataTransferConfiguration(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = DataTransferService.InsertOrUpdateDataTransferConfiguration(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = DataTransferService.InsertOrUpdateDataTransferConfiguration(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = DataTransferService.LockDataTransferConfiguration(model);
            error = DataTransferService.InsertOrUpdateDataTransferConfiguration(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }

        [TestMethod]
        public void MoveToArchiveTest() {
            string errorMsg = "";
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a temp file
            var tempFile = GetTempFile(".txt");
            File.WriteAllText(tempFile, LorumIpsum());

            // Now archive it
            var dts = new DataTransferService.DataTransferService(db);
            var config = createDataTransfer(testCompany, testUser);

            bool bRc = dts.MoveToArchive(config, tempFile, ref errorMsg);
            Assert.IsTrue(bRc == false, errorMsg);

            // Check if the file has been moved
            Assert.IsTrue(!File.Exists(tempFile), $"Error: File '{tempFile}' still exists when it should have been deleted");

            string newTarget = config.ArchiveFolder.AddString("\\") + tempFile.FileName();
            Assert.IsTrue(File.Exists(newTarget), $"Error: File '{newTarget}' could not be found in its archived location");
        }

        [TestMethod]
        public void MoveToErrorTest() {
            string errorMsg = "";
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a temp file
            var tempFile = GetTempFile(".txt");
            File.WriteAllText(tempFile, LorumIpsum());

            // Now archive it
            var dts = new DataTransferService.DataTransferService(db);
            var config = createDataTransfer(testCompany, testUser);

            bool bRc = dts.MoveToError(config, tempFile, ref errorMsg);
            Assert.IsTrue(bRc == false, errorMsg);

            // Check if the file has been moved
            Assert.IsTrue(!File.Exists(tempFile), $"Error: File '{tempFile}' still exists when it should have been deleted");

            string newTarget = config.ErrorFolder.AddString("\\") + tempFile.FileName();
            Assert.IsTrue(File.Exists(newTarget), $"Error: File '{newTarget}' could not be found in its error location");
        }

        private FileTransferConfigurationModel createDataTransfer(CompanyModel company, UserModel user) {
            var dataType = LookupService.FindLOVItemByValueModel(LOVName.FileTransferDataType, ((int)FileTransferDataType.WarehousePick).ToString());

            var config = new FileTransferConfigurationModel {
                CompanyId = company.Id,
                TransferType = FileTransferType.Send,
                TransferTypeText = FileTransferType.Send.ToString(),
                DataTypeId = dataType.Id,
                DataTypeText = dataType.ItemText,
                CreatedDate = DateTimeOffset.Now,
                CreatedById = user.Id,
                CreatedByText = user.Name,
                TransferName = RandomString(),
                Protocol = FTPProtocol.FTP,
                ProtocolText = FTPProtocol.FTP.ToString(),
                ArchiveFolder = Path.GetTempPath() + "Archive",
                ErrorFolder = Path.GetTempPath() + "Error",
            };

            db.AddFolderToLog(config.ArchiveFolder, 10);
            db.AddFolderToLog(config.ErrorFolder, 10);

            return config;
        }
    }
}
