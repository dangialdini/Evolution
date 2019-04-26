using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.SalesService;
using Evolution.Enumerations;

namespace Evolution.SalesServiceTests {
    public partial class SalesServiceTests {
        [TestMethod]
        public void AddCompanyInformationTest() {
            var testCompany = GetTestCompanyAU();

            var dict = new Dictionary<string, string>();
            SalesService.AddCompanyInformation(testCompany, dict);

            // Check that each property has been added to the list
            isEqual(dict, "SITEFOLDER", GetAppSetting("SiteFolder", ""));
            isEqual(dict, "LOGOIMAGE", GetAppSetting("SiteFolder", "") + @"\Content\Logos\" + testCompany.FormLogo);
            isEqual(dict, "COMPANYNAME", testCompany.CompanyName);
            isEqual(dict, "ABN", testCompany.ABN);
            isEqual(dict, "COMPANYADDRESS", testCompany.CompanyAddress);
            isEqual(dict, "PHONENUMBER", testCompany.PhoneNumber);
            isEqual(dict, "FAXNUMBER", testCompany.FaxNumber);
            isEqual(dict, "WEBSITE", testCompany.Website);
            isEqual(dict, "BANKNAME", testCompany.BankName);
            isEqual(dict, "ACCOUNTNAME", testCompany.AccountName);
            isEqual(dict, "ACCOUNTNUMBER", testCompany.AccountNumber);
            isEqual(dict, "ACCOUNTBSB", testCompany.AccountBSB);
            isEqual(dict, "SWIFT", (string.IsNullOrEmpty(testCompany.Swift) ? "" : testCompany.Swift));
            isEqual(dict, "BRANCH", (string.IsNullOrEmpty(testCompany.Branch) ? "" : testCompany.Branch));

            double surcharge = (testCompany.AmexSurcharge == null ? 0 : testCompany.AmexSurcharge.Value);
            isEqual(dict, "AMEXSURCHARGE", (surcharge * 100).ToString("N2"));

            surcharge = (testCompany.VisaSurcharge == null ? 0 : testCompany.VisaSurcharge.Value);
            isEqual(dict, "VISASURCHARGE", (surcharge * 100).ToString("N2"));

            surcharge = (testCompany.MCSurcharge == null ? 0 : testCompany.MCSurcharge.Value);
            isEqual(dict, "MCSURCHARGE", (surcharge * 100).ToString("N2"));

            isEqual(dict, "EMAILADDRESSPURCHASING", testCompany.EmailAddressPurchasing);
            isEqual(dict, "EMAILADDRESSSALES", testCompany.EmailAddressSales);
            isEqual(dict, "EMAILADDRESSACCOUNTS", testCompany.EmailAddressAccounts);
        }

        private void isEqual(Dictionary<string, string> dict, string key, string value) {
            var actual = dict[key];
            Assert.IsTrue(actual == value, $"Error: {key} values holds {actual} when {value} was expected");
        }
    }
}