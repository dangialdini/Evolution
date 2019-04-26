using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Evolution.Resources;

namespace Evolution.Models.Models {
    public class CustomerDefaultModel {
        public int Id { set; get; }
        public int CompanyId { set; get; }
        public int? CountryId { set; get; }
        [StringLength(20)]
        [Display(Name = "lblPostCode", ResourceType = typeof(EvolutionResources))]
        public string Postcode { set; get; }
        public int? CurrencyId { set; get; }
        public int? TermId { set; get; }
        public int? PriceLevelId { set; get; }
        public int? TaxCodeId { set; get; }
        public int? TaxCodeWithoutTaxId { set; get; }
        public decimal? CreditLimit { set; get; }
        public decimal? VolumeDiscount { set; get; }
        public int? PrintedForm { set; get; }
        //public int? ShippingMethodId { set; get; }
        public int? SalespersonId { set; get; }
        public int? FreightCarrierId { set; get; }
        public bool IsManualFreight { set; get; }
        public bool OnHold { set; get; } = false;
        public bool SendInvoices { set; get; } = false;
        public bool EmailAcctMgrOnSaleChange { set; get; } = false;
        public bool EmailAcctMgrOnLinkedPurchaseChange { set; get; } = false;
        public bool RequireAuthorisation4OrderQtyChange { set; get; } = false;
        public bool AllowSalesInNonMSQMultiples { set; get; } = false;
        //public string ProductLabelName { set; get; }
        public bool SendPOSFile { set; get; } = false;
        public bool RemoveCustNameFromAddressWhenDrop { set; get; } = false;
        public bool ExcludeFromSalesGraphs { set; get; } = false;
        public int? CustomerTypeId { set; get; }
        public decimal? FreightRate { set; get; }
        public decimal? MinFreightPerOrder { set; get; }
        public decimal? MinFreightThreshold { set; get; }
        public decimal? FreightWhenBelowThreshold { set; get; }
        public int? DefaultTemplateType { set; get; }       // 1=Invoice, 2=P/Slip
        public int? DefaultTemplateId { set; get; }
        public bool Enabled { set; get; } = true;

        // Additional properties
        public string CountryNameText { set; get; }
        public string CurrencyCodeText { set; get; }
        public string CustomerTypeText { set; get; }
        public string SalesPersonText { set; get; }
        public string PaymentTermsText { set; get; }
        public string TaxCodeText { set; get; }
        public string TaxCodeWithoutTaxIdText { set; get; }
    }

    public class CustomerDefaultListModel : BaseListModel {
        public List<CustomerDefaultModel> Items { set; get; } = new List<CustomerDefaultModel>();
    }
}
