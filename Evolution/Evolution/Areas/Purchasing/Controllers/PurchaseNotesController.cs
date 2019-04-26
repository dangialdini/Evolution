using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.PurchasingService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Purchasing.Controllers { 
    public class PurchaseNotesController : BaseController {
        // GET: Purchasing/PurchaseNotes
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Index() {
            return PurchaseNotes(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult PurchaseNotes(int id) {     // The id is that of the Temp Table POH
            var model = createModel(id);
            return View("PurchaseNotes", model);
        }

        ViewModelBase createModel(int purchaseId) {     // Id of temp table POH
            var model = new NoteListViewModel();
            var poht = PurchasingService.FindPurchaseOrderHeaderTempModel(purchaseId, CurrentCompany);

            PrepareViewModel(model,
                             EvolutionResources.bnrPurchaseNotes + (poht == null ? "" : " - Order Number: " + poht.OrderNumber),
                             purchaseId,
                             MakeMenuOptionFlags(0, poht.OriginalRowId));
            model.ParentId = purchaseId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetPurchaseNotes(int index, int purchaseId, int pageNo, int pageSize, string search) {
            var tempPoh = PurchasingService.FindPurchaseOrderHeaderTempModel(purchaseId, CurrentCompany, false);
            return Json(NoteService.FindNotesListModel(NoteType.Purchase, 
                                                       (tempPoh == null ? 0 : tempPoh.OriginalRowId), 
                                                       index, 
                                                       pageNo, 
                                                       pageSize, 
                                                       search, 
                                                       MediaSize.Medium,
                                                       640, 480), 
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Edit(int id, int purchaseId) {
            var model = new EditNoteViewModel();
            prepareEditModel(model, id, purchaseId);

            model.Note = NoteService.FindNoteModel(id, model.CurrentCompany, purchaseId, NoteType.Purchase);
            model.LGS = NoteService.LockNote(model.Note);

            return View("EditNote", model);
        }

        void prepareEditModel(EditNoteViewModel model, int id, int purchaseId) {
            var poht = PurchasingService.FindPurchaseOrderHeaderTempModel(purchaseId, CurrentCompany);

            string title = EvolutionResources.bnrAddEditPurchaseNote + (poht == null ? "" : " - Order Number: " + poht.OrderNumber);
            if (id <= 0) title += " - " + EvolutionResources.lblNewNote;

            PrepareViewModel(model,
                             title,
                             purchaseId,
                             MakeMenuOptionFlags(0, poht.OriginalRowId));
            model.ParentId = purchaseId;

            model.MaxUploadFileSize = GetMaxFileUploadSize();
            model.ValidFileTypes = MediaServices.GetValidMediaTypes();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Delete(int index, int id) {
            var model = new PurchaseNoteListModel();
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
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Save(EditNoteViewModel model, string command) {
            if (command.ToLower() == "save") {
                var poht = PurchasingService.FindPurchaseOrderHeaderTempModel(model.ParentId, CurrentCompany);

                int tempParentId = model.Note.ParentId;
                model.Note.ParentId = poht.OriginalRowId;

                var modelError = NoteService.InsertOrUpdateNote(model.Note, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    model.Note.ParentId = tempParentId;
                    prepareEditModel(model, model.CurrentCompany.Id, model.Note.ParentId);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "Note_" + modelError.FieldName);
                    return View("EditNote", model);
                }
            }
            return RedirectToAction("PurchaseNotes", new { id = model.ParentId });
        }
    }
}
