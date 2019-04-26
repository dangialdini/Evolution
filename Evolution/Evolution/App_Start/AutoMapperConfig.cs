using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Evolution.DAL;
using Evolution.Models.Models;
using AutoMapper;

namespace Evolution {
    public static class AutoMapperConfig {
        public static void RegisterMappings() {

            Mapper.Initialize(cfg => {
                //cfg.CreateMissingTypeMaps = true;     // Create missing mappings 'on the fly'
                cfg.CreateMap<Company, CompanyModel>();
            });
        }
    }
}