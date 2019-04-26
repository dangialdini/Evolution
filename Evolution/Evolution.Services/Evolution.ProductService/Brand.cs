using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.ProductService {
    public partial class ProductService {

        #region Public members    

        public List<BrandModel> FindBrandModel() {
            return db.FindBrands()
                     .Select(b => new BrandModel {
                         Id = b.Id,
                         BrandName = b.BrandName,
                         Enabled = b.Enabled
                     })
                     .ToList();
        }

        public List<ListItemModel> FindBrandListItemModel() {
            return db.FindBrands()
                     .Select(b => new ListItemModel {
                         Id = b.Id.ToString(),
                         Text = b.BrandName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public BrandListModel FindBrandListModel(int index, int pageNo, int pageSize, string search, bool bShowHidden = true) {
            var model = new BrandListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindBrands(bShowHidden)
                             .Where(b => string.IsNullOrEmpty(search) ||
                                         (b.BrandName != null && b.BrandName.ToLower().Contains(search.ToLower())));

            model.TotalRecords = allItems.Count();
            foreach(var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public BrandModel FindBrandModel(int id, bool bCreateEmptyIfNotfound = true) {
            BrandModel model = null;

            var b = db.FindBrand(id);
            if (b == null) {
                if (bCreateEmptyIfNotfound) model = new BrandModel();

            } else {
                model = MapToModel(b);
            }

            return model;
        }

        public BrandModel MapToModel(Brand item) {
            var newItem = Mapper.Map<Brand, BrandModel>(item);
            newItem.ProductCount = item.Products.Count();
            return newItem;
        }

        public BrandModel MapToModel(BrandModel item) {
            var newItem = Mapper.Map<BrandModel, BrandModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateBrand(BrandModel brand, UserModel user, string lockGuid) {
            var error = validateModel(brand);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Brand).ToString(), brand.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "BrandName");

                } else {
                    Brand temp = null;
                    if (brand.Id != 0) temp = db.FindBrand(brand.Id);
                    if (temp == null) temp = new Brand();

                    Mapper.Map<BrandModel, Brand>(brand, temp);

                    db.InsertOrUpdateBrand(temp);
                    brand.Id = temp.Id;
                }
            }
            return error;
        }

        public Error DeleteBrand(int id) {
            var error = new Error();

            var brand = db.FindBrand(id);
            if (brand != null) {
                int productCount = brand.Products.Count();
                if (productCount > 0) {
                    error.SetError(EvolutionResources.errCantDeleteBrandWhenProductsLinked, "", productCount.ToString());
                } else {
                    db.DeleteBrand(id);
                }
            }
            return error;
        }

        public string LockBrand(BrandModel model) {
            return db.LockRecord(typeof(Brand).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(BrandModel model) {
            var error = isValidRequiredString(getFieldValue(model.BrandName), 64, "BrandName", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var taxCode = db.FindBrand(model.BrandName);
                if (taxCode != null &&                 // Record was found
                    ((taxCode.Id == 0 ||               // when creating new or
                      taxCode.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errBrandAlreadyExists, "BrandName");
                }
            }

            return error;
        }

        #endregion
    }
}
