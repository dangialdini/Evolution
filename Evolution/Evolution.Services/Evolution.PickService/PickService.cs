using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using AutoMapper;

namespace Evolution.PickService {
    public partial class PickService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public PickService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<PickHeader, PickHeaderModel>();
                cfg.CreateMap<PickHeaderModel, PickHeader>();
                cfg.CreateMap<PickDetail, PickDetailModel>();
                cfg.CreateMap<PickDetailModel, PickDetail>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion
    }
}
