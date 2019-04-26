using System;
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
        public void FindPortsListItemModelTest() {
            var user = GetTestUser();
            var model = LookupService.FindPortsListItemModel();
            var dbData = db.FindPorts();

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.PortName, $"Error: Model Text is {item.Text} when {dbItem.PortName} was expected");
            }

            // Add another item a make sure it is found
            var newItem = createPort();
            var error = LookupService.InsertOrUpdatePort(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindPortsListItemModel();
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeletePort(newItem.Id);

            model = LookupService.FindPortsListItemModel();
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void InsertOrUpdatePortTest() {
            // Tested in DeletePortTest, but additional tests here
            var testUser = GetTestUser();

            var testPort1 = createPort();
            var error = LookupService.InsertOrUpdatePort(testPort1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindPort(testPort1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testPort1, testModel);

            var testPort2 = createPort();
            error = LookupService.InsertOrUpdatePort(testPort2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindPort(testPort2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testPort2, testModel);


            // Try to create a Port with the same name
            var dupItem = LookupService.Clone(testPort1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdatePort(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate Port returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockPort(testPort1);

            testPort1.PortName = RandomString().Left(30);
            error = LookupService.InsertOrUpdatePort(testPort1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockPort(testPort1);

            testPort1.PortName = testPort2.PortName;
            error = LookupService.InsertOrUpdatePort(testPort1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a Port to the same name as an existing Port returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeletePortTest() {
            // Get a test user
            var user = GetTestUser();

            // Create a price level
            PortModel model = createPort();

            var error = LookupService.InsertOrUpdatePort(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindPort(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeletePort(model.Id);

            // And check that is was deleted
            result = db.FindPort(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockPortTest() {
            var testUser = GetTestUser();

            // Create a record
            var model = createPort();

            var error = LookupService.InsertOrUpdatePort(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockPort(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdatePort(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdatePort(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockPort(model);
            error = LookupService.InsertOrUpdatePort(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        private PortModel createPort()
        {
            var model = new PortModel {
                PortName = RandomString(),
                Enabled = true
            };
            return model;
        }
    }
}
