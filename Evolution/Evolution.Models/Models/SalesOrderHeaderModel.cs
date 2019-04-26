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
    public class SalesOrderHeaderModel {
        // Additional fields (placed here to maintain screen validation message order)
        [Required]
        [MinLength(2)]
        [Display(Name = "lblCustomerName", ResourceType = typeof(EvolutionResources))]
        public string CustomerName { set; get; } = "";

        // Main fields
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        //[dSource] [int] NULL,
        public int? SourceId { set; get; } = null;
        public int? CustomerId { set; get; } = null;
        [Display(Name = "lblOrderNumber", ResourceType = typeof(EvolutionResources))]
        public int OrderNumber { set; get; } = 0;
        [StringLength(255)]
        [Display(Name = "lblCustomerPO", ResourceType = typeof(EvolutionResources))]
        public string CustPO { set; get; } = "";
        [Display(Name = "lblOrderDate", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? OrderDate { set; get; }
        [Display(Name = "lblAdvUSFinal", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? RequiredDate { set; get; }                   // Avd US Final
        [Display(Name = "lblCompletedDate", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? CompletedDate { set; get; }
        [Required]
        [StringLength(255)]
        [Display(Name = "lblShippingAddress", ResourceType = typeof(EvolutionResources))]
        public string ShipAddress1 { set; get; } = "";
        public string ShipAddress2 { set; get; } = "";
        public string ShipAddress3 { set; get; } = "";
        public string ShipAddress4 { set; get; } = "";
        [Required]
        [StringLength(60)]
        [Display(Name = "lblSuburb", ResourceType = typeof(EvolutionResources))]
        public string ShipSuburb { set; get; } = "";
        [Required]
        [StringLength(20)]
        [Display(Name = "lblState", ResourceType = typeof(EvolutionResources))]
        public string ShipState { set; get; } = "";
        [Required]
        [StringLength(12)]
        [Display(Name = "lblPostCode", ResourceType = typeof(EvolutionResources))]
        public string ShipPostcode { set; get; } = "";
        public int? SOStatus { set; get; } = null;
        public int? SOSubstatus { set; get; } = null;
        public int? SalespersonId { set; get; } = null;
        //[OrderMemo] [nvarchar](255) NULL,
        //[OrderComment] [nvarchar](255) NULL,
        //[ReferralSource] [int] NULL,
        public int? LocationId { set; get; } = null;
        public int? TermsId { set; get; } = null;
        //[OrderBalance] [decimal](18, 4) NULL,
        public int? ShippingMethodId { set; get; } = null;
        [Required]
        [Display(Name = "lblDeliveryWindowOpen", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? DeliveryWindowOpen { set; get; }
        [Required]
        [Display(Name = "lblDeliveryWindowClose", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? DeliveryWindowClose { set; get; }
        //public bool ManualDWO { set; get; } = false;      // Superceded by ManualDWSet
        //public bool ManualDWC { set; get; } = false;
        public bool ManualDWSet { set; get; } = false;
        [StringLength(25)]
        public string ShipMethodAccount { set; get; } = "";
        public int? NextActionId { set; get; } = null;
        public bool IsConfirmedAddress { set; get; } = false;
        public int? StockTransferNo { set; get; } = null;
        public bool IsManualFreight { set; get; } = false;
        [Display(Name = "lblFreightRate", ResourceType = typeof(EvolutionResources))]
        public decimal? FreightRate { set; get; } = null;
        [Display(Name = "lblMinFreightPerOrder", ResourceType = typeof(EvolutionResources))]
        public decimal? MinFreightPerOrder { set; get; } = null;
        public int? FreightCarrierId { set; get; } = null;
        [StringLength(30)]
        public string DeliveryInstructions { set; get; } = "";
        [StringLength(30)]
        public string DeliveryContact { set; get; } = "";
        public int? ShipCountryId { set; get; } = null;
        [Display(Name = "lblHeldUntil", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? OrderHoldExpiryDate { set; get; }
        [StringLength(30)]
        public string SignedBy { set; get; } = "";
        [Display(Name = "lblDateSigned", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? DateSigned { set; get; }
        public int? MethodSignedId { set; get; } = null;
        public int? PrintedForm { set; get; } = null;         // Shipping template
        [Display(Name = "lblOARChangeDate", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? OARChangeDate { set; get; }
        [StringLength(100)]
        public string WarehouseInstructions { set; get; } = "";
        //[IsMSQProblem] [bit] NOT NULL,
        [StringLength(52)]
        [Display(Name = "lblEndUserName", ResourceType = typeof(EvolutionResources))]
        public string EndUserName { set; get; } = "";
        public bool IsOverrideMSQ { set; get; } = false;
        [Display(Name = "lblNextReviewDate", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? NextReviewDate { set; get; }
        public int? FreightTermId { set; get; } = null;
        public int? CreditCardId { set; get; } = null;
        //[WebsiteOrderNo] [nvarchar](20) NULL,
        //[WebsiteTransactionID] [nvarchar](20) NULL,
        //[SiteName] [nvarchar](50) NULL,
        //[FreightExGST] [decimal](10, 2) NULL,
        //[FreightGST] [decimal](10, 2) NULL,
        //[IsProcessed] [bit] NOT NULL,
        public bool IsRetailSale { set; get; }
        public bool IsRetailHoldingOrder { set; get; } = false;
        [StringLength(30)]
        public string EDI_Department { set; get; } = "";
        [StringLength(30)]
        public string EDI_DCCode { set; get; } = "";
        [StringLength(30)]
        public string EDI_StoreNo { set; get; } = "";
        public int? OrderTypeId { set; get; } = null;
        public int? BrandCategoryId { set; get; } = null;
        public DateTimeOffset? DateCreated { set; get; }
        public DateTimeOffset? DateModified { set; get; }

        // Additional fields
        public List<SalesOrderDetailModel> SalesOrderDetails = new List<SalesOrderDetailModel>();

        public string OrderDateISO { get { return OrderDate.ISODate(); } }
        public string RequiredDateISO { get { return RequiredDate.ISODate(); } }                   // Avd US Final
        public string CompletedDateISO { get { return CompletedDate.ISODate(); } }
        public string SalesPersonName { set; get; } = "";
        public string SOStatusText { set; get; } = "";
        public string SOSubStatusText { set; get; } = "";
        public SalesOrderHeaderStatus? SOStatusValue { set; get; } = null;
        public string DeliveryWindowOpenISO { get { return DeliveryWindowOpen.ISODate(); } }
        public string DeliveryWindowCloseISO { get { return DeliveryWindowClose.ISODate(); } }
        public string NextActionText { set; get; } = "";
        public string BrandCategoryText { set; get; } = "";
        public string RegionText { get; set; } = "";
        public string CountryText { get; set; } = "";
        public string LocationText { get; set; } = "";
        public string SourceText { set; get; } = "";
        public string OrderNumberUrl { set; get; } = "";
        public string FullAddress {
            get {
                string rc = ShipAddress1;
                if (!string.IsNullOrEmpty(ShipAddress2)) rc += "\r\n" + ShipAddress2;
                if (!string.IsNullOrEmpty(ShipAddress3)) rc += "\r\n" + ShipAddress3;
                if (!string.IsNullOrEmpty(ShipAddress4)) rc += "\r\n" + ShipAddress4;
                if (!string.IsNullOrEmpty(ShipSuburb)) rc += "\r\n" + ShipSuburb;
                if (!string.IsNullOrEmpty(ShipState)) rc += "\r\n" + ShipState;
                if (!string.IsNullOrEmpty(ShipPostcode)) rc += "\r\n" + ShipPostcode;
                return rc;
            }
        }

    }

    public class SalesOrderHeaderListModel : BaseListModel {
        public List<SalesOrderHeaderModel> Items { set; get; } = new List<SalesOrderHeaderModel>();
    }
}
