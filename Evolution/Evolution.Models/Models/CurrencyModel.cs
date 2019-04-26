using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class CurrencyModel {
        public int Id { set; get; } = 0;
        public string CurrencyCode { set; get; } = "";
        public string CurrencyName { set; get; } = "";
        public decimal? ExchangeRate { set; get; } = 0;
        public string CurrencySymbol { set; get; } = "";
        public string FormatTemplate { set; get; } = "";
        public bool Enabled { set; get; } = false;
    }

    public class CurrencyListModel : BaseListModel {
        public List<CurrencyModel> Items { set; get; } = new List<CurrencyModel>();
    }
}
