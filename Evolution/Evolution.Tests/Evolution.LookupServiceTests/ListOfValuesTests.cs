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
        public void FindLOVsListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var lovList = LookupService.FindLOVsListItemModel(true);
            int expected = 10,
                actual = lovList.Count();
            Assert.IsTrue(actual >= expected, $"Error: {actual} Multi-tenanted Lists of values were found when {expected} or more were expected");

            lovList = LookupService.FindLOVsListItemModel(false);
            expected = 10;
            actual = lovList.Count();
            Assert.IsTrue(actual >= expected, $"Error: {actual} NON Multi-tenanted Lists of values were found when {expected} or more were expected");

            // There is no ability to add new LOV's, therefore, there is no test for adding them
        }

        [TestMethod]
        public void FindLOVsModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            var lovList = LookupService.FindLOVsModel(true);
            int expected = 10,
                actual = lovList.Count();
            Assert.IsTrue(actual >= expected, $"Error: {actual} Multi-tenanted Lists of values were found when {expected} or more were expected");

            lovList = LookupService.FindLOVsModel(false);
            expected = 10;
            actual = lovList.Count();
            Assert.IsTrue(actual >= expected, $"Error: {actual} NON Multi-tenanted Lists of values were found when {expected} or more were expected");

            // There is no ability to add new LOV's, therefore, there is no test for adding them
        }

        [TestMethod]
        public void FindLOVItemsListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user, true);
            var lov = GetRandomLOV(testCompany.Id);

            var model = LookupService.FindLOVItemsListItemModel(testCompany, lov.LOVName);
            var dbData = db.FindLOVItems(testCompany.Id, lov.Id);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                Assert.IsTrue(item.Text == dbItem.ItemText, $"Error: Model Text is {item.Text} when {dbItem.ItemText} was expected");
            }

            // Add another item a make sure it is found
            var newItem = createLOVItem(testCompany, lov);
            var error = LookupService.InsertOrUpdateLOVItem(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindLOVItemsListItemModel(testCompany, lov.LOVName);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteLOVItem(newItem.Id);

            model = LookupService.FindLOVItemsListItemModel(testCompany, lov.LOVName);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindLOVItemsModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user, true);
            var lov = GetRandomLOV(testCompany.Id);

            var model = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 0, PageSize, "");
            var dbData = db.FindLOVItems(testCompany.Id, lov.Id);

            int expected = dbData.Count(),
                actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = LookupService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createLOVItem(testCompany, lov);
            var error = LookupService.InsertOrUpdateLOVItem(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 0, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteLOVItem(newItem.Id);

            model = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 0, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindLOVItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user, true);
            var lov = GetRandomLOV(testCompany.Id);

            var newItem = createLOVItem(testCompany, lov);
            var error = LookupService.InsertOrUpdateLOVItem(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var model = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 0, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteLOVItem(newItem.Id);

            model = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 0, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindLOVItemByValueModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user, true);

            var item = LookupService.FindLOVItemByValueModel(LOVName.SalesPersonType, "1");
            Assert.IsTrue(item != null, "Error: A NULL value was returned when an object was expected");
            string actual = item.ItemText,
                   expected = "Account Manager";
            Assert.IsTrue(actual == expected, $"Error: {actual} was returned when {expected} was expected");

            item = LookupService.FindLOVItemByValueModel(LOVName.SalesPersonType, "2");
            Assert.IsTrue(item != null, "Error: A NULL value was returned when an object was expected");
            actual = item.ItemText;
            expected = "Account Admin Manager";
            Assert.IsTrue(actual == expected, $"Error: {actual} was returned when {expected} was expected");
        }

        [TestMethod]
        public void MoveLOVItemUpTest() {
            // Check that moving an item up an LOV order list is correctly maintained
            int numItems = 5;
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            List<string> excludes = new List<string>();
            excludes.Add("OrderNo");        // Exclude this because it will change in these tests

            // Create a new LOV
            LOV lov = createLOVWithItems(testCompany, numItems);
            LOVListModel model = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 1, PageSize, "");

            // Move the last item up one place
            int idx = model.Items.Count() - 1;
            LookupService.MoveLOVItemUp(testCompany, lov.Id, model.Items[idx].Id);

            LOVListModel test = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 1, PageSize, "");
            idx--;
            AreEqual(model.Items[numItems - 1], test.Items[numItems - 2], excludes);
            int expectedOrderNo = idx + 1,
                actualOrderNo = test.Items[idx].OrderNo;
            Assert.IsTrue(actualOrderNo == expectedOrderNo, $"Error: OrderNo is {actualOrderNo} when {expectedOrderNo} was expected");

            // Now move it to the top and check that it got there
            while (idx > 0) {
                LookupService.MoveLOVItemUp(testCompany, lov.Id, test.Items[idx].Id);
                test = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 1, PageSize, "");
                idx--;
            }
            AreEqual(model.Items[numItems - 1], test.Items[idx], excludes);

            // Now try to move it up again (shouldn't move before OrderNo 1)
            LookupService.MoveLOVItemUp(testCompany, lov.Id, test.Items[0].Id);
            test = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 1, PageSize, "");
            AreEqual(model.Items[numItems - 1], test.Items[0], excludes);
        }

        [TestMethod]
        public void MoveLOVItemDownTest() {
            // Check that moving an item down an LOV order list is correctly maintained
            int numItems = 5;
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            List<string> excludes = new List<string>();
            excludes.Add("OrderNo");        // Exclude this because it will change in these tests

            // Create a new LOV
            LOV lov = createLOVWithItems(testCompany, numItems);
            LOVListModel model = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 1, PageSize, "");

            // Move the last item up one place
            int idx = 0;
            LookupService.MoveLOVItemDown(testCompany, lov.Id, model.Items[idx].Id);

            LOVListModel test = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 1, PageSize, "");
            idx++;
            AreEqual(model.Items[0], test.Items[1], excludes);
            int expectedOrderNo = idx + 1,
                actualOrderNo = test.Items[idx].OrderNo;
            Assert.IsTrue(actualOrderNo == expectedOrderNo, $"Error: OrderNo is {actualOrderNo} when {expectedOrderNo} was expected");

            // Now move it to the top and check that it got there
            while (idx < numItems) {
                LookupService.MoveLOVItemDown(testCompany, lov.Id, test.Items[idx].Id);
                test = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 1, PageSize, "");
                idx++;
            }
            AreEqual(model.Items[0], test.Items[numItems - 1], excludes);

            // Now try to move it down again (shouldn't move beyond end)
            LookupService.MoveLOVItemDown(testCompany, lov.Id, test.Items[0].Id);
            test = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 1, PageSize, "");
            AreEqual(model.Items[0], test.Items[numItems - 1], excludes);
        }

        [TestMethod]
        public void InsertOrUpdateLOVItemTest() {
            // Tested in FindLOVItemModelTest and multiple other tests, but
            // with extra tests here

            // Get a test user
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var lov = GetRandomLOV(testCompany.Id);

            // Create some LOV items
            var testItem1 = createLOVItem(testCompany, lov);
            var error = LookupService.InsertOrUpdateLOVItem(testItem1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindLOVItem(testItem1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testItem1, testModel);


            var testItem2 = createLOVItem(testCompany, lov);
            error = LookupService.InsertOrUpdateLOVItem(testItem2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindLOVItem(testItem2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testItem2, testModel);


            // Try to create an item with the same text
            var dupItem = LookupService.Clone(testItem1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateLOVItem(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate LOVItem returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockLOVItem(testItem1);

            testItem1.ItemText = RandomString();
            error = LookupService.InsertOrUpdateLOVItem(testItem1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename the item to an existing item text (should fail)
            lgs = LookupService.LockLOVItem(testItem1);

            testItem1.ItemText = testItem2.ItemText;
            error = LookupService.InsertOrUpdateLOVItem(testItem1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming an LOVItem to the same name as an existing LOVItem returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteLOVItemTest() {
            // This test takes a while to run because it takes a copy of the LOVs

            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user, true);
            var lov = GetRandomLOV(testCompany.Id);

            // Create an LOV item
            LOVItemModel model = createLOVItem(testCompany, lov);

            var error = LookupService.InsertOrUpdateLOVItem(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindLOVItem(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteLOVItem(model.Id);

            // And check that is was deleted
            result = db.FindLOVItem(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockLOVItemTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var lov = GetRandomLOV(testCompany.Id);

            // Create a record
            var model = createLOVItem(testCompany, lov);

            var error = LookupService.InsertOrUpdateLOVItem(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockLOVItem(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateLOVItem(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateLOVItem(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockLOVItem(model);
            error = LookupService.InsertOrUpdateLOVItem(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void CopyLOVsTest() {
            // Tested by all tests which create a test company
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }

        [TestMethod]
        public void CloneTest() {
            // Tested by all READ tests in this module
        }

        LOVItemModel createLOVItem(CompanyModel company, LOV lov) {
            LOVItemModel model = new LOVItemModel {
                CompanyId = company.Id,
                LovId = lov.Id,
                ItemText = RandomString(),
                ItemValue1 = RandomString().Left(16),
                ItemValue2 = RandomString().Left(16),
                Enabled = true
            };
            return model;
        }

        LOV GetRandomLOV(int? companyId, bool bMultiTenanted = true) {
            // Gets a random LOV for the parameter company which has items in it
            List<LOV> lovList = db.FindLOVs(bMultiTenanted)
                                  .Where(lov => lov.LOVItems
                                                   .Where(lovi => lovi.CompanyId == companyId)
                                                   .Count() > 0)
                                  .ToList();

            int random = RandomInt(0, lovList.Count() - 1);

            return lovList[random];
        }

        LOV createLOVWithItems(CompanyModel company, int numItemsToAdd) {
            LOV lov = new LOV {
                LOVName = RandomString(),
                MultiTenanted = true,
                Enabled = true
            };
            db.InsertOrUpdateLOV(lov);

            // Now add some items
            for(int i = 0; i < numItemsToAdd; i++) {
                LOVItem newItem = new LOVItem {
                    LovId = lov.Id,
                    CompanyId = company.Id,
                    ItemText = RandomString(),
                    OrderNo = i + 1,
                    Enabled = true
                };
                db.InsertOrUpdateLOVItem(newItem);
            }

            return lov;
        }
    }
}
