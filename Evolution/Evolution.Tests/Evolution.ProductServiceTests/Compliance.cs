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

namespace Evolution.ProductServiceTests {
    public partial class ProductServiceTests {
        [TestMethod]
        public void FindProductComplianceListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            // Create a product
            var product = createProduct(testCompany, testUser);
            ProductService.InsertOrUpdateProduct(product, testUser, "");

            // Test there is no ProductComplianceList
            var model = ProductService.FindProductComplianceListModel(product.Id);
            var dbData = db.FindProductCompliances(product.Id);

            int expected = dbData.Count();
            int actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach(var item in model.Items) {
                var dbItem = dbData.Where(r => r.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = ProductService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            //  Add a new item and make sure it is found
            var newItem = createProductCompliance(testCompany, product);
            ProductService.InsertOrUpdateProductCompliance(newItem, "");

            model = ProductService.FindProductComplianceListModel(product.Id);
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was return when a non-NULL value was expected");

            // Delete it and make sure it dissapears
            ProductService.DeleteProductCompliance(newItem.Id);
            model = ProductService.FindProductComplianceListModel(newItem.Id);
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was return when a NULL value was expected");
        }

        [TestMethod]
        public void FindProductComplianceModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            
            // Create a Product Compliance
            var product = createProduct(testCompany, testUser);
            ProductService.InsertOrUpdateProduct(product, testUser, "");
            var model = createProductCompliance(testCompany, product);
            var error = ProductService.InsertOrUpdateProductCompliance(model, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = ProductService.FindProductComplianceModel(model.Id, product.Id);
            AreEqual(model, test);
        }

        [TestMethod]
        public void FindProductComplianceAttachmentListModelTest() {
            // Tested in:
            //  FindProductComplianceAttachmentListModelTest
            //  DeleteProductComplianceAttachmentTest
        }

        [TestMethod]
        public void AddMediaToProductComplianceTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var product = createProduct(testCompany, testUser);
            ProductService.InsertOrUpdateProduct(product, testUser, "");

            var pcList = ProductService.FindProductComplianceListModel(product.Id);
            if(pcList.Items.Count > 0) {
                foreach (var pc in pcList.Items)
                    ProductService.DeleteProductCompliance(pc.Id);
            }

            // Check number of pc in list before test
            pcList = ProductService.FindProductComplianceListModel(product.Id);
            int expected = 0;
            int actual = pcList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Create X number of records
            int randomNoOfRecords = RandomInt(5, 20);
            for(var i = 0; i < randomNoOfRecords; i++) {
                // Create Product Compliance
                var prodCom = createProductCompliance(testCompany, product);
                var error = ProductService.InsertOrUpdateProductCompliance(prodCom, "");
                Assert.IsTrue(!error.IsError, error.Message);
                // Create Media
                var media = new MediaModel();
                var targetFile = GetTempFile(".jpg");
                error = MediaServices.InsertOrUpdateMedia(media, testCompany, testUser, Evolution.Enumerations.MediaFolder.ProductCompliance,
                                                                targetFile, "", prodCom.ProductId, prodCom.Id, FileCopyType.Move);
                Assert.IsTrue(!error.IsError, error.Message);

                ProductService.AddMediaToProductCompliance(prodCom, media);
            }
            
            pcList = ProductService.FindProductComplianceListModel(product.Id);
            expected = randomNoOfRecords;
            actual = pcList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Delete them
            foreach(var pc in pcList.Items) {
                ProductService.DeleteProductCompliance(pc.Id);
            }
            // Check number of pc in list before test
            pcList = ProductService.FindProductComplianceListModel(product.Id);
            expected = 0;
            actual = pcList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");
        }

