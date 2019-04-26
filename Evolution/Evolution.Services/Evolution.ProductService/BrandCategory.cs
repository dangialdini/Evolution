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

        public List<BrandCategoryModel> FindBrandCategoriesModel(CompanyModel company) {
            return db.FindBrandCategories(company.Id)
                     .Select(bc => new BrandCategoryModel {
                         Id = bc.Id,
                         CompanyId = bc.CompanyId,
                         CategoryName = bc.CategoryName,
                         Enabled = bc.Enabled
                     })
                     .ToList();
        }

        public List<ListItemModel> FindBrandCategoryListItemModel(CompanyModel company) {
            return db.FindBrandCategories(company.Id)
                     .Select(bc => new ListItemModel {
                         Id = bc.Id.ToString(),
                         Text = bc.CategoryName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public BrandCategoryListModel FindBrandCategoriesListModel(int companyId, int index, int pageNo, int pageSize, string search) {
            var model = new BrandCategoryListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindBrandCategories(companyId, true)
                             .Where(bc => string.IsNullOrEmpty(search) ||
                                          (bc.CategoryName != null && bc.CategoryName.ToLower().Contains(search.ToLower())));

            model.TotalRecords = allItems.Count();
            foreach(var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public BrandCategoryModel FindBrandCategoryModel(int id, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            BrandCategoryModel model = null;

            var bc = db.FindBrandCategory(id);
            if (bc == null) {
                if (bCreateEmptyIfNotfound) model = new BrandCategoryModel { CompanyId = company.Id };

            } else {
                model = MapToModel(bc);
            }

            return model;
        }

        public BrandCategoryModel MapToModel(BrandCategory item) {
            var newItem = Mapper.Map<BrandCategory, BrandCategoryModel>(item);
            newItem.BrandCount = item.BrandBrandCategories.Count();
            return newItem;
        }

        public BrandCategoryModel MapToModel(BrandCategoryModel item) {
            var newItem = Mapper.Map<BrandCategoryModel, BrandCategoryModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateBrandCategory(BrandCategoryModel category, 
                                                      List<int> selectedBrandIds,
                                                      UserModel user, string lockGuid) {
            var error = validateModel(category);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(BrandCategory).ToString(), category.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "CategoryName");

                } else {
                    BrandCategory temp = null;
                    if (category.Id != 0) temp = db.FindBrandCategory(category.Id);
                    if (temp == null) temp = new BrandCategory();

                    Mapper.Map<BrandCategoryModel, BrandCategory>(category, temp);

                    db.InsertOrUpdateBrandCategory(temp, selectedBrandIds);
                    category.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteBrandCategory(int id) {
            db.DeleteBrandCategory(id);
        }

        public string LockBrandCategory(BrandCategoryModel model) {
            return db.LockRecord(typeof(BrandCategory).ToString(), model.Id);
        }

        public BrandBrandCategoryModel AddBrandToBrandCategory(CompanyModel company, BrandModel brand, BrandCategoryModel brandCategory) {
            BrandBrandCategoryModel result = new BrandBrandCategoryModel();

            BrandBrandCategory bbc = db.FindBrandBrandCategories(brandCategory.Id)
                                       .Where(bbci => bbci.BrandId == brand.Id)
                                       .FirstOrDefault();
            if (bbc == null) {
                bbc = new BrandBrandCategory {
                    Company = db.FindCompany(company.Id),
                    BrandCategory = db.FindBrandCategory(brandCategory.Id),
                    Brand = db.FindBrand(brand.Id)
                };
                db.InsertOrUpdateBrandBrandCategory(bbc);
            }
            Mapper.Map(bbc, result);

            return result;
        }

        public void DeleteBrandFromBrandCategory(BrandModel brand, BrandCategoryModel brandCategory) {
            db.DeleteBrandFromBrandCategory(brandCategory.CompanyId, brandCategory.Id, brand.Id);
        }

        public void CopyBrandCategories(CompanyModel source, CompanyModel target, UserModel user) {
            foreach (var brandCategory in FindBrandCategoriesModel(source)) {
                var newItem = Mapper.Map<BrandCategoryModel, BrandCategoryModel>(brandCategory);
                newItem.Id = 0;
                newItem.CompanyId = target.Id;
                InsertOrUpdateBrandCategory(newItem, null, user, "");
            }
        }

        #endregion

        #region Private methods

        private Error validateModel(BrandCategoryModel model) {
            var error = isValidRequiredString(getFieldValue(model.CategoryName), 64, "CategoryName", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var category = db.FindBrandCategory(model.CompanyId, model.CategoryName);
                if (category != null &&                 // Record was found
                    ((category.Id == 0 ||               // when creating new or
                      category.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errBrandCategoryAlreadyExists, "CategoryName");
                }
            }

            return error;
        }

        #endregion
    }
}
