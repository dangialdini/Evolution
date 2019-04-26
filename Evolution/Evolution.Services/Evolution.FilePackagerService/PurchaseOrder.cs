using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.CSVFileService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.FileCompressionService;
using Evolution.FileManagerService;
using Evolution.EMailService;
using Evolution.MediaService;
using Evolution.PDFService;
using Evolution.TemplateService;
using Evolution.Resources;

namespace Evolution.FilePackagerService {
    public partial class FilePackagerService : CommonService.CommonService {

        #region Private enums

        private enum FileRecipient {
            Warehouse = 1,
            FreightForwarder = 2
        }

        #endregion

        #region Public methods

        public Error SendPurchaseOrderToSupplier(int purchaseOrderHeaderTempId, UserModel sender,
                                                 string pdfFile) {
            var error = new Error();

            if (string.IsNullOrEmpty(sender.EMail)) {
                error.SetError(EvolutionResources.errInvalidSenderEMailAddress, 
                               "", 
                               (string.IsNullOrEmpty(sender.EMail) ? "" : sender.EMail),
                               sender.FullName);
            } else {
                var poht = db.FindPurchaseOrderHeaderTemp(purchaseOrderHeaderTempId);
                if (poht == null) {
                    error.SetError(EvolutionResources.errRecordError, "", "PurchaseOrderHeaderTemp", purchaseOrderHeaderTempId.ToString());

                } else {
                    var purchaseOrderHeaderId = (poht.OriginalRowId == null ? 0 : poht.OriginalRowId.Value);
                    var poh = db.FindPurchaseOrderHeader(poht.OriginalRowId.Value);
                    if (poh == null) {
                        error.SetError(EvolutionResources.errRecordError, "", "PurchaseOrderHeader", purchaseOrderHeaderId.ToString());

                    } else if (poh.Supplier == null) {
                        error.SetError(EvolutionResources.errCantSendOrderPurchaseOrderHasNoSupplier);

                    } else if (string.IsNullOrEmpty(poh.Supplier.Email)) {
                        error.SetError(EvolutionResources.errCantSendOrderSupplierHasNoEMail, "", poh.Supplier.Name);

                    } else {
                        // Attach the PDF to a note against the Purchase Order
                        NoteService.NoteService noteService = new NoteService.NoteService(db);

                        error = noteService.AttachNoteToPurchaseOrder(poh,
                                                                        sender,
                                                                        "Purchase Order Sent to Supplier", "",
                                                                        pdfFile.ToStringList(),
                                                                        FileCopyType.Copy);
                        if (!error.IsError) {
                            poh.DatePOSentToSupplier = DateTimeOffset.Now;
                            db.InsertOrUpdatePurchaseOrderHeader(poh);

                            poht.DatePOSentToSupplier = poh.DatePOSentToSupplier;
                            db.InsertOrUpdatePurchaseOrderHeaderTemp(poht);

                            // EMail the purchase order to the supplier
                            var message = new EMailMessage(sender, MessageTemplateType.PurchaseOrderNotificationSupplier);

                            var company = CompanyService.FindCompanyModel(poh.CompanyId);

                            message.AddProperty("PURCHASEORDERNO", poh.OrderNumber.Value);
                            message.AddProperty("COMPANYNAME", company.FriendlyName);
                            message.AddProperty("USERNAME", sender.FullName);
                            message.AddProperty("EMAIL", sender.EMail);

                            message.AddRecipient(poh.Supplier.Email);
                            message.AddAttachment(pdfFile, FileCopyType.Move);

                            EMailService.EMailService emailService = new Evolution.EMailService.EMailService(db, company);
                            error = emailService.SendEMail(message);
                        }
                    }
                }
            }

            return error;
        }

