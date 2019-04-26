using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class ProductComplianceModel {
        public int Id { set; get; } = 0;
        public int ProductId { set; get; }
        public int ComplianceCategoryId { set; get; }
        public int MarketId { set; get; }
        public List<ProductComplianceAttachmentModel> Attachments { set; get; } = new List<ProductComplianceAttachmentModel>();

        // Aditional fields
        public string ComplianceCategoryText { set; get; }
        public string MarketNameText { set; get; }
        public string AttachmentHtml { set; get; }
    }

    public class ProductComplianceAttachmentModel {
        public int Id { set; get; } = 0;
        public int ProductComplianceId { set; get; } = 0;
        public int MediaId { set; get; } = 0;

        // Additional fields
        public string FileName { set; get; }
        public string QualName { set; get; }
        public bool Selected { set; get; }
    }

    public class ProductComplianceListModel : BaseListModel {
        public List<ProductComplianceModel> Items { set; get; } = new List<ProductComplianceModel>();
    }
    
    public class ProductComplianceAttachmentListModel : BaseListModel {
        public List<ProductComplianceAttachmentModel> Items { get; set; } = new List<ProductComplianceAttachmentModel>();
    }
}
