using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class PickHeaderModel {
        public int Id { set; get; }
        public int? CompanyId { set; get; }
        public int? CustomerId { set; get; }
        public DateTimeOffset? PickDate { set; get; }
        public int? PickStatusId { set; get; }
        public int? LocationId { set; get; }
        public DateTimeOffset? STWDate { set; get; }
        public DateTimeOffset? PickComplete { set; get; }
        public DateTimeOffset? PackComplete { set; get; }
        public string ShipAddress1 { set; get; }
        public string ShipAddress2 { set; get; }
        public string ShipAddress3 { set; get; }
        public string ShipAddress4 { set; get; }
        public string ShipSuburb { set; get; }
        public string ShipState { set; get; }
        public string ShipPostcode { set; get; }
        public DateTimeOffset? InvoiceDate { set; get; }
        public int? InvoiceNumber { set; get; }
        public bool InvoiceFinalised { set; get; }
        public int? ShipMethodId { set; get; }
        public int? SalesPersonId { set; get; }
        public DateTimeOffset? ShipDate { set; get; }
        public string TrackingNumber { set; get; }
        public int? BoxCount { set; get; }
        public int? PickPriority { set; get; }
        public int? PickedById { set; get; }
        public int? PackedById { set; get; }
        public DateTimeOffset? ReadyForShippingDate { set; get; }
        public int? ShippingDocumentId { set; get; }
        public DateTimeOffset? AddedToShipManifestDate { set; get; }
        public bool DocumentPrinted { set; get; }
        public string CustPO { set; get; }
        public string SecretComment { set; get; }
        public string PickComment { set; get; }
        public string ShipMethodAccount { set; get; }
        public double? FreightCost { set; get; }
        public string DeliveryInstructions { set; get; }
        public string CustomerContact { set; get; }
        public bool IsManualFreight { set; get; }
        public int? ShipCountryId { set; get; }
        public decimal? OurFreightCost { set; get; }
        public string EnteredBy { set; get; }
        public string WarehouseInstructions { set; get; }
        public string EndUserName { set; get; }
        public int? CreditCardId { set; get; }
        public bool IsRetailPick { set; get; }
        public bool IsUploadedToWarehouse { set; get; }
        public int? TermsID { set; get; }
        public string UnregisteredFreightCarrier { set; get; }
        public DateTimeOffset? DateCreditCardCharged { set; get; }
        public int? OrderTypeId { set; get; }

        // Additional fields
        public List<PickDetailModel> PickDetails = new List<PickDetailModel>();
        public List<string> PickFiles { set; get; } = new List<string>();
        public string PickDropFolder { set; get; }
        public string CustomerName { get; set; }
        public string Status { get; set; } = "";
        public string InvoiceDateISO { get { return InvoiceDate.ISODate(); } }
        public string STWDateISO { get { return STWDate.ISODate(); } }
        public string LocationName { get; set; } = "";
        public string ShipCountry { get; set; } = "";
    }

    public class PicksListModel : BaseListModel {
        public List<PickHeaderModel> Items { get; set; } = new List<PickHeaderModel>();
    }
}
