using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.Enumerations;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests : BaseTest {
        [TestMethod]
        public void CreateOrderConfirmationPdfTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompanyAU();

            // Find a random sales order header
            int range = db.FindSalesOrderHeaders(testCompany.Id).Count();
            var sohList = db.FindSalesOrderHeaders(testCompany.Id)
                            .Skip(RandomInt(0, range - 1))
                            .Take(1)
                            .FirstOrDefault();
            // Uncomment the following line to get a document for a random SOH, possibly with no items
            //var soh = SalesService.MapToModel(sohList);
            // Uncomment the following line to get a document known to have a number of items
            var soh = SalesService.FindSalesOrderHeaderModel(45176, testCompany);   // 158 items

            var template = LookupService.FindDocumentTemplateModel(DocumentTemplateCategory.SalesOrders, DocumentTemplateType.OrderConfirmation);

            string pdfFile = "";
            var error = SalesService.CreateOrderConfirmationPdf(soh,
                                                                template, 
                                                                null,
                                                                true,
                                                                ref pdfFile);
            MediaServices.AddFileToLog(pdfFile, 20);        // So it gets cleaned up later
            Assert.IsTrue(File.Exists(pdfFile));

            Assert.IsTrue(!error.IsError, error.Message);
        }
    }
}
