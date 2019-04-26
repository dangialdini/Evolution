using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using System.IO;
using System.Xml.Serialization;
using Evolution.PepperiImportService;
using Evolution.TaskProcessor;

namespace Evolution.PepperiImportServiceTests {
    [TestClass]
    public partial class PepperiImportServiceTests : BaseTest {
        [TestMethod]
        public void GetFilesToImportTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser); // testCompany.CompanyName not an actual name
            var customer = GetTestCustomer(testCompany, testUser);
            var taskUser = GetTaskUser();

            // Setup a Task
            var fit = new FileImportTask(db);
            var testTask = fit.StartTask();
            // File location for Test Data
            string fileLocation = GetAppSetting("SourceFolder", "") + @"\Evolution.Tests\Evolution.PepperiImportServiceTests\TestData";

            PepperiTransactionTempModel.SalesTransaction transaction = new PepperiTransactionTempModel.SalesTransaction();
            XmlSerializer deserializer = new XmlSerializer(typeof(PepperiTransactionTempModel.SalesTransaction));
            PepperiImportService.PepperiImportService service = new PepperiImportService.PepperiImportService(db);

            TextReader tReader;
            PepperiImportHeaderTemp dbHeaderData;
            PepperiImportHeaderTemp headerModel;
            List<PepperiImportDetailTemp> detailsModel;
            IEnumerable<PepperiImportDetailTemp> dbDetailsData;
            PepperiImportDetailTemp dbItem;

            #region TEST: Normal File
            // ************************* //
            // *** TEST: Normal File *** //
            string normalFile = fileLocation + "\\Normal\\SalesOrder_Example.xml";
            // Save the XML Data to the temps tables in Db
            service.ProcessXml(normalFile, testCompany.AccountName, taskUser, testTask);
            tReader = new StreamReader(normalFile);
            transaction = (PepperiTransactionTempModel.SalesTransaction)deserializer.Deserialize(tReader);

            // Header
            headerModel = service.MapFileImportHeaderToTemp(testCompany.AccountName, transaction.TransactionHeader, taskUser, testTask);
            dbHeaderData = db.FindPepperiImportHeaderTemps(Convert.ToInt32(transaction.TransactionHeader.TransactionHeaderFields.WrntyID));
            Assert.IsTrue(headerModel.WrntyId == dbHeaderData.WrntyId, $"Error: WrntyId {headerModel.WrntyId} was expected, when {dbHeaderData.WrntyId} was found");

            var testHeaderModel = service.GetTempTableData(testTask);
            AreEqual(dbHeaderData, testHeaderModel);

            // Lines
            detailsModel = service.MapFileImportDetailToTemp(testCompany.AccountName, transaction.TransactionLines, transaction.TransactionHeader, taskUser, testTask);
            dbDetailsData = db.FindPepperiImportDetailTemps(Convert.ToInt32(transaction.TransactionHeader.TransactionHeaderFields.WrntyID));
            foreach (var line in detailsModel) {
                dbItem = dbDetailsData.Where(m => m.ItemWrntyId == line.ItemWrntyId).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(line.ItemWrntyId == dbItem.ItemWrntyId, $"Error: WrntyId {line.ItemWrntyId} was expected, when {dbItem.ItemWrntyId} was found");
            }

            var testDetailModel = testHeaderModel.PepperiImportDetailTemps;
            AreEqual(dbDetailsData, testDetailModel);

            // Clean the database for below tests
            db.CleanPepperiImportTempTables();

            #endregion
            #region TEST: No Header(Empty File)
            // ************************* //
            // TEST: No Header(Empty)
            string noHeaderFile = fileLocation + "\\NoHeader\\SalesOrder_NoHeader_Example.xml";
            service.ProcessXml(noHeaderFile, testCompany.AccountName, taskUser, testTask);
            tReader = new StreamReader(noHeaderFile);
            transaction = (PepperiTransactionTempModel.SalesTransaction)deserializer.Deserialize(tReader);

            // Header
            dbHeaderData = db.FindPepperiImportHeaderTempRecord();
            if (transaction == null || transaction.TransactionHeader == null || transaction.TransactionHeader.TransactionHeaderFields == null)
                headerModel = null;
            Assert.AreEqual(dbHeaderData, headerModel, $"Error: Database data does not match the data in the XML file");
            #endregion
            #region TEST: Header, No Lines
            // ************************* //
            // TEST: No Lines
            string noLinesFile = fileLocation + "\\NoLines\\SalesOrder_NoLines_Example.xml";
            service.ProcessXml(noLinesFile, testCompany.AccountName, taskUser, testTask);
            tReader = new StreamReader(noLinesFile);
            transaction = (PepperiTransactionTempModel.SalesTransaction)deserializer.Deserialize(tReader);

