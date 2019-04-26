using Evolution.CommonService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Evolution.Models.Models;
using System.Xml.Serialization;
using System.IO;
using Evolution.DAL;
using Evolution.CompanyService;
using Evolution.CustomerService;
using Evolution.Enumerations;
using Evolution.LookupService;
using Evolution.ProductService;
using System.Globalization;

namespace Evolution.ShopifyImportService {
    public class ShopifyImportService : CommonService.CommonService {

        #region Private members

        private CompanyService.CompanyService _companyService = null;
        protected CompanyService.CompanyService CompanyService {
            get {
                if (_companyService == null) _companyService = new Evolution.CompanyService.CompanyService(db);
                return _companyService;
            }
        }

        private CustomerService.CustomerService _customerService = null;
        protected CustomerService.CustomerService CustomerService {
            get {
                if (_customerService == null) _customerService = new Evolution.CustomerService.CustomerService(db);
                return _customerService;
            }
        }

        private LookupService.LookupService _lookupService = null;
        protected LookupService.LookupService LookupService {
            get {
                if (_lookupService == null) _lookupService = new LookupService.LookupService(db);
                return _lookupService;
            }
        }

        private ProductService.ProductService _productService = null;
        protected ProductService.ProductService ProductService {
            get {
                if (_productService == null) _productService = new ProductService.ProductService(db);
                return _productService;
            }
        }

        #endregion

        #region Construction

        protected IMapper Mapper = null;

        public ShopifyImportService(Evolution.DAL.EvolutionEntities dbEntities) : base(dbEntities) {
        }

        #endregion

        #region Import Shopify order

        public ShopifyImportTempModel.Data ProcessXml(string fileName, string businessName) {
            ShopifyImportTempModel.Data model = new ShopifyImportTempModel.Data();
            XmlSerializer serializer = new XmlSerializer(typeof(ShopifyImportTempModel.Data));

            using (TextReader reader = new StreamReader(fileName)) {
                model = (ShopifyImportTempModel.Data)serializer.Deserialize(reader);
                reader.Close();
            }
            return model;
        }

