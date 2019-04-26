using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class ShipmentResultModel {
        public int ShipmentId { set; get; } = 0;        // Shipping Id
        public int? ShipmentContentId { set; get; } = null;
        public int PohId { set; get; } = 0;
        public int SupplierId { set; get; } = 0;
        public string SupplierName { set; get; } = "";
        public decimal OrderNumber { set; get; } = 0;
        public string ProductBrand { set; get; } = "";
        public decimal CBMEstimate { set; get; } = 0;
        public int ShippingMethodId { set; get; } = 0;
        public string ShippingMethod { set; get; } = "";
        public int PortDepartId { set; get; } = 0;
        public string PortDepart { set; get; } = "";
        public int PortArrivalId { set; get; } = 0;
        public string PortArrival { set; get; } = "";
        public DateTimeOffset? DatePreAlertETA { set; get; } = null;
        public string DatePreAlertETAISO { get { return DatePreAlertETA.ISODate(); } }
        public DateTimeOffset OrderDate;
        public string OrderDateISO { get { return OrderDate.ISODate(); } }
        public DateTimeOffset? DateDepartSupplier { set; get; } = null;
        public string DateDepartSupplierISO { get { return DateDepartSupplier.ISODate(); } }
        public DateTimeOffset? CancelDate { set; get; } = null;
        public string CancelDateISO { get { return CancelDate.ISODate(); } }
        public int POStatus { set; get; } = 0;
        public string POStatusText { set; get; } = "";
        public DateTimeOffset? SupplierInvoiceDate { set; get; } = null;
        public string SupplierInvoiceDateISO { get { return SupplierInvoiceDate.ISODate(); } }
        public string SeasonId { set; get; } = "";
        public string Season { set; get; } = "";
        public DateTimeOffset? RequiredShipDate { set; get; } = null;
        public string RequiredShipDateISO { get { return RequiredShipDate.ISODate(); } }
        public DateTimeOffset? DateExpDel { set; get; } = null;
        public string DateExpDelISO { get { return DateExpDel.ISODate(); } }
        public DateTimeOffset? Date100Shipped { set; get; } = null;
        public string Date100ShippedISO { get { return Date100Shipped.ISODate(); } }
        public DateTimeOffset? DateWarehouseAccept { set; get; } = null;
        public string DateWarehouseAcceptISO { get { return DateWarehouseAccept.ISODate(); } }
        public string CurrentStatus { set; get; } = "";
        public int SalesPersonId { set; get; } = 0;
        public string SalesPerson { set; get; } = "";
        public int BrandCategoryID { set; get; } = 0;
        public string BrandCategory { set; get; } = "";
    }

    public class ShipmentResultListModel : BaseListModel {
        public List<ShipmentResultModel> Items { set; get; } = new List<ShipmentResultModel>();
    }
}
