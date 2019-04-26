using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.SalesService;

namespace Evolution.SalesService {
    public partial class SalesService {

        #region Public methods

        public Error CreatePickDocumentPdf(PickHeaderModel pickHeader, DocumentTemplateModel template, 
                                            string pdfFile, bool showCancelledItems, 
                                            ref string outputFile, int maxItems = Int32.MaxValue) {
            var error = new Error();

            string tempFile = MediaService.MediaService.GetTempFile(".html");

            if(string.IsNullOrEmpty(pdfFile)) {
                outputFile = MediaService.MediaService.GetTempFile().FolderName() + "\\" + pickHeader.Id + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
            } else {
                outputFile = pdfFile;
            }

            decimal subTotal = 0;
            var company = CompanyService.FindCompanyModel(pickHeader.CompanyId == null ? 0 : pickHeader.CompanyId.Value);
            var customer = CustomerService.FindCustomerModel(pickHeader.CustomerId == null ? 0 : pickHeader.CustomerId.Value, company);
            var paymentTerms = LookupService.FindPaymentTermModel(pickHeader.TermsID == null ? 0 : pickHeader.TermsID.Value);
            var taxCode = LookupService.FindTaxCodeModel(customer.TaxCodeId);
            var currency = LookupService.FindCurrencyModel(company.DefaultCurrencyID == null ? 0 : company.DefaultCurrencyID.Value);
            
            Dictionary<string, string> headerProps = new Dictionary<string, string>();
            List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();

            AddCompanyInformation(company, headerProps);

            var pickDetailListModel = PickService.FindPickDetailListModel(company, pickHeader).FirstOrDefault();
            var sod = FindSalesOrderDetailModel(pickDetailListModel.SalesOrderDetailId);
            var soh = FindSalesOrderHeaderModel(sod.SalesOrderHeaderId, company);

            // RETAIL INVOICE/PACKING SLIP
            headerProps.AddProperty("ORDERDATE", formatDate(soh.OrderDate, company.DateFormat));
            headerProps.AddProperty("OURREF", soh.OrderNumber);

            // STANDARD PACKING SLIP
            headerProps.AddProperty("REFERENCENUMBER", soh.OrderNumber);
            headerProps.AddProperty("INVOICENUMBER", pickHeader.InvoiceNumber);
            headerProps.AddProperty("SALESDATE", formatDate(soh.OrderDate, company.DateFormat));
            headerProps.AddProperty("TERMS", paymentTerms.TermText); // ??

            headerProps.AddProperty("CUSTOMERCONTACT", pickHeader.CustomerContact);
            var address = pickHeader.ShipAddress1 + "<br/>";
            address += (!string.IsNullOrWhiteSpace(pickHeader.ShipAddress2)) ? pickHeader.ShipAddress2 + "<br/>" : "";
            address += (!string.IsNullOrWhiteSpace(pickHeader.ShipAddress3)) ? pickHeader.ShipAddress3 + "<br/>" : "";
            address += (!string.IsNullOrWhiteSpace(pickHeader.ShipAddress4)) ? pickHeader.ShipAddress4 + "<br/>" : "";
            address += pickHeader.ShipSuburb + " " + pickHeader.ShipState + " " + pickHeader.ShipPostcode + "<br/>";
            address += pickHeader.ShipCountry;
            headerProps.AddProperty("DELIVERYADDRESS", address);

            headerProps.AddProperty("ORDERNUMBER", soh.OrderNumber);
            headerProps.AddProperty("PURCHASEORDERNUMBER", soh.CustPO);

            // Add items
            int itemCount = 1;
            foreach(var pickDetail in PickService.FindPickDetailListModel(company, pickHeader)) {
                var product = ProductService.FindProductModel(pickDetail.ProductId.Value, null, company, false);

                Dictionary<string, string> line = new Dictionary<string, string>();
                line.AddProperty("ORDERQTY", pickDetail.QtyToPick); // Correct?
                line.AddProperty("ITEMNUMBER", product.ItemNumber);
                line.AddProperty("DESCRIPTION", itemCount.ToString() +  " " + sod.ProductDescription);

                // RETAIL INVOICE/PACKING SLIP
                var unitPriceExTax = sod.UnitPriceExTax.Value;
                var discountPc = sod.DiscountPercent.Value;
                var linePrice = (pickDetail.QtyToPick * unitPriceExTax - ((pickDetail.QtyToPick * unitPriceExTax) / 100 * discountPc)).Value;

                sod = FindSalesOrderDetailModel(pickDetail.SalesOrderDetailId);
                string allocated = AllocationService.GetExpectedShipdate(sod);
                if (allocated.Count() > 5) allocated = DateTimeOffset.Parse(allocated).ToString(company.DateFormat);

                line.AddProperty("EXPSHIPDATE", allocated);
                line.AddProperty("LINEPRICE", sod.UnitPriceExTax.Value.ToString("#,##0.00"));
                subTotal += linePrice;

                records.Add(line);
                itemCount++;

                // STILL TO SHIP: TODO
            }

            // RETAIL INVOICE/PACKING SLIP
            headerProps.AddProperty("CURRENCYSYMBOL", currency.CurrencySymbol);
            headerProps.AddProperty("SALEAMOUNTEX", subTotal.ToString("#,##0.00"));
            headerProps.AddProperty("FREIGHT", pickHeader.FreightCost ?? 0);
            subTotal += Convert.ToDecimal(pickHeader.FreightCost);
            var subTotalIncGst = subTotal + (taxCode.TaxPercentageRate == null ? 0 : (subTotal / 100 * taxCode.TaxPercentageRate.Value));
            headerProps.AddProperty("SALEAMOUNTINC", subTotal.ToString("#,##0.00"));
            headerProps.AddProperty("GST", (subTotalIncGst - subTotal).ToString("#,##0.00"));
            headerProps.AddProperty("TAXNAME", taxCode.TaxCode);


            return DocumentService.CreateDocumentPdf(headerProps, records, template.QualTemplateFile, outputFile, maxItems);
        }

        #endregion
    }
}
