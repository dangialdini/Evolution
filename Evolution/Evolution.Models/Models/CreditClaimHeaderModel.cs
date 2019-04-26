using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class CreditClaimHeaderModel {
        public int Id { get; set; }
        public DateTimeOffset? DateCreated { get; set; }
        public int? ClaimNumber { get; set; }
        public int? ClaimCategoryID { get; set; }
        public int? CustomerId { get; set; }
        public string NotesAndComments { get; set; }
        public int? StatusId { get; set; }
        public int? ApprovedById { get; set; }
        public DateTimeOffset? ApprovedByDate { get; set; }
        public DateTimeOffset? DateFinalised { get; set; }
        public DateTimeOffset? DateCancelled { get; set; }
        public string PickupAddress { get; set; }
        public DateTimeOffset? DateRejected { get; set; }
        public int? RejectionReasonId { get; set; }
        public string RejectionReasonComments { get; set; }
        public int? ReturnTypeId { get; set; }
        public int? Carrier { get; set; }
        public string ConNote { get; set; }
        public DateTimeOffset? PickupBookedDate { get; set; }
        public DateTimeOffset? ReceivedByWarehoseDate { get; set; }
        public int? PickupBoxCount { get; set; }
        public int? PickupBoxWeightKg { get; set; }
        public string PickupBoxDimensions { get; set; }
        public string PickupSpecialInstructions { get; set; }
        public bool ReplacementRequired { get; set; }
        public int? ReplacementOrderId { get; set; }
        public int? CustomerContactId { get; set; }
        public string CustomerContactEmail { get; set; }
        public string CustomerContactPhone { get; set; }
        public decimal? FreightToCredit { get; set; }
        public string CustomerReference { get; set; }
        public int? ReturnToWarehouseId { get; set; }
        public string ReturnAddress { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTimeOffset? ReceivedByWarehouseDate { get; set; }
        public decimal? FreightCreditPercent { get; set; }
        public decimal? FreightCreditFixedAmount { get; set; }
        public int? RaisedById { get; set; }
        public int? CreditNoteFreightDescriptionId { get; set; }
        public int? ReplacementOrderNo { get; set; }
        public int? RejectedById { get; set; }
        public int? ReturnAddressId { get; set; }
    }

    public class CreditClaimHeaderListModel : BaseListModel {
        public List<CreditClaimHeaderModel> Items { get; set; } = new List<CreditClaimHeaderModel>();
    }
}
