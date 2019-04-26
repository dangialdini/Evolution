using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class ShipmentModel {
        public int Id { set; get; } = 0;
        public int OriginalRowId { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int? CarrierVesselId { set; get; } = null;
        public string VoyageNo { set; get; } = "";
        public int? ShippingMethodId { set; get; } = null;
        public int? PortDepartId { set; get; } = null;
        public int? PortArrivalId { set; get; } = null;
        public DateTimeOffset? Date100Shipped { set; get; } = null;
        public DateTimeOffset? DatePreAlertETA { set; get; } = null;    // Landing date
        public DateTimeOffset? DateWarehouseAccept { set; get; } = null;
        public string ConsolidationName { set; get; } = "";
        public string Comments { set; get; } = "";
        public string ConsignmentNo { set; get; } = "";
        public int? SeasonId { set; get; } = null;
        public bool IsTranshipment { set; get; } = false;
        public double? CBMCharged { set; get; } = 0;
        public string ContainerNo { set; get; } = "";
        public int? CountPackages { set; get; } = null;
        public DateTimeOffset? DateUnpackSlipRcvd { set; get; } = null;
        public DateTimeOffset? DateExpDel { set; get; } = null;

        // Additional fields
        public double? CBMEstimate { set; get; } = 0;
        public string CarrierVesselText { set; get; } = "";
        public string ShippingMethodText { set; get; } = "";
        public string PortDepartText { set; get; } = "";
        public string PortArrivalText { set; get; } = "";
        public string SeasonText { set; get; } = "";
    }

    public class ShipmentListModel : BaseListModel {
        public List<ShipmentModel> Items { set; get; } = new List<ShipmentModel>();
    }
}
