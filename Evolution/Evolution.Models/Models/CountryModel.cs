using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class CountryModel {
        public int Id { set; get; } = 0;
        public string CountryName { set; get; } = "";
        public string ISO2Code { set; get; } = "";
        public string ISO3Code { set; get; } = "";
        public int? UNCode { set; get; } = 0;
        public bool Enabled { set; get; } = false;
    }

    public class CountryListModel : BaseListModel {
        public List<CountryModel> Items { set; get; } = new List<CountryModel>();
    }
}
