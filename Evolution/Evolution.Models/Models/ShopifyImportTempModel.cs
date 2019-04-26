using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Evolution.Models.Models {
    public class ShopifyImportTempModel {


        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        // ***** DATA *****
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(ElementName = "Data", Namespace = "", IsNullable = false)]
        public partial class Data {

            private DataOrder[] orderField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("Order")]
            public DataOrder[] Order {
                get {
                    return this.orderField;
                }
                set {
                    this.orderField = value;
                }
            }
        }

        // ***** ORDER *****
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class DataOrder {

            private uint orderNumberField;

            private ulong shopifyOrderIdField;

            private string storeNameField;

            private ulong orderDateField;

            private DataOrderBilling_address billing_addressField;

            private DataOrderShipping_address shipping_addressField;

            private DataOrderSalesOrderItem[] salesOrderItemsField;

            private DataOrderDiscount discountField;

            private DataOrderShipping shippingField;

            private DataOrderSalesOrderExtraItem[] salesOrderExtraItemsField;

            private OrderComments commentsField;

            /// <remarks/>
            public uint OrderNumber {
                get {
                    return this.orderNumberField;
                }
                set {
                    this.orderNumberField = value;
                }
            }

            /// <remarks/>
            public ulong ShopifyOrderId {
                get {
                    return this.shopifyOrderIdField;
                }
                set {
                    this.shopifyOrderIdField = value;
                }
            }

            /// <remarks/>
            public string StoreName {
                get {
                    return this.storeNameField;
                }
                set {
                    this.storeNameField = value;
                }
            }

            /// <remarks/>
            public ulong OrderDate {
                get {
                    return this.orderDateField;
                }
                set {
                    this.orderDateField = value;
                }
            }

            /// <remarks/>
            public DataOrderBilling_address billing_address {
                get {
                    return this.billing_addressField;
                }
                set {
                    this.billing_addressField = value;
                }
            }

            /// <remarks/>
            public DataOrderShipping_address shipping_address {
                get {
                    return this.shipping_addressField;
                }
                set {
                    this.shipping_addressField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("SalesOrderItem", IsNullable = false)]
            public DataOrderSalesOrderItem[] SalesOrderItems {
                get {
                    return this.salesOrderItemsField;
                }
                set {
                    this.salesOrderItemsField = value;
                }
            }

            /// <remarks/>
            public DataOrderDiscount Discount {
                get {
                    return this.discountField;
                }
                set {
                    this.discountField = value;
                }
            }

            /// <remarks/>
            public DataOrderShipping Shipping {
                get {
                    return this.shippingField;
                }
                set {
                    this.shippingField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("SalesOrderExtraItem", IsNullable = false)]
            public DataOrderSalesOrderExtraItem[] SalesOrderExtraItems {
                get {
                    return this.salesOrderExtraItemsField;
                }
                set {
                    this.salesOrderExtraItemsField = value;
                }
            }

            /// <remarks/>
            public OrderComments Comments {
                get {
                    return this.commentsField;
                }
                set {
                    this.commentsField = value;
                }
            }
        }

        // ***** BILLING ADDRESS *****
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class DataOrderBilling_address {

            private string companyField;

            private string telephoneField;

            private string emailField;

            private string nameField;

            private string address1Field;

            private string address2Field;

            private string cityField;

            private string stateField;

            private string postcodeField;

            private string countryField;

            /// <remarks/>
            public string company {
                get {
                    return this.companyField;
                }
                set {
                    this.companyField = value;
                }
            }

            /// <remarks/>
            public string telephone {
                get {
                    return this.telephoneField;
                }
                set {
                    this.telephoneField = value;
                }
            }

            /// <remarks/>
            public string email {
                get {
                    return this.emailField;
                }
                set {
                    this.emailField = value;
                }
            }

            /// <remarks/>
            public string name {
                get {
                    return this.nameField;
                }
                set {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            public string address1 {
                get {
                    return this.address1Field;
                }
                set {
                    this.address1Field = value;
                }
            }

            /// <remarks/>
            public string address2 {
                get {
                    return this.address2Field;
                }
                set {
                    this.address2Field = value;
                }
            }

            /// <remarks/>
            public string city {
                get {
                    return this.cityField;
                }
                set {
                    this.cityField = value;
                }
            }

            /// <remarks/>
            public string state {
                get {
                    return this.stateField;
                }
                set {
                    this.stateField = value;
                }
            }

            /// <remarks/>
            public string postcode {
                get {
                    return this.postcodeField;
                }
                set {
                    this.postcodeField = value;
                }
            }

            /// <remarks/>
            public string country {
                get {
                    return this.countryField;
                }
                set {
                    this.countryField = value;
                }
            }
        }

        // ***** SHIIPPING ADDRESS *****
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class DataOrderShipping_address {

            private string companyField;

            private string telephoneField;

            private string emailField;

            private string nameField;

            private string address1Field;

            private string address2Field;

            private string cityField;

            private string stateField;

            private string postcodeField;

            private string countryField;

            /// <remarks/>
            public string company {
                get {
                    return this.companyField;
                }
                set {
                    this.companyField = value;
                }
            }

            /// <remarks/>
            public string telephone {
                get {
                    return this.telephoneField;
                }
                set {
                    this.telephoneField = value;
                }
            }

            /// <remarks/>
            public string email {
                get {
                    return this.emailField;
                }
                set {
                    this.emailField = value;
                }
            }

            /// <remarks/>
            public string name {
                get {
                    return this.nameField;
                }
                set {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            public string address1 {
                get {
                    return this.address1Field;
                }
                set {
                    this.address1Field = value;
                }
            }

            /// <remarks/>
            public string address2 {
                get {
                    return this.address2Field;
                }
                set {
                    this.address2Field = value;
                }
            }

            /// <remarks/>
            public string city {
                get {
                    return this.cityField;
                }
                set {
                    this.cityField = value;
                }
            }

            /// <remarks/>
            public string state {
                get {
                    return this.stateField;
                }
                set {
                    this.stateField = value;
                }
            }

            /// <remarks/>
            public string postcode {
                get {
                    return this.postcodeField;
                }
                set {
                    this.postcodeField = value;
                }
            }

            /// <remarks/>
            public string country {
                get {
                    return this.countryField;
                }
                set {
                    this.countryField = value;
                }
            }
        }

        // ***** SALES ORDER ITEMS *****
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class DataOrderSalesOrderItem {

            private uint idField;

            private string nameField;

            private string skuField;

            private byte quantityField;

            private decimal priceField;

            private decimal? taxField;

            private decimal? tax_percentField;

            private decimal discountField;

            private decimal discount_percentField;

            /// <remarks/>
            public uint id {
                get {
                    return this.idField;
                }
                set {
                    this.idField = value;
                }
            }

            /// <remarks/>
            public string name {
                get {
                    return this.nameField;
                }
                set {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            public string sku {
                get {
                    return this.skuField;
                }
                set {
                    this.skuField = value;
                }
            }

            /// <remarks/>
            public byte quantity {
                get {
                    return this.quantityField;
                }
                set {
                    this.quantityField = value;
                }
            }

            /// <remarks/>
            public decimal price {
                get {
                    return this.priceField;
                }
                set {
                    this.priceField = value;
                }
            }

            /// <remarks/>
            public decimal? tax {
                get {
                    return this.taxField;
                }
                set {
                    this.taxField = value;
                }
            }

            /// <remarks/>
            public decimal? tax_percent {
                get {
                    return this.tax_percentField;
                }
                set {
                    this.tax_percentField = value;
                }
            }

            /// <remarks/>
            public decimal discount {
                get {
                    return this.discountField;
                }
                set {
                    this.discountField = value;
                }
            }

            /// <remarks/>
            public decimal discount_percent {
                get {
                    return this.discount_percentField;
                }
                set {
                    this.discount_percentField = value;
                }
            }
        }

        // ***** DISCOUNT *****
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class DataOrderDiscount {

            private string labelField = "";

            private decimal priceField = 0;

            /// <remarks/>
            public string label {
                get {
                    return this.labelField;
                }
                set {
                    this.labelField = value;
                }
            }

            /// <remarks/>
            public decimal price {
                get {
                    return this.priceField;
                }
                set {
                    this.priceField = value;
                }
            }
        }

        // ***** SHIPPING *****
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class DataOrderShipping {

            private string labelField = "";

            private decimal priceField = 0;

            /// <remarks/>
            public string label {
                get {
                    return this.labelField;
                }
                set {
                    this.labelField = value;
                }
            }

            /// <remarks/>
            public decimal price {
                get {
                    return this.priceField;
                }
                set {
                    this.priceField = value;
                }
            }
        }

        // ***** EXTRA ITEMS *****
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class DataOrderSalesOrderExtraItem {

            private string labelField = "";

            private decimal priceField = 0;

            /// <remarks/>
            public string label {
                get {
                    return this.labelField;
                }
                set {
                    this.labelField = value;
                }
            }

            /// <remarks/>
            public decimal price {
                get {
                    return this.priceField;
                }
                set {
                    this.priceField = value;
                }
            }
        }

        // ***** COMMENTS *****
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class OrderComments {

            private string commentField = "";

            /// <remarks/>
            public string Comment {
                get {
                    return this.commentField;
                }
                set {
                    this.commentField = value;
                }
            }
        }
    }
}
