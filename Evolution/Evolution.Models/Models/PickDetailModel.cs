using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class PickDetailModel {
        public int Id { set; get; }
        public int? PickHeaderId { set; get; }
        public int? ProductId { set; get; }
        public int? QtyToPick { set; get; }
        public int? QtyPicked { set; get; }
        public int? PickDetailStatusId { set; get; }
        public int SalesOrderDetailId { set; get; }
        public int? PickLocationId { set; get; }
        public bool IsReportedToWebsite { set; get; }
    }
}
