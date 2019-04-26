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
        public void CreatePicksTest() {
            int numSohs = 6,
                itemCount = 5;

            var sohList = new List<SalesOrderHeaderModel>();
            var pickHeaders = new List<PickHeaderModel>();

            createPicksTest(numSohs, itemCount, sohList, false, pickHeaders);

            // Check that multiple picks have been created
            int expected = numSohs,
                actual = pickHeaders.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} picks were created when {expected} were expected");

            // Check that each pick has the correct number of items
            for (int i = 0; i < numSohs; i++) {
                expected = itemCount;
                actual = pickHeaders[i].PickDetails.Count();
                Assert.IsTrue(actual == expected, $"Error: {actual} items were created on pick #{i} when {expected} were expected");

                Assert.IsTrue(File.Exists(pickHeaders[i].PickFiles[0]), $"Error: Pick text file {pickHeaders[i].PickFiles[0]} could not be found");
                expected = 1;
                actual = pickHeaders[i].PickFiles.Count();
                Assert.IsTrue(actual == expected, $"Error: {actual} Pick text file(s) were found when {expected} was expected");
            }
        }

        [TestMethod]
        public void CombinePicksTest() {
            int numSohs = 6,
                itemCount = 5;

            var sohList = new List<SalesOrderHeaderModel>();
            var pickHeaders = new List<PickHeaderModel>();

            createPicksTest(numSohs, itemCount, sohList, true, pickHeaders);

            // Check that a single pick has has been created which combines all orders
            int expected = 1,
                actual = pickHeaders.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} picks were created when {expected} were expected");

            // Check that the pick contains all items
            expected = numSohs * itemCount;
            actual = pickHeaders[0].PickDetails.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were created on pick when {expected} were expected");

            Assert.IsTrue(File.Exists(pickHeaders[0].PickFiles[0]), $"Error: Pick text file {pickHeaders[0].PickFiles[0]} could not be found");
            expected = 1;
            actual = pickHeaders[0].PickFiles.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} Pick text file(s) were found when {expected} was expected");

            // Need to do tests around different combining rules eg all from same supplier
        }

        void createPicksTest(int numSohs, int itemCount, 
                             List<SalesOrderHeaderModel> sohList, 
                             bool bCombine, 
                             List<PickHeaderModel> pickHeaders) {

            // Create a test sale
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            CreateTestTransfers(testCompany, testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Create some SOHs
            for (int i = 0; i < numSohs; i++) {
                sohList.Add(GetTestSalesOrderHeader(testCompany, testCustomer, testUser, itemCount, true));
            }

            var error = PickService.CreatePicks(testCompany, sohList, bCombine, pickHeaders);

            // Log all the files so they get cleaned up at test tear-down
            foreach (var pickH in pickHeaders) db.LogTestFile(pickH.PickFiles);

            Assert.IsTrue(!error.IsError, error.Message);
        }
    }
}
