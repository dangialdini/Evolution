using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class EMailQueueModel {
        public int Id { set; get; } = 0;
        public DateTimeOffset QueuedDate { set; get; }
        public String QueuedDateISO { get { return QueuedDate.ISODate(); } }
        public String SenderAddress { set; get; } = "";
        public String ReplyToAddress { set; get; } = "";
        public String RecipientAddress { set; get; } = "";
        public String MessageSubject { set; get; } = "";
        public String MessageText { set; get; } = "";
        public int AttachmentCount { set; get; } = 0;
        public int Retries { set; get; } = 0;
    }

    public class EMailQueueListModel : BaseListModel {
        public List<EMailQueueModel> Items { set; get; } = new List<EMailQueueModel>();
    }
}
