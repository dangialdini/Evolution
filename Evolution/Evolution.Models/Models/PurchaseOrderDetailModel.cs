using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Evolution.Resources;
using System.ComponentModel;

namespace Evolution.Models.Models {
    public class PurchaseOrderDetailModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int OriginalRowId { set; get; } = 0;
        public int PurchaseOrderHeaderId { set; get; } = 0;
        public int? LineNumber { set; get; } = null;
        public int? ProductId { set; get; } = null;
        [Required]
        [StringLength(255)]
        [Display(Name="lblProductDescription", ResourceType = typeof(EvolutionResources))]
        public string ProductDescription { set; get; } = "";
        [Required]
        [Range(0.01, 999999)]
        [Display(Name = "lblUnitPriceExTax", ResourceType = typeof(EvolutionResources))]
        public decimal? UnitPriceExTax { set; get; } = null;
        [Required]
        [Display(Name = "lblTaxCode", ResourceType = typeof(EvolutionResources))]
        public int TaxCodeId { set; get; } = 0;
        [Required]
        [Display(Name = "lblDiscountPercent", ResourceType = typeof(EvolutionResources))]
        public decimal? DiscountPercent { set; get; } = 0;
        [Required]
        [Range(1, Int32.MaxValue)]
        [Display(Name = "lblOrderQty", ResourceType = typeof(EvolutionResources))]
        public int? OrderQty { set; get; } = 0;
        //public int? LineStatus { set; get; } = null;
        //public bool IsReceived { set; get; } = false;
    }

    public class PurchaseOrderDetailListModel : BaseListModel {
        public List<PurchaseOrderDetailModel> Items { set; get; } = new List<PurchaseOrderDetailModel>();
    }
}
