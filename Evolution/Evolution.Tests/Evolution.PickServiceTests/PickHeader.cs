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
        public void FindPicksListModelTest() {
            int itemCount = 5,
                numSohs = 6;

            // Create a test sale
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            CreateTestTransfers(testCompany, testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Create some SOHs
            var sohList = new List<SalesOrderHeaderModel>();
            var pickHeaders = new List<PickHeaderModel>();

            for (int i = 0; i < numSohs; i++) {
                sohList.Add(GetTestSalesOrderHeader(testCompany, testCustomer, testUser, itemCount, true));
            }

            var error = PickService.CreatePicks(testCompany, sohList, false, pickHeaders);
            foreach (var pickH in pickHeaders) {
                db.LogTestFile(pickH.PickFiles);
                PickService.SetPickSentToWarehouseDate(pickH, DateTimeOffset.Now);
            }

            Assert.IsTrue(!error.IsError, error.Message);

            // Now find the picks
            var pickList = PickService.FindPicksListModel(testCompany.Id, DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now)
                                      .Items
                                      .OrderBy(pl => pl.Id)
                                      .ToList();

            // Check that the list contains all the picks we created
            int expected = numSohs,
                actual = pickList.Count();
            Assert.IsTrue(actual == expected, "Error: {actual} picks were returned when {expected} were expected");

            var exceptions = new List<string>();
            exceptions.Add("PickDetails");
            exceptions.Add("PickDropFolder");       // Because we don't know it at test prep

            for (int i = 0; i < numSohs; i++) {
                AreEqual(pickHeaders[i], pickList[i], exceptions);
            }

            // Delete the picks
            for(int i = 0; i < pickHeaders.Count(); i++) {
                PickService.DeletePick(pickHeaders[i]);
            }

            // Make sure they no longer exist
            pickList = PickService.FindPicksListModel(testCompany.Id, DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now).Items;
            expected = 0;
            actual = pickList.Count();
            Assert.IsTrue(actual == expected, "Error: {actual} picks were returned when {expected} were expected");
        }

        [TestMethod]
        public void DeletePickTest() {
            // Tested in FindPicksListModelTest() above
        }
    }
}
