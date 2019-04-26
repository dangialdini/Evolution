using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.Enumerations;

namespace Evolution.PurchasingServiceTests {
    public partial class PurchasingServiceTests : BaseTest {
        [TestMethod]
        public void CreatePurchaseOrderPdfTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompanyAU();

            // Find a random purchase order header
            var poh = db.FindPurchaseOrderHeaders(testCompany.Id)
                        .Skip(RandomInt(0, 4999))
                        .Take(1)
                        .FirstOrDefault();

            string pdfFile = "";
            var error = PurchasingService.CreatePurchaseOrderPdf(poh,
                                                                 testCompany.POSupplierTemplateId, //DocumentTemplateType.PurchaseOrder, 
                                                                 null, 
                                                                 ref pdfFile);
            MediaServices.AddFileToLog(pdfFile, 20);        // So it gets cleaned up later

            Assert.IsTrue(!error.IsError, error.Message);
        }
    }
}
