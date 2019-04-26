using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class SaleNoteModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int SaleId { set; get; } = 0;
        public DateTimeOffset DateCreated { set; get; }
        public string DateCreatedISO { get { return DateCreated.ISODate(); } }
        public int CreatedById { set; get; } = 0;
        public string CreatedBy { set; get; } = "";
        public string Subject { set; get; } = "";
        public string Message { set; get; } = "";
        public string Attachments { set; get; } = "";
        public IEnumerable<HttpPostedFileBase> Files { get; set; }
    }

    public class SaleNoteListModel : BaseListModel {
        public List<SaleNoteModel> Items { set; get; } = new List<SaleNoteModel>();
    }
}
