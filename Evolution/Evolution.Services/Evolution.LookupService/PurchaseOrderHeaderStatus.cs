using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.LookupService {
    public partial class LookupService {

        #region Public members    

        public List<ListItemModel> FindPurchaseOrderHeaderStatusListItemModel() {
            return db.FindPurchaseOrderHeaderStatuses()
                     .Select(pohs => new ListItemModel {
                         Id = pohs.Id.ToString(),
                         Text = pohs.StatusName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public PurchaseOrderHeaderStatusModel FindPurchaseOrderHeaderStatusByValueModel(PurchaseOrderStatus pos) {
            PurchaseOrderHeaderStatusModel model = null;

            var item = db.FindPurchaseOrderHeaderStatuses()
                         .Where(pohs => pohs.StatusValue == (int)pos)
                         .FirstOrDefault();
            if (item != null) model = MapToModel(item);

            return model;
        }

        public PurchaseOrderHeaderStatusModel MapToModel(PurchaseOrderHeaderStatu item) {
            var newItem = Mapper.Map<PurchaseOrderHeaderStatu, PurchaseOrderHeaderStatusModel>(item);
            return newItem;
        }

        public PurchaseOrderHeaderStatusModel Clone(PurchaseOrderHeaderStatusModel item) {
            var newItem = Mapper.Map<PurchaseOrderHeaderStatusModel, PurchaseOrderHeaderStatusModel>(item);
            return newItem;
        }

        public Error InsertOrUpdatePurchaseOrderHeaderStatus(PurchaseOrderHeaderStatusModel priceLevel, UserModel user, string lockGuid) {
            var error = validateModel(priceLevel);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(PurchaseOrderHeaderStatu).ToString(), priceLevel.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "StatusName");

                } else {
                    PurchaseOrderHeaderStatu temp = null;
                    if (priceLevel.Id != 0) temp = db.FindPurchaseOrderHeaderStatus(priceLevel.Id);
                    if (temp == null) temp = new PurchaseOrderHeaderStatu();

                    Mapper.Map< PurchaseOrderHeaderStatusModel, PurchaseOrderHeaderStatu>(priceLevel, temp);

                    db.InsertOrUpdatePurchaseOrderHeaderStatus(temp);
                    priceLevel.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeletePurchaseOrderHeaderStatus(int id) {
            db.DeletePurchaseOrderHeaderStatus(id);
        }

        public string LockPurchaseOrderHeaderStatus(PurchaseOrderHeaderStatusModel model) {
            return db.LockRecord(typeof(PurchaseOrderHeaderStatu).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(PurchaseOrderHeaderStatusModel model) {
            var error = isValidRequiredString(getFieldValue(model.StatusName), 50, "StatusName", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var pohs = db.FindPurchaseOrderHeaderStatuses()
                             .Where(pos => pos.StatusName == model.StatusName ||
                                           pos.StatusValue == model.StatusValue)
                             .FirstOrDefault();
                if (pohs != null &&                 // Record was found
                    ((pohs.Id == 0 ||               // when creating new or
                      pohs.Id != model.Id))) {      // when updating existing
                    if (pohs.StatusValue == model.StatusValue) {
                        error.SetError(EvolutionResources.errItemAlreadyExists, "StatusValue");
                    } else {
                        error.SetError(EvolutionResources.errItemAlreadyExists, "StatusName");
                    }
                }
            }

            return error;
        }

        #endregion
    }
}
