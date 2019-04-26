using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class MessageTemplateModel {
        public int Id { set; get; } = 0;
        public int TemplateId { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string Subject { set; get; } = "";
        public string Message { set; get; } = "";
        public bool Enabled { set; get; } = false;
    }

    public class MessageTemplateListModel : BaseListModel {
        public List<MessageTemplateModel> Items { set; get; } = new List<MessageTemplateModel>();
    }
}
