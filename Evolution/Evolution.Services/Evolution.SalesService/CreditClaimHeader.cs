using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Resources;
using Evolution.CommonService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.SalesService {
    public partial class SalesService {

        public CreditClaimHeaderListModel FindCreditClaimHeaders() {
            CreditClaimHeaderListModel model = new CreditClaimHeaderListModel();

            foreach (var cch in db.FindCreditClaimHeaders()) {
                model.Items.Add(MapToModel(cch));    
            }
            return model;
        }

        public CreditClaimHeaderModel MapToModel(CreditClaimHeader item) {
            return Mapper.Map<CreditClaimHeader, CreditClaimHeaderModel>(item);
        }

        public Error InsertOrUpdateCreditClaimHeader(CreditClaimHeaderModel cch, string lockGuid) {
            var error = new Error();
            error = validateModel(cch);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(CreditClaimHeaderModel).ToString(), cch.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "");
                } else {
                    CreditClaimHeader temp = null;
                    if (cch.Id != 0) temp = db.FindCreditClaimHeader(cch.Id);
                    if (temp == null) temp = new CreditClaimHeader();

                    Mapper.Map<CreditClaimHeaderModel, CreditClaimHeader>(cch, temp);

                    db.InsertOrUpdateCreditClaimHeader(temp);
                    cch.Id = temp.Id;
                }
            }
            return error;
        }

        public void CleanCreditClaimTables() {
            db.CleanCreditClaimTables();
        }
            

        #region Private methods

        private Error validateModel(CreditClaimHeaderModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.NotesAndComments), 2048, "NotesAndComments", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.PickupAddress), 255, "PickupAddress", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.RejectionReasonComments), 250, "RejectionReasonComments", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ConNote), 30, "ConNote", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.PickupBoxDimensions), 100, "PickupBoxDimensions", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.PickupSpecialInstructions), 255, "PickupSpecialInstructions", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.CustomerContactEmail), 100, "CustomerContactEmail", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.CustomerContactPhone), 20, "CustomerContactPhone", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.CustomerReference), 30, "CustomerReference", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ReturnAddress), 255, "ReturnAddress", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.DeliveryAddress), 255, "DeliveryAddress", EvolutionResources.errTextDataRequiredInField);
            return error;
        }

        #endregion
    }
}