        [TestMethod]
        public void InsertOrUpdateProductComplianceTest() {
            // Tested in DeleteProductComplianceTest, but additional tests here
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var product = createProduct(testCompany, testUser);
            ProductService.InsertOrUpdateProduct(product, testUser, "");
            var testPC1 = createProductCompliance(testCompany, product);
            var error = ProductService.InsertOrUpdateProductCompliance(testPC1, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var test = db.FindProductCompliance(testPC1.Id);
            var testModel = ProductService.MapToModel(test);
            AreEqual(testPC1, testModel);

            var testPC2 = createProductCompliance(testCompany, product);
            error = ProductService.InsertOrUpdateProductCompliance(testPC2, "");
            Assert.IsTrue(!error.IsError, error.Message);

            test = db.FindProductCompliance(testPC2.Id);
            testModel = ProductService.MapToModel(test);
            AreEqual(testPC2, testModel);
        }

        [TestMethod]
        public void DeleteProductComplianceTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var product = createProduct(testCompany, testUser);
            ProductService.InsertOrUpdateProduct(product, testUser, "");
            var model = createProductCompliance(testCompany, product);
            var error = ProductService.InsertOrUpdateProductCompliance(model, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindProductCompliance(model.Id);
            var test = ProductService.MapToModel(result);
            AreEqual(model, test);

            // Now delete it
            ProductService.DeleteProductCompliance(model.Id);

            // And check that is was deleted
            result = db.FindProductCompliance(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        [TestMethod]
        public void DeleteProductComplianceAttachmentTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a Product Compliance
            var product = createProduct(testCompany, testUser);
            ProductService.InsertOrUpdateProduct(product, testUser, "");
            var productComp = createProductCompliance(testCompany, product);
            var error = ProductService.InsertOrUpdateProductCompliance(productComp, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check number of PC attachments before test
            var pcaList = ProductService.FindProductComplianceAttachmentListModel(productComp.Id);
            int expected = 0;
            int actual = pcaList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Create X number of Product Compliance Attachments
            int randomNoOfRecords = RandomInt(5, 20);
            for (var i = 0; i < randomNoOfRecords; i++) {
                // Create Attachment
                var media = new MediaModel();
                var targetFile = GetTempFile(".jpg");
                error = MediaServices.InsertOrUpdateMedia(media, testCompany, testUser, Evolution.Enumerations.MediaFolder.ProductCompliance,
                                                                targetFile, "", productComp.ProductId, productComp.Id, FileCopyType.Move);
                Assert.IsTrue(!error.IsError, error.Message);
                ProductService.AddMediaToProductCompliance(productComp, media);
            }

            pcaList = ProductService.FindProductComplianceAttachmentListModel(productComp.Id);
            expected = randomNoOfRecords;
            actual = pcaList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Delete them
            foreach (var pca in pcaList.Items) {
                ProductService.DeleteProductComplianceAttachment(pca);
            }
            // Check number of pc in list before test
            pcaList = ProductService.FindProductComplianceAttachmentListModel(productComp.Id);
            expected = 0;
            actual = pcaList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");
        }

        [TestMethod]
        public void LockProductComplianceTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var product = createProduct(testCompany, testUser);
            ProductService.InsertOrUpdateProduct(product, testUser, "");
            var model = createProductCompliance(testCompany, product);
            var error = ProductService.InsertOrUpdateProductCompliance(model, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Get the current Lock
            string lockGuid = ProductService.LockProductCompliance(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = ProductService.InsertOrUpdateProductCompliance(model, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = ProductService.InsertOrUpdateProductCompliance(model, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = ProductService.LockProductCompliance(model);
            error = ProductService.InsertOrUpdateProductCompliance(model, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        private ProductComplianceModel createProductCompliance(CompanyModel testCompany, ProductModel product) {
            var cc = LookupService.FindLOVItemsModel(testCompany, LOVName.ComplianceCategory).FirstOrDefault();
            var market = LookupService.FindLOVItemsModel(testCompany, LOVName.MarketingLocation).FirstOrDefault();

            var model = new ProductComplianceModel();
            model.ProductId = product.Id;
            model.ComplianceCategoryId = cc.Id;
            model.ComplianceCategoryText = cc.ItemText;
            model.MarketId = market.Id;
            model.MarketNameText = market.ItemText;
            return model;
        }
    }
}
