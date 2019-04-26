using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.CustomerService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Customers.Controllers {
    public class CustomerNotesController : BaseController {
        // GET: Customers/CustomerNotes
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return CustomerNotes(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult CustomerNotes(int id) {
            var model = createModel(id);
            return View("CustomerNotes", model);
        }

        ViewModelBase createModel(int customerId) {
            var model = new NoteListViewModel();
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            PrepareViewModel(model,
                             EvolutionResources.bnrCustomerNotes + (customer == null ? "" : " - " + customer.Name),
                             customerId,
                             MakeMenuOptionFlags(customerId, 0));
            model.ParentId = customerId;                      

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomerNotes(int index, int customerId, int pageNo, int pageSize, string search) {
            return Json(NoteService.FindNotesListModel(NoteType.Customer, 
                                                       customerId, 
                                                       index, 
                                                       pageNo, 
                                                       pageSize, 
                                                       search,
                                                       MediaSize.Medium,
                                                       640, 480), 
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Edit(int id, int customerId) {
            var model = new EditNoteViewModel();
            prepareEditModel(model, id, customerId);

            model.Note = NoteService.FindNoteModel(id, model.CurrentCompany, customerId, NoteType.Customer);
            model.LGS = NoteService.LockNote(model.Note);

            return View("EditNote", model);
        }

        void prepareEditModel(EditNoteViewModel model, int id, int customerId) {
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            string title = EvolutionResources.bnrAddEditCustomerNote + (customer == null ? "" : " - " + customer.Name);
            if (id <= 0) title += " - " + EvolutionResources.lblNewNote;

            PrepareViewModel(model, title, customerId, MakeMenuOptionFlags(customerId, 0));
            model.ParentId = customerId;

            model.MaxUploadFileSize = GetMaxFileUploadSize();
            model.ValidFileTypes = MediaServices.GetValidMediaTypes();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Delete(int index, int id) {
            var model = new NoteListModel();
            model.GridIndex = index;

            try {
                NoteService.DeleteNote(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditNoteViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = NoteService.InsertOrUpdateNote(model.Note, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model, model.CurrentCompany.Id, model.Note.ParentId);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "Note_" + modelError.FieldName);
                    return View("EditNote", model);
                }
            }
            return RedirectToAction("CustomerNotes", new { id = model.ParentId });
        }
    }
}
