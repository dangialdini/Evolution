using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.ProductServiceTests {
    public partial class ProductServiceTests {
        [TestMethod]
        public void FindBrandBrandCategoriesListItemModelTest() {
            // BrandBrandCategory links brands to a brand category
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            // Create a brandcategory with brands
            int expected = 10;
            var brandCategory1 = createBrandCategoryWithBrands(testCompany, user, expected);

            // Now get the BrandBrandCategory list
            var brandBrandCatList1 = ProductService.FindBrandBrandCategoriesListItemModel(brandCategory1);
            int actual = brandBrandCatList1.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Create a second brand category with list and make sure the two don't interfere with each other
            expected = 4;
            var brandCategory2 = createBrandCategoryWithBrands(testCompany, user, expected);

            // Now get the BrandBrandCategory list
            var brandBrandCatList2 = ProductService.FindBrandBrandCategoriesListItemModel(brandCategory2);
            actual = brandBrandCatList2.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            expected = 10;
            brandBrandCatList1 = ProductService.FindBrandBrandCategoriesListItemModel(brandCategory1);
            actual = brandBrandCatList1.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Now drop a brand and check the categories again
            // Note that FindBrandBrandCategoriesListItemModel returns BrandIds in its records and NOT BrandBrandCategory.Id
            int tempBrandId = Convert.ToInt32(brandBrandCatList1[0].Id);
            var temp = db.FindBrandBrandCategories(brandCategory1.Id)
                         .Where(bbc => bbc.BrandId == tempBrandId)
                         .FirstOrDefault();
            Assert.IsTrue(temp != null, "Error: A NULL value was returned when a non-NULL value was expected");
            var tempBrand = ProductService.FindBrandModel(temp.BrandId);

            ProductService.DeleteBrandFromBrandCategory(tempBrand, brandCategory1);

            expected = 9;
            brandBrandCatList1 = ProductService.FindBrandBrandCategoriesListItemModel(brandCategory1);
            actual = brandBrandCatList1.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            expected = 25;
            brandCategory2 = createBrandCategoryWithBrands(testCompany, user, expected);

            // Now get the BrandBrandCategory list again
            brandBrandCatList2 = ProductService.FindBrandBrandCategoriesListItemModel(brandCategory2);
            actual = brandBrandCatList2.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
        }

        private BrandCategoryModel createBrandCategoryWithBrands(CompanyModel testCompany, UserModel testUser, int numBrandsToAdd) {
            // Create a brand category
            BrandCategoryModel brandCategory = createBrandCategory(testCompany);
            ProductService.InsertOrUpdateBrandCategory(brandCategory, null, testUser, "");

            // Now attach random brands
            var brandList = ProductService.FindBrandListModel(0, 1, PageSize, "").Items;
            int actual = brandList.Count();
            Assert.IsTrue(actual > 0, $"Error: {actual} Brands were found when 1 or more were expected");

            for (int i = 0; i < numBrandsToAdd; i++) {
                int rand = RandomInt(0, brandList.Count() - 1);
                ProductService.AddBrandToBrandCategory(testCompany, brandList[rand], brandCategory);
                brandList.RemoveAt(rand);       // So we don't try to add the same item again
            }

            return brandCategory;
        }
    }
}
