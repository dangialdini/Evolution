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

        public List<PaymentTermModel> FindPaymentTermsModel(int companyId) {
            List<PaymentTermModel> model = new List<PaymentTermModel>();
            foreach(var term in db.FindPaymentTerms(companyId)) {
                model.Add(MapToModel(term));
            }
            return model;
        }

        public List<ListItemModel> FindPaymentTermsListItemModel(CompanyModel company) {
            List<ListItemModel> model = new List<ListItemModel>();

            foreach (var pt in db.FindPaymentTerms(company.Id)) {
                var newModel = new ListItemModel {
                    Id = pt.Id.ToString(),
                    Text = buildPaymentTerm(pt),
                    ImageURL = ""
                };
                model.Add(newModel);
            }

            return model;
        }

        private string buildPaymentTerm(PaymentTerm pt) {
            string tmpTerms = "";

            switch (pt.TermsOfPaymentId.ToUpper()) {
            case "COD":
                tmpTerms = "Cash on Delivery";
                break;
            case "DMAE":
                tmpTerms = pt.BalanceDueDate.ToString() + " days after EOM";           // BDDate
                break;
            case "GND":
                tmpTerms = pt.BalanceDueDays.ToString() + " days from Invoice Date";   // BDDays
                break;
            case "NDAE":
                tmpTerms = pt.BalanceDueDays.ToString() + " days after EOM";           // BDDays
                break;
            case "PPD":
                tmpTerms = "Prepaid";
                break;
            case "N/A":
                tmpTerms = "Not applicable";
                break;
            default:
                tmpTerms = "ERROR";
                break;
            }

            return tmpTerms;
        }

        public PaymentTermListModel FindPaymentTermsListModel(int companyId, int index, int pageNo, int pageSize) {
            var model = new PaymentTermListModel();

            model.GridIndex = index;
            var allItems = db.FindPaymentTerms(companyId, true);

            model.TotalRecords = allItems.Count();
            foreach(var term in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(term));
            }

            return model;
        }

        public PaymentTermModel FindPaymentTermModel(int id, bool bCreateEmptyIfNotfound = true) {
            PaymentTermModel model = null;

            var pt = db.FindPaymentTerm(id);
            if (pt == null) {
                if(bCreateEmptyIfNotfound) model = new PaymentTermModel();

            } else {
                model = MapToModel(pt);
            }

            return model;
        }

        public PaymentTermModel MapToModel(PaymentTerm item) {
            var model = Mapper.Map<PaymentTerm, PaymentTermModel>(item);

            model.TermText = buildPaymentTerm(item);

            return model;
        }

        public Error InsertOrUpdatePaymentTerm(PaymentTermModel paymentTerm, UserModel user, string lockGuid) {
            var error = validateModel(paymentTerm);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(PaymentTerm).ToString(), paymentTerm.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "LatePaymentChargePercent");

                } else {
                    PaymentTerm temp = null;
                    if (paymentTerm.Id != 0) temp = db.FindPaymentTerm(paymentTerm.Id);
                    if (temp == null) temp = new PaymentTerm();

                    Mapper.Map<PaymentTermModel, PaymentTerm>(paymentTerm, temp);

                    db.InsertOrUpdatePaymentTerm(temp);
                    paymentTerm.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeletePaymentTerm(int id) {
            db.DeletePaymentTerm(id);
        }

        public string LockPaymentTerm(PaymentTermModel model) {
            return db.LockRecord(typeof(PaymentTerm).ToString(), model.Id);
        }

        public void CopyPaymentTerms(CompanyModel source, CompanyModel target, UserModel user) {
            foreach(var paymentTerm in FindPaymentTermsModel(source.Id)) {
                var newItem = Mapper.Map<PaymentTermModel, PaymentTermModel>(paymentTerm);
                newItem.Id = 0;
                newItem.CompanyId = target.Id;
                InsertOrUpdatePaymentTerm(newItem, user, "");
            }
        }

        #endregion

        #region Private methods

        private Error validateModel(PaymentTermModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.TermsOfPaymentId), 4, "TermsOfPaymentId", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.DiscountDate), 3, "DiscountDate", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.BalanceDueDate), 3, "BalanceDueDate", EvolutionResources.errTextDataRequiredInField);
            return error;
        }

        #endregion
    }
}
