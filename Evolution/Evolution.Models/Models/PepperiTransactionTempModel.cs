using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models
{
    public class PepperiTransactionTempModel
    {
        #region Transaction Header

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class SalesTransaction {

            private SalesTransactionTransactionHeader transactionHeaderField;

            private SalesTransactionTransactionLine[] transactionLinesField;

            /// <remarks/>
            public SalesTransactionTransactionHeader TransactionHeader {
                get {
                    return this.transactionHeaderField;
                }
                set {
                    this.transactionHeaderField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("TransactionLine", IsNullable = false)]
            public SalesTransactionTransactionLine[] TransactionLines {
                get {
                    return this.transactionLinesField;
                }
                set {
                    this.transactionLinesField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionHeader {

            private SalesTransactionTransactionHeaderTransactionHeaderFields transactionHeaderFieldsField;

            private SalesTransactionTransactionHeaderCatalogFields catalogFieldsField;

            private SalesTransactionTransactionHeaderSalesRepFields salesRepFieldsField;

            private SalesTransactionTransactionHeaderAccountFields accountFieldsField;

            private SalesTransactionTransactionHeaderBillingFields billingFieldsField;

            private SalesTransactionTransactionHeaderShippingFields shippingFieldsField;

            private SalesTransactionTransactionHeaderTotals totalsField;

            private object contactPersonFieldsField;

            private SalesTransactionTransactionHeaderTransactionCustomFields transactionCustomFieldsField;

            /// <remarks/>
            public SalesTransactionTransactionHeaderTransactionHeaderFields TransactionHeaderFields {
                get {
                    return this.transactionHeaderFieldsField;
                }
                set {
                    this.transactionHeaderFieldsField = value;
                }
            }

            /// <remarks/>
            public SalesTransactionTransactionHeaderCatalogFields CatalogFields {
                get {
                    return this.catalogFieldsField;
                }
                set {
                    this.catalogFieldsField = value;
                }
            }

            /// <remarks/>
            public SalesTransactionTransactionHeaderSalesRepFields SalesRepFields {
                get {
                    return this.salesRepFieldsField;
                }
                set {
                    this.salesRepFieldsField = value;
                }
            }

            /// <remarks/>
            public SalesTransactionTransactionHeaderAccountFields AccountFields {
                get {
                    return this.accountFieldsField;
                }
                set {
                    this.accountFieldsField = value;
                }
            }

            /// <remarks/>
            public SalesTransactionTransactionHeaderBillingFields BillingFields {
                get {
                    return this.billingFieldsField;
                }
                set {
                    this.billingFieldsField = value;
                }
            }

            /// <remarks/>
            public SalesTransactionTransactionHeaderShippingFields ShippingFields {
                get {
                    return this.shippingFieldsField;
                }
                set {
                    this.shippingFieldsField = value;
                }
            }

            /// <remarks/>
            public SalesTransactionTransactionHeaderTotals Totals {
                get {
                    return this.totalsField;
                }
                set {
                    this.totalsField = value;
                }
            }

            /// <remarks/>
            public object ContactPersonFields {
                get {
                    return this.contactPersonFieldsField;
                }
                set {
                    this.contactPersonFieldsField = value;
                }
            }

            /// <remarks/>
            public SalesTransactionTransactionHeaderTransactionCustomFields TransactionCustomFields {
                get {
                    return this.transactionCustomFieldsField;
                }
                set {
                    this.transactionCustomFieldsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionHeaderTransactionHeaderFields {

            private int wrntyIDField;

            private string typeField;

            private string statusField;

            private string creationDateTimeField;

            private string modificationDateTimeField;

            private string actionDateTimeField;

            private string deliveryDateField;

            private object remarkField;

            /// <remarks/>
            public int WrntyID {
                get {
                    return this.wrntyIDField;
                }
                set {
                    this.wrntyIDField = value;
                }
            }

            /// <remarks/>
            public string Type {
                get {
                    return this.typeField;
                }
                set {
                    this.typeField = value;
                }
            }

            /// <remarks/>
            public string Status {
                get {
                    return this.statusField;
                }
                set {
                    this.statusField = value;
                }
            }

            /// <remarks/>
            public string CreationDateTime {
                get {
                    return this.creationDateTimeField;
                }
                set {
                    this.creationDateTimeField = value;
                }
            }

            /// <remarks/>
            public string ModificationDateTime {
                get {
                    return this.modificationDateTimeField;
                }
                set {
                    this.modificationDateTimeField = value;
                }
            }

            /// <remarks/>
            public string ActionDateTime {
                get {
                    return this.actionDateTimeField;
                }
                set {
                    this.actionDateTimeField = value;
                }
            }

            /// <remarks/>
            public string DeliveryDate {
                get {
                    return this.deliveryDateField;
                }
                set {
                    this.deliveryDateField = value;
                }
            }

            /// <remarks/>
            public object Remark {
                get {
                    return this.remarkField;
                }
                set {
                    this.remarkField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionHeaderCatalogFields {

            private string catalogIDField;

            private object catalogDescriptionField;

            private byte catalogPriceFactorField;

            private string catalogExpirationDateField;

            /// <remarks/>
            public string CatalogID {
                get {
                    return this.catalogIDField;
                }
                set {
                    this.catalogIDField = value;
                }
            }

            /// <remarks/>
            public object CatalogDescription {
                get {
                    return this.catalogDescriptionField;
                }
                set {
                    this.catalogDescriptionField = value;
                }
            }

            /// <remarks/>
            public byte CatalogPriceFactor {
                get {
                    return this.catalogPriceFactorField;
                }
                set {
                    this.catalogPriceFactorField = value;
                }
            }

            /// <remarks/>
            public string CatalogExpirationDate {
                get {
                    return this.catalogExpirationDateField;
                }
                set {
                    this.catalogExpirationDateField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionHeaderSalesRepFields {

            private string agentNameField;

            private object agentExternalIDField;

            private string agentEmailField;

            /// <remarks/>
            public string AgentName {
                get {
                    return this.agentNameField;
                }
                set {
                    this.agentNameField = value;
                }
            }

            /// <remarks/>
            public object AgentExternalID {
                get {
                    return this.agentExternalIDField;
                }
                set {
                    this.agentExternalIDField = value;
                }
            }

            /// <remarks/>
            public string AgentEmail {
                get {
                    return this.agentEmailField;
                }
                set {
                    this.agentEmailField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionHeaderAccountFields {

            private int accountWrntyIDField;

            private long accountExternalIDField;

            private string accountCreationDateField;

            private string accountNameField;

            private string accountPhoneField;

            private object accountMobileField;

            private object accountFaxField;

            private string accountEmailField;

            private string accountStreetField;

            private string accountCityField;

            private string accountStateField;

            private string accountCountryField;

            private string accountZipCodeField;

            private object accountPriceLevelNameField;

            /// <remarks/>
            public int AccountWrntyID {
                get {
                    return this.accountWrntyIDField;
                }
                set {
                    this.accountWrntyIDField = value;
                }
            }

            /// <remarks/>
            public long AccountExternalID {
                get {
                    return this.accountExternalIDField;
                }
                set {
                    this.accountExternalIDField = value;
                }
            }

            /// <remarks/>
            public string AccountCreationDate {
                get {
                    return this.accountCreationDateField;
                }
                set {
                    this.accountCreationDateField = value;
                }
            }

            /// <remarks/>
            public string AccountName {
                get {
                    return this.accountNameField;
                }
                set {
                    this.accountNameField = value;
                }
            }

            /// <remarks/>
            public string AccountPhone {
                get {
                    return this.accountPhoneField;
                }
                set {
                    this.accountPhoneField = value;
                }
            }

            /// <remarks/>
            public object AccountMobile {
                get {
                    return this.accountMobileField;
                }
                set {
                    this.accountMobileField = value;
                }
            }

            /// <remarks/>
            public object AccountFax {
                get {
                    return this.accountFaxField;
                }
                set {
                    this.accountFaxField = value;
                }
            }

            /// <remarks/>
            public string AccountEmail {
                get {
                    return this.accountEmailField;
                }
                set {
                    this.accountEmailField = value;
                }
            }

            /// <remarks/>
            public string AccountStreet {
                get {
                    return this.accountStreetField;
                }
                set {
                    this.accountStreetField = value;
                }
            }

            /// <remarks/>
            public string AccountCity {
                get {
                    return this.accountCityField;
                }
                set {
                    this.accountCityField = value;
                }
            }

            /// <remarks/>
            public string AccountState {
                get {
                    return this.accountStateField;
                }
                set {
                    this.accountStateField = value;
                }
            }

            /// <remarks/>
            public string AccountCountry {
                get {
                    return this.accountCountryField;
                }
                set {
                    this.accountCountryField = value;
                }
            }

            /// <remarks/>
            public string AccountZipCode {
                get {
                    return this.accountZipCodeField;
                }
                set {
                    this.accountZipCodeField = value;
                }
            }

            /// <remarks/>
            public object AccountPriceLevelName {
                get {
                    return this.accountPriceLevelNameField;
                }
                set {
                    this.accountPriceLevelNameField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionHeaderBillingFields {

            private string billToNameField;

            private string billToStreetField;

            private string billToCityField;

            private string billToStateField;

            private string billToCountryField;

            private string billToZipCodeField;

            private string billToPhoneField;

            /// <remarks/>
            public string BillToName {
                get {
                    return this.billToNameField;
                }
                set {
                    this.billToNameField = value;
                }
            }

            /// <remarks/>
            public string BillToStreet {
                get {
                    return this.billToStreetField;
                }
                set {
                    this.billToStreetField = value;
                }
            }

            /// <remarks/>
            public string BillToCity {
                get {
                    return this.billToCityField;
                }
                set {
                    this.billToCityField = value;
                }
            }

            /// <remarks/>
            public string BillToState {
                get {
                    return this.billToStateField;
                }
                set {
                    this.billToStateField = value;
                }
            }

            /// <remarks/>
            public string BillToCountry {
                get {
                    return this.billToCountryField;
                }
                set {
                    this.billToCountryField = value;
                }
            }

            /// <remarks/>
            public string BillToZipCode {
                get {
                    return this.billToZipCodeField;
                }
                set {
                    this.billToZipCodeField = value;
                }
            }

            /// <remarks/>
            public string BillToPhone {
                get {
                    return this.billToPhoneField;
                }
                set {
                    this.billToPhoneField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionHeaderShippingFields {

            private ushort shipToExternalIDField;

            private string shipToNameField;

            private string shipToStreetField;

            private string shipToCityField;

            private string shipToStateField;

            private string shipToCountryField;

            private string shipToZipCodeField;

            private string shipToPhoneField;

            /// <remarks/>
            public ushort ShipToExternalID {
                get {
                    return this.shipToExternalIDField;
                }
                set {
                    this.shipToExternalIDField = value;
                }
            }

            /// <remarks/>
            public string ShipToName {
                get {
                    return this.shipToNameField;
                }
                set {
                    this.shipToNameField = value;
                }
            }

            /// <remarks/>
            public string ShipToStreet {
                get {
                    return this.shipToStreetField;
                }
                set {
                    this.shipToStreetField = value;
                }
            }

            /// <remarks/>
            public string ShipToCity {
                get {
                    return this.shipToCityField;
                }
                set {
                    this.shipToCityField = value;
                }
            }

            /// <remarks/>
            public string ShipToState {
                get {
                    return this.shipToStateField;
                }
                set {
                    this.shipToStateField = value;
                }
            }

            /// <remarks/>
            public string ShipToCountry {
                get {
                    return this.shipToCountryField;
                }
                set {
                    this.shipToCountryField = value;
                }
            }

            /// <remarks/>
            public string ShipToZipCode {
                get {
                    return this.shipToZipCodeField;
                }
                set {
                    this.shipToZipCodeField = value;
                }
            }

            /// <remarks/>
            public string ShipToPhone {
                get {
                    return this.shipToPhoneField;
                }
                set {
                    this.shipToPhoneField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionHeaderTotals {

            private string currencyField;

            private int totalItemsCountField;

            private decimal subTotalField;

            private decimal subTotalAfterItemsDiscountField;

            private decimal grandTotalField;

            private decimal discountPercentageField;

            private decimal taxPercentageField;

            /// <remarks/>
            public string Currency {
                get {
                    return this.currencyField;
                }
                set {
                    this.currencyField = value;
                }
            }

            /// <remarks/>
            public int TotalItemsCount {
                get {
                    return this.totalItemsCountField;
                }
                set {
                    this.totalItemsCountField = value;
                }
            }

            /// <remarks/>
            public decimal SubTotal {
                get {
                    return this.subTotalField;
                }
                set {
                    this.subTotalField = value;
                }
            }

            /// <remarks/>
            public decimal SubTotalAfterItemsDiscount {
                get {
                    return this.subTotalAfterItemsDiscountField;
                }
                set {
                    this.subTotalAfterItemsDiscountField = value;
                }
            }

            /// <remarks/>
            public decimal GrandTotal {
                get {
                    return this.grandTotalField;
                }
                set {
                    this.grandTotalField = value;
                }
            }

            /// <remarks/>
            public decimal DiscountPercentage {
                get {
                    return this.discountPercentageField;
                }
                set {
                    this.discountPercentageField = value;
                }
            }

            /// <remarks/>
            public decimal TaxPercentage {
                get {
                    return this.taxPercentageField;
                }
                set {
                    this.taxPercentageField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionHeaderTransactionCustomFields {

            private decimal tSAGSTField;

            private string tSADeliveryWindowOpenField;

            private string tSADeliveryWindowCloseField;

            private object tSAOrderTakenBy;
            private decimal tSATaxRate;
            private decimal tSASubTotalBeforeTax;
            private decimal tSAGrandTotal;
            private int salespersonID;
            private string filespec;
            private bool isNewCustomer;

            /// <remarks/>
            public decimal TSAGST {
                get {
                    return this.tSAGSTField;
                }
                set {
                    this.tSAGSTField = value;
                }
            }

            /// <remarks/>
            public string TSADeliveryWindowOpen {
                get {
                    return this.tSADeliveryWindowOpenField;
                }
                set {
                    this.tSADeliveryWindowOpenField = value;
                }
            }

            /// <remarks/>
            public string TSADeliveryWindowClose {
                get {
                    return this.tSADeliveryWindowCloseField;
                }
                set {
                    this.tSADeliveryWindowCloseField = value;
                }
            }

            public object TSAOrderTakenBy {
                get {
                    return this.tSAOrderTakenBy;
                }
                set {
                    this.tSAOrderTakenBy = value;
                }
            }

            public decimal TSATaxRate {
                get {
                    return this.tSATaxRate;
                }
                set {
                    this.tSATaxRate = value;
                }
            }

            // 
            public decimal TSASubTotalBeforeTax {
                get {
                    return this.tSASubTotalBeforeTax;
                }
                set {
                    this.tSASubTotalBeforeTax = value;
                }
            }

            public decimal TSAGrandTotal {
                get {
                    return this.tSAGrandTotal;
                }
                set {
                    this.tSAGrandTotal = value;
                }
            }

            public int SalespersonID {
                get {
                    return this.salespersonID;
                }
                set {
                    this.salespersonID = value;
                }
            }

            public string Filespec {
                get {
                    return this.filespec;
                }
                set {
                    this.filespec = value;
                }
            }

            public bool IsNewCustomer {
                get {
                    return this.isNewCustomer;
                }
                set {
                    this.isNewCustomer = value;
                }
            }
        }

        #endregion

        #region Transaction Line

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionLine {

            private SalesTransactionTransactionLineTransactionLineFields transactionLineFieldsField;

            private SalesTransactionTransactionLineItemFields itemFieldsField;

            private SalesTransactionTransactionLineTransactionLineCustomFields transactionLineCustomFieldsField;

            /// <remarks/>
            public SalesTransactionTransactionLineTransactionLineFields TransactionLineFields {
                get {
                    return this.transactionLineFieldsField;
                }
                set {
                    this.transactionLineFieldsField = value;
                }
            }

            /// <remarks/>
            public SalesTransactionTransactionLineItemFields ItemFields {
                get {
                    return this.itemFieldsField;
                }
                set {
                    this.itemFieldsField = value;
                }
            }

            /// <remarks/>
            public SalesTransactionTransactionLineTransactionLineCustomFields TransactionLineCustomFields {
                get {
                    return this.transactionLineCustomFieldsField;
                }
                set {
                    this.transactionLineCustomFieldsField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionLineTransactionLineFields {

            private int unitsQuantityField;

            private decimal unitPriceField;

            private decimal unitDiscountPercentageField;

            private decimal unitPriceAfterDiscountField;

            private decimal totalUnitsPriceAfterDiscountField;

            private string deliveryDateField;

            private int transactionWrntyIDField;

            private object transactionExternalIDField;

            private int lineNumberField;

            /// <remarks/>
            public int UnitsQuantity {
                get {
                    return this.unitsQuantityField;
                }
                set {
                    this.unitsQuantityField = value;
                }
            }

            /// <remarks/>
            public decimal UnitPrice {
                get {
                    return this.unitPriceField;
                }
                set {
                    this.unitPriceField = value;
                }
            }

            /// <remarks/>
            public decimal UnitDiscountPercentage {
                get {
                    return this.unitDiscountPercentageField;
                }
                set {
                    this.unitDiscountPercentageField = value;
                }
            }

            /// <remarks/>
            public decimal UnitPriceAfterDiscount {
                get {
                    return this.unitPriceAfterDiscountField;
                }
                set {
                    this.unitPriceAfterDiscountField = value;
                }
            }

            /// <remarks/>
            public decimal TotalUnitsPriceAfterDiscount {
                get {
                    return this.totalUnitsPriceAfterDiscountField;
                }
                set {
                    this.totalUnitsPriceAfterDiscountField = value;
                }
            }

            /// <remarks/>
            public string DeliveryDate {
                get {
                    return this.deliveryDateField;
                }
                set {
                    this.deliveryDateField = value;
                }
            }

            /// <remarks/>
            public int TransactionWrntyID {
                get {
                    return this.transactionWrntyIDField;
                }
                set {
                    this.transactionWrntyIDField = value;
                }
            }

            /// <remarks/>
            public object TransactionExternalID {
                get {
                    return this.transactionExternalIDField;
                }
                set {
                    this.transactionExternalIDField = value;
                }
            }

            /// <remarks/>
            public int LineNumber {
                get {
                    return this.lineNumberField;
                }
                set {
                    this.lineNumberField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionLineItemFields {

            private int itemWrntyIDField;

            private string itemExternalIDField;

            private string itemMainCategoryField;

            private string itemMainCategoryCodeField;

            private string itemNameField;

            private decimal itemPriceField;

            private int itemInStockQuantityField;

            private string tSANextAvailableDateField;

            private string tSATotalAvailableField;

            /// <remarks/>
            public int ItemWrntyID {
                get {
                    return this.itemWrntyIDField;
                }
                set {
                    this.itemWrntyIDField = value;
                }
            }

            /// <remarks/>
            public string ItemExternalID {
                get {
                    return this.itemExternalIDField;
                }
                set {
                    this.itemExternalIDField = value;
                }
            }

            /// <remarks/>
            public string ItemMainCategory {
                get {
                    return this.itemMainCategoryField;
                }
                set {
                    this.itemMainCategoryField = value;
                }
            }

            /// <remarks/>
            public string ItemMainCategoryCode {
                get {
                    return this.itemMainCategoryCodeField;
                }
                set {
                    this.itemMainCategoryCodeField = value;
                }
            }

            /// <remarks/>
            public string ItemName {
                get {
                    return this.itemNameField;
                }
                set {
                    this.itemNameField = value;
                }
            }

            /// <remarks/>
            public decimal ItemPrice {
                get {
                    return this.itemPriceField;
                }
                set {
                    this.itemPriceField = value;
                }
            }

            /// <remarks/>
            public int ItemInStockQuantity {
                get {
                    return this.itemInStockQuantityField;
                }
                set {
                    this.itemInStockQuantityField = value;
                }
            }

            /// <remarks/>
            public string TSANextAvailableDate {
                get {
                    return this.tSANextAvailableDateField;
                }
                set {
                    this.tSANextAvailableDateField = value;
                }
            }

            /// <remarks/>
            public string TSATotalAvailable {
                get {
                    return this.tSATotalAvailableField;
                }
                set {
                    this.tSATotalAvailableField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class SalesTransactionTransactionLineTransactionLineCustomFields {

            private object tSADuePDFField;
            private decimal tSALineAmount;

            /// <remarks/>
            public object TSADuePDF {
                get {
                    return this.tSADuePDFField;
                }
                set {
                    this.tSADuePDFField = value;
                }
            }

            /// <remarks/>
            public decimal TSALineAmount {
                get {
                    return this.tSALineAmount;
                }
                set {
                    this.tSALineAmount = value;
                }
            }
        }

        #endregion

    }

}
