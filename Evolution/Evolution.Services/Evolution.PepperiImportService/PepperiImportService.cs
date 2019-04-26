using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Extensions;
using Evolution.Models.Models;
using Evolution.Enumerations;
using AutoMapper;
using Evolution.CompanyService;
using Evolution.TaskService;
using System.Xml.Serialization;
using System.Xml;
using Evolution.LookupService;
using Evolution.CustomerService;
using Evolution.ProductService;
using Evolution.MembershipManagementService;

namespace Evolution.PepperiImportService 
{
    public class PepperiImportService : CommonService.CommonService
    {
        #region Private members

        private TaskService.TaskService _taskService = null;
        protected TaskService.TaskService TaskService {
            get {
                if (_taskService == null) _taskService = new TaskService.TaskService(db);
                return _taskService;
            }
        }

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

        #region Construction

        protected IMapper Mapper = null;

        public PepperiImportService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            //var config = new MapperConfiguration((cfg => {
            //    //cfg.CreateMap<PepperiImportHeaderTemp, PepperiTransactionTempModel.SalesTransactionTransactionHeader>();
            //    //cfg.CreateMap<PepperiImportDetailTemp, PepperiTransactionTempModel.SalesTransactionTransactionHeaderTransactionHeaderFields>();
            //    //cfg.CreateMap<PepperiTransactionTempModel.SalesTransactionTransactionHeader, PepperiImportHeaderTemp>();
            //    //cfg.CreateMap<PepperiTransactionTempModel.SalesTransactionTransactionLine, PepperiImportHeaderTemp>();
            //}));

            //Mapper = config.CreateMapper();
        }

        #endregion

        #region Import XML to TEMP tables 

        public bool ProcessXml(string fileName, string businessName, UserModel taskUser, ScheduledTask task) {
            TaskService.WriteTaskLog(task, $"Success: Processing file '{fileName}'", LogSeverity.Normal);

            var transaction = ReadFile(fileName, task);

            // Check if the file has data
            if (transaction.TransactionHeader.TransactionHeaderFields != null && transaction.TransactionHeader != null && transaction != null) {

                if(!string.IsNullOrEmpty(transaction.TransactionHeader.AccountFields.AccountName))
                    TaskService.WriteTaskLog(task, $"Success: Found customer: {transaction.TransactionHeader.AccountFields.AccountName}", LogSeverity.Normal);

                if (ProcessTransaction(businessName, transaction, taskUser, task)) {
                    TaskService.WriteTaskLog(task, $"Success: Saved to TEMP tables - {transaction.TransactionLines.Count()} lines processed", LogSeverity.Normal);

                    PepperiImportHeaderTemp piht = GetTempTableData(task);
                    if (piht != null && piht.PepperiImportDetailTemps.Count > 0) {
                        CopyTempDataToProduction(piht, piht.PepperiImportDetailTemps, task);
                        TaskService.WriteTaskLog(task, $"Success: Order {piht.OrderNumber} successfully saved to SALES tables", LogSeverity.Normal);
                        return true;
                    } else {
                        TaskService.WriteTaskLog(task, $"Error. One and or both TEMP tables contain no data", LogSeverity.Severe);
                        return false;
                    }
                } else {
                    return false;
                }
            } else {
                TaskService.WriteTaskLog(task, $"Error: The file '{fileName}' you are trying to import is empty / has no data", LogSeverity.Severe);
                return false;
            }
        }

        // DeserializeXmlFile
        public PepperiTransactionTempModel.SalesTransaction ReadFile(string fileName, ScheduledTask task) {
            PepperiTransactionTempModel.SalesTransaction transaction = new PepperiTransactionTempModel.SalesTransaction();

            XmlSerializer serializer = new XmlSerializer(typeof(PepperiTransactionTempModel.SalesTransaction));
            TextReader tReader = new StreamReader(fileName);
            try {
                transaction = (PepperiTransactionTempModel.SalesTransaction)serializer.Deserialize(tReader);
                tReader.Close();              
            } catch(Exception ex) {
                TaskService.WriteTaskLog(task, $"Error: There was a problem trying to import file '{fileName}'\r\n{ex.Message}", LogSeverity.Severe);
            }
            return transaction;
        }

