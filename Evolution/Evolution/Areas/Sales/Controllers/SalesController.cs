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
using System.Globalization;

namespace Evolution.Areas.Sales.Controllers {
    public class SalesController : BaseController {

        #region Sales list

        // GET: Sales/Sales
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return Sales();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Sales() {
            var model = createModel();
            return View("Sales", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult SalesForCustomer(int custId) {
            var model = createModel();

            // Modify the browser cookies
            Response.Cookies.Remove("sales_ddlSalesPerson");
            Response.Cookies.Remove("sales_ddlSOStatus");
            Response.Cookies.Remove("sales:search");

            var cust = CustomerService.FindCustomerModel(custId, CurrentCompany, false);
            if(cust != null) Response.Cookies.Add(new HttpCookie("sales:search", cust.Name));

            return View("Sales", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrSales, 0, MenuOptionFlag.RequiresNoSale);
            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetSalesPersonList() {
            var model = createModel();
            var list = SalesService.FindSalesPersonListItemModel(model.CurrentCompany);
            list.Insert(0, new ListItemModel { Id = "0", Text = EvolutionResources.lblNone });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetRegionList() {
            var model = createModel();
            var list = LookupService.FindRegionsListItemModel(CurrentCompany, true);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCountryList() {
            var model = createModel();
            var list = LookupService.FindCountriesListItemModel();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetLocationList() {
            var model = createModel();
            var list = LookupService.FindLocationListItemModel(CurrentCompany);
            list.Insert(0, new ListItemModel { Id = "0", Text = EvolutionResources.lblNone });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetSOStatusList() {
            var model = createModel();
            var list = LookupService.FindSalesOrderHeaderStatusListItemModel();
            list.Insert(0, new ListItemModel { Id = "0", Text = EvolutionResources.lblNone });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetBrandCategoryList() {
            var model = createModel();
            var list = ProductService.FindBrandCategoryListItemModel(CurrentCompany);
            list.Insert(0, new ListItemModel { Id = "0", Text = EvolutionResources.lblNone });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetSales(int index, int pageNo, int pageSize, string search,
                                     int salesPerson, int region, int country, int location, int soStatus, int brandCategory,
                                     string sortColumn, int sortOrder) {
            var model = createModel();
            return Json(SalesService.FindSalesOrderHeadersListModel(model.CurrentCompany.Id, 
                                                                    index, 
                                                                    pageNo, 
                                                                    pageSize, 
                                                                    search,
                                                                    salesPerson,
                                                                    region,
                                                                    country,
                                                                    location,
                                                                    soStatus,
                                                                    brandCategory,
                                                                    sortColumn, 
                                                                    (SortOrder)sortOrder), 
                        JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Sale Editing

        #region Add/Edit Sale

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Add() {
            // Called when the user clicks 'Create' to create a new sale
            var editModel = new EditSalesOrderHeaderTempViewModel();

            var sale = SalesService.FindSalesOrderHeaderModel(0, CurrentCompany, true);
            sale.OrderDate = DateTimeOffset.Now;
            sale.SalespersonId = CurrentUser.Id;
            sale.BrandCategoryId = CurrentUser.DefaultBrandCategoryId;
            var freightCarrier = LookupService.FindFreightCarriersListItemModel(CurrentCompany)
                                           .Where(fc => fc.Text.ToLower() == "unspecified")
                                           .FirstOrDefault();
            if (freightCarrier != null) sale.FreightCarrierId = Convert.ToInt32(freightCarrier.Id);
            sale.ShipCountryId = CurrentCompany.DefaultCountryID;
            sale.NextActionId = LookupService.FindSaleNextActionId(SaleNextAction.None);

            // Copy the order into temp tables for editing
            editModel.SaleTemp = SalesService.CopySaleToTemp(CurrentCompany, sale, CurrentUser, false);

            return EditDetails(editModel.SaleTemp.Id);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Edit(int id) {
            // Called when a user clicks to edit a sale
            var model = new EditSalesOrderHeaderTempViewModel();

            var sale = SalesService.FindSalesOrderHeaderModel(id, CurrentCompany, true);

            // Copy the order into temp tables for editing
            model.SaleTemp = SalesService.CopySaleToTemp(CurrentCompany, sale, CurrentUser, false);
            prepareEditModel(model, model.SaleTemp.Id);

            // If any of the items on the order are partially or fully completed, then we 
            // disable ading and editing of items.
            // The ids of SalesOrderLineStatus' are the same as the enums.
            var items = SalesService.FindSalesOrderDetailTempsListModel(CurrentCompany.Id, model.SaleTemp.Id, 0);
            foreach (var item in items.Items) {
                if (item.LineStatusId == (int)SalesOrderLineStatus.SentForPicking ||
                   item.LineStatusId == (int)SalesOrderLineStatus.Complete) {
                    model.PartiallyComplete = true;
                    break;
                }
            }

            model.LGS = SalesService.LockSalesOrderHeader(sale);
            model.LGST = SalesService.LockSalesOrderHeaderTemp(model.SaleTemp);

            return View("Edit", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult EditDetails(int id) {
            // Called when a user is on one of the sales screens and clicks the sale details option
            var model = new EditSalesOrderHeaderTempViewModel();

            // Use the data in the temp tables
            var saleTemp = SalesService.FindSalesOrderHeaderTempModel(id, CurrentCompany, true);
            saleTemp.UserId = CurrentUser.Id;

            model.SaleTemp = saleTemp;
            prepareEditModel(model, id);

            var sale = SalesService.FindSalesOrderHeaderModel(saleTemp.OriginalRowId, CurrentCompany, true);

            // If any of the items on the order are partially or fully completed, then we 
            // disable adding and editing of items.
            // The ids of SalesOrderLineStatus' are the same as the enums.
            var items = SalesService.FindSalesOrderDetailTempsListModel(CurrentCompany.Id, saleTemp.Id, 0);
            foreach(var item in items.Items) {
                if(item.LineStatusId == (int)SalesOrderLineStatus.SentForPicking || 
                   item.LineStatusId == (int)SalesOrderLineStatus.Complete) {
                    model.PartiallyComplete = true;
                    break;
                }
            }

            model.LGS = SalesService.LockSalesOrderHeader(sale);
            model.LGST = SalesService.LockSalesOrderHeaderTemp(model.SaleTemp);

            return View("Edit", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult CopyOrder(int id) {
            // Called to copy an order
            var model = new EditSalesOrderHeaderTempViewModel();

            var sale = SalesService.FindSalesOrderHeaderModel(id, CurrentCompany, true);

            // Copy the order into temp tables for editing
            model.SaleTemp = SalesService.CopySaleToTemp(CurrentCompany, sale, CurrentUser, true);
            prepareEditModel(model, model.SaleTemp.Id);

            model.LGS = SalesService.LockSalesOrderHeader(sale);
            model.LGST = SalesService.LockSalesOrderHeaderTemp(model.SaleTemp);

            return View("Edit", model);
        }

        void prepareEditModel(EditSalesOrderHeaderTempViewModel model, int id) {
            var soht = SalesService.FindSalesOrderHeaderTempModel(id, CurrentCompany, false);

            PrepareViewModel(model, EvolutionResources.bnrAddEditSale + " - " + EvolutionResources.lblOrderNumber + ": " + model.SaleTemp.OrderNumber.ToString(), id, MakeMenuOptionFlags(0, 0, 0, soht.OriginalRowId) + MenuOptionFlag.RequiresNewSale);
            model.ParentId = id;

            model.LocationList = LookupService.FindLocationListItemModel(model.CurrentCompany);
            model.ShippingTemplateList = LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.Invoice);
            model.CountryList = LookupService.FindCountriesListItemModel();
            model.OrderTypeList = LookupService.FindLOVItemsListItemModel(model.CurrentCompany, LOVName.OrderType);
            model.BrandCategoryList = ProductService.FindBrandCategoryListItemModel(model.CurrentCompany);
            model.SOStatusList = LookupService.FindSalesOrderHeaderStatusListItemModel();
            model.UserList = MembershipManagementService.FindUserListItemModel();
            model.PaymentTermsList = LookupService.FindPaymentTermsListItemModel(model.CurrentCompany);
            if (model.SaleTemp.CustomerId != null) {
                model.CreditCardList = CustomerService.FindCreditCardsListItemModel(model.CurrentCompany, model.SaleTemp.CustomerId.Value);
            }

            model.FreightCarrierList = LookupService.FindFreightCarriersListItemModel(model.CurrentCompany);
            model.FreightTermList = LookupService.FindLOVItemsListItemModel(model.CurrentCompany, LOVName.FreightTerm);

            model.MethodSignedList = LookupService.FindMethodSignedListItemModel();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomers(string search) {
            return Json(CustomerService.FindCustomersListItemModel(CurrentCompany, search, 100), 
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetDeliveryWindowClose(string windowOpen) {
            DateTime dt = DateTime.ParseExact(windowOpen, CurrentUser.DateFormat, CultureInfo.InvariantCulture);

            var result = new JSONResultModel();
            result.Data = LookupService.GetDeliveryWindow(dt).ToString(CurrentUser.DateFormat);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomerAddresses(int id) {
            return Json(CustomerService.FindCustomerAddressesListModel(id),
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomerDetails(int id) {
            var result = new JSONResultModel {
                Data = CustomerService.FindCustomerModel(id, CurrentCompany, true)
            };
            if (result.Data == null) result.Error.SetError(EvolutionResources.errFailedToRetrieveCustomer, "SaleTemp_CustomerName");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCards(int id) {
            var result = new JSONResultModel {
                Data = CustomerService.FindCreditCardsListItemModel(CurrentCompany, id)
            };
            if (result.Data == null) result.Error.SetError(EvolutionResources.errFailedToRetrieveCustomerCards, "SaleTemp_CustomerName");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Sale details list

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetSalesOrderDetailTemps(int index, int parentId, int pageNo, int pageSize, string search,
                                                    string sortColumn, int sortOrder) {

            return Json(SalesService.FindSalesOrderDetailTempsListModel(CurrentCompany.Id, parentId, index, pageNo, pageSize, search,
                                                                       sortColumn, (SortOrder)sortOrder),
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult AddEditItem(int parentId, int id) {
            var model = new EditSalesOrderDetailTempViewModel();

            model.SalesOrderDetailTemp = SalesService.FindSalesOrderDetailTempModel(id, CurrentCompany, true);
            model.SalesOrderDetailTemp.SalesOrderHeaderTempId = parentId;

            // Copy the tax code from the customer record
            var customer = SalesService.FindCustomer(model.SalesOrderDetailTemp, CurrentCompany);
            if(customer != null) model.SalesOrderDetailTemp.TaxCodeId = customer.TaxCodeId;

            model.LGS = SalesService.LockSalesOrderDetailTemp(model.SalesOrderDetailTemp);

            return View("AddEditItem", model);
        }

        private void prepareEditModel(EditSalesOrderDetailTempViewModel model, int parentId) {
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetProducts(int brandCategoryId, string search) {
            var avail = LookupService.FindLOVItemByValueModel(LOVName.ProductAvailability, ((int)ProductAvailability.Live).ToString());
            return Json(ProductService.FindProductsForBrandCategoryListItemModel(brandCategoryId, (avail == null ? 0 : avail.Id), 0, 1, 100, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetUnitPrice(string pn, int custId, bool isOverrideMSQ) {
            // id is the string productNumber
            var result = new JSONResultModel();
            var product = ProductService.FindProductModel(pn, null, null, false);
            if (product != null) {
                var data = new PricingInfoModel();
                var prodPrice = ProductService.FindProductPrice(CurrentCompany, product.Id, custId);
                if(prodPrice != null) {
                    data.SellingPrice = prodPrice.SellingPrice;
                    data.MinSaleQty = (product.MinSaleQty == null || isOverrideMSQ ? 1 : product.MinSaleQty.Value);     // Was CustomField3 in MYOB
                    result.Data = data;
                } else {
                    result.Error.SetError(EvolutionResources.errProductPriceNotFound, "SalesOrderDetailTemp_UnitPriceExTax");
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        [ValidateAntiForgeryToken]
        public ActionResult DoAddItem(EditSalesOrderDetailTempViewModel model, string command) {
            if (command.ToLower() == "additem") {
                prepareEditModel(model, model.ParentId);
                if (ModelState.IsValid) {
                    // Add the item to the temp table
                    model.SalesOrderDetailTemp.CompanyId = CurrentCompany.Id;

                    var modelError = SalesService.InsertOrUpdateSalesOrderDetailTemp(model.SalesOrderDetailTemp, CurrentUser, model.LGS);
                    if (modelError.IsError) {
                        ModelState.AddModelError("SalesOrderDetailTemp_" + modelError.FieldName, modelError.Message);
                    }
                }
            }
            return View("AddEditItem", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetOrderSummary(int id) {
            var poht = SalesService.FindSalesOrderHeaderTempModel(id, CurrentCompany);
            var model = SalesService.CreateOrderSummary(poht);
            model.CurrencySymbol = LookupService.FindCurrencySymbol(CurrentCompany.DefaultCurrencyID);
            return View("OrderSummary", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Delete(int index, int id, int parentId) {
            var model = new SalesOrderDetailTempListModel();
            model.GridIndex = index;
            try {
                //SalesService.MarkSalesOrderHeaderTempAsChanged(parentId);

                SalesService.DeleteSalesOrderDetailTemp(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region MSQ Ovverride

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult OverrideMSQ(int id) {
            // Provides the override MSQ dialog
            var model = new OverrideMSQViewModel();
            model.SalesOrderHeaderTempId = id;
            model.UserList = MembershipManagementService.FindUsersInGroup("SuperUser");
            return View("OverrideMSQ", model);
        }

        #endregion

        #region Save Sale Order

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult SaveKeyProperties(int sohtId, int locationId, int brandcatgeoryId, 
                                              int? oaId, bool isOverrideMSQ, string lgst) {
            var result = new JSONResultModel();

            var soht = SalesService.FindSalesOrderHeaderTempModel(sohtId, CurrentCompany, false);
            if (soht != null) {
                soht.LocationId = locationId;
                soht.BrandCategoryId = brandcatgeoryId;
                soht.OverrideApproverId = oaId;
                soht.IsOverrideMSQ = isOverrideMSQ;
                result.Error = SalesService.InsertOrUpdateSalesOrderHeaderTemp(soht, CurrentUser, lgst, true);
                if (!result.Error.IsError) result.Data = SalesService.LockSalesOrderHeaderTemp(soht);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditSalesOrderHeaderTempViewModel model, string command) {
            switch (command.ToLower()) {
            case "save":
                // Save the screen data back to the temp tables, then copy to live tables and exit
                if (ModelState.IsValid) {
                    adjustDates(model.SaleTemp, model.TZ);

                    var modelError = new Error();

                    var items = SalesService.FindSalesOrderDetailTempsListModel(CurrentCompany.Id, model.SaleTemp.Id, 0, 1, 9999, "");
                    if (items.Items.Count() == 0) {
                        prepareEditModel(model, model.SaleTemp.Id);
                        model.SetErrorOnField(ErrorIcon.Error,
                                              EvolutionResources.errCantSaveSaleWithNoLines,
                                              "SaleTemp_EndUserName");
                        return View("Edit", model);

                    } else {
                        modelError = SalesService.InsertOrUpdateSalesOrderHeaderTemp(model.SaleTemp, CurrentUser, model.LGST, false);
                        if (modelError.IsError) {
                            prepareEditModel(model, model.SaleTemp.Id);
                            model.SetErrorOnField(ErrorIcon.Error,
                                                  modelError.Message,
                                                  "SaleTemp_" + modelError.FieldName);
                            return View("Edit", model);

                        } else {
                            // Copy the temp tables back to the main tables
                            modelError = SalesService.CopyTempToSalesOrderHeader(model.SaleTemp.Id, CurrentUser, model.LGS);
                            if (modelError.IsError) {
                                prepareEditModel(model, model.SaleTemp.Id);
                                model.SetErrorOnField(ErrorIcon.Error,
                                                      modelError.Message,
                                                      "SaleTemp_" + modelError.FieldName);
                            } else {
                                if(model.SaleTemp.OverrideApproverId != null) {
                                    // Send override approbal notification
                                    var sohm = SalesService.FindSalesOrderHeaderModel(modelError.Id, CurrentCompany, false);

                                    SalesService.SendMSQOverrideEMail(CurrentCompany,
                                                                      CurrentUser,
                                                                      MembershipManagementService.FindUserModel(model.SaleTemp.OverrideApproverId.Value),
                                                                      sohm);

                                    // Update the temp record so that we don't send a second approval email
                                    model.SaleTemp.OverrideApproverId = null;
                                    SalesService.InsertOrUpdateSalesOrderHeaderTemp(model.SaleTemp, CurrentUser,
                                                                                    SalesService.LockSalesOrderHeaderTemp(model.SaleTemp),
                                                                                    false);
                                }

                                return RedirectToAction("Sales");
                            }
                        }
                    }
                }
                prepareEditModel(model, model.SaleTemp.Id);
                return View("Edit", model);

            default:
                return RedirectToAction("Sales");
            }
        }

        #endregion

        #region Private methods

        private void adjustDates(SalesOrderHeaderTempModel model, string tz) {
            model.DeliveryWindowOpen = GetFieldValue(model.DeliveryWindowOpen, tz);
            model.DeliveryWindowClose = GetFieldValue(model.DeliveryWindowClose, tz);
            model.OARChangeDate = GetFieldValue(model.OARChangeDate, tz);
            model.OrderHoldExpiryDate = GetFieldValue(model.OrderHoldExpiryDate, tz);
            model.NextReviewDate = GetFieldValue(model.NextReviewDate, tz);
            model.OrderDate = GetFieldValue(model.OrderDate, tz);
            model.DateSigned = GetFieldValue(model.DateSigned, tz);
        }

        #endregion

        #endregion
    }
}
