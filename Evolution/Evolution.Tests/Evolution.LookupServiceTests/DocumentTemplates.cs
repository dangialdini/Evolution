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
        public void FindDocumentTemplatesModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindDocumentTemplatesModel();
            var dbData = db.FindDocumentTemplates();

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = LookupService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var newItem = createDocumentTemplate(testCompany, DocumentTemplateCategory.Pickslip, DocumentTemplateType.None);
            var error = LookupService.InsertOrUpdateDocumentTemplate(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindDocumentTemplatesModel();
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteDocumentTemplate(newItem.Id);

            model = LookupService.FindDocumentTemplatesModel();
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindDocumentTemplatesListItemModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.Invoice);
            var dbData = db.FindDocumentTemplates(DocumentTemplateCategory.Invoice);

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");

                var test = LookupService.MapToModel(dbItem);
                AreEqual(item, test);
            }

            // Add another item a make sure it is found
            var newItem = createDocumentTemplate(testCompany, DocumentTemplateCategory.Invoice, DocumentTemplateType.None);
            var error = LookupService.InsertOrUpdateDocumentTemplate(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.Invoice);
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteDocumentTemplate(newItem.Id);

            model = LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.Invoice);
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindDocumentTemplatesListModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindDocumentTemplatesListModel(0, 1, PageSize, "");
            var dbData = db.FindDocumentTemplates();

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
            var newItem = createDocumentTemplate(testCompany, DocumentTemplateCategory.Invoice, DocumentTemplateType.None);
            var error = LookupService.InsertOrUpdateDocumentTemplate(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindDocumentTemplatesListModel(0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            LookupService.DeleteDocumentTemplate(newItem.Id);

            model = LookupService.FindDocumentTemplatesListModel(0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindDocumentTemplateModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = LookupService.FindDocumentTemplatesModel();
            var dbData = db.FindDocumentTemplates();

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            var template1 = model.FirstOrDefault();
            var template2 = dbData.FirstOrDefault();

            var excludes = new List<string>();
            excludes.Add("TemplateCategory");       // Can't compare ints with enums
            excludes.Add("TemplateType");
            AreEqual(template1, template2, excludes);

            Assert.IsTrue(template1.TemplateCategory == (DocumentTemplateCategory)template2.TemplateCategory, $"Error: {template1.TemplateCategory} and {(DocumentTemplateCategory)template2.TemplateCategory} do not match");
            Assert.IsTrue(template1.TemplateType == (DocumentTemplateType)template2.TemplateType, $"Error: {template1.TemplateType} and {(DocumentTemplateType)template2.TemplateType} do not match");

            // Add another item and make sure it is found
            var newItem = createDocumentTemplate(testCompany, DocumentTemplateCategory.Invoice, DocumentTemplateType.None);
            var error = LookupService.InsertOrUpdateDocumentTemplate(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = LookupService.FindDocumentTemplatesModel();
            var testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, $"Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it dissapears
            LookupService.DeleteDocumentTemplate(newItem.Id);
            model = LookupService.FindDocumentTemplatesModel();
            testItem = model.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void InsertOrUpdateDocumentTemplateTest() {
            // Tested in DeleteDocumentTemplateTest, but additional tests here
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCurrency = db.FindCurrency("AUD");

            var testDocumentTemplate1 = createDocumentTemplate(testCompany, DocumentTemplateCategory.Invoice, DocumentTemplateType.None);
            var error = LookupService.InsertOrUpdateDocumentTemplate(testDocumentTemplate1, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindDocumentTemplate(testDocumentTemplate1.Id);

            var testModel = LookupService.MapToModel(test);

            AreEqual(testDocumentTemplate1, testModel);

            var testDocumentTemplate2 = createDocumentTemplate(testCompany, DocumentTemplateCategory.Pickslip, DocumentTemplateType.None);
            error = LookupService.InsertOrUpdateDocumentTemplate(testDocumentTemplate2, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindDocumentTemplate(testDocumentTemplate2.Id);

            testModel = LookupService.MapToModel(test);

            AreEqual(testDocumentTemplate2, testModel);


            // Try to create a DocumentTemplate with the same name
            var dupItem = LookupService.Clone(testDocumentTemplate1);
            dupItem.Id = 0;
            error = LookupService.InsertOrUpdateDocumentTemplate(dupItem, testUser, "");
            Assert.IsTrue(error.IsError, "Error: Creating a duplicate DocumentTemplate returned no error when it should have returned a 'duplicate' error");

            // Try to rename the item to a non-existing name (should work)
            string lgs = LookupService.LockDocumentTemplate(testDocumentTemplate1);

            testDocumentTemplate1.Name = RandomString();
            error = LookupService.InsertOrUpdateDocumentTemplate(testDocumentTemplate1, testUser, lgs);
            Assert.IsTrue(!error.IsError, error.Message);

            // Try to rename to an existing item (should fail)
            lgs = LookupService.LockDocumentTemplate(testDocumentTemplate1);

            testDocumentTemplate1.Name = testDocumentTemplate2.Name;
            error = LookupService.InsertOrUpdateDocumentTemplate(testDocumentTemplate1, testUser, lgs);
            Assert.IsTrue(error.IsError, "Error: Renaming a DocumentTemplate to the same name as an existing DocumentTemplate returned no error when it should have returned a 'duplicate' error");
        }

        [TestMethod]
        public void DeleteDocumentTemplateTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a price level
            DocumentTemplateModel model = createDocumentTemplate(testCompany, DocumentTemplateCategory.Invoice, DocumentTemplateType.None);

            var error = LookupService.InsertOrUpdateDocumentTemplate(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindDocumentTemplate(model.Id);
            var test = LookupService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            LookupService.DeleteDocumentTemplate(model.Id);

            // And check that is was deleted
            result = db.FindDocumentTemplate(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void LockDocumentTemplateTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCurrency = db.FindCurrency("AUD");

            // Create a record
            var model = createDocumentTemplate(testCompany, DocumentTemplateCategory.Invoice, DocumentTemplateType.None);

            var error = LookupService.InsertOrUpdateDocumentTemplate(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = LookupService.LockDocumentTemplate(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = LookupService.InsertOrUpdateDocumentTemplate(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = LookupService.InsertOrUpdateDocumentTemplate(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = LookupService.LockDocumentTemplate(model);
            error = LookupService.InsertOrUpdateDocumentTemplate(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void CopyDocumentTemplatesTest() {
            // Tested by all tests which create a test company
        }

        DocumentTemplateModel createDocumentTemplate(CompanyModel company, DocumentTemplateCategory templateCategory, DocumentTemplateType templateType) {
            DocumentTemplateModel model = new DocumentTemplateModel {
                Name = RandomString(),
                Description = RandomString(),
                TemplateCategory = templateCategory,
                TemplateType = templateType,
                TemplateFile = "",
                QualTemplateFile = GetAppSetting("SiteFolder", "") + $"\\App_Data",
                Enabled = true
            };
            return model;
        }
    }
}