            // Lines
            var dbDetails = db.FindAllPepperiImportDetailTempRecords().FirstOrDefault();
            if(transaction == null || transaction.TransactionLines == null || transaction.TransactionLines.FirstOrDefault() == null || transaction.TransactionLines.Length == 0) {
                detailsModel = null;
            }
            Assert.AreEqual(dbDetails, detailsModel, $"");
            #endregion
        }

        [TestMethod]
        public void GetTempDataTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser); // testCompany.CompanyName not an actual name
            var company = CompanyService.FindCompanyFriendlyNameModel(testCompany.AccountName);
            var taskUser = GetTaskUser();

            var fit = new FileImportTask(db);
            var testTask = fit.StartTask();
            PepperiImportService.PepperiImportService service = new PepperiImportService.PepperiImportService(db);

            string fileLocation = GetAppSetting("SourceFolder", "") + @"\Evolution.Tests\Evolution.PepperiImportServiceTests\TestData";
            PepperiTransactionTempModel.SalesTransaction transaction = new PepperiTransactionTempModel.SalesTransaction();
            PepperiImportHeaderTemp piht;
            SalesOrderHeader soHeader;

            int expected;
            int actual;

            #region TEST: Normal File
            // ************************* //
            // *** TEST: Normal File *** //
            string normalFile = fileLocation + "\\Normal\\SalesOrder_Example.xml";
            transaction = service.ReadFile(normalFile, testTask);
            // Create sales record in TEMP tables
            service.ProcessTransaction(company.AccountName, transaction, taskUser, testTask);

            // Get Newly Created Sales Model
            piht = service.GetTempTableData(testTask);
            // Save TEMP data to Sales Tables
            soHeader = service.CopyTempDataToProduction(piht, piht.PepperiImportDetailTemps, testTask);

            // Check Header is found
            actual = db.FindSalesOrderHeaders(company.Id).Where(s => s.OrderNumber == soHeader.OrderNumber).Count();
            expected = 1;
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected.");

            // Check Details are found
            actual = db.FindSalesOrderDetails(company.Id, soHeader.Id).Count();
            expected = soHeader.SalesOrderDetails.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected.");

            #endregion
            #region TEST: No Header(Empty File)
            // ************************* //
            // TEST: No Header(Empty)
            // Get the number of records in the production table BEFORE attempting to process the XML file
            var headerActual = db.FindSalesOrderHeaders(company.Id).Count();
            var detailsActual = db.FindSalesOrderDetails(company.Id).Count();
            string noHeaderFile = fileLocation + "\\NoHeader\\SalesOrder_NoHeader_Example.xml";
            transaction = service.ReadFile(noHeaderFile, testTask);
            // Attempt to create sales record in TEMP tables
            service.ProcessTransaction(company.AccountName, transaction, taskUser, testTask);

            // Check that no data was saved in the Sales tables after attempting to process the XML file
            // Header
            expected = db.FindSalesOrderHeaders(company.Id).Count();
            Assert.IsTrue(headerActual == expected, $"Error: {actual} items were returned when {expected} were expected.");
            // Details
            expected = db.FindSalesOrderDetails(company.Id).Count();
            Assert.IsTrue(detailsActual == expected, $"Error: {actual} items were returned when {expected} were expected.");

            #endregion
            #region TEST: Header, No Lines
            // ************************* //
            // TEST: No Lines
            string noLinesFile = fileLocation + "\\NoLines\\SalesOrder_NoLines_Example.xml";
            transaction = service.ReadFile(noLinesFile, testTask);
            // Attempt to create sales record in TEMP tables
            service.ProcessTransaction(company.AccountName, transaction, taskUser, testTask);
            
            // Check that no data was saved in the Sales tables after attempting to process the XML file
            // Header
            expected = db.FindSalesOrderHeaders(company.Id).Count();
            Assert.IsTrue(headerActual == expected, $"Error: {actual} items were returned when {expected} were expected.");
            // Details
            expected = db.FindSalesOrderDetails(company.Id).Count();
            Assert.IsTrue(detailsActual == expected, $"Error: {actual} items were returned when {expected} were expected.");

            #endregion
        }

        [TestMethod]
        public void ProcessXmlTest() {
            // Tested in GetFilesToImportTest
        }

        [TestMethod]
        public void ReadFileTest() {
            // Tested in GetFilesToImportTest
        }

        [TestMethod]
        public void ProcessTransactionTest() {
            // Tested in GetFilesToImportTest
        }

        [TestMethod]
        public void MapFileImportHeaderToTempTest() {
            // Tested in GetFilesToImportTest
        }

        [TestMethod]
        public void MapFileImportDetailToTempTest() {
            // Tested in GetFilesToImportTest
        }

        [TestMethod]
        public void GetTempTableDataTest() {
            // Tested in GetFilesToImportTest
        }

        [TestMethod]
        public void CopyTempDataToProductionTest() {
            // Tested in GetFilesToImportTest
        }
    }
}