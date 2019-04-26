using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.SalesService;
using Evolution.MediaService;
using Evolution.Enumerations;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests {
        [TestMethod]
        public void ValidateOrdersTest() {
            // Tested in ImportSalesTest
        }

        [TestMethod]
        public void ImportSalesTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompanyAU();

            // Use the file import service to load the file into staging tables
            FileImportService.FileImportService fiService = new FileImportService.FileImportService(db);

            string testFile = GetAppSetting("SourceFolder", "") + @"\Evolution.Tests\Evolution.SalesServiceTests\TestData\Sales.csv";
            string fileName = MediaServices.GetTempFolder() + "Sales.csv";
            var error = MediaService.MediaService.CopyOrMoveFile(testFile, fileName, FileCopyType.Copy);
            Assert.IsTrue(!error.IsError, error.Message);

            error = fiService.UploadFile(testCompany, testUser, fileName, true);
            Assert.IsTrue(!error.IsError, error.Message);

            // Get the column headings from the first line of the file
            bool bFirst = true;
            int lineCount = 0;
            string firstLine = "";
            using (var sr = new StreamReader(testFile)) {
                string lineText;
                while ((lineText = sr.ReadLine()) != null) {
                    if (bFirst) {
                        firstLine = lineText.Replace("\t", ",");
                        bFirst = false;
                    } else {
                        lineCount++;
                    }
                }
            }
            var headings = firstLine.Split(',')
                                    .ToList();

            // Get the data mapping model
            var data = fiService.GetData(testCompany, testUser);
            int expected = lineCount,
                actual = data.Lines.Count();
            Assert.IsTrue(expected == actual, $"Error: {actual} line(s) were found when {expected} were expected");

            // Validate the sales records
            error = SalesService.ValidateOrders(testCompany, testUser, headings, "dd/MM/yyyy");
            Assert.IsTrue(!error.IsError, error.Message);

            // Now import the sales
            var soStatus = LookupService.FindSalesOrderHeaderStatusListItemModel()
                                        .FirstOrDefault();
            var source = LookupService.FindLOVItemsListItemModel(testCompany, LOVName.OrderSource)
                                      .FirstOrDefault();

            error = SalesService.ImportSales(testCompany, 
                                             testUser, 
                                             Convert.ToInt32(soStatus.Id), 
                                             Convert.ToInt32(source.Id),
                                             headings,
                                             testUser.DateFormat,
                                             0);
            Assert.IsTrue(!error.IsError, error.Message);
        }
    }
}
