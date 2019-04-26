using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.SupplierService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Suppliers.Controllers
{
    public class SuppliersController : BaseController
    {
        // GET: Suppliers/Suppliers
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Index()
        {
            var model = createModel();
            return View("Suppliers", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Suppliers() {
            var model = createModel();
            return View("Suppliers", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrSuppliers, 0, MenuOptionFlag.RequiresNoSupplier);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetSuppliers(int index, int pageNo, int pageSize, string search,
                                         int country) {
            var model = createModel();
            return Json(SupplierService.FindSuppliersListModel(CurrentCompany,
                                                               index, 
                                                               pageNo, 
                                                               pageSize, 
                                                               search,
                                                               country), 
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetCountryList() {
            var model = createModel();
            var list = LookupService.FindCountriesListItemModel();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Edit(int id) {
            var model = new EditSupplierViewModel();
            prepareEditModel(model, id);

            model.Supplier = SupplierService.FindSupplierModel(id);
            model.SupplierAddress = SupplierService.FindSupplierAddressModel(id);
            model.LGS = SupplierService.LockSupplier(model.Supplier);

            return View(model);
        }

        void prepareEditModel(EditSupplierViewModel model, int id) {
            PrepareViewModel(model, EvolutionResources.bnrAddEditSupplier, id, MenuOptionFlag.RequiresSupplier);

            model.CountryList = LookupService.FindCountriesListItemModel();
            model.CurrencyList = LookupService.FindCurrenciesListItemModel();
            model.TaxCodeList = LookupService.FindTaxCodesListItemModel(model.CurrentCompany);
            model.SupplierTermList = LookupService.FindSupplierTermsListItemModel(model.CurrentCompany);
            model.CommercialTermList = LookupService.FindLOVItemsListItemModel(model.CurrentCompany, LOVName.CommercialTerms);
            //model.FreightForwarderList = LookupService.FindFreightForwardersListItemModel(model.CurrentCompany);
            model.PortList = LookupService.FindPortsListItemModel();
            model.ShipMethodList = LookupService.FindLOVItemsListItemModel(model.CurrentCompany, LOVName.ShippingMethod);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Delete(int index, int id) {
            var model = new SupplierListModel();
            model.GridIndex = index;
            try {
                var error = SupplierService.DeleteSupplier(id);
                model.Error.SetError(error.Message);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Save(EditSupplierViewModel model, string command) {
            if (command.ToLower() == "save") {
                // Validate everything before saving - we don't want to save a supplier and
                // then find we can't save their address!
                var modelError = SupplierService.ValidateSupplierModel(model.Supplier);
                if (modelError.IsError) {
                    prepareEditModel(model, model.Supplier.Id);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "Supplier_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    modelError = SupplierService.ValidateAddressModel(model.SupplierAddress);
                    if (modelError.IsError) {
                        prepareEditModel(model, model.Supplier.Id);
                        model.SetErrorOnField(ErrorIcon.Error,
                                              modelError.Message,
                                              "SupplierAddress_" + modelError.FieldName);
                        return View("Edit", model);

                    } else {
                        modelError = SupplierService.InsertOrUpdateSupplier(model.Supplier, CurrentUser, model.LGS);
                        if (modelError.IsError) {
                            prepareEditModel(model, model.Supplier.Id);
                            model.SetErrorOnField(ErrorIcon.Error,
                                                  modelError.Message,
                                                  "Supplier_" + modelError.FieldName);
                            return View("Edit", model);

                        } else {
                            model.SupplierAddress.SupplierId = model.Supplier.Id;
                            modelError = SupplierService.InsertOrUpdateSupplierAddress(model.SupplierAddress, CurrentUser);
                            if (modelError.IsError) {
                                prepareEditModel(model, model.Supplier.Id);
                                model.SetErrorOnField(ErrorIcon.Error,
                                                      modelError.Message,
                                                      "SupplierAddress_" + modelError.FieldName);
                                return View("Edit", model);

                            } else {
                                return RedirectToAction("Suppliers");
                            }
                        }
                    }
                }

            } else {
                return RedirectToAction("Suppliers");
            }
        }
    }
}