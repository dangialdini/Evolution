using Evolution.DAL;
using Evolution.Models.Models;
using AutoMapper;

namespace Evolution.SupplierService {
    public partial class SupplierService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public SupplierService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<Supplier, Supplier>();
                cfg.CreateMap<Supplier, SupplierModel>();
                cfg.CreateMap<SupplierModel, Supplier>();
                cfg.CreateMap<SupplierModel, SupplierModel>();
                cfg.CreateMap<SupplierAddress, SupplierAddress>();
                cfg.CreateMap<SupplierAddress, SupplierAddressModel>();
                cfg.CreateMap<SupplierAddressModel, SupplierAddress>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion
    }
}
