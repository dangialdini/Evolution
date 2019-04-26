using Evolution.DAL;
using Evolution.Models.Models;
using AutoMapper;

namespace Evolution.SystemService {
    public partial class SystemService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public SystemService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<Log, LogModel>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion
    }
}