        public bool ProcessTransaction(string businessName, PepperiTransactionTempModel.SalesTransaction transaction, UserModel taskUser, ScheduledTask task) {
            PepperiImportHeaderTemp transactionHeader = null;
            List<PepperiImportDetailTemp> transactionDetails = new List<PepperiImportDetailTemp>();

            try {
                db.CleanPepperiImportTempTables();
                transactionHeader = MapFileImportHeaderToTemp(businessName, transaction.TransactionHeader, taskUser, task);
                transactionDetails = MapFileImportDetailToTemp(businessName, transaction.TransactionLines, transaction.TransactionHeader, taskUser, task);
                transactionHeader.BrandCategoryId = transactionDetails[0].BrandCategoryId;

            } catch(Exception ex) {
                TaskService.WriteTaskLog(task, $"Error: @ProcessTransaction\r\n{ex.Message}", LogSeverity.Severe);
                return false;
            }

            if (transactionHeader != null && transactionDetails.Count > 0) {
                db.InsertPepperiImportFile(transactionHeader, transactionDetails);
                return true;
            } else {
                return false;
            }
        }

        public PepperiImportHeaderTemp MapFileImportHeaderToTemp(string businessName,
                                                    PepperiTransactionTempModel.SalesTransactionTransactionHeader th, 
                                                    UserModel taskUser,
                                                    ScheduledTask task) {
            PepperiImportHeaderTemp piht = new PepperiImportHeaderTemp();

            var company = CompanyService.FindCompanyFriendlyNameModel(businessName);
            var customer = GetCustomer(company, th, taskUser);

            if (company != null) {
                piht.CompanyId = company.Id;
                piht.WrntyId = Convert.ToInt32(th.TransactionHeaderFields.WrntyID);                                                                                                     
                piht.OrderType = th.TransactionHeaderFields.Type;
                piht.Status = th.TransactionHeaderFields.Status;
                piht.CreationDateTime = DateTimeOffset.Parse(th.TransactionHeaderFields.CreationDateTime);                                                                              
                piht.ModificationDateTime = DateTimeOffset.Parse(th.TransactionHeaderFields.ModificationDateTime);
                piht.ActionDateTime = DateTimeOffset.Parse(th.TransactionHeaderFields.ActionDateTime);
                piht.DeliveryDate = DateTimeOffsetExtensions.ParseDate(DateTimeOffset.Parse(th.TransactionHeaderFields.DeliveryDate), (TimeZoneInfo.Local.BaseUtcOffset.Hours * 60));   
                piht.Remark =  th.TransactionHeaderFields.Remark as string;                                                                                                              
                piht.CatalogId = th.CatalogFields.CatalogID;
                piht.CatalogDescription = th.CatalogFields.CatalogDescription as string;
                piht.CatalogPriceFactor = th.CatalogFields.CatalogPriceFactor;
                piht.CatalogExpirationDate = DateTimeOffsetExtensions.ParseDate(DateTimeOffset.Parse(th.CatalogFields.CatalogExpirationDate), (TimeZoneInfo.Local.BaseUtcOffset.Hours * 60));
                piht.AgentName = th.SalesRepFields.AgentName;
                piht.AgentExternalId = (th.SalesRepFields.AgentExternalID as string == null) ? 0 : Convert.ToInt64(th.SalesRepFields.AgentExternalID);
                piht.AgentEmail = th.SalesRepFields.AgentEmail;
                piht.AccountWrntyId = th.AccountFields.AccountWrntyID;
                piht.AccountExternalId = th.AccountFields.AccountExternalID;
                piht.AccountCreationDate = DateTimeOffset.Parse(th.AccountFields.AccountCreationDate);
                piht.AccountName = th.AccountFields.AccountName;
                piht.AccountPhone = th.AccountFields.AccountPhone;
                piht.AccountMobile = th.AccountFields.AccountMobile as string;
                piht.AccountFax = th.AccountFields.AccountFax as string;
                piht.AccountEmail = th.AccountFields.AccountEmail;
                piht.CustomerId = customer.Id;
                piht.AccountStreet = th.AccountFields.AccountStreet;
                piht.AccountCity = th.AccountFields.AccountCity;
                piht.AccountState = th.AccountFields.AccountState;
                piht.AccountCountryId = LookupService.FindCountryModel(th.AccountFields.AccountCountry).Id;
                piht.AccountCountry = th.AccountFields.AccountCountry;
                piht.AccountZipCode = th.AccountFields.AccountZipCode;
                piht.AccountPriceLevelName = th.AccountFields.AccountPriceLevelName as string;
                piht.BillToName = th.BillingFields.BillToName;
                piht.BillToStreet = th.BillingFields.BillToStreet;
                piht.BillToCity = th.BillingFields.BillToCity;
                piht.BillToState = th.BillingFields.BillToState;
                piht.BillToCountryId = LookupService.FindCountryModel(th.BillingFields.BillToCountry).Id;
                piht.BillToCountry = th.BillingFields.BillToCountry;
                piht.BillToZipCode = th.BillingFields.BillToZipCode;
                piht.BillToPhone = th.BillingFields.BillToPhone;
                piht.ShipToExternalId = th.ShippingFields.ShipToExternalID;
                piht.ShipToName = th.ShippingFields.ShipToName;
                piht.ShipToStreet = th.ShippingFields.ShipToStreet;
                piht.ShipToCity = th.ShippingFields.ShipToCity;
                piht.ShipToState = th.ShippingFields.ShipToState;
                piht.ShipToCountryId = LookupService.FindCountryModel(th.ShippingFields.ShipToCountry).Id;
                piht.ShipToCountry = th.ShippingFields.ShipToCountry;
                piht.ShipToZipCode = th.ShippingFields.ShipToZipCode;
                piht.ShipToPhone = th.ShippingFields.ShipToPhone;
                piht.Currency = th.Totals.Currency;
                piht.TotalItemsCount = th.Totals.TotalItemsCount;
                piht.SubTotal = th.Totals.SubTotal;
                piht.SubTotalAfterItemsDiscount = th.Totals.SubTotalAfterItemsDiscount;
                piht.GrandTotal = th.Totals.GrandTotal;
                piht.DiscountPercentage = th.Totals.DiscountPercentage;
                piht.TaxPercentage = th.Totals.TaxPercentage;
                piht.TSAGST = th.TransactionCustomFields.TSAGST;
                piht.TSADeliveryWindowOpen = (th.TransactionCustomFields.TSADeliveryWindowOpen == "") ? DateTimeOffset.Parse(th.TransactionHeaderFields.CreationDateTime) : DateTimeOffsetExtensions.ParseDate(DateTimeOffset.Parse(th.TransactionCustomFields.TSADeliveryWindowOpen), (TimeZoneInfo.Local.BaseUtcOffset.Hours * 60));
                piht.TSADeliveryWindowClose = (th.TransactionCustomFields.TSADeliveryWindowClose == "") ? LookupService.GetDeliveryWindow(piht.TSADeliveryWindowOpen.Value) : DateTimeOffsetExtensions.ParseDate(DateTimeOffset.Parse(th.TransactionCustomFields.TSADeliveryWindowClose), (TimeZoneInfo.Local.BaseUtcOffset.Hours * 60));
                piht.TSAOrderTakenBy = (th.TransactionCustomFields.TSAOrderTakenBy as string == null) ? th.TransactionCustomFields.TSAOrderTakenBy as string : th.TransactionCustomFields.TSAOrderTakenBy.ToString();
                piht.TSATaxRate = th.TransactionCustomFields.TSATaxRate;
                piht.TSASubTotalBeforeTax = th.TransactionCustomFields.TSASubTotalBeforeTax;
                piht.TSAGrandTotal = th.TransactionCustomFields.TSAGrandTotal;
                piht.SalespersonId = GetSalespersonId(th.SalesRepFields.AgentEmail);
                piht.Filespec = th.TransactionCustomFields.Filespec;
                piht.IsNewCustomer = th.TransactionCustomFields.IsNewCustomer;
                piht.OrderNumber = (int)LookupService.GetNextSequenceNumber(company, SequenceNumberType.SalesOrderNumber);
                piht.SOStatus = (int)SalesOrderHeaderStatus.ConfirmedOrder;
                piht.SOSubStatus = (int)SalesOrderHeaderSubStatus.Unpicked;
                piht.LocationId = company.DefaultLocationID.Value;
                piht.IsConfirmedAddress = (th.TransactionCustomFields.IsNewCustomer == true) ? false : true;
                piht.SignedBy = "Customer";
                piht.MethodSignedId = LookupService.FindMethodSignedModel("Pepperi").Id;
                piht.IsMSQProblem = false;
                piht.IsOverrideMSQ = false;
                piht.SourceId = LookupService.FindLOVItemModel(LOVName.OrderSource, "Pepperi").Id;
                piht.NextActionId = LookupService.FindSaleNextActionId(Enumerations.SaleNextAction.None);

            } else {
                TaskService.WriteTaskLog(task, $"Error: Failed to find company '{businessName}' - @MapFileImportHeaderToTemp", LogSeverity.Severe);
            }
            return piht;
        }

