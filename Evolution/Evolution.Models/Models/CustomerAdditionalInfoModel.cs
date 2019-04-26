using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Enumerations;

namespace Evolution.Models.Models {
    public class CustomerAdditionalInfoModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string DeliveryInstructions { set; get; } = "";
        public string PricingInstructions { set; get; } = "";
        public bool PlacesForwardOrders { set; get; } = false;
        public int? RegionId { set; get; } = null;
        public int? ShippingTemplateId { set; get; } = null;
        public int ShippingTemplateType { set; get; } = (int)DocumentTemplateCategory.Invoice;		// 1=Invoice, 2=Packing Slip
        public string ProductLabelName { set; get; } = "";
        public int? UnassignedRetailInvoiceNumber { set; get; } = null;     // Default Retail Invoice No
        public string OurVendorId { set; get; } = "";
        public int? SourceId { set; get; } = null;
        public string EDI_VendorNo { set; get; } = "";
        public int? OrderTypeId { set; get; } = null;                       // Supply items by
    }
}
