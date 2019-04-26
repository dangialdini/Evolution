using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Enumerations;
using CommonTest;
using Evolution.CommonService;

namespace Evolution.LookupServiceTests {
    public partial class LookupServiceTests {
        [TestMethod]
        public void GetNextSequenceNumberTest() {
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);

            decimal actual = 0;

            // At this stage, no sequences or orders exist
            for (int expected = 1; expected <= 10; expected++) {
                actual = LookupService.GetNextSequenceNumber(testCompany, SequenceNumberType.PurchaseOrderNumber);

                Assert.IsTrue(actual == expected, $"Error: {actual} was returned when {expected} was expected");
            }

            // Find the "Warehouse" location
            var location = db.FindLocations()
                             .Where(l => l.LocationName == "Warehouse")
                             .FirstOrDefault();

            // Find a supplier with lots of products
            var supplier = db.FindSuppliers(testCompany.Id)
                             .Where(s => s.Products.Count() > 100)
                             .FirstOrDefault();

            // Now create a purchase order with a large purchase order number
            var poh = new PurchaseOrderHeaderModel {
                CompanyId = testCompany.Id,
                LocationId = location.Id,
                POStatus = Convert.ToInt32(LookupService.FindPurchaseOrderHeaderStatusListItemModel().First().Id),
                SupplierId = supplier.Id,
                SupplierInv = "INV" + RandomInt(1000, 9999).ToString(),
                RequiredDate = RandomDateTime(),
                OrderNumber = 1000
            };

            var error = PurchasingService.InsertOrUpdatePurchaseOrderHeader(poh, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Check that the correct number is returned
            // On the first iteration, the next number should be one more than the largest purchase order number ie 1001
            for (int expected = 1001; expected <= 1010; expected++) {
                actual = LookupService.GetNextSequenceNumber(testCompany, SequenceNumberType.PurchaseOrderNumber);
                Assert.IsTrue(actual == expected, $"Error: {actual} was returned when {expected} was expected");
            }
        }

        [TestMethod]
        public void GetNextSequenceNumberTest2() {

            checkNumber(50, false, 50.1m);
            checkNumber(50.1m, false, 50.11m);
            checkNumber(50.11m, false, 50.111m);
            
            checkNumber(50, true, 50.1m);
            checkNumber(50.1m, true, 50.2m);
            checkNumber(50.2m, true, 50.3m);
            checkNumber(50.11m, true, 50.12m);
            checkNumber(50.12m, true, 50.13m);
        }

        private void checkNumber(decimal existingNumber, bool sameSequence, decimal expected) {
            decimal nextNum = LookupService.GetNextSequenceNumber(null, SequenceNumberType.PurchaseOrderNumber,
                                                                  existingNumber, sameSequence);
            Assert.IsTrue(nextNum == expected, $"Error: {nextNum} was returned when {expected} was expected");
        }
    }
}
