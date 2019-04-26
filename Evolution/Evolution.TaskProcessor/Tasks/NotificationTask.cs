using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.TaskService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.TaskProcessor {
    public class NotificationTask : TaskBase {

        public NotificationTask(EvolutionEntities dbEntities) : base(dbEntities) { }

        public override string GetTaskName() { return TaskName.NotificationTask; }

        public override int DoProcessing(string[] args) {
            // When this task is run, it performs checks to see if events
            // have passed and sends notifications to the relevant users.

            // It is recommended that this task is only run once per day as it will resend
            // notifications if users haven't fixed the data issues which caused the notifications
            // to be sent.

            CheckForPassedUnpackDates();

            return 0;
        }

        public void CheckForPassedUnpackDates() {
            foreach (var company in CompanyService.FindCompaniesModel()) {
                CheckCompanyForPassedUnpackDates(company);
            }
        }

        public Error CheckCompanyForPassedUnpackDates(CompanyModel company) {
            var error = new Error();

            foreach (var undeliveredOrder in PurchasingService.FindUndeliveredPurchaseOrders(company).Items) {
                string errorMsg = "";
                var users = PurchasingService.FindOrderPurchasers(undeliveredOrder,
                                                                  company,
                                                                  undeliveredOrder.OrderNumber.Value,
                                                                  ref errorMsg);
                if (users == null) {
                    WriteTaskLog(errorMsg, LogSeverity.Severe);

                } else {
                    // Send an email
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.AddProperty("PURCHASEORDERNO", undeliveredOrder.OrderNumber.ToString());
                    dict.AddProperty("COMPANYNAME", company.FriendlyName);

                    var purchaser = MembershipManagementService.FindUserModel(undeliveredOrder.SalespersonId.Value);
                    dict.AddProperty("PURCHASER", (purchaser == null ? "" : purchaser.FullName));
                    dict.AddProperty("REALISTICETA", (undeliveredOrder.RealisticRequiredDate == null ? 
                                                      "" :
                                                      undeliveredOrder.RealisticRequiredDate.Value.ToString(company.DateFormat)));

                    var supplier = SupplierService.FindSupplierModel(undeliveredOrder.SupplierId.Value);
                    dict.AddProperty("SUPPLIER", supplier.Name);

                    string url = GetConfigSetting("SiteHttp", "");
                    url += "/Purchasing/Purchasing/Edit?id=" + undeliveredOrder.Id.ToString();
                    dict.AddProperty("URL", url);

                    error = SendMessage(company,
                                        MembershipManagementService.FindNoReplyMailSenderUser(),
                                        MessageTemplateType.UndeliveredPurchaseNotification,
                                        TaskType.Default,
                                        users,
                                        dict);
                }
            }
            return error;
        }
    }
}

