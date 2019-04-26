using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.TemplateService;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.TemplateServiceTests {
    [TestClass]
    public class TemplateServiceTests : BaseTest {
        [TestMethod]
        public void LoadTemplateFileTest() {
            var templateProperties = new TemplateProperties {
                PageHeaderClass = "PageHeader",
                ItemClass = "ItemSection",
                PageFooterClass = "PageFooter",
                DocumentFooterClass = "DocumentFooter"
            };
            TemplateService.TemplateService ts = new TemplateService.TemplateService();

            string templateFile = GetAppSetting("SiteFolder", "") + @"\App_Data\PurchaseOrderTemplates\PurchaseOrder-Default.html";
            var error = ts.LoadTemplateFile(templateFile, templateProperties);


            // Build the document
            int pageNo = 1,
                itemsOnPage = 0;
            Dictionary<string, string> dict = new Dictionary<string, string>();

            ts.AddContent("PageHeader", null);

            // Add some items
            for (int i = 1; i <= 50; i++) {
                dict = new Dictionary<string, string>();
                dict.AddProperty("ORDERQTY", i);
                dict.AddProperty("SUPPLIERITEMNUMBER", i);
                dict.AddProperty("OURPRODCODE", i);
                dict.AddProperty("DESCRIPTION", i);
                dict.AddProperty("UNITPRICEEXTAX", i);
                dict.AddProperty("DISCOUNTPERCENT", i);
                dict.AddProperty("LINEPRICE", i);

                if (itemsOnPage >= 39) {
                    dict.AddProperty("PAGENO", pageNo);
                    ts.AddContent("PageFooter", dict);      // Includes css page-break

                    pageNo++;
                    dict.AddProperty("PAGENO", pageNo);
                    ts.AddContent("PageHeader", dict);
                    itemsOnPage = 0;
                }

                ts.AddContent("ItemSection", dict);
                itemsOnPage++;
            }

            // End of document
            dict = new Dictionary<string, string>();
            dict.AddProperty("PAGENO", pageNo);

            if (itemsOnPage > 20) {
                // Not enough room left on page, so break
                ts.AddContent("PageFooter", dict);      // Includes css page-break

                pageNo++;
                dict.AddProperty("PAGENO", pageNo);
                ts.AddContent("PageHeader", dict);
                itemsOnPage = 0;
            }

            dict = new Dictionary<string, string>();
            dict.AddProperty("PAGENO", pageNo);

            ts.AddContent("DocumentFooter", dict);

            // Get the document
            string docText = ts.Render(dict);

            //using (StreamWriter sw = new StreamWriter(@"c:\temp\Debug.html")) {
            //    sw.Write(ts.Render());
            //}

            string macro = "ORDERQTY";
            Assert.IsTrue(docText.IndexOf(macro) == -1, $"Error: Template macro {macro} was found when it was expected to be substituted");
            macro = "SUPPLIERITEMNUMBER";
            Assert.IsTrue(docText.IndexOf(macro) == -1, $"Error: Template macro {macro} was found when it was expected to be substituted");
            macro = "OURPRODCODE";
            Assert.IsTrue(docText.IndexOf(macro) == -1, $"Error: Template macro {macro} was found when it was expected to be substituted");
            macro = "DESCRIPTION";
            Assert.IsTrue(docText.IndexOf(macro) == -1, $"Error: Template macro {macro} was found when it was expected to be substituted");
            macro = "UNITPRICEEXTAX";
            Assert.IsTrue(docText.IndexOf(macro) == -1, $"Error: Template macro {macro} was found when it was expected to be substituted");
            macro = "DISCOUNTPERCENT";
            Assert.IsTrue(docText.IndexOf(macro) == -1, $"Error: Template macro {macro} was found when it was expected to be substituted");
            macro = "LINEPRICE";
            Assert.IsTrue(docText.IndexOf(macro) == -1, $"Error: Template macro {macro} was found when it was expected to be substituted");
        }

        [TestMethod]
        public void LoadTemplateTest() {
            // Tested in LoadTemplateFileTest above - LoadTemplateFile calls LoadTemplate
        }

        [TestMethod]
        public void AddContentTest() {
            // Tested in LoadTemplateFileTest above
        }

        [TestMethod]
        public void RenderTest() {
            // Tested in LoadTemplateFileTest above
        }

        [TestMethod]
        public void EMailTemplateTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            TemplateService.TemplateService ts = new TemplateService.TemplateService();

            var emailTemplate = db.FindMessageTemplate(testCompany.Id, MessageTemplateType.PurchaseOrderNotificationSupplier);

            var error = ts.LoadTemplate(emailTemplate.Message);

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.AddProperty("PURCHASEORDERNO", RandomInt());
            dict.AddProperty("COMPANYNAME", RandomString());
            dict.AddProperty("USERNAME", RandomString());
            dict.AddProperty("EMAIL", RandomEMail());

            // Get the document
            string docText = ts.Render(dict);

            //using (StreamWriter sw = new StreamWriter(@"c:\temp\Debug.html")) {
            //    sw.Write(docText);
            //}

            string macro = "{PURCHASEORDERNO}";
            Assert.IsTrue(docText.IndexOf(macro) == -1, $"Error: Template macro {macro} was found when it was expected to be substituted");
            macro = "{COMPANYNAME}";
            Assert.IsTrue(docText.IndexOf(macro) == -1, $"Error: Template macro {macro} was found when it was expected to be substituted");
            macro = "{USERNAME}";
            Assert.IsTrue(docText.IndexOf(macro) == -1, $"Error: Template macro {macro} was found when it was expected to be substituted");
            macro = "{EMAIL}";
            Assert.IsTrue(docText.IndexOf(macro) == -1, $"Error: Template macro {macro} was found when it was expected to be substituted");
        }
    }
}
