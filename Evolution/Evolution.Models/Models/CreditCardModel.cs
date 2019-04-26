using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class CreditCardModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int CustomerId { set; get; } = 0;
        public int CreditCardProviderId { set; get; } = 0;
        public string CreditCardNo { set; get; } = "";
        public string NameOnCard { set; get; } = "";
        public string Expiry { set; get; } = "";
        public string CCV { set; get; } = "";
        public string Notes { set; get; } = "";
        public bool Enabled { set; get; } = false;

        // Additional fields
        public string CardProviderName { set; get; } = "";
        public string CardProviderLogo { set; get; } = "";
    }

    public class CreditCardListModel : BaseListModel {
        public List<CreditCardModel> Items { set; get; } = new List<CreditCardModel>();
    }
}
