//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Evolution.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Shipment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Shipment()
        {
            this.ShipmentContents = new HashSet<ShipmentContent>();
        }
    
        public int Id { get; set; }
        public Nullable<int> OriginalRowId { get; set; }
        public Nullable<int> CarrierVesselId { get; set; }
        public int CompanyId { get; set; }
        public string VoyageNo { get; set; }
        public Nullable<int> ShippingMethodId { get; set; }
        public Nullable<int> PortDepartId { get; set; }
        public Nullable<int> PortArrivalId { get; set; }
        public Nullable<System.DateTimeOffset> Date100Shipped { get; set; }
        public Nullable<System.DateTimeOffset> DatePreAlertETA { get; set; }
        public Nullable<System.DateTimeOffset> DateWarehouseAccept { get; set; }
        public string ConsolidationName { get; set; }
        public string Comments { get; set; }
        public string ConsignmentNo { get; set; }
        public Nullable<int> SeasonId { get; set; }
        public bool IsTranshipment { get; set; }
        public Nullable<double> CBMCharged { get; set; }
        public string ContainerNo { get; set; }
        public Nullable<int> CountPackages { get; set; }
        public Nullable<System.DateTimeOffset> DateUnpackSlipRcvd { get; set; }
        public Nullable<System.DateTimeOffset> DateExpDel { get; set; }
    
        public virtual CarrierVessel CarrierVessel { get; set; }
        public virtual LOVItem LOVItem_ShippingMethod { get; set; }
        public virtual Port Port_PortArrival { get; set; }
        public virtual Port Port_PortDeparture { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShipmentContent> ShipmentContents { get; set; }
        public virtual Company Company { get; set; }
        public virtual LOVItem LOVItem_Season { get; set; }
    }
}
