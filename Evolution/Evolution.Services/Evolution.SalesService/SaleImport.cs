using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;

namespace Evolution.SalesService {
    public partial class SalesService {

        List<string> _headings;

        public Error ValidateOrders(CompanyModel company, UserModel user, List<string> headings,
                                         string dateFormat) {
            var error = new Error();

            _headings = headings;

            int rowNum = 1,
                customerIdx = -1,
                countryIdx = -1,
                locationIdx = -1,
                dwoIdx = -1,
                dwcIdx = -1,
                brandCatIdx = -1,
                productIdx = -1;

            foreach (var row in db.FindFileImportRows(company.Id, user.Id)
                                  .Skip(1)         // Skip first record (headers)
                                  .ToList()) {
                if(rowNum == 1) {
                    customerIdx = findField(row, "CustomerName");
                    countryIdx = findField(row, "ShipCountry");
                    locationIdx = findField(row, "LocationId");
                    dwoIdx = findField(row, "DeliveryWindowOpen");
                    dwcIdx = findField(row, "DeliveryWindowClose");

                    brandCatIdx = findField(row, "BrandCategory");
                    productIdx = findField(row, "ItemNumber");

                    rowNum++;
                }

                var fields = row.FileImportFields.Cast<FileImportField>().ToList();

                FileImportField customer = null;
                if(customerIdx >= 0) customer = fields[customerIdx];
                FileImportField country = null;
                if (countryIdx >= 0) country = fields[countryIdx];
                FileImportField location = null;
                if (locationIdx >= 0) location = fields[locationIdx];
                FileImportField dwo = null;
                if (dwoIdx >= 0) dwo = fields[dwoIdx];
                FileImportField dwc = null;
                if (dwcIdx >= 0) dwc = fields[dwcIdx];
                FileImportField brandCat = null;
                if (brandCatIdx >= 0) brandCat = fields[brandCatIdx];
                FileImportField product = null;
                if (productIdx >= 0) product = fields[productIdx];

                if (customer != null && !string.IsNullOrEmpty(customer.Value.Trim())) {
                    // Find the customer
                    var cust = db.FindCustomer(company.Id, customer.Value);
                    if (cust == null) {
                        row.ErrorMessage = "Customer '" + customer.Value + "' not found!";

                    } else {
                        row.CustomerId = cust.Id;

                        if (country == null || db.FindCountry(country.Value) == null) {
                            row.ErrorMessage = "Country '" + (country == null ? "" : country.Value) + "' not found!";

                        } else if (location == null || db.FindLocation(company.Id, location.Value) == null) {
                            row.ErrorMessage = "Ship From Location '" + (location == null ? "" : location.Value) + "' not found!";

                        } else if (dwo == null || !string.IsNullOrEmpty(dwo.Value) && !dwo.Value.IsValidDate(dateFormat)) {
                            row.ErrorMessage = "Invalid Delivery Window Open Date '" + (dwo == null ? "" : dwo.Value) + "' !";

                        } else if (dwc == null || !string.IsNullOrEmpty(dwc.Value) && !dwc.Value.IsValidDate(dateFormat)) {
                            row.ErrorMessage = "Invalid Delivery Window Close Date '" + (dwc == null ? "" : dwc.Value) + "' !";

                        } else if (brandCat == null || db.FindBrandCategory(company.Id, brandCat.Value) == null) {
                            row.ErrorMessage = "Brand Category '" + (brandCat == null ? "" : brandCat.Value) + "' not found!";

                        } else if(product == null) {
                            row.ErrorMessage = "Item Number '" + (product == null ? "" : product.Value) + "' not found!";

                        } else {
                            var prod = db.FindProduct(product.Value);
                            if (prod == null) {
                                row.ErrorMessage = "Item Number '" + (product == null ? "" : product.Value) + "' not found!";

                            } else {
                                row.ProductId = prod.Id;
                                row.SupplierId = prod.PrimarySupplierId;

                                row.ErrorMessage = "";
                            }
                        }
                    }

                } else {
                    row.ErrorMessage = "Invalid Customer! Please ensure that a Customer column is selected or a valid customer name supplied";
                }

                db.InsertOrUpdateFileImportRow(row);

                if (!string.IsNullOrEmpty(row.ErrorMessage)) {
                    error.SetError(EvolutionResources.errImportErrorsFound);
                }
            }

            return error;
        }

