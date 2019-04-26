using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.TaskManagerService {
    public class TaskManagerService : CommonService.CommonService {

        #region Private members

        CompanyModel _company = null;

        #endregion

        #region Construction

        protected IMapper Mapper = null;

        public TaskManagerService(EvolutionEntities dbEntities, CompanyModel company = null) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<DAL.Task, TaskModel>();
                cfg.CreateMap<TaskModel, DAL.Task>();
                cfg.CreateMap<TaskModel, TaskModel>();
                cfg.CreateMap<Company, CompanyModel>();
            }));

            Mapper = config.CreateMapper();

            // Other initialisations
            if (company != null) {
                // Company supplied as a parameter
                _company = company;
            } else {
                // No company, so get the default 'head office' company
                var temp = db.FindParentCompany();
                if (temp != null) {
                    _company = Mapper.Map<Company, CompanyModel>(temp);
                } else {
                    throw new Exception("Error: No Parent Company is configured!");
                }
            }
        }

        public TaskListModel FindTaskListModel(CompanyModel company,
                                               UserModel user,
                                               int index,
                                               int pageNo,
                                               int pageSize,
                                               int period,
                                               int businessUnitId,
                                               int userId,
                                               int taskTypeId,
                                               int customerId,
                                               string search,
                                               int tz,
                                               bool bIncludeDeleted = false) {
            var model = new TaskListModel();

            var start = DateTimeOffset.Now.StartOfDay(tz);
            var end = start.EndOfDay(tz);

            if (period < 0) {
                start = start.AddDays(period);
            } else if (period > 0) {
                end = end.AddDays(period);
            }

            model.GridIndex = index;
            var allItems = db.FindTasks()
                             .Where(t => t.CompanyId == company.Id &&
                                         ((t.CreatedDate >= start || t.DueDate >= start) &&
                                          (t.CreatedDate <= end || t.DueDate <= end)) &&
                                         (businessUnitId == 0 || t.BusinessUnitId == businessUnitId) &&
                                         (userId == 0 || t.UserId == user.Id) &&
                                         (taskTypeId == 0 || t.TaskTypeId == taskTypeId) &&
                                         (bIncludeDeleted == true || t.Enabled == true) &&
                                         (customerId == 0 || t.CustomerId == customerId) &&
                                         ((string.IsNullOrEmpty(search) || (!string.IsNullOrEmpty(search) && (t.Description != null && t.Description.Contains(search) ||
                                                                                                              t.Customer != null && t.Customer.Name.Contains(search))))));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = mapToModel(item);
                model.Items.Add(newItem);
            }

            return model;
        }

        public TaskModel FindTaskModel(int id, CompanyModel company, UserModel user, bool bCreateIfNotFound) {
            TaskModel model = null;

            var t = db.FindTask(id);
            if (t == null) {
                if (bCreateIfNotFound) model = new TaskModel { CompanyId = company.Id, UserId = user.Id };
            } else {
                model = mapToModel(t);
            }
            return model;
        }

        TaskModel mapToModel(DAL.Task item) {
            var newItem = Mapper.Map<DAL.Task, TaskModel>(item);

            newItem.BusinessUnit = item.LOVItem_BusinessUnit.ItemText;
            newItem.AssigneeName = item.User_Assignee.Name.Replace(".", " ").WordCapitalise();
            newItem.TaskTypeText = item.LOVItem_TaskType.ItemText;
            if (item.User_CompletedBy != null) newItem.CompletedByName = item.User_CompletedBy.Name.Replace(".", " ").WordCapitalise();
            newItem.StatusText = item.LOVItem_Status.ItemText;
            newItem.StatusColour = item.LOVItem_Status.Colour;
            if (newItem.DueDate != null) {
                var eod = newItem.DueDate.Value.EndOfDay((int)item.CreatedDate.Offset.TotalMinutes);
                if(eod <= DateTimeOffset.Now &&
                   newItem.StatusText.ToLower() != "completed") newItem.StatusColour = "red";
            }
            if (item.Customer != null) {
                newItem.CustomerName = item.Customer.Name;
                newItem.CustomerUrl = "<a href=\"/Customers/Customers/Edit?id=" + item.CustomerId.ToString() + "\">" + item.Customer.Name + "</a>";
            } else {
                newItem.CustomerName = "";
                newItem.CustomerUrl = "";
            }
            return newItem;
        }

        public TaskModel MapToModel(TaskModel item) {
            var newItem = Mapper.Map<TaskModel, TaskModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateTask(TaskModel task, string lockGuid) {
            var error = validateTaskModel(task);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(DAL.Task).ToString(), task.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "Name");

                } else {
                    DAL.Task temp = null;
                    if (task.Id != 0) temp = db.FindTask(task.Id);
                    if (temp == null) temp = new DAL.Task();

                    Mapper.Map<TaskModel, DAL.Task>(task, temp);

                    db.InsertOrUpdateTask(temp);
                    task.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteTask(int id) {
            var task = db.FindTask(id);
            if (task != null) {
                task.Enabled = false;
                db.InsertOrUpdateTask(task);
            }
        }

        public string LockTask(TaskModel model) {
            return db.LockRecord(typeof(DAL.Task).ToString(), model.Id);
        }

        private Error validateTaskModel(TaskModel model) {
            if (model.CustomerId == 0) model.CustomerId = null;
            Error error = isValidRequiredString(getFieldValue(model.Title), 128, "Title", EvolutionResources.errSupplierNameMustBeEntered);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Description), 8192, "CancelMessage", EvolutionResources.errTextDataRequiredInField);

            return error;
        }

        #endregion

        #region Public members - sending Tasks

        public Error SendTask(MessageTemplateType msgId,
                                      TaskType notificationType,
                                      LOVItemModel businessArea,
                                      List<UserModel> recipients,
                                      int? customerId,
                                      Dictionary<string, string> dict) {
            var error = new Error();

            var messageTemplate = db.FindMessageTemplate(_company.Id, msgId);
            if (messageTemplate == null) {
                error.SetError(EvolutionResources.errEMailTemplateNotFound);
            } else {
                error = SendTask(messageTemplate.Subject,
                                 messageTemplate.Message,
                                 notificationType,
                                 businessArea,
                                 recipients,
                                 customerId,
                                 dict);
            }
            return error;
        }

        public Error SendTask(MessageTemplateType msgId,
                              TaskType notificationType,
                              LOVItemModel businessArea,
                              UserModel recipient,
                              int? customerId,
                              Dictionary<string, string> dict) {
            var error = new Error();

            var messageTemplate = db.FindMessageTemplate(_company.Id, msgId);
            if (messageTemplate == null) {
                error.SetError(EvolutionResources.errEMailTemplateNotFound);
            } else {
                error = SendTask(messageTemplate.Subject,
                                 messageTemplate.Message,
                                 notificationType,
                                 businessArea,
                                 recipient,
                                 customerId,
                                 dict);
            }
            return error;
        }

        public Error SendTask(string subject,
                              string message,
                              TaskType notificationType,
                              LOVItemModel businessArea,
                              List<UserModel> recipients,
                              int? customerId,
                              Dictionary<string, string> dict) {
            var error = new Error();

            foreach (var user in recipients) {
                var tempError = SendTask(subject,
                                         message,
                                         notificationType,
                                         businessArea,
                                         user,
                                         customerId,
                                         dict);

                // If an error occurs, we allow all further sending to continue, but we
                // report the first error as the return error
                if (tempError.IsError && !error.IsError) error = tempError;
            }
            return error;
        }

        public Error SendTask(string title,
                              string description,
                              TaskType taskType,
                              LOVItemModel businessArea,
                              UserModel recipient,
                              int? customerId,
                              Dictionary<string, string> dict) {
            var error = new Error();

            AddOrganisationDetails(dict);
            dict.AddProperty("MESSAGEHEADER", "");      // Hide these from
            dict.AddProperty("MESSAGEFOOTER", "");      // notifications

            var newTask = new DAL.Task {
                CompanyId = _company.Id,
                CreatedDate = DateTimeOffset.Now,
                LOVItem_TaskType = db.FindLOVItemByValue1(null, LOVName.TaskType, ((int)taskType).ToString()),
                BusinessUnitId = businessArea.Id,
                UserId = recipient.Id,
                Title = title.DoSubstitutions(dict),
                Description = description.DoSubstitutions(dict),
                LOVItem_Status = db.FindLOVItems(null, db.FindLOV(LOVName.TaskStatus).Id).FirstOrDefault(),
                CustomerId = customerId,
                Enabled = true
            };
            db.InsertOrUpdateTask(newTask);
            error.Id = newTask.Id;

            return error;
        }

        public void AddOrganisationDetails(Dictionary<string, string> dict) {
            var header = db.FindMessageTemplate(_company.Id, MessageTemplateType.EMailHeader);
            string templateText = (header == null ? "" : header.Message);
            dict.AddProperty("MESSAGEHEADER", templateText);

            var footer = db.FindMessageTemplate(_company.Id, MessageTemplateType.EMailFooter);
            templateText = (header == null ? "" : header.Message);
            dict.AddProperty("MESSAGEFOOTER", templateText);

            dict.AddProperty("COMPANYNAME", _company.CompanyName);
        }

        public void AddUserDetails(UserModel user, Dictionary<string, string> dict) {
            dict.AddProperty("USERNAME", user.FullName);
            dict.AddProperty("EMAIL", user.EMail);
        }

        public int GetTaskCount() {
            return db.FindTasks().Count();
        }

        #endregion
    }
}
