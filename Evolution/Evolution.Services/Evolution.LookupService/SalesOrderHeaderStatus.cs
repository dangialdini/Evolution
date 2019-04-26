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

        public List<ListItemModel> FindSalesOrderHeaderStatusListItemModel() {
            return db.FindSalesOrderHeaderStatuses()
                     .Select(ohs => new ListItemModel {
                         Id = ohs.Id.ToString(),
                         Text = ohs.StatusName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public SalesOrderHeaderStatusModel FindSalesOrderHeaderStatusByValueModel(SalesOrderHeaderStatus sos) {
            SalesOrderHeaderStatusModel model = null;

            var item = db.FindSalesOrderHeaderStatuses()
                         .Where(ohs => ohs.StatusValue == (int)sos)
                         .FirstOrDefault();
            if (item != null) model = MapToModel(item);

            return model;
        }

        public SalesOrderHeaderStatusModel MapToModel(SalesOrderHeaderStatu item) {
            var model = Mapper.Map<SalesOrderHeaderStatu, SalesOrderHeaderStatusModel>(item);
            return model;
        }

        public SalesOrderHeaderStatusModel Clone(SalesOrderHeaderStatusModel item) {
            var newItem = Mapper.Map<SalesOrderHeaderStatusModel, SalesOrderHeaderStatusModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateSalesOrderHeaderStatus(SalesOrderHeaderStatusModel status, UserModel user, string lockGuid) {
            var error = validateModel(status);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(SalesOrderHeaderStatu).ToString(), status.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "StatusName");

                } else {
                    SalesOrderHeaderStatu temp = null;
                    if (status.Id != 0) temp = db.FindSalesOrderHeaderStatus(status.Id);
                    if (temp == null) temp = new SalesOrderHeaderStatu();

                    Mapper.Map<SalesOrderHeaderStatusModel, SalesOrderHeaderStatu>(status, temp);

                    db.InsertOrUpdateSalesOrderHeaderStatus(temp);
                    status.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteSalesOrderHeaderStatus(int id) {
            db.DeleteSalesOrderHeaderStatus(id);
        }

        public string LockSalesOrderHeaderStatus(SalesOrderHeaderStatusModel model) {
            return db.LockRecord(typeof(SalesOrderHeaderStatu).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(SalesOrderHeaderStatusModel model) {
            var error = isValidRequiredString(getFieldValue(model.StatusName), 50, "StatusName", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var ohs = db.FindSalesOrderHeaderStatuses()
                             .Where(pos => pos.StatusName == model.StatusName ||
                                           pos.StatusValue == model.StatusValue)
                             .FirstOrDefault();
                if (ohs != null &&                 // Record was found
                    ((ohs.Id == 0 ||               // when creating new or
                      ohs.Id != model.Id))) {      // when updating existing
                    if (ohs.StatusValue == model.StatusValue) {
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
