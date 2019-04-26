using System;
using System.Linq;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.SalesService {
    public partial class SalesService : CommonService.CommonService {

        #region Private members

        private CompanyService.CompanyService _companyService = null;
        protected CompanyService.CompanyService CompanyService {
            get {
                if (_companyService == null) _companyService = new Evolution.CompanyService.CompanyService(db);
                return _companyService;
            }
        }

        private LookupService.LookupService _lookupService = null;
        protected LookupService.LookupService LookupService {
            get {
                if (_lookupService == null) _lookupService = new LookupService.LookupService(db);
                return _lookupService;
            }
        }

        private CustomerService.CustomerService _customerService = null;
        protected CustomerService.CustomerService CustomerService {
            get {
                if (_customerService == null) _customerService = new CustomerService.CustomerService(db);
                return _customerService;
            }
        }
        /*
        private EMailService.EMailService _emailService = null;
        protected EMailService.EMailService EMailService {
            get {
                if (_emailService == null) _emailService = new EMailService.EMailService(db);
                return _emailService;
            }
        }
        */
        private SupplierService.SupplierService _supplierService = null;
        protected SupplierService.SupplierService SupplierService {
            get {
                if (_supplierService == null) _supplierService = new SupplierService.SupplierService(db);
                return _supplierService;
            }
        }

        private MediaService.MediaService _mediaService = null;
        protected MediaService.MediaService MediaServices {
            get {
                if (_mediaService == null) _mediaService = new MediaService.MediaService(db);
                return _mediaService;
            }
        }

        private AllocationService.AllocationService _allocationService = null;
        protected AllocationService.AllocationService AllocationService {
            get {
                if(_allocationService == null) _allocationService = new AllocationService.AllocationService(db);
                return _allocationService;
            }
        }

        private DocumentService.DocumentService _documentService = null;
        protected DocumentService.DocumentService DocumentService {
            get {
                if (_documentService == null) _documentService = new DocumentService.DocumentService(db);
                return _documentService;
            }
        }

        private ProductService.ProductService _productService = null;
        protected ProductService.ProductService ProductService {
            get {
                if (_productService == null) _productService = new Evolution.ProductService.ProductService(db);
                return _productService;
            }
        }

        private NoteService.NoteService _noteService = null;
        protected NoteService.NoteService NoteService {
            get {
                if (_noteService == null) _noteService = new Evolution.NoteService.NoteService(db);
                return _noteService;
            }
        }

        private MembershipManagementService.MembershipManagementService _mms = null;
        protected MembershipManagementService.MembershipManagementService MembershipManagementService {
            get {
                if (_mms == null) _mms = new MembershipManagementService.MembershipManagementService(db);
                return _mms;
            }
        }

        private TaskManagerService.TaskManagerService _taskManagerService = null;
        protected TaskManagerService.TaskManagerService GetTaskManagerService(CompanyModel company) {
            if (_taskManagerService == null) _taskManagerService = new TaskManagerService.TaskManagerService(db, company);
            return _taskManagerService;
        }

        private FilePackagerService.FilePackagerService _packagerService = null;
        protected FilePackagerService.FilePackagerService FilePackagerService {
            get {
                if (_packagerService == null) _packagerService = new FilePackagerService.FilePackagerService(db);
                return _packagerService;
            }
        }

        private PickService.PickService _pickService = null;
        protected PickService.PickService PickService {
            get {
                if (_pickService == null) _pickService = new PickService.PickService(db);
                return _pickService;
            }
        }

        private DataTransferService.DataTransferService _dateTransferService = null;
        protected DataTransferService.DataTransferService DataTransferService {
            get {
                if (_dateTransferService == null) _dateTransferService = new DataTransferService.DataTransferService(db);
                return _dateTransferService;
            }
        }

        #endregion

        #region Construction

        protected IMapper Mapper = null;

        public SalesService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<SalesOrderHeader, SalesOrderHeader>();
                cfg.CreateMap<SalesOrderHeader, SalesOrderHeaderModel>();
                cfg.CreateMap<SalesOrderHeader, SalesOrderHeaderTemp>();
                cfg.CreateMap<SalesOrderHeaderModel, SalesOrderHeader>()
                    .ForMember(s => s.BrandCategory, opt => opt.Ignore())
                    .ForMember(s => s.SalesOrderDetails, opt => opt.Ignore());
                cfg.CreateMap<SalesOrderHeaderTemp, SalesOrderHeaderTempModel>()
                    .ForMember(s => s.SOStatusValue, opt => opt.Ignore());
                cfg.CreateMap<SalesOrderHeaderTemp, SalesOrderHeader>();
                cfg.CreateMap<SalesOrderHeaderTempModel, SalesOrderHeaderTemp>()
                    .ForMember(s => s.BrandCategory, opt => opt.Ignore());
                cfg.CreateMap<SalesOrderDetailTemp, SalesOrderDetailTempModel>();
                cfg.CreateMap<SalesOrderDetailTempModel, SalesOrderDetailTemp>();
                cfg.CreateMap<SalesOrderDetail, SalesOrderDetailModel>();
                cfg.CreateMap<SalesOrderDetailModel, SalesOrderDetail>();
                cfg.CreateMap<FindCancellationSummaryList_Result, CancellationSummaryModel>();
                cfg.CreateMap<FindTransactionDrillDown_Result, TransactionDrillDownModel>();
                cfg.CreateMap<CreditClaimReplacementOrderModel, CreditClaimReplacementOrder>();
                cfg.CreateMap<CreditClaimReplacementOrder, CreditClaimReplacementOrderModel>();
                cfg.CreateMap<CreditClaimHeaderModel, CreditClaimHeader>();
                cfg.CreateMap<CreditClaimHeader, CreditClaimHeaderModel>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion

        #region Copying methods

        public SalesOrderHeaderTempModel CopySaleToTemp(CompanyModel company,
                                                        SalesOrderHeaderModel salesOrderHeader,
                                                        UserModel user,
                                                        bool createCopyOfOrder) {
            SalesOrderHeaderTempModel result = new SalesOrderHeaderTempModel();
            SalesOrderHeaderTemp soht = new SalesOrderHeaderTemp();

            if (salesOrderHeader.Id > 0) {
                // Editing an existing sale
                var soh = db.FindSalesOrderHeader(salesOrderHeader.Id);
                if (soh != null) {
                    if (createCopyOfOrder) {
                        // Copying/duplicating an order
                        Mapper.Map<SalesOrderHeader, SalesOrderHeaderTemp>(soh, soht);
                        soht.Id = 0;
                        soht.OriginalRowId = null;
                        soht.UserId = user.Id;
                        soht.OrderNumber = (int)LookupService.GetNextSequenceNumber(company, SequenceNumberType.SalesOrderNumber);
                        soht.OrderDate = DateTimeOffset.Now;
                        soht.SaleNextAction = db.FindSaleNextActions()
                                                .Where(sna => sna.Id == (int)Enumerations.SaleNextAction.None)
                                                .FirstOrDefault();

                        db.InsertOrUpdateSalesOrderHeaderTemp(soht);
                        result = mapToModel(soht);

                    } else {
                        // Editing an existing order
                        // Copy the header
                        soht = db.FindSalesOrderHeaderTemps(company.Id)
                                 .Where(p => p.UserId == user.Id &&
                                             p.OriginalRowId == salesOrderHeader.Id)
                                 .FirstOrDefault();
                        if (soht != null) {
                            // Already exists in the temp tables so update it with the latest data
                            int tempId = soht.Id;
                            Mapper.Map<SalesOrderHeader, SalesOrderHeaderTemp>(soh, soht);
                            soht.Id = tempId;
                            soht.OriginalRowId = (createCopyOfOrder ? 0 : salesOrderHeader.Id);
                            soht.UserId = user.Id;

                            db.InsertOrUpdateSalesOrderHeaderTemp(soht);
                            result = mapToModel(soht);

                        } else {
                            // Doesn't exist, so copy
                            soht = Mapper.Map<SalesOrderHeader, SalesOrderHeaderTemp>(soh);
                            soht.Id = 0;
                            soht.OriginalRowId = (createCopyOfOrder ? 0 : salesOrderHeader.Id);
                            soht.UserId = user.Id;

                            db.InsertOrUpdateSalesOrderHeaderTemp(soht);
                            result = mapToModel(soht);
                        }
                    }

                    // Now copy/merge the details
                    db.CopySaleToTemp(company.Id, user.Id, salesOrderHeader.Id, soht.Id, (createCopyOfOrder ? 1 : 0));
                }

            } else {
                // New sale
                soht.CompanyId = company.Id;
                soht.UserId = user.Id;
                soht.OrderNumber = salesOrderHeader.OrderNumber;
                soht.OrderDate = salesOrderHeader.OrderDate;
                soht.SalespersonId = salesOrderHeader.SalespersonId;
                soht.BrandCategoryId = salesOrderHeader.BrandCategoryId;
                soht.LocationId = salesOrderHeader.LocationId;
                soht.ShipCountryId = salesOrderHeader.ShipCountryId;
                soht.FreightCarrierId = salesOrderHeader.FreightCarrierId;
                soht.FreightTermId = salesOrderHeader.FreightTermId;
                soht.NextActionId = salesOrderHeader.NextActionId;

                db.InsertOrUpdateSalesOrderHeaderTemp(soht);
                result = Mapper.Map<SalesOrderHeaderTemp, SalesOrderHeaderTempModel>(soht);
            }

            return result;
        }

        public Error CopyTempToSalesOrderHeader(int salesOrderHeaderTempId, UserModel user, string lockGuid) {
            var error = new Error();
            SalesOrderHeader soh = null;

            var soht = db.FindSalesOrderHeaderTemp(salesOrderHeaderTempId);
            if (soht == null) {
                error.SetError(EvolutionResources.errRecordError, "OrderNumber", "SalesOrderHeaderTemp", salesOrderHeaderTempId.ToString());

            } else {
                if (soht.OriginalRowId != null && soht.OriginalRowId != 0) {
                    // Updating
                    if (!db.IsLockStillValid(typeof(SalesOrderHeader).ToString(), soht.OriginalRowId.Value, lockGuid)) {
                        error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "OrderNumber");

                    } else {
                        SalesOrderHeader before = null;

                        soh = db.FindSalesOrderHeader(soht.OriginalRowId.Value);
                        if (soh != null) {
                            // Found, so update
                            before = Mapper.Map<SalesOrderHeader, SalesOrderHeader>(soh);

                            Mapper.Map<SalesOrderHeaderTemp, SalesOrderHeader>(soht, soh);
                            soh.Id = soht.OriginalRowId.Value;

                        } else {
                            // Not found, so treat as new
                            soh = Mapper.Map<SalesOrderHeaderTemp, SalesOrderHeader>(soht);
                            soh.Id = 0;
                        }
                        db.InsertOrUpdateSalesOrderHeader(soh);
                        error.Id = soh.Id;

                        AuditService.LogChanges(typeof(SalesOrderHeader).ToString(), BusinessArea.SalesOrderDetails, user, before, soh);
                    }

                } else {
                    // New record
                    soh = Mapper.Map<SalesOrderHeaderTemp, SalesOrderHeader>(soht);
                    soh.Id = 0;
                    db.InsertOrUpdateSalesOrderHeader(soh);
                    error.Id = soh.Id;

                    AuditService.LogChanges(typeof(SalesOrderHeader).ToString(), BusinessArea.SalesOrderDetails, user, null, soh);
                }

                // Now copy the details from temp to live tables
                if (!error.IsError) {
                    db.CopyTempToSale(soht.CompanyId, soht.Id, soh.Id);

                    // And perform allocations
                    AllocationService.AllocateOnSalesOrderSave(Mapper.Map<SalesOrderHeader, SalesOrderHeaderModel>(soh));
                }
            }
            return error;
        }

        #endregion
    }
}
