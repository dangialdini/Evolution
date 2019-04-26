using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.PDFService;
using Evolution.Extensions;

namespace Evolution.PDFServiceTests {
    [TestClass]
    public class PDFServiceTests : BaseTest {
        [TestMethod]
        public void ConvertHtmlFileToPDFTest() {
            string sourceFile = SourceRoot + @"\Evolution\App_Data\PurchaseOrderTemplates\PurchaseOrder-Default.html";
            string sourceCss = SourceRoot + @"\Evolution\App_Data\PurchaseOrderTemplates\TemplateStyles.css";
            string targetFile = TempFileFolder + "\\" + RandomString() + ".pdf";
            LogTestFile(targetFile);        // So it gets cleaned up

            var error = PDFService.PDFService.ConvertHtmlFileToPDF(sourceFile, targetFile);
            Assert.IsTrue(!error.IsError, error.Message);
            Assert.IsTrue(File.Exists(targetFile), $"Error: The file {targetFile} could not be found!");
        }

        [TestMethod]
        public void GetPageSizesTest() {
            string pageSizes = PDFService.PDFService.GetPageSizes().Trim();
            Assert.IsTrue(!string.IsNullOrEmpty(pageSizes), "Error: An empty string was returned when a | separated list of strings was expected");
            Assert.IsTrue(pageSizes.CountOf("|") > 0, "Error: The returned string was expected to be a | separated list of strings but no | characters were found");
        }
    }
}
