using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Evolution.DAL;
using Evolution.Enumerations;
using Evolution.TaskService;
using Evolution.Models.Models;

namespace Evolution.TaskProcessor {
    public class TaskBase {

        #region Private Members

        protected EvolutionEntities _db;

        private List<string> taskParams = new List<string>();
        private bool bParamsChanged = false;

        private ScheduledTask _task = null;
        protected ScheduledTask Task {
            get {
                return _task;
            }
        }

        #endregion

        #region Protected members

        private TaskService.TaskService _taskService = null;
        protected TaskService.TaskService TaskService { get {
                if (_taskService == null) _taskService = new TaskService.TaskService(_db);
                return _taskService;
            }
        }

        private PurchasingService.PurchasingService _purchasingService = null;
        protected PurchasingService.PurchasingService PurchasingService {
            get {
                if (_purchasingService == null) _purchasingService = new PurchasingService.PurchasingService(_db);
                return _purchasingService;
            }
        }

        private NoteService.NoteService _noteService = null;
        protected NoteService.NoteService NoteService {
            get {
                if (_noteService == null) _noteService = new NoteService.NoteService(_db);
                return _noteService;
            }
        }

        private MembershipManagementService.MembershipManagementService _mms = null;
        protected MembershipManagementService.MembershipManagementService MembershipManagementService {
            get {
                if (_mms == null) _mms = new MembershipManagementService.MembershipManagementService(_db);
                return _mms;
            }
        }

        private ProductService.ProductService _productService = null;
        protected ProductService.ProductService ProductService {
            get {
                if (_productService == null) _productService = new ProductService.ProductService(_db);
                return _productService;
            }
        }

        private MediaService.MediaService _mediaService = null;
        protected MediaService.MediaService MediaService {
            get {
                if (_mediaService == null) _mediaService = new MediaService.MediaService(_db);
                return _mediaService;
            }
        }

        private CompanyService.CompanyService _companyService = null;
        protected CompanyService.CompanyService CompanyService {
            get {
                if (_companyService == null) _companyService = new CompanyService.CompanyService(_db);
                return _companyService;
            }
        }

        private SupplierService.SupplierService _supplierService = null;
        protected SupplierService.SupplierService SupplierService {
            get {
                if (_supplierService == null) _supplierService = new SupplierService.SupplierService(_db);
                return _supplierService;
            }
        }

        #endregion

        #region Construction

        public TaskBase(EvolutionEntities db) {
            _db = db;
            _db.Database.CommandTimeout = 900;      // 15 minutes
        }

        #endregion

        #region Task Running

        public void Run(string[] args) {
            // Start the task
            StartTask(args != null && args.Length > 1 ? args[1] : null);            
            if (_task.Enabled) {
                // Read the parameters
                if(!string.IsNullOrEmpty(_task.Parameters)) {
                    taskParams = _task.Parameters.Replace("\r", "").Split('\n').ToList();
                }

                // Do the task actions
                int rc = DoProcessing(args);

                // Save the parameters if changed
                if(bParamsChanged) {
                    string paramList = "";
                    foreach (var paramLine in taskParams) paramList += paramLine + "\r\n";
                    _task.Parameters = paramList;
                }

                // End the task
                EndTask(rc);
            }
        }

        public ScheduledTask StartTask(string param = "") {
            string taskName = GetTaskName();
            if (taskName == null) {
                WriteLog("Error: TaskBase.GetTaskName() has not been overridden - task has no name!");

            } else {
                _task = TaskService.StartTask(taskName, param);    // Creates task if it doesn't exist
            }
            return _task;
        }

        public void EndTask(int rc) {
            TaskService.EndTask(_task, rc);
        }

        public virtual string GetTaskName() { return null; }
        public virtual int DoProcessing(string[] args) { return 0; }

        #endregion

        #region Task parameters

        protected string GetTaskParameter(string keyName, string defaultValue) {
            bool bFound = false;
            string rc = defaultValue;

            if (taskParams == null) {
                SetTaskParameter(keyName, rc);
            } else {
                foreach (var paramLine in taskParams) {
                    if (paramLine.Trim().ToLower().IndexOf(keyName.ToLower() + "=") == 0) {
                        rc = paramLine;
                        int pos = rc.IndexOf("=");
                        if (pos != -1) rc = rc.Substring(pos + 1).Trim();
                        bFound = true;
                        break;
                    }
                }
                if(!bFound) SetTaskParameter(keyName, rc);
            }
            return rc;
        }

