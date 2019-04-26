using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Evolution.Models.Models;
using Evolution.Resources;
using Evolution.Enumerations;

namespace Evolution.Models.ViewModels {

    public class SalePrintOptionsViewModel : ViewModelBase {
        public int SalesOrderHeaderTempId { set; get; } = 0;
        public int TemplateId { set; get; } = 0;
        public bool ShowCancelledItems { set; get; } = true;
        public bool SaveInSaleNotesAttachments { set; get; } = false;
        public bool ViewCreatedDocument { set; get; } = false;
        public bool SendAsEMail { set; get; } = false;
        public string Subject { set; get; } = "";
        public string Message { set; get; } = "";
        public bool SaveAsContact { set; get; } = false;

        public CustomerContactModel CustomerContact { set; get; } = new CustomerContactModel();

        public List<ListItemModel> TemplateList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> AvailableRecipientsList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> SelectedRecipientsList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> CopiedRecipientsList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> SalutationList { set; get; } = new List<ListItemModel>();
    }
}
