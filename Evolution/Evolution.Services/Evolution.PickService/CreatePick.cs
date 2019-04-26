using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.CSVFileService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;
using System.Xml.Linq;

namespace Evolution.PickService {
    public partial class PickService : CommonService.CommonService {

        #region Public methods

        public Error CreatePicks(CompanyModel company, List<SalesOrderHeaderModel> sohList, bool bCombine,
                                 List<PickHeaderModel> pickHeaders) {
            // Given a list of SalesOrderHeaders and a flag indicating whether to create
            // individual picks or a single combined pick, creates the required pick(s)
            // in the PickHeader and PickDetail database tables.
            // If successful, it then creates the CSV pick data files.
            var error = new Error();
            if(sohList != null) {
                bool bFirst = true;
                PickHeaderModel pickHeader = null;

                foreach (var soh in sohList) {
                    if (bFirst || !bCombine) {
                        pickHeader = new PickHeaderModel();
                        error = createPick(company, soh, ref pickHeader);
                    }
                    if (!error.IsError) {
                        error = addPickDetails(pickHeader, soh.SalesOrderDetails);

                        if (bCombine) {
                            // If combined, we only save the first pick header because there will only be one
                            if (bFirst) pickHeaders.Add(pickHeader);
                            bFirst = false;
                        } else {
                            // If not combined, save every pick header
                            pickHeaders.Add(pickHeader);

                            // Create a pick CSV file for this sale
                            error = createPickDataFile(pickHeader);
                        }
                    }
                    if (error.IsError) break;
                }

                // If no errors and combined, create the CSV for the combined sales
                if (!error.IsError && bCombine) error = createPickDataFile(pickHeader);
            }
            return error;
        }

        private Error createPick(CompanyModel company, SalesOrderHeaderModel soh, ref PickHeaderModel pickHeader) {
            // Creates a pick in the pick database tables.
            // This method does not create any files.
            var error = new Error();

            if (soh.SalesOrderDetails == null || soh.SalesOrderDetails.Count == 0) {
                error.SetError(EvolutionResources.errCannotCreatePickWithNoItems, "", soh.OrderNumber.ToString());

            } else {
                LookupService.LookupService lookupService = new LookupService.LookupService(db);
                var pickH = new PickHeader {
                    CompanyId = soh.CompanyId,
                    CustomerId = soh.CustomerId,
                    //public DateTimeOffset? PickDate { set; get; }
                    //public int? PickStatusId { set; get; }
                    LocationId = soh.LocationId,
                    //public DateTimeOffset? STWDate { set; get; }
                    //public DateTimeOffset? PickComplete { set; get; }
                    //public DateTimeOffset? PackComplete { set; get; }
                    ShipAddress1 = soh.ShipAddress1,
                    ShipAddress2 = soh.ShipAddress2,
                    ShipAddress3 = soh.ShipAddress3,
                    ShipAddress4 = soh.ShipAddress4,
                    ShipSuburb = soh.ShipSuburb,
                    ShipState = soh.ShipState,
                    ShipPostcode = soh.ShipPostcode,
                    //public DateTimeOffset? InvoiceDate { set; get; }
                    InvoiceNumber = Convert.ToInt32(lookupService.GetNextSequenceNumber(company, SequenceNumberType.InvoiceNumber)),
                    //public bool InvoiceFinalised { set; get; }
                    ShipMethodId = soh.ShippingMethodId,
                    SalesPersonId = soh.SalespersonId,
                    //public DateTimeOffset? ShipDate { set; get; }
                    //public string TrackingNumber { set; get; }
                    //public int? BoxCount { set; get; }
                    //public int? PickPriority { set; get; }
                    //public int? PickedById { set; get; }
                    //public int? PackedById { set; get; }
                    //public DateTimeOffset? ReadyForShippingDate { set; get; }
                    //public int? ShippingDocumentId { set; get; }
                    //public DateTimeOffset? AddedToShipManifestDate { set; get; }
                    //public bool DocumentPrinted { set; get; }
                    CustPO = soh.CustPO,
                    //public string SecretComment { set; get; }
                    //public string PickComment { set; get; }
                    //public string ShipMethodAccount { set; get; }
                    //public double? FreightCost { set; get; }
                    DeliveryInstructions = soh.DeliveryInstructions,
                    //public string CustomerContact { set; get; }
                    IsManualFreight = soh.IsManualFreight,
                    ShipCountryId = soh.ShipCountryId,
                    //public decimal? OurFreightCost { set; get; }
                    //public string EnteredBy { set; get; }
                    WarehouseInstructions = soh.WarehouseInstructions,
                    EndUserName = soh.EndUserName,
                    CreditCardId = soh.CreditCardId,
                    //public bool IsRetailPick { set; get; }
                    //public bool IsUploadedToWarehouse { set; get; }
                    TermsID = soh.TermsId,
                    //public string UnregisteredFreightCarrier { set; get; }
                    //public DateTimeOffset? DateCreditCardCharged { set; get; }
                    OrderTypeId = soh.OrderTypeId
                };
                db.InsertOrUpdatePickHeader(pickH);
                mapToModel(pickH, pickHeader);
            }
            return error;
        }

        private Error addPickDetails(PickHeaderModel pickHeader, List<SalesOrderDetailModel> sodList) {
            // Adds detail lines to a pick.
            var error = new Error();

            foreach (var item in sodList) {
                var pickD = new PickDetail {
                    CompanyId = pickHeader.CompanyId.Value,
                    PickHeaderId = pickHeader.Id,
                    ProductId = item.ProductId,
                    QtyToPick = item.PickQty,
                    QtyPicked = 0,
                    PickDetailStatusId = (int)PickDetailStatus.ToBePicked,
                    SalesOrderDetailId = item.Id,
                    PickLocationId = pickHeader.LocationId,
                    IsReportedToWebsite = false
                };
                db.InsertOrUpdatePickDetail(pickD);

                pickHeader.PickDetails.Add(mapToModel(pickD));
            }
            return error;
        }

