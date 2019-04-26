using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;

namespace Evolution.Areas.Sales.Controllers {
    public class CancellationController : BaseController {
        // GET: Sales/Cancellation
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            var model = loadViewModel(0);
            saveViewModel(model);
            return View("Cancellation1", model);
        }

        private void saveViewModel(CancellationViewModel model) {
            Session["CVM"] = model;
        }

        private CancellationViewModel loadViewModel(int stepNo) {
            CancellationViewModel model = null;
            string title = "";

            if(stepNo == 0) {
                model = new CancellationViewModel();
            } else {
                try {
                    model = (CancellationViewModel)Session["CVM"];
                } catch {
                    model = new CancellationViewModel();
                }
            }

            switch (stepNo) {
            case 0:
            case 1:
                title = EvolutionResources.bnrSelectCriteria;
                stepNo = 1;
                model.Step1.ProductStatusList = LookupService.FindLOVItemsModel(null, LOVName.ProductStatus)
                                                             .Where(lovi => lovi.ItemValue1 != "0")
                                                             .Select(lovi => new ListItemModel { Id = lovi.ItemValue1, Text = lovi.ItemText } )
                                                             .ToList();
                while(model.Step1.ProductStatus.Count() < model.Step1.ProductStatusList.Count()) {
                    model.Step1.ProductStatus.Add(new ProductStatusValue());
                }
                model.Step1.BrandCategoryList = ProductService.FindBrandCategoryListItemModel(CurrentCompany);
                break;

            case 2:
                title = EvolutionResources.bnrSelectCustomers;
                model.Step2.CustomerList.AvailableItemsLabel = EvolutionResources.lblAvailableCustomers;
                model.Step2.CustomerList.SelectedItemsLabel = EvolutionResources.lblSelectedCustomers;
                break;

            case 3:
                title = EvolutionResources.bnrSelectOrders;
                model.Step3.OrderList.AvailableItemsLabel = EvolutionResources.lblAvailableOrders;
                model.Step3.OrderList.SelectedItemsLabel = EvolutionResources.lblSelectedOrders;
                break;

            case 4:
                title = EvolutionResources.bnrSelectProducts;
                model.Step4.ProductList.AvailableItemsLabel = EvolutionResources.lblAvailableProducts;
                model.Step4.ProductList.SelectedItemsLabel = EvolutionResources.lblSelectedProducts;
                break;
            case 5:
                title = EvolutionResources.bnrSelectWarehouse;
                model.Step5.WarehouseList.AvailableItemsLabel = EvolutionResources.lblAvailableWarehouses;
                model.Step5.WarehouseList.SelectedItemsLabel = EvolutionResources.lblSelectedWarehouses;
                break;
            case 6:
                title = EvolutionResources.bnrSelectAccountManagers;
                model.Step6.AccountManagerList.AvailableItemsLabel = EvolutionResources.lblAvailableAccountManagers;
                model.Step6.AccountManagerList.SelectedItemsLabel = EvolutionResources.lblSelectedAccountManagers;
                break;
            case 7:
                title = EvolutionResources.bnrSummaryConfirmation;
                break;
            }

            model.Menu.Menu1.Options.Clear();
            model.Menu.Menu2.Options.Clear();

            PrepareViewModel(model, EvolutionResources.bnrCancellations +
                             (string.IsNullOrEmpty(title) ? "" : " - " + title) +
                             " - Step " + stepNo.ToString() + " of 7",
                             0,
                             MenuOptionFlag.RequiresNoSale);
            return model;
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult SaveStep(CancellationViewModel model, string command) {
            int stepNo = 0;
            string[] selectedIds = null;
            List<ListItemModel> availItems;
            Error viewError = null;

            var temp = loadViewModel(1);

            switch (command.ToLower()) {
            case "nextstep2":
                // Move from step 1 (Parameters) to step 2 (Customer Selection)
                temp.Step1 = model.Step1;
                stepNo = 2;

                temp.Step2.CustomerList.SetAvailableItems(SalesService.FindCancellationCustomersListItemModel(CurrentCompany, temp));
                temp.Step2.CustomerList.ControlPrefix = "Step2_CustomerList_";
                break;

            case "nextstep3":
                // Move from step 2 (Customer selection) to step 3 (Order selection)
                selectedIds = null;
                try {
                    selectedIds = Request.Form["Step2.CustomerList.SelectedIds"].Split(',');
                } catch { }
                if(selectedIds != null) {
                    availItems = SalesService.FindCancellationCustomersListItemModel(CurrentCompany, temp);
                    temp.Step2.CustomerList.SelectedItems = new List<ListItemModel>();
                    foreach (var id in selectedIds) {
                        var item = availItems.Where(ai => ai.Id == id.ToString())
                                             .FirstOrDefault();
                        if (item != null) temp.Step2.CustomerList.SelectedItems.Add(item);
                    }
                }
                stepNo = 3;
                temp.Step3.OrderList.SetAvailableItems(SalesService.FindCancellationOrdersListItemModel(CurrentCompany, temp));
                temp.Step3.OrderList.ControlPrefix = "Step3_OrderList_";
                break;

            case "nextstep4":
                // Move from step 3 (Order selection) to step 4 (Product selection)
                selectedIds = null;
                try {
                    selectedIds = Request.Form["Step3.OrderList.SelectedIds"].Split(',');
                } catch { }
                if (selectedIds != null) {
                    availItems = SalesService.FindCancellationOrdersListItemModel(CurrentCompany, temp);
                    temp.Step3.OrderList.SelectedItems = new List<ListItemModel>();
                    foreach (var id in selectedIds) {
                        var item = availItems.Where(ai => ai.Id == id.ToString())
                                             .FirstOrDefault();
                        if (item != null) temp.Step3.OrderList.SelectedItems.Add(item);
                    }
                }
                stepNo = 4;
                temp.Step4.ProductList.SetAvailableItems(SalesService.FindCancellationProductListItemModel(CurrentCompany, temp));
                temp.Step4.ProductList.ControlPrefix = "Step4_ProductList_";
                break;

            case "nextstep5":
                // Move from step 4 (Product selection) to step 5 (Warehouse selection)
                selectedIds = null;
                try {
                    selectedIds = Request.Form["Step4.ProductList.SelectedIds"].Split(',');
                } catch { }
                if (selectedIds != null) {
                    availItems = SalesService.FindCancellationProductListItemModel(CurrentCompany, temp);
                    temp.Step4.ProductList.SelectedItems = new List<ListItemModel>();
                    foreach (var id in selectedIds) {
                        var item = availItems.Where(ai => ai.Id == id.ToString())
                                             .FirstOrDefault();
                        if (item != null) temp.Step4.ProductList.SelectedItems.Add(item);
                    }
                }
                stepNo = 5;
                temp.Step5.WarehouseList.SetAvailableItems(SalesService.FindCancellationWarehouseListItemModel(CurrentCompany, temp));
                temp.Step5.WarehouseList.ControlPrefix = "Step5_WarehouseList_";
                break;

            case "nextstep6":
                // Move from step 5 (Warehouse selection) to step 6 (Account Manager selection)
                selectedIds = null;
                try {
                    selectedIds = Request.Form["Step5.WarehouseList.SelectedIds"].Split(',');
                } catch { }
                if (selectedIds != null) {
                    availItems = SalesService.FindCancellationWarehouseListItemModel(CurrentCompany, temp);
                    temp.Step5.WarehouseList.SelectedItems = new List<ListItemModel>();
                    foreach (var id in selectedIds) {
                        var item = availItems.Where(ai => ai.Id == id.ToString())
                                             .FirstOrDefault();
                        if (item != null) temp.Step5.WarehouseList.SelectedItems.Add(item);
                    }
                }
                stepNo = 6;
                temp.Step6.AccountManagerList.SetAvailableItems(SalesService.FindCancellationAccountManagerListItemModel(CurrentCompany, temp));
                temp.Step6.AccountManagerList.ControlPrefix = "Step6_AccountManagerList_";
                break;

            case "nextstep7":
                // Move from step 6 (Account Manager selection) to step 7 (Cancellation confirmation)
                selectedIds = null;
                try {
                    selectedIds = Request.Form["Step6.AccountManagerList.SelectedIds"].Split(',');
                } catch { }
                if (selectedIds != null) {
                    availItems = SalesService.FindCancellationAccountManagerListItemModel(CurrentCompany, temp);
                    temp.Step6.AccountManagerList.SelectedItems = new List<ListItemModel>();
                    foreach (var id in selectedIds) {
                        var item = availItems.Where(ai => ai.Id == id.ToString())
                                             .FirstOrDefault();
                        if (item != null) temp.Step6.AccountManagerList.SelectedItems.Add(item);
                    }
                }
                stepNo = 7;
                break;

            case "nextstep8":
                // Do the cancellations
                viewError = new Error();
                var error = SalesService.DoCancellations(CurrentCompany, temp);
                if(error.IsError) {
                    // Stay on step 7
                    viewError.SetError(error.Message);
                    stepNo = 7;
                } else {
                    viewError.SetInfo(error.Message);

                    temp.Step2.CustomerList.SelectedItemList.Clear();
                    temp.Step2.CustomerList.SelectedItems.Clear();
                    stepNo = 1;
                }
                break;

            case "prevstep1":
                // Move from step 2 (Customer Selection) to step 1 (Parameters)
                temp.Step2.CustomerList.SelectedItemList.Clear();
                temp.Step2.CustomerList.SelectedItems.Clear();
                stepNo = 1;
                break;

            case "prevstep2":
                // Move from step 3 (Order Selection) to step 2 (Customer selection)
                temp.Step3.OrderList.SelectedItemList.Clear();
                temp.Step3.OrderList.SelectedItems.Clear();
                stepNo = 2;
                temp.Step2.CustomerList.SetAvailableItems(SalesService.FindCancellationCustomersListItemModel(CurrentCompany, temp));
                temp.Step2.CustomerList.SetSelectedItems(temp.Step2.CustomerList.SelectedItems.ToList());
                temp.Step2.CustomerList.ControlPrefix = "Step2_CustomerList_";
                break;

            case "prevstep3":
                // Move from step 4 (Product selection) to step 3 (Order Selection)
                temp.Step4.ProductList.SelectedItemList.Clear();
                temp.Step4.ProductList.SelectedItems.Clear();
                stepNo = 3;
                temp.Step3.OrderList.SetAvailableItems(SalesService.FindCancellationOrdersListItemModel(CurrentCompany, temp));
                temp.Step3.OrderList.SetSelectedItems(temp.Step3.OrderList.SelectedItems.ToList());
                temp.Step3.OrderList.ControlPrefix = "Step3_OrderList_";
                break;

            case "prevstep4":
                // Move from step 5 (Warehouse selection) to step 4 (Product selection)
                temp.Step5.WarehouseList.SelectedItemList.Clear();
                temp.Step5.WarehouseList.SelectedItems.Clear();
                stepNo = 4;
                temp.Step4.ProductList.SetAvailableItems(SalesService.FindCancellationProductListItemModel(CurrentCompany, temp));
                temp.Step4.ProductList.SetSelectedItems(temp.Step4.ProductList.SelectedItems.ToList());
                temp.Step4.ProductList.ControlPrefix = "Step4_ProductList_";
                break;

            case "prevstep5":
                // Move from step 6 (Account Manager selection) to step 5 (Warehouse selection)
                temp.Step6.AccountManagerList.SelectedItemList.Clear();
                temp.Step6.AccountManagerList.SelectedItems.Clear();
                stepNo = 5;
                temp.Step5.WarehouseList.SetAvailableItems(SalesService.FindCancellationWarehouseListItemModel(CurrentCompany, temp));
                temp.Step5.WarehouseList.SetSelectedItems(temp.Step5.WarehouseList.SelectedItems.ToList());
                temp.Step5.WarehouseList.ControlPrefix = "Step5_WarehouseList_";
                break;

            case "prevstep6":
                // Move from step 7 (Cancellation Confirmation) to step 6 (Account Manager selection)
                stepNo = 6;
                temp.Step6.AccountManagerList.SetAvailableItems(SalesService.FindCancellationAccountManagerListItemModel(CurrentCompany, temp));
                temp.Step6.AccountManagerList.SetSelectedItems(temp.Step6.AccountManagerList.SelectedItems.ToList());
                temp.Step6.AccountManagerList.ControlPrefix = "Step6_AccountManagerList_";
                temp.Error = new Error();   // Clear any errors
                break;
            }
            saveViewModel(temp);
            temp = loadViewModel(stepNo);

            if (viewError != null) temp.Error = viewError;

            return View("Cancellation" + stepNo.ToString(), temp);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCancellationSummaryList(int index) {
            var temp = loadViewModel(7);
            return Json(SalesService.FindCancellationSummaryListModel(CurrentCompany, temp, index),
                        JsonRequestBehavior.AllowGet);
        }
    }
}
