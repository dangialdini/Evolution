using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;

namespace Evolution.PickServiceTests {
    public partial class PickServiceTests : BaseTest {
        [TestMethod]
        public void CreatePicksTest() {
            // This test creates multiple picks
            int itemCount = 10;

            // Create a test sale
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            CreateTestTransfers(testCompany, testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var sohList = new List<SalesOrderHeaderModel>();
            var pickHeaders = new List<PickHeaderModel>();

            sohList.Add(GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 0, true));
            var error = PickService.CreatePicks(testCompany, sohList, false, pickHeaders);
            Assert.IsTrue(error.IsError, "Error: Creating a Pick with no lines should have cause an error");

            int beforePicks = db.FindPickHeaders(testCompany.Id).Count(),
                beforePickLines = db.FindPickDetails(testCompany.Id).Count();

            sohList = new List<SalesOrderHeaderModel>();
            sohList.Add(GetTestSalesOrderHeader(testCompany, testCustomer, testUser, itemCount, true));
            error = PickService.CreatePicks(testCompany, sohList, false, pickHeaders);

            foreach (var pickH in pickHeaders) db.LogTestFile(pickH.PickFiles);

            Assert.IsTrue(!error.IsError, error.Message);

            int afterPicks = db.FindPickHeaders(testCompany.Id).Count(),
                afterPickLines = db.FindPickDetails(testCompany.Id).Count();

            int expected = 1,
                actual = afterPicks - beforePicks;
            Assert.IsTrue(actual == expected, $"Error: {actual} pick headers were found when {expected} were expected");

            expected = itemCount;
            actual = afterPickLines - beforePickLines;
            Assert.IsTrue(actual == expected, $"Error: {actual} pick lines were found when {expected} were expected");

            // Check to see if the pick CSV file exists
            var ph = pickHeaders.FirstOrDefault();
            Assert.IsTrue(ph != null, "Error: No Pick Headers were found");

            Assert.IsTrue(ph.PickFiles.Count() > 0, "Error: No Pick Data File was returned");
            Assert.IsTrue(File.Exists(ph.PickFiles[0]), $"Error: Pick Data File '{ph.PickFiles[0]}' could not be found");

            // Cleanup
            File.Delete(ph.PickFiles[0]);
        }
    }
}
