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

namespace Evolution.PurchasingService {
    public partial class PurchasingService {

        List<string> _headings;

        public Error ValidateOrders(CompanyModel company, UserModel user, List<string> headings) {
            var error = new Error();

            _headings = headings;

            foreach (var row in db.FindFileImportRows(company.Id, user.Id)
                                  .Skip(1)         // Skip first record (headers)
                                  .ToList()) {

                var prodCode     = getField(row, "ProductCode");
                var supplierName = getField(row, "SupplierName");
                var unitPrice    = getField(row, "UnitPrice");
                var quantity     = getField(row, "Quantity");

                if (prodCode != null && !string.IsNullOrEmpty(prodCode.Value.Trim())) {
                    // Find the product
                    var products = db.FindProducts()
                                     .Where(p => p.ItemNumber == prodCode.Value)
                                     .ToList();
                    if (products == null || products.Count() == 0) {
                        row.ErrorMessage = "Product '" + prodCode.Value + "' not found!";
                    } else if (products.Count() > 1) {
                        row.ErrorMessage = "Multiple Products with code '" + prodCode.Value + "' found!";
                    } else {
                        row.Product = db.FindProduct(prodCode.Value);

                        // Find the supplier
                        var suppliers = db.FindSuppliers(company.Id)
                                          .Where(s => s.CompanyId == company.Id &&
                                                      s.Name == supplierName.Value)
                                          .ToList();
                        if (suppliers == null || suppliers.Count() == 0) {
                            if (supplierName != null && string.IsNullOrEmpty(supplierName.Value)) {
                                row.ErrorMessage = "Supplier could not be found! Please ensure that a Supplier column is selected";
                            } else {
                                row.ErrorMessage = "Supplier '" + supplierName.Value + "' not found!";
                            }
                        } else if (suppliers.Count() > 1) {
                            row.ErrorMessage = "Multiple Suppliers with name '" + supplierName.Value + "' found!";
                        } else { 
                            row.Supplier = suppliers.First();

                            // Check the unit price
                            if(unitPrice != null && !unitPrice.Value.IsValidDec()) {
                                row.ErrorMessage = "Invalid Unit Price! Please ensure that the Unit Price is a monetary value and that a Unit Price column is selected";

                            } else {
                                if(quantity != null && !quantity.Value.IsValidInt()) {
                                    row.ErrorMessage = "Invalid Quantity! Please ensure that the Quantity is an integer value and that a Quantity column is selected";
                                } else {
                                    row.ErrorMessage = "";
                                }
                            }
                        }
                    }

                } else {
                    row.ErrorMessage = "Invalid Product Code! Please ensure that a Product Code column is selected";
                }

                db.InsertOrUpdateFileImportRow(row);

                if (!string.IsNullOrEmpty(row.ErrorMessage)) {
                    error.SetError(EvolutionResources.errImportErrorsFound);
                }
            }

            return error;
        }

        public Error ImportOrders(CompanyModel company, 
                                       UserModel user, 
                                       int locationId, 
                                       List<string> headings) {
            var error = new Error();

            int lastSupplierId = -1,
                lineNo = 0;
            PurchaseOrderHeaderModel poh = null;

            // Create an order for each supplier
            foreach (var row in db.FindFileImportRows(company.Id, user.Id)
                                  .Skip(1)         // Skip first record (headers)
                                  .OrderBy(r => r.Product.Supplier.Name)
                                  .ToList()) {

                if (row.SupplierId != lastSupplierId) {
                    // Found another supplier, so start a new order
                    poh = new PurchaseOrderHeaderModel {
                        CompanyId = company.Id,
                        SupplierId = row.SupplierId,
                        OrderNumber = LookupService.GetNextSequenceNumber(company, SequenceNumberType.PurchaseOrderNumber),
                        OrderDate = DateTimeOffset.Now,
                        POStatus = LookupService.FindPurchaseOrderHeaderStatusByValueModel(PurchaseOrderStatus.OrderPlanned).Id,
                        SalespersonId = user.Id,
                        LocationId = locationId,
                        CurrencyId = row.Product.Supplier.CurrencyId,
                        ExchangeRate = LookupService.FindCurrencyModel(row.Product.Supplier.CurrencyId.Value).ExchangeRate.Value
                    };
                    InsertOrUpdatePurchaseOrderHeader(poh, user, "");

                    lastSupplierId = row.SupplierId.Value;
                    lineNo = 1000;
                }

                // Add items to the new order
                var pod = new PurchaseOrderDetailModel {
                    CompanyId = company.Id,
                    PurchaseOrderHeaderId = poh.Id,
                    LineNumber = lineNo,
                    ProductId = row.ProductId,
                    ProductDescription = row.Product.ItemName,
                    UnitPriceExTax = Convert.ToDecimal(getField(row, "UnitPrice").Value),
                    TaxCodeId = row.Product.Supplier.TaxCodeId.Value,
                    OrderQty = Convert.ToInt32(getField(row, "Quantity").Value)
                };
                InsertOrUpdatePurchaseOrderDetail(pod, user, "");

                lineNo += 1000;
            }

            return error;
        }

        private FileImportField getField(FileImportRow row, string fieldName) {
            FileImportField result = null;
            var fields = row.FileImportFields.Cast<FileImportField>().ToList();

            for (int i = 0; i < _headings.Count(); i++) {
                if (_headings[i].ToLower() == fieldName.ToLower()) {
                    result = fields[i];
                    i = _headings.Count();
                }
            }
            return result;
        }
    }
}
