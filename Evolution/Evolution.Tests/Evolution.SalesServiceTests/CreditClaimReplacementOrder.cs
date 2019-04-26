using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.DAL;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests : BaseTest {
        [TestMethod]
        public void IsCreditClaimReplacementOrderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testSale = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 10);

            // Test there is no credit replacement order
            var result = SalesService.IsCreditClaimReplacementOrder(testSale);
            Assert.IsTrue(result == false, "Error: true was returned when false was expected");

            // Create a Credit Claim Header
            CreditClaimHeaderModel cchm = getCreditClaimHeader();

            // Create CCRO Model & Insert it to db
            CreditClaimReplacementOrderModel model = getCreditClaimReplacementOrder(testCompany, cchm, testSale);
            SalesService.InsertOrUpdateCreditClaimReplacementOrder(model, "");

            // Read db & Compare db record against Model
            var dbData = db.FindCreditClaimReplacementOrder(model.Id);
            var temp = SalesService.MapToModel(dbData);
            AreEqual(temp, model);

            // Test there is a credit replacement order
            result = SalesService.IsCreditClaimReplacementOrder(testSale);
            Assert.IsTrue(result == true, "Error: false was returned when true was expected");

            // Delete it
            db.DeleteCreditClaimReplacementOrder(model.Id);

            // Read db - make sure model has been deleted from db
            dbData = db.FindCreditClaimReplacementOrder(model.Id);
            Assert.IsTrue(dbData == null, "Error: ");

            // Test there is no credit replacement order
            result = SalesService.IsCreditClaimReplacementOrder(testSale);
            Assert.IsTrue(result == false, "Error: true was returned when false was expected");

        }

        [TestMethod]
        public void InsertOrUpdateCreditClaimReplacementOrderTest() {
            // Tested in IsCreditClaimReplacementOrderTest
        }

        private CreditClaimHeaderModel getCreditClaimHeader() {
            var cchm = new CreditClaimHeaderModel();
            cchm.ReplacementRequired = false;
             var error = SalesService.InsertOrUpdateCreditClaimHeader(cchm, "");

            return cchm;
        }

        private CreditClaimReplacementOrderModel getCreditClaimReplacementOrder(CompanyModel company, CreditClaimHeaderModel cch, SalesOrderHeaderModel soh) {
            var model = new CreditClaimReplacementOrderModel();
            model.CompanyId = company.Id;
            model.CreditClaimHeaderId = cch.Id;
            model.SalesOrderHeaderId = soh.Id;
            return model;
        }
    }
}