        public Error SendPurchaseOrderToWarehouse(int purchaseOrderHeaderTempId) {
            var error = new Error();

            var poht = db.FindPurchaseOrderHeaderTemp(purchaseOrderHeaderTempId);
            if (poht == null) {
                error.SetError(EvolutionResources.errRecordError, "", "PurchaseOrderHeaderTemp", purchaseOrderHeaderTempId.ToString());

            } else {
                var purchaseOrderHeaderId = (poht.OriginalRowId == null ? 0 : poht.OriginalRowId.Value);
                var poh = db.FindPurchaseOrderHeader(purchaseOrderHeaderId);
                if (poh == null) {
                    error.SetError(EvolutionResources.errRecordError, "", "PurchaseOrderHeader", purchaseOrderHeaderId.ToString());

                } else {
                    if (poh.Location == null) {
                        error.SetError(EvolutionResources.errOrderHasNoLocationSet);

                    } else {
                        var fileRecipient = FileRecipient.Warehouse;
                        FileTransferConfigurationModel template = getFileTransferConfigurationModel(poh, fileRecipient);
                        if (template == null) {
                            error.SetError(EvolutionResources.errFailedToFindFileTransferConfigForDeliveryLocation,
                                           "", poh.Location.LocationName);
                        } else {
                            error = processPurchaseOrder(poh, template, fileRecipient);

                            if (!error.IsError) {
                                poh.DateSentToWhs = DateTimeOffset.Now;
                                db.InsertOrUpdatePurchaseOrderHeader(poh);

                                poht.DateSentToWhs = poh.DateSentToWhs;
                                db.InsertOrUpdatePurchaseOrderHeaderTemp(poht);
                            }
                        }
                    }
                }
            }

            return error;
        }

        public Error SendPurchaseOrderToFreightForwarder(int purchaseOrderHeaderTempId) {
            var error = new Error();

            var poht = db.FindPurchaseOrderHeaderTemp(purchaseOrderHeaderTempId);
            if (poht == null) {
                error.SetError(EvolutionResources.errRecordError, "", "PurchaseOrderHeaderTemp", purchaseOrderHeaderTempId.ToString());

            } else {
                var purchaseOrderHeaderId = (poht.OriginalRowId == null ? 0 : poht.OriginalRowId.Value);
                    var poh = db.FindPurchaseOrderHeader(purchaseOrderHeaderId);
                if (poh == null) {
                    error.SetError(EvolutionResources.errRecordError, "", "PurchaseOrderHeader", purchaseOrderHeaderId.ToString());

                } else {
                    if (poh.FreightForwarder == null) {
                        error.SetError(EvolutionResources.errOrderHasNoFreightForwarderSet);

                    } else {
                        var fileRecipient = FileRecipient.FreightForwarder;
                        FileTransferConfigurationModel template = getFileTransferConfigurationModel(poh, fileRecipient);
                        if (template == null) {
                            error.SetError(EvolutionResources.errFailedToFindFileTransferConfigForFreightForwarder,
                                           "", poh.FreightForwarder.Name);
                        } else {
                            error = processPurchaseOrder(poh, template, fileRecipient);

                            if (!error.IsError) {
                                poh.DateSentToFF = DateTimeOffset.Now;
                                db.InsertOrUpdatePurchaseOrderHeader(poh);

                                poht.DateSentToFF = poh.DateSentToFF;
                                db.InsertOrUpdatePurchaseOrderHeaderTemp(poht);
                            }
                        }
                    }
                }
            }

            return error;
        }

        #endregion

        #region Private members

        private FileTransferConfigurationModel getFileTransferConfigurationModel(PurchaseOrderHeader poh,
                                                                            FileRecipient fileRecipient) {
            FileTransferConfigurationModel template = null;

            switch (fileRecipient) {
            case FileRecipient.Warehouse:
                LocationModel warehouse = null;
                if (poh.LocationId != null) warehouse = LookupService.FindLocationModel(poh.LocationId.Value, false);
                if (warehouse != null) {
                    template = DataTransferService.FindFileTransferConfigurationForWarehouseModel(warehouse, FileTransferDataType.WarehousePurchase);
                }
                break;

            case FileRecipient.FreightForwarder:
                FreightForwarderModel freightForwarder = null;
                if (poh.FreightForwarderId != null) freightForwarder = LookupService.FindFreightForwarderModel(poh.FreightForwarderId.Value, false);
                if (freightForwarder != null) {
                    template = DataTransferService.FindFileTransferConfigurationForFreightForwarder(freightForwarder, FileTransferDataType.FreightForwarderPurchase);
                }
                break;
            }

            return template;
        }

