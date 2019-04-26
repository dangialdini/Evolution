using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.ViewModels;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.AuditService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.SalesService {
    public partial class SalesService {

        public bool IsCreditClaimReplacementOrder(SalesOrderHeaderModel soh) {
            var temp = db.FindCreditClaimReplacementOrderForSoh(soh.CompanyId, soh.Id);
            return temp != null;
        }

        public Error InsertOrUpdateCreditClaimReplacementOrder(CreditClaimReplacementOrderModel ccro, string lockGuid) {
            var error = new Error();
            error = validateModel(ccro);
            if (!error.IsError) {
                // Check that the lock is still current
                if(!db.IsLockStillValid(typeof(CreditClaimReplacementOrder).ToString(), ccro.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "");
                } else {
                    CreditClaimReplacementOrder temp = null;
                    if (ccro.Id != 0) temp = db.FindCreditClaimReplacementOrder(ccro.Id);
                    if (temp == null) temp = new CreditClaimReplacementOrder();

                    Mapper.Map<CreditClaimReplacementOrderModel, CreditClaimReplacementOrder>(ccro, temp);

                    db.InsertOrUpdateCreditClaimReplacementOrder(temp);
                    ccro.Id = temp.Id;
                }
            }
            return error;
        }

        public CreditClaimReplacementOrderModel MapToModel(CreditClaimReplacementOrder ccro) {
            var newItem = Mapper.Map<CreditClaimReplacementOrder, CreditClaimReplacementOrderModel>(ccro);
            return newItem;
        }

        #region Private methods

        private Error validateModel(CreditClaimReplacementOrderModel model) {
            return new Error();
        }

        #endregion
    }
}
