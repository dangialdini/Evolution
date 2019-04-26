using System.Linq;
using System.Data.Entity;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.LookupService;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.PurchasingService {
    public partial class PurchasingService : CommonService.CommonService {

        #region Private members

        private LookupService.LookupService _lookupService = null;
        protected LookupService.LookupService LookupService {
            get {
                if (_lookupService == null) _lookupService = new LookupService.LookupService(db);
                return _lookupService;
            }
        }

        private ShipmentService.ShipmentService _shipmentService = null;
        protected ShipmentService.ShipmentService ShipmentService {
            get {
                if (_shipmentService == null) _shipmentService = new Evolution.ShipmentService.ShipmentService(db);
                return _shipmentService;
            }
        }

        private CompanyService.CompanyService _companyService = null;
        protected CompanyService.CompanyService CompanyService {
            get {
                if (_companyService == null) _companyService = new Evolution.CompanyService.CompanyService(db);
                return _companyService;
            }
        }

        private MembershipManagementService.MembershipManagementService _mms = null;
        protected MembershipManagementService.MembershipManagementService MembershipManagementService {
            get {
                if (_mms == null) _mms = new Evolution.MembershipManagementService.MembershipManagementService(db);
                return _mms;
            }
        }

        private ProductService.ProductService _productService = null;
        protected ProductService.ProductService ProductService {
            get {
                if (_productService == null) _productService = new ProductService.ProductService(db);
                return _productService;
            }
        }

        private DocumentService.DocumentService _documentService = null;
        protected DocumentService.DocumentService DocumentService {
            get {
                if (_documentService == null) _documentService = new DocumentService.DocumentService(db);
                return _documentService;
            }
        }

        private NoteService.NoteService _noteService = null;
        protected NoteService.NoteService NoteService {
            get {
                if (_noteService == null) _noteService = new NoteService.NoteService(db);
                return _noteService;
            }
        }

        private SupplierService.SupplierService _supplierService = null;
        protected SupplierService.SupplierService SupplierService {
            get {
                if (_supplierService == null) _supplierService = new SupplierService.SupplierService(db);
                return _supplierService;
            }
        }

        private AllocationService.AllocationService _allocationService = null;
        protected AllocationService.AllocationService AllocationService {
            get {
                if (_allocationService == null) _allocationService = new AllocationService.AllocationService(db);
                return _allocationService;
            }
        }

        private MediaService.MediaService _mediaService = null;
        protected MediaService.MediaService MediaServices {
            get {
                if (_mediaService == null) _mediaService = new MediaService.MediaService(db);
                return _mediaService;
            }
        }

        #endregion

        #region Construction

        protected IMapper Mapper = null;

        public PurchasingService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<PurchaseOrderHeader, PurchaseOrderHeader>();
                cfg.CreateMap<PurchaseOrderHeader, PurchaseOrderHeaderModel>();
                cfg.CreateMap<PurchaseOrderHeader, PurchaseOrderHeaderTemp>();
                cfg.CreateMap<PurchaseOrderHeaderModel, PurchaseOrderHeader>();
                cfg.CreateMap<PurchaseOrderHeaderModel, PurchaseOrderHeaderModel>();
                cfg.CreateMap<PurchaseOrderHeaderTemp, PurchaseOrderHeaderTempModel>();
                cfg.CreateMap<PurchaseOrderHeaderTemp, PurchaseOrderHeader>();
                cfg.CreateMap<PurchaseOrderHeaderTempModel, PurchaseOrderSummaryModel>();
                cfg.CreateMap<PurchaseOrderHeaderTempModel, PurchaseOrderHeaderTemp>();
                cfg.CreateMap<PurchaseOrderDetailModel, PurchaseOrderDetail>()
                                .ForSourceMember(s => s.OriginalRowId, t => t.Ignore())
                                .ForSourceMember(s => s.LineNumber, t => t.Ignore());
                cfg.CreateMap<PurchaseOrderDetail, PurchaseOrderDetail>();
                cfg.CreateMap<PurchaseOrderDetail, PurchaseOrderDetailSplitModel>();
                cfg.CreateMap<PurchaseOrderDetailModel, PurchaseOrderDetailModel>();
                cfg.CreateMap<PurchaseOrderDetailTemp, PurchaseOrderDetailTempModel>();
                cfg.CreateMap<PurchaseOrderDetailTempModel, PurchaseOrderDetailTemp>()
                                .ForSourceMember(s => s.OriginalRowId, t => t.Ignore())
                                .ForSourceMember(s => s.LineNumber, t => t.Ignore());
                cfg.CreateMap<PurchaseOrderDetailTempModel, PurchaseOrderDetailTempModel>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion

        #region Copying methods

        public PurchaseOrderHeaderTempModel CopyPurchaseOrderToTemp(CompanyModel company,
                                                                    PurchaseOrderHeaderModel purchaseOrderHeader, 
                                                                    UserModel user) {
            PurchaseOrderHeaderTempModel result = new PurchaseOrderHeaderTempModel();
            PurchaseOrderHeaderTemp poht = new PurchaseOrderHeaderTemp();

            // Clean the temp tables
            if (purchaseOrderHeader.Id > 0) {
                // Editing an existing order
                var poh = db.FindPurchaseOrderHeader(purchaseOrderHeader.Id);
                if (poh != null) {
                    // Copy the header
                    poht = db.FindPurchaseOrderHeaderTemps(company.Id)
                             .Where(p => p.UserId == user.Id &&
                                         p.OriginalRowId == purchaseOrderHeader.Id)
                             .FirstOrDefault();
                    if (poht != null) {
                        // Already exists in the temp tables so update it with the latest data
                        int tempId = poht.Id;
                        Mapper.Map<PurchaseOrderHeader, PurchaseOrderHeaderTemp>(poh, poht);
                        poht.Id = tempId;
                        poht.OriginalRowId = purchaseOrderHeader.Id;
                        poht.UserId = user.Id;

                        db.InsertOrUpdatePurchaseOrderHeaderTemp(poht);
                        result = mapToModel(poht);

                    } else {
                        // Doesn't exist, so copy
                        poht = Mapper.Map<PurchaseOrderHeader, PurchaseOrderHeaderTemp>(poh);
                        poht.Id = 0;
                        poht.OriginalRowId = purchaseOrderHeader.Id;
                        poht.UserId = user.Id;

                        db.InsertOrUpdatePurchaseOrderHeaderTemp(poht);
                        result = mapToModel(poht);
                    }

                    // Now copy/merge the details
                    db.CopyPurchaseOrderToTemp(company.Id, user.Id, purchaseOrderHeader.Id, poht.Id);

                    result.Splitable = db.FindPurchaseOrderDetailTemps(company.Id, poht.Id).Count() > 0;
                }

            } else {
                // New purchase
                poht.CompanyId = company.Id;
                poht.UserId = user.Id;
                poht.OrderNumber = purchaseOrderHeader.OrderNumber;
                poht.OrderDate = purchaseOrderHeader.OrderDate;
                poht.SalespersonId = purchaseOrderHeader.SalespersonId;
                poht.BrandCategoryId = purchaseOrderHeader.BrandCategoryId;
                poht.LocationId = purchaseOrderHeader.LocationId;
                poht.CancelMessage = purchaseOrderHeader.CancelMessage;

                db.InsertOrUpdatePurchaseOrderHeaderTemp(poht);
                result = mapToModel(poht);
            }

            return result;
        }

        public Error CopyTempToPurchaseOrderHeader(int purchaseOrderHeaderTempId, UserModel user, string lockGuid) {
            var error = new Error();
            PurchaseOrderHeader poh = null;

            var poht = db.FindPurchaseOrderHeaderTemp(purchaseOrderHeaderTempId);
            if (poht == null) {
                error.SetError(EvolutionResources.errRecordError, "OrderNumber", "PurchaseOrderHeaderTemp", purchaseOrderHeaderTempId.ToString());

            } else {
                if (poht.OriginalRowId != null && poht.OriginalRowId != 0) {
                    // Updating
                    if (!db.IsLockStillValid(typeof(PurchaseOrderHeader).ToString(), poht.OriginalRowId.Value, lockGuid)) {
                        error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "OrderNumber");

                    } else {
                        PurchaseOrderHeader before = null;

                        poh = db.FindPurchaseOrderHeader(poht.OriginalRowId.Value);
                        if (poh != null) {
                            // Found, so update
                            before = Mapper.Map<PurchaseOrderHeader, PurchaseOrderHeader>(poh);

                            Mapper.Map<PurchaseOrderHeaderTemp, PurchaseOrderHeader>(poht, poh);
                            poh.Id = poht.OriginalRowId.Value;

                        } else {
                            // Not found, so treat as new
                            poh = Mapper.Map<PurchaseOrderHeaderTemp, PurchaseOrderHeader>(poht);
                            poh.Id = 0;
                        }
                        db.InsertOrUpdatePurchaseOrderHeader(poh);

                        AuditService.LogChanges(typeof(PurchaseOrderHeader).ToString(), BusinessArea.PurchaseOrderDetails, user, before, poh);

                        // TBD: If a purchase order has changed, we need to send emails to
                        //      the account admin (according to the rules set up on the customer record)
                        //      to advise of the change.
                        //      This also needs to check allocations and advise sales staff if their
                        //      orders are going to be affected whether positively or negatively.



                    }

                } else {
                    // New record
                    poh = Mapper.Map<PurchaseOrderHeaderTemp, PurchaseOrderHeader>(poht);
                    poh.Id = 0;
                    db.InsertOrUpdatePurchaseOrderHeader(poh);
                    error.Id = poh.Id;

                    AuditService.LogChanges(typeof(PurchaseOrderHeader).ToString(), BusinessArea.PurchaseOrderDetails, user, null, poh);
                }

                // Now copy the details from temp to live tables
                if (!error.IsError) {
                    db.CopyTempToPurchaseOrder(poht.CompanyId, poht.Id, poh.Id);

                    // And perform allocations
                    AllocationService.AllocateOnPurchaseOrderSave(Mapper.Map<PurchaseOrderHeader, PurchaseOrderHeaderModel>(poh));
                }
            }
            return error;
        }

        #endregion
    }
}
