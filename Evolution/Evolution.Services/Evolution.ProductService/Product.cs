using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.MediaService;
using Evolution.AuditService;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.ProductService {
    public partial class ProductService {

        #region Private members

        MediaService.MediaService _mediaServices = null;
        MediaService.MediaService MediaServices {
            get {
                if (_mediaServices == null) _mediaServices = new MediaService.MediaService(db);
                return _mediaServices;
            }
        }

        #endregion

        #region Public members    

        public List<ProductModel> FindProductsForBrandModel(int brandId,
                                                      string sortColumn = "", SortOrder sortOrder = SortOrder.Asc,
                                                      bool bShowHidden = false) {
            return db.FindProductsForBrand(brandId, sortColumn, sortOrder, bShowHidden)
                     .Select(p => new ProductModel {
                         Id = p.Id,
                         BrandId = p.BrandId,
                         BrandName = p.Brand.BrandName,
                         CreatedDate = p.CreatedDate,
                         CreatedById = p.CreatedById,
                         CreatedByText = (p.User_CreatedBy.FirstName + " " + p.User_CreatedBy.LastName).Trim(),
                         ItemName = p.ItemName,
                         ItemNumber = p.ItemNumber,
                         Picture = (p.ProductMedia != null ? p.ProductMedia.Medium.FolderName + p.ProductMedia.Medium.FileName : "/Content/default.jpg"),
                         ProductStatus = p.ProductStatus,
                         PrimarySupplierId = p.PrimarySupplierId,
                         TaxCodeId = p.Supplier.TaxCodeId,
                         SupplierName = p.Supplier.Name,
                         Enabled = p.Enabled
                     })
                     .ToList();
        }

        public List<ListItemModel> FindProductsForBrandCategoryListItemModel(int brandCategoryId,
                                                                             int availabilityId = 0,
                                                                             int index = 0, 
                                                                             int pageNo = 1, 
                                                                             int pageSize = Int32.MaxValue, 
                                                                             string search = "") {
            // Used in PurchaseOrder add line screen
            var model = new List<ListItemModel>();
            List<Product> allItems = new List<Product>();

            string mediaUrl = MediaServices.GetProductImageFolder(true);

            // Do a case-insensitive search
            var brandCategory = db.FindBrandCategory(brandCategoryId);
            if (brandCategory != null) {
                foreach (var category in brandCategory.BrandBrandCategories) {
                    allItems.AddRange(category.Brand
                                              .Products
                                              .Where(p => (availabilityId == 0 || p.ProductAvailabilityId == availabilityId) &&
                                                          (string.IsNullOrEmpty(search) ||
                                                           (p.ItemName != null && p.ItemName.ToLower().Contains(search.ToLower())) ||
                                                           (p.ItemNumber != null && p.ItemNumber.ToLower().Contains(search.ToLower())))));
                }

                model = allItems.OrderBy(pl => pl.ItemNumber)
                                .Skip((pageNo - 1) * pageSize)
                                .Take(pageSize)
                                .Select(p => new ListItemModel {
                                            Id = p.ItemNumber,
                                            Text = p.ItemName,
                                            ImageURL = (p.ProductMedia != null ? p.ProductMedia.Medium.FolderName + p.ProductMedia.Medium.FileName : "")
                                      }).ToList();                
            }
            return model;
        }

        public List<ListItemModel> FindProductListItemModel(string search, int maxRows = 0) {
            List<ListItemModel> model = new List<ListItemModel>();

            if (maxRows > 0) {
                foreach (var item in db.FindProducts()
                                       .Where(p => p.LOVItem_ProductAvailability.ItemValue1 == ((int)ProductAvailability.Live).ToString() &&
                                                   (string.IsNullOrEmpty(search) ||
                                                    p.ItemName.Contains(search) ||
                                                    p.ItemNumber.Contains(search)))
                                       .Take(maxRows)) {
                    model.Add(new ListItemModel(item.ItemName, item.Id.ToString())); //.ItemNumber));
                }
            } else {
                foreach (var item in db.FindProducts()
                                       .Where(p => p.LOVItem_ProductAvailability.ItemValue1 == ((int)ProductAvailability.Live).ToString() &&
                                                   (string.IsNullOrEmpty(search) ||
                                                    p.ItemName.Contains(search) ||
                                                    p.ItemNumber.Contains(search)))) {
                    model.Add(new ListItemModel(item.ItemName, item.Id.ToString())); //.ItemNumber));
                }
            }
            return model;
        }

        public ProductListModel FindProductsListModel(int brandId,
                                                      int availabilityId,
                                                      int index = 0, 
                                                      int pageNo = 1, 
                                                      int pageSize = Int32.MaxValue, 
                                                      string search = "",
                                                      string sortColumn = "", 
                                                      SortOrder sortOrder = SortOrder.Asc) {
            var model = new ProductListModel();

            string mediaUrl = MediaServices.GetProductImageFolder(true);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindProductsForBrand(brandId, sortColumn, sortOrder, true)
                             .Where(p => (availabilityId == 0 || p.ProductAvailabilityId == availabilityId) &&
                                         (string.IsNullOrEmpty(search) ||
                                          (p.ItemName != null && p.ItemName.ToLower().Contains(search.ToLower())) ||
                                          (p.ItemNumber != null && p.ItemNumber.ToLower().Contains(search.ToLower()))));

            model.TotalRecords = allItems.Count();

            if (sortColumn.ToLower() == "productstatustext") {
                // Sorting done here because the text is not obtained from database field
                foreach (var item in allItems) {
                    var newItem = MapToModel(item);
                    newItem.Picture = (item.ProductMedia != null ? item.ProductMedia.Medium.FolderName + item.ProductMedia.Medium.FileName : "");
                    model.Items.Add(newItem);
                }

                if (sortOrder == SortOrder.Desc) {
                    model.Items = model.Items
                                       .OrderByDescending(i => i.ProductStatusText)
                                       .Skip((pageNo - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToList();
                } else {
                    model.Items = model.Items
                                       .OrderBy(i => i.ProductStatusText)
                                       .Skip((pageNo - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToList();
                }

            } else {
                foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                      .Take(pageSize)) {
                    model.Items.Add(MapToModel(item));
                }
            }
            return model;
        }

        public ProductModel FindProductModel(int id, int? brandId, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            ProductModel model = null;

            var p = db.FindProduct(id);
            if (p == null) {
                if (bCreateEmptyIfNotfound) model = new ProductModel { BrandId = brandId };

            } else {
                model = MapToModel(p);
            }
            if (model != null && string.IsNullOrEmpty(model.Picture)) model.Picture = "/Content/default.jpg";

            return model;
        }

        public ProductModel FindProductModel(string productNo, int? brandId, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            ProductModel model = null;

            var p = db.FindProduct(productNo);
            if (p == null) {
                if (bCreateEmptyIfNotfound) model = new ProductModel { BrandId = brandId };

            } else {
                model = MapToModel(p);
            }
            if (model != null) {
                if (string.IsNullOrEmpty(model.Picture)) model.Picture = "/Content/default.jpg";
            }

            return model;
        }

        public BrandCategoryModel FindProductBrandCategoryModel(int companyId, int productId) {
            var p = db.FindProductBrandCategory(companyId, productId);
            return MapToModel(p);
        }

        List<LOVItem> productStatusList = null;

        public ProductModel MapToModel(Product item) {
            var newItem = Mapper.Map<Product, ProductModel>(item);
            if (item.Brand != null) {
                newItem.BrandName = item.Brand.BrandName;
                var brandBrandCategory = item.Brand
                                             .BrandBrandCategories
                                             .Where(bbc => bbc.BrandId == item.BrandId)
                                             .FirstOrDefault();
                if (brandBrandCategory != null) newItem.Category = brandBrandCategory.BrandCategory.CategoryName;
            }
            if (item.CreatedById != null) newItem.CreatedByText = (item.User_CreatedBy.FirstName + " " + item.User_CreatedBy.LastName).Trim();
            if (item.ApprovedById != null) newItem.ApprovedByText = (item.User_ApprovalBy.FirstName + " " + item.User_ApprovalBy.LastName).Trim();

            if (item.Supplier != null) {
                newItem.SupplierName = item.Supplier.Name;
                newItem.TaxCodeId = item.Supplier.TaxCodeId;
            }

            if (item.ProductAvailabilityId != null) {
                var prodAvail = db.FindLOVItem(item.ProductAvailabilityId.Value);
                newItem.ProductAvailabilityText = prodAvail.ItemText;
            }

            if (productStatusList == null) {
                var lov = db.FindLOV(LOVName.ProductStatus);
                if (lov != null) productStatusList = lov.LOVItems.ToList();
            }
            if (productStatusList != null) {
                var temp = productStatusList.Where(sl => sl.ItemValue1 == newItem.ProductStatus.ToString())
                                            .FirstOrDefault();
                if (temp != null) newItem.ProductStatusText = temp.ItemText;
            }

            if (item.ProductMedia != null) {
                var media = MediaServices.MapToModel(item.ProductMedia.Medium, MediaSize.Medium, (int)MediaSize.MediumW, (int)MediaSize.MediumH);
                newItem.Picture = MediaServices.GetMediaFileName(media, true);
                newItem.PictureHtml = MediaServices.GetMediaHtml(media, MediaSize.Medium, 70, 70);
            } else {
                newItem.Picture = "/Content/default.jpg";
                newItem.PictureHtml = "";
            }
            if (item.LOVItem_Material != null) newItem.MaterialText = item.LOVItem_Material.ItemText;

            if(item.ProductAdditionalCategory != null) {
                Mapper.Map(item.ProductAdditionalCategory, newItem.AdditionalCategory);
            }

            return newItem;
        }

        public ProductModel MapToModel(ProductModel item) {
            return Mapper.Map<ProductModel, ProductModel>(item);
        }

        public void ApplyUnitConversions(ProductModel product, UnitOfMeasure uom) { 
            // Converts a model in metric to the user's 'view' units.
            // The model must be in metric to start with.
            if (uom == UnitOfMeasure.Imperial) {
                // Convert measures to imperial
                product.Length = product.Length.CmToInches();
                product.Width = product.Width.CmToInches();
                product.Height = product.Height.CmToInches();
                product.Weight = product.Weight.KgToLb();

                product.PackedLength = product.PackedLength.CmToInches();
                product.PackedWidth = product.PackedWidth.CmToInches();
                product.PackedHeight = product.PackedHeight.CmToInches();
                product.PackedWeight = product.PackedWeight.KgToLb();

                product.InnerLength = product.InnerLength.CmToInches();
                product.InnerWidth = product.InnerWidth.CmToInches();
                product.InnerHeight = product.InnerHeight.CmToInches();
                product.InnerWeight = product.InnerWeight.KgToLb();

                product.MasterLength = product.MasterLength.MToFeet();
                product.MasterWidth = product.MasterWidth.MToFeet();
                product.MasterHeight = product.MasterHeight.MToFeet();
                product.MasterWeight = product.MasterWeight.KgToLb();
            }
        }

        public Error InsertOrUpdateProduct(ProductModel product, UserModel user, string lockGuid) {
            Error error = ValidateModel(product);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Product).ToString(), product.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "ItemNumber");

                } else {
                    // Save the product
                    Product temp = null;
                    if (product.Id != 0) temp = db.FindProduct(product.Id);
                    if (temp == null) temp = new Product();

                    var before = Mapper.Map<Product, Product>(temp);

                    Mapper.Map<ProductModel, Product>(product, temp);

                    if (temp.Id == 0) {
                        // Newly created record
                        temp.CreatedById = user.Id;
                        temp.CreatedDate = DateTimeOffset.Now;
                    }
                    db.InsertOrUpdateProduct(temp);
                    product.Id = temp.Id;

                    logChanges(before, temp, user);

                    // Now save the additional category info
                    var addCat = temp.ProductAdditionalCategory;
                    if (addCat == null) addCat = new DAL.ProductAdditionalCategory { Product = temp };
                    Mapper.Map(product.AdditionalCategory, addCat);
                    addCat.Id = product.Id;
                    db.InsertOrUpdateProductAdditionalCategory(addCat);
                }
            }
            return error;
        }

        private double? ConvertMetric(double? value, double conversionFactor) {
            double? result = null;
            if (value != null) result = value.Value * conversionFactor;
            return result;
        }

        public Error DeleteProduct(int id) {
            var error = new Error();

            // Check for orders containing the product
            int itemCount = db.FindPurchaseOrderDetailsWithProduct(id)
                              .Count();
            if (itemCount > 0) {
                error.SetError(EvolutionResources.errCantDeleteProductUsedByOrders, "", itemCount.ToString());

            } else {
                // Check for sales containing the product
                itemCount = db.FindSalesOrderDetailsWithProduct(id)
                              .Where(sod => sod.ProductId == id)
                              .Count();
                if (itemCount > 0) {
                    error.SetError(EvolutionResources.errCantDeleteProductUsedBySales, "", itemCount.ToString());

                } else {
                    // OK to delete
                    // Delete all media items, including media files
                    foreach (var productMedia in db.FindProductMedias(id)
                                                  .ToList()) {
                        DeleteProductMedia(productMedia.Id);
                    }

                    // And finally, delete the product
                    db.DeleteProduct(id);
                }
            }
            return error;
        }

        public string LockProduct(ProductModel model) {
            return db.LockRecord(typeof(Product).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        public Error ValidateModel(ProductModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.ItemNumber), 30, "ItemNumber", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.ItemName), 30, "ItemName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ItemNameLong), 100, "ItemNameLong", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ItemNameFormat), 40, "ItemNameFormat", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ItemNameStyle), 40, "ItemNameStyle", EvolutionResources.errTextDataRequiredInField);
            //if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ItemShortDescription), 30, "ItemShortDescription", EvolutionResources.errMneumonicRequired);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ItemDescription), 255, "ItemDescription", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Set), 10, "Set", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.BarCode), 31, "BarCode", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.InnerBarCode), 31, "innerBarCode", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.MasterBarCode), 31, "MasterBarCode", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.SupplierItemNumber), 30, "SupplierItemNumber", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.SupplierItemName), 30, "SupplierItemName", EvolutionResources.errTextDataRequiredInField);

            // Conversion adjustments
            if(!error.IsError) {
                if (model.Length != null && model.Width != null && model.Height != null) {
                    model.UnitCBM = (model.Length.Value / 100) *
                                    (model.Width.Value / 100) *
                                    (model.Height.Value / 100);
                }
                model.ItemName = model.ItemName.Trim();
                if (model.ProductStatus == null) model.ProductStatus = 0;
                if (model.ItemName.Right(1) == "%") {
                    model.ProductStatus = ProductStatus.CantGetAnyMore;
                    model.ItemName = model.ItemName.Substring(0, model.ItemName.Length - 1);
                }
                if (model.ItemName.Right(1) == "^") {
                    model.ProductStatus = ProductStatus.EndOfLineSecret;
                    model.ItemName = model.ItemName.Substring(0, model.ItemName.Length - 1);
                }
                if (model.ItemName.Right(1) == "#") {
                    model.ProductStatus = ProductStatus.EndOfLine;
                    model.ItemName = model.ItemName.Substring(0, model.ItemName.Length - 1);
                }

                // Descripion: "<B>Rainbow Foil Balloon</B><BR>75 x 32 x 45 cm<BR>PP, PE<BR>"
                if(!string.IsNullOrEmpty(model.ItemDescription) &&
                   model.ItemDescription.IndexOf("<BR>") != -1) {
                    string[] desc = model.ItemDescription.Replace("<BR>", "|").Split('|');

                    if(desc.Length >= 2) {
                        string[] dims = desc[1].ToLower().Replace("cm", "").Split('x');
                        if(dims != null) {
                            if (dims.Length >= 1) model.Length = dims[0].Trim().ParseDouble();
                            if (dims.Length >= 2) model.Width = dims[1].Trim().ParseDouble();
                            if (dims.Length >= 3) model.Height = dims[2].Trim().ParseDouble();
                        }
                    }
                    if(desc.Length >= 3) {
                        var lov = db.FindLOV(LOVName.Material);
                        if (lov != null) {
                            var currentItem = db.FindLOVItem(model.MaterialId == null ? 0 : model.MaterialId.Value);
                            var newItem = db.FindLOVItem(null, lov.Id, desc[2].Trim());

                            if (currentItem == null || string.IsNullOrEmpty(currentItem.ItemText) ||
                                newItem != null) {
                                if(newItem != null) model.MaterialId = newItem.Id;
                            }
                        }
                    }
                }
            }

            return error;
        }

        private void logChanges(Product before, Product after, UserModel user) {
            AuditService.LogChanges(typeof(Product).ToString(), BusinessArea.ProductDetails, user, before, after);
        }

        #endregion
    }
}
