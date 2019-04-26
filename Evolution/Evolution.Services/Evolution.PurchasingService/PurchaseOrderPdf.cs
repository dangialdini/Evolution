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

namespace Evolution.PurchasingService {
    public partial class PurchasingService {

        #region Public methods

        #region Sending Purchase Order to Warehouse

        public Error SendPurchaseOrderToWarehouse(int purchaseOrderHeaderTempId) {
            // Create PO CSV, then send to Warehouse
            FilePackagerService.FilePackagerService fpService = new FilePackagerService.FilePackagerService(db);
            return fpService.SendPurchaseOrderToWarehouse(purchaseOrderHeaderTempId);
        }

        #endregion

        #region Sending Purchase Order to supplier

        public Error SendPurchaseOrderToSupplier(PurchaseOrderHeaderTempModel poht, UserModel sender, CompanyModel company) {
            // Create PO PDF and CSV, then send to Warehouse
            string pdfFile = "";
            var error = CreatePurchaseOrderPdf(poht, company.POSupplierTemplateId, null, ref pdfFile);
            if (!error.IsError) {
                FilePackagerService.FilePackagerService fpService = new FilePackagerService.FilePackagerService(db);
                error = fpService.SendPurchaseOrderToSupplier(poht.Id, sender, pdfFile);
            }
            if (error.IsError) MediaService.MediaService.DeleteFile(pdfFile);
            return error;
        }

        #endregion

        #region Sending Purchase Order to Freight Forwarder

        public Error SendPurchaseOrderToFreightForwarder(int purchaseOrderHeaderTempId) {
            // Create PO CSV, then send to Warehouse
            FilePackagerService.FilePackagerService fpService = new FilePackagerService.FilePackagerService(db);
            return fpService.SendPurchaseOrderToFreightForwarder(purchaseOrderHeaderTempId);
        }

        #endregion

        #region Purchse Order PDF creation

        public Error CreatePurchaseOrderPdf(PurchaseOrderHeaderTempModel poht, int? templateId,
                                            string pdfFile, ref string outputFile) {
            return CreatePurchaseOrderPdf(db.FindPurchaseOrderHeader(poht.OriginalRowId), templateId ?? 0, pdfFile, ref outputFile);
        }


        public Error CreatePurchaseOrderPdf(PurchaseOrderHeaderModel poh, int? templateId,
                                            string pdfFile, ref string outputFile) {
            return CreatePurchaseOrderPdf(db.FindPurchaseOrderHeader(poh.Id), templateId ?? 0, pdfFile, ref outputFile);
        }

