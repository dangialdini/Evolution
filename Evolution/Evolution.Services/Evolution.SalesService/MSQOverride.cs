using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.SalesService {
    public partial class SalesService {
        public Error SendMSQOverrideEMail(CompanyModel company, 
                                          UserModel sender, 
                                          UserModel recipient,
                                          SalesOrderHeaderModel model) {
            var error = new Error();

            // Send a message to the selected user, indicating that they have been
            // nominated as an approver of an MSQ override
            var message = new EMailMessage(sender, recipient, MessageTemplateType.MSQChangeNotification);

            message.AddProperty("ORDERNO", model.OrderNumber);
            message.AddProperty("SALESPERSON", sender.FullName);
            message.AddProperty("URL", GetConfigurationSetting("SiteHttp", "") + "/Sales/Sales/Edit?id=" + model.Id.ToString());

            EMailService.EMailService emailService = new Evolution.EMailService.EMailService(db, company);
            error = emailService.SendEMail(message);

            if (!error.IsError) {
                var NotificationService = GetTaskManagerService(company);

                error = NotificationService.SendTask(Enumerations.MessageTemplateType.MSQChangeNotification,
                                                     TaskType.MSQChangeNotification,
                                                     LookupService.FindLOVItemsModel(company, LOVName.BusinessUnit)
                                                                  .Where(bu => bu.ItemText == "Sales")
                                                                  .FirstOrDefault(),
                                                     recipient,
                                                     model.CustomerId,
                                                     message.Dict);
            }
            return error;
        }
    }
}
