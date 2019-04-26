using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.Enumerations;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using System.IO;
using Evolution.DataTransferService;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests : BaseTest {
        [TestMethod]
        public void CreatePickDocumentPdfTest() {
            // Tested in CreateStandardPickDocumentPdfTest
            // Testid in CreateRetailPickDocumentPdfTest
        }

        [TestMethod]
        public void CreatePickDocumentStandardPdfTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testCustomerAdditionalInfo = CustomerService.FindCustomerAdditionalInfoModel(testCustomer.Id, testCompany);

            testCustomerAdditionalInfo.ShippingTemplateId = LookupService.FindDocumentTemplateModel(DocumentTemplateCategory.Pickslip, DocumentTemplateType.PackingSlip).Id;
            var error = CustomerService.InsertOrUpdateCustomerAdditionalInfo(testCustomerAdditionalInfo, testUser, CustomerService.LockCustomer(testCustomer));
            Assert.IsTrue(!error.IsError, error.Message);

            var testCreditCard = GetTestCreditCard(testCompany, testCustomer);
            var testSalesOrderHeader = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 20);
            var location = LookupService.FindLocationModel(testCompany.DefaultLocationID.Value);

            CreateTestTransfers(testCompany, testUser);

            var testFolder = Path.GetTempPath() + RandomString();
            var transferName = "Test Transfer:Send-" + FileTransferDataType.WarehousePick;
            var sendConfig = DataTransferService.FindDataTransferConfigurationModel(transferName);

            testSalesOrderHeader.NextActionId = LookupService.FindSaleNextActionId(Enumerations.SaleNextAction.ShipSomething);
            testSalesOrderHeader.FreightCarrierId = LookupService.FindFreightCarriersListModel(testCompany.Id).Items.FirstOrDefault().Id;
            testSalesOrderHeader.CreditCardId = testCreditCard.Id;
            error = SalesService.InsertOrUpdateSalesOrderHeader(testSalesOrderHeader, testUser, SalesService.LockSalesOrderHeader(testSalesOrderHeader));
            Assert.IsTrue(!error.IsError, error.Message);

            error = SalesService.CreatePicks(testCompany, testUser, testSalesOrderHeader.Id.ToString(), false);
            Assert.IsTrue(!error.IsError, error.Message);
        }

        [TestMethod]
        public void CreatePickDocumentRetailPdfTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testCustomerAdditionalInfo = CustomerService.FindCustomerAdditionalInfoModel(testCustomer.Id, testCompany);

            testCustomerAdditionalInfo.ShippingTemplateId = LookupService.FindDocumentTemplateModel(DocumentTemplateCategory.Pickslip, DocumentTemplateType.PackingSlipRetail).Id;
            var error = CustomerService.InsertOrUpdateCustomerAdditionalInfo(testCustomerAdditionalInfo, testUser, CustomerService.LockCustomer(testCustomer));
            Assert.IsTrue(!error.IsError, error.Message);

            var testCreditCard = GetTestCreditCard(testCompany, testCustomer);
            var testSalesOrderHeader = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 41);
            var location = LookupService.FindLocationModel(testCompany.DefaultLocationID.Value);

            CreateTestTransfers(testCompany, testUser);

            var testFolder = Path.GetTempPath() + RandomString();
            var transferName = "Test Transfer:Send-" + FileTransferDataType.WarehousePick;
            var sendConfig = DataTransferService.FindDataTransferConfigurationModel(transferName);

            testSalesOrderHeader.NextActionId = LookupService.FindSaleNextActionId(Enumerations.SaleNextAction.ShipSomething);
            testSalesOrderHeader.FreightCarrierId = LookupService.FindFreightCarriersListModel(testCompany.Id).Items.FirstOrDefault().Id;
            testSalesOrderHeader.CreditCardId = testCreditCard.Id;
            error = SalesService.InsertOrUpdateSalesOrderHeader(testSalesOrderHeader, testUser, SalesService.LockSalesOrderHeader(testSalesOrderHeader));
            Assert.IsTrue(!error.IsError, error.Message);

            error = SalesService.CreatePicks(testCompany, testUser, testSalesOrderHeader.Id.ToString(), false);
            Assert.IsTrue(!error.IsError, error.Message);
        }
    }
}
