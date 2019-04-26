using System;
using System.Collections.Generic;
using System.IO;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evolution.ShopifyImportServiceTests {
    [TestClass]
    public class ShopifyImportServiceTests : BaseTest {
        [TestMethod]
        public void ProcessXmlTest() {
            // Tested in ShopifyImportTaskTest
        }

        [TestMethod]
        public void MapOrderToTempTest() {
            // Tested in MissingOrderHeaderTest & MissingOrderLinesTest
        }

        [TestMethod]
        public void SaveDataToTempTablesTest() {
            // Tested in GetShopifyTempTableDataTest
        }

        [TestMethod]
        public void GetShopifyTempTableDataTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.ShopifyImportServiceTests\TestData";
            var xmlFile = Directory.GetFiles(testLocation);
            var taskUser = GetTaskUser();
            Dictionary<string, string> configDetails = new Dictionary<string, string> {
                { "StoreId", "1" },
                { "CustomerName", "AAA Company.com.au Sales" },
                { "BrandCategory", "Company" },
                { "DataSource", "OrderHive" }
            };

            ShopifyImportService.ShopifyImportService service = new ShopifyImportService.ShopifyImportService(db);
            
            foreach(var fileName in xmlFile) {
                int expectedOrders;
                int actualOrders;
                int expectedItems = 0;
                int actualItems = 0;

                var orders = service.ProcessXml(fileName, testCompany.AccountName);
                expectedOrders = orders.Order.Length;

                List<ShopifyImportHeaderTemp> sitmList = new List<ShopifyImportHeaderTemp>();
                foreach(var order in orders.Order) {
                    expectedItems += order.SalesOrderItems.Length;
                    
                    var shopifyTemp = service.MapOrderToTemp(testCompany.AccountName, configDetails, order, taskUser);
                    sitmList.Add(shopifyTemp);
                }
                service.SaveDataToTempTables(sitmList);
                
                // Get data from TEMP tables
                sitmList = service.GetShopifyTempTableData();
                actualOrders = sitmList.Count;
                foreach(var order in sitmList) {
                    actualItems += order.ShopifyImportDetailTemps.Count;
                }

                Assert.AreEqual(expectedOrders, actualOrders, $"Error: {expectedOrders} order were expected for file {fileName}, when {actualOrders} were retrieved from the databse");
                Assert.AreEqual(expectedItems, actualItems, $"Error: {expectedItems} items were expected for file {fileName}, when {actualItems} were retrieved from the database");
            }
        }

        [TestMethod]
        public void CopyTempDataToSalesModelTest() {
            // Tested in ShopifyImportTaskTest
        }

        [TestMethod]
        public void SaveSalesOrdersTest() {
            // Tested in ShopifyImportTaskTest
        }

        [TestMethod]
        public void EmptyFileTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.ShopifyImportServiceTests\TestData\Empty";
            var files = Directory.GetFiles(testLocation);
            ShopifyImportService.ShopifyImportService service = new ShopifyImportService.ShopifyImportService(db);

            // It is expected that this test will fail in code as the XML files are empty - one is completly empty, the other only has a <Data> node.
            // This test will catch the failed attat at processing the file and return a true value
            bool errored = false;
            int expectedErrors = 3;
            int actual = 0;
            ShopifyImportTempModel.Data file = null;
            foreach(var xmlFile in files) {
                try {
                    file = service.ProcessXml(xmlFile, testCompany.AccountName);
                } catch {
                    errored = true;
                }
                actual++;
                Assert.IsTrue(errored == true, "Error: Call to 'ProcessXml' should have cause an exception but none was thrown");
            }
            Assert.IsTrue(expectedErrors == actual, $"Error: number of files found where {actual} when {expectedErrors} were expected");
        }

        [TestMethod]
        public void MissingOrderHeaderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.ShopifyImportServiceTests\TestData\MissingOrderHeader";
            var xmlFile = Directory.GetFiles(testLocation);
            var taskUser = GetTaskUser();
            Dictionary<string, string> configDetails = new Dictionary<string, string> {
                { "StoreId", "Companysydney" },
                { "CustomerName", "AAA Company.com.au Sales" },
                { "BrandCategory", "Company" },
                { "DataSource", "OrderHive" }
            };

            ShopifyImportService.ShopifyImportService service = new ShopifyImportService.ShopifyImportService(db);
            bool errored = false;

            // It is expected that this test will fail in code as there is no header in the xml file
            // This test will catch the failed attempt at mapping the file to the TEMP model and return a true value

            ShopifyImportTempModel.Data fileData = null;
            try {
                fileData = service.ProcessXml(xmlFile[0], testCompany.AccountName);
                service.MapOrderToTemp(testCompany.AccountName, configDetails, fileData.Order[0], taskUser);
            } catch {
                errored = true;
            }
            Assert.IsTrue(errored == true, "Error: Call to 'MapOrderToTemp' should have caused an exception but none was thrown");
        }

        [TestMethod]
        public void MissingOrderLinesTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testLocation = @"C:\Development\Evolution\Evolution\Evolution.Tests\Evolution.ShopifyImportServiceTests\TestData\MissingOrderLines";
            var xmlFile = Directory.GetFiles(testLocation);
            var taskUser = GetTaskUser();
            Dictionary<string, string> configDetails = new Dictionary<string, string> {
                { "StoreId", "Companysydney" },
                { "CustomerName", "AAA Company.com.au Sales" },
                { "BrandCategory", "Company" },
                { "DataSource", "OrderHive" }
            };

            ShopifyImportService.ShopifyImportService service = new ShopifyImportService.ShopifyImportService(db);
            bool errored = false;

            // It is expected that this test will fail in code as there is no header in the xml file
            // This test will catch the failed attempt at mapping the file to the TEMP model and return a true value
            try {
                var fileData = service.ProcessXml(xmlFile[0], testCompany.AccountName);
                service.MapOrderToTemp(testCompany.AccountName, configDetails, fileData.Order[0], taskUser);
            } catch {
                errored = true;
            }
            Assert.IsTrue(errored == true, "Error: Call to 'MapOrderToTemp' should have caused an exception but none was thrown");
        }
    }
}
