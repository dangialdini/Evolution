using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.Models.Models {
    public class CustomerAddressModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int? CustomerId { set; get; } = null;
        public int AddressTypeId { set; get; } = 0;
        public string Street { set; get; } = "";
        public string City { set; get; } = "";
        public string State { set; get; } = "";
        public string Postcode { set; get; } = "";
        //public string StreetLine1 { set; get; } = "";
        //public string StreetLine2 { set; get; } = "";
        //public string StreetLine3 { set; get; } = "";
        //public string StreetLine4 { set; get; } = "";
        public DateTimeOffset? DateStart { set; get; } = DateTimeOffset.Now;
        public string DateStartISO { get { return DateStart.ISODate(); } }
        public DateTimeOffset? DateEnd { set; get; } = null;
        public string DateEndISO { get { return DateEnd.ISODate(); } }
        public int? CountryId { set; get; } = 0;

        // Additional fields
        public AddressType AddressType { set; get; }
        public string AddressTypeText { set; get; }
        public string CountryName { set; get; } = "";

        // Additional methods
        public bool IsValid {
            get {
                if(!string.IsNullOrEmpty(Street) &&
                   !string.IsNullOrEmpty(City) &&
                   !string.IsNullOrEmpty(State) &&
                   !string.IsNullOrEmpty(Postcode) &&
                   CountryId != null) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    }

    public class CustomerAddressListModel : BaseListModel {
        public List<CustomerAddressModel> Items { set; get; } = new List<CustomerAddressModel>();
    }
}
