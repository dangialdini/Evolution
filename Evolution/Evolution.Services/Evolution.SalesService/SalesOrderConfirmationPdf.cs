using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.SupplierService;
using Evolution.LookupService;
using Evolution.Resources;

namespace Evolution.SalesService {
    public partial class SalesService {

        #region Public methods

        public Error CreateOrderConfirmationPdf(SalesOrderHeaderModel soh, DocumentTemplateModel template,
                                                string pdfFile,
                                                bool showCancelledItems,
                                                ref string outputFile,
                                                int maxItems = Int32.MaxValue) {
            var error = new Error();

            string tempFile = MediaService.MediaService.GetTempFile(".html");

            if (string.IsNullOrEmpty(pdfFile)) {
                outputFile = MediaService.MediaService.GetTempFile().FolderName() + "\\" + soh.OrderNumber + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
            } else {
                outputFile = pdfFile;
            }

            // Insert the lines
            decimal     subTotal = 0,
                        subTotalIncGst = 0,
                        freightTotal = 0;

            CompanyService.CompanyService companyService = new CompanyService.CompanyService(db);
            var company = companyService.FindCompanyModel(soh.CompanyId);

            CustomerService.CustomerService customerService = new CustomerService.CustomerService(db);
            var customer = customerService.FindCustomerModel(soh.CustomerId == null ? 0 : soh.CustomerId.Value,
                                                                company);

            var paymentTerms = LookupService.FindPaymentTermModel(soh.TermsId == null ? 0 : soh.TermsId.Value);

            var taxCode = LookupService.FindTaxCodeModel(customer.TaxCodeId);
            subTotalIncGst = subTotal + (taxCode.TaxPercentageRate == null ? 0 : (subTotal / 100 * taxCode.TaxPercentageRate.Value));

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

            string deliveryWindow = "";
            if (soh.DeliveryWindowOpen != null) deliveryWindow = soh.DeliveryWindowOpen.Value.ToString(company.DateFormat);
            if (soh.DeliveryWindowClose != null) {
                if (!string.IsNullOrEmpty(deliveryWindow)) deliveryWindow += " - ";
                deliveryWindow += soh.DeliveryWindowClose.Value.ToString(company.DateFormat);
            }
            headerProps.AddProperty("DELIVERYWINDOW", deliveryWindow);
            headerProps.AddProperty("SALESPERSON", soh.SalesPersonName);

            // Add items
            int itemCount = 1;
            foreach (var sod in FindSalesOrderDetailListModel(company, soh)) {
                if (sod.LineStatusId != (int)SalesOrderLineStatus.Cancelled || showCancelledItems) {
                    decimal unitPriceExTax = (sod.UnitPriceExTax == null ? 0 : sod.UnitPriceExTax.Value);
                    decimal discountPc = (sod.DiscountPercent == null ? 0 : sod.DiscountPercent.Value);
                    decimal linePrice = (sod.OrderQty.Value * unitPriceExTax - ((sod.OrderQty.Value * unitPriceExTax) / 100 * discountPc));

                    Dictionary<string, string> line = new Dictionary<string, string>();
                    line.AddProperty("ORDERQTY", sod.OrderQty);

                    var product = ProductService.FindProductModel(sod.ProductId == null ? 0 : sod.ProductId.Value,
                                                                null, company);
                    line.AddProperty("ITEMNUMBER", product.ItemNumber);
                    line.AddProperty("DESCRIPTION", itemCount.ToString() + " " + sod.ProductDescription);

                    var ecd = AllocationService.CalculateExpectedCompletionDate(sod);
                    if (ecd != null) {
                        line.AddProperty("INSTOCK", ecd.Value.ToString(company.DateFormat));
                    } else {
                        line.AddProperty("INSTOCK", "");
                    }

                    line.AddProperty("UNITPRICEEXTAX", unitPriceExTax.ToString("#,##0.000"));
                    line.AddProperty("DISCOUNTPERCENT", discountPc.ToString("#,##0.00"));
                    line.AddProperty("LINEPRICE", linePrice.ToString("#,##0.000"));

                    subTotal += linePrice;

                    records.Add(line);
                    itemCount++;
                }
            }

            headerProps.AddProperty("TAXNAME", taxCode.TaxCode);
            headerProps.AddProperty("CURRENCYSYMBOL", currency.CurrencySymbol);

            headerProps.AddProperty("SALEAMOUNTEX", subTotal.ToString("#,##0.00"));

            freightTotal = CalculateEstimatedFreight(soh, customer);
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
