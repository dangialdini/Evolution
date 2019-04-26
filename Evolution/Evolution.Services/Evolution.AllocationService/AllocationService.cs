using System.Linq;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using AutoMapper;

namespace Evolution.AllocationService {
    public partial class AllocationService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public AllocationService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<FindAllocations_Result, AllocationResultModel>();
                cfg.CreateMap<Allocation, AllocationModel>();
                cfg.CreateMap<AllocationModel, Allocation>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion

        #region Private members
        #endregion
    }
}
