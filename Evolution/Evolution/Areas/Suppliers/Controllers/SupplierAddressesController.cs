using Evolution.Enumerations;
using Evolution.Models.ViewModels;
using Evolution.SupplierService;
using Evolution.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using System.Web.Mvc;
using Evolution.Controllers.Application;
using Evolution.Resources;
using Evolution.Models.Models;

namespace Evolution.Areas.Suppliers.Controllers
{
    public class SupplierAddressesController : BaseController {
        // GET: Suppliers/SupplierAddresses
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Index()
        {
            return SupplierAddresses(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult SupplierAddresses(int id) {
            var model = createModel(id);
            return View("SupplierAddresses", model);
        }

        ViewModelBase createModel(int supplierId) {
            var model = new SupplierAddressesListViewModel();
            var supplier = SupplierService.FindSupplierModel(supplierId);

            PrepareViewModel(model,
                             EvolutionResources.bnrSupplierAddresses + (supplier == null ? "" : " - " + supplier.Name),
                             supplierId,
                             MenuOptionFlag.RequiresSupplier);
            model.ParentId = supplierId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetSupplierAddresses(int index, int supplierId, int pageNo, int pageSize, string search) {
            return Json(SupplierService.FindSupplierAddressesListModel(supplierId, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Edit(int id, int supplierId) {
            var model = new EditSupplierAddressViewModel();
            prepareEditModel(model, id, supplierId);

            model.SupplierAddress = SupplierService.FindSupplierAddressModel(id, supplierId, model.CurrentCompany);
            model.LGS = SupplierService.LockSupplierAddress(model.SupplierAddress);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        void prepareEditModel(EditSupplierAddressViewModel model, int id, int supplierId) {
            var supplier = SupplierService.FindSupplierModel(supplierId);

            string title = EvolutionResources.bnrAddEditSupplierAddress + (supplier == null ? "" : " - " + supplier.Name);
            if (id <= 0) title += " - " + EvolutionResources.lblNewAddress;

            PrepareViewModel(model, title, supplierId, MakeMenuOptionFlags(supplierId, 0));

            model.AddressTypeList = LookupService.FindLOVItemsListItemModel(null, LOVName.AddressType);
            model.CountryList = LookupService.FindCountriesListItemModel();
            model.ParentId = supplierId;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Delete(int index, int id) {
            var model = new SupplierAddressListModel();
            model.GridIndex = index;

            try {
                SupplierService.DeleteSupplierAddress(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Save(EditSupplierAddressViewModel model, string command) {
            if(command.ToLower() == "save") {
                var modelError = SupplierService.InsertOrUpdateSupplierAddress(model.SupplierAddress, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model, model.SupplierAddress.Id, model.SupplierAddress.SupplierId.Value);
                    model.SetErrorOnField(ErrorIcon.Error, modelError.Message, "SupplierAddress_" + modelError.FieldName);
                    return View("Edit", model);
                } else {
                    return RedirectToAction("SupplierAddresses", new { id = model.ParentId });
                }
            } else {
                return RedirectToAction("SupplierAddresses", new { id = model.ParentId });
            }
        }
    }
}