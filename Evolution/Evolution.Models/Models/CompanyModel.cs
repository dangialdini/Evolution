using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Enumerations;

namespace Evolution.Models.Models {
    public class CompanyModel {
        public int Id { set; get; } = 0;
        public string ABN { set; get; } = "";
        public string CompanyName { set; get; } = "";
        public string FriendlyName { set; get; } = "";
        public string CompanyAddress { set; get; } = "";
        public string PhoneNumber { set; get; } = "";
        public string FaxNumber { set; get; } = "";
        public string Website { set; get; } = "";
        public string ShippingAddress { set; get; } = "";
        public string BankName { set; get; } = "";
        public string AccountName { set; get; } = "";
        public string AccountNumber { set; get; } = "";
        public string Swift { set; get; } = "";
        public string Branch { set; get; } = "";
        public string BranchAddress { set; get; } = "";
        public string AccountBSB { set; get; } = "";
        public double? AmexSurcharge { set; get; } = 0;
        public double? VisaSurcharge { set; get; } = 0;
        public double? MCSurcharge { set; get; } = 0;
        public string StatementBody { set; get; } = "";
        public string CancelMessage { set; get; } = "";
        public string RoutingNo { set; get; } = "";
        public string EmailAddressPurchasing { set; get; } = "";
        public string EmailAddressSales { set; get; } = "";
        public string EmailAddressAccounts { set; get; } = "";
        public string EmailAddressBCC { set; get; } = "";
        public string BCC { set; get; } = "";
        public int? DefaultLocationID { set; get; } = null;
        public int? DefaultCountryID { set; get; } = null;
        public int? DefaultCurrencyID { set; get; } = null;
        public bool IsParentCompany { set; get; } = false;
        public string MarginLogo { set; get; } = "";
        public string FormLogo { set; get; } = "";
        public string DateFormat { set; get; } = "";
        public int ProductTemplateId { set; get; } = 0;
        public UnitOfMeasure UnitOfMeasure { set; get; } = UnitOfMeasure.Metric;
        public int? POWarehouseTemplateId { set; get; } = null;
        public int? POSupplierTemplateId { set; get; } = null;
        public int? POFreightForwarderTemplateId { set; get; } = null;
        public int? DefaultImportUserId { set; get; }
        public bool Enabled { set; get; } = false;

        // Additional fields
        public string LengthUnit { set; get; } = "cm";
        public string LongLengthUnit { set; get; } = "m";
        public string WeightUnit { set; get; } = "kg";
    }

    public class CompanyListModel : BaseListModel {
        public List<CompanyModel> Items { set; get; } = new List<CompanyModel>();
    }
}
