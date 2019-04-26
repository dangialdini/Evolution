using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class AuditModel {
        public int Id { set; get; } = 0;
        public DateTimeOffset ChangedDate { set; get; }
        public string ChangedDateISO { get { return ChangedDate.ISODate(); } }
        public string TableName { set; get; } = "";
        public string BusinessArea { set; get; } = "";
        public int RowId { set; get; } = 0;
        public int UserId { set; get; } = 0;
        public string FieldName { set; get; } = "";
        public string BeforeValue { set; get; } = "";
        public string AfterValue { set; get; } = "";
    }

    public class AuditListModel : BaseListModel {
        public List<AuditModel> Items { set; get; } = new List<AuditModel>();
    }
}
