using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class DataTransferQueueModel {
        public int Id { set; get; } = 0;
        public DateTimeOffset QueuedDate { set; get; }
        public String QueuedDateISO { set; get; } = "";
        public int FileTransferConfigId { set; get; } = 0;
        public String SourceFileName { set; get; } = "";
        public String TargetFileName { set; get; } = "";
        public DateTimeOffset HoldUntil { set; get; }
        public String HoldUntilISO { set; get; } = "";

        // Additional fields
        public String TransferName { set; get; } = "";
        public String FTPHost { set; get; } = "";
        public String TransferTypeText { set; get; } = "";
        public string ProtocolText { set; get; } = "";
    }

    public class DataTransferQueueListModel : BaseListModel {
        public List<DataTransferQueueModel> Items { set; get; } = new List<DataTransferQueueModel>();
    }
}
