using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.CompanyAdmin.Controllers {
    public class MessageTemplatesController : BaseController {
        // GET: CompanyAdmin/MessageTemplates
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            return MessageTemplates();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult MessageTemplates() {
            var model = createModel();
            return View("MessageTemplates", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrMessageTemplates);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetMessageTemplates(int index, int pageNo, int pageSize, string search) {
            var model = createModel();

            var templateList = LookupService.FindMessageTemplatesListModel(model.CurrentCompany.Id, index, pageNo, pageSize, search);

            Dictionary<string, string> dict = new Dictionary<string, string>();
            EMailService.AddOrganisationDetails(dict);
            EMailService.AddUserDetails(CurrentUser, dict);

            foreach(var template in templateList.Items) {
                template.Subject = template.Subject.DoSubstitutions(dict);
                template.Message = template.Message.DoSubstitutions(dict);
            }

            return Json(templateList, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditMessageTemplateViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditMessageTemplate);

            model.MessageTemplate = LookupService.FindMessageTemplateModel(id);
            model.MessageTemplate.CompanyId = model.CurrentCompany.Id;
            model.LGS = LookupService.LockMessageTemplate(model.MessageTemplate);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new MessageTemplateListModel();
            model.GridIndex = index;
            try {
                LookupService.DeleteMessageTemplate(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        [ValidateInput(false)]
        public ActionResult Save(EditMessageTemplateViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdateMessageTemplate(model.MessageTemplate, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditMessageTemplate);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "MessageTemplate_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("MessageTemplates");
                }

            } else {
                return RedirectToAction("MessageTemplates");
            }
        }
    }
}
