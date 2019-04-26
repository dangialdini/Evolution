using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;

namespace Evolution.FileImportServiceTests {
    [TestClass]
    public class FileImportServiceTests : BaseTest {
        [TestMethod]
        public void UploadFileTest() {
            // Tested by SalesServiceTests.ImportSalesTest()
        }

        [TestMethod]
        public void InsertOrUpdateFileImportRowTest() {
            // Tested by SalesServiceTests.ImportSalesTest()
        }

        [TestMethod]
        public void GetDataTest() {
            // Tested by SalesServiceTests.ImportSalesTest()
        }

        [TestMethod]
        public void GetHeadingListTest() {
            var fiService = new FileImportService.FileImportService(db);
            var headings = fiService.GetHeadingList("Sale.dat");

            int expected = 10,
                actual = headings.Count();
            Assert.IsTrue(actual > expected, $"Error: {actual} item(s) were returned when more than {actual} were expected");
        }
    }
}
