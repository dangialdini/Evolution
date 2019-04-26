using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.PurchasingService;
using Evolution.Enumerations;

namespace Evolution.PurchasingServiceTests {
    public partial class PurchasingServiceTests {
        [TestMethod]
        public void CompleteOrderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);
            var poh = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            Assert.IsTrue(poh.POStatusValue != PurchaseOrderStatus.Closed, $"Error: Purchase order status {PurchaseOrderStatus.Closed} was found when any other value was expected");

            var podList = PurchasingService.FindPurchaseOrderDetailListModel(poh);

            var testProd1 = ProductService.FindProductModel(podList.Items[0].ProductId.Value, null, null, false);
            var testProd2 = ProductService.FindProductModel(podList.Items[1].ProductId.Value, null, null, false);

            
            // Clear the records for the first product
            db.DeleteAllocationsForProduct(testCompany.Id, testProd1.Id);

            testProd1.QuantityOnHand = 0;
            string lgs = ProductService.LockProduct(testProd1);
            ProductService.InsertOrUpdateProduct(testProd1, testUser, lgs);

            db.DeleteProductLocationsForProduct(testCompany.Id, testProd1.Id);


            // Copy the order to temp
            var poht = PurchasingService.CopyPurchaseOrderToTemp(testCompany, poh, testUser);

            // Complete the order
            var error = PurchasingService.CompleteOrder(testCompany, testUser, poht.Id);
            Assert.IsTrue(!error.IsError, error.Message);

            poht = PurchasingService.FindPurchaseOrderHeaderTempModel(poht.Id, testCompany, false);
            Assert.IsTrue(poht.POStatusValue == PurchaseOrderStatus.Closed, $"Error: Purchase order status {poht.POStatusValue} was returned when {PurchaseOrderStatus.Closed} was expected");

            // Now try to complete the order again
            error = PurchasingService.CompleteOrder(testCompany, testUser, poht.Id);
            Assert.IsTrue(error.IsError, "Error: No error was returned when one was expected - an order cannot be completed twice");

            poht = PurchasingService.FindPurchaseOrderHeaderTempModel(poht.Id, testCompany, false);
            Assert.IsTrue(poht.POStatusValue == PurchaseOrderStatus.Closed, $"Error: Purchase order status {poht.POStatusValue} was returned when {PurchaseOrderStatus.Closed} was expected");


            // Check the results of the completion
            // Force a reload of all objects
            //ReloadDbContext();

            // Check the Allocations table for the first product
            // The Allocations table records quantity reservations against order lines.
            var allocs1 = AllocationService.FindAllocationListModel(testCompany, testProd1);
            int expected = 0,       // Because it was deleted above
                actual = allocs1.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} records were returned when {expected} were expected");

            // Check the Allocations table for the second product
            var allocs2 = AllocationService.FindAllocationListModel(testCompany, testProd2);
            expected = 1;
            actual = allocs2.Items.Count();
            Assert.IsTrue(actual >= expected, $"Error: {actual} records were returned when >={expected} were expected");

            expected = (int)podList.Items[1].OrderQty;
            actual = (int)allocs2.Items[0].Quantity;
            Assert.IsTrue(actual >= expected, $"Error: Allocation quantity {actual} was returned when {expected} was expected");


            // Check that the product table has been updated
            // Product.QuantityOnHand is for all locations whereas Allocation and ProductLocation
            // are for individual locations so the quantity in Product will always be higher than
            // the latter two.
            testProd1 = ProductService.FindProductModel(podList.Items[0].ProductId.Value, null, null, false);
            expected = (int)podList.Items[0].OrderQty;
            actual = (int)testProd1.QuantityOnHand.Value;
            Assert.IsTrue(actual >= expected, $"Error: Product quantity {actual} was returned when {expected} was expected");

            testProd2 = ProductService.FindProductModel(podList.Items[1].ProductId.Value, null, null, false);
            expected = (int)podList.Items[1].OrderQty;
            actual = (int)testProd2.QuantityOnHand.Value;
            Assert.IsTrue(actual >= expected, $"Error: Product quantity {actual} was returned when {expected} was expected");


            // Check that the ProductLocation table has been updated
            var prodLoc = ProductService.FindProductLocationModel(testCompany, testProd1, poh.LocationId.Value);
            expected = (int)podList.Items[0].OrderQty;
            actual = (int)prodLoc.QuantityOnHand;
            Assert.IsTrue(actual == expected, $"Error: ProductLocation quantity {actual} was returned when {expected} was expected");

            prodLoc = ProductService.FindProductLocationModel(testCompany, testProd2, poh.LocationId.Value);
            expected = (int)podList.Items[1].OrderQty;
            actual = (int)prodLoc.QuantityOnHand;
            Assert.IsTrue(actual == expected, $"Error: ProductLocation quantity {actual} was returned when {expected} was expected");
        }
    }
}
