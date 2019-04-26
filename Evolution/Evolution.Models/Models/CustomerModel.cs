using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class CustomerModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public DateTimeOffset? CreatedDate { set; get; }
        public string CreatedDateISO { get { return CreatedDate.ISODate(); } }
        public int? CreatedById { set; get; } = null;
        public int CardRecordId { set; get; } = 0;
        public string Name { set; get; } = "";
        public int CurrencyId { set; get; } = 0;
        public int? CustomerTypeId { set; get; } = null;
        public int? PotentialId { set; get; } = null;
        public int? MiscGroupingId { set; get; } = null;
        public int PaymentTermId { set; get; } = 0;
        public int PriceLevelId { set; get; } = 0;
        public int TaxCodeId { set; get; } = 0;
        public int? FreightTaxCodeId { set; get; } = null;
        public bool UseCustomerTaxCode { set; get; } = false;
        public decimal? CreditLimit { set; get; } = null;
        public decimal? VolumeDiscount { set; get; } = null;
        public int? ShippingMethodId { set; get; } = null;
        public bool OnHold { set; get; } = false;
        public decimal? FreightRate { set; get; } = null;
        public decimal? MinFreightPerOrder { set; get; } = null;
        public int? FreightCarrierId { set; get; } = null;
        public bool SendInvoices { set; get; } = false;
        public bool EmailAcctMgrOnSaleChange { set; get; } = false;
        public bool EmailAcctMgrOnLinkedPurchaseChange { set; get; } = false;
        public bool RequireAuthorisation4OrderQtyChange { set; get; } = false;
        public bool AllowSalesInNonMSQMultiples { set; get; } = false;
        public bool SendPOSFile { set; get; } = false;
        public bool RemoveCustNameFromAddressWhenDrop { set; get; } = false;
        public bool ExcludeFromSalesGraphs { set; get; } = false;
        public decimal? MinFreightThreshold { set; get; } = null;
        public decimal? FreightWhenBelowThreshold { set; get; } = null;
        public string TaxId { set; get; } = "";         // This is the ABN, VAT Reg No etc
        public string PricingInstructions { set; get; } = "";
        public bool SyncToMYOB { set; get; } = false;
        public bool InMYOB { set; get; } = false;
        public bool PlacesForwardOrders { set; get; } = false;
        //public int? RegionId { set; get; } = null;        // Moved to CustomerAdditionalInfo
        public string OurVendorId { set; get; } = "";
        public string ConflictSensitivityNotes { set; get; } = "";
        public double? CurrentBalance { set; get; } = null;
        public double? OpenOrders { set; get; } = null;
        public bool Enabled { set; get; } = false;

        // Additional fields
        public string RegionText { set; get; } = "";
    }

    public class CustomerListModel : BaseListModel {
        public List<CustomerModel> Items { set; get; } = new List<CustomerModel>();
    }
}
