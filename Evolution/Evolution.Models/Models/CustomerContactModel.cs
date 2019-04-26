using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class CustomerContactModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int? CustomerId { set; get; } = null;
        public string ContactFirstname { set; get; } = "";
        public string ContactSurname { set; get; } = "";
        public string ContactKnownAs { set; get; } = "";
        public string ContactEmail { set; get; } = "";
        public string Position { set; get; } = "";
        public string ContactSalutation { set; get; } = "";
        public string ContactPhone1 { set; get; } = "";
        public string ContactPhone2 { set; get; } = "";
        public string ContactPhone3 { set; get; } = "";
        public string ContactFax { set; get; } = "";
        public string ContactPhoneNotes { set; get; } = "";
        public string ContactNotes { set; get; } = "";
        public bool SendStatement { set; get; } = false;
        public bool SendInvoice { set; get; } = false;
        public bool MailingList { set; get; } = false;
        public bool PrimaryContact { set; get; } = false;
        public bool ReceiveCatalog { set; get; } = false;
        public bool Enabled { set; get; } = false;
    }

    public class CustomerContactListModel : BaseListModel {
        public List<CustomerContactModel> Items { set; get; } = new List<CustomerContactModel>();
    }
}