        public List<PepperiImportDetailTemp> MapFileImportDetailToTemp(string businessName,
                                                    PepperiTransactionTempModel.SalesTransactionTransactionLine[] tls,
                                                    PepperiTransactionTempModel.SalesTransactionTransactionHeader th, 
                                                    UserModel taskUser,
                                                    ScheduledTask task) {
            List<PepperiImportDetailTemp> details = new List<PepperiImportDetailTemp>();
            var lineNumber = 0;

            var company = CompanyService.FindCompanyFriendlyNameModel(businessName);
            var customer = GetCustomer(company, th, taskUser);

            if (company != null) {
                if (tls.Length != 0) {
                    foreach (PepperiTransactionTempModel.SalesTransactionTransactionLine tl in tls) {
                        var pidt = new PepperiImportDetailTemp();
                        pidt.CompanyId = company.Id;

                        var product = ProductService.FindProductModel(tl.ItemFields.ItemExternalID, null, company, false);
                        // Stop import if product is false
                        pidt.ProductId = product.Id;
                        pidt.BrandCategoryId = ProductService.FindProductBrandCategoryModel(company.Id, pidt.ProductId.Value).Id;
                        pidt.ItemWrntyId = tl.ItemFields.ItemWrntyID;
                        pidt.ItemExternalId = tl.ItemFields.ItemExternalID;
                        pidt.ItemMainCategory = tl.ItemFields.ItemMainCategory;
                        pidt.ItemMainCategoryCode = tl.ItemFields.ItemMainCategoryCode;
                        pidt.ItemName = tl.ItemFields.ItemName.Replace(",", "").Replace("'","");
                        pidt.ItemPrice = tl.ItemFields.ItemPrice;
                        pidt.ItemInStockQuantity = tl.ItemFields.ItemInStockQuantity;
                        pidt.TSANextAvailableDate = (tl.ItemFields.TSANextAvailableDate == "") ? (DateTimeOffset?)null : DateTimeOffsetExtensions.ParseDate(DateTimeOffset.Parse(tl.ItemFields.TSANextAvailableDate), (TimeZoneInfo.Local.BaseUtcOffset.Hours * 60));
                        pidt.TSATotalAvailable = (string.IsNullOrEmpty(tl.ItemFields.TSATotalAvailable)) ? 0 : Convert.ToInt32(tl.ItemFields.TSATotalAvailable);
                        pidt.TSADuePDF = tl.TransactionLineCustomFields.TSADuePDF as string;
                        pidt.TSALineAmount = tl.TransactionLineCustomFields.TSALineAmount;
                        pidt.UnitsQuantity = tl.TransactionLineFields.UnitsQuantity;
                        pidt.UnitPrice = tl.TransactionLineFields.UnitPrice;
                        pidt.UnitDiscountPercentage = tl.TransactionLineFields.UnitDiscountPercentage;
                        pidt.UnitPriceAfterDiscount = tl.TransactionLineFields.UnitPriceAfterDiscount;
                        pidt.TotalUnitsPriceAfterDiscount = tl.TransactionLineFields.TotalUnitsPriceAfterDiscount;
                        pidt.DeliveryDate = DateTimeOffsetExtensions.ParseDate(DateTimeOffset.Parse(tl.TransactionLineFields.DeliveryDate), (TimeZoneInfo.Local.BaseUtcOffset.Hours * 60));
                        pidt.TransactionWrntyId = tl.TransactionLineFields.TransactionWrntyID;
                        pidt.TransactionExternalId = (tl.TransactionLineFields.TransactionExternalID as string == null) ? (long?)null : Convert.ToInt64(tl.TransactionLineFields.TransactionExternalID);
                        pidt.LineNumber = lineNumber;
                        pidt.TaxCodeId = customer.TaxCodeId;
                        pidt.DiscountPercent = tl.TransactionLineFields.UnitDiscountPercentage;
                        details.Add(pidt);

                        lineNumber += 100;
                    }
                }
            } else {
                TaskService.WriteTaskLog(task, $"Error: Failed to find company '{businessName}' - @MapFileImportDetailToTemp", LogSeverity.Severe);
            }
            
            return details;
        }

