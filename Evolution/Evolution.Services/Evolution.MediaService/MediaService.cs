using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Configuration;
using System.Configuration;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;
using System.Drawing;

namespace Evolution.MediaService
{
    public partial class MediaService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public MediaService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<Medium, MediaModel>();
                cfg.CreateMap<MediaModel, Medium>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion
    }
}
