using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.SalesService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Sales.Controllers {
    public class SaleNotesController : BaseController {
        // GET: Sales/SaleNotes
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index(int id = 0) {
            return SaleNotes(id);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult SaleNotes(int id) {     // The id is that of the Temp Table POH
            var model = createModel(id);
            return View("SaleNotes", model);
        }

        ViewModelBase createModel(int saleId) {     // Id of temp table POH
            var model = new NoteListViewModel();
            var soht = SalesService.FindSalesOrderHeaderTempModel(saleId, CurrentCompany);

            PrepareViewModel(model,
                             EvolutionResources.bnrSaleNotes + (soht == null ? "" : " - Order Number: " + soht.OrderNumber),
                             saleId,
                             MakeMenuOptionFlags(0, 0, 0, soht.OriginalRowId));
            model.ParentId = saleId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetSaleNotes(int index, int saleId, int pageNo, int pageSize, string search) {
            var tempPoh = SalesService.FindSalesOrderHeaderTempModel(saleId, CurrentCompany, false);
            return Json(NoteService.FindNotesListModel(NoteType.Sale,
                                                       (tempPoh == null ? 0 : tempPoh.OriginalRowId),
                                                       index,
                                                       pageNo,
                                                       pageSize,
                                                       search,
                                                       MediaSize.Medium,
                                                       640, 480),
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Edit(int id, int saleId) {
            var model = new EditNoteViewModel();
            prepareEditModel(model, id, saleId);

            model.Note = NoteService.FindNoteModel(id, model.CurrentCompany, saleId, NoteType.Sale);
            model.LGS = NoteService.LockNote(model.Note);

            return View("EditNote", model);
        }

        void prepareEditModel(EditNoteViewModel model, int id, int saleId) {
            var soht = SalesService.FindSalesOrderHeaderTempModel(saleId, CurrentCompany);

            string title = EvolutionResources.bnrAddEditSaleNote + (soht == null ? "" : " - Order Number: " + soht.OrderNumber);
            if (id <= 0) title += " - " + EvolutionResources.lblNewNote;

            PrepareViewModel(model,
                             title,
                             saleId,
                             MakeMenuOptionFlags(0, 0, 0, soht.OriginalRowId));
            model.ParentId = saleId;

            model.MaxUploadFileSize = GetMaxFileUploadSize();
            model.ValidFileTypes = MediaServices.GetValidMediaTypes();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Delete(int index, int id) {
            var model = new SaleNoteListModel();
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
                var soht = SalesService.FindSalesOrderHeaderTempModel(model.ParentId, CurrentCompany);

                int tempParentId = model.Note.ParentId;
                model.Note.ParentId = soht.OriginalRowId;

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
            return RedirectToAction("SaleNotes", new { id = model.ParentId });
        }
    }
}
