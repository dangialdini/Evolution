using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;

namespace Evolution.Controllers.Application {
    [Authorize]
    public class HomeController : BaseController {

        #region Dashboard

        #region Main Dashboard

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult Index() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrHome);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult SelectCompany(int id) {
            int errorCode = 0;
            MembershipManagementService.SaveProperty(MMSProperty.CurrentCompany, id);
            return Json(errorCode, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Task List

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult GetTasks(int index, int pageNo, int pageSize, 
                                     int period, int businessUnit, int user, int taskType,
                                     int custid, string search, int tz) {

            return Json(TaskManagerService.FindTaskListModel(CurrentCompany,
                                                             CurrentUser, 
                                                             index, 
                                                             pageNo, 
                                                             pageSize, 
                                                             period,
                                                             businessUnit,
                                                             user,
                                                             taskType,
                                                             custid,
                                                             search,
                                                             tz,
                                                             false),
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult GetBusinessUnitList() {
            var items = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.BusinessUnit);
            items.Insert(0, new ListItemModel(EvolutionResources.strAll, "0"));
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult GetPeriodList() {
            var items = new List<ListItemModel>();
            items.Add(new ListItemModel(EvolutionResources.strAll, "0"));

            foreach (var item in LookupService.FindLOVItemsModel(null, LOVName.TaskListPeriod)) {
                items.Add(new ListItemModel(item.ItemText, item.ItemValue1));
            }
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult GetUserList() {
            var items = MembershipManagementService.FindUserListItemModel();
            items.Insert(0, new ListItemModel(EvolutionResources.strAll, "0"));
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult GetTaskTypeList() {
            var items = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.TaskType);
            items.Insert(0, new ListItemModel(EvolutionResources.strAll, "0"));
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult GetTaskStatusList() {
            var items = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.TaskStatus);
            items.Insert(0, new ListItemModel(EvolutionResources.strAll, "0"));
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult GetCustomerList() {
            var items = CustomerService.FindCustomersListItemModel(CurrentCompany, "");
            items.Insert(0, new ListItemModel(EvolutionResources.strAll, "0"));
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult AddEditTask(int id) {
            var model = new EditTaskViewModel();

            model.Task = TaskManagerService.FindTaskModel(id, CurrentCompany, CurrentUser, true);
            prepareEditModel(model);
            model.LGS = TaskManagerService.LockTask(model.Task);

            return View("AddEditTask", model);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        [ValidateInput(false)]
        public ActionResult DoAddTask(EditTaskViewModel model, string command) {
            prepareEditModel(model);
            if (command == "additem" || command == "complete") {
                if (ModelState.IsValid) {
                    if(command == "complete") {
                        model.Task.CompletedById = CurrentUser.Id;
                        model.Task.CompletionDate = DateTimeOffset.Now;

                        var status = LookupService.FindLOVItemsModel(CurrentCompany, LOVName.TaskStatus)
                                                  .Where(s => s.ItemText == "Completed" || s.ItemText == "Actioned" || s.ItemText == "Completed/Actioned")
                                                  .FirstOrDefault();
                        if(status != null) model.Task.StatusId = status.Id;
                    }
                    var error = TaskManagerService.InsertOrUpdateTask(model.Task, model.LGS);
                    if (error.IsError) {
                        ModelState.AddModelError("Task_" + error.FieldName, error.Message);

                    } else if (model.Followup.AddFollowup) {
                        var followupTask = TaskManagerService.MapToModel(model.Task);
                        followupTask.Id = 0;
                        followupTask.CreatedDate = DateTime.Now;
                        followupTask.DueDate = model.Followup.FollowupDate;
                        followupTask.Title = ("Follow-up: " + model.Task.Title).ShrinkString(128);
                        followupTask.Description = model.Followup.FollowupNote;

                        error = TaskManagerService.InsertOrUpdateTask(followupTask, "");
                        if (error.IsError) {
                            ModelState.AddModelError("Task_" + error.FieldName, error.Message);
                        }
                    }
                }
            }
            return View("AddEditTask", model);
        }

        private void prepareEditModel(EditTaskViewModel model) {
            model.BusinessUnitList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.BusinessUnit);
            model.TaskTypeList = LookupService.FindLOVItemsListItemModel(null, LOVName.TaskType);
            model.StatusList = LookupService.FindLOVItemsListItemModel(null, LOVName.TaskStatus);
            model.UserList = MembershipManagementService.FindUserListItemModel();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomers(string search) {
            return Json(CustomerService.FindCustomersListItemModel(CurrentCompany, search, 50), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult Delete(int index, int id) {
            var model = new TaskListModel();
            model.GridIndex = index;
            try {
                TaskManagerService.DeleteTask(id);
            } catch (Exception e1) {
                model.Error.Icon = ErrorIcon.Error;
                model.Error.Message = "Error: " + e1.Message;
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Sales Panel

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult GetSaleSummary(int index, int pageNo, int pageSize, string search) {
            return Json(SalesService.FindSalesOrderHeaderSummaryListModel(CurrentCompany, CurrentUser, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Purchases panel

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult GetPurchaseSummary(int index, int pageNo, int pageSize, string search) {
            return Json(PurchasingService.FindPurchaseOrderHeaderSummaryListModel(CurrentCompany, CurrentUser, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion
    }
}