        private Error createPickDataFile(PickHeaderModel pickH) {
            // Create a pick data file
            var error = new Error();

            // Get the file transfer for the pick
            var location = db.FindLocation(pickH.LocationId ?? 0);
            if (location == null) location = new Location();

            var transferConfig = db.FindFileTransferConfiguration(location.Id,
                                                                  FileTransferType.Send,
                                                                  FileTransferDataType.WarehousePick);
            if (transferConfig == null) {
                error.SetError(EvolutionResources.errCannotDropOrderNoDataTransfer, "", pickH.InvoiceNumber.ToString(), location.LocationName);

            } else {
                // Load the template configuration file
                string configFile = getTemplateFileName(transferConfig),
                       tempFile = "";

                XElement doc = XElement.Load(configFile);
                var file = doc.Element("File");

                error = createPickDataFile(pickH, file, ref tempFile);
                if (!error.IsError) {
                    pickH.PickFiles.Add(tempFile);
                    pickH.PickDropFolder = transferConfig.SourceFolder;
                }
            }
            return error;
        }

        #endregion

        #region Private methods

        private Error createPickDataFile(PickHeaderModel pickH, XElement file, ref string tempFile) {
            var error = new Error();

            try {
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
                    foreach (var fieldElement in formatElement.Elements("Fields").Elements()) {
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
                var extn = file.Attribute("DataFileExtension").Value;
                tempFile = Path.GetTempPath() + pickH.Id.ToString() + ".CSV";   // extn;

                // Create the CSV writer
                CSVWriter sw = new CSVWriter();
                sw.CreateFile(tempFile, formats);

                // Write the header lines
                for (int i = 0; i < sw.Formats.Count(); i++) {
                    sw.CurrentFormat = i;
                    sw.WriteHeaderLine();
                }

                var customer = db.FindCustomer(pickH.CustomerId.Value);
                var country = db.FindCountry(pickH.ShipCountryId.Value);

                int idx = 0;
                if (sw.Formats.Count > 1) {
                    // Now write the first data line
                    sw.CurrentFormat = idx++;

                    data.AddProperty("PickID", pickH.Id.ToString());
                    data.AddProperty("HeaderLabel", "H");
                    data.AddProperty("CustomerName", customer.Name);
                    data.AddProperty("CustomerOrderNumber", pickH.CustPO);
                    data.AddProperty("ShipAddress1", pickH.ShipAddress1);
                    data.AddProperty("ShipAddress2", pickH.ShipAddress2);
                    data.AddProperty("ShipAddress3", pickH.ShipAddress3);
                    data.AddProperty("ShipSuburb", pickH.ShipSuburb);
                    data.AddProperty("ShipState", pickH.ShipState);
                    data.AddProperty("ShipPostcode", pickH.ShipPostcode);
                    data.AddProperty("ShipCountry", country.CountryName);
                    data.AddProperty("OnforwarderName", "");
                    data.AddProperty("OnforwarderAddress", "");
                    data.AddProperty("OnForwarderSuburb", "");
                    data.AddProperty("OnForwarderPostcode", "");
                    data.AddProperty("OnForwarderState", "");
                    data.AddProperty("OnForwarderCode", "");
                    data.AddProperty("CarrierCode", "");
                    data.AddProperty("CustomerID", pickH.CustomerId);
                    data.AddProperty("DeliveryInstructions", pickH.DeliveryInstructions);
                    data.AddProperty("CustomerContact", "");
                    data.AddProperty("PartnerEDI", "");
                    sw.WriteLine(data);
                }

                // Now write the second and subsequent data lines
                sw.CurrentFormat = idx;

                foreach (var pickD in pickH.PickDetails) {
                    data = new Dictionary<string, string>();

                    // Properties for Warehouse and common
                    var product = db.FindProduct(pickD.ProductId.Value);

                    data.AddProperty("PickID", pickH.Id);
                    data.AddProperty("DetailLabel", "I");
                    data.AddProperty("PickQty", pickD.QtyToPick);
                    data.AddProperty("ItemNumber", product.ItemNumber);
                    data.AddProperty("PickLineID", pickD.Id);
                    sw.WriteLine(data);
                }

                sw.Close();

            } catch(Exception ex) {
                error.SetError(ex);
            }

            return error;
        }

        private string getTemplateFileName(FileTransferConfiguration template) {
            string fileName = GetConfigurationSetting("SiteFolder", "");
            fileName += "\\App_Data\\DataTransferTemplates\\" + template.ConfigurationTemplate;

            return fileName;
        }

        private void mapToModel(PickHeader pickH, PickHeaderModel pickModel) {
            Mapper.Map<PickHeader, PickHeaderModel>(pickH, pickModel);

            if (pickH.Location != null) pickModel.LocationName = pickH.Location.LocationName;
            if (pickH.Country != null) pickModel.ShipCountry = pickH.Country.CountryName;
        }

        private PickDetailModel mapToModel(PickDetail pickD) {
            return Mapper.Map<PickDetail, PickDetailModel>(pickD);
        }

        private string doReplacements(string str) {
            string rc = str.Replace("\\t", "\t");
            rc = rc.Replace("&quot;", "\"");
            return rc;
        }

        #endregion
    }
}
