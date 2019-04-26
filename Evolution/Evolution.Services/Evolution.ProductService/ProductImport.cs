using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.MediaService;
using Evolution.FileCompressionService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;

namespace Evolution.ProductService {
    public partial class ProductService {

        List<string> _headings = null;

        public Error ValidateProducts(CompanyModel company, UserModel user, List<string> headings) {
            var error = new Error();

            _headings = headings;

            var brandList = db.FindBrands().ToList();

            foreach (var row in db.FindFileImportRows(company.Id, user.Id)
                                  .Skip(1)         // Skip first record (headers)
                                  .ToList()) {
                var brandName   = getField(row, "BrandName");
                var itemName    = getField(row, "ItemName");
                var itemNumber  = getField(row, "ItemNumber");
                var description = getField(row, "ItemDescription");
                var unitCBM     = getField(row, "UnitCBM");
                var minSaleQty  = getField(row, "MinSaleQty");
                var active      = getField(row, "Active");

                if (brandName != null && !string.IsNullOrEmpty(brandName.Value.Trim())) {
                    // Find the brand
                    if (brandList.Where(b => b.BrandName.ToLower() == brandName.Value.ToLower())
                                .Count() == 0) {
                        row.ErrorMessage = "Brand '" + brandName.Value + "' not found!";

                    } else if (itemName == null || string.IsNullOrEmpty(itemName.Value.Trim())) {
                        row.ErrorMessage = "Item Name must not be empty!";

                    } else if (itemNumber == null || string.IsNullOrEmpty(itemNumber.Value.Trim())) {
                        row.ErrorMessage = "Item Number must not be empty!";

                    } else if (unitCBM == null || string.IsNullOrEmpty(unitCBM.Value.Trim()) ||
                                !unitCBM.Value.IsValidDec()) {
                        row.ErrorMessage = "Invalid Unit CBM! Please ensure that a decimal value is supplied and that the Unit CBM column is selected";

                    } else if (minSaleQty == null || string.IsNullOrEmpty(minSaleQty.Value.Trim()) ||
                                !minSaleQty.Value.IsValidInt()) {
                        row.ErrorMessage = "Invalid Minimum Sale Quantity! Please ensure that an ineteger value is supplied and that the Min Sale Qty column is selected";

                    } else if (active == null || string.IsNullOrEmpty(active.Value.Trim())) {
                        row.ErrorMessage = "Please ensure that the Active column is selected";
                    } else {
                        row.ErrorMessage = "";
                    }

                } else {
                    row.ErrorMessage = "Invalid Brand Name! Please ensure that a Brand Name column is selected";
                }

                db.InsertOrUpdateFileImportRow(row);

                if (!string.IsNullOrEmpty(row.ErrorMessage)) {
                    error.SetError(EvolutionResources.errImportErrorsFound);
                }
            }

            return error;
        }

        public Error ImportProducts(CompanyModel company,
                                    UserModel user,
                                    List<string> headings,
                                    string zipImageFile) {
            var error = new Error();

            foreach (var row in db.FindFileImportRows(company.Id, user.Id)
                                  .Skip(1)         // Skip first record (headers)
                                  .ToList()) {

                var brandName = getField(row, "BrandName");
                var itemName = getField(row, "ItemName");
                var itemNumber = getField(row, "ItemNumber");
                var description = getField(row, "ItemDescription");
                var unitCBM = getField(row, "UnitCBM");
                var minSaleQty = getField(row, "MinSaleQty");
                var active = getField(row, "active");

                var product = db.FindProduct(itemNumber.Value);
                if (product == null) product = new Product {
                    CreatedDate = DateTimeOffset.Now,
                    CreatedById = user.Id
                };

                product.Brand = db.FindBrand(brandName.Value);
                product.ItemName = itemName.Value;
                product.ItemNumber = itemNumber.Value;
                product.ItemDescription = description.Value;
                product.UnitCBM = unitCBM.Value.ParseDouble();
                product.MinSaleQty = minSaleQty.Value.ParseInt();
                product.Enabled = active.Value.ParseBool();
                db.InsertOrUpdateProduct(product);
            }

            // Import the images for the products
            var unfoundCodes = "";

            var tempFolder = MediaServices.GetTempFolder() + "ProductImport";
            MediaService.MediaService.CreateDirectory(tempFolder);

            error = FileCompressionService.Zip.UnzipFile(zipImageFile, tempFolder);
            if (!error.IsError) {
                string[] imageList = Directory.GetFiles(tempFolder);
                if (imageList != null) {
                    foreach (var imageFile in imageList) {
                        string prodCode = imageFile.FileName().ChangeExtension("");

                        // Strip version numbers
                        int pos1 = prodCode.IndexOf("[");
                        if (pos1 != -1) prodCode = prodCode.Substring(0, pos1);

                        var product = FindProductModel(prodCode, -1, company, false);
                        if (product == null) {
                            if (!string.IsNullOrEmpty(unfoundCodes)) unfoundCodes += ", ";
                            unfoundCodes += imageFile.FileName();

                        } else {
                            // Create a media item and attach it to the product
                            var prodMedia = new ProductMediaModel();
                            error = AddMediaToProduct(product,
                                                      company,
                                                      user,
                                                      imageFile,
                                                      prodMedia,
                                                      FileCopyType.Move);

                            if (!error.IsError) {
                                // If the product doesn't have a default image, set it to the image just uploaded
                                if (product.PrimaryMediaId == null) {
                                    product.PrimaryMediaId = prodMedia.Id;
                                    InsertOrUpdateProduct(product, user, LockProduct(product));
                                }
                            }
                        }
                    }
                }
            }
            MediaService.MediaService.DeleteDirectory(tempFolder);

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
