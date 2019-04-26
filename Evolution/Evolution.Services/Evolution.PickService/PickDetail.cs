using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;

namespace Evolution.PickService {
    public partial class PickService {

        public List<PickDetailModel> FindPickDetailListModel(CompanyModel company, PickHeaderModel pickHeader) {
            var model = new List<PickDetailModel>();

            foreach(var item in db.FindPickDetails(company.Id, pickHeader.Id)) {
                model.Add(mapToModel(item));
            }

            return model;
        }
    }
}
