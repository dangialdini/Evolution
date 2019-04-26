using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CustomerService;
using Evolution.MediaService;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.CustomerServiceTests {
    public partial class CustomerServiceTests {
        [TestMethod]
        public void FindCustomerAdditionalInfoModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a customer
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testAdditionalInfo = CustomerService.FindCustomerAdditionalInfoModel(testCustomer.Id, testCompany);

            // Attach data to it and save it
            testAdditionalInfo.DeliveryInstructions = RandomString();
            testAdditionalInfo.PricingInstructions = RandomString();
            testAdditionalInfo.PlacesForwardOrders = true;
            testAdditionalInfo.RegionId = Convert.ToInt32(LookupService.FindRegionsListItemModel(testCompany.Id).First().Id);
            testAdditionalInfo.UnassignedRetailInvoiceNumber = 65535;
            testAdditionalInfo.OurVendorId = RandomString();
            testAdditionalInfo.EDI_VendorNo = RandomString().Left(30);
            testAdditionalInfo.SourceId = Convert.ToInt32(LookupService.FindLOVItemsListItemModel(testCompany, LOVName.Source).First().Id);
            testAdditionalInfo.OrderTypeId = Convert.ToInt32(LookupService.FindLOVItemsListItemModel(testCompany, LOVName.OrderType).First().Id);

            var error = CustomerService.InsertOrUpdateCustomerAdditionalInfo(testAdditionalInfo, testUser, CustomerService.LockCustomerAdditionalInfo(testAdditionalInfo));
            Assert.IsTrue(!error.IsError, error.Message);

            // Retrieve the customer and compare
            var testModel = CustomerService.FindCustomerAdditionalInfoModel(testCustomer.Id, testCompany);

            AreEqual(testAdditionalInfo, testModel);
        }

        [TestMethod]
        public void InsertOrUpdateCustomerAdditionalInfoTest() {
            // Tested in FindCustomerAdditionalInfoModelTest
        }

        [TestMethod]
        public void LockCustomerAdditionalInfoTest() {
            // Tested in customer LockRecordTest
        }

        [TestMethod]
        public void MapToAdditionalInfoModelTest() {
            // Tested by all READ tests in this module
        }
    }
}