        private Error processPurchaseOrder(PurchaseOrderHeader poh, 
                                           FileTransferConfigurationModel template,
                                           FileRecipient fileRecipient) {
            var error = new Error();

            // Load the template configuration file
            string configFile = getTemplateFileName(template),
                   tempFile = "",
                   zipFile = "";

            XElement doc = XElement.Load(configFile);

            var file = doc.Element("File");

            error = createPurchaseOrderDataFile(poh, file, ref tempFile);
            if (!error.IsError) {
                // Check if the file is compressed individually and sent
                bool bCompress = file.Attribute("CompressFile").Value.ParseBool();
                if(bCompress) { 
                    // File is to be compressed
                    zipFile = tempFile.ChangeExtension(".zip");

                    error = Zip.ZipFile(tempFile, zipFile);

                    if (error.IsError) {
                        FileManagerService.FileManagerService.DeleteFile(zipFile);
                    } else {
                        tempFile = zipFile;
                    }
                }

                bool bDelTempFile = true;
                if(!error.IsError) {
                    if (file.Attribute("FTPFile").Value.ParseBool()) {
                        // Copy the file to the FTP pickup folder
                        error = moveFileToFTPFolder(tempFile, template.SourceFolder, poh.OrderNumber + (bCompress ? ".zip" : ".txt"));
                    }
                    if (!error.IsError && file.Attribute("EMailFile").Value.ParseBool()) {
                        // Queue the file to be sent as an email
                        var company = CompanyService.FindCompanyModel(poh.CompanyId);

                        var purchaser = MembershipManagementService.FindUserModel(poh.SalespersonId.Value);

                        string email = "";
                        if (fileRecipient == FileRecipient.FreightForwarder) {
                            var fforwarder = LookupService.FindFreightForwarderModel(poh.FreightForwarderId.Value);
                            if (string.IsNullOrEmpty(fforwarder.Email)) {
                                error.SetError(EvolutionResources.errCantSendEMailToBlankFreightForwarderAddress);
                            } else {
                                email = fforwarder.Email;
                            }
                        } else {
                            error.SetError(EvolutionResources.errEMailingWarehouseNotSupported);
                        }

                        if (!error.IsError) { 
                            var recipient = new UserModel { EMail = email };
                            var message = new EMailMessage(purchaser, recipient, MessageTemplateType.PurchaseOrderNotificationFF);

                            EMailService(company).AddOrganisationDetails(message.Dict);
                            EMailService().AddUserDetails(purchaser, message.Dict);
                            message.AddProperty("PURCHASEORDERNO", poh.OrderNumber.Value);

                            message.AddAttachment(tempFile);

                            error = EMailService().SendEMail(message);
                            if(!error.IsError) bDelTempFile = false;
                        }
                    }
                }
                if(bDelTempFile) FileManagerService.FileManagerService.DeleteFile(tempFile);
            }

            return error;
        }

        #endregion

        #region Private members

