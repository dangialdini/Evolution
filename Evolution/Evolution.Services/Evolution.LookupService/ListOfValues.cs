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

namespace Evolution.LookupService {
    public partial class LookupService {

        #region Public members    

        public List<ListItemModel> FindLOVsListItemModel(bool bMultiTenanted, bool bShowHidden = false) {
            return db.FindLOVs(bMultiTenanted, bShowHidden)
                     .Select(l => new ListItemModel {
                         Id = l.Id.ToString(),
                         Text = l.LOVName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public List<LOVModel> FindLOVsModel(bool bMultiTenanted, bool bShowHidden = false) {
            return db.FindLOVs(bMultiTenanted, bShowHidden)
                     .Select(l => new LOVModel {
                         Id = l.Id,
                         LOVName = l.LOVName,
                         MultiTenanted = l.MultiTenanted
                     })
                     .ToList();
        }

        public List<ListItemModel> FindLOVItemsListItemModel(CompanyModel company, string lovName, bool bInsertSelect = false) {
            List<ListItemModel> items = new List<ListItemModel>();
            var lov = db.FindLOV(lovName);
            if (lov != null) {
                if (lov.MultiTenanted) {
                    items = lov.LOVItems
                               .Where(li => li.CompanyId == company.Id &&
                                            li.Enabled == true)
                               .OrderBy(li => li.OrderNo)
                               .ThenBy(li => li.ItemText)
                               .Select(li => new ListItemModel {
                                   Id = li.Id.ToString(),
                                   Text = li.ItemText,
                                   ImageURL = ""
                               })
                               .ToList();
                } else {
                    items = lov.LOVItems
                               .Where(li => li.Enabled == true)
                               .OrderBy(li => li.OrderNo)
                               .ThenBy(li => li.ItemText)
                               .Select(li => new ListItemModel {
                                   Id = li.Id.ToString(),
                                   Text = li.ItemText,
                                   ImageURL = ""
                               })
                               .ToList();
                }
            }
            if (bInsertSelect) items.Insert(0, new ListItemModel("Select...", "0"));
            return items;
        }

        public List<LOVItemModel> FindLOVItemsModel(CompanyModel company, string lovName) {
            List<LOVItemModel> items = new List<LOVItemModel>();
            var lov = db.FindLOV(lovName);
            if (lov != null) {
                if (lov.MultiTenanted) {
                    items = lov.LOVItems
                               .Where(li => li.CompanyId == company.Id &&
                                            li.Enabled == true)
                               .OrderBy(li => li.OrderNo)
                               .ThenBy(li => li.ItemText)
                               .Select(li => new LOVItemModel {
                                   Id = li.Id,
                                   ItemText = li.ItemText,
                                   ItemValue1 = li.ItemValue1,
                                   ItemValue2 = li.ItemValue2,
                                   Colour = (string.IsNullOrEmpty(li.Colour) ? "black" : li.Colour)
                               })
                               .ToList();
                } else {
                    items = lov.LOVItems
                               .Where(li => li.Enabled == true)
                               .OrderBy(li => li.OrderNo)
                               .ThenBy(li => li.ItemText)
                               .Select(li => new LOVItemModel {
                                   Id = li.Id,
                                   ItemText = li.ItemText,
                                   ItemValue1 = li.ItemValue1,
                                   ItemValue2 = li.ItemValue2,
                                   Colour = (string.IsNullOrEmpty(li.Colour) ? "black" : li.Colour)
                               })
                               .ToList();
                }
            }
            return items;
        }

        public LOVListModel FindLOVItemsModel(int? companyId, int index, int lovId, int pageNo, int pageSize, string search, bool bShowHidden = false) {
            var model = new LOVListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindLOVItems(companyId, lovId, bShowHidden)
                             .Where(li => string.IsNullOrEmpty(search) ||
                                          (li.ItemText != null && li.ItemText.ToLower().Contains(search.ToLower())) ||
                                          (li.ItemValue1 != null && li.ItemValue1.ToLower().Contains(search.ToLower())) ||
                                          (li.ItemValue2 != null && li.ItemValue2.ToLower().Contains(search.ToLower())) ||
                                          (li.Colour != null && li.Colour.ToLower().Contains(search.ToLower()))
                                   );

            model.TotalRecords = allItems.Count();
            model.Items = allItems.OrderBy(li => li.OrderNo)
                                  .ThenBy(li => li.ItemText)
                                  .Skip((pageNo - 1) * pageSize)
                                  .Take(pageSize)
                                  .Select(li => new LOVItemModel {
                                      Id = li.Id,
                                      CompanyId = li.CompanyId,
                                      LovId = li.LovId,
                                      ItemText = li.ItemText,
                                      ItemValue1 = (string.IsNullOrEmpty(li.ItemValue1) ? "" : li.ItemValue1),
                                      ItemValue2 = (string.IsNullOrEmpty(li.ItemValue2) ? "" : li.ItemValue2),
                                      Colour = (string.IsNullOrEmpty(li.Colour) ? "black" : li.Colour),
                                      OrderNo = li.OrderNo,
                                      Enabled = li.Enabled
                                  }).ToList();
            return model;
        }

        public LOVItemModel FindLOVItemModel(int id, int lovId, bool bCreateEmptyIfNotfound = true) {
            LOVItemModel model = null;

            var lovItem = db.FindLOVItem(id);
            if (lovItem == null) {
                if(bCreateEmptyIfNotfound) model = new LOVItemModel { LovId = lovId };

            } else {
                model = MapToModel(lovItem);
            }

            return model;
        }

        public LOVItemModel FindLOVItemModel(int id, string lovName, bool bCreateEmptyIfNotfound = true) {
            LOVItemModel model = null;

            var lov = db.FindLOV(lovName);
            if (lov != null) model = FindLOVItemModel(id, lov.Id, bCreateEmptyIfNotfound);

            return model;
        }

        public LOVItemModel FindLOVItemModel(string lovName, string itemText) {
            LOVItemModel model = null;

            var lov = db.FindLOV(lovName);
            if (lov != null) model = MapToModel(lov.LOVItems
                                                   .Where(lovi => lovi.ItemText.ToLower() == itemText.ToLower())
                                                   .FirstOrDefault());
            return model;
        }

        public LOVItemModel FindLOVItemModel(CompanyModel company, string lovName, string itemText) {
            LOVItemModel model = null;

            var lov = db.FindLOV(lovName);
            if (lov != null) model = MapToModel(lov.LOVItems
                                                   .Where(lovi => lovi.CompanyId == company.Id && 
                                                                  lovi.ItemText.ToLower() == itemText.ToLower())
                                                   .FirstOrDefault());
            return model;
        }

        public LOVItemModel FindLOVItemByValueModel(string lovName, string itemValue) {
            LOVItemModel model = null;

            var lov = db.FindLOV(lovName);
            if (lov != null) model = MapToModel(lov.LOVItems
                                                   .Where(lovi => lovi.ItemValue1 == itemValue)
                                                   .FirstOrDefault());
            return model;
        }

        public LOVItemModel MapToModel(LOVItem item) {
            if (item != null) {
                var newItem = Mapper.Map<LOVItem, LOVItemModel>(item);
                if (string.IsNullOrEmpty(newItem.Colour)) newItem.Colour = "black";
                return newItem;
            } else {
                return null;
            }
        }

        public LOVItemModel Clone(LOVItemModel item) {
            var newItem = Mapper.Map<LOVItemModel, LOVItemModel>(item);
            if (string.IsNullOrEmpty(newItem.Colour)) newItem.Colour = "black";
            return newItem;
        }

        public void MoveLOVItemUp(CompanyModel company, int lovId, int id) {
            if (company == null) {
                db.MoveLOVItemUp(null, lovId, id);
            } else {
                db.MoveLOVItemUp(company.Id, lovId, id);
            }
        }

        public void MoveLOVItemDown(CompanyModel company, int lovId, int id) {
            if (company == null) {
                db.MoveLOVItemDown(null, lovId, id);
            } else {
                db.MoveLOVItemDown(company.Id, lovId, id);
            }
        }

        public Error InsertOrUpdateLOVItem(LOVItemModel lovItem, UserModel user, string lockGuid) {
            var error = validateModel(lovItem);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(LOVItem).ToString(), lovItem.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "ItemText");

                } else {
                    LOVItem temp = null;
                    if (lovItem.Id != 0) temp = db.FindLOVItem(lovItem.Id);
                    if (temp == null) temp = new LOVItem();

                    Mapper.Map<LOVItemModel, LOVItem>(lovItem, temp);

                    if (lovItem.CompanyId < 1) lovItem.CompanyId = null;

                    db.InsertOrUpdateLOVItem(temp);
                    lovItem.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteLOVItem(int id) {
            db.DeleteLOVItem(id);
        }

        public string LockLOVItem(LOVItemModel model) {
            return db.LockRecord(typeof(LOVItem).ToString(), model.Id);
        }

        public void CopyLOVs(CompanyModel source, CompanyModel target) {
            db.CopyLOVs(source.Id, target.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(LOVItemModel model) {
            var error = isValidRequiredString(getFieldValue(model.ItemText), 128, "ItemText", EvolutionResources.errItemTextIsRequired);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ItemValue1), 16, "ItemValue1", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ItemValue2), 16, "ItemValue2", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Colour), 24, "Colour", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if an item with the same name already exists
                var lovItem = db.FindLOVItem(model.CompanyId, model.LovId, model.ItemText);
                if (lovItem != null &&                 // Supplier was found
                    ((model.Id == 0 ||                 // when creating a new supplier or
                      lovItem.Id != model.Id))) {      // when updating an existing supplier
                    error.SetError(EvolutionResources.errItemAlreadyExists, "ItemText");
                }
            }

            return error;
        }

        #endregion
    }
}
