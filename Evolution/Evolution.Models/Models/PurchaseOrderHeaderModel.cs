using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;

namespace Evolution.Models.Models {
    public class PurchaseOrderHeaderModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        //[dSource] [int] NULL,
        //[SourceId] [int] NULL,
        [Required]
        public int? SupplierId { set; get; } = null;
        public string SupplierName { set; get; } = "";          // Auto filled
        [Required]
        [Display(Name = "lblOrderNumber", ResourceType = typeof(EvolutionResources))]
        public decimal? OrderNumber { set; get; } = 0;
        [StringLength(255)]
        [Display(Name = "lblSupplierInvoiceNo", ResourceType = typeof(EvolutionResources))]
        public string SupplierInv { set; get; } = "";                       // Supplier Invoice Number
        [Required]
        [Display(Name = "lblOrderDate", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? OrderDate { set; get; }
        [Display(Name = "lblAdvUSFinal", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? RequiredDate { set; get; }                   // Avd US Final / Advertised ETA
        [Display(Name = "lblCompletedDate", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? CompletedDate { set; get; }
        [StringLength(255)]
        public string ShipAddress1 { set; get; } = "";
        [StringLength(255)]
        public string ShipAddress2 { set; get; } = "";
        [StringLength(255)]
        public string ShipAddress3 { set; get; } = "";
        [StringLength(255)]
        public string ShipAddress4 { set; get; } = "";
        //[ShipSuburb] [nvarchar](60) NULL,
        //[ShipState] [nvarchar](20) NULL,
        //[ShipPostcode] [nvarchar](12) NULL,
        public int POStatus { set; get; } = 0;
        public int? SalespersonId { set; get; } = null;
        //[OrderMemo] [nvarchar](255) NULL,
        [StringLength(255)]
        public string OrderComment { set; get; } = "";
        //[ReferralSource] [int] NULL,
        public int? LocationId { set; get; } = null;
        public int? CurrencyId { set; get; } = null;
        public int? PaymentTermsId { set; get; } = null;                    // Was 'TermsId'
        //[OrderBalance] [decimal](18, 4) NULL,
        //[UnpackedBy] [int] NULL,
        //[EnteredBy] [int] NULL,
        //[UnpackCommenced] [datetimeoffset](7) NULL,
        //[UnpackCompleted] [datetimeoffset](7) NULL,
        //[ReceiptedIntoStock] [datetimeoffset](7) NULL,
        [Display(Name = "lblSRDFinal", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? RequiredShipDate { set; get; } = null;       // SRD Final / Stock Ready Date 
        public DateTimeOffset? CancelDate { set; get; } = null;
        [StringLength(2048)]
        [Display(Name = "lblCancelMessage", ResourceType = typeof(EvolutionResources))]
        public string CancelMessage { set; get; } = "";
        public bool StockOrder { set; get; } = false;
        public int? CommercialTermsId { set; get; } = null;
        public int? FreightForwarderId { set; get; } = null;
        public int? PortId { set; get; } = null;                            // Departure Port
        public int? ShipMethodId { set; get; } = null;
        [Display(Name = "lblRealisticETA", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? RealisticRequiredDate { set; get; } = null;  // Realistic ETA / Unpack Date
        public int? StockTransferNo { set; get; } = null;
        public DateTimeOffset? DateSentToWhs { set; get; } = null;          // Date sent to warehouse
        public DateTimeOffset? DateSentToFF { set; get; } = null;           // Date sent to freight forwarder
        [Range(0.00, 999999)]
        [Display(Name = "lblExchangeRate", ResourceType = typeof(EvolutionResources))]
        public decimal? ExchangeRate { set; get; } = null;
        [Display(Name = "lblAdvUSInitial", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? RequiredDate_Original { set; get; } = null;  // Avd US Initial
        [Display(Name = "lblDatePOSentToSupplier", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? DatePOSentToSupplier { set; get; } = null;
        [Display(Name = "lblDateOrderConfirmed", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? DateOrderConfirmed { set; get; } = null;
        [StringLength(20)]
        [Display(Name = "lblOrderConfirmationNo", ResourceType = typeof(EvolutionResources))]
        public string OrderConfirmationNo { set; get; } = "";
        [Display(Name = "lblSupplierInvoiceDate", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? SupplierInvoiceDate { set; get; } = null;
        public bool IsFirstOrder { set; get; } = false;
        public int? PortArrivalId { set; get; } = null;
	    public bool IsDeposit2BePaid { set; get; } = false;
        [Display(Name = "lblSRDInitial", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? RequiredShipDate_Original { set; get; } = null;  // SRD Initial
        //[LateReason1] [nvarchar](50) NULL,
        //[LatenessFaultPercentage1] [real] NULL,
        //[LateReason2] [nvarchar](50) NULL,
        //[LatenessFaultPercentage2] [real] NULL,
        public int? ContainerTypeId { set; get; } = null;
        public int? BrandCategoryId { set; get; } = null;

        // Additional fields
        public string SalesPersonName { set; get; } = "";
        public string OrderDateISO { get { return OrderDate.ISODate(); } }
        public string RequiredDateISO { get { return RequiredDate.ISODate(); } }    // Avd US Final / Advertised Unpack Slip Final
        public string CompletedDateISO { get { return CompletedDate.ISODate(); } }
        public DateTimeOffset? LandingDate { set; get; }                    // Calculated from other table lookups
        public string LandingDateISO { get { return LandingDate.ISODate(); } }
        public string RealisticRequiredDateISO { get { return RealisticRequiredDate.ISODate(); } } // Realistic ETA
        public string POStatusText { set; get; } = "";
        public PurchaseOrderStatus POStatusValue { set; get; }
        public bool Splitable { set; get; } = false;
        public string OrderNumberUrl { set; get; } = "";
    }

    public class PurchaseOrderHeaderListModel : BaseListModel {
        public List<PurchaseOrderHeaderModel> Items { set; get; } = new List<PurchaseOrderHeaderModel>();
    }
}
