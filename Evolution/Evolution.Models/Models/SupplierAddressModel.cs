using Evolution.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class SupplierAddressModel {
        public int Id { set; get; } = 0;
        public int? CompanyId { get; set; } = null;
        public int? SupplierId { set; get; } = null;
        public int? AddressTypeId { get; set; } = null;
        public string Street { set; get; } = "";
        public string City { set; get; } = "";
        public string State { set; get; } = "";
        public string Postcode { set; get; } = "";
        public string StreetLine1 { set; get; } = "";
        public string StreetLine2 { set; get; } = "";
        public string StreetLine3 { set; get; } = "";
        public string StreetLine4 { set; get; } = "";
        public int? CountryId { set; get; } = 0;
        public string CountryName { set; get; } = "";

        // Additional members
        public string FullAddress {
            get {
                string rc = Street;
                if (!string.IsNullOrEmpty(City)) rc += "\r\n" + City;
                if (!string.IsNullOrEmpty(State)) rc += "\r\n" + State;
                if (!string.IsNullOrEmpty(Postcode)) rc += "\r\n" + Postcode;
                if (!string.IsNullOrEmpty(CountryName)) rc += "\r\n" + CountryName;
                return rc;
            }
        }

        // Additional fields
        public AddressType AddressType { get; set; }
        public string AddressTypeText { get; set; }
    }

    public class SupplierAddressListModel : BaseListModel {
        public List<SupplierAddressModel> Items { set; get; } = new List<SupplierAddressModel>();
    }
}
