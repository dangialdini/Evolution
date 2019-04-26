using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.Models.Models {
    public class NoteModel {
        public NoteModel() {
            for (int i = 0; i < 4; i++) UrlReferences.Add(new UrlReference());
        }

        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public NoteType NoteType { set; get; }
        public int ParentId { set; get; } = 0;
        public DateTimeOffset DateCreated { set; get; } = DateTimeOffset.Now;
        public string DateCreatedISO { get { return DateCreated.ISODate(); } }
        public int CreatedById { set; get; } = 0;
        public string CreatedBy { set; get; } = "";
        public string Subject { set; get; } = "";
        public string Message { set; get; } = "";
        public string Attachments { set; get; } = "";
        public IEnumerable<HttpPostedFileBase> Files { get; set; }
        public List<UrlReference> UrlReferences { set; get; } = new List<UrlReference>();
    }

    public class NoteListModel : BaseListModel {
        public List<NoteModel> Items { set; get; } = new List<NoteModel>();
    }

    public class UrlReference {
        public string Url { set; get; } = "";
        public string Description { set; get; } = "";
    }
}
