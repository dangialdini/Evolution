using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class LocationModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string CanBeSold { set; get; } = "";
        public string LocationIdentification { set; get; } = "";
        public string LocationName { set; get; } = "";
        public string Street { set; get; } = "";
        public string City { set; get; } = "";
        public string State { set; get; } = "";
        public string PostCode { set; get; } = "";
        public string Country { set; get; } = "";
        public string Contact { set; get; } = "";
        public string ContactPhone { set; get; } = "";
        public string Notes { set; get; } = "";
        public bool Enabled { set; get; } = false;

        // Additional methods
        public string FullAddress {
            get {
                string rc = Street;
                if (!string.IsNullOrEmpty(City)) rc += "\r\n" + City;
                if (!string.IsNullOrEmpty(State)) rc += "\r\n" + State;
                if (!string.IsNullOrEmpty(PostCode)) rc += "\r\n" + PostCode;
                if (!string.IsNullOrEmpty(Country)) rc += "\r\n" + Country;
                return rc;
            }
        }
    }

    public class LocationListModel : BaseListModel {
        public List<LocationModel> Items { set; get; } = new List<LocationModel>();
    }
}
