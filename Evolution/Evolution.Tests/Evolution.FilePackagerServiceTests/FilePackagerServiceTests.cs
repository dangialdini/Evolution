using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.PurchasingService;
using Evolution.LookupService;
using Evolution.Models.Models;
using Evolution.Enumerations;

namespace Evolution.FilePackagerServiceTests {
    [TestClass]
    public partial class FilePackagerServiceTests : BaseTest {
        [TestMethod]
        public void ProcessPurchaseOrderTest() {
            // This is tested by purchaseOrderToWarehouseTest and purchaseOrderToFreightForwarderTest below
        }

        [TestMethod]
        public void SendPurchaseOrderToSupplierTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var poh = GetTestPurchaseOrderHeader(testCompany, testUser, 58);

            // Copy the purchase order to the temp tables
            var poht = PurchasingService.CopyPurchaseOrderToTemp(testCompany, poh, testUser);

            // Create the PDF
            string pdfFile = "";
            var error = PurchasingService.CreatePurchaseOrderPdf(poh,
                                                                 testCompany.POSupplierTemplateId,  //DocumentTemplateType.PurchaseOrder, 
                                                                 null, 
                                                                 ref pdfFile);
            Assert.IsTrue(!error.IsError, error.Message);

            // Send the purchase order
            FilePackagerService.FilePackagerService fpService = new FilePackagerService.FilePackagerService(db);
            error = fpService.SendPurchaseOrderToSupplier(poht.Id, testUser, pdfFile);
            Assert.IsTrue(!error.IsError, error.Message);
        }

        [TestMethod]
        public void SendPurchaseOrderToWarehouseTest() {
            // This test places a file in an FTP 'send' folder but does not actually send the file.
            // It also queues an EMail, so make sure TaskProcessor email sending is switched off! 

            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            CreateTestTransfers(testCompany, testUser);

            // Create a purchase order with items
            var poh = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            // Copy the purchase order to the temp tables
            var poht = PurchasingService.CopyPurchaseOrderToTemp(testCompany, poh, testUser);

            // Send the purchase order
            FilePackagerService.FilePackagerService fpService = new FilePackagerService.FilePackagerService(db);
            var error = fpService.SendPurchaseOrderToWarehouse(poht.Id);
            Assert.IsTrue(!error.IsError, error.Message);
        }

        [TestMethod]
        public void SendPurchaseOrderToFreightForwarderTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            CreateTestTransfers(testCompany, testUser);

            // Create a purchase order with items
            var poh = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            // Copy the purchase order to the temp tables
            var poht = PurchasingService.CopyPurchaseOrderToTemp(testCompany, poh, testUser);

            // Send the purchase order
            FilePackagerService.FilePackagerService fpService = new FilePackagerService.FilePackagerService(db);
            var error = fpService.SendPurchaseOrderToFreightForwarder(poht.Id);
            Assert.IsTrue(!error.IsError, error.Message);
        }
    }
}
