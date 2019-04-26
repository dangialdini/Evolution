using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.LookupService;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.LookupServiceTests {
    public partial class LookupServiceTests {
        [TestMethod]
        public void FindLocationListItemModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompanyAU();
            var locationList = LookupService.FindLocationListItemModel(testCompany);
            Assert.IsTrue(locationList.Count() >= 10, $"Error: {locationList.Count()} Locations were found when 10 or more were expected");

            // TBD: 12/10/2017 Need to add tests to create and delete locations.
            //                 At the time of writing the service APIs don't exist for this.
        }

        [TestMethod]
        public void FindLocationModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var model = createLocation(testCompany);
            var error = LookupService.InsertOrUpdateLocation(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = LookupService.FindLocationModel(model.Id, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void InsertOrUpdateLocationTest() {
            // Tested in DeleteLocationTest, but additional tests here
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            var testLocation1 = createLocation(testCompany);
            var error = LookupService.InsertOrUpdateLocation(testLocation1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindLocation(testLocation1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testLocation1, testModel);

            var testLocation2 = createLocation(testCompany);
            error = LookupService.InsertOrUpdateLocation(testLocation2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindLocation(testLocation2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testLocation2, testModel);


            // Try to create a Location with the same name
            var dupItem = LookupService.Clone(testLocation1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateLocation(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate Location Id returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockLocation(testLocation1);

            testLocation1.LocationIdentification = RandomString().Left(10);
            error = LookupService.InsertOrUpdateLocation(testLocation1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockLocation(testLocation1);

            testLocation1.LocationIdentification = testLocation2.LocationIdentification;
            error = LookupService.InsertOrUpdateLocation(testLocation1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a Location Id to the same name as an existing Locatoin returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteLocationTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a price level
            LocationModel model = createLocation(testCompany);

            var error = LookupService.InsertOrUpdateLocation(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindLocation(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteLocation(model.Id);

            // And check that is was deleted
            result = db.FindLocation(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockLocationTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = createLocation(testCompany);

            var error = LookupService.InsertOrUpdateLocation(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockLocation(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateLocation(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateLocation(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockLocation(model);
            error = LookupService.InsertOrUpdateLocation(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        LocationModel createLocation(CompanyModel company) {
            LocationModel model = new LocationModel {
                CompanyId = company.Id,
                LocationIdentification = RandomString().Left(10),
                LocationName = RandomString().Left(30),
                Enabled = true
            };
            return model;
        }
    }
}
