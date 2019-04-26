using Evolution.DAL;
using Evolution.Models.Models;
using AutoMapper;

namespace Evolution.ShipmentService {
    public partial class ShipmentService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public ShipmentService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<Shipment, Shipment>();
                cfg.CreateMap<Shipment, ShipmentModel>();
                cfg.CreateMap<ShipmentModel, Shipment>()
                             .ForMember(s => s.CarrierVessel, t => t.Ignore());
                cfg.CreateMap<ShipmentContent, ShipmentContentModel>();
                cfg.CreateMap<ShipmentContentModel, ShipmentContent>();
                cfg.CreateMap<FindShippingRegister_Result, ShipmentResultModel>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion
    }
}
