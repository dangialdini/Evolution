using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class SupplierModel {
        public int Id { set; get; } = 0;
        public int? CompanyId { get; set; } = null;
        public DateTimeOffset CreatedDate { set; get; }
        public string CreatedDateISO { get { return CreatedDate.ISODate(); } }
        public int? CreatedById { set; get; } = 0;
        public string Name { set; get; } = "";
        public int CurrencyId { set; get; } = 0;
        public string Notes { set; get; } = "";
        public int? SupplierTermId { set; get; } = null;
        public int? TaxCodeId { set; get; } = null;
        public string CancelMessage { set; get; } = "";
        public int? PortId { set; get; } = null;
        public int? ShipMethodId { set; get; } = null;
        public bool IsProductSupplier { set; get; } = false;
        public string ContactName { set; get; } = "";
        public string Phone1 { set; get; } = "";
        public string Phone2 { set; get; } = "";
        public int? CommercialTermsId { set; get; } = null;
        public string Email { set; get; } = "";
        public string URL { set; get; } = "";
        public bool IsVerticalSupplier { set; get; } = false;
        public bool IsManufacturer { set; get; } = false;
        public bool IsTrader { set; get; } = false;
        public bool IsAgent { set; get; } = false;
        //[FreightForwarderId_AU] [int] NULL,
        //[FreightForwarderId_DTC] [int] NULL,
        //[FreightForwarderId_UK] [int] NULL,
        //[FreightForwarderId_US] [int] NULL,
        //[PurchaserId] [int] NULL,
        public bool Enabled { set; get; } = false;

        // Address details
        public string Street { set; get; } = "";
        public string City { set; get; } = "";
        public string State { set; get; } = "";
        public string Postcode { set; get; } = "";
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
    }

    public class SupplierListModel : BaseListModel {
        public List<SupplierModel> Items { set; get; } = new List<SupplierModel>();
    }
}
