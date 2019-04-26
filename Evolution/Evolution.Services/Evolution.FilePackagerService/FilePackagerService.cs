using System;
using System.IO;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Resources;
using Evolution.Enumerations;

namespace Evolution.FilePackagerService {
    public partial class FilePackagerService : CommonService.CommonService {

        #region Construction

        public FilePackagerService(EvolutionEntities dbEntities) : base(dbEntities) {
        }

        #endregion

        #region Private members

        private CompanyService.CompanyService _companyService = null;
        protected CompanyService.CompanyService CompanyService {
            get {
                if(_companyService == null) _companyService = new CompanyService.CompanyService(db);
                return _companyService;
            }
        }

        private MembershipManagementService.MembershipManagementService _mms = null;
        protected MembershipManagementService.MembershipManagementService MembershipManagementService {
            get {
                if(_mms == null) _mms = new MembershipManagementService.MembershipManagementService(db);
                return _mms;
            }
        }

        private EMailService.EMailService _emailService = null;
        protected EMailService.EMailService EMailService(CompanyModel company = null) {
            if (_emailService == null) _emailService = new EMailService.EMailService(db, company);
            return _emailService;
        }

        private LookupService.LookupService _lookupService = null;
        protected LookupService.LookupService LookupService {
            get {
                if (_lookupService == null) _lookupService = new LookupService.LookupService(db);
                return _lookupService;
            }
        }

        private SupplierService.SupplierService _supplierService = null;
        protected SupplierService.SupplierService SupplierService {
            get {
                if (_supplierService == null) _supplierService = new SupplierService.SupplierService(db);
                return _supplierService;
            }
        }

        private DataTransferService.DataTransferService _dataTransferService = null;
        protected DataTransferService.DataTransferService DataTransferService {
            get {
                if (_dataTransferService == null) _dataTransferService = new DataTransferService.DataTransferService(db);
                return _dataTransferService;
            }
        }

        private string getTemplateFileName(FileTransferConfigurationModel template) {
            string fileName = GetConfigurationSetting("SiteFolder", "");
            fileName += "\\App_Data\\DataTransferTemplates\\" + template.ConfigurationTemplate;

            return fileName;
        }

        private Error moveFileToFTPFolder(string sourceQualFile, string targetFolder, string targetFileName) {
            var error = new Error();

            try {
                Directory.CreateDirectory(targetFolder);
            } catch { }

            string targetFile = targetFolder + "\\" + targetFileName;

            try {
                File.Delete(targetFile);
                File.Move(sourceQualFile, targetFile);

            } catch (Exception e1) {
                error.SetError(e1.Message);
            }

            return error;
        }

        #endregion
    }
}
