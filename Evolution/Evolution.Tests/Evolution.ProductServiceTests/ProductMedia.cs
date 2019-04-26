using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
        public void FindProductMediaListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a product
            var testProduct = createProduct(testCompany, testUser);
            var error = ProductService.InsertOrUpdateProduct(testProduct, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check the media
            var productMedias = ProductService.FindProductMediaListModel(testProduct.Id, 0).Items;
            int expected = 0,
                actual = productMedias.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Add some media items
            string[] files = { "EvolutionLogo.png",
                               "IconExclamation.png",
                               "Processing.gif" };

            var prodMediaModels = new List<ProductMediaModel>();

            foreach (string fileName in files) {
                var sourceFile = GetAppSetting("SiteFolder", "") + "\\Content\\" + fileName;

                var prodMedia = new ProductMediaModel();
                error = ProductService.AddMediaToProduct(testProduct, testCompany, testUser, sourceFile, prodMedia, FileCopyType.Copy);
                Assert.IsTrue(!error.IsError, error.Message);

                prodMediaModels.Add(prodMedia);
            }

            // Check the media
            while (prodMediaModels.Count > 0) {
                productMedias = ProductService.FindProductMediaListModel(testProduct.Id, 0).Items;
                expected = prodMediaModels.Count();
                actual = productMedias.Count();
                Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

                error = ProductService.DeleteProductMedia(prodMediaModels[0].Id);
                Assert.IsTrue(!error.IsError, error.Message);

                prodMediaModels.RemoveAt(0);
            }

            // Final check
            productMedias = ProductService.FindProductMediaListModel(testProduct.Id, 0).Items;
            expected = 0;
            actual = productMedias.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
        }

        [TestMethod]
        public void GetProductImageTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a product
            var testProduct = createProduct(testCompany, testUser);
            var error = ProductService.InsertOrUpdateProduct(testProduct, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check the media
            var productMedias = ProductService.FindProductMediaListModel(testProduct.Id, 0).Items;
            int expected = 0,
                actual = productMedias.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Add a media item
            var sourceFile = GetAppSetting("SiteFolder", "") + "\\Content\\EvolutionLogo.png";

            var prodMedia = new ProductMediaModel();
            error = ProductService.AddMediaToProduct(testProduct, testCompany, testUser, sourceFile, prodMedia, FileCopyType.Copy);
            Assert.IsTrue(!error.IsError, error.Message);

            // Set it as the primary media
            error = ProductService.SetPrimaryMedia(testProduct, prodMedia, testUser, ProductService.LockProduct(testProduct));
            Assert.IsTrue(!error.IsError, error.Message);

            // Get the image
            MediaSize[] sizes = { MediaSize.Large,
                                  MediaSize.Medium,
                                  MediaSize.Small };

            foreach (var mediaSize in sizes) {
                string prodImage = ProductService.GetProductImage(testProduct, mediaSize, 10, 10, true).FileName();
                Assert.IsTrue(!string.IsNullOrEmpty(prodImage), $"Error: an image URL was expected but an empty/NULL string was returned");

                string compare = "EvolutionLogo.png";
                Assert.IsTrue(prodImage == compare, $"Error: {prodImage} was returned when {compare} was expected");
            }

            // Cleanup
            error = ProductService.DeleteProduct(testProduct.Id);
            Assert.IsTrue(!error.IsError, error.Message);
        }

        [TestMethod]
        public void AddMediaToProductTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a product
            var testProduct = createProduct(testCompany, testUser);
            var error = ProductService.InsertOrUpdateProduct(testProduct, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Create some media
            var sourceFile = GetAppSetting("SiteFolder", "") + "\\Content\\EvolutionLogo.png";

            var media = new MediaModel { };
            error = MediaServices.InsertOrUpdateMedia(media, testCompany, testUser, Enumerations.MediaFolder.Product, sourceFile, "", testProduct.Id, -1, FileCopyType.None);
            Assert.IsTrue(!error.IsError, error.Message);

            // Add the media to the product
            var prodMedia = new ProductMediaModel();
            error = ProductService.AddMediaToProduct(testProduct, testCompany, testUser, sourceFile, prodMedia, FileCopyType.Copy);
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that the product has its media attached
            var checkProd = db.FindProduct(testProduct.Id);
            Assert.IsTrue(checkProd != null, $"Error: A NULL value was returned when an objetc was expected");

            var checkProdMedias = checkProd.ProductMedias.ToList();
            int expected = 1,
                actual = checkProdMedias.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} were expected");

            var checkProdMedia = checkProd.ProductMedias.First();
            expected = prodMedia.Id;
            actual = checkProd.PrimaryMediaId.Value;
            Assert.IsTrue(actual == expected, $"Error: The ProductMedia record references Media Id #{actual} when #{expected} were expected");

            // Check that the primary media is what was set
            var checkMedia = checkProdMedia.Medium;
            expected = prodMedia.MediaId;
            actual = checkMedia.Id;
            Assert.IsTrue(actual == expected, $"Error: Media record Id #{actual} was found when #{expected} was expected");

            // Check that the media has a media file
            var media2 = MediaServices.FindMediaModel(checkMedia.Id);
            var mediaFile = MediaServices.GetMediaFileName(media2, false);
            Assert.IsTrue(File.Exists(mediaFile), $"Error: File {mediaFile} was not found");

            // Clean up - delete the product
            error = ProductService.DeleteProduct(testProduct.Id);   // This deletes the media as well
            Assert.IsTrue(!error.IsError, error.Message);
        }

        [TestMethod]
        public void SetPrimaryMediaTest() {
            // Tested in GetProductImageTest() above
        }

        [TestMethod]
        public void InsertOrUpdateProductMediaTest() {
            // Tested by all tests which add media to a product
        }

        [TestMethod]
        public void DeleteProductMediaTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a product
            var testProduct = createProduct(testCompany, testUser);
            var error = ProductService.InsertOrUpdateProduct(testProduct, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Create some media
            var sourceFile = GetAppSetting("SiteFolder", "") + "\\Content\\EvolutionLogo.png";

            var media = new MediaModel { };
            error = MediaServices.InsertOrUpdateMedia(media, testCompany, testUser, Enumerations.MediaFolder.Product, sourceFile, "", testProduct.Id, -1, FileCopyType.None);
            Assert.IsTrue(!error.IsError, error.Message);

            // Add the media to the product
            var prodMedia = new ProductMediaModel();
            error = ProductService.AddMediaToProduct(testProduct, testCompany, testUser, sourceFile, prodMedia, FileCopyType.Copy);
            Assert.IsTrue(!error.IsError, error.Message);

            var checkMedia = MediaServices.FindMediaModel(prodMedia.MediaId);
            var mediaFile = MediaServices.GetMediaFileName(checkMedia, false);

            // Now delete the product media
            error = ProductService.DeleteProductMedia(prodMedia.Id);
            Assert.IsTrue(!error.IsError, error.Message);

            Assert.IsTrue(!File.Exists(mediaFile), $"Error: Media file {mediaFile} was found when it should have been deleted");
        }
    }
}
