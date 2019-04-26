using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class LogModel {
        public int Id { set; get; } = 0;
        public int LogSection { set; get; } = 0;
        public int Severity { set; get; } = 0;
        public string SeverityText { set; get; } = "";
        public DateTimeOffset? LogDate { set; get; } = null;
        public string LogDateISO { get { return LogDate.ISODate(); } }
        public string Url { set; get; } = "";
        public string Message { set; get; } = "";

        // Additional fields
        public string Colour { set; get; } = "";
    }

    public class LogListModel : BaseListModel {
        public List<LogModel> Items { set; get; } = new List<LogModel>();
    }
}