        public Error ImportSales(CompanyModel company,
                                      UserModel user,
                                      int soStatusId,
                                      int sourceId,
                                      List<string> headings,
                                      string dateFormat,
                                      int tz) {
            var error = new Error();

            int lastCustomerId = -1,
                lineNo = 0;
            SalesOrderHeaderModel soh = null;

            var lineStatus = db.FindSalesOrderLineStatuses()
                               .Where(s => s.StatusName.ToLower() == "unpicked")
                               .FirstOrDefault();

            var nextAction = db.FindSaleNextActions()
                               .Where(na => na.Id == (int)Enumerations.SaleNextAction.None)
                               .FirstOrDefault();

            // Create an order for each supplier
            foreach (var row in db.FindFileImportRows(company.Id, user.Id)
                                  .Skip(1)         // Skip first record (headers)
                                  .OrderBy(r => r.CustomerId)
                                  .ToList()) {

                if (row.CustomerId != lastCustomerId) {
                    // Found another supplier, so start a new order
                    soh = new SalesOrderHeaderModel {
                        CompanyId = company.Id,
                        SourceId = sourceId,
                        CustomerId = row.CustomerId,
                        OrderNumber = (int)LookupService.GetNextSequenceNumber(company, SequenceNumberType.SalesOrderNumber),
                        OrderDate = DateTimeOffset.Now,
                        SOStatus = soStatusId,
                        SalespersonId = user.Id,
                        IsConfirmedAddress = false,
                        IsManualFreight = false,
                        DateCreated = DateTimeOffset.Now,
                    };

                    soh.EndUserName = getFieldValue(row, "EndUserName");

                    soh.ShipAddress1 = getFieldValue(row, "ShipAddress1");
                    soh.ShipSuburb = getFieldValue(row, "ShipSuburb");
                    soh.ShipState = getFieldValue(row, "ShipState");
                    soh.ShipPostcode = getFieldValue(row, "ShipPostcode");
                    soh.ShipCountryId = db.FindCountry(getFieldValue(row, "ShipCountry")).Id;
                    soh.LocationId = db.FindLocation(company.Id, getFieldValue(row, "LocationId")).Id;

                    soh.WarehouseInstructions = getFieldValue(row, "WarehouseInstructions");

                    soh.CustPO = getFieldValue(row, "CustPO");

                    soh.DeliveryWindowOpen = getFieldValue(row, "DeliveryWindowOpen").PadLeft(10, '0').ParseDateTime(dateFormat, tz);
                    soh.DeliveryWindowClose = getFieldValue(row, "DeliveryWindowClose").PadLeft(10, '0').ParseDateTime(dateFormat, tz);

                    soh.ManualDWSet = getFieldValue(row, "ManualDWSet").ParseBool();
                    soh.NextActionId = nextAction.Id;

                    soh.DeliveryInstructions = getFieldValue(row, "DeliveryInstructions");
                    soh.DeliveryContact = getFieldValue(row, "DeliveryContact");

                    soh.BrandCategoryId = db.FindBrandCategory(company.Id, getFieldValue(row, "BrandCategory")).Id;

                    InsertOrUpdateSalesOrderHeader(soh, user, "");

                    lastCustomerId = row.CustomerId.Value;
                    lineNo = 1000;
                }

                // Add items to the new order
                var sod = new SalesOrderDetailModel {
                    CompanyId = company.Id,
                    SalesOrderHeaderId = soh.Id,
                    LineNumber = lineNo,
                    ProductId = row.ProductId,
                    ProductDescription = row.Product.ItemName,
                    UnitPriceExTax = Convert.ToDecimal(getField(row, "UnitPriceExTax").Value),
                    DiscountPercent = Convert.ToDecimal(getField(row, "DiscountPercent").Value),
                    TaxCodeId = null,
                    OrderQty = Convert.ToInt32(getField(row, "Quantity").Value),
                    LineStatusId = null
                };
                if (row.Supplier != null) sod.TaxCodeId = row.Supplier.TaxCodeId.Value;
                if (lineStatus != null) sod.LineStatusId = lineStatus.Id;

                InsertOrUpdateSalesOrderDetail(sod, "");

                lineNo += 1000;
            }

            return error;
        }

        private int findField(FileImportRow row, string fieldName) {
            int rc = -1;

            for (int i = 0; i < _headings.Count() && rc == -1; i++) {
                if (_headings[i].ToLower() == fieldName.ToLower()) rc = i;
            }
            return rc;
        }

        private FileImportField getField(FileImportRow row, string fieldName) {
            FileImportField result = null;
            var fields = row.FileImportFields.Cast<FileImportField>().ToList();

            int idx = findField(row, fieldName);
            if(idx != -1) result = fields[idx];
            return result;
        }

        private string getFieldValue(FileImportRow row, string fieldName) {
            var field = getField(row, fieldName);
            return (field == null ? "" : field.Value);
        }
    }
}
