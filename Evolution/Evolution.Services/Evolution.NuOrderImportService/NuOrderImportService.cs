using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Extensions;
using Evolution.Models.Models;
using Evolution.Enumerations;
using AutoMapper;
using Evolution.TaskService;
using Evolution.CompanyService;
using Evolution.CustomerService;
using Evolution.ProductService;
using Evolution.LookupService;
using Evolution.CSVFileService;
using Evolution.MembershipManagementService;
using System.Globalization;

namespace Evolution.NuOrderImportService {
    public class NuOrderImportService : CommonService.CommonService {

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
                if (_customerService == null) _customerService = new CustomerService.CustomerService(db);
                return _customerService;
            }
        }

        private ProductService.ProductService _productService = null;
        protected ProductService.ProductService ProductService {
            get {
                if (_productService == null) _productService = new ProductService.ProductService(db);
                return _productService;
            }
        }

        private LookupService.LookupService _lookupService = null;
        protected LookupService.LookupService LookupService {
            get {
                if (_lookupService == null) _lookupService = new LookupService.LookupService(db);
                return _lookupService;
            }
        }

        private MembershipManagementService.MembershipManagementService _membershipManagementService = null;
        protected MembershipManagementService.MembershipManagementService MembershipManagementService {
            get {
                if (_membershipManagementService == null) _membershipManagementService = new MembershipManagementService.MembershipManagementService(db);
                return _membershipManagementService;
            }
        }

        #endregion

        #region Contruction

        protected IMapper Mapper = null;

        public NuOrderImportService(EvolutionEntities dbEntities) : base(dbEntities) {

        }

        #endregion

        #region Import NuOrder order

        public List<Dictionary<string, string>> ProcessFile(string fileName, string businessName) {
            List<Dictionary<string, string>> orderList = new List<Dictionary<string, string>>();

            CSVFileService.CSVReader reader = new CSVReader();
            reader.OpenFile(fileName, true);
            Dictionary<string, string> fileLine = new Dictionary<string, string>();
            while ((fileLine = reader.ReadLine()) != null) {
                orderList.Add(fileLine);
            }
            reader.Close();

            if (orderList.Count() > 0) {
                return orderList;
            } else {
                return null;
            }
        }

        public List<NuOrderImportTemp> MapFileToTemp(string businessName, List<Dictionary<string, string>> orderLines, UserModel taskUser) {
            List<NuOrderImportTemp> nuOrderImportTempList = new List<NuOrderImportTemp>();

            CompanyModel company = CompanyService.FindCompanyFriendlyNameModel(businessName);
            if(orderLines == null) {
                return null;
            } else {
                foreach (var line in orderLines) {
                    CustomerModel customer = GetCustomer(company, line, taskUser);

                    NuOrderImportTemp noit = new NuOrderImportTemp();
                    noit.CompanyId = company.Id;
                    noit.SourceId = LookupService.FindLOVItemModel(LOVName.OrderSource, "Nuorder").Id;
                    noit.CustomerId = customer.Id;
                    noit.OrderNumber = null;
                    noit.CustomerPO = line["Order Number"];
                    noit.RequiredDate = DateTimeOffset.Parse(line["Order Date"]);
                    noit.CompanyName = line["Customer Code"];
                    noit.EndUserName = line["Buyer"];
                    noit.ShipAddress1 = line["Shipping Line 1"];
                    noit.ShipAddress2 = line["Shipping Line 2"];
                    noit.Filespec = null;
                    noit.Telephone = null;
                    noit.ShipAddress3 = line["Shipping Line 3"];
                    noit.ShipAddress4 = line["Shipping Line 4"];
                    noit.ShipSuburb = line["Shipping City"];
                    noit.ShipState = line["Shipping State"];
                    noit.ShipPostcode = line["Shipping Zip"];
                    noit.ShipCountryId = LookupService.FindCountryModel(line["Shipping Country"]).Id;
                    noit.SOStatus = (int)SalesOrderHeaderStatus.ConfirmedOrder;
                    noit.SOSubstatus = (int)SalesOrderHeaderSubStatus.Unpicked;
                    noit.SalesPersonId = MembershipManagementService.FindUserByAliasName(line["Sales Rep"].Replace(" ", ".")).Id;
                    noit.OrderComment = line["Order Notes"];
                    noit.LocationId = company.DefaultLocationID.Value;
                    noit.TermsId = customer.PaymentTermId;
                    noit.ShippingMethodId = customer.ShippingMethodId;
                    noit.TaxPercent = LookupService.FindTaxCodeModel(customer.TaxCodeId).TaxPercentageRate / 100;
                    noit.TaxCodeId = customer.TaxCodeId;
                    noit.WebsiteOrderNo = line["Order Number"];
                    noit.EmailAddress = null;
                    noit.Comments = line["Order Notes"];
                    noit.IsError = false;

                    // Date Fields are in US format, so need to Parse them accordingly
                    string dateFormat = "MM/dd/yyyy";
                    var orderDate = line["Order Date"];
                    noit.OrderDate = DateTimeOffsetExtensions.ParseDate(DateTimeOffset.ParseExact(orderDate, dateFormat, CultureInfo.InvariantCulture), TimeZoneInfo.Local.BaseUtcOffset.Hours, OffsetType.Hours);
                    var startDate = line["Ship Date Start"];
                    noit.DeliveryWindowOpen = (startDate == null || string.IsNullOrWhiteSpace(startDate)) ? noit.OrderDate : DateTimeOffsetExtensions.ParseDate(DateTimeOffset.ParseExact(startDate, dateFormat, CultureInfo.InvariantCulture), TimeZoneInfo.Local.BaseUtcOffset.Hours, OffsetType.Hours);
                    var endDate = line["Ship Date End"];
                    noit.DeliveryWindowClose = (endDate == null || string.IsNullOrWhiteSpace(endDate) ? LookupService.GetDeliveryWindow(noit.DeliveryWindowOpen.Value) : DateTimeOffsetExtensions.ParseDate(DateTimeOffset.ParseExact(endDate, dateFormat, CultureInfo.InvariantCulture), TimeZoneInfo.Local.BaseUtcOffset.Hours, OffsetType.Hours));

                    noit.ManualDWO = false;
                    noit.ManualDWC = true;
                    noit.ShippingMethodAccount = null;
                    noit.NextActionId = LookupService.FindSaleNextActionId(Enumerations.SaleNextAction.None);
                    noit.IsConfirmedAddress = true;
                    noit.IsManualFreight = false;
                    noit.FreightRate = customer.FreightRate;
                    noit.MinFreightPerOrder = customer.MinFreightPerOrder;
                    noit.FreightCarrierId = customer.FreightCarrierId;
                    noit.WarehouseInstructions = null;
                    noit.DeliveryInstructions = null;
                    noit.DeliveryContact = null;
                    noit.FreightTermId = null;
                    noit.OrderTypeId = null;
                    noit.SignedBy = "Customer";
                    noit.DateSigned = noit.OrderDate;
                    noit.MethodSignedId = LookupService.FindMethodSignedModel("NuOrder").Id;
                    //noit.PrintedForm = null;
                    noit.IsMSQProblem = false;
                    noit.IsOverrideMSQ = false;
                    noit.SiteName = "NuOrder";
                    noit.IsProcessed = false;
                    noit.IsRetailSale = false;
                    noit.IsRetailHoldingOrder = false;

                    noit.LineNumber = null;
                    var product = ProductService.FindProductModel(line["Style Number"], null, company, false);
                    noit.ItemId = product.Id;
                    noit.ItemNumber = line["Style Number"];
                    noit.ItemDescription = product.ItemDescription;
                    noit.BrandCategoryId = ProductService.FindProductBrandCategoryModel(company.Id, product.Id).Id;
                    noit.UnitPriceExTax = Convert.ToDecimal(line["Price Per"]);
                    noit.UnitPriceTax = Convert.ToDecimal(line["Price Per"]) * noit.TaxPercent;
                    var volumeDiscountPercent = customer.VolumeDiscount / 100;
                    noit.Discount = Convert.ToDecimal(line["Price Per"]) * volumeDiscountPercent;
                    noit.DiscountPercent = customer.VolumeDiscount;
                    noit.OrderQty = Convert.ToInt32(line["Quantity"]);
                    noit.AllocQty = 0;
                    noit.PickQty = 0;
                    noit.InvQty = 0;
                    noit.LineStatusId = (int)SalesOrderLineStatus.Unpicked;
                    noit.DateCreated = DateTimeOffset.Now;
                    noit.DateModified = DateTimeOffset.Now;

                    nuOrderImportTempList.Add(noit);
                }
            }
            

            if (SaveDataToTempTables(nuOrderImportTempList)) {
                return nuOrderImportTempList;
            } else {
                return null;
            }
        }

        #endregion

        #region Copy TEMP tables to Production

        public List<NuOrderImportTemp> GetTempTableData() {
            return db.FindNuOrderImportTempRecord().ToList();
        }

        public bool CopyTempDataToProduction(List<NuOrderImportTemp> orderLines, string businessName) {
            List<SalesOrderHeader> soHeaders = new List<SalesOrderHeader>();

            IEnumerable<IGrouping<string, NuOrderImportTemp>> query = orderLines.GroupBy(g => g.CustomerPO);
            foreach (IGrouping<string, NuOrderImportTemp> order in query) {

                SalesOrderHeader soh = MapTempToSalesHeader(order.ToList(), businessName);
                List<SalesOrderDetail> sod = MapTempToSalesDetails(order.ToList());
                foreach(var detail in sod)
                    soh.SalesOrderDetails.Add(detail);

                soHeaders.Add(soh);
            }

            if (soHeaders.Count > 0) {
                db.SaveNuOrderData(soHeaders);
                return true;
            }
            else
                return false;
        }

        #endregion

        #region Private Methods

        private CustomerModel GetCustomer(CompanyModel company, Dictionary<string, string> order, UserModel taskUser) {
            CustomerModel customer = CustomerService.FindCustomerModel(company.Id, order["Customer Code"]);
            if(customer == null) {
                customer = new CustomerModel();
                customer.CompanyId = company.Id;
                customer.Name = order["Customer Code"];
                customer.CreatedDate = DateTime.Now;
                var salesRep = order["Sales Rep"].Replace(" ", ".");
                customer.CreatedById = MembershipManagementService.FindUserByAliasName(salesRep).Id;

                CustomerService.SetCustomerDefaults(company, customer, order["Shipping Country"], order["Shipping Zip"]);
                CustomerService.InsertOrUpdateCustomer(customer, taskUser);
            }
            return customer;
        }

        private bool SaveDataToTempTables(List<NuOrderImportTemp> orders) {
            db.CleanNuOrderImportTempTable();
            foreach (var orderLine in orders) {
                db.InsertNuOrderImportOrderLine(orderLine);
            }
            return true;
        }

        private SalesOrderHeader MapTempToSalesHeader(List<NuOrderImportTemp> order, string businessName) {
            CompanyModel company = CompanyService.FindCompanyFriendlyNameModel(businessName);
            Customer customer = db.FindCustomer(company.Id, order[0].Customer.Name);
            SalesOrderHeader soHeader = new SalesOrderHeader();
            soHeader.CompanyId = order[0].CompanyId;
            soHeader.SourceId = order[0].SourceId;
            soHeader.CustomerId = order[0].CustomerId;
            soHeader.OrderNumber = (int)LookupService.GetNextSequenceNumber(company, SequenceNumberType.SalesOrderNumber);
            soHeader.CustPO = order[0].CustomerPO;
            soHeader.OrderDate = order[0].OrderDate;
            soHeader.RequiredDate = order[0].OrderDate;
            soHeader.ShipAddress1 = order[0].ShipAddress1;
            soHeader.ShipAddress2 = order[0].ShipAddress2;
            soHeader.ShipAddress3 = order[0].ShipAddress3;
            soHeader.ShipAddress4 = order[0].ShipAddress4;
            soHeader.ShipSuburb = order[0].ShipSuburb;
            soHeader.ShipState = order[0].ShipState;
            soHeader.ShipPostcode = order[0].ShipPostcode;
            soHeader.ShipCountryId = order[0].ShipCountryId;
            soHeader.SOStatus = order[0].SOStatus;
            soHeader.SOSubstatus = order[0].SOSubstatus;
            soHeader.SalespersonId = order[0].SalesPersonId;
            soHeader.OrderComment = order[0].OrderComment;
            soHeader.LocationId = order[0].LocationId;
            soHeader.TermsId = order[0].TermsId;
            soHeader.ShippingMethodId = order[0].ShippingMethodId;
            soHeader.DeliveryWindowOpen = order[0].DeliveryWindowOpen;
            soHeader.DeliveryWindowClose = order[0].DeliveryWindowClose;
            soHeader.ManualDWSet = false;
            soHeader.ShipMethodAccount = order[0].ShippingMethodAccount;
            soHeader.NextActionId = order[0].NextActionId;
            soHeader.IsConfirmedAddress = order[0].IsConfirmedAddress;
            soHeader.IsManualFreight = order[0].IsManualFreight;
            soHeader.FreightRate = order[0].FreightRate;
            soHeader.MinFreightPerOrder = order[0].MinFreightPerOrder;
            soHeader.FreightCarrierId = order[0].FreightCarrierId;
            soHeader.DeliveryInstructions = customer.DeliveryInstructions;
            soHeader.DeliveryContact = customer.DeliveryContact;
            soHeader.SignedBy = order[0].SignedBy;
            soHeader.DateSigned = order[0].DateSigned;
            soHeader.MethodSignedId = order[0].MethodSignedId;
            soHeader.PrintedForm = order[0].PrintedForm;
            soHeader.WarehouseInstructions = customer.WarehouseInstructions;
            soHeader.IsMSQProblem = order[0].IsMSQProblem;
            soHeader.EndUserName = order[0].EndUserName;
            soHeader.IsOverrideMSQ = order[0].IsOverrideMSQ;
            soHeader.FreightTermId = customer.FreightTermId;
            soHeader.SiteName = order[0].SiteName;
            soHeader.IsProcessed = order[0].IsProcessed.Value;
            soHeader.IsRetailSale = order[0].IsRetailSale.Value;
            soHeader.IsRetailHoldingOrder = order[0].IsRetailHoldingOrder.Value;
            soHeader.WebsiteOrderNo = order[0].WebsiteOrderNo;
            soHeader.BrandCategoryId = order[0].BrandCategoryId;
            soHeader.OrderTypeId = customer.OrderTypeId;
            return soHeader;
        }

        private List<SalesOrderDetail> MapTempToSalesDetails(List<NuOrderImportTemp> order) {
            List<SalesOrderDetail> soDetails = new List<SalesOrderDetail>();
            var lineNumber = 0;

            foreach(var line in order) {
                SalesOrderDetail soDetail = new SalesOrderDetail();
                soDetail.CompanyId = line.CompanyId;
                soDetail.LineNumber = lineNumber;
                soDetail.ProductId = line.ItemId;
                soDetail.ProductDescription = line.ItemDescription;
                soDetail.UnitPriceExTax = line.UnitPriceExTax;
                soDetail.UnitPriceGST = line.UnitPriceTax;
                soDetail.DiscountPercent = line.DiscountPercent;
                soDetail.TaxCodeId = line.TaxCodeId;
                soDetail.OrderQty = line.OrderQty;
                soDetail.AllocQty = line.AllocQty;
                soDetail.PickQty = line.PickQty;
                soDetail.InvQty = line.InvQty;
                soDetail.LineStatusId = line.LineStatusId;
                soDetail.DateCreated = line.DateCreated;
                soDetail.DateModified = line.DateModified;
                soDetails.Add(soDetail);

                lineNumber += 100;
            }
            return soDetails;
        }
        #endregion
    }
}
