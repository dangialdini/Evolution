using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.Configuration;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.Extensions;
using Evolution.Enumerations;
using AutoMapper;
using System.Web.Configuration;
using Evolution.PickService;

namespace Evolution.Controllers.Application
{
    public class BaseController : Controller
    {
        #region Private Members

        private LogService.LogService _logService = null;
        private EMailService.EMailService _emailService = null;
        private MembershipManagementService.MembershipManagementService _mms = null;
        private MenuService.MenuService _menuService = null;
        private LookupService.LookupService _lookupService = null;
        private CompanyService.CompanyService _companyService = null;
        private MediaService.MediaService _mediaServices = null;
        private CustomerService.CustomerService _customerService = null;
        private ProductService.ProductService _productService = null;
        private SupplierService.SupplierService _supplierService = null;
        private AuditService.AuditService _auditService = null;
        private PurchasingService.PurchasingService _purchasingService = null;
        private ShipmentService.ShipmentService _shipmentService = null;
        private TaskService.TaskService _taskService = null;
        private SalesService.SalesService _salesService = null;
        private SystemService.SystemService _systemService = null;
        private DataTransferService.DataTransferService _dataTransferService = null;
        private NoteService.NoteService _noteService = null;
        private TaskManagerService.TaskManagerService _taskManagerService = null;
        private FileImportService.FileImportService _fileImportService = null;
        private AllocationService.AllocationService _allocationService = null;
        private PickService.PickService _pickService = null;

        #endregion

        #region Protected Members

        protected EvolutionEntities db = new EvolutionEntities();

        protected LogService.LogService LogService {
            get {
                if (_logService == null) _logService = new LogService.LogService(db);
                return _logService;
            }
        }

        protected EMailService.EMailService EMailService {
            get {
                if (_emailService == null) _emailService = new EMailService.EMailService(db, CurrentCompany);
                return _emailService;
            }
        }

        protected MembershipManagementService.MembershipManagementService MembershipManagementService {
            get {
                if (_mms == null) _mms = new Evolution.MembershipManagementService.MembershipManagementService(db);
                return _mms;
            }
        }

        protected UserModel CurrentUser { get { return MembershipManagementService.User; } }

        protected MenuService.MenuService MenuService {
            get {
                if (_menuService == null) _menuService = new MenuService.MenuService(db);
                return _menuService;
            }
        }

        protected LookupService.LookupService LookupService {
            get {
                if (_lookupService == null) _lookupService = new LookupService.LookupService(db);
                return _lookupService;
            }
        }

        protected CompanyService.CompanyService CompanyService {
            get {
                if (_companyService == null) _companyService = new CompanyService.CompanyService(db);
                return _companyService;
            }
        }

        protected CompanyModel CurrentCompany {
            get {
                var company = CompanyService.FindCompanyModel(MembershipManagementService.GetProperty(MMSProperty.CurrentCompany, -1));
                return company;
            }
        }

        protected MediaService.MediaService MediaServices {
            get {
                if (_mediaServices == null) _mediaServices = new MediaService.MediaService(db);
                return _mediaServices;
            }
        }

        protected CustomerService.CustomerService CustomerService {
            get {
                if (_customerService == null) _customerService = new CustomerService.CustomerService(db);
                return _customerService;
            }
        }

        protected ProductService.ProductService ProductService {
            get {
                if (_productService == null) _productService = new ProductService.ProductService(db);
                return _productService;
            }
        }

        protected SupplierService.SupplierService SupplierService {
            get {
                if (_supplierService == null) _supplierService = new SupplierService.SupplierService(db);
                return _supplierService;
            }
        }

        protected AuditService.AuditService AuditService {
            get {
                if (_auditService == null) _auditService = new AuditService.AuditService(db);
                return _auditService;
            }
        }

        protected PurchasingService.PurchasingService PurchasingService {
            get {
                if (_purchasingService == null) _purchasingService = new PurchasingService.PurchasingService(db);
                return _purchasingService;
            }
        }

        protected ShipmentService.ShipmentService ShipmentService {
            get {
                if (_shipmentService == null) _shipmentService = new ShipmentService.ShipmentService(db);
                return _shipmentService;
            }
        }

        protected PickService.PickService PickService {
            get {
                if (_pickService == null) _pickService = new PickService.PickService(db);
                return _pickService;
            }
        }

        protected TaskService.TaskService TaskService {
            get {
                if (_taskService == null) _taskService = new TaskService.TaskService(db);
                return _taskService;
            }
        }

        protected SalesService.SalesService SalesService {
            get {
                if (_salesService == null) _salesService = new SalesService.SalesService(db);
                return _salesService;
            }
        }

        protected SystemService.SystemService SystemService {
            get {
                if (_systemService == null) _systemService = new SystemService.SystemService(db);
                return _systemService;
            }
        }

