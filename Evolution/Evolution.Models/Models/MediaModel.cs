using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class MediaModel {
        public int Id { set; get; } = 0;
        public int? CompanyId { set; get; } = null;
        public DateTimeOffset? CreatedDate { set; get; } = null;
        public int? CreatedById { set; get; } = null;
        public DateTimeOffset? ModifiedDate { set; get; } = null;
        public int? ModifiedById { set; get; } = null;
        public int MediaTypeId { set; get; } = 0;
        public string Title { set; get; } = "";
        public string FolderName { set; get; } = "";
        public string FileName { set; get; } = "";
        public int? ImageW { set; get; } = null;
        public int? ImageH { set; get; } = null;

        // Additional fields
        public bool Lightboxable { set; get; } = false;
        public string MediaFile { set; get; } = "";
        public string MediaHtml { set; get; } = "";
    }
}