        public ShopifyImportHeaderTemp MapOrderToTemp(string businessName, Dictionary<string, string> configDetails, ShopifyImportTempModel.DataOrder order, UserModel taskUser) {
            CompanyModel company = CompanyService.FindCompanyFriendlyNameModel(businessName);
            if (order == null) {
                return null;
            } else {
                if (company != null) {
                    CustomerModel customer = GetCustomer(company, configDetails["CustomerName"], order, taskUser);

                    ShopifyImportHeaderTemp temp = new ShopifyImportHeaderTemp();
                    temp.CompanyId = company.Id;
                    temp.CustomerId = customer.Id;
                    temp.SOStatus = (int)SalesOrderHeaderStatus.ConfirmedOrder;
                    temp.SOSubstatus = (int)SalesOrderHeaderSubStatus.Unpicked;
                    temp.LocationId = company.DefaultLocationID;
                    temp.TaxCodeId = customer.TaxCodeId;

                    // Grab the first product tax and use, if its 0 use the customer card
                    decimal taxRate = 0;
                    if(order.SalesOrderItems[0].tax_percent != null && order.SalesOrderItems[0].tax_percent != 0) {
                        taxRate = order.SalesOrderItems[0].tax_percent.Value;
                    } else {
                        taxRate = LookupService.FindTaxCodeModel(customer.TaxCodeId).TaxPercentageRate.Value;
                    }

                    temp.FreightCarrierId = customer.FreightCarrierId;
                    temp.MethodSignedId = LookupService.FindMethodSignedModel("Shopify").Id;
                    temp.FreightTermId = null;
                    var product = ProductService.FindProductModel(order.SalesOrderItems[0].sku, null, company, false);
                    temp.BrandCategoryId = ProductService.FindBrandCategoriesListModel(company.Id, 0, 1, 1, configDetails["BrandCategory"]).Items.FirstOrDefault().Id;
                    temp.SourceId = LookupService.FindLOVItemModel(LOVName.OrderSource, configDetails["DataSource"]).Id;
                    temp.StoreId = Convert.ToInt32(configDetails["StoreId"]);

                    var salesperson = CustomerService.FindBrandCategorySalesPersonsModel(company, customer, temp.BrandCategoryId.Value, SalesPersonType.AccountManager).FirstOrDefault();
                    if(salesperson != null) {
                        temp.SalesPersonId = salesperson.UserId;
                    } else {
                        temp.SalesPersonId = company.DefaultImportUserId;
                    }
                    
                    temp.TermsId = customer.PaymentTermId;
                    temp.ShippingMethodId = customer.ShippingMethodId;
                    temp.NextActionId = LookupService.FindSaleNextActionId(Enumerations.SaleNextAction.None);
                    temp.OrderTypeId = LookupService.FindLOVItemsModel(company, LOVName.OrderType).FirstOrDefault().Id;
                    temp.WebsiteTransactionId = order.ShopifyOrderId.ToString();
                    temp.OrderNumber = null;
                    temp.CustomerPO = order.OrderNumber.ToString();
                    string dateFormat = "yyyyMMddHHmmss";
                    temp.OrderDate = DateTimeOffset.ParseExact(order.OrderDate.ToString(), dateFormat, CultureInfo.InvariantCulture);
                    temp.RequiredDate = DateTimeOffset.ParseExact(order.OrderDate.ToString(), dateFormat, CultureInfo.InvariantCulture);
                    temp.CompanyName = order.shipping_address.company.ToString();
                    temp.EndUserName = order.shipping_address.name;

                    // Shipping Address
                    temp.ShipAddress1 = order.shipping_address.address1;
                    temp.ShipAddress2 = order.shipping_address.address2;
                    temp.ShipAddress3 = null;
                    temp.ShipAddress4 = null;
                    temp.ShipSuburb = order.shipping_address.city;
                    temp.ShipState = order.shipping_address.state;
                    temp.ShipPostcode = order.shipping_address.postcode;
                    temp.ShipCountryId = LookupService.FindCountryModel(order.shipping_address.country.ToString()).Id;
                    temp.Telephone = order.shipping_address.telephone;
                    temp.EmailAddress = order.shipping_address.email.ToString();

                    // SalesOrderItems
                    int lineNumber = 0;
                    foreach (var item in order.SalesOrderItems) {
                        ShopifyImportDetailTemp detail = new ShopifyImportDetailTemp();
                        detail.CompanyId = company.Id;
                        detail.WebsiteLineItemId = item.id.ToString();
                        detail.LineNumber = lineNumber;
                        detail.ProductDescription = item.name;
                        detail.ProductNumber = item.sku;
                        product = ProductService.FindProductModel(item.sku, null, company, false);
                        detail.ProductId = product.Id;
                        detail.OrderQty = item.quantity;
                        detail.TaxPercent = (item.tax_percent == null) ? 0 : item.tax_percent;
                        detail.TaxCodeId = customer.TaxCodeId;
                        detail.UnitPriceExTax = Math.Round((item.price / (1 + (Convert.ToDecimal(detail.TaxPercent)))), 2); // CHECK - Wrong calculations?
                        detail.UnitPriceTax = (item.tax == null) ? 0 : item.tax;
                        detail.Discount = item.discount;
                        detail.DiscountPercent = item.discount_percent;

                        detail.TaxCodeId = customer.TaxCodeId;
                        detail.AllocQty = 0;
                        detail.PickQty = 0;
                        detail.InvQty = 0;
                        detail.LineStatusId = (int)SalesOrderLineStatus.Unpicked;
                        detail.DateCreated = DateTimeOffset.Now;
                        detail.DateModified = DateTimeOffset.Now;
                        detail.ReallocationItem = true;

                        temp.ShopifyImportDetailTemps.Add(detail);
                        lineNumber += 100;
                    }

                    // Discount
                    if(order.Discount != null) {
                        temp.WholeOrderDiscount = Math.Abs(order.Discount.price);
                        // add new line - lookup product table with "#DSPR" -> create a new product() -> lineNumber += 100
                    }

                    // Shipping
                    temp.FreightGST = 0;
                    temp.FreightExGST = 0;
                    if (order.Shipping != null) {
                        int index = order.Shipping.label.IndexOf("(");
                        if (taxRate != 0) {
                            temp.FreightGST = Math.Round((order.Shipping.price / (Convert.ToDecimal(1 + (taxRate / 100)) * 10)), 2);
                            temp.FreightExGST = Math.Round((order.Shipping.price - Convert.ToDecimal(temp.FreightGST)), 2);
                        }
                        if (index == -1)
                            temp.FreightLabel = order.Shipping.label;
                        else
                            temp.FreightLabel = order.Shipping.label.Substring(0, index).Trim();
                    }

                    // SalesOrderExtraItems
                    if(order.SalesOrderExtraItems != null) {
                        foreach (var orderitem in order.SalesOrderExtraItems) {
                            if (orderitem.label == "DISCOUNT") {
                                temp.WholeOrderDiscount = orderitem.price;
                            } else if (orderitem.label.Contains("Shipment") || orderitem.label.Contains("Shipping")) {
                                int index = orderitem.label.IndexOf("(");

                                if (taxRate == 0) {
                                    temp.FreightExGST = temp.FreightExGST + orderitem.price;
                                } else {
                                    temp.FreightGST = Math.Round((orderitem.price / (Convert.ToDecimal(1 + (taxRate / 100)) * 10)), 2);
                                    temp.FreightExGST = Math.Round((orderitem.price - Convert.ToDecimal(temp.FreightGST)), 2);
                                }

                                if (index == -1)
                                    temp.FreightLabel = orderitem.label;
                                else
                                    temp.FreightLabel = orderitem.label.Substring(0, index).Trim();
                            } else {
                                temp.WholeOrderDiscount = 0;
                            }
                        }
                    }

                    // Comments
                    if(order.Comments != null) {
                        if (order.Comments.ToString().Contains("eBay")) {
                            temp.OrderComment = order.Comments.Comment.Replace(",", "").Replace("\"", "").Replace(System.Environment.NewLine, ""); // May need to truncate this string (nvarchar(255))
                            temp.Comments = "";
                        } else {
                            temp.Comments = order.Comments.Comment.Replace(",", "").Replace("\"", "").Replace(System.Environment.NewLine, ""); // May need to truncate this string (nvarchar(255))
                            temp.OrderComment = "";
                        }
                    }
                    
                    temp.WebsiteOrderNo = order.OrderNumber.ToString();
                    temp.Filespec = null;
                    temp.IsError = false;
                    temp.DeliveryWindowOpen = temp.OrderDate;
                    temp.DeliveryWindowClose = temp.DeliveryWindowOpen.Value.AddDays(1);
                    temp.ManualDWSet = false;
                    temp.ShippingMethodAccount = null;
                    temp.IsConfirmedAddress = true;
                    temp.IsManualFreight = true;
                    temp.FreightRate = 0;
                    temp.MinFreightPerOrder = 0;
                    temp.DeliveryInstructions = null;
                    temp.DeliveryContact = order.shipping_address.name;
                    temp.SignedBy = "Customer";
                    temp.DateSigned = temp.OrderDate;
                    temp.WarehouseInstructions = null;
                    temp.IsMSQProblem = false;
                    temp.EndUserName = order.shipping_address.name;
                    temp.IsOverrideMSQ = false;
                    temp.SiteName = configDetails["DataSource"];
                    temp.IsProcessed = false;
                    temp.IsRetailSale = true;
                    temp.IsRetailHoldingOrder = false;
                    return temp;
                }
                return null;
            }
        }

