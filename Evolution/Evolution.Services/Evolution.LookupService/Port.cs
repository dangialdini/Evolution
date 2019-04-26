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

        public List<ListItemModel> FindPortsListItemModel() {
            return db.FindPorts()
                     .Select(c => new ListItemModel {
                         Id = c.Id.ToString(),
                         Text = c.PortName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public PortModel MapToModel(Port item) {
            var newItem = Mapper.Map<Port, PortModel>(item);
            return newItem;
        }

        public PortModel Clone(PortModel item) {
            var newItem = Mapper.Map<PortModel, PortModel>(item);
            return newItem;
        }

        public Error InsertOrUpdatePort(PortModel port, UserModel user, string lockGuid) {
            var error = validateModel(port);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Port).ToString(), port.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "PortName");

                } else {
                    Port temp = null;
                    if (port.Id != 0) temp = db.FindPort(port.Id);
                    if (temp == null) temp = new Port();

                    Mapper.Map<PortModel, Port>(port, temp);

                    db.InsertOrUpdatePort(temp);
                    port.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeletePort(int id) {
            db.DeletePort(id);
        }

        public string LockPort(PortModel model) {
            return db.LockRecord(typeof(Port).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(PortModel model) {
            var error = isValidRequiredString(getFieldValue(model.PortName), 50, "PortName", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var forwarder = db.FindPort(model.PortName);
                if (forwarder != null &&                 // Record was found
                    ((forwarder.Id == 0 ||               // when creating new or
                      forwarder.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "PortName");
                }
            }

            return error;
        }

        #endregion
    }
}