        public Error CreatePurchaseOrderPdf(PurchaseOrderHeader poh, int? templateId,
                                            string pdfFile, ref string outputFile) {
            var error = new Error();

            var template = LookupService.FindDocumentTemplateModel(templateId ?? 0);
            if (template == null) {
                error.SetRecordError("DocumentTemplate", templateId ?? 0);

            } else {
                string templateFile = template.QualTemplateFile;
                string tempFile = MediaService.MediaService.GetTempFile(".html");

                if (string.IsNullOrEmpty(pdfFile)) {
                    outputFile = MediaService.MediaService.GetTempFile().FolderName() + "\\" + poh.OrderNumber + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                } else {
                    outputFile = pdfFile;
                }

                // Insert the lines
                decimal subTotal = 0,
                        subTotalIncGst = 0;

                SupplierModel supplier = (poh.SupplierId == null ? new SupplierModel() : SupplierService.FindSupplierModel(poh.SupplierId.Value));
                var supplierTerms = (supplier == null ? new SupplierTermModel() : LookupService.FindSupplierTermModel(supplier.SupplierTermId.Value));

                // Freight Terms are "Commercial Terms" and are stored in LOV(12)
                var freightTerms = (poh.LOVItem_CommercialTerms == null ? new LOVItem() : poh.LOVItem_CommercialTerms);

                var port = (poh.Port_Port == null ? new Port() : poh.Port_Port);
                var shipMethod = (poh.LOVItem_ShipMethod == null ? new LOVItem() : poh.LOVItem_ShipMethod);
                var containerType = (poh.ContainerType == null ? new ContainerType() : poh.ContainerType);
                var freightForwarder = (poh.FreightForwarder == null ? new FreightForwarder() : poh.FreightForwarder);

                var taxCode = (poh.Supplier.TaxCode == null ? new TaxCode() : poh.Supplier.TaxCode);
                subTotalIncGst = subTotal + (taxCode.TaxPercentageRate == null ? 0 : (subTotal / 100 * taxCode.TaxPercentageRate.Value));

                var currency = (poh.Currency == null ? new Currency() : poh.Currency);
                var warehouse = (poh.Location == null ? new LocationModel() : LookupService.FindLocationModel(poh.LocationId.Value));
                var arrivalPort = (poh.Port_PortArrival == null ? new Port() : poh.Port_PortArrival);

                Dictionary<string, string> headerProps = new Dictionary<string, string>();
                List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();

                headerProps.AddProperty("LOGOIMAGE", GetConfigurationSetting("SiteFolder", "") + @"\Content\Logos\" + poh.Company.FormLogo);
                headerProps.AddProperty("ABN", poh.Company.ABN);
                headerProps.AddProperty("COMPANYADDRESS", poh.Company.CompanyAddress);
                headerProps.AddProperty("PHONENUMBER", poh.Company.PhoneNumber);
                headerProps.AddProperty("FAXNUMBER", poh.Company.FaxNumber);
                headerProps.AddProperty("WEBSITE", poh.Company.Website);
                headerProps.AddProperty("EMAIL", poh.Company.EmailAddressPurchasing);
                headerProps.AddProperty("ORDERNUMBER", poh.OrderNumber.ToString());
                headerProps.AddProperty("ORDERDATE", formatDate(poh.OrderDate, poh.Company.DateFormat));
                headerProps.AddProperty("PAYMENTTERMS", supplierTerms.SupplierTermName);

                headerProps.AddProperty("SUPPLIERNAME", supplier.Name);
                headerProps.AddProperty("SUPPLIERADDRESS", supplier.Street);
                headerProps.AddProperty("SUPPLIERCITY", supplier.City);
                headerProps.AddProperty("SUPPLIERSTATE", supplier.State);
                headerProps.AddProperty("SUPPLIERPOSTCODE", supplier.Postcode);
                headerProps.AddProperty("SUPPLIERCOUNTRY", supplier.CountryName);
                headerProps.AddProperty("SUPPLIERCONTACTNAME", poh.Supplier.ContactName);
                headerProps.AddProperty("SUPPLIERPHONENUMBER", poh.Supplier.Phone1);
                headerProps.AddProperty("SUPPLIERFAXNUMBER", poh.Supplier.Phone2);

                headerProps.AddProperty("DELIVERYADDRESS", warehouse.FullAddress.Replace("\r\n", "<br/>"));

                headerProps.AddProperty("FREIGHTTERMS", freightTerms.ItemText);
                headerProps.AddProperty("PORTNAME", port.PortName);
                headerProps.AddProperty("SHIPMETHOD", shipMethod.ItemText);
                headerProps.AddProperty("CONTAINERTYPE", containerType.ContainerType1);

                headerProps.AddProperty("REQUIREDSHIPDATE", formatDate(poh.RequiredShipDate, poh.Company.DateFormat));
                headerProps.AddProperty("CANCELDATE", formatDate(poh.CancelDate, poh.Company.DateFormat));
                headerProps.AddProperty("CANCELMESSAGE", poh.CancelMessage);

                headerProps.AddProperty("FREIGHTFORWARDERNAME", freightForwarder.Name);
                headerProps.AddProperty("FFADDRESS", freightForwarder.Address);
                headerProps.AddProperty("FFPHONE", freightForwarder.Phone);
                headerProps.AddProperty("FFEMAIL", freightForwarder.Email);

                headerProps.AddProperty("CONSIGNEECOMPANYNAME", poh.Company.FriendlyName);
                headerProps.AddProperty("CONSIGNEEADDRESS", poh.Company.CompanyAddress);
                headerProps.AddProperty("CONSIGNEEPHONENUMBER", poh.Company.PhoneNumber);
                headerProps.AddProperty("CONSIGNEEFAXNUMBER", poh.Company.FaxNumber);

                headerProps.AddProperty("SALESTAXNAME", taxCode.TaxCode1);
                headerProps.AddProperty("CURRENCYSYMBOL", currency.CurrencySymbol);

                headerProps.AddProperty("WAREHOUSEADDRESS", warehouse.FullAddress.Replace("\r\n", "<br/>"));
                headerProps.AddProperty("WAREHOUSEPHONE", warehouse.ContactPhone);

                headerProps.AddProperty("ARRIVALPORT", arrivalPort.PortName);

                // Add items
                int itemCount = 1;
                foreach (var pod in poh.PurchaseOrderDetails) {
                    decimal unitPriceExTax = (pod.UnitPriceExTax == null ? 0 : pod.UnitPriceExTax.Value);
                    decimal discountPc = (pod.DiscountPercent == null ? 0 : pod.DiscountPercent.Value);
                    decimal linePrice = (pod.OrderQty.Value * unitPriceExTax - ((pod.OrderQty.Value * unitPriceExTax) / 100 * discountPc));

                    Dictionary<string, string> line = new Dictionary<string, string>();
                    line.AddProperty("ORDERQTY", pod.OrderQty);
                    line.AddProperty("SUPPLIERITEMNUMBER", (pod.Product.SupplierItemNumber == "0" ? "" : pod.Product.SupplierItemNumber));
                    line.AddProperty("OURPRODCODE", pod.Product.ItemNumber);
                    line.AddProperty("DESCRIPTION", itemCount.ToString() + " " + pod.ProductDescription);
                    line.AddProperty("UNITPRICEEXTAX", unitPriceExTax.ToString("#,##0.000"));
                    line.AddProperty("DISCOUNTPERCENT", discountPc.ToString("#,##0.00"));
                    line.AddProperty("LINEPRICE", linePrice.ToString("#,##0.000"));

                    subTotal += linePrice;
                    subTotalIncGst += linePrice;
                    line.AddProperty("PURCHASEAMOUNTEX", subTotal.ToString("#,##0.00"));
                    line.AddProperty("PURCHASEAMOUNTINC", subTotalIncGst.ToString("#,##0.00"));

                    line.AddProperty("TAXNAME", taxCode.TaxCode1);
                    line.AddProperty("CURRENCYSYMBOL", currency.CurrencySymbol);

                    records.Add(line);
                    itemCount++;
                }

                error = DocumentService.CreateDocumentPdf(headerProps, records, templateFile, outputFile);
            }
            return error;
        }

        #endregion

        #endregion
    }
}
