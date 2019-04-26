using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.CustomerService;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.CustomerServiceTests {
    public partial class CustomerServiceTests {
        [TestMethod]
        public void FindBrandCategorySalesPersonsListModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var testCustomer = GetTestCustomer(testCompany, user);

            var model = CustomerService.FindBrandCategorySalesPersonsListModel(testCompany.Id, 0, 1, PageSize, "");
            var dbData = db.FindBrandCategorySalesPersons(testCompany.Id);

            int expected = dbData.Count(),
                actual = model.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model.Items) {
                var dbItem = dbData.Where(m => m.Id == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");
                var temp = CustomerService.MapToModel(dbItem);
                AreEqual(item, temp);
            }

            // Add another item a make sure it is found
            var brandCategory = ProductService.FindBrandCategoriesModel(testCompany).First();

            var newItem = createBrandCategorySalesPerson(testCompany, brandCategory, testCustomer, user, getSalesPersonType());
            var error = CustomerService.InsertOrUpdateBrandCategorySalesPerson(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = CustomerService.FindBrandCategorySalesPersonsListModel(testCustomer.Id, 0, 1, PageSize, "");
            var testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            CustomerService.DeleteBrandCategorySalesPerson(newItem.Id);

            model = CustomerService.FindBrandCategorySalesPersonsListModel(testCompany.Id, 0, 1, PageSize, "");
            testItem = model.Items.Where(i => i.Id == newItem.Id).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindBrandCategorySalesPersonModelTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var testCustomer = GetTestCustomer(testCompany, user);
            var brandCategory = ProductService.FindBrandCategoriesModel(testCompany).First();

            var model = createBrandCategorySalesPerson(testCompany, brandCategory, testCustomer, user, getSalesPersonType());
            var error = CustomerService.InsertOrUpdateBrandCategorySalesPerson(model, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var test = CustomerService.FindBrandCategorySalesPersonModel(model.Id, testCompany, testCustomer, false);
            AreEqual(model, test);
        }

        [TestMethod]
        public void FindBrandCategorySalesPersonsModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Create a brand
            var brand = new BrandModel {
                BrandName = RandomString(),
                Enabled = true
            };
            var error = ProductService.InsertOrUpdateBrand(brand, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            var brandIds = new List<int>();
            brandIds.Add(brand.Id);

            // Add the brand to a brand category
            var brandCategory = new BrandCategoryModel {
                CompanyId = testCompany.Id,
                CategoryName = RandomString(),
                Enabled = true
            };
            error = ProductService.InsertOrUpdateBrandCategory(brandCategory, brandIds, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Add some users as the sales persons
            var salesPerson1 = GetTestUser();
            var model1 = createBrandCategorySalesPerson(testCompany, brandCategory, testCustomer, salesPerson1, getSalesPersonType());
            error = CustomerService.InsertOrUpdateBrandCategorySalesPerson(model1, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var salesPerson2 = GetTestUser();
            var model2 = createBrandCategorySalesPerson(testCompany, brandCategory, testCustomer, salesPerson2, getSalesPersonType());
            error = CustomerService.InsertOrUpdateBrandCategorySalesPerson(model2, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            var salesPerson3 = GetTestUser();
            var model3 = createBrandCategorySalesPerson(testCompany, brandCategory, testCustomer, salesPerson2, getAdminPersonType());
            error = CustomerService.InsertOrUpdateBrandCategorySalesPerson(model3, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Find the Brand category sales persons
            var salesPersons = CustomerService.FindBrandCategorySalesPersonsListModel(testCustomer.Id, 0, 1, Int32.MaxValue, "");
            int expected = 3,
                actual = salesPersons.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
        }

        [TestMethod]
        public void InsertOrUpdateBrandCategorySalesPersonTest() {
            // Tested in DeleteBrandCategorySalesPersonTest
        }

        [TestMethod]
        public void LockBrandCategorySalesPersonTest() {
            // Tested in DeleteBrandCategorySalesPersonTest
        }

        [TestMethod]
        public void DeleteBrandCategorySalesPersonTest() {
            // Get a test user
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var testCustomer = GetTestCustomer(testCompany, user);
            var brandCategory = ProductService.FindBrandCategoriesModel(testCompany).First();

            // Create a price level
            BrandCategorySalesPersonModel model = createBrandCategorySalesPerson(testCompany, brandCategory, testCustomer, user, getSalesPersonType());

            var error = CustomerService.InsertOrUpdateBrandCategorySalesPerson(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that it was written
            var result = db.FindBrandCategorySalesPerson(model.Id);
            AreEqual(model, result);

            // Now delete it
            CustomerService.DeleteBrandCategorySalesPerson(model.Id);

            // And check that is was deleted
            result = db.FindBrandCategorySalesPerson(model.Id);
            Assert.IsTrue(result == null, "Error: A non-NULL value was returned when a NULL value was expected - record delete failed");
        }

        private BrandCategorySalesPersonModel createBrandCategorySalesPerson(CompanyModel testCompany, 
                                                                             BrandCategoryModel brandCategory,
                                                                             CustomerModel customer, UserModel user,
                                                                             LOVItemModel salesPersonType) {
            var tempUser = db.FindUser(user.Id) ?? new User { Id = 0, FirstName = "", LastName = "" };

            BrandCategorySalesPersonModel model = new BrandCategorySalesPersonModel {
                CompanyId = testCompany.Id,
                BrandCategoryId = brandCategory.Id,
                BrandCategoryName = db.FindBrandCategory(brandCategory.Id).CategoryName,
                CustomerId = customer.Id,
                UserId = user.Id,
                UserName = (tempUser.FirstName + " " + tempUser.LastName).Trim().WordCapitalise(),
                SalesPersonTypeId = salesPersonType.Id,
                SalesPersonTypeName = db.FindLOVItem(salesPersonType.Id).ItemText
            };
            return model;
        }

        private LOVItemModel getSalesPersonType() {
            var lov = LookupService.FindLOVsModel(false)
                                   .Where(l => l.LOVName == LOVName.SalesPersonType)
                                   .First();
            return LookupService.FindLOVItemsModel(null, 0, lov.Id, 1, 1000, "")
                                .Items
                                .Where(i => i.ItemValue1.ToString() == ((int)SalesPersonType.AccountManager).ToString())
                                .FirstOrDefault();
        }

        private LOVItemModel getAdminPersonType() {
            var lov = LookupService.FindLOVsModel(false)
                                   .Where(l => l.LOVName == LOVName.SalesPersonType)
                                   .First();
            return LookupService.FindLOVItemsModel(null, 0, lov.Id, 1, 1000, "")
                                .Items
                                .Where(i => i.ItemValue1.ToString() == ((int)SalesPersonType.AccountAdmin).ToString())
                                .FirstOrDefault();
        }
    }
}
