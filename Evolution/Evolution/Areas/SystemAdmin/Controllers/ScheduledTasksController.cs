using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.SystemAdmin.Controllers {
    public class ScheduledTasksController : BaseController {
        // GET: SystemAdmin/ScheduledTasks
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            return ScheduledTasks();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult ScheduledTasks() {
            var model = createModel();
            return View("ScheduledTasks", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrScheduledTasks);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetScheduledTasks(int index, int pageNo, int pageSize, string search) {
            return Json(TaskService.FindScheduledTasksListModel(index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditScheduledTaskViewModel();
            prepareEditModel(model);

            model.ScheduledTask = TaskService.FindScheduledTaskModel(id);
            model.LGS = TaskService.LockScheduledTask(model.ScheduledTask);

            return View(model);
        }

        void prepareEditModel(EditScheduledTaskViewModel model) {
            PrepareViewModel(model, EvolutionResources.bnrAddEditScheduledTask);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new ScheduledTaskListModel();
            model.GridIndex = index;
            try {
                TaskService.DeleteScheduledTask(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditScheduledTaskViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = TaskService.InsertOrUpdateScheduledTask(model.ScheduledTask, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditScheduledTask);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "ScheduledTask_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("ScheduledTasks");
                }

            } else {
                return RedirectToAction("ScheduledTasks");
            }
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult RunTask(int taskId) {
            var model = createModel();
            try {
                var error = TaskService.RunTask(taskId);
                if (error.IsError) {
                    model.SetError(ErrorIcon.Error, error.Message);
                } else {
                    model.SetError(ErrorIcon.Information, error.Message);
                }

            } catch (Exception e1) {
                model.Error.Icon = ErrorIcon.Error;
                model.Error.Message = "Error: " + e1.Message;
            }
            return View("ScheduledTasks", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult ViewLog(int taskId) {
            var model = new ViewModelBase();

            var task = TaskService.FindScheduledTaskModel(taskId);
            PrepareViewModel(model, EvolutionResources.bnrScheduledTasks + (task == null ? "" : " - " + task.TaskName));
            model.ParentId = taskId;

            return View("TaskLog", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetTaskLog(int index, int parentId, int pageNo, int pageSize, string search, int severity, 
                                       string dateFrom, string dateTo, int tz) {

            if (dateFrom.Trim().Length <= 10) dateFrom = dateFrom.Trim() + " 00:00:00";
            if (dateTo.Trim().Length <= 10) dateTo = dateTo.Trim() + " 23:59:59";

            return Json(TaskService.FindScheduledTaskLogListModel(index,
                                                                  parentId,
                                                                  pageNo,
                                                                  pageSize,
                                                                  search,
                                                                  severity,
                                                                  dateFrom.ParseDateTime(CurrentUser.DateFormat + " HH:mm:ss", tz),   //dtFromO, 
                                                                  dateTo.ParseDateTime(CurrentUser.DateFormat + " HH:mm:ss", tz)),    //dtToO), 
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetSectionList() {
            return Json(LookupService.FindLogSectionListItemModel(), JsonRequestBehavior.AllowGet);
        }
    }
}
