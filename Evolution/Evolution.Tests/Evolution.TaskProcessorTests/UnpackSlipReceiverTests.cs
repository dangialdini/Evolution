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
    public partial class UnpackSlipReceiverTaskTests : BaseTest {
        public void GetTaskNameTest() {
            var task = new UnpackSlipReceiverTask(db);
            string expected = TaskName.UnpackSlipReceiverTask,
                   actual = task.GetTaskName();
            Assert.IsTrue(actual == expected, $"Error: GetTaskName() returned {actual} when {expected} was expected. Check that the derived task class overrides the GetTaskName() method");
        }

        [TestMethod]
        public void DoProcessingTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            AddUserToUserGroups(testCompany, testUser);

            CreateTestTransfers(testCompany, testUser);

            // Create a purchase order
            var poh = GetTestPurchaseOrderHeader(testCompany, testUser, 4);

            // Get the test unpack slip data transfer service
            var config = GetDataTransferService(testCompany).FindDataTransferConfigurationModel("Test Transfer:Send-WarehouseUnpackSlip");

            // Create an unpack slip file
            string unpackFile = createUnpackListFile(poh);

            // Upload it to the test location
            FTPService.FTPService ftp = new FTPService.FTPService(config.FTPHost, config.UserId, config.Password, config.Protocol);
            string errorMsg = "";
            bool result = ftp.UploadFile(unpackFile, config.TargetFolder, ref errorMsg);
            Assert.IsTrue(result == false, errorMsg);

            // Get the receive config
            config = GetDataTransferService(testCompany).FindDataTransferConfigurationModel("Test Transfer:Receive-WarehouseUnpackSlip");

            // Now run-up the task processor
            UnpackSlipReceiverTask task = new UnpackSlipReceiverTask(db);
            string[] args = { TaskName.UnpackSlipReceiverTask,
                              config.TransferName };
            task.Run(args);
        }

        [TestMethod]
        public void SetTransferConfigTest() {
            // Tested in DoProcessingTest above
        }

        private string createUnpackListFile(PurchaseOrderHeaderModel poh) {
            string fileName = GetTempFile(".csv");
            using (StreamWriter sw = new StreamWriter(fileName)) {
                sw.WriteLine("PO_NUMBER,LINE_NR,PART_NUMBER,QTY_RECEIVED,UOM,VARIANCE");
                sw.WriteLine(poh.OrderNumber.ToString() + ",1,ABC,24,EACH,0");
                sw.WriteLine(poh.OrderNumber.ToString() + ",2, ABC, 36, EACH, 0");
                sw.WriteLine(poh.OrderNumber.ToString() + ",3, ABC, 36, EACH, 0");
                sw.WriteLine(poh.OrderNumber.ToString() + ",4, ABC, 24, EACH, 0");
                sw.WriteLine(poh.OrderNumber.ToString() + ",5, ABC, 24, EACH, 0");
                sw.WriteLine(poh.OrderNumber.ToString() + ",6, ABC, 200, EACH, 0");
            }
            return fileName;
        }
    }
}