        private Error createPurchaseOrderDataFile(PurchaseOrderHeader poh, XElement file, ref string tempFile) {
            var error = new Error();
            Dictionary<string, string> data = new Dictionary<string, string>();

            // Create the CSV writer
            List<CSVFormat> formats = new List<CSVFormat>();

            // Read the formatting configuration from the XML file
            foreach (var formatElement in file.Elements("Formats").Elements()) {
                // Read the formatting options
                CSVFormat fileFormat = new CSVFormat {
                    DataFieldDelimiter = doReplacements(formatElement.Attribute("DataFieldDelimiter").Value),
                    DataFieldDelimiterUsage = (CSVDelimiterUsage)Enum.Parse(typeof(CSVDelimiterUsage), 
                                                                            formatElement.Attribute("DataFieldDelimiterUsage").Value),
                    DataFieldSeparator = doReplacements(formatElement.Attribute("DataFieldSeparator").Value),
                    HeaderFieldDelimiter = doReplacements(formatElement.Attribute("HeaderFieldDelimiter").Value),
                    HeaderFieldSeparator = doReplacements(formatElement.Attribute("HeaderFieldSeparator").Value)
                };

                // Now read the fields
                foreach(var fieldElement in formatElement.Elements("Fields").Elements()) {
                    CSVField fieldFormat = new CSVField {
                        FieldName = fieldElement.Attribute("Name").Value,
                        FieldType = (CSVFieldType)Enum.Parse(typeof(CSVFieldType),
                                                             fieldElement.Attribute("Type").Value)
                    };
                    fileFormat.Fields.Add(fieldFormat);
                }

                formats.Add(fileFormat);
            }

            // Create a temp file
            tempFile = Path.GetTempFileName();

            // Create the CSV writer
            CSVWriter sw = new CSVWriter();
            sw.CreateFile(tempFile, formats);

            // Write the header lines
            for (int i = 0; i < sw.Formats.Count(); i++) {
                sw.CurrentFormat = i;
                sw.WriteHeaderLine();
            }

            int idx = 0;
            if (sw.Formats.Count > 1) {
                // Now write the first data line
                sw.CurrentFormat = idx++;

                data.AddProperty("PurchaseID", poh.Id.ToString());
                data.AddProperty("HeaderLabel", "H");
                data.AddProperty("SupplierName", poh.Supplier.Name);
                data.AddProperty("SupplierInvoice", poh.SupplierInv);
                data.AddProperty("SupplierID", poh.SupplierId);

                string expDate = (poh.RequiredDate == null ? "" : poh.RequiredDate.Value.ToString("dd/MM/yyyy"));
                data.AddProperty("ExpectedDate", expDate);

                double totalCbms = 0;
                foreach (var pod in poh.PurchaseOrderDetails) {
                    if (pod.OrderQty != null && pod.Product.UnitCBM != null) totalCbms += pod.OrderQty.Value * pod.Product.UnitCBM.Value;
                }

                data.AddProperty("CBMs", totalCbms);
                data.AddProperty("SKUs", poh.PurchaseOrderDetails.Count);
                sw.WriteLine(data);
            }

            // Now write the second and subsequent data lines
            sw.CurrentFormat = idx;

            foreach (var pohd in poh.PurchaseOrderDetails) {
                var prodPrice = pohd.Product
                                    .ProductPrices
                                    .Where(pp => pp.PriceLevel == "A")
                                    .FirstOrDefault();

                data = new Dictionary<string, string>();

                // Properties for Warehouse and common
                data.AddProperty("PurchaseID", poh.OrderNumber.ToString());
                data.AddProperty("DetailLabel", "I");
                data.AddProperty("PurchaseQty", pohd.OrderQty);
                data.AddProperty("ItemNumber", pohd.Product.ItemNumber);
                data.AddProperty("ItemDescription", pohd.Product.ItemName);
                data.AddProperty("PurchaseLineID", pohd.Id);
                data.AddProperty("Barcode", pohd.Product.BarCode);
                data.AddProperty("PackSize", prodPrice.QuantityBreakAmount);
                data.AddProperty("ProductDimensions", "0");  // In ACCESS this comes from tblItemAdditional, but the table is empty
                data.AddProperty("ProductWeight", "0");      // In ACCESS this comes from tblItemAdditional, but the table is empty

                // Properties for freight forwarder
                data.AddProperty("OrderNumber", poh.OrderNumber.ToString());
                data.AddProperty("OrderQty", pohd.OrderQty);
                data.AddProperty("SupplierID", poh.SupplierId.ToString());
                data.AddProperty("SupplierName", poh.Supplier.Name);
                data.AddProperty("ItemUnitPrice", (pohd.UnitPriceExTax == null ? "" : pohd.UnitPriceExTax.Value.ToString()));
                data.AddProperty("CurrencyCode", (poh.Currency == null ? "" : poh.Currency.CurrencyCode));
                data.AddProperty("OrderDate", (poh.OrderDate == null ? "" : poh.OrderDate.Value.ToString("dd/MM/yyyy")));
                data.AddProperty("RequiredShipDate", (poh.RequiredShipDate == null ? "" : poh.RequiredShipDate.Value.ToString("dd/MM/yyyy")));
                data.AddProperty("ShipMethod", (poh.ShipMethodId == null ? "" : db.FindLOVItem(poh.ShipMethodId.Value).ItemText));
                data.AddProperty("PortName", (poh.PortId == null ? "" : db.FindPort(poh.PortId.Value).PortName));
                data.AddProperty("Destimation", (poh.PortArrivalId == null ? "" : db.FindPort(poh.PortArrivalId.Value).PortName));
                data.AddProperty("ExchangeRate", poh.ExchangeRate);
                sw.WriteLine(data);
            }

            sw.Close();

            return error;
        }

        #endregion

        #region Private methods

        private string doReplacements(string str) {
            string rc = str.Replace("\\t", "\t");
            rc = rc.Replace("&quot;", "\"");
            return rc;
        }

        #endregion
    }
}
