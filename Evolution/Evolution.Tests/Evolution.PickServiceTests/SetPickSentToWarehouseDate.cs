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
        public void SetPickSentToWarehouseDateTest() {
            int numSohs = 1,
                itemCount = 15;

            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            CreateTestTransfers(testCompany, testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var sohList = new List<SalesOrderHeaderModel>();
            var pickHeaders = new List<PickHeaderModel>();

            // Create some SOHs
            for (int i = 0; i < numSohs; i++) {
                sohList.Add(GetTestSalesOrderHeader(testCompany, testCustomer, testUser, itemCount, true));
            }

            var error = PickService.CreatePicks(testCompany, sohList, true, pickHeaders);

            // Log all the files so they get cleaned up at test tear-down
            foreach (var pickH in pickHeaders) {
                db.LogTestFile(pickH.PickFiles);
                //PickService.SetPickSentToWarehouseDate(pickH, DateTimeOffset.Now);
            }

            // Check the sent to warehouse state
            var pickList = PickService.FindPicksListModel(testCompany.Id, null, null);
            int expected = 1,
                actual = pickList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} picks were created when {expected} were expected");

            Assert.IsTrue(pickList.Items[0].STWDate == null, $"Error: {pickList.Items[0].STWDate} for STW Date was found when NULL was expected");

            // Change the state
            var stwDate = DateTimeOffset.Now;
            PickService.SetPickSentToWarehouseDate(pickList.Items[0], stwDate);

            // Check it
            pickList = PickService.FindPicksListModel(testCompany.Id, DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now);
            expected = 1;
            actual = pickList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} picks were created when {expected} were expected");

            Assert.IsTrue(pickList.Items[0].STWDate == stwDate, $"Error: {pickList.Items[0].STWDate} for STW Date was found when {stwDate} was expected");
        }
    }
}
