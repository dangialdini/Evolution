using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Evolution.Resources;

namespace Evolution.Models.Models {
    public class ProductModel {
        public int Id { set; get; } = 0;
        public int? BrandId { set; get; } = 0;
        public DateTimeOffset? CreatedDate { set; get; } = null;
        public int? CreatedById { set; get; } = 0;
        public DateTimeOffset? ApprovedDate { set; get; } = null;
        public int? ApprovedById { set; get; } = null;
        [Required]
        [StringLength(30)]
        [Display(Name = "lblProductNumber", ResourceType = typeof(EvolutionResources))]
        public string ItemNumber { set; get; } = "";
        [Required]
        [StringLength(30)]
        [Display(Name = "lblProductName", ResourceType = typeof(EvolutionResources))]
        public string ItemName { set; get; } = "";
        [StringLength(81)]
        [Display(Name = "lblProductNameLong", ResourceType = typeof(EvolutionResources))]
        public string ItemNameLong { set; get; } = "";
        [StringLength(40)]
        [Display(Name = "lblProductNameFormat", ResourceType = typeof(EvolutionResources))]
        public string ItemNameFormat { set; get; } = "";
        [StringLength(40)]
        [Display(Name = "lblProductNameStyle", ResourceType = typeof(EvolutionResources))]
        public string ItemNameStyle { set; get; } = "";
        public int? AB { set; get; } = null;
        [StringLength(10)]
        [Display(Name = "lblSet", ResourceType = typeof(EvolutionResources))]
        public string Set { set; get; } = "";
        public double? QuantityOnHand { set; get; } = null;
        [AllowHtml]
        [StringLength(255)]
        public string ItemDescription { set; get; } = "";
        [StringLength(31)]
        [Display(Name = "lblBarcode", ResourceType = typeof(EvolutionResources))]
        public string BarCode { set; get; } = "";       // Product barcode
        [StringLength(31)]
        [Display(Name = "lblInnerBarcode", ResourceType = typeof(EvolutionResources))]
        public string InnerBarCode { set; get; } = "";
        [StringLength(31)]
        [Display(Name = "lblMasterBarcode", ResourceType = typeof(EvolutionResources))]
        public string MasterBarCode { set; get; } = "";
        [Range(0, 99)]
        public int? HSCode { set; get; } = null;
        public string Picture { set; get; } = "";
        public double? UnitCBM { set; get; } = 0;
        public int? MinSaleQty { set; get; } = null;
        [Range(0, 9999999)]
        public int? MinOrderQty1 { set; get; } = null;
        [Range(0, 9999999)]
        public int? MinOrderQty2 { set; get; } = null;
        public int? PrimarySupplierId { set; get; } = null;
        public string SupplierItemNumber { set; get; } = "";
        public string SupplierItemName { set; get; } = "";
        public int? ManufacturerId { set; get; } = null;
        public int? ProductStatus { set; get; } = null;
        public int? PrimaryMediaId { set; get; } = null;

        public double? Length { set; get; } = null;
        public double? Width { set; get; } = null;
        public double? Height { set; get; } = null;
        public double? Weight { set; get; } = null;

        public double? PackedLength { set; get; } = null;
        public double? PackedWidth { set; get; } = null;
        public double? PackedHeight { set; get; } = null;
        public double? PackedWeight { set; get; } = null;

        public double? InnerLength { set; get; } = null;
        public double? InnerWidth { set; get; } = null;
        public double? InnerHeight { set; get; } = null;
        public double? InnerWeight { set; get; } = null;

        public double? MasterLength { set; get; } = null;
        public double? MasterWidth { set; get; } = null;
        public double? MasterHeight { set; get; } = null;
        public double? MasterWeight { set; get; } = null;
        public double? WeightPerProduct { set; get; } = null;

        public int? MaterialId { set; get; } = null;
        public int? CountryOfOriginId { set; get; }
        [Range(0, 9999999)]
        public int? InnerQuantity { set; get; } = null;
        [Range(0, 9999999)]
        public int? OuterQuantity { set; get; } = null;
        [Range(0, 9999999)]
        public int? MasterQuantity { set; get; } = null;
        public int? PackingTypeId { set; get; } = null;
        public bool Enabled { set; get; } = false;
        public int? WebCategoryId { set; get; } = null;
        public int? WebSubCategoryId { set; get; } = null;
        public int? ProductAvailabilityId { set; get; } = null;

        public ProductAdditionalCategoryModel AdditionalCategory { set; get; } = new ProductAdditionalCategoryModel();

        // Additional properties
        public string BrandName { set; get; } = "";
        public string CreatedByText { set; get; } = "";
        public string ApprovedByText { set; get; } = "";
        public string Category { set; get; } = "";
        public string SupplierName { set; get; } = "";
        public int? TaxCodeId { set; get; } = 0;
        public string ProductAvailabilityText { set; get; } = " ";
        public string ProductStatusText { set; get; } = " ";
        public string PictureHtml { set; get; } = "";
        public string Dimensions {
            get {
                string result = "";
                if(Length != null && Width != null && Height != null) {
                    result = $"{Length.Value} x {Width.Value} x {Height.Value} cm";
                }
                return result;
            }
        }
        public string BarCodeFile1 { set; get; } = "";
        public string BarCodeFile2 { set; get; } = "";
        public string BarCodeFile3 { set; get; } = "";
        public string MaterialText { set; get; } = "";
    }

    public class ProductListModel : BaseListModel {
        public List<ProductModel> Items { set; get; } = new List<ProductModel>();
    }
}
