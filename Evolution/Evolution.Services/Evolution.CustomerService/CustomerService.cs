using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.CustomerService {
    public partial class CustomerService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public CustomerService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<CreditCard, CreditCardModel>();
                cfg.CreateMap<CreditCardModel, CreditCard>();
                cfg.CreateMap<Customer, Customer>();
                cfg.CreateMap<Customer, CustomerModel>();
                cfg.CreateMap<Customer, CustomerAdditionalInfoModel>();
                cfg.CreateMap<Customer, CustomerFreightModel>();
                cfg.CreateMap<CustomerFreightModel, Customer>();
                cfg.CreateMap<CustomerAdditionalInfoModel, Customer>();
                cfg.CreateMap<CustomerAddress, CustomerAddressModel>();
                cfg.CreateMap<CustomerAddressModel, CustomerAddress>();
                cfg.CreateMap<CustomerModel, Customer>();
                cfg.CreateMap<CustomerModel, CustomerModel > ();
                cfg.CreateMap<CustomerContact, CustomerContactModel>();
                cfg.CreateMap<CustomerContactModel, CustomerContact>();
                cfg.CreateMap<CustomerConflictSensitivity, CustomerConflictModel>();
                cfg.CreateMap<CustomerConflictModel, CustomerConflictSensitivity>();
                cfg.CreateMap<CustomerDefault, CustomerDefaultModel>();
                cfg.CreateMap<CustomerDefaultModel, CustomerDefault>();
                cfg.CreateMap<BrandCategorySalesPerson, BrandCategorySalesPerson>();
                cfg.CreateMap<BrandCategorySalesPerson, BrandCategorySalesPersonModel>();
                cfg.CreateMap<BrandCategorySalesPersonModel, BrandCategorySalesPerson>();
                cfg.CreateMap<MarketingGroupSubscription, CustomerMarketingModel>();
                cfg.CreateMap<CustomerMarketingModel, MarketingGroupSubscription>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion
    }
}