        protected DataTransferService.DataTransferService DataTransferService {
            get {
                if (_dataTransferService == null) _dataTransferService = new DataTransferService.DataTransferService(db);
                return _dataTransferService;
            }
        }

        protected NoteService.NoteService NoteService {
            get {
                if (_noteService == null) _noteService = new NoteService.NoteService(db);
                return _noteService;
            }
        }

        protected TaskManagerService.TaskManagerService TaskManagerService {
            get {
                if (_taskManagerService == null) _taskManagerService = new TaskManagerService.TaskManagerService(db);
                return _taskManagerService;
            }
        }

        protected FileImportService.FileImportService FileImportService {
            get {
                if (_fileImportService == null) _fileImportService = new FileImportService.FileImportService(db);
                return _fileImportService;
            }
        }

        protected AllocationService.AllocationService AllocationService {
            get {
                if (_allocationService == null) _allocationService = new AllocationService.AllocationService(db);
                return _allocationService;
            }
        }

        #endregion

        #region View Model Handling

        protected void PrepareViewModel(ViewModelBase model, 
                                        string pageTitle, int currentItemId = 0, int objectFlags = 0) {
            model.PageTitle = pageTitle;

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.AddProperty("id", currentItemId);

            UserModel user = MembershipManagementService.User;
            if (user != null) {
                model.DisplayDateFormat = user.DateFormat;
                model.JQDateFormat = user.DateFormat.ToLower().Replace("yyyy", "yy");
                dict.AddProperty("USERNAME", user.Name.Replace(".", " ").WordCapitalise());
            } else {
                dict.AddProperty("USERNAME", "");
            }

            var tempAreaName = this.ControllerContext.RouteData.DataTokens["area"];
            var areaName = (tempAreaName == null ? "" : tempAreaName.ToString() + ":");
            var controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            // Object flags are:
            //      1   Requires Customer
            //      2   Requires Purchase
            //      4
            //      8
            //      16  Requires Product
            //      32  Requires no product
            model.Menu = MenuService.GetMenu(0, 
                                             MembershipManagementService.IsLoggedIn, 
                                             MembershipManagementService.FindUserRoles(), 
                                             objectFlags, 
                                             areaName + controllerName, 
                                             dict);

            // Get the company list
            model.AvailableCompanies = CompanyService.FindCompaniesListItemModel();

            int selectedCompanyId = MembershipManagementService.GetProperty(MMSProperty.CurrentCompany, -1);            
            if(selectedCompanyId == -1) {
                // No company selected, so use the first in the list
                if(model.AvailableCompanies.Count() > 0) selectedCompanyId = Convert.ToInt32(model.AvailableCompanies.First().Id);
            }
            if (selectedCompanyId > 0) {
                model.CurrentCompany = CompanyService.FindCompanyModel(selectedCompanyId);
                model.MarginLogo = model.CurrentCompany.MarginLogo;
            }
        }


        public int MakeMenuOptionFlags(int p1, int p2 = 0, int p3 = 0, int p4 = 0, int p5 = 0, int p6 = 0, int p7 = 0, int p8 = 0) {
            int rc = 0;
            if (p1 > 0) rc |= MenuOptionFlag.RequiresCustomer;
            if (p2 > 0) rc |= MenuOptionFlag.RequiresPurchase;
            if (p3 > 0) rc |= MenuOptionFlag.RequiresShipment;
            if (p4 > 0) rc |= MenuOptionFlag.RequiresSale;
            if (p5 > 0) rc |= MenuOptionFlag.RequiresProduct;
            if (p6 > 0) rc |= MenuOptionFlag.RequiresNoProduct;
            if (p7 > 0) rc |= 64;
            if (p8 > 0) rc |= 128;
            return rc;
        }

        protected string GetFieldValue(string value) {
            if(value == null) {
                return "";
            } else {
                return value.Trim();
            }
        }

        protected DateTimeOffset? GetFieldValue(DateTimeOffset? dto, string tz) {
            if (dto == null) {
                return null;
            } else {
                return dto.Value.ParseDate(tz);
            }
        }

        protected DateTimeOffset Now(string tz) {
            return DateTimeOffsetExtensions.Now(tz);
        }

        #endregion

        #region Web.config

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

        protected int GetMaxFileUploadSize() {
            int rc = 0;
            // web.config setting is in KB
            HttpRuntimeSection section = (HttpRuntimeSection)ConfigurationManager.GetSection("system.web/httpRuntime");
            if (section != null) rc = section.MaxRequestLength * 1024;  // Get into bytes
            rc -= 2500;             // Less http header overhead
            rc -= 8192;             // Note text
            rc -= 512 * 4;          // URL references
            return rc;
        }

        #endregion

        #region Error handling

        protected void WriteLog(Exception ex, string rawUrl = "") {
            db.WriteLog(ex, rawUrl);
        }

        #endregion
    }
}
