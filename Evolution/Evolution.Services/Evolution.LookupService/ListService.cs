using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using AutoMapper;

namespace Evolution.LookupService {
    public partial class LookupService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public LookupService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<Country, CountryModel>();
                cfg.CreateMap<CountryModel, Country>();
                cfg.CreateMap<CountryModel, CountryModel>();
                cfg.CreateMap<Currency, CurrencyModel>();
                cfg.CreateMap<CurrencyModel, Currency>();
                cfg.CreateMap<CurrencyModel, CurrencyModel>();
                cfg.CreateMap<FreightCarrier, FreightCarrierModel>()
                                .ForMember(d => d.FreightCarrier, opts => opts.MapFrom(s => s.FreightCarrier1));
                cfg.CreateMap<FreightCarrierModel, FreightCarrier>()
                                .ForMember(d => d.FreightCarrier1, opts => opts.MapFrom(s => s.FreightCarrier));
                cfg.CreateMap<FreightCarrierModel, FreightCarrierModel>();
                cfg.CreateMap<FreightForwarder, FreightForwarderModel>();
                cfg.CreateMap<FreightForwarderModel, FreightForwarder>();
                cfg.CreateMap<FreightForwarderModel, FreightForwarderModel>();
                cfg.CreateMap<LOVItem, LOVItemModel>();
                cfg.CreateMap<LOVItemModel, LOVItem>();
                cfg.CreateMap<LOVItemModel, LOVItemModel>();
                cfg.CreateMap<Location, LocationModel>();
                cfg.CreateMap<LocationModel, Location>();
                cfg.CreateMap<LocationModel, LocationModel>();
                cfg.CreateMap<MarketingGroup, MarketingGroupModel>();
                cfg.CreateMap<MarketingGroupModel, MarketingGroup>();
                cfg.CreateMap<MarketingGroupModel, MarketingGroupModel>();
                cfg.CreateMap<MessageTemplate, MessageTemplateModel>();
                cfg.CreateMap<MessageTemplateModel, MessageTemplate>();
                cfg.CreateMap<PaymentTerm, PaymentTermModel>();
                cfg.CreateMap<PaymentTermModel, PaymentTerm>();
                cfg.CreateMap<Port, PortModel>();
                cfg.CreateMap<PortModel, Port>();
                cfg.CreateMap<PortModel, PortModel>();
                cfg.CreateMap<PriceLevel, PriceLevelModel>();
                cfg.CreateMap<PriceLevelModel, PriceLevel>();
                cfg.CreateMap<PriceLevelModel, PriceLevelModel>();
                cfg.CreateMap<PurchaseOrderHeaderStatu, PurchaseOrderHeaderStatusModel>();
                cfg.CreateMap<PurchaseOrderHeaderStatusModel, PurchaseOrderHeaderStatu>();
                cfg.CreateMap<PurchaseOrderHeaderStatusModel, PurchaseOrderHeaderStatusModel>();
                cfg.CreateMap<Region, RegionModel>();
                cfg.CreateMap<RegionModel, Region>();
                cfg.CreateMap<RegionModel, RegionModel>();
                cfg.CreateMap<SalesOrderHeaderStatu, SalesOrderHeaderStatusModel>();
                cfg.CreateMap<SalesOrderHeaderStatusModel, SalesOrderHeaderStatu>();
                cfg.CreateMap<DocumentTemplate, DocumentTemplateModel>();
                cfg.CreateMap<DocumentTemplateModel, DocumentTemplate>();
                cfg.CreateMap<DocumentTemplateModel, DocumentTemplateModel>();
                cfg.CreateMap<SupplierTerm, SupplierTermModel>();
                cfg.CreateMap<SupplierTermModel, SupplierTerm>();
                cfg.CreateMap<SupplierTermModel, SupplierTermModel>();
                cfg.CreateMap<TaxCode, TaxCodeModel>()
                                .ForMember(d => d.TaxCode, opts => opts.MapFrom(s => s.TaxCode1));
                cfg.CreateMap<TaxCodeModel, TaxCode>()
                                .ForMember(d => d.TaxCode1, opts => opts.MapFrom(s => s.TaxCode));
                cfg.CreateMap<TaxCodeModel, TaxCodeModel>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion
    }
}
