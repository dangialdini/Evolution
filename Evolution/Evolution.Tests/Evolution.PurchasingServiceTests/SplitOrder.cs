using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.PurchasingService;
using Evolution.TaskProcessor;
using Evolution.Enumerations;

namespace Evolution.PurchasingServiceTests {
    public partial class PurchasingServiceTests {
        [TestMethod]
        public void FindSplitPurchaseDetailsListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            int itemCount = 6;
            var testOrder = GetTestPurchaseOrderHeader(testCompany, testUser, itemCount);
            var testDetails = PurchasingService.FindPurchaseOrderDetailListModel(testOrder)
                                               .Items
                                               .OrderBy(td => td.Id)
                                               .ToList();

            var splitModel = PurchasingService.FindSplitPurchaseDetailsListModel(testCompany, testOrder.Id, 0, 1, 1000)
                                              .Items
                                              .OrderBy(sd => sd.PurchaseOrderDetailId)
                                              .ToList();
            int expected = itemCount,
                actual = splitModel.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Check that all the items match
            for(int i = 0; i < itemCount; i++) {
                expected = testDetails[i].Id;
                actual = splitModel[i].PurchaseOrderDetailId;
                Assert.IsTrue(actual == expected, $"Error: Object Id {actual} was found when Id {expected} was expected");

                expected = testDetails[i].OrderQty.Value;
                actual = splitModel[i].OrigOrderQty;
                Assert.IsTrue(actual == expected, $"Error: Order Qty {actual} was found when {expected} was expected");
            }
        }

        [TestMethod]
        public void SplitOrderTest() {
            // This is a stub for all the tests in this module
        }

        [TestMethod]
        public void SplitToExistingOrderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            var sourceOrder = GetTestPurchaseOrderHeader(testCompany, testUser, 4);
            var sourceOrderItems = PurchasingService.FindPurchaseOrderDetailListModel(sourceOrder);

            // Create a model to split an item off to a new order
            SplitPurchaseModel model = new SplitPurchaseModel {
                PurchaseOrderHeaderId = sourceOrder.Id,
                SupplierName = "",
                OrderNumber = 0,
                AdvertisedETA = null,
                NewOrderAdvertisedETA = DateTimeOffset.Now.AddDays(20),
                LocationId = 0
            };

            
            // Try to split off more items than the original order
            var existingOrder = GetTestPurchaseOrderHeader(testCompany, testUser, 0);
            var splitItem = createSplitItemModel(sourceOrderItems.Items[0].Id, 0, 1500, existingOrder.Id, 1);
            model.SplitItems.Add(splitItem);

            int updatedPOId = 0,
                newPOId = 0;
            var lgs = PurchasingService.LockPurchaseOrderHeader(sourceOrder);
            var error = PurchasingService.SplitOrder(testCompany, model, testUser, lgs, ref updatedPOId, ref newPOId);
            Assert.IsTrue(error.IsError, "$Error: An error was expected but none was returned");


            // Try to split off to an invalid 'existing' order
            splitItem = createSplitItemModel(sourceOrderItems.Items[0].Id, 0, 5, -1, 1);
            model.SplitItems.Clear();
            model.SplitItems.Add(splitItem);

            error = PurchasingService.SplitOrder(testCompany, model, testUser, lgs, ref updatedPOId, ref newPOId);
            Assert.IsTrue(error.IsError, "$Error: An error was expected but none was returned");


            // Now try to split a valid number of items off to an existing order
            int originalQty = sourceOrderItems.Items[0].OrderQty.Value,
                splitQty = 10;

            splitItem = createSplitItemModel(sourceOrderItems.Items[0].Id, 0, splitQty, existingOrder.Id, 1);
            model.SplitItems.Clear();
            model.SplitItems.Add(splitItem);
            lgs = PurchasingService.LockPurchaseOrderHeader(sourceOrder);
            error = PurchasingService.SplitOrder(testCompany, model, testUser, lgs, ref updatedPOId, ref newPOId);
            Assert.IsTrue(!error.IsError, error.Message);


            // Check the newly created copy of the original record we have split
            var updatedOrder = PurchasingService.FindPurchaseOrderHeaderModel(updatedPOId, testCompany, false);
            Assert.IsTrue(updatedOrder != null, "Error: A NULL value was returned when an object was expected");

