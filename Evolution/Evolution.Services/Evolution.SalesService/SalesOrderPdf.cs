using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Drawing.Imaging;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.SupplierService;
using Evolution.LookupService;
using Evolution.MediaService;
using Evolution.Resources;
using Evolution.BarCodeService;

namespace Evolution.SalesService {
    public partial class SalesService {

        #region Private methods

        public void AddCompanyInformation(CompanyModel company, Dictionary<string, string> props) {
            props.AddProperty("SITEFOLDER", GetConfigurationSetting("SiteFolder", ""));
            props.AddProperty("LOGOIMAGE", GetConfigurationSetting("SiteFolder", "") + @"\Content\Logos\" + company.FormLogo);
            props.AddProperty("COMPANYNAME", company.CompanyName);
            props.AddProperty("ABN", company.ABN);
            props.AddProperty("COMPANYADDRESS", company.CompanyAddress);
            props.AddProperty("PHONENUMBER", company.PhoneNumber);
            props.AddProperty("FAXNUMBER", company.FaxNumber);
            props.AddProperty("WEBSITE", company.Website);
            props.AddProperty("BANKNAME", company.BankName);
            props.AddProperty("ACCOUNTNAME", company.AccountName);
            props.AddProperty("ACCOUNTNUMBER", company.AccountNumber);
            props.AddProperty("ACCOUNTBSB", company.AccountBSB);
            props.AddProperty("SWIFT", company.Swift);
            props.AddProperty("BRANCH", company.Branch);

            double surcharge = (company.AmexSurcharge == null ? 0 : company.AmexSurcharge.Value);
            props.AddProperty("AMEXSURCHARGE", (surcharge * 100).ToString("N2"));

            surcharge = (company.VisaSurcharge == null ? 0 : company.VisaSurcharge.Value);
            props.AddProperty("VISASURCHARGE", (surcharge * 100).ToString("N2"));

            surcharge = (company.MCSurcharge == null ? 0 : company.MCSurcharge.Value);
            props.AddProperty("MCSURCHARGE", (surcharge * 100).ToString("N2"));

            props.AddProperty("EMAILADDRESSPURCHASING", company.EmailAddressPurchasing);
            props.AddProperty("EMAILADDRESSSALES", company.EmailAddressSales);
            props.AddProperty("EMAILADDRESSACCOUNTS", company.EmailAddressAccounts);
        }

        // This method is called by others in this module to provide a single point
        // fo creating sales-related PDFs
        private Error createSalesOrderPdf(SalesOrderHeaderModel soh, DocumentTemplateModel template,
                                          string pdfFile, bool showCancelledItems,
                                          ref string outputFile, int maxItems = Int32.MaxValue) {
            var error = new Error();

            string tempFile = MediaService.MediaService.GetTempFile(".html");

            if (string.IsNullOrEmpty(pdfFile)) {
                outputFile = MediaService.MediaService.GetTempFile().FolderName() + "\\" + soh.OrderNumber + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
            } else {
                outputFile = pdfFile;
            }

            // Insert the lines
            decimal subTotal = 0,
                    subTotalIncGst = 0,
                    freightTotal = 0;

            CompanyService.CompanyService companyService = new CompanyService.CompanyService(db);
            var company = companyService.FindCompanyModel(soh.CompanyId);

            CustomerService.CustomerService customerService = new CustomerService.CustomerService(db);
            var customer = customerService.FindCustomerModel(soh.CustomerId == null ? 0 : soh.CustomerId.Value,
                                                                company);

            var paymentTerms = LookupService.FindPaymentTermModel(soh.TermsId == null ? 0 : soh.TermsId.Value);

            var taxCode = LookupService.FindTaxCodeModel(customer.TaxCodeId);

            var currency = LookupService.FindCurrencyModel(company.DefaultCurrencyID == null ? 0 : company.DefaultCurrencyID.Value);

            Dictionary<string, string> headerProps = new Dictionary<string, string>();
            List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();

            AddCompanyInformation(company, headerProps);

            headerProps.AddProperty("ORDERNUMBER", soh.OrderNumber.ToString());
            headerProps.AddProperty("CUSTPO", soh.CustPO);
            headerProps.AddProperty("ORDERDATE", formatDate(soh.OrderDate, company.DateFormat));
            headerProps.AddProperty("PAYMENTTERMS", paymentTerms.TermText);

            var salesMgr = customerService.FindBrandCategorySalesPersonsModel(company, customer, soh.BrandCategoryId.Value, SalesPersonType.AccountAdmin).FirstOrDefault();
            if (salesMgr != null) {
                headerProps.AddProperty("ACCOUNTMANAGER", salesMgr.UserName);
            } else {
                headerProps.AddProperty("ACCOUNTMANAGER", "");
            }

            headerProps.AddProperty("CUSTOMERNAME", customer.Name);

            var contact = customerService.FindPrimaryCustomerContactsModel(customer)
                                         .FirstOrDefault();
            headerProps.AddProperty("CUSTOMERCONTACT", contact.ContactFirstname + " " + contact.ContactSurname);

            var addrs = customerService.FindCurrentCustomerAddresses(customer, AddressType.Billing)
                                       .FirstOrDefault();
            if (addrs == null) addrs = new CustomerAddressModel();

            headerProps.AddProperty("STREET", addrs.Street);
            headerProps.AddProperty("CITY", addrs.City);
            headerProps.AddProperty("STATE", addrs.State);
            headerProps.AddProperty("POSTCODE", addrs.Postcode);
            headerProps.AddProperty("COUNTRY", addrs.CountryName);
            headerProps.AddProperty("PHONENO", contact.ContactPhone1);
            headerProps.AddProperty("FAXNUMBER", contact.ContactFax);

            headerProps.AddProperty("DELIVERYADDRESS", soh.FullAddress.Replace("\r\n", "<br/>"));

            headerProps.AddProperty("TAXNAME", taxCode.TaxCode);
            headerProps.AddProperty("CURRENCYSYMBOL", currency.CurrencySymbol);

            var shipMethod = LookupService.FindLOVItemModel((soh.ShippingMethodId == null ? 0 : soh.ShippingMethodId.Value),
                                                            LOVName.ShippingMethod);
            headerProps.AddProperty("DELIVERYVIA", shipMethod.ItemText);

            headerProps.AddProperty("REQUIREDDATE", (soh.RequiredDate == null ? "" : soh.RequiredDate.Value.ToString(company.DateFormat)));

            headerProps.AddProperty("SALESPERSON", soh.SalesPersonName);

            // Add items
            var barCodeService = new BarCodeService.BarCodeService(db);

            int itemCount = 1;
            foreach (var sod in FindSalesOrderDetailListModel(company, soh)) {
                if ((template.TemplateType == DocumentTemplateType.SaleCancellation && sod.LineStatusId == (int)SalesOrderLineStatus.Cancelled) ||
                    (template.TemplateType != DocumentTemplateType.SaleCancellation && (sod.LineStatusId != (int)SalesOrderLineStatus.Cancelled || showCancelledItems))) {

                    decimal unitPriceExTax = (sod.UnitPriceExTax == null ? 0 : sod.UnitPriceExTax.Value);
                    decimal discountPc = (sod.DiscountPercent == null ? 0 : sod.DiscountPercent.Value);
                    decimal orderQty = (sod.OrderQty == null ? 0 : sod.OrderQty.Value);
                    decimal totalExTax = unitPriceExTax * orderQty;
                    decimal linePrice = (orderQty * unitPriceExTax - ((orderQty * unitPriceExTax) / 100 * discountPc));

                    Dictionary<string, string> line = new Dictionary<string, string>();
                    line.AddProperty("ORDERQTY", sod.OrderQty);

                    var product = ProductService.FindProductModel(sod.ProductId.Value, null, company, false);
                    string mediaImage = ProductService.GetProductImage(product, MediaSize.Large, 640, 480, false);
                    line.AddProperty("PRODUCTIMAGE", mediaImage);

                    line.AddProperty("ITEMNUMBER", product.ItemNumber);
                    line.AddProperty("ITEMNAME", product.ItemName);
                    line.AddProperty("DESCRIPTION", itemCount.ToString() + " " + sod.ProductDescription);
                    line.AddProperty("UNITPRICEEXTAX", unitPriceExTax.ToString("#,##0.00"));
                    line.AddProperty("TOTALEXTAX", totalExTax.ToString("#,##0.00"));
                    line.AddProperty("DISCOUNTPERCENT", discountPc.ToString("#,##0.00"));
                    line.AddProperty("LINEPRICE", linePrice.ToString("#,##0.00"));
                    //line.AddProperty("RRP", "");

                    line.AddProperty("BRANDNAME", product.BrandName);
                    line.AddProperty("CATEGORY", product.Category);     // NOT Brand Category

                    string dimensions = "";
                    if (product.Length != null && product.Width != null && product.Height != null) {
                        if (company.UnitOfMeasure == UnitOfMeasure.Imperial) {
                            dimensions = product.Length.CmToInches().ToString();
                            dimensions += " x " + product.Width.CmToInches().ToString();
                            dimensions += " x " + product.Height.CmToInches().ToString();
                            dimensions += " " + company.LengthUnit;
                        } else {
                            dimensions = product.Length.ToString();
                            dimensions += " x " + product.Width.ToString();
                            dimensions += " x " + product.Height.ToString();
                            dimensions += " " + company.LengthUnit;
                        }
                    }
                    line.AddProperty("DIMENSIONS", dimensions);

                    string barCodeFile = "";
                    if (!string.IsNullOrEmpty(product.BarCode)) {
                        barCodeFile = barCodeService.GetBarCode(product.BarCode, true);
                        if(!string.IsNullOrEmpty(barCodeFile)) { 
                            line.AddProperty("BARCODE", $"<img src=\"{barCodeFile}\"/>");
                        } else {
                            line.AddProperty("BARCODE", "");
                        }
                    } else {
                        line.AddProperty("BARCODE", "");
                    }

                    line.AddProperty("MINSALEQTY", product.MinSaleQty.ToString());
                    line.AddProperty("MATERIAL", product.MaterialText);

                    subTotal += linePrice;

                    records.Add(line);
                    itemCount++;
                }
            }

            headerProps.AddProperty("TAXNAME", taxCode.TaxCode);
            headerProps.AddProperty("CURRENCYSYMBOL", currency.CurrencySymbol);
            headerProps.AddProperty("CURRENCYCODE", currency.CurrencyCode);

            headerProps.AddProperty("SALEAMOUNTEX", subTotal.ToString("#,##0.00"));

            if (template.TemplateType == DocumentTemplateType.ProFormaInvoice ||
                template.TemplateType == DocumentTemplateType.OrderConfirmation) {
                freightTotal = CalculateEstimatedFreight(soh, customer);
            }
            headerProps.AddProperty("ESTIMATEDFREIGHT", freightTotal.ToString("#,##0.00"));

            subTotal += freightTotal;
            subTotalIncGst = subTotal + (taxCode.TaxPercentageRate == null ? 0 : (subTotal / 100 * taxCode.TaxPercentageRate.Value));

            headerProps.AddProperty("SALEAMOUNTINC", subTotalIncGst.ToString("#,##0.00"));

            headerProps.AddProperty("GST", (subTotalIncGst - subTotal).ToString("#,##0.00"));

            return DocumentService.CreateDocumentPdf(headerProps, records, template.QualTemplateFile, outputFile, maxItems);
        }

        #endregion
    }
}
