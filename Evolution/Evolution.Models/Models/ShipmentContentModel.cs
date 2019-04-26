using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class ShipmentContentModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int? ShipmentId { set; get; } = null;
        public int? PurchaseOrderHeaderId { set; get; } = null;
        public decimal? OrderNumber { set; get; } = null;       // Was PONo
        public double? CBMEstimate { set; get; } = 0;
        public double? CBMCharged { set; get; } = 0;
        public DateTimeOffset? DateDepartSupplier { set; get; }
        public string Comments { set; get; } = "";
        public int? SupplierId { set; get; } = null;
        public string ProductBrand { set; get; } = "";
        /*
        [PurchaseValue] [float] NULL,
	    [CurrencyId] [int] NULL,
	    [CostedExchangeRate] [real] NULL,
	    [DatePOSentToSupplier] [datetimeoffset](7) NULL,
	    [DateOrderConfirmed] [datetimeoffset](7) NULL,
	    [OrderConfirmationNo] [nvarchar](20) NULL,
        */
	    public double EstCBMRate { set; get; } = 0;
	    public double EstDutyRate { set; get; } = 0;
	    public double TotalLandedPrice { set; get; } = 0;
        /*
	    [MainShipInvNo] [nvarchar](25) NULL,
	    [MainShipInvDate] [datetimeoffset](7) NULL,
	    [MainShipAmountLocalEx] [float] NULL,
	    [MainShipCurrencyId] [int] NULL,
	    [MainShipExchangeRate] [float] NULL,
	    [MainShipAUDAmountEx] [float] NULL,
	    [MainShipGST] [float] NULL,
	    [OSChargesInvNo] [nvarchar](25) NULL,
	    [OSChargesInvDate] [datetimeoffset](7) NULL,
	    [OSChargesAmountLocalEx] [float] NULL,
	    [OSChargesCurrencyId] [int] NULL,
	    [OSChargesExchangeRate] [float] NULL,
	    [OSChargesAUDAmountEx] [float] NULL,
	    [OSChargesGSTAmount] [float] NULL,
	    [LocalHandInvNo] [nvarchar](25) NULL,
	    [LocalHandInvDate] [datetimeoffset](7) NULL,
	    [LocalHandAUDAmountEx] [float] NULL,
	    [LocalHandGSTAmount] [float] NULL,
	    [LocalShipInvNo] [nvarchar](25) NULL,
	    [LocalShipInvDate] [datetimeoffset](7) NULL,
	    [LocalShipAUDAmountEx] [float] NULL,
	    [LocalShipGSTAmount] [float] NULL,
	    [OtherShipInvNo] [nvarchar](25) NULL,
	    [OtherShipInvDate] [datetimeoffset](7) NULL,
	    [OtherShipAmountLocalEx] [float] NULL,
	    [OtherShipCurrencyId] [int] NULL,
	    [OtherShipExchangeRate] [float] NULL,
	    [OtherShipGSTAmount] [float] NULL,
	    [OtherShipAUDAmountEx] [float] NULL,
	    [MainGSTDutyInvNo] [nvarchar](20) NULL,
	    [MainGSTDutyInvDate] [datetimeoffset](7) NULL,
	    [MainGSTDutyNoDutyNoGSTAmount] [float] NULL,
	    [MainGSTDutyDutyAmount] [float] NULL,
	    [MainGSTDutyGSTAmount] [float] NULL,
	    [SupplierFreightLocalAmountEx] [float] NULL,
	    [SupplierFreightCurrencyID] [int] NULL,
	    [SupplierFreightAUDAmountEx] [float] NULL,
	    [ProdDevLocalAmountEx] [float] NULL,
	    [ProdDevLocalCurrencyId] [int] NULL,
	    [ProdDevAUDAmountEx] [float] NULL,
	    [PortToWarehouseInvNo] [nvarchar](20) NULL,
	    [PortToWarehouseAUDAmountEx] [float] NULL,
	    [LandingFreightActualAmount] [float] NULL,
	    [LandingDutyActualAmount] [float] NULL,
	    [LCSMYOBRate] [float] NULL,
	    [LCSMYOBRateTotal] [float] NULL,
	    [LCSMYOBDuty] [float] NULL,
	    [LCSMYOBDutyTotal] [float] NULL,
	    [UnspreadLCDiscrepancy] [float] NULL,
	    [SupplierInvoiceNo] [nvarchar](25) NULL,
	    [SupplierInvoiceDate] [datetimeoffset](7) NULL,
        */
        public double PackageWeight { set; get; } = 0;

        // Additional fields
        public string SupplierName { set; get; } = "";
    }

    public class ShipmentContentListModel : BaseListModel {
        public List<ShipmentContentModel> Items { set; get; } = new List<ShipmentContentModel>();
    }
}