        public void SaveDataToTempTables(List<ShopifyImportHeaderTemp> orders) {
            db.CleanShopifyImportTempTables();
            foreach (var order in orders) {
                db.InsertShopifyImportFile(order);
            }
        }

        public List<ShopifyImportHeaderTemp> GetShopifyTempTableData() {
            return db.FindShopifyImportHeaderTempRecords().ToList();
        }

        public List<SalesOrderHeader> CopyTempDataToSalesModel(List<ShopifyImportHeaderTemp> headers, string businessName) {
            List<SalesOrderHeader> soHeaders = new List<SalesOrderHeader>();
            
            foreach (var order in headers) {
                SalesOrderHeader soh = MapTempToSalesOrderHeader(order, businessName);
                soh.SalesOrderDetails = MapTempToSalesDetails(order.ShopifyImportDetailTemps);
                soHeaders.Add(soh);
            }
            return soHeaders;
        }

        public bool SaveSalesOrders(List<SalesOrderHeader> orders) {
            db.CleanShopifyImportTempTables();
            foreach(var order in orders) {
                db.SaveShopifyData(order);
            }
            db.SaveChanges();
            return true;
        }

        #endregion

        #region Private Methods

        private CustomerModel GetCustomer(CompanyModel company, string customerName, ShopifyImportTempModel.DataOrder order, UserModel taskUser) {
            CustomerModel customer = CustomerService.FindCustomerModel(company.Id, customerName);
            if (customer == null) {
                customer = new CustomerModel();
                customer.CompanyId = company.Id;
                customer.Name = customerName;
                customer.CreatedDate = DateTime.Now;
                // NEED MORE DEFAULTS??
                //customer.CreatedById = ??
                
                CustomerService.SetCustomerDefaults(company, customer, order.shipping_address.country, order.shipping_address.postcode.ToString());
                CustomerService.InsertOrUpdateCustomer(customer, taskUser);
            }
            return customer;
        }