        #endregion


        #region Copy TEMP tables to Production tables

        public PepperiImportHeaderTemp GetTempTableData(ScheduledTask task) {
            PepperiImportHeaderTemp piht = new PepperiImportHeaderTemp();
            SalesOrderHeader soHeader = new SalesOrderHeader();

            try {
                piht = db.FindPepperiImportHeaderTempRecord();
            } catch (Exception ex) {
                TaskService.WriteTaskLog(task, $"Error: There was an error retreiving data from the TEMP tables - @GetTempTableData" + ex.Message, LogSeverity.Severe);
            }
            return piht;
        }

        public SalesOrderHeader CopyTempDataToProduction(PepperiImportHeaderTemp piht, ICollection<PepperiImportDetailTemp> pidt, ScheduledTask task) {
            SalesOrderHeader salesOrderHeader = null;
            try {
                Customer customer = db.FindCustomer(piht.CompanyId, piht.AccountName);
                salesOrderHeader = MapTempToSalesHeader(piht, task, customer);
                salesOrderHeader.SalesOrderDetails = MapTempToSalesDetail(pidt, task, customer);
            } catch (Exception ex) {
                TaskService.WriteTaskLog(task, ex.Message, LogSeverity.Severe);
            }

            if(salesOrderHeader != null && salesOrderHeader.SalesOrderDetails.Count > 0) {
                try {
                    db.SavePepperiData(salesOrderHeader, salesOrderHeader.SalesOrderDetails);
                } catch (Exception ex) {
                    TaskService.WriteTaskLog(task, ex.Message, LogSeverity.Severe);
                }
            } else {
                TaskService.WriteTaskLog(task, $"Error. Couldn't save Pepperi data to production tables. No data in either table (Header or Details) to process - @CopyTempDataToProduction", LogSeverity.Severe);
            }
            return salesOrderHeader;
        }

