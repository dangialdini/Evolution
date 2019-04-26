using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.AuditService;
using Evolution.CommonService;
using Evolution.CompanyService;
using Evolution.CustomerService;
using Evolution.DAL;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.LookupService;
using Evolution.MediaService;
using Evolution.MembershipManagementService;
using Evolution.Models.Models;
using Evolution.ProductService;
using Evolution.PurchasingService;
using Evolution.SalesService;
using Evolution.SupplierService;
using Evolution.SystemService;
using Evolution.NoteService;
using Evolution.ShipmentService;
using Evolution.DataTransferService;
using Evolution.DocumentService;
using Evolution.AllocationService;
using Evolution.EMailService;
using Evolution.TaskManagerService;
using Evolution.PickService;

namespace CommonTest {
    public class BaseTest {

        #region Members

        private int _cmdTimeoutSecs = 900;      // 15 minutes

        protected EvolutionEntities db = null;

        protected string ApplicationName { set; get; }
        protected string SourceRoot { set; get; }
        protected string TempFileFolder { set; get; }
        protected string MediaFolder { set; get; }
        protected string MediaHttp { set; get; }

        // Page size for all searches
        protected int PageSize { get { return 1000; } }

        private CompanyService _companyService = null;
        protected CompanyService CompanyService { get {
                if (_companyService == null) _companyService = new CompanyService(db);
                return _companyService;
            }
        }

        private CustomerService _customerService = null;
        protected CustomerService CustomerService {
            get {
                if (_customerService == null) _customerService = new CustomerService(db);
                return _customerService;
            }
        }

        private MediaService _mediaServices = null;
        protected MediaService MediaServices {
            get {
                if (_mediaServices == null) _mediaServices = new MediaService(db);
                return _mediaServices;
            }
        }

        private LookupService _lookupService = null;
        protected LookupService LookupService {
            get {
                if (_lookupService == null) _lookupService = new LookupService(db);
                return _lookupService;
            }
        }

        private MembershipManagementService _mms = null;
        protected MembershipManagementService MembershipManagementService {
            get {
                if (_mms == null) _mms = new MembershipManagementService(db);
                return _mms;
            }
        }

        private ProductService _productService = null;
        protected ProductService ProductService {
            get {
                if (_productService == null) _productService = new ProductService(db);
                return _productService;
            }
        }

        private SupplierService _supplierService = null;
        protected SupplierService SupplierService {
            get {
                if (_supplierService == null) _supplierService = new SupplierService(db);
                return _supplierService;
            }
        }

        private PurchasingService _purchasingService = null;
        protected PurchasingService PurchasingService {
            get {
                if (_purchasingService == null) _purchasingService = new PurchasingService(db);
                return _purchasingService;
            }
        }

        private SalesService _salesService = null;
        protected SalesService SalesService {
            get {
                if (_salesService == null) _salesService = new SalesService(db);
                return _salesService;
            }
        }

        private SystemService _systemService = null;
        protected SystemService SystemService {
            get {
                if (_systemService == null) _systemService = new SystemService(db);
                return _systemService;
            }
        }

        private AuditService _auditService = null;
        protected AuditService AuditService {
            get {
                if (_auditService == null) _auditService = new AuditService(db);
                return _auditService;
            }
        }

        private NoteService _noteService = null;
        protected NoteService NoteService {
            get {
                if (_noteService == null) _noteService = new NoteService(db);
                return _noteService;
            }
        }

        private ShipmentService _shipmentService = null;
        protected ShipmentService ShipmentService {
            get {
                if (_shipmentService == null) _shipmentService = new ShipmentService(db);
                return _shipmentService;
            }
        }

        private DataTransferService _dts = null;
        protected DataTransferService DataTransferService {
            get {
                if (_dts == null) _dts = new DataTransferService(db);
                return _dts;
            }
        }
        protected DataTransferService GetDataTransferService(CompanyModel company = null) {
            if (_dts == null) _dts = new DataTransferService(db, company);
            return _dts;
        }

        private DocumentService _docService = null;
        protected DocumentService DocumentService {
            get {
                if (_docService == null) _docService = new DocumentService(db);
                return _docService;
            }
        }

        private AllocationService _allocationService = null;
        protected AllocationService AllocationService {
            get {
                if (_allocationService == null) _allocationService = new AllocationService(db);
                return _allocationService;
            }
        }

        private EMailService _emailService = null;
        protected EMailService GetEMailService(CompanyModel company = null) {
            if (_emailService == null) _emailService = new EMailService(db, company);
            return _emailService;
        }

        private TaskManagerService _taskManagerService = null;
        protected TaskManagerService GetTaskManagerService(CompanyModel company = null) {
            if (_taskManagerService == null) _taskManagerService = new TaskManagerService(db, company);
            return _taskManagerService;
        }

        private PickService _pickService = null;
        protected PickService PickService {
            get {
                if (_pickService == null) _pickService = new PickService(db);
                return _pickService;
            }
        }

        #endregion

        #region Construction

