using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Extensions;

namespace Evolution.CompanyServiceTests {
    public partial class CompanyServiceTests : BaseTest {
        // There are no tests for deleting companies because companies can only
        // be 'soft' deleted ie disabled

        [TestMethod]
        public void FindCompaniesListItemModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Search for the companies
            var result = CompanyService.FindCompaniesListItemModel()
                                       .OrderBy(c => c.Id)
                                       .ToList();
            int expectedResult = 1;
            Assert.IsTrue(result.Count >= expectedResult, $"Error: {result.Count} records were found when >{expectedResult} were expected");

            // Check that our test company appears in the list
            var checkCompany = result.Where(r => r.Id == testCompany.Id.ToString())
                                     .SingleOrDefault();
            Assert.IsNotNull(checkCompany, $"Error: Test Company record Id {testCompany.Id} was expected but none was returned");

            // Disable the test company and check again
            string lockGuid = CompanyService.LockCompany(testCompany);

            testCompany.Enabled = false;
            var error = CompanyService.InsertOrUpdateCompany(testCompany, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            var result2 = CompanyService.FindCompaniesListItemModel()
                                        .OrderBy(c => c.Id)
                                        .ToList();

            // Check that our test company doesn't appear in the list
            checkCompany = result2.Where(r => r.Id == testCompany.Id.ToString())
                                  .SingleOrDefault();
            if (checkCompany != null) Assert.Fail($"Error: Test Company record Id {checkCompany.Id} was returned when none was expected");
        }

        [TestMethod]
        public void FindCompaniesModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Search for the companies
            var result = CompanyService.FindCompaniesModel()
                                       .OrderBy(c => c.Id)
                                       .ToList();
            int expectedResult = 1;
            Assert.IsTrue(result.Count >= expectedResult, $"Error: {result.Count} records were found when >{expectedResult} were expected");

            // Check that our test company appears in the list
            var checkCompany = result.Where(r => r.Id == testCompany.Id)
                                     .SingleOrDefault();
            Assert.IsNotNull(checkCompany, $"Error: Test Company record Id {testCompany.Id} was expected but none was returned");

            // Disable the test company and check again
            string lockGuid = CompanyService.LockCompany(testCompany);

            testCompany.Enabled = false;
            var error = CompanyService.InsertOrUpdateCompany(testCompany, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            var result2 = CompanyService.FindCompaniesModel()
                                        .OrderBy(c => c.Id)
                                        .ToList();

            // Check that our test company doesn't appear in the list
            checkCompany = result2.Where(r => r.Id == testCompany.Id)
                                  .SingleOrDefault();
            if (checkCompany != null) Assert.Fail($"Error: Test Company record Id {checkCompany.Id} was returned when none was expected");
        }

        [TestMethod]
        public void FindCompaniesListModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Search for the companies
            var result = CompanyService.FindCompaniesListModel(0, 1, PageSize, "");
            int expectedResult = 1;
            Assert.IsTrue(result.Items.Count >= expectedResult, $"Error: {result.Items.Count} records were found when >{expectedResult} were expected");

            // Check that our test company appears in the list
            var checkCompany = result.Items
                                     .Where(r => r.Id == testCompany.Id)
                                     .SingleOrDefault();
            Assert.IsNotNull(checkCompany, $"Error: Test Company record Id {testCompany.Id} was expected but none was returned");

            // Disable the test company and check again
            string lockGuid = CompanyService.LockCompany(testCompany);

            testCompany.Enabled = false;
            var error = CompanyService.InsertOrUpdateCompany(testCompany, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            var result2 = CompanyService.FindCompaniesListModel(0, 1, PageSize, "");

            // Check that our test company doesn't appear in the list
            checkCompany = result2.Items
                                  .Where(r => r.Id == testCompany.Id)
                                  .SingleOrDefault();
            if (checkCompany != null) Assert.Fail($"Error: Test Company record Id {checkCompany.Id} was returned when none was expected");
        }

        [TestMethod]
        public void FindCompanyTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Search for the companies
            var result = CompanyService.FindCompany(testCompany.Id);
            Assert.IsTrue(result != null, "Error: NULL as returned when a company was expected");

            var temp = CompanyService.MapToModel(result);
            AreEqual(testCompany, temp);
        }

        [TestMethod]
        public void FindCompanyModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Search for the companys
            var result = CompanyService.FindCompanyModel(testCompany.Id, false);
            Assert.IsTrue(result != null, "Error: NULL as returned when a company was expected");
            AreEqual(testCompany, result);
        }

        [TestMethod]
        public void FindCompanyFriendlyNameModelTest() {
            // Tested by GetTestCompany ie in all tests which get a test company
        }

        [TestMethod]
        public void FindParentCompanyModelTest() {
            // Tested by GetTestCompany ie in all tests which get a test company
        }

        [TestMethod]
        public void InsertOrUpdateCompanyTest() {
            // Tested by all tests which create a company, but additional tests here
            var testUser = GetTestUser();

            var testCompany1 = GetTestCompany(testUser);

            var test = db.FindCompany(testCompany1.Id);

            var testModel = CompanyService.MapToModel(test);

            AreEqual(testCompany1, testModel);

            var testCompany2 = GetTestCompany(testUser);

            test = db.FindCompany(testCompany2.Id);

            testModel = CompanyService.MapToModel(test);

            AreEqual(testCompany2, testModel);


            // Try to create a Company with the same name
            var dupItem = CompanyService.MapToModel(testCompany1);
            dupItem.Id = 0;
            var error = CompanyService.InsertOrUpdateCompany(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate Company returned no error when it should have returned a 'duplicate' error");

            // Try to rename the CompanyName to a non-existing name (should work)
            string lgs = CompanyService.LockCompany(testCompany1);

            testCompany1.CompanyName = RandomString();
            testCompany1.FriendlyName = testCompany1.CompanyName.Left(32);
            error = CompanyService.InsertOrUpdateCompany(testCompany1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = CompanyService.LockCompany(testCompany1);

            testCompany1.CompanyName = testCompany2.CompanyName;
            testCompany1.FriendlyName = testCompany2.FriendlyName;
            error = CompanyService.InsertOrUpdateCompany(testCompany1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a Company to the same name as an existing Company returned no error when it should have returned a 'duplicate' error");

            // Try to rename the FriendlyName to a non-existing name (should work)
            lgs = CompanyService.LockCompany(testCompany1);

            testCompany1.CompanyName = RandomString();
            testCompany1.FriendlyName = testCompany1.CompanyName.Left(32);
            error = CompanyService.InsertOrUpdateCompany(testCompany1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = CompanyService.LockCompany(testCompany1);

            testCompany1.FriendlyName = testCompany2.CompanyName;
            error = CompanyService.InsertOrUpdateCompany(testCompany1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a Company to the same name as an existing Company returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void LockCompanyTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Lock it
            string lockGuid = CompanyService.LockCompany(testCompany);

            // Check that the lock was written
            var lockRecord = db.FindLock(typeof(Company).ToString(), testCompany.Id);
            Assert.IsTrue(lockRecord != null, "Error: Lock record was not found");
            Assert.IsTrue(lockRecord.LockGuid == lockGuid, $"Error: Lock Guid was {lockRecord.LockGuid} when {lockGuid} was expected");

            // We can't test lock being deleted if company is deleted
            // because we never hard-delete companies
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }
    }
}