        private SalesOrderHeader MapTempToSalesHeader(PepperiImportHeaderTemp piht, ScheduledTask task, Customer customer) {
            SalesOrderHeader salesOrderHeader = null;
            if (customer != null) {
                salesOrderHeader = new SalesOrderHeader();
                salesOrderHeader.CompanyId = piht.CompanyId;
                salesOrderHeader.SourceId = piht.SourceId;
                salesOrderHeader.CustPO = piht.WrntyId.ToString();
                salesOrderHeader.CustomerId = piht.CustomerId;
                salesOrderHeader.OrderNumber = piht.OrderNumber;
                salesOrderHeader.OrderDate = piht.CreationDateTime;
                salesOrderHeader.RequiredDate = piht.DeliveryDate;
                salesOrderHeader.ShipAddress1 = piht.ShipToStreet;
                salesOrderHeader.ShipSuburb = piht.ShipToCity;
                salesOrderHeader.ShipState = piht.ShipToState;
                salesOrderHeader.ShipPostcode = piht.ShipToZipCode;
                salesOrderHeader.ShipCountryId = (int)piht.ShipToCountryId;
                salesOrderHeader.SOStatus = piht.SOStatus;
                salesOrderHeader.SOSubstatus = piht.SOSubStatus;
                salesOrderHeader.SalespersonId = (int)piht.SalespersonId;
                salesOrderHeader.OrderComment = piht.Remark;
                salesOrderHeader.LocationId = piht.LocationId;
                salesOrderHeader.TermsId = customer.PaymentTermId;
                salesOrderHeader.ShippingMethodId = customer.ShippingMethodId;
                salesOrderHeader.DeliveryWindowOpen = piht.TSADeliveryWindowOpen;
                salesOrderHeader.DeliveryWindowClose = piht.TSADeliveryWindowClose;
                salesOrderHeader.ManualDWSet = false;
                salesOrderHeader.ShipMethodAccount = customer.ShipMethodAccount;
                salesOrderHeader.NextActionId = piht.NextActionId;
                salesOrderHeader.IsConfirmedAddress = piht.IsConfirmedAddress;
                salesOrderHeader.IsManualFreight = customer.IsManualFreight ?? false;
                salesOrderHeader.FreightRate = customer.FreightRate;
                salesOrderHeader.MinFreightPerOrder = customer.MinFreightPerOrder;
                salesOrderHeader.FreightCarrierId = customer.FreightCarrierId;
                salesOrderHeader.DeliveryContact = customer.DeliveryContact;
                salesOrderHeader.SignedBy = piht.SignedBy;
                salesOrderHeader.DateSigned = piht.CreationDateTime;
                salesOrderHeader.MethodSignedId = piht.MethodSignedId;
                //salesOrderHeader.PrintedForm = customer.PrintedForm; // **
                salesOrderHeader.WarehouseInstructions = customer.WarehouseInstructions;
                salesOrderHeader.DeliveryInstructions = customer.DeliveryInstructions;
                salesOrderHeader.IsMSQProblem = piht.IsMSQProblem;
                salesOrderHeader.IsOverrideMSQ = piht.IsOverrideMSQ;
                salesOrderHeader.FreightTermId = customer.FreightTermId;
                salesOrderHeader.IsProcessed = false;
                salesOrderHeader.IsRetailSale = false;
                salesOrderHeader.IsRetailHoldingOrder = false;
                salesOrderHeader.OrderTypeId = customer.OrderTypeId;
                salesOrderHeader.BrandCategoryId = piht.BrandCategoryId;
                salesOrderHeader.DateCreated = DateTimeOffset.Now;
            }
            return salesOrderHeader;
        }

