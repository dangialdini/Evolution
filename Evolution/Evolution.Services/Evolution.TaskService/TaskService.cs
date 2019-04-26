using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Resources;
using System.Diagnostics;
using AutoMapper;

namespace Evolution.TaskService {
    public class TaskService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public TaskService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<ScheduledTask, ScheduledTaskModel>();
                cfg.CreateMap<ScheduledTaskModel, ScheduledTask>()
                                .ForSourceMember(s => s.CurrentState, t => t.Ignore())
                                .ForSourceMember(s => s.LastRun, t => t.Ignore());
                cfg.CreateMap<ScheduledTaskLog, ScheduledTaskLogModel>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion

        #region Public members

        #region Task editing

        public ScheduledTaskListModel FindScheduledTasksListModel(int index, int pageNo, int pageSize, string search) {
            var model = new ScheduledTaskListModel();

            model.GridIndex = index;
            var allItems = db.FindScheduledTasks()
                             .Where(st => string.IsNullOrEmpty(search) ||
                                         (st.TaskName != null && st.TaskName.ToLower().Contains(search.ToLower())))
                             .ToList();

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                  .Take(pageSize)) {
                var newItem = MapToModel(item);
                model.Items.Add(newItem);
            }

            return model;
        }

        public ScheduledTaskModel FindScheduledTaskModel(string taskName, bool bCreateEmptyIfNotfound = true) {
            ScheduledTaskModel model = null;

            var st = db.FindScheduledTask(taskName);
            if (st == null) {
                if (bCreateEmptyIfNotfound) model = new ScheduledTaskModel();

            } else {
                model = MapToModel(st);
            }

            return model;
        }

        public ScheduledTaskModel FindScheduledTaskModel(int id, bool bCreateEmptyIfNotfound = true) {
            ScheduledTaskModel model = null;

            var st = db.FindScheduledTask(id);
            if (st == null) {
                if (bCreateEmptyIfNotfound) model = new ScheduledTaskModel();

            } else {
                model = MapToModel(st);
            }

            return model;
        }

        public ScheduledTaskModel MapToModel(ScheduledTask item) {
            var newItem = Mapper.Map<ScheduledTask, ScheduledTaskModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateScheduledTask(ScheduledTaskModel task, UserModel user, string lockGuid) {
            var error = validateModel(task);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(ScheduledTask).ToString(), task.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "TaskName");

                } else {
                    ScheduledTask temp = null;
                    if (task.Id != 0) temp = db.FindScheduledTask(task.Id);
                    if (temp == null) temp = new ScheduledTask();

                    // The following mapping does not copy:
                    //      CurrentState       // Preserve the database values for these fields as they could be running
                    //      LastRun
                    Mapper.Map<ScheduledTaskModel, ScheduledTask>(task, temp);

                    db.InsertOrUpdateScheduledTask(temp);
                    task.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteScheduledTask(ScheduledTaskModel model) {
            db.DeleteScheduledTask(model.Id);
        }

        public void DeleteScheduledTask(int id) {
            db.DeleteScheduledTask(id);
        }

        public string LockScheduledTask(ScheduledTaskModel model) {
            return db.LockRecord(typeof(ScheduledTask).ToString(), model.Id);
        }

        #endregion

        #region Task logs

        public ScheduledTaskLogListModel FindScheduledTaskLogListModel(int index, int taskId, int pageNo, int pageSize,
                                                                       string search, int severity,
                                                                       DateTimeOffset? dateFrom, DateTimeOffset? dateTo) {
            var model = new ScheduledTaskLogListModel();

            model.GridIndex = index;
            var allItems = db.FindScheduledTaskLogs(taskId, search, severity, dateFrom, dateTo)
                             .ToList();

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                         .Take(pageSize)) {
                var newItem = Mapper.Map<ScheduledTaskLog, ScheduledTaskLogModel>(item);

                newItem.SeverityText = Enum.GetName(typeof(LogSeverity), item.Severity);
                if (!string.IsNullOrEmpty(item.StackTrace)) newItem.Message += "<br/><br/>" + item.StackTrace;


                switch ((LogSeverity)item.Severity) {
                case LogSeverity.Warning:
                    newItem.Colour = "#0000FF";
                    break;
                case LogSeverity.Severe:
                    newItem.Colour = "#FF0000";
                    break;
                default:
                    newItem.Colour = "#000000";
                    break;
                }

                model.Items.Add(newItem);
            }

            return model;
        }

        #endregion

        #region Private methods

        private Error validateModel(ScheduledTaskModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.TaskName), 64, "TaskName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.CmdParameter), 255, "CmdParameter", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.CurrentState), 32, "CurrentState", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Parameters), 2048, "Parameters", EvolutionResources.errTextDataRequiredInField);
            return error;
        }

        #endregion

        #region Task running

        public Error RunTask(int id, bool bWaitForComplation = false) {
            // Used to run a task directly from the Scheduled Tasks screen
            var error = new Error();

            try {
                ScheduledTask task = db.FindScheduledTask(id);
                if (task.Enabled) {
                    // Start the task asynchronously
                    // TBD: Going forward, this will need to be modified to kick of SQL Jobs
                    Process process = new Process();
                    process.StartInfo.FileName = GetConfigurationSetting("ServiceExecutable", "");

                    process.StartInfo.Arguments = task.TaskName.Replace(" ", "");
                    if (!string.IsNullOrEmpty(task.CmdParameter)) process.StartInfo.Arguments = process.StartInfo.Arguments + " \"" + task.CmdParameter + "\"";

                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.Start();

                    if (bWaitForComplation) process.WaitForExit();

                    error.SetInfo(EvolutionResources.infTaskSuccessfullyQueued, "", task.TaskName);

                } else {
                    error.SetError(EvolutionResources.errCantStartTaskBecauseItIsDisabled, "", task.TaskName);
                }
            } catch(Exception e1) {
                error.SetError(e1);
            }

            return error;
        }

        public ScheduledTask StartTask(string taskName, string cmdParameter = "", bool bEnabled = true) {

            var task = db.FindScheduledTask(taskName, cmdParameter ?? "");
            if(task == null) {
                task = new ScheduledTask { TaskName = taskName,                                            
                                           CmdParameter = cmdParameter ?? "",
                                           CurrentState = TaskState.NotRunning,
                                           LastRun = DateTime.Now,
                                           Enabled = bEnabled };
                db.InsertOrUpdateScheduledTask(task);
            }

            if(task != null && task.Enabled) {
                task.LastRun = DateTimeOffset.Now;
                SetTaskStatus(task, TaskState.Running);

                WriteTaskLog(task, "Task Started");
            }
            return task;
        }

        public void EndTask(ScheduledTask task, int exitCode) {
            if (task != null) {
                WriteTaskLog(task, $"Task Completed with exit code {exitCode}");

                task.LastRun = DateTimeOffset.Now;
                SetTaskStatus(task, TaskState.NotRunning);
            }
        }

        public void EnableTask(ScheduledTaskModel task, bool bEnabled) {
            var tsk = db.FindScheduledTask(task.Id);
            if (tsk != null) EnableTask(tsk, bEnabled);
        }

        public void EnableTask(ScheduledTask task, bool bEnabled) {
            if (task != null) {
                if (bEnabled) {
                    WriteTaskLog(task, "Task Enabled");
                    task.Enabled = true;
                    SetTaskStatus(task, TaskState.NotRunning);
                } else {
                    WriteTaskLog(task, "Task Disabled");
                    task.Enabled = false;
                    SetTaskStatus(task, "Disabled");
                }
            }
        }

        public void SetTaskStatus(ScheduledTask task, string status) {
            task.CurrentState = status;
            db.InsertOrUpdateScheduledTask(task);
        }

        public ScheduledTaskLog WriteTaskLog(ScheduledTask task, string message, LogSeverity severity = LogSeverity.Normal) {
            ScheduledTaskLog log = new ScheduledTaskLog {
                ScheduledTask = task,
                Severity = (int)severity,
                LogDate = DateTime.Now,
                Message = message
            };
            db.InsertOrUpdateScheduledTaskLog(log);

            return log;
        }

        public ScheduledTaskLog FindTaskLog(int id) {
            return db.FindScheduledTaskLog(id);
        }

        #endregion

        #endregion
    }
}
