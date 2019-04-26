using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.Models.Models;

namespace Evolution.CustomerServiceTests {
    public partial class CustomerServiceTests {
        [TestMethod]
        public void FindCustomerFreightModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            // Create a customer
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testFreight = CustomerService.FindCustomerFreightModel(testCustomer, testCompany);

            // Add freight to it and save it
            testFreight.MinFreightPerOrder = 10;
            testFreight.FreightCarrierId = LookupService.FindFreightCarriersListModel(testCompany.Id)
                                                        .Items
                                                        .First()
                                                        .Id;
            testFreight.IsManualFreight = true;
            testFreight.FreightRate = 20;
            testFreight.MinFreightPerOrder = 30;
            testFreight.MinFreightThreshold = 40;
            testFreight.FreightWhenBelowThreshold = 50;
            testFreight.DeliveryInstructions = "Delivery instructions";
            testFreight.DeliveryContact = "Delivery contact";
            testFreight.FreightTermId = null;
            testFreight.ShipMethodAccount = "Ship Method AC";
            testFreight.WarehouseInstructions = "Warehouse instructions";

            var error = CustomerService.InsertOrUpdateCustomerFreight(testFreight, testUser, CustomerService.LockCustomerFreight(testFreight));
            Assert.IsTrue(!error.IsError, error.Message);

            // Retrieve the customer and compare
            var testModel = CustomerService.FindCustomerFreightModel(testCustomer, testCompany);

            AreEqual(testFreight, testModel);
        }

        [TestMethod]
        public void InsertOrUpdateCustomerFreightTest() {
            // Tested in FindCustomerFreightModelTest
        }

        [TestMethod]
        public void LockCustomerFreightTest() {
            // Tested in customer LockRecordTest
        }

        [TestMethod]
        public void MapToCustomerFreightModelTest() {
            // Tested by all READ tests in this module
        }
    }
}
