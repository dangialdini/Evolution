using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.PurchasingService;

namespace Evolution.PurchasingServiceTests {
    public partial class PurchasingServiceTests {
        [TestMethod]
        public void ValidateOrdersTest() {
            // Tested below
        }

        [TestMethod]
        public void ImportOrdersTest() {
            var testUser = GetTestUser();
            //var testCompany = GetTestCompany(testUser, true);
            var testCompany = GetTestCompanyAU();

            List<string> headings = new List<string>();
            headings.Add("ProductCode");
            headings.Add("UnitPrice");
            headings.Add("Quantity");
            headings.Add("SupplierName");

            // Create a temporary file of test data
            string tempFile = Path.GetTempFileName();

            var productList = db.FindProducts()
                                .Where(p => p.PrimarySupplierId != null)
                                .ToList();

            using (var sw = new StreamWriter(tempFile)) {
                // Write a random number of products
                string temp = "";
                for (int i = 0; i < headings.Count(); i++) {
                    if (!string.IsNullOrEmpty(temp)) temp += ",";
                    temp += headings[i];
                }
                sw.WriteLine(temp);

                for(int i = 0; i < 25; i++) {
                    int rnd = RandomInt(0, productList.Count() - 1);
                    var product = productList[rnd];

                    string line = "\"" + product.ItemNumber + "\",";
                    line += RandomInt(1, 25).ToString() + ",";
                    line += rnd.ToString() + ",\"";
                    line += product.Supplier.Name + "\"";
                    sw.WriteLine(line);

                    productList.RemoveAt(rnd);      // So we don't get the same product again
                }
            }

            // Now import the file into the database
            FileImportService.FileImportService fis = new FileImportService.FileImportService(db);

            var error = fis.UploadFile(testCompany, testUser, tempFile, true);
            Assert.IsTrue(!error.IsError, error.Message);

            // Validate the items
            error = PurchasingService.ValidateOrders(testCompany, testUser, headings);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now create orders
            error = PurchasingService.ImportOrders(testCompany, testUser, testCompany.DefaultLocationID.Value, headings);
            Assert.IsTrue(!error.IsError, error.Message);
        }
    }
}
