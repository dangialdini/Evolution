using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Enumerations;
using AutoMapper;

namespace Evolution.DataTransferService {
    public partial class DataTransferService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public DataTransferService(EvolutionEntities dbEntities, CompanyModel company = null) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<FileTransferConfiguration, FileTransferConfigurationModel>();
                cfg.CreateMap<FileTransferConfigurationModel, FileTransferConfiguration>()
                    .ForMember(s => s.Id, opt => opt.Ignore());
            }));

            Mapper = config.CreateMapper();
        }

        #endregion
    }
}