        protected int GetTaskParameter(string keyName, int defaultValue) {
            string temp = GetTaskParameter(keyName, defaultValue.ToString());

            int rc;
            try {
                rc = Convert.ToInt32(temp);
            } catch {
                rc = defaultValue;
            }

            return rc;
        }

        protected DateTime GetTaskParameter(string keyName, DateTime defaultValue) {
            string temp = GetTaskParameter(keyName, defaultValue.ToString());

            DateTime rc;
            try {
                rc = Convert.ToDateTime(temp);
            } catch {
                rc = defaultValue;
            }

            return rc;
        }

        protected UserModel GetTaskUser() {
            string userId = "taskuser.taskuser";
            var user = GetTaskParameter("TaskUser", userId);
            var taskUser = _db.FindUser(user);
            if(taskUser == null) {
                WriteTaskLog($"Error: Failed to find user '{userId}' to run service with!", LogSeverity.Severe);
                return null;
            } else {
                return new UserModel {
                    Id = taskUser.Id,
                    Name = taskUser.Name,
                    FirstName = taskUser.FirstName,
                    LastName = taskUser.LastName,
                    EMail = taskUser.EMail,
                    DefaultBrandCategoryId = taskUser.DefaultBrandCategoryId,
                    DefaultCompanyId = taskUser.DefaultCompanyId,
                    DateFormat = taskUser.DateFormat,
                    Enabled = taskUser.Enabled
                };
            }
        }

        protected void SetTaskParameter(string keyName, string keyValue) {

            bool bFound = false;

            if (taskParams == null) taskParams = new List<string>();

            for (int i = 0; i < taskParams.Count() && !bFound; i++) {
                if (taskParams[i].Trim().ToLower().IndexOf(keyName.ToLower() + "=") == 0) {

                    taskParams[i] = keyName + "=" + keyValue;
                    bFound = true;
                }
            }

            if (!bFound) taskParams.Add(keyName + "=" + keyValue);

            bParamsChanged = true;
        }

        protected void SetTaskParameter(string keyName, int keyValue) {
            SetTaskParameter(keyName, keyValue.ToString());
        }

        protected void SetTaskParameter(string keyName, DateTime keyValue) {
            SetTaskParameter(keyName, keyValue.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        #endregion

        #region app.Config settings

        protected string GetConfigSetting(string keyName, string defaultValue) {
            string rc;

            AppSettingsReader reader = new AppSettingsReader();
            try {
                rc = (String)reader.GetValue(keyName, defaultValue.GetType());
            } catch {
                rc = defaultValue;
            }

            return rc;
        }

        protected int GetConfigSetting(string keyName, int defaultValue) {
            int rc;

            AppSettingsReader reader = new AppSettingsReader();
            try {
                rc = (int)reader.GetValue(keyName, defaultValue.GetType());
            } catch {
                rc = defaultValue;
            }

            return rc;
        }

        #endregion

        #region Helpers

        public void WriteLog(string message, LogSeverity severity = LogSeverity.Normal, LogSection logSection = LogSection.SystemError) {
            Console.WriteLine(message);
            _db.WriteLog(logSection, severity, message);
        }

        public void WriteTaskLog(string message, LogSeverity severity = LogSeverity.Normal) {
            Console.WriteLine(message);
            TaskService.WriteTaskLog(_task, message, severity);
        }

        #endregion

        #region EMailing/Notifications

        protected Error SendMessage(CompanyModel company,
                                         UserModel sender, 
                                         MessageTemplateType templateId,
                                         TaskType notificationType,
                                         List<UserModel> recipients,
                                         Dictionary<string, string> dict,
                                         List<string> attachments = null) {

            // Send as email
            var message = new EMailMessage(sender, templateId);
            message.AddRecipients(recipients);
            message.AddProperties(dict);
            message.AddAttachments(attachments);


            EMailService.EMailService es = new EMailService.EMailService(_db, company);
            var error = es.SendEMail(message);
            //if (!error.IsError) {
                // Now send as a notification
            //    TaskManagerService.TaskManagerService ts = new TaskManagerService.TaskManagerService(_db, company);
            //    error = ts.SendTask(templateId, notificationType, recipients, dict);
            //}
            return error;
        }

        #endregion
    }
}