            var updatedPohd = PurchasingService.FindPurchaseOrderDetailListModel(updatedOrder);
            int expected = originalQty - splitQty,
                actual = updatedPohd.Items[0].OrderQty.Value;
            Assert.IsTrue(actual == expected, $"Error: The order quantity is now {actual} when {expected} was expected");

            // .. and that the updated order has a note with PDF attachment
            var noteList = NoteService.FindNotesListModel(NoteType.Purchase, updatedPOId, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: (1) Order was found to have {actual} notes when {expected} were expected");

            var noteAttachments = NoteService.FindNoteAttachmentsModel(noteList.Items[0], MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteAttachments.Count();
            Assert.IsTrue(actual == expected, $"Error: (1) Order note was found to have {actual} attachment PDFs when {expected} were expected");


            // Look for the new record in the exiting order
            var newPohdList = db.FindPurchaseOrderDetails(testCompany.Id, existingOrder.Id)
                                .OrderByDescending(ni => ni.Id)
                                .ToList();
            Assert.IsTrue(newPohdList.Count() == 1, $"Error: {newPohdList.Count()} items were returned when 1 was expected");
            expected = splitQty;
            actual = newPohdList[0].OrderQty.Value;
            Assert.IsTrue(actual == splitQty, $"Error: {actual} items were returned when {expected} was expected");

            // .. and that the existing order has a note with PDF attachment
            noteList = NoteService.FindNotesListModel(NoteType.Purchase, existingOrder.Id, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: (2) Order was found to have {actual} notes when {expected} were expected");

            noteAttachments = NoteService.FindNoteAttachmentsModel(noteList.Items[0], MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteAttachments.Count();
            Assert.IsTrue(actual == expected, $"Error: (2) Order note was found to have {actual} attachment PDFs when {expected} were expected");

            // TBD: Check that the allocations have been modified
        }

        [TestMethod]
        public void SplitToNewOrderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            var sourceOrder = GetTestPurchaseOrderHeader(testCompany, testUser, 4);
            var sourceOrderItems = PurchasingService.FindPurchaseOrderDetailListModel(sourceOrder);

            // Create a model to split an item off to a new order
            SplitPurchaseModel model = new SplitPurchaseModel {
                PurchaseOrderHeaderId = sourceOrder.Id,
                SupplierName = "",
                OrderNumber = 0,
                AdvertisedETA = null,
                NewOrderAdvertisedETA = DateTimeOffset.Now.AddDays(20),
                LocationId = 0
            };


            // Try to split off more items than the original order to a new order
            var splitItem = createSplitItemModel(sourceOrderItems.Items[0].Id, 1500, 0, 0, 1);
            model.SplitItems.Add(splitItem);

            int updatedPOId = 0,
                newPOId = 0;
            var lgs = PurchasingService.LockPurchaseOrderHeader(sourceOrder);
            var error = PurchasingService.SplitOrder(testCompany, model, testUser, lgs, ref updatedPOId, ref newPOId);
            Assert.IsTrue(error.IsError, "$Error: An error was expected but none was returned");


            // Try to split a valid number of items off to a new order
            int originalQty = sourceOrderItems.Items[0].OrderQty.Value,
                splitQty = 10;

            lgs = PurchasingService.LockPurchaseOrderHeader(sourceOrder);
            splitItem = createSplitItemModel(sourceOrderItems.Items[0].Id, 10, 0, 0, 1);
            model.SplitItems.Clear();
            model.SplitItems.Add(splitItem);

            error = PurchasingService.SplitOrder(testCompany, model, testUser, lgs, ref updatedPOId, ref newPOId);
            Assert.IsTrue(!error.IsError, error.Message);
            Assert.IsTrue(newPOId != 0, "Error: An order id was expected to be returned in Error.Id but 0 was returned");


            // Check the newly created copy of the original record we have split
            var updatedOrder = PurchasingService.FindPurchaseOrderHeaderModel(updatedPOId, testCompany, false);
            Assert.IsTrue(updatedOrder != null, "Error: A NULL value was returned when an object was expected");

            var updatedPohd = PurchasingService.FindPurchaseOrderDetailListModel(updatedOrder);
            int expected = originalQty - splitQty,
                actual = updatedPohd.Items[0].OrderQty.Value;
            Assert.IsTrue(actual == expected, $"Error: The order quantity is now {actual} when {expected} was expected");

            // .. and that the updated order has a note with PDF attachment
            var noteList = NoteService.FindNotesListModel(NoteType.Purchase, updatedPOId, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: (1) Order was found to have {actual} notes when {expected} were expected");

            var noteAttachments = NoteService.FindNoteAttachmentsModel(noteList.Items[0], MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteAttachments.Count();
            Assert.IsTrue(actual == expected, $"Error: (1) Order note was found to have {actual} attachment PDFs when {expected} were expected");


            // Check the records we created
            var checkOrder = PurchasingService.FindPurchaseOrderHeaderModel(newPOId, testCompany, false);
            Assert.IsTrue(checkOrder != null, "Error: A NULL value was returned when an object was expected");

            var checkDetails = PurchasingService.FindPurchaseOrderDetailListModel(checkOrder);
            actual = checkDetails.Items.Count();
            expected = 1;
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} was expected");

            actual = checkDetails.Items[0].ProductId.Value;
            expected = sourceOrderItems.Items[0].ProductId.Value;
            Assert.IsTrue(actual == expected, $"Error: ProductId {actual} was returned when {expected} was expected");

            actual = checkDetails.Items[0].OrderQty.Value;
            expected = splitQty;
            Assert.IsTrue(actual == expected, $"Error: Quantity {actual} was returned when {expected} was expected");

            // .. and that the new order has a note with PDF attachment
            noteList = NoteService.FindNotesListModel(NoteType.Purchase, newPOId, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: (2) Order was found to have {actual} notes when {expected} were expected");

            noteAttachments = NoteService.FindNoteAttachmentsModel(noteList.Items[0], MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteAttachments.Count();
            Assert.IsTrue(actual == expected, $"Error: (2) Order note was found to have {actual} attachment PDFs when {expected} were expected");

            // TBD: Check that the allocations have been modified
        }

        [TestMethod]
        public void SplitToExistingAndNewOrderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser, true);

            var sourceOrder = GetTestPurchaseOrderHeader(testCompany, testUser, 4);
            var sourceOrderItems = PurchasingService.FindPurchaseOrderDetailListModel(sourceOrder);

            // Create a model to split an item off to a new order
            SplitPurchaseModel model = new SplitPurchaseModel {
                PurchaseOrderHeaderId = sourceOrder.Id,
                SupplierName = "",
                OrderNumber = 0,
                AdvertisedETA = null,
                NewOrderAdvertisedETA = DateTimeOffset.Now.AddDays(20),
                LocationId = 0
            };

            // Try to split off more items than the original order to an existing and a new order
            var existingOrder = GetTestPurchaseOrderHeader(testCompany, testUser, 0);
            var splitItem = createSplitItemModel(sourceOrderItems.Items[0].Id, 1500, 1500, existingOrder.Id, 1);
            model.SplitItems.Add(splitItem);

            int updatedPOId = 0,
                newPOId = 0;
            var lgs = PurchasingService.LockPurchaseOrderHeader(sourceOrder);
            var error = PurchasingService.SplitOrder(testCompany, model, testUser, lgs, ref updatedPOId, ref newPOId);
            Assert.IsTrue(error.IsError, "$Error: An error was expected but none was returned");


            // Try to split a valid number of items off to an existing and a new order
            int originalQty = sourceOrderItems.Items[0].OrderQty.Value,
                splitQty1 = 8,
                splitQty2 = 12;

            splitItem = createSplitItemModel(sourceOrderItems.Items[0].Id, splitQty1, splitQty2, existingOrder.Id, 1);
            model.SplitItems.Clear();
            model.SplitItems.Add(splitItem);

            lgs = PurchasingService.LockPurchaseOrderHeader(sourceOrder);
            error = PurchasingService.SplitOrder(testCompany, model, testUser, lgs, ref updatedPOId, ref newPOId);
            Assert.IsTrue(!error.IsError, error.Message);
            Assert.IsTrue(newPOId != 0, "Error: A New order id was expected to be returned but 0 was returned");


            // Check the new created copy of the original record we have split
            var updatedOrder = PurchasingService.FindPurchaseOrderHeaderModel(updatedPOId, testCompany, false);
            Assert.IsTrue(updatedOrder != null, "Error: A NULL value was returned when an object was expected");

            var updatedPohd = PurchasingService.FindPurchaseOrderDetailListModel(updatedOrder);
            int expected = originalQty - (splitQty1 + splitQty2),
                actual = updatedPohd.Items[0].OrderQty.Value;
            Assert.IsTrue(actual == expected, $"Error: The order quantity is now {actual} when {expected} was expected");


            // Check the new order we created
            var checkOrder = PurchasingService.FindPurchaseOrderHeaderModel(newPOId, testCompany, false);
            Assert.IsTrue(checkOrder != null, "Error: A NULL value was returned when an object was expected");

            // ..and the item we added to the new order
            var checkDetails = PurchasingService.FindPurchaseOrderDetailListModel(checkOrder);
            actual = checkDetails.Items.Count();
            expected = 1;
            Assert.IsTrue(actual == 1, $"Error: {actual} items were returned when {expected} was expected");

            actual = checkDetails.Items[0].ProductId.Value;
            expected = sourceOrderItems.Items[0].ProductId.Value;
            Assert.IsTrue(actual == expected, $"Error: ProductId {actual} was returned when {expected} was expected");

            actual = checkDetails.Items[0].OrderQty.Value;
            expected = splitQty1;
            Assert.IsTrue(actual == expected, $"Error: Quantity {actual} was returned when {expected} was expected");

            // .. and that the new order has a note with PDF attachment
            var noteList = NoteService.FindNotesListModel(NoteType.Purchase, newPOId, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: (1) Order was found to have {actual} notes when {expected} were expected");

            var noteAttachments = NoteService.FindNoteAttachmentsModel(noteList.Items[0], MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteAttachments.Count();
            Assert.IsTrue(actual == expected, $"Error: (1) Order note was found to have {actual} attachment PDFs when {expected} were expected");


            // Look for the new details records added to an existing order
            var newPohdList = db.FindPurchaseOrderDetails(testCompany.Id, existingOrder.Id)
                                .OrderByDescending(ni => ni.Id)
                                .ToList();
            Assert.IsTrue(newPohdList.Count() == 1, $"Error: {newPohdList.Count()} items were returned when 1 was expected");
            actual = newPohdList[0].OrderQty.Value;
            expected = splitQty2;
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} was expected");

            // ..and the details of the added item
            actual = newPohdList[0].ProductId.Value;
            expected = sourceOrderItems.Items[0].ProductId.Value;
            Assert.IsTrue(actual == expected, $"Error: ProductId {actual} was returned when {expected} was expected");

            actual = newPohdList[0].OrderQty.Value;
            expected = splitQty2;
            Assert.IsTrue(actual == expected, $"Error: Quantity {actual} was returned when {expected} was expected");

            // .. and that the existing order has a note with PDF attachment
            noteList = NoteService.FindNotesListModel(NoteType.Purchase, existingOrder.Id, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteList.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: (2) Order was found to have {actual} notes when {expected} were expected");

            noteAttachments = NoteService.FindNoteAttachmentsModel(noteList.Items[0], MediaSize.Medium, 0, 0);
            expected = 1;
            actual = noteAttachments.Count();
            Assert.IsTrue(actual == expected, $"Error: (2) Order note was found to have {actual} attachment PDFs when {expected} were expected");

            // TBD: Check that the allocations have been modified
        }

        [TestMethod]
        public void MapToSplitModelTest() {
            // This method is tested by FindSplitPurchaseDetailsListModelTest
        }

        private SplitPurchaseItemModel createSplitItemModel(int pohdId, int newQty, int targetQty, int targetOrderId, int rowNumber) {
            return new SplitPurchaseItemModel {
                PurchaseOrderDetailId = pohdId,
                NewOrderQty = newQty,
                TargetOrderQty = targetQty,
                TargetOrderId = targetOrderId,
                RowNumber = rowNumber
            };
        }
    }
}