        private List<SalesOrderDetail> MapTempToSalesDetail(ICollection<PepperiImportDetailTemp> pidt, ScheduledTask task, Customer customer) {
            List<SalesOrderDetail> salesOrderDetails = new List<SalesOrderDetail>();
            foreach (var line in pidt) {
                SalesOrderDetail salesOrderDetail = new SalesOrderDetail();
                salesOrderDetail.CompanyId = line.CompanyId;
                salesOrderDetail.LineNumber = line.LineNumber;
                salesOrderDetail.ProductId = line.ProductId;
                salesOrderDetail.ProductDescription = line.ItemName;
                salesOrderDetail.DiscountPercent = line.DiscountPercent;
                salesOrderDetail.UnitPriceExTax = Math.Round(line.UnitPrice.Value, 2);
                var taxRate = LookupService.FindTaxCodeModel(line.TaxCodeId.Value).TaxPercentageRate;
                salesOrderDetail.UnitPriceGST = Math.Round(line.UnitPrice.Value, 2) * (taxRate / 100);
                salesOrderDetail.TaxCodeId = customer.TaxCodeId;
                salesOrderDetail.OrderQty = 0;
                salesOrderDetail.AllocQty = 0;
                salesOrderDetail.PickQty = 0;
                salesOrderDetail.InvQty = 0;
                salesOrderDetail.LineStatusId = (int)SalesOrderLineStatus.Unpicked;
                salesOrderDetail.DateCreated = DateTimeOffset.Now;
                salesOrderDetail.DateModified = DateTimeOffset.Now;

                salesOrderDetails.Add(salesOrderDetail);
            }
            return salesOrderDetails;
        }

        #endregion


        #region Private Methods

        private long GetSalespersonId(string email) {
            var search = email.Substring(0, email.IndexOf('@'));
            return MembershipManagementService.FindUserModel(search).Id;
        }

        private CustomerModel GetCustomer(CompanyModel company, PepperiTransactionTempModel.SalesTransactionTransactionHeader transactionHeader, UserModel taskUser) {
            CustomerModel customer = CustomerService.FindCustomerModel(company.Id, transactionHeader.AccountFields.AccountName);

            if (customer == null) {
                customer = new CustomerModel();
                customer.CompanyId = company.Id;
                customer.Name = transactionHeader.AccountFields.AccountName;
                customer.CreatedDate = DateTime.Now;
                customer.CreatedById = (int)GetSalespersonId(transactionHeader.SalesRepFields.AgentEmail);

                CustomerService.SetCustomerDefaults(company, 
                                                    customer, 
                                                    transactionHeader.AccountFields.AccountCountry, 
                                                    transactionHeader.AccountFields.AccountZipCode);
                CustomerService.InsertOrUpdateCustomer(customer, taskUser);
            }
            return customer;
        }

        #endregion
    }
}