        public BaseTest() {
            ApplicationName = GetAppSetting("ApplicationName", "");
            SourceRoot = GetAppSetting("SourceFolder", "");
            TempFileFolder = GetAppSetting("TempFileFolder", "");
            MediaFolder = GetAppSetting("MediaFolder", "");
            MediaHttp = GetAppSetting("MediaHttp", "");
        }

        [TestInitialize()]
        public void Initialize() {
            // Called at the beginning of a test to start
            db = new EvolutionEntities();
            db.Database.CommandTimeout = _cmdTimeoutSecs;
            db.StartTest();
        }

        public void LogTestFile(List<string> fileNames) {
            db.LogTestFile(fileNames);
        }

        public void LogTestFile(string fileName) {
            db.LogTestFile(fileName);
        }

        public void LogTestFolder(string folderName) {
            db.LogTestFolder(folderName);
        }

        [TestCleanup()]
        public void Cleanup() {
            // Called at the end of a test to cleanup, regardless of whether the test failed or not.
            db.EndTest();
        }

        #endregion

        #region Configuration file reading

        public string GetAppSetting(string keyName, string defaultValue) {
            string rc = "";
            try {
                rc = ConfigurationManager.AppSettings[keyName].ToString();
            } catch {
                rc = defaultValue;
            }
            return rc;
        }

        public int GetAppSetting(string keyName, int defaultValue) {
            int rc = 0;
            string temp = GetAppSetting(keyName, defaultValue.ToString());
            try {
                rc = Convert.ToInt32(temp);
            } catch {
                rc = defaultValue;
            }
            return rc;
        }

        public bool GetAppSetting(string keyName, bool defaultValue) {
            bool rc = defaultValue;
            string temp = GetAppSetting(keyName, defaultValue.ToString()).ToLower();
            if (temp == "1" ||
                temp == "true" ||
                temp == "t" ||
                temp == "yes" ||
                temp == "y" ||
                temp == "on") rc = true;
            return rc;
        }

        public string GetAppSetting(string key, string defaultValue, FTPProtocol protocol) {
            string prefix = protocol.ToString();
            return GetAppSetting(prefix + key, defaultValue);
        }

        #endregion

        #region Comparisons

        public bool AreEqual(Dictionary<string, string> param1, Dictionary<string, string> param2) {
            bool bRc = true;

            var keys1 = param1.Keys.ToList();
            var keys2 = param2.Keys.ToList();

            Assert.IsTrue(keys1.Count() == keys2.Count(), $"Error: Param2 contains {keys2.Count()} item(s) when it should contain {keys1.Count()}");

            // Check the keys
            for (int i = 0; i < keys2.Count; i++) {
                Assert.IsTrue(keys1[i] == keys2[i], $"Error: Param2 key {i} is '{keys2[i]}' when it shoudl be {keys1[i]}");
            }

            // Check the values
            foreach (var key in keys1) {
                Assert.IsTrue(param1[key] == param2[key], $"Error: Param2 key {param2[key]} is '' when it should be {param1[key]}");
            }

            return bRc;
        }

        public bool AreEqual(Object src, Object dst, List<string> excludes = null) {
            bool bRc = true;

            if (src == null) {
                Assert.Fail("Error: Source object is NULL - cannot perform comparison");

            } else if (dst == null) {
                Assert.Fail("Error: Target object is NULL - cannot perform comparison");

            } else {
                string result = "";

                var srcT = src.GetType();
                var dstT = dst.GetType();
                foreach (var f in srcT.GetFields()) {
                    if (excludes == null || !excludes.Contains(f.Name)) {
                        var srcV = f.GetValue(f.Name);

                        var dstF = dstT.GetField(f.Name);
                        var dstV = dstF.GetValue(f.Name);

                        if (srcV != dstV) {
                            if (string.IsNullOrEmpty(result)) {
                                result = "Error:";
                            }
                            result += "\r\n  Target field {f.Name} has value {srcV} when {dstV} was expected";
                        }
                    }
                }

                foreach(var p in srcT.GetProperties()) {
                    if (excludes == null || !excludes.Contains(p.Name)) {
                        //object srcV = srcT.GetProperty(p.Name);
                        var srcV = p.GetValue(src, null);

                        var dstF = dstT.GetProperty(p.Name);
                        if (dstF != null) {
                            var dstV = dstF.GetValue(dst, null);

                            //if (srcV != dstV && srcV != null && srcV.ToString() != dstV.ToString()) {
                            if (srcV != dstV) {
                                // Values are different. Either could be null.
                                bool matched = false;
                                if (srcV != null && dstV != null) {
                                    if (srcV.ToString() == dstV.ToString()) {
                                        matched = true;
                                    }
                                }

                                if (!matched) {
                                    if (string.IsNullOrEmpty(result)) {
                                        result = "Error:";
                                    }
                                    result += $"\r\n  Target property {p.Name} has value '{srcV ?? "NULL"}' when '{dstV ?? "NULL"}' was expected";
                                }
                            }
                        }
                    }
                }

                Assert.IsTrue(string.IsNullOrEmpty(result), result);
            }

            return bRc;
        }

