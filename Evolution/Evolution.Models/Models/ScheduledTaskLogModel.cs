using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class ScheduledTaskLogModel {
        public int Id { set; get; } = 0;
        public int ScheduledTaskId { set; get; } = 0;
        public int Severity { set; get; } = 0;
        public string SeverityText { set; get; } = "";
        public DateTimeOffset? LogDate { set; get; } = null;
        public string LogDateISO { get { return LogDate.ISODate(); } }
        public string Message { set; get; } = "";

        // Additional fields
        public string Colour { set; get; } = "";
    }

    public class ScheduledTaskLogListModel : BaseListModel {
        public List<ScheduledTaskLogModel> Items { set; get; } = new List<ScheduledTaskLogModel>();
    }
}
