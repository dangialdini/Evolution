using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.Enumerations;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests : BaseTest {
        [TestMethod]
        public void CreateSalesDocumentPdfTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var soh = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 100);

            var template = LookupService.FindDocumentTemplateModel(DocumentTemplateCategory.SalesOrders, DocumentTemplateType.ConfirmedOrder);

            string outputFile = "";
            var error = SalesService.CreateSalesDocumentPdf(soh, template.Id, "", true, ref outputFile);
            MediaServices.AddFileToLog(outputFile, 10);
            Assert.IsTrue(!error.IsError, error.Message);
            Assert.IsTrue(File.Exists(outputFile), $"Error: File '{outputFile}' could not be found");
        }
    }
}