        #endregion

        #region Helpers

        protected UserModel GetTestUser(bool bEnabled = true, bool bInsertToDb = true) {
            var user = new UserModel();
            user.Name = RandomString();
            user.LastName = user.Name;
            user.FirstName = user.Name.Left(20);
            user.EMail = RandomEMail();
            user.Enabled = bEnabled;

            if (bInsertToDb) {
                MembershipManagementService.InsertOrUpdateUser(user, null, "");

                // Add the user to some user groups
                MembershipManagementService.AddUserToGroup("Test Purchasing", user);
            }

            return user;
        }

        protected void AddUserToUserGroups(CompanyModel company, UserModel user) {
            // Add user to groups which are named for eah brand category
            foreach (var brandCat in ProductService.FindBrandCategoriesModel(company)) {
                MembershipManagementService.AddUserToGroup(brandCat.CategoryName + " purchasing", user);
            }
        }

        protected CompanyModel GetTestCompanyAU() {
            // This company should ONLY be used in tests 
            // which do lookups and NO updates
            return CompanyService.FindCompanyFriendlyNameModel("COMPANYNAME Pty Ltd");
        }

        protected CompanyModel GetTestCompany(UserModel testUser, bool bCopyLOVs = false, bool bEnabled = true) {
            // This should be used for all tests as by doing so, the tests can be run against the production
            // database without interfering with the production company data - the data is isolated into separate companies

            // Make a copy of the COMPANYNAME organisation
            var friendlyName = "COMPANYNAME Pty Ltd";
            var testCompany = CompanyService.FindCompanyFriendlyNameModel(friendlyName);
            Assert.IsTrue(testCompany != null, $"Error: Failed to find Company '{friendlyName}'");

            testCompany.Id = 0;
            testCompany.CompanyName = RandomString();
            testCompany.FriendlyName = testCompany.CompanyName.Left(32);
            testCompany.DefaultCountryID = LookupService.FindCountryModel("Australia").Id;
            testCompany.IsParentCompany = false;        // Must have this otherwise the test company incorrectly becomes
                                                        // the parent company which breaks the database configuration

            var error = CompanyService.InsertOrUpdateCompany(testCompany, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Create lookup tables - copy them from the AU company
            var orgAu = GetTestCompanyAU();

            db.CopyDataForTest(orgAu.Id, testCompany.Id);

            testCompany.DefaultLocationID = db.FindLocation(testCompany.Id, "Warehouse").Id;
            error = CompanyService.InsertOrUpdateCompany(testCompany, testUser, CompanyService.LockCompany(testCompany));
            Assert.IsTrue(!error.IsError, error.Message);

            // Message Templates
            int actual = LookupService.FindMessageTemplatesListModel(testCompany.Id, 0, 1, PageSize, "").Items.Count();
            Assert.IsTrue(actual > 0, $"Error: {actual} EMail Templates were found for the test Company when more were expected");

            // Lists of values
            if (bCopyLOVs) {
                int totalItems = 0;
                foreach (var lov in LookupService.FindLOVsModel(true)) {
                    var lovItems = LookupService.FindLOVItemsModel(testCompany.Id, 0, lov.Id, 1, PageSize, "");
                    totalItems += lovItems.Items.Count();
                }
                Assert.IsTrue(totalItems > 0, $"Error: {totalItems} LOV Items were found for the test Company when more were expected");
            }

            // Brand Categories
            actual = ProductService.FindBrandCategoriesListModel(testCompany.Id, 0, 1, PageSize, "").Items.Count();
            Assert.IsTrue(actual > 0, $"Error: {actual} Brand Categories were found for the test Company when more were expected");

            // Marketing Groups
            actual = LookupService.FindMarketingGroupsListModel(testCompany.Id, 0, 1, PageSize, "").Items.Count();
            Assert.IsTrue(actual > 0, $"Error: {actual} Marketing Groups were found for the test Company when more were expected");

            // Payment terms
            actual = LookupService.FindPaymentTermsListModel(testCompany.Id, 0, 1, PageSize).Items.Count();
            Assert.IsTrue(actual > 0, $"Error: {actual} Payment Terms were found for the test Company when more were expected");

            // Price levels
            actual = LookupService.FindPriceLevelsListModel(testCompany.Id, 0, 1, PageSize, "").Items.Count();
            Assert.IsTrue(actual > 0, $"Error: {actual} Marketing Price Levels were found for the test Company when more were expected");

            // Regions
            actual = LookupService.FindRegionsListModel(testCompany.Id, 0, 1, PageSize, "").Items.Count();
            Assert.IsTrue(actual > 0, $"Error: {actual} Regions were found for the test Company when more were expected");

            // Shipping templates
            actual = LookupService.FindDocumentTemplatesListModel(0, 1, PageSize, "").Items.Count();
            Assert.IsTrue(actual > 0, $"Error: {actual} Shipping Templates were found for the test Company when more were expected");

            // Tax code
            actual = LookupService.FindTaxCodesListModel(testCompany.Id, 0, 1, PageSize, "").Items.Count();
            Assert.IsTrue(actual > 0, $"Error: {actual} Tax Codes were found for the test Company when more were expected");

            // Locations
            actual = LookupService.FindLocationListItemModel(testCompany).Count();
            Assert.IsTrue(actual > 0, $"Error: {actual} Locations were found for the test Company when more were expected");

            // Freight carriers
            actual = LookupService.FindFreightCarriersListModel(testCompany.Id).Items.Count();
            Assert.IsTrue(actual > 0, $"Error: {actual} Freight Carriers were found for the test Company when more were expected");

            return testCompany;
        }

        protected void CreateTestTransfers(CompanyModel testCompany, UserModel testUser) {
            GetDataTransferService(testCompany);    // Initialise DTS with a company

            // Find the "Warehouse" location
            string locationName = "Warehouse";
            var location = LookupService.MapToModel(db.FindLocation(testCompany.Id, locationName));
            Assert.IsTrue(location != null, $"Error: Location '{locationName}' could not be found");

            // Create a file transfer configurations for the location
            var ff = GetTestFreightForwarder(testCompany);

            // Create file transfer configurations
            var transferName = "Test Transfer";

            var sendConfigs = new List<TestFileTransfer>();
            sendConfigs.Add(new TestFileTransfer(FileTransferDataType.WarehousePurchase, "Warehouse_PurchaseOrder.xml", ""));
            sendConfigs.Add(new TestFileTransfer(FileTransferDataType.WarehousePick, "Warehouse_Pick.xml", "{PICKNO}.{EXTN}"));
            sendConfigs.Add(new TestFileTransfer(FileTransferDataType.FreightForwarderPurchase, "FreightForwarder.xml", ""));
            sendConfigs.Add(new TestFileTransfer(FileTransferDataType.WarehouseUnpackSlip, "", ""));

            var recvConfigs = new List<TestFileTransfer>();
            recvConfigs.Add(new TestFileTransfer(FileTransferDataType.WarehouseCompletedPurchase, "", ""));
            recvConfigs.Add(new TestFileTransfer(FileTransferDataType.WarehouseCompletedPick, "", ""));
            recvConfigs.Add(new TestFileTransfer(FileTransferDataType.FreightForwarderPurchase, "", ""));
            recvConfigs.Add(new TestFileTransfer(FileTransferDataType.WarehouseUnpackSlip, "", ""));

            foreach (var config in sendConfigs) {
                GetTestTransfer(testCompany,
                                testUser,
                                location,
                                FileTransferType.Send,
                                FTPProtocol.FTP,
                                "c:\\temp\\" + config.DataType.ToString() + "\\Send",
                                "/test/Evolution/" + config.DataType.ToString() + "/",
                                "/test/Evolution/" + config.DataType.ToString() + "/Archive/",
                                "/test/Evolution/" + config.DataType.ToString() + "/Error/",
                                config,
                                ff);
            }

            foreach (var config in recvConfigs) {
                GetTestTransfer(testCompany,
                                testUser,
                                location,
                                FileTransferType.Receive,
                                FTPProtocol.FTP,
                                "/test/Evolution/" + config.DataType.ToString() + "/",
                                "c:\\temp\\" + transferName.Replace(" ", "") + "\\" + config.DataType.ToString(),
                                "c:\\temp\\" + transferName.Replace(" ", "") + "\\" + config.DataType.ToString() + "\\Archive",
                                "c:\\temp\\" + transferName.Replace(" ", "") + "\\" + config.DataType.ToString() + "\\Error",
                                config,
                                null);
            }
        }

        protected FreightForwarder GetTestFreightForwarder(CompanyModel testCompany) {
            string ffName = "Test Freight Forwarder";
            var ff = db.FindFreightForwarders()
                       .Where(l => l.Name == ffName)
                       .FirstOrDefault();
            if(ff == null) {
                ff = new FreightForwarder {
                    CompanyId = testCompany.Id,
                    Name = ffName,
                    Email = RandomEMail(),
                    Enabled = true
                };
                db.InsertOrUpdateFreightForwarder(ff);
            }
            return ff;
        }

        protected CustomerModel GetTestCustomer(CompanyModel testCompany, UserModel testUser, bool bEnabled = true, bool bWriteToDb = true) {
            var currency = LookupService.FindCurrenciesModel().First();
            var paymentTerm = LookupService.FindPaymentTermsModel(testCompany.Id).First();
            var priceLevel = LookupService.FindPriceLevelsModel(testCompany.Id).First();
            var taxCode = LookupService.FindTaxCodesModel(testCompany.Id).First();
            var region = LookupService.FindRegionsModel(testCompany.Id).FirstOrDefault();

            var customer = new CustomerModel();
            customer.CompanyId = testCompany.Id;
            customer.CreatedDate = DateTime.Now;
            customer.CreatedById = testUser.Id;
            customer.Name = RandomString();
            customer.CurrencyId = currency.Id;
            customer.PaymentTermId = paymentTerm.Id;
            customer.PriceLevelId = priceLevel.Id;
            customer.TaxCodeId = taxCode.Id;
            customer.Enabled = bEnabled;

            if (bWriteToDb) {
                var error = CustomerService.InsertOrUpdateCustomer(customer, testUser, "");
                Assert.IsTrue(!error.IsError, error.Message);

                // Additional info
                var additionalInfo = new CustomerAdditionalInfoModel {
                    Id = customer.Id,
                    CompanyId = testCompany.Id,
                    RegionId = region.Id,
                    ShippingTemplateId = LookupService.FindDocumentTemplateModel(DocumentTemplateCategory.Pickslip, DocumentTemplateType.PackingSlip).Id,
                    ShippingTemplateType = (int)DocumentTemplateCategory.Invoice,
                    SourceId = LookupService.FindLOVItemsModel(testCompany, LOVName.Source).FirstOrDefault().Id
                };
                error = CustomerService.InsertOrUpdateCustomerAdditionalInfo(additionalInfo, testUser, CustomerService.LockCustomer(customer));
                Assert.IsTrue(!error.IsError, error.Message);

                // Address
                var address = new CustomerAddressModel() {
                    CompanyId = testCompany.Id,
                    CustomerId = customer.Id,
                    AddressTypeId = LookupService.FindLOVItemByValueModel(LOVName.AddressType, ((int)AddressType.Billing).ToString()).Id,
                    Street = RandomString(),
                    City = RandomString().Left(50),
                    State = RandomString().Left(20),
                    Postcode = RandomString().Left(10),
                    DateStart = DateTimeOffset.Now,
                    DateEnd = null,
                    CountryId = LookupService.FindCountryModel("Australia").Id
                };
                error = CustomerService.InsertOrUpdateCustomerAddress(address);
                Assert.IsTrue(!error.IsError, error.Message);

                // Now add a primary contact
                var contact = new CustomerContactModel {
                    CompanyId = testCompany.Id,
                    CustomerId = customer.Id,
                    ContactFirstname = RandomString(),
                    ContactSurname = RandomString(),
                    ContactKnownAs = RandomString().Left(30),
                    ContactEmail = RandomEMail(),
                    //public string Position { set; get; } = "";
                    //public string ContactSalutation { set; get; } = "";
                    //public string ContactPhone1 { set; get; } = "";
                    //public string ContactPhone2 { set; get; } = "";
                    //public string ContactPhone3 { set; get; } = "";
                    //public string ContactFax { set; get; } = "";
                    //public string ContactPhoneNotes { set; get; } = "";
                    //public string ContactNotes { set; get; } = "";
                    //public bool SendStatement { set; get; } = false;
                    //public bool SendInvoice { set; get; } = false;
                    //public bool MailingList { set; get; } = false;
                    PrimaryContact = true,
                    //public bool ReceiveCatalog { set; get; } = false;
                    Enabled = true
                };

                error = CustomerService.InsertOrUpdateCustomerContact(contact, testUser, "");
                Assert.IsTrue(!error.IsError, error.Message);
            }

            return customer;
        }

        protected SupplierModel GetTestSupplier(UserModel testUser, bool bEnabled = true) {
            // Create a supplier
            var supplier = new SupplierModel();
            supplier.CreatedById = testUser.Id;
            supplier.Name = RandomString();
            supplier.Enabled = bEnabled;

            var error = SupplierService.InsertOrUpdateSupplier(supplier, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Add an addres
            var addrs = new SupplierAddressModel {
                SupplierId = supplier.Id,
                Street = RandomString(),
                City = RandomString(),
                State = RandomString().Left(20),
                Postcode = RandomString().Left(10),
                StreetLine1 = RandomString(),
                StreetLine2 = RandomString(),
                StreetLine3 = RandomString(),
                StreetLine4 = RandomString()
            };

            error = SupplierService.InsertOrUpdateSupplierAddress(addrs, testUser);
            Assert.IsTrue(!error.IsError, error.Message);

            return supplier;
        }

        protected PurchaseOrderHeaderModel GetTestPurchaseOrderHeader(CompanyModel testCompany, UserModel testUser,
                                                                      int numItemsToAdd = 0) {
            // Find a supplier with lots of products
            var suppliers = db.FindSuppliers(testCompany.Id)
                              .Where(s => !string.IsNullOrEmpty(s.Email) &&
                                          s.Products.Where(p => p.Enabled == true)
                                                    .Count() > numItemsToAdd)
                              .ToList();
            int randomSupplier = RandomInt(0, suppliers.Count() - 1);
            var supplier = suppliers[randomSupplier];

            // Find a port
            var ports = LookupService.FindPortsListItemModel().ToList();


            // Create a purchase order header
            var shipMethods = LookupService.FindLOVItemsListItemModel(testCompany, LOVName.ShippingMethod).Where(sm => sm.Text == "Seafreight");
            Assert.IsTrue(shipMethods.Count() > 0, "Error: No shipping methods found - make sure the Company for this test was created with CopyLOVs=true");

            var poStatus = LookupService.FindPurchaseOrderHeaderStatusByValueModel(PurchaseOrderStatus.OrderPOSentToSupplier);

            var brandCategory = supplier.Products         // Use the supplier's first product to get a brand category
                                          .First()
                                          .Brand
                                          .BrandBrandCategories
                                          .First()
                                          .BrandCategory;

            var poh = new PurchaseOrderHeaderModel {
                CompanyId = testCompany.Id,
                LocationId = testCompany.DefaultLocationID,
                BrandCategoryId = brandCategory.Id,
                CurrencyId = Convert.ToInt32(LookupService.FindCurrenciesListItemModel().First().Id),
                FreightForwarderId = GetTestFreightForwarder(testCompany).Id,
                SalespersonId = testUser.Id,
                SalesPersonName = (testUser.FirstName.WordCapitalise() + " " + testUser.LastName.WordCapitalise()).Trim(),
                POStatus = poStatus.Id,
                POStatusText = poStatus.StatusName,
                POStatusValue = (PurchaseOrderStatus)poStatus.StatusValue,
                SupplierId = supplier.Id,
                SupplierName = supplier.Name,
                SupplierInv = "INV" + RandomInt(1000, 9999).ToString(),
                RequiredDate = RandomDateTime(),
                OrderNumber = LookupService.GetNextSequenceNumber(testCompany, SequenceNumberType.PurchaseOrderNumber),
                OrderDate = RandomDateTime(),
                RequiredShipDate = RandomDateTime(),
                PortId = Convert.ToInt32(ports[RandomInt(0, ports.Count() - 1)].Id),
                PortArrivalId = Convert.ToInt32(ports[RandomInt(0, ports.Count() - 1)].Id),
                ShipMethodId = Convert.ToInt32(shipMethods.First().Id),
                ExchangeRate = RandomInt(),
                PaymentTermsId = Convert.ToInt32(LookupService.FindPaymentTermsListItemModel(testCompany).First().Id),
                CommercialTermsId = Convert.ToInt32(LookupService.FindLOVItemsListItemModel(testCompany, LOVName.CommercialTerms).First().Id),
                ContainerTypeId = Convert.ToInt32(LookupService.FindContainerTypeListItemModel().First().Id),
                //LandingDate = RandomDateTime(),   // This is a calculated field and is not stored in this table
                RealisticRequiredDate = RandomDateTime(),
                CompletedDate = RandomDateTime(),
                Splitable = numItemsToAdd > 0
            };

            var error = PurchasingService.InsertOrUpdatePurchaseOrderHeader(poh, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Create a user group
            string groupName = brandCategory.CategoryName + " Purchasing";
            var userGroup = db.FindUserGroup(groupName);
            if (userGroup == null) {
                userGroup = new UserGroup {
                    GroupName = groupName,
                    Enabled = true
                };
                db.InsertOrUpdateUserGroup(userGroup);
            }

            // Add our test user to the group
            var groupUser = db.FindUserGroupUser(userGroup.Id, testUser.Id);
            if (groupUser == null) {
                groupUser = new UserGroupUser {
                    UserGroup = userGroup,
                    UserId = testUser.Id
                };
                db.InsertOrUpdateUserGroupUser(groupUser);
            }

            // If requested to add items to the order...
            if (numItemsToAdd > 0) {
                // ...add some items
                ListItemModel taxCode = LookupService.FindTaxCodesListItemModel(testCompany).First();

                var supplierProds = db.FindProductsForSupplier(poh.SupplierId.Value)
                                      .Take(numItemsToAdd)
                                      .ToList();
                Assert.IsTrue(supplierProds.Count >= numItemsToAdd, $"Error: Supplier {supplier.Name} only has {supplierProds.Count()} Products - cannot add {numItemsToAdd} product(s)");

                int i = 0;
                foreach (var product in supplierProds) {
                    PurchaseOrderDetailModel pod = new PurchaseOrderDetailModel {
                        CompanyId = testCompany.Id,
                        PurchaseOrderHeaderId = poh.Id,
                        LineNumber = i * 1000 + 1000,
                        ProductId = product.Id,
                        ProductDescription = product.ItemName,
                        UnitPriceExTax = (product.BaseSellingPrice == null ? 0 : (decimal)product.BaseSellingPrice.Value),
                        TaxCodeId = Convert.ToInt32(taxCode.Id),
                        DiscountPercent = 0,
                        OrderQty = RandomInt(20, 200)
                        //public int? LineStatus { set; get; } = null;
                        //public bool IsReceived { set; get; } = false;
                    };
                    error = PurchasingService.InsertOrUpdatePurchaseOrderDetail(pod, testUser, "");
                    Assert.IsTrue(!error.IsError, error.Message);

                    // Create an allocation
                    var allocation = new AllocationModel {
                        CompanyId = testCompany.Id,
                        ProductId = pod.ProductId,
                        SaleLineId = null,
                        PurchaseLineId = pod.Id,
                        Quantity = pod.OrderQty.Value,
                        LocationId = poh.LocationId,
                        DateCreated = DateTimeOffset.Now
                    };
                    error = AllocationService.InsertOrUpdateAllocation(allocation, testUser, "");
                    Assert.IsTrue(!error.IsError, error.Message);
                    i++;
                }
            }

            return poh;
        }

        protected SalesOrderHeaderModel GetTestSalesOrderHeader(CompanyModel testCompany,
                                                                CustomerModel customer,
                                                                UserModel testUser,
                                                                int numItemsToAdd = 0, 
                                                                bool bLoadDetails = false) {

            var soStatus = LookupService.FindSalesOrderHeaderStatusByValueModel(SalesOrderHeaderStatus.Quote);
            var brandCategory = ProductService.FindBrandCategoriesModel(testCompany).FirstOrDefault();

            var soh = new SalesOrderHeaderModel {
                CompanyId = testCompany.Id,
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                OrderNumber = (int)LookupService.GetNextSequenceNumber(testCompany, SequenceNumberType.SalesOrderNumber),
                OrderDate = RandomDateTime(),
                RequiredDate = RandomDateTime(),
                SOStatus = soStatus.Id,
                SOStatusValue = (SalesOrderHeaderStatus)soStatus.StatusValue,
                SOStatusText = soStatus.StatusName,
                ShipAddress1 = RandomString(),
                ShipSuburb = RandomString(),
                ShipState = RandomString().Left(3),
                ShipPostcode = RandomString().Left(4),
                ShipCountryId = LookupService.FindCountryModel("Australia").Id,
                LocationId = testCompany.DefaultLocationID,
                DeliveryWindowOpen = DateTimeOffset.Now,
                BrandCategoryId = brandCategory.Id,
                BrandCategoryText = brandCategory.CategoryName,
                SalespersonId = testUser.Id,
                SalesPersonName = testUser.FullName.WordCapitalise()
            };
            soh.DeliveryWindowClose = LookupService.GetDeliveryWindow(soh.DeliveryWindowOpen.Value);

            var error = SalesService.InsertOrUpdateSalesOrderHeader(soh, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            List<Product> productList = db.FindProducts()
                                          .Skip(numItemsToAdd * 10)
                                          .ToList();

            int numAdded = 0;
            for (int i = 0; i < productList.Count() && numAdded < numItemsToAdd; i++) {
                var prodPrice = ProductService.FindProductPrice(testCompany, productList[i].Id, customer.Id);
                if (prodPrice != null && prodPrice.SellingPrice > 0) {
                    // Only add products which have a non-zero sell-price
                    var sod = new SalesOrderDetailModel {
                        CompanyId = testCompany.Id,
                        SalesOrderHeaderId = soh.Id,
                        LineNumber = 1000,
                        ProductId = productList[i].Id,
                        OrderQty = RandomInt(productList[i].MinSaleQty ?? 1),
                        UnitPriceExTax = prodPrice.SellingPrice
                        //ConflictFlag = false,
                        //ConflictApproved = false,
                        //ReallocateItem = false
                    };

                    error = SalesService.InsertOrUpdateSalesOrderDetail(sod, "");
                    Assert.IsTrue(!error.IsError, error.Message);

                    if(bLoadDetails) soh.SalesOrderDetails.Add(sod);
                    numAdded++;
                }
            }
            Assert.IsTrue(numAdded == numItemsToAdd, $"Error: {numItemsToAdd} were requested to be added to a Sales order but only {numAdded} could be found");

            return soh;
        }

        protected SalesOrderHeaderTempModel GetTestSalesOrderHeaderTemp(CompanyModel testCompany,
                                                                    CustomerModel customer,
                                                                    UserModel testUser,
                                                                    int numItemsToAdd = 0) {

            var soh = GetTestSalesOrderHeader(testCompany, customer, testUser, numItemsToAdd);
            return SalesService.CopySaleToTemp(testCompany, soh, testUser, false);
        }

        protected NoteModel CreateCustomerNote(int companyId, int customerId, UserModel user, bool addAttachment) {
            NoteModel model = new NoteModel {
                CompanyId = companyId,
                NoteType = NoteType.Customer,
                ParentId = customerId,
                DateCreated = DateTime.Now,
                CreatedById = user.Id,
                CreatedBy = (user.FirstName + " " + user.LastName).Trim().WordCapitalise(),
                Subject = "Test Note Subject",
                Message = "Test Note Message"
            };
            var error = NoteService.InsertOrUpdateNote(model, user, "");

            Assert.IsFalse(error.IsError, error.Message);

            if (addAttachment) {
                // Create a media item - copy the app logo
                string fileName = RandomString();
                string targetFolder = MediaServices.GetTempFolder();
                string target = targetFolder + "\\" + fileName + ".jpg";
                string source = GetAppSetting("SourceFolder", "") + "\\Evolution\\Content\\EvolutionLogo.png";

                try {
                    Directory.CreateDirectory(targetFolder);
                    File.Copy(source, target, true);
                } catch (Exception e1) {
                    Assert.Fail("Error: " + e1.Message);
                }
                LogTestFile(target);    // Log the file for later deletion

                // Attach it
                NoteService.AttachMediaItemToNote(model, user, target, target.FileName(), FileCopyType.Move);
            }

            return model;
        }

        protected ShipmentModel GetTestShipment(CompanyModel company, UserModel user, int ordersToAdd = 0) {
            var model = new ShipmentModel {
                CompanyId = company.Id,
            };
            var error = ShipmentService.InsertOrUpdateShipment(model, user, "");
            Assert.IsTrue(!error.IsError, error.Message);

            if(ordersToAdd > 0) {
                var pohList = "";
                for(int i = 0; i < ordersToAdd; i++) {
                    var poh = GetTestPurchaseOrderHeader(company, user, RandomInt(1, 20));
                    if (i > 0) pohList += ",";
                    pohList += poh.Id.ToString();
                }
                ShipmentService.AddPurchaseOrders(company, user, model, pohList);
            }
            return model;
        }

        public FileTransferConfigurationModel GetTestTransfer(CompanyModel testCompany,
                                                              UserModel testUser,
                                                              LocationModel location,
                                                              FileTransferType transferType,
                                                              FTPProtocol protocol,
                                                              string sourceFolder,
                                                              string targetFolder,
                                                              string archiveFolder,
                                                              string errorFolder,
                                                              TestFileTransfer config,
                                                              FreightForwarder ff = null) {
            var transferConfig = new FileTransferConfigurationModel {
                CompanyId = testCompany.Id,
                TransferType = transferType,
                DataTypeId = db.FindLOVItemByValue1(testCompany.Id, LOVName.FileTransferDataType, ((int)config.DataType).ToString()).Id,
                CreatedDate = DateTimeOffset.Now,
                CreatedById = testUser.Id,
                TransferName = "Test Transfer:" + transferType.ToString() + "-" + config.DataType.ToString(),
                FTPHost = GetAppSetting("Host", "", protocol),
                UserId = GetAppSetting("Login", "", protocol),
                Password = GetAppSetting("Password", "", protocol),
                Protocol = protocol,
                SourceFolder = sourceFolder,
                TargetFolder = targetFolder,
                ArchiveFolder = archiveFolder,
                ErrorFolder = errorFolder,
                ConfigurationTemplate = config.Template,
                LocationId = location.Id,
                FreightForwarderId = null,
                TargetFileNameFormat = config.FileNameFormat,
                Enabled = true
            };
            if (ff != null) transferConfig.FreightForwarderId = ff.Id;

            var error = DataTransferService.InsertOrUpdateDataTransferConfiguration(transferConfig,
                                                                                    testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            return transferConfig;
        }

        protected UserModel GetTaskUser() {
            var user = db.FindUser("TaskUser");
            if (user == null)
                return null;
            else
                return new UserModel { Id = user.Id };
        }

        protected CreditCardModel GetTestCreditCard(CompanyModel testCompany, CustomerModel testCustomer) {
            var creditCard = new CreditCardModel {
                CompanyId = testCompany.Id,
                CustomerId = testCustomer.Id,
                CreditCardNo = RandomString().Left(16),
                NameOnCard = RandomString().Left(42),
                Expiry = RandomString().Left(5),
                CCV = RandomInt(100, 999).ToString(),
                Notes = RandomString().Left(100)
            };

            var error = CustomerService.InsertOrUpdateCreditCard(creditCard, "");
            Assert.IsTrue(!error.IsError, error.Message);

            return creditCard;
        }

        protected string RandomString() {
            return Guid.NewGuid().ToString();
        }

        protected string LorumIpsum() {
            string str = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor";
            str += " incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud";
            str += " exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure";
            str += " dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.";
            str += " Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit";
            str += " anim id est laborum.";
            return str;
        }

        Random random = null;

        protected int RandomInt(int rangeFrom = 1, int rangeTo = 1000) {
            if(random == null) random = new Random();
            return random.Next(rangeFrom, rangeTo);
        }

        protected DateTime RandomDateTime() {
            int days = RandomInt(0, 31),
                months = RandomInt(0, 24),
                hours = RandomInt(0, 23),
                minutes = RandomInt(0, 59),
                seconds = RandomInt(0, 59);

            DateTime now = DateTime.Now;
            now = now.AddDays(days);
            now = now.AddMonths(months);
            now = now.AddHours(hours);
            now = now.AddMinutes(minutes);
            now = now.AddSeconds(seconds);

            return now;
        }

        protected string RandomEMail() {
            return RandomString() + "@placeholder.com.au";
        }

        #endregion

        #region Temporary files

        protected string GetTempFile(string extn = ".dat") {
            string rc = Path.GetTempFileName();
            File.Delete(rc);
            rc = rc.ChangeExtension(extn);
            LogTestFile(rc);
            return rc;
        }

        #endregion
    }

    public class TestFileTransfer {
        public FileTransferDataType DataType { set; get; }
        public string Template { set; get; }
        public string FileNameFormat { set; get; }

        public TestFileTransfer(FileTransferDataType dataType, string template, string fileNameFormat) {
            DataType = dataType;
            Template = template;
            FileNameFormat = fileNameFormat;
        }
    }
}