        private SalesOrderHeader MapTempToSalesOrderHeader(ShopifyImportHeaderTemp headerTemp, string businessName) {
            CompanyModel company = CompanyService.FindCompanyFriendlyNameModel(businessName);
            Customer customer = db.FindCustomer(company.Id, headerTemp.Customer.Name);
            SalesOrderHeader soHeader = new SalesOrderHeader();
            soHeader.CompanyId = headerTemp.CompanyId;
            soHeader.SourceId = headerTemp.SourceId;
            soHeader.CustomerId = headerTemp.CustomerId;
            soHeader.OrderTypeId = headerTemp.OrderTypeId;
            soHeader.OrderNumber = (int)LookupService.GetNextSequenceNumber(company, SequenceNumberType.SalesOrderNumber);
            soHeader.CustPO = headerTemp.CustomerPO;
            soHeader.OrderDate = headerTemp.OrderDate;
            soHeader.RequiredDate = headerTemp.OrderDate;
            soHeader.ShipAddress1 = headerTemp.ShipAddress1;
            soHeader.ShipAddress2 = headerTemp.ShipAddress2;
            soHeader.ShipAddress3 = headerTemp.ShipAddress3;
            soHeader.ShipAddress4 = headerTemp.ShipAddress4;
            soHeader.ShipSuburb = headerTemp.ShipSuburb;
            soHeader.ShipState = headerTemp.ShipState;
            soHeader.ShipPostcode = headerTemp.ShipPostcode;
            soHeader.ShipCountryId = headerTemp.ShipCountryId;
            soHeader.SOStatus = headerTemp.SOStatus;
            soHeader.SOSubstatus = headerTemp.SOSubstatus;
            soHeader.SalespersonId = headerTemp.SalesPersonId;
            soHeader.OrderComment = headerTemp.OrderComment;
            soHeader.LocationId = headerTemp.LocationId;
            soHeader.TermsId = headerTemp.TermsId;
            soHeader.FreightTermId = customer.FreightTermId;
            soHeader.ShippingMethodId = headerTemp.ShippingMethodId;
            soHeader.DeliveryWindowOpen = headerTemp.DeliveryWindowOpen;
            soHeader.DeliveryWindowClose = headerTemp.DeliveryWindowClose;
            soHeader.ManualDWSet = false;
            soHeader.ShipMethodAccount = customer.ShipMethodAccount;
            soHeader.NextActionId = headerTemp.NextActionId;
            soHeader.IsConfirmedAddress = headerTemp.IsConfirmedAddress;
            soHeader.IsManualFreight = headerTemp.IsManualFreight;
            soHeader.FreightRate = headerTemp.FreightRate;
            soHeader.MinFreightPerOrder = headerTemp.MinFreightPerOrder;
            soHeader.FreightCarrierId = headerTemp.FreightCarrierId;
            soHeader.DeliveryInstructions = customer.DeliveryInstructions;
            soHeader.DeliveryContact = customer.DeliveryContact;
            soHeader.SignedBy = headerTemp.SignedBy;
            soHeader.DateSigned = headerTemp.DateSigned;
            soHeader.MethodSignedId = headerTemp.MethodSignedId;
            soHeader.PrintedForm = headerTemp.PrintedForm;

            soHeader.WarehouseInstructions = "";
            if (!string.IsNullOrEmpty(headerTemp.FreightLabel)) soHeader.WarehouseInstructions += headerTemp.FreightLabel;
            if (!string.IsNullOrEmpty(headerTemp.CompanyName)) soHeader.WarehouseInstructions += " :: Company: " + headerTemp.CompanyName;
            if (!string.IsNullOrEmpty(headerTemp.Telephone)) soHeader.WarehouseInstructions += " :: Phone: " + headerTemp.Telephone;
            if (!string.IsNullOrEmpty(customer.WarehouseInstructions)) soHeader.WarehouseInstructions += " :: " + customer.WarehouseInstructions;

            soHeader.IsMSQProblem = headerTemp.IsMSQProblem;
            soHeader.EndUserName = headerTemp.EndUserName;
            soHeader.IsOverrideMSQ = headerTemp.IsOverrideMSQ;
            soHeader.SiteName = headerTemp.SiteName;
            soHeader.IsProcessed = headerTemp.IsProcessed.Value;
            soHeader.IsRetailSale = headerTemp.IsRetailSale.Value;
            soHeader.IsRetailHoldingOrder = headerTemp.IsRetailHoldingOrder.Value;
            soHeader.WebsiteOrderNo = headerTemp.WebsiteOrderNo;
            soHeader.BrandCategoryId = headerTemp.BrandCategoryId;

            return soHeader;
        }

        private List<SalesOrderDetail> MapTempToSalesDetails(ICollection<ShopifyImportDetailTemp> detailTemp) {
            List<SalesOrderDetail> soDetails = new List<SalesOrderDetail>();

            foreach (var item in detailTemp) {
                SalesOrderDetail soDetail = new SalesOrderDetail();
                soDetail.CompanyId = item.CompanyId.Value;
                soDetail.LineNumber = item.LineNumber;
                soDetail.ProductId = item.ProductId;
                soDetail.ProductDescription = item.ProductDescription;
                soDetail.UnitPriceExTax = item.UnitPriceExTax;
                soDetail.UnitPriceGST = item.UnitPriceTax;
                soDetail.DiscountPercent = item.DiscountPercent;
                soDetail.TaxCodeId = item.TaxCodeId;
                soDetail.OrderQty = item.OrderQty;
                soDetail.AllocQty = item.AllocQty;
                soDetail.PickQty = item.PickQty;
                soDetail.InvQty = item.InvQty;
                soDetail.LineStatusId = item.LineStatusId;
                soDetail.DateCreated = item.DateCreated;
                soDetail.DateModified = item.DateModified;

                soDetails.Add(soDetail);
            }
            return soDetails;
        }

        #endregion
    }
}
