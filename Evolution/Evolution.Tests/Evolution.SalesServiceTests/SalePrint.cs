using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.SalesService;
using Evolution.MediaService;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests {
        [TestMethod]
        public void PrintSaleTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompanyAU();

            // Find a random sales order header
            int range = db.FindSalesOrderHeaders(testCompany.Id).Count();
            var sohList = db.FindSalesOrderHeaders(testCompany.Id)
                            .Skip(RandomInt(0, range - 1))
                            .Take(1)
                            .FirstOrDefault();
            // Uncomment the following line to get a document for a random SOH, possibly with no items
            //var soh = SalesService.MapToModel(sohList);
            // Uncomment the following line to get a document known to have a number of items
            var soh = SalesService.FindSalesOrderHeaderModel(58, testCompany);
            var soht = SalesService.CopySaleToTemp(testCompany, soh, testUser, false);
            var customer = CustomerService.FindCustomerModel(soht.CustomerId == null ? 0 : soht.CustomerId.Value, testCompany);

            SalePrintOptionsViewModel model = new SalePrintOptionsViewModel {
                SalesOrderHeaderTempId = soht.Id,
                TemplateId = LookupService.FindDocumentTemplateModel(DocumentTemplateCategory.SalesOrders, DocumentTemplateType.OrderConfirmation).Id,
                ShowCancelledItems = true,
                SaveInSaleNotesAttachments = true,
                ViewCreatedDocument = false,
                SendAsEMail = true,
                Subject = "Test Subject",
                Message = "Test Message",
                SaveAsContact = true
            };
            model.CustomerContact.CustomerId = customer.Id;

            // Get all the recipients
            List<ListItemModel> recipients = CustomerService.FindCustomerRecipients(soht, testCompany, testUser);

            bool bAlt = false;
            string selectedIds = "To:OTH";
            for (int i = 0; i < recipients.Count(); i++) {
                var user = recipients[i];
                selectedIds += "," + (bAlt ? "To:" : "CC:") + user.Id.ToString();
                bAlt = !bAlt;
            }

            // 'Other user'
            model.CustomerContact.ContactFirstname = RandomString();
            model.CustomerContact.ContactSurname = RandomString();
            model.CustomerContact.ContactEmail = RandomEMail();

            // Print the sale
            var error = SalesService.PrintSale(model,
                                               testCompany, testUser, selectedIds);
            Assert.IsTrue(!error.IsError, error.Message);
        }

        [TestMethod]
        public void BuildRecipientListsTest() {
            // Tested in PrintSaleTest()
        }
    }
}
