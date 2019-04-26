using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using System.Data;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.DAL {

    public partial class EvolutionEntities {

        #region Testing

        DbContextTransaction transaction = null;

        public bool IsTesting { get { return transaction != null; } }

        public void StartTest() {
            transaction = Database.BeginTransaction();
            // To view table contents of uncommitted transactions, use SSMS:
            //  set transaction isolation level read uncommitted;
        }

        public void EndTest() {
            int fileDeletes = 0,
                folderDeletes = 0;
            CleanupTestFiles();     // Cleanup files created in the transaction
            CleanupFileLogs(ref fileDeletes, ref folderDeletes, true);
            transaction.Rollback();
            transaction.Dispose();
            CleanupTestFiles();     // Cleanup any that get left behind
            CleanupFileLogs(ref fileDeletes, ref folderDeletes, true);
        }

        public void LogTestFile(List<string> fileNames) {
            if (IsTesting && fileNames != null) {
                foreach (var fileName in fileNames) {
                    LogTestFile(fileName);
                }
            }
        }

        public void LogTestFile(string fileName) {
            if (IsTesting) {
                TestFileLog testFileLog = new TestFileLog {
                    ItemType = 1,
                    Name = fileName
                };
                TestFileLogs.Add(testFileLog);
                SaveChanges();
            }
        }

        private bool isFolderDeletable(string folderName) {
            string[] undeletables = { @"C:\Development\Evolution\DataTransfers",
                                          @"C:\Development\Evolution\DataTransfers\n",
                                          @"C:\Development\Evolution\DataTransfers\n\UnpackSlips",
                                          @"C:\Development\Evolution\DataTransfers\n\UnpackSlips\Errors",
                                          @"C:\Development\Evolution\Evolution\Evolution\Media\n",
                                          @"C:\Development\Evolution\Evolution\Evolution\Media\n\Customer",
                                          @"C:\Development\Evolution\Evolution\Evolution\Media\n\Purchase",
                                          @"C:\Development\Evolution\Evolution\Evolution\Media\n\Sale",
                                          @"C:\Development\Evolution\Evolution\Evolution\Media\n\Supplier",
                                          @"C:\Development\Evolution\Evolution\Evolution\Media\n\Temp",
                                          Path.GetTempPath().ToLower().TrimEnd('\\')
                };

            bool bFound = false;
            for (int orgId = 1; orgId <= 3 && !bFound; orgId++) {
                for (int i = 0; i < undeletables.Length && !bFound; i++) {
                    string temp = undeletables[i].ToLower().Replace("\\n\\", "\\" + orgId.ToString() + "\\");
                    if (folderName.ToLower() == temp) {
                        bFound = true;
                    }
                }
            }

            return !bFound;
        }

        public void LogTestFolder(string folderName) {
            if (IsTesting) {
                if (isFolderDeletable(folderName)) {
                    TestFileLog testFileLog = new TestFileLog {
                        ItemType = 2,
                        Name = folderName
                    };
                    TestFileLogs.Add(testFileLog);
                    SaveChanges();
                }
            }
        }

        public void CleanupTestFiles() {
            // Delete all files queued for deletion
            for (int i = 1; i < 10; i++) {
                List<TestFileLog> testFileLogs = TestFileLogs.OrderByDescending(tfl => tfl.Id)
                                                             .ToList();
                if (testFileLogs.Count() == 0) {
                    i = 10;
                } else {
                    for (int j = 0; j < testFileLogs.Count(); j++) {
                        var file = testFileLogs[j];

                        bool bDelRec = true;

                        if (file.ItemType == 2) {
                            // Folder
                            if (Directory.Exists(file.Name)) {
                                // Delete the files
                                string[] fileList = null;
                                try {
                                    fileList = Directory.GetFiles(file.Name);
                                } catch { }
                                if (fileList != null) {
                                    foreach (var fileName in fileList) {
                                        try {
                                            File.Delete(fileName);
                                        } catch { }
                                    }
                                }

                                // Now delete the folder
                                try {
                                    Directory.Delete(file.Name, true);
                                } catch {
                                    bDelRec = false;    // So we try again later
                                }
                            } else {
                                // Doesn't exist, so just remove record
                            }

                        } else {
                            // File
                            if (File.Exists(file.Name)) {
                                try {
                                    File.Delete(file.Name);
                                } catch {
                                    bDelRec = false;    // So we try again later
                                }
                            } else {
                                // Doesn't exist, so just remove record
                            }
                        }

                        if (bDelRec) {
                            try {
                                TestFileLogs.RemoveRange(TestFileLogs.Where(tfl => tfl.Id == file.Id));
                                SaveChanges();
                            } catch { }
                        }
                    }
                }
            }
        }

        #endregion

        #region Allocation table

        public IQueryable<Allocation> FindAllocationsToPurchaseOrder(int purchaseOrderId) {
            return Allocations.Where(a => a.PurchaseOrderDetail.PurchaseOrderHeaderId == purchaseOrderId)
                              .OrderBy(a => a.Id);      // This ordering is required to facilitate splitting/reallocation on fifo basis
        }

        public IQueryable<Allocation> FindPurchaseAllocations(int purchaseLineId) {
            return Allocations.Where(a => a.PurchaseLineId == purchaseLineId);
        }

        public int FindPurchaseAllocationCount(int purchaseLineId) {
            // Parameter is the id of a PurchaseOrderDetail (line) record
            int rc = 0;
            var det = PurchaseOrderDetails.Where(pod => pod.Id == purchaseLineId)
                                          .FirstOrDefault();
            if (det != null) rc = (int)det.Allocations
                                          .Select(pa => pa.Quantity)
                                          .DefaultIfEmpty(0)
                                          .Sum();
            return rc;
        }

        public IQueryable<Allocation> FindAllocations(int companyId, int productId) {
            return Allocations.Where(a => a.CompanyId == companyId &&
                                          a.ProductId == productId);
        }

        public IQueryable<Allocation> FindAllocationsForCompany(int companyId) {
            return Allocations.Where(a => a.CompanyId == companyId);
        }

        public IQueryable<Allocation> FindAllocationsForSalesOrderDetail(int companyId, int saleOrderDetailId) {
            return Allocations.Where(a => a.CompanyId == companyId &&
                                          a.SaleLineId == saleOrderDetailId &&
                                          (a.PurchaseLineId == null || a.PurchaseLineId == 0));
        }

        public Allocation FindAllocation(int id) {
            return Allocations.Where(a => a.Id == id)
                              .SingleOrDefault();
        }

        public void InsertOrUpdateAllocation(Allocation allocation) {
            bool bNew = allocation.Id == 0;
            if (bNew) {
                Allocations.Add(allocation);
            } else {
                //Entry(allocation).State = EntityState.Modified;
            }
            SaveChanges();
        }

        public void DeleteAllocation(int id) {
            Allocations.RemoveRange(Allocations.Where(a => a.Id == id));

            deleteLock(typeof(Allocation).ToString(), id);
        }

        public void DeleteAllocationsForProduct(int companyId, int productId) {
            Allocations.RemoveRange(Allocations.Where(a => a.CompanyId == companyId &&
                                                      a.ProductId == productId));
            SaveChanges();
        }

        public void DeleteAllocationsForSaleLine(int companyId, int sodId) {
            Allocations.RemoveRange(Allocations.Where(a => a.CompanyId == companyId &&
                                                           a.SaleLineId == sodId));
            SaveChanges();
        }

        public void DeleteAllocationsForPurchaseLine(int companyId, int podId) {
            Allocations.RemoveRange(Allocations.Where(a => a.CompanyId == companyId &&
                                                           a.PurchaseLineId == podId));
            SaveChanges();
        }

        #endregion

        #region AuditLog table

        public IQueryable<AuditLog> FindAuditLogs(string tableName, int rowId = -1) {
            return AuditLogs.Where(al => al.TableName == tableName &&
                                         (rowId == -1 || al.RowId == rowId))
                            .OrderBy(al => al.ChangedDate);
        }

        public void InsertAuditLog(string tableName, string businessArea, int rowId, int userId,
                                   string fieldName, string beforeValue, string afterValue) {
            AuditLog log = new AuditLog {
                ChangedDate = DateTimeOffset.Now,
                TableName = tableName,
                BusinessArea = businessArea,
                RowId = rowId,
                UserId = userId,
                FieldName = fieldName,
                BeforeValue = beforeValue,
                AfterValue = afterValue
            };
            AuditLogs.Add(log);
            SaveChanges();
        }

        #endregion

        #region Brand table

        public IQueryable<Brand> FindBrands(bool bShowHidden = false) {
            return Brands.Where(b => (bShowHidden == true || b.Enabled == true))
                         .OrderBy(b => b.BrandName);
        }

        public Brand FindBrand(int id) {
            return Brands.Where(b => b.Id == id)
                         .SingleOrDefault();
        }

        public Brand FindBrand(string brandName) {
            return Brands.Where(b => b.BrandName == brandName)
                         .FirstOrDefault();
        }

        public void InsertOrUpdateBrand(Brand brand) {
            bool bNew = brand.Id == 0;
            if (bNew) Brands.Add(brand);
            SaveChanges();
        }

        public void DeleteBrand(int id) {
            Brands.RemoveRange(Brands.Where(b => b.Id == id));

            deleteLock(typeof(Brand).ToString(), id);
        }

        #endregion

        #region BrandBrandCategory table

        public IQueryable<BrandBrandCategory> FindBrandBrandCategories(int brandCategoryId) {
            return BrandBrandCategories.Where(bc => bc.BrandCategoryId == brandCategoryId);
        }

        public BrandBrandCategory FindBrandBrandCategory(int id) {
            return BrandBrandCategories.Where(bbc => bbc.Id == id)
                                       .SingleOrDefault();
        }

        public void InsertOrUpdateBrandBrandCategory(BrandBrandCategory bbc) {
            bool bNew = bbc.Id == 0;
            if (bNew) BrandBrandCategories.Add(bbc);
            SaveChanges();
        }

        public void DeleteBrandBrandCategory(int id) {
            BrandBrandCategories.RemoveRange(BrandBrandCategories.Where(bc => bc.Id == id));

            deleteLock(typeof(BrandBrandCategory).ToString(), id);
        }

        #endregion

        #region BrandCategory table

        public IQueryable<BrandCategory> FindBrandCategories(int companyId, bool bShowHidden = false) {
            return BrandCategories.Where(bc => bc.CompanyId == companyId &&
                                                (bShowHidden == true || bc.Enabled == true))
                                    .OrderBy(bc => bc.CategoryName);
        }

        public BrandCategory FindBrandCategory(int id) {
            return BrandCategories.Where(bc => bc.Id == id)
                                  .SingleOrDefault();
        }

        public BrandCategory FindBrandCategory(int companyId, string categoryName) {
            return BrandCategories.Where(bc => bc.CompanyId == companyId &&
                                               bc.CategoryName == categoryName)
                                  .FirstOrDefault();
        }

        public void InsertOrUpdateBrandCategory(BrandCategory category, List<int> brandIds = null) {
            bool bNew = category.Id == 0;
            if (bNew) BrandCategories.Add(category);
            SaveChanges();

            if (brandIds != null) {
                var bbcList = FindBrandBrandCategories(category.Id).ToList();

                // Delete all the items which are not in the new list
                foreach (var bbc in bbcList) {
                    if (brandIds.IndexOf(bbc.BrandId) == -1) {
                        // Not in new list, so delete it
                        DeleteBrandBrandCategory(bbc.Id);
                    }
                }

                // Add all the items in the new list which are not in the database
                foreach (var brandId in brandIds) {
                    if (bbcList.Where(bl => bl.BrandId == brandId).Count() == 0) {
                        // Not found, so add it
                        BrandBrandCategory newItem = new BrandBrandCategory {
                            CompanyId = category.CompanyId,
                            BrandCategoryId = category.Id,
                            BrandId = brandId
                        };
                        InsertOrUpdateBrandBrandCategory(newItem);
                    }
                }
            }
        }

        public void DeleteBrandCategory(int id) {
            BrandBrandCategories.RemoveRange(BrandBrandCategories.Where(bbc => bbc.BrandCategoryId == id));
            SaveChanges();

            BrandCategories.RemoveRange(BrandCategories.Where(bc => bc.Id == id));

            deleteLock(typeof(BrandCategory).ToString(), id);
        }

        public void DeleteBrandFromBrandCategory(int companyId, int brandCategoryId, int brandIdToRemove) {
            BrandBrandCategories.RemoveRange(BrandBrandCategories.Where(bbc => bbc.CompanyId == companyId &&
                                                                               bbc.BrandCategoryId == brandCategoryId &&
                                                                               bbc.BrandId == brandIdToRemove));
            SaveChanges();
        }

        #endregion

        #region BrandCategorySalesPerson table

        public IQueryable<BrandCategorySalesPerson> FindBrandCategorySalesPersons(int customerId, bool bShowHidden = false) {
            return BrandCategorySalesPersons.Where(sp => sp.CustomerId == customerId)
                                            .OrderBy(sp => sp.User.LastName)
                                            .ThenBy(sp => sp.LOVItem.ItemText);
        }

        public BrandCategorySalesPerson FindBrandCategorySalesPerson(int id) {
            return BrandCategorySalesPersons.Where(sp => sp.Id == id)
                                         .SingleOrDefault();
        }

        public void InsertOrUpdateBrandCategorySalesPerson(BrandCategorySalesPerson salesPerson) {
            // Check if record already exists
            bool bNew = salesPerson.Id == 0;
            if (bNew) {
                if (BrandCategorySalesPersons.Where(bcsp => bcsp.CompanyId == salesPerson.CompanyId &&
                                                            bcsp.BrandCategoryId == salesPerson.BrandCategoryId &&
                                                            bcsp.CustomerId == salesPerson.CustomerId &&
                                                            bcsp.UserId == salesPerson.UserId &&
                                                            bcsp.SalesPersonTypeId == salesPerson.SalesPersonTypeId)
                                            .Count() == 0) {
                    // Record doesn't exist, so add it
                    BrandCategorySalesPersons.Add(salesPerson);
                    SaveChanges();
                }
            } else {
                SaveChanges();
            }
        }

        public void DeleteBrandCategorySalesPerson(int id) {
            BrandCategorySalesPersons.RemoveRange(BrandCategorySalesPersons.Where(sp => sp.Id == id));

            deleteLock(typeof(BrandCategorySalesPerson).ToString(), id);
        }

        #endregion

        #region CarrierVessel

        public IQueryable<CarrierVessel> FindCarrierVessels(bool bShowHidden = false) {
            return CarrierVessels.Where(cv => (bShowHidden == true || cv.Enabled == true))
                                 .OrderBy(cv => cv.CarrierVesselName);
        }

        public CarrierVessel FindCarrierVessel(int id) {
            return CarrierVessels.Where(cv => cv.Id == id)
                                 .SingleOrDefault();
        }

        public void InsertOrUpdateCarrierVessel(CarrierVessel carrierVessel) {
            bool bNew = carrierVessel.Id == 0;
            if (bNew) CarrierVessels.Add(carrierVessel);
            SaveChanges();
        }

        public void DeleteCarrierVessel(int id) {
            CarrierVessels.RemoveRange(CarrierVessels.Where(cv => cv.Id == id));

            deleteLock(typeof(CarrierVessel).ToString(), id);
        }

        #endregion

        #region Company table

        public IQueryable<Company> FindCompanies(bool bShowHidden = false) {
            var items = Companies.Where(c => (bShowHidden == true || c.Enabled == true))
                                 .OrderBy(c => c.FriendlyName);
            foreach (var item in items) Entry(item).State = System.Data.Entity.EntityState.Detached;
            return items;
        }

        public Company FindCompany(int id) {
            return Companies.Where(c => c.Id == id)
                            .SingleOrDefault();
        }

        public Company FindParentCompany() {
            return Companies.Where(c => c.IsParentCompany == true)
                            .FirstOrDefault();
        }

        public void InsertOrUpdateCompany(Company company) {
            bool bNew = company.Id == 0;
            if (bNew) Companies.Add(company);
            SaveChanges();
        }

        #endregion

        #region ContainerType

        public IQueryable<ContainerType> FindContainerTypes(bool bShowHidden = false) {
            return ContainerTypes.Where(ct => (bShowHidden == true || ct.Enabled == true))
                             .OrderBy(ct => ct.ContainerType1);
        }

        public ContainerType FindContainerType(int id) {
            return ContainerTypes.Where(ct => ct.Id == id)
                                 .SingleOrDefault();
        }

        public void InsertOrUpdateContainerType(ContainerType containerType) {
            bool bNew = containerType.Id == 0;
            if (bNew) ContainerTypes.Add(containerType);
            SaveChanges();
        }

        public void DeleteContainerType(int id) {
            ContainerTypes.RemoveRange(ContainerTypes.Where(ct => ct.Id == id));

            deleteLock(typeof(ContainerType).ToString(), id);
        }

        #endregion

        #region Country table

        public IQueryable<Country> FindCountries(bool bShowHidden = false) {
            return Countries.Where(c => (bShowHidden == true || c.Enabled == true))
                            .OrderBy(c => c.CountryName);
        }

        public Country FindCountry(int id) {
            return Countries.Where(c => c.Id == id)
                            .SingleOrDefault();
        }

        public Country FindCountry(string countryName) {
            return Countries.Where(c => c.CountryName == countryName)
                            .FirstOrDefault();
        }

        public void InsertOrUpdateCountry(Country country) {
            bool bNew = country.Id == 0;
            if (bNew) Countries.Add(country);
            SaveChanges();
        }

        public void DeleteCountry(int id) {
            Countries.RemoveRange(Countries.Where(c => c.Id == id));

            deleteLock(typeof(Country).ToString(), id);
        }

        #endregion

        #region CreditCard table

        public IQueryable<CreditCard> FindCreditCards() {
            return CreditCards.OrderBy(cc => cc.CreditCardNo);
        }

        public IQueryable<CreditCard> FindCreditCards(int companyId, int customerId) {
            return CreditCards.Where(cc => cc.CompanyId == companyId &&
                                           cc.CustomerId == customerId)
                              .OrderBy(cc => cc.CreditCardNo);
        }

        public CreditCard FindCreditCard(int id) {
            return CreditCards.Where(cc => cc.Id == id)
                              .SingleOrDefault();
        }

        public void InsertOrUpdateCreditCard(CreditCard card) {
            bool bNew = card.Id == 0;
            if (bNew) CreditCards.Add(card);
            SaveChanges();
        }

        public void DeleteCreditCard(int id) {
            CreditCards.RemoveRange(CreditCards.Where(c => c.Id == id));

            deleteLock(typeof(CreditCard).ToString(), id);
        }

        public void RefreshCreditCards() {
            foreach (var entity in CreditCards) {
                Entry(entity).State = EntityState.Detached;
            }
        }

        #endregion

        #region CreditCardProvider table

        public IQueryable<CreditCardProvider> FindCreditCardProviders() {
            return CreditCardProviders.OrderBy(ccp => ccp.ProviderName);
        }

        #endregion

        #region CreditClaimHeader

        public IQueryable<CreditClaimHeader> FindCreditClaimHeaders() {
            return CreditClaimHeaders.OrderBy(cch => cch.Id);
        }

        public CreditClaimHeader FindCreditClaimHeader(int id) {
            return CreditClaimHeaders.Where(cch => cch.Id == id)
                                     .FirstOrDefault();
        }

        public void InsertOrUpdateCreditClaimHeader(CreditClaimHeader cch) {
            if (cch.Id == 0) CreditClaimHeaders.Add(cch);
            SaveChanges();
        }

        public void CleanCreditClaimTables() {
            CreditClaimLines.RemoveRange(CreditClaimLines);
            CreditClaimHeaders.RemoveRange(CreditClaimHeaders);
            SaveChanges();
        }

        #endregion

        #region CreditClaimReplacementOrder table

        public CreditClaimReplacementOrder FindCreditClaimReplacementOrderForSoh(int companyId, int sohId) {
            return CreditClaimReplacementOrders.Where(ccro => ccro.CompanyId == companyId &&
                                                              ccro.SalesOrderHeaderId == sohId)
                                               .FirstOrDefault();
        }

        public IQueryable<CreditClaimReplacementOrder> FindCreditClaimReplacementOrders() {
            return CreditClaimReplacementOrders.OrderBy(ccro => ccro.Id);
        }

        public CreditClaimReplacementOrder FindCreditClaimReplacementOrder(int id) {
            return CreditClaimReplacementOrders.Where(ccro => ccro.Id == id)
                                               .FirstOrDefault();
        }

        public void InsertOrUpdateCreditClaimReplacementOrder(CreditClaimReplacementOrder ccro) {
            if (ccro.Id == 0) CreditClaimReplacementOrders.Add(ccro);
            SaveChanges();
        }

        public void DeleteCreditClaimReplacementOrder(int id) {
            CreditClaimReplacementOrders.RemoveRange(CreditClaimReplacementOrders.Where(c => c.Id == id));
            deleteLock(typeof(CreditClaimReplacementOrder).ToString(), id);
        }
        
        #endregion

        #region Currencies table

        public IQueryable<Currency> FindCurrencies(bool bShowHidden = false) {
            return Currencies.Where(c => (bShowHidden == true || c.Enabled == true))
                             .OrderBy(c => c.CurrencyCode)
                             .ThenBy(c => c.CurrencyName);
        }

        public Currency FindCurrency(int id) {
            return Currencies.Where(c => c.Id == id)
                             .SingleOrDefault();
        }

        public Currency FindCurrency(string currencyCode) {
            return Currencies.Where(c => c.CurrencyCode == currencyCode)
                             .FirstOrDefault();
        }

        public void InsertOrUpdateCurrency(Currency currency) {
            bool bNew = currency.Id == 0;
            if (bNew) Currencies.Add(currency);
            SaveChanges();
        }

        public void DeleteCurrency(int id) {
            Currencies.RemoveRange(Currencies.Where(c => c.Id == id));

            deleteLock(typeof(Currency).ToString(), id);
        }

        #endregion

        #region Customer table

        public IQueryable<Customer> FindCustomers(int companyId, string sortColumn = "", SortOrder sortOrder = SortOrder.Asc, bool bShowHidden = false) {
            switch (sortColumn.ToLower()) {
            case "createddate":
            case "createddateiso":
                if (sortOrder == SortOrder.Desc) {
                    return Customers.Where(c => c.CompanyId == companyId &&
                                                (bShowHidden == true || c.Enabled == true))
                                    .OrderByDescending(c => c.CreatedDate);
                } else {
                    return Customers.Where(c => c.CompanyId == companyId &&
                                                (bShowHidden == true || c.Enabled == true))
                                    .OrderBy(c => c.CreatedDate);
                }
            case "name":
            default:
                if (sortOrder == SortOrder.Desc) {
                    return Customers.Where(c => c.CompanyId == companyId &&
                                                (bShowHidden == true || c.Enabled == true))
                                    .OrderByDescending(c => c.Name);
                } else {
                    return Customers.Where(c => c.CompanyId == companyId &&
                                                (bShowHidden == true || c.Enabled == true))
                                    .OrderBy(c => c.Name);
                }
            }
        }

        public Customer FindCustomer(int id) {
            return Customers.Where(c => c.Id == id)
                            .SingleOrDefault();
        }

        public Customer FindCustomer(int companyId, string customerName) {
            return Customers.Where(c => c.CompanyId == companyId &&
                                        c.Name == customerName)
                            .FirstOrDefault();
        }

        public void InsertOrUpdateCustomer(Customer customer) {
            bool bNew = customer.Id == 0;
            if (bNew) Customers.Add(customer);
            SaveChanges();
        }

        #endregion

        #region CustomerAddress table

        public IQueryable<CustomerAddress> FindCustomerAddresses(int customerId) {
            return CustomerAddresses.Where(ca => ca.CustomerId == customerId)
                                    .OrderBy(ca => ca.Street);
        }

        public CustomerAddress FindCustomerAddress(int id) {
            return CustomerAddresses.Where(ca => ca.Id == id)
                                    .SingleOrDefault();
        }

        public void InsertOrUpdateCustomerAddress(CustomerAddress address) {
            bool bNew = address.Id == 0;
            if (bNew) CustomerAddresses.Add(address);
            SaveChanges();
        }

        public void DeleteCustomerAddress(int id) {
            CustomerAddresses.RemoveRange(CustomerAddresses.Where(ca => ca.Id == id));

            deleteLock(typeof(CustomerAddress).ToString(), id);
        }

        public void RefreshCustomerAddresses() {
            foreach(var entity in CustomerAddresses) {
                Entry(entity).State = EntityState.Detached;
            }
        }

        #endregion

        #region CustomerConflict table

        public IQueryable<CustomerConflictSensitivity> FindCustomerConflicts(int customerId) {
            return CustomerConflictSensitivities.Where(cc => cc.CustomerId == customerId);
        }

        public CustomerConflictSensitivity FindCustomerConflict(int id) {
            return CustomerConflictSensitivities.Where(cc => cc.Id == id)
                                                .SingleOrDefault();
        }

        public void InsertOrUpdateCustomerConflict(CustomerConflictSensitivity conflict) {
            bool bNew = conflict.Id == 0;
            if (bNew) CustomerConflictSensitivities.Add(conflict);
            SaveChanges();
        }

        public void DeleteCustomerConflict(int id) {
            CustomerConflictSensitivities.RemoveRange(CustomerConflictSensitivities.Where(cc => cc.Id == id));

            deleteLock(typeof(CustomerConflictSensitivity).ToString(), id);
        }

        #endregion

        #region CustomerContact table

        public IQueryable<CustomerContact> FindCustomerContacts(int customerId) {
            return CustomerContacts.Where(cc => cc.CustomerId == customerId)
                                   .OrderBy(cc => cc.ContactSurname);
        }

        public CustomerContact FindCustomerContact(int customerId, string firstname, string surname) {
            return CustomerContacts.Where(cc => cc.CustomerId == customerId &&
                                                cc.ContactFirstname == firstname &&
                                                cc.ContactSurname == surname)
                                   .FirstOrDefault();
        }

        public CustomerContact FindCustomerContact(int id) {
            return CustomerContacts.Where(cc => cc.Id == id)
                                   .SingleOrDefault();
        }

        public void InsertOrUpdateCustomerContact(CustomerContact contact) {
            bool bNew = contact.Id == 0;
            if (bNew) CustomerContacts.Add(contact);
            SaveChanges();
        }

        public void DeleteCustomerContact(int id) {
            CustomerContacts.RemoveRange(CustomerContacts.Where(cc => cc.Id == id));

            deleteLock(typeof(CustomerContact).ToString(), id);
        }

        #endregion

        #region CustomerDefault table

        public IQueryable<CustomerDefault> FindCustomerDefaults(int companyId) {
            return CustomerDefaults.Where(cd => cd.CompanyId == companyId)
                                   .OrderBy(cd => cd.Country.CountryName)
                                   .ThenBy(cd => cd.Postcode);
        }

        public CustomerDefault FindCustomerDefaults(int companyId, int? countryId, string postCode) {
            CustomerDefault defaults = null;

            if (countryId != null && !string.IsNullOrEmpty(postCode)) {
                defaults = CustomerDefaults.Where(cd => cd.CompanyId == companyId &&
                                                        cd.CountryId == countryId &&
                                                        cd.Postcode == postCode)
                                           .FirstOrDefault();
            }
            if(defaults == null && countryId != null) {
                defaults = CustomerDefaults.Where(cd => cd.CompanyId == companyId &&
                                                        cd.CountryId == countryId &&
                                                        string.IsNullOrEmpty(cd.Postcode))
                                           .FirstOrDefault();
            }
            if(defaults == null) {
                defaults = CustomerDefaults.Where(cd => cd.CompanyId == companyId &&
                                                        cd.CountryId == null &&
                                                        string.IsNullOrEmpty(cd.Postcode))
                                           .FirstOrDefault();
            }
            return defaults;
        }

        public CustomerDefault FindCustomerDefault(int id) {
            return CustomerDefaults.Where(cd => cd.Id == id)
                                   .SingleOrDefault();
        }

        public void InsertOrUpdateCustomerDefault(CustomerDefault defaults) {
            if (defaults.Id == 0) CustomerDefaults.Add(defaults);
            SaveChanges();
        }

        public void DeleteCustomerDefault(int id) {
            CustomerDefaults.RemoveRange(CustomerDefaults.Where(cd => cd.Id == id));
            SaveChanges();
        }

        public void RefreshCustomerDefaults() {
            foreach (var entity in CustomerDefaults) {
                Entry(entity).State = EntityState.Detached;
            }
        }

        #endregion

        #region DeliveryWindow table

        public DeliveryWindow FindDeliveryWindow(int ddmm) {
            return DeliveryWindows.Where(dw => ddmm >= dw.MMDDStart &&
                                               ddmm <= dw.MMDDEnd)
                                  .FirstOrDefault();
        }

        #endregion

        #region DocumentTemplate table

        public IQueryable<DocumentTemplate> FindDocumentTemplates(bool bShowHidden = false) {
            return DocumentTemplates.Where(dt => (dt.Enabled == true || bShowHidden))
                                    .OrderBy(dt => dt.Name);
        }

        public IQueryable<DocumentTemplate> FindDocumentTemplates(DocumentTemplateCategory templateCategory, bool bShowHidden = false) {
            return DocumentTemplates.Where(dt => (dt.Enabled == true || bShowHidden) &&
                                                 dt.TemplateCategory == (int)templateCategory)
                                    .OrderBy(dt => dt.Name);
        }

        public DocumentTemplate FindDocumentTemplate(int id) {
            return DocumentTemplates.Where(dt => dt.Id == id)
                                    .SingleOrDefault();
        }

        public DocumentTemplate FindDocumentTemplate(string templateName) {
            return DocumentTemplates.Where(dt => dt.Name == templateName)
                                    .FirstOrDefault();
        }

        public void InsertOrUpdateDocumentTemplate(DocumentTemplate template) {
            bool bNew = template.Id == 0;
            if (bNew) DocumentTemplates.Add(template);
            SaveChanges();
        }

        public void DeleteDocumentTemplate(int id) {
            DocumentTemplates.RemoveRange(DocumentTemplates.Where(dt => dt.Id == id));

            deleteLock(typeof(DocumentTemplate).ToString(), id);
        }

        #endregion

        #region EMailQueue table

        public IQueryable<EMailQueue> FindEMailQueues() {
            return EMailQueues.OrderBy(eq => eq.QueuedDate);
        }

        public void AddEMailQueue(string sender, string recipient, string subject, string message) {
            EMailQueue email = new EMailQueue {
                QueuedDate = DateTimeOffset.Now,
                SenderAddress = sender,
                ReplyToAddress = sender,
                RecipientAddress = recipient,
                MessageSubject = subject,
                MessageText = message,
                Retries = 0
            };
            InsertOrUpdateEMailQueue(email);
        }

        public void InsertOrUpdateEMailQueue(EMailQueue email) {
            bool bNew = email.Id == 0;
            if (bNew) {
                email.QueuedDate = DateTimeOffset.Now;
                email.Retries = 0;      // Number of retries so far
                EMailQueues.Add(email);
            }
            SaveChanges();
        }

        public void EmptyEMailQueue() {
            foreach (var email in EMailQueues.ToList()) {
                DeleteEMailQueue(email.Id);
            }
        }

        public void DeleteEMailQueue(int id) {
            // Deleting an email queue record removes the record, attachment records and attachment files.
            // The files are stored in a tempoarry location and are NOT the 'live' files.
            var email = EMailQueues.Where(eq => eq.Id == id)
                                   .SingleOrDefault();
            if (email != null) {
                // Found the email
                string attachmentFolder = "";

                var firstAttachment = email.EMailQueueAttachments.FirstOrDefault();
                if (firstAttachment != null) {
                    // Got at least one attachment, so use the path name in the first one
                    // as the location of temp attachment files for this email
                    attachmentFolder = firstAttachment.FileName.FolderName();
                }

                // Delete the attachment records
                EMailQueueAttachments.RemoveRange(email.EMailQueueAttachments);
                SaveChanges();

                // Delete the email record
                EMailQueues.Remove(email);
                SaveChanges();

                // Remove the attachment folder (and all attachment temp files in it)
                if (!string.IsNullOrEmpty(attachmentFolder)) {
                    try {
                        string[] files = Directory.GetFiles(attachmentFolder);
                        foreach (var file in files) {
                            try {
                                File.Delete(file);
                            } catch { }
                        }
                    } catch { }
                    try {
                        Directory.Delete(attachmentFolder);
                    } catch { }
                }
            }
        }

        public void DeleteEMailQueue(EMailQueue email) {
            DeleteEMailQueue(email.Id);
        }

        #endregion

        #region EMailQueueAttachment table

        public void InsertOrUpdateEMailQueueAttachment(EMailQueueAttachment attachment) {
            bool bNew = attachment.Id == 0;
            if (bNew) EMailQueueAttachments.Add(attachment);
            SaveChanges();
        }

        #endregion

        #region FileImportField

        public void InsertOrUpdateFileImportField(FileImportField fif, bool bSaveChanges = true) {
            bool bNew = fif.Id == 0;
            if (bNew) FileImportFields.Add(fif);
            if (bSaveChanges) {
                SaveChanges();
            }
        }

        public void RefreshFileImportFields() {
            foreach (var entity in FileImportFields) {
                Entry(entity).Reload();
            }
        }

        #endregion

        #region FileImportFile table

        public void InsertFileImportFile(int companyId, int userId, string text) {
            var row = new FileImportFile {
                CompanyId = companyId,
                UserId = userId,
                Text = text
            };
            FileImportFiles.Add(row);
            SaveChanges();
        }

        #endregion

        #region FileImportRow table

        public void ClearFileImportRows(int companyId, int userId) {
            FileImportFiles.RemoveRange(FileImportFiles.Where(fir => fir.CompanyId == companyId &&
                                                                     fir.UserId == userId));
            SaveChanges();

            foreach (var item in FindFileImportRows(companyId, userId).ToList()) {
                FileImportFields.RemoveRange(item.FileImportFields);
                FileImportRows.Remove(item);
                SaveChanges();
            }
        }

        public IQueryable<FileImportRow> FindFileImportRows(int companyId, int userId) {
            return FileImportRows.Where(fir => fir.CompanyId == companyId &&
                                               fir.UserId == userId)
                                 .OrderBy(fir => fir.Id);
        }

        public FileImportRow FindFileImportRow(int companyId, int userId, int id) {
            return FileImportRows.Where(fir => fir.CompanyId == companyId &&
                                               fir.UserId == userId &&
                                               fir.Id == id)
                                 .FirstOrDefault();
        }

        public void InsertOrUpdateFileImportRow(FileImportRow fir) {
            bool bNew = fir.Id == 0;
            if (bNew) FileImportRows.Add(fir);
            SaveChanges();
        }

        #endregion

        #region FileLog table

        // Records temporary files which are to be deleted after a period of time

        public void AddFileToLog(string fileName, int minutes) {
            var log = new FileLog {
                ItemType = 1,
                CreatedDate = DateTimeOffset.Now,
                ItemName = fileName,
                DeleteAfterDate = DateTimeOffset.Now.AddMinutes(minutes)
            };
            InsertOrUpdateFileLog(log);
        }

        public void AddFolderToLog(string folderName, int minutes) {
            if (isFolderDeletable(folderName)) {
                var log = new FileLog {
                    ItemType = 2,
                    CreatedDate = DateTimeOffset.Now,
                    ItemName = folderName,
                    DeleteAfterDate = DateTimeOffset.Now.AddMinutes(minutes)
                };
                InsertOrUpdateFileLog(log);
            }
        }

        public List<FileLog> FindFileLogs(bool bAll = false) {
            List<FileLog> logs;
            if (bAll) {
                logs = FileLogs.OrderByDescending(fl => fl.CreatedDate)
                               .ToList();
            } else {
                var now = DateTimeOffset.Now;
                logs = FileLogs.Where(fl => now > fl.DeleteAfterDate)
                               .OrderByDescending(fl => fl.CreatedDate)
                               .ToList();
            }
            return logs;
        }

        public void CleanupFileLogs(ref int folderDeletes, ref int fileDeletes, bool bAll = false) {
            folderDeletes = 0;
            fileDeletes = 0;

            foreach (var log in FindFileLogs(bAll)) {
                bool bError = false;
                switch (log.ItemType) {
                    case 1:
                        // File
                        try {
                            if (File.Exists(log.ItemName)) File.Delete(log.ItemName);
                            fileDeletes++;
                        } catch {
                            bError = true;
                        }
                        break;

                    case 2:
                        // Folder
                        try {
                            if (Directory.Exists(log.ItemName)) Directory.Delete(log.ItemName);
                            folderDeletes++;
                        } catch {
                            bError = true;
                        }
                        break;
                }
                if (!bError) {
                    FileLogs.Remove(log);
                    SaveChanges();
                }
            }
        }

        public void InsertOrUpdateFileLog(FileLog log) {
            if (log.Id == 0) FileLogs.Add(log);
            SaveChanges();
        }

        #endregion

        #region FileTransferConfiguration table

        public IQueryable<FileTransferConfiguration> FindFileTransferConfigurations(bool bShowHidden = false) {
            return FileTransferConfigurations.Where(ftc => (bShowHidden == true || ftc.Enabled == true))
                                             .OrderBy(ftc => ftc.TransferName);
        }

        public FileTransferConfiguration FindFileTransferConfiguration(int id) {
            return FileTransferConfigurations.Where(ftc => ftc.Id == id)
                                             .SingleOrDefault();
        }

        public FileTransferConfiguration FindFileTransferConfiguration(string transferName) {
            return FileTransferConfigurations.Where(ftc => ftc.TransferName == transferName)
                                             .SingleOrDefault();
        }

        public FileTransferConfiguration FindFileTransferConfiguration(int locationId, 
                                                                       FileTransferType transferType, 
                                                                       FileTransferDataType dataType) {
            // Find a data transfer for a location for the required direction and data type
            return FileTransferConfigurations.Where(ftc => ftc.LocationId == locationId &&
                                                           ftc.TransferType == (int)transferType &&
                                                           ftc.LOVItem_DataType.ItemValue1 == ((int)dataType).ToString() &&
                                                           ftc.Enabled == true)
                                             .FirstOrDefault();
        }

        public void InsertOrUpdateFileTransferConfiguration(FileTransferConfiguration config) {
            bool bNew = config.Id == 0;
            if (bNew) FileTransferConfigurations.Add(config);
            SaveChanges();
        }

        public void DeleteFileTransferConfiguration(int id) {
            FileTransferConfigurations.RemoveRange(FileTransferConfigurations.Where(ftc => ftc.Id == id));

            deleteLock(typeof(FileTransferConfiguration).ToString(), id);
        }

        #endregion

        #region FreightCarrier table

        public IQueryable<FreightCarrier> FindFreightCarriers(int companyId, bool bShowHidden = false) {
            return FreightCarriers.Where(c => c.CompanyId == companyId &&
                                              (bShowHidden == true || c.Enabled == true))
                                  .OrderBy(c => c.FreightCarrier1);
        }

        public FreightCarrier FindFreightCarrier(int id) {
            return FreightCarriers.Where(c => c.Id == id)
                                  .SingleOrDefault();
        }

        public FreightCarrier FindFreightCarrier(int companyId, string freightCarrierName) {
            return FreightCarriers.Where(c => c.CompanyId == companyId &&
                                              c.FreightCarrier1 == freightCarrierName)
                                  .FirstOrDefault();
        }

        public void InsertOrUpdateFreightCarrier(FreightCarrier carrier) {
            bool bNew = carrier.Id == 0;
            if (bNew) FreightCarriers.Add(carrier);
            SaveChanges();
        }

        public void DeleteFreightCarrier(int id) {
            FreightCarriers.RemoveRange(FreightCarriers.Where(c => c.Id == id));

            deleteLock(typeof(FreightCarrier).ToString(), id);
        }

        #endregion

        #region FreightForwarders table

        public IQueryable<FreightForwarder> FindFreightForwarders(int companyId = -1, bool bShowHidden = false) {
            return FreightForwarders.Where(ff => (companyId == -1 || ff.CompanyId == companyId) &&
                                                 (bShowHidden == true || ff.Enabled == true))
                                    .OrderBy(ff => ff.Name);
        }

        public FreightForwarder FindFreightForwarder(int id) {
            return FreightForwarders.Where(ff => ff.Id == id)
                                    .SingleOrDefault();
        }

        public FreightForwarder FindFreightForwarder(int companyId, string forwarderName) {
            return FreightForwarders.Where(ff => ff.CompanyId == companyId &&
                                                 ff.Name == forwarderName)
                                    .FirstOrDefault();
        }

        public void InsertOrUpdateFreightForwarder(FreightForwarder freightForwarder) {
            bool bNew = freightForwarder.Id == 0;
            if (bNew) FreightForwarders.Add(freightForwarder);
            SaveChanges();
        }

        public void DeleteFreightForwarder(int id) {
            FreightForwarders.RemoveRange(FreightForwarders.Where(ff => ff.Id == id));

            deleteLock(typeof(FreightForwarder).ToString(), id);
        }

        #endregion

        #region Location table

        public IQueryable<Location> FindLocations(int companyId = -1, bool bShowHidden = false) {
            return Locations.Where(l => (companyId == -1 || l.CompanyId == companyId) &&
                                        (bShowHidden == true || l.Enabled == true))
                            .OrderBy(l => l.LocationName);
        }

        public Location FindLocation(int id) {
            return Locations.Where(l => l.Id == id)
                            .SingleOrDefault();
        }

        public Location FindLocation(int companyId, string locationIdentification) {
            return Locations.Where(l => l.CompanyId == companyId &&
                                        l.LocationIdentification == locationIdentification)
                            .FirstOrDefault();
        }

        public void InsertOrUpdateLocation(Location location) {
            bool bNew = location.Id == 0;
            if (bNew) Locations.Add(location);
            SaveChanges();
        }

        public void DeleteLocation(int id) {
            Locations.RemoveRange(Locations.Where(ff => ff.Id == id));

            deleteLock(typeof(Location).ToString(), id);
        }

        #endregion

        #region Lock table

        private const int lockPeriod = 20;      // Minutes
        private Object thisLock = new Object();

        public string LockRecord(string tableName, int rowId) {
            // Returns the GUID of the lock obtained
            string lockGuid = "",
                    tempTableName = adjustTableName(tableName);

            // Remove all old locks
            deleteExpiredLocks();

            // Look for a lock on the requested table/row
            if (rowId > 0) {
                lock (thisLock) {
                    Lock recordLock = Locks.Where(l => l.TableName == tempTableName &&
                                                       l.LockedRowId == rowId)
                                           .FirstOrDefault();
                    if (recordLock == null) {
                        // No lock record found, so create one
                        recordLock = new Lock {
                            TableName = tempTableName,
                            LockedRowId = rowId,
                            TimeStamp = DateTimeOffset.Now,
                            LockGuid = Guid.NewGuid().ToString()
                        };
                        Locks.Add(recordLock);
                        SaveChanges();
                    }
                    lockGuid = recordLock.LockGuid;
                }
            }

            return lockGuid;
        }

        private string updateLock(string tableName, int rowId) {
            // Called by all record update methods to increment locks to
            // trigger 'updated by another user'
            string lockGuid = "",
                    tempTableName = adjustTableName(tableName);

            if (rowId > 0) {
                // Records with a rowId of zero are new so we don't create locks for them
                lock (thisLock) {
                    Lock recordLock = Locks.Where(l => l.TableName == tempTableName &&
                                                       l.LockedRowId == rowId)
                                           .FirstOrDefault();
                    if (recordLock == null) {
                        recordLock = new Lock {
                            TableName = tempTableName,
                            LockedRowId = rowId,
                            TimeStamp = DateTimeOffset.Now,
                            LockGuid = Guid.NewGuid().ToString()
                        };
                        Locks.Add(recordLock);
                    } else {
                        recordLock.LockGuid = Guid.NewGuid().ToString();
                    }
                    SaveChanges();
                    lockGuid = recordLock.LockGuid;
                }
            }
            return lockGuid;
        }

        public Lock FindLock(string tableName, int rowId) {
            string tempTableName = adjustTableName(tableName);
            return Locks.Where(l => l.TableName == tempTableName &&
                                    l.LockedRowId == rowId)
                        .FirstOrDefault();
        }

        string adjustTableName(string tableName) {
            string tempTableName = tableName;
            int pos = tempTableName.LastIndexOf(".");
            if (pos != -1) tempTableName = tempTableName.Substring(pos + 1);
            return tempTableName;
        }

        public bool IsLockStillValid(string tableName, int rowId, string lockGuid) {
            // This method is called before a save to check if a lock has expired
            if (rowId == 0) {
                // Id 0 = new record which doesn't physically exist to be locked
                return true;

            } else {
                DateTimeOffset cutoffTime = DateTimeOffset.Now.AddMinutes(-1 * lockPeriod);

                string tempTableName = adjustTableName(tableName);

                Lock recordLock = Locks.Where(l => l.TableName == tempTableName &&
                                                   l.LockedRowId == rowId &&
                                                   l.TimeStamp > cutoffTime &&
                                                   l.LockGuid == lockGuid)
                                       .FirstOrDefault();
                if (recordLock == null) {
                    // Lock not found or it has changed
                    return false;
                } else {
                    // Lock valid, so update it to prevent other users saving.
                    // This assumes that after calling this method, there will always be
                    // a database record update.
                    updateLock(tableName, rowId);
                    return true;
                }
            }
        }

        private void deleteLock(string tableName, int rowId) {

            string tempTableName = adjustTableName(tableName);

            if (!string.IsNullOrEmpty(tempTableName) && rowId > 0) {
                Locks.RemoveRange(Locks.Where(l => l.TableName == tempTableName &&
                                                   l.LockedRowId == rowId));
            }
            deleteExpiredLocks();
        }

        private void deleteExpiredLocks() {
            DateTimeOffset cutoffTime = DateTimeOffset.Now.AddMinutes(-1 * lockPeriod);

            Locks.RemoveRange(Locks.Where(l => l.TimeStamp < cutoffTime));
            SaveChanges();
        }

        #endregion

        #region Log table

        public Log FindLog(int id) {
            return Logs.Where(l => l.Id == id)
                       .SingleOrDefault();
        }

        public IQueryable<Log> FindLogs() {
            return Logs.OrderBy(l => l.LogDate);
        }

        public IQueryable<Log> FindLogs(string search,
                                        int section,
                                        int severity,
                                        DateTimeOffset? dateFrom, DateTimeOffset? dateTo) {
            // If the dateFrom has milliseconds, truncate them so that we round down 
            // to the nearest second asthis is the grandularity the user can see
            DateTimeOffset? dtFrom = dateFrom;
            if (dtFrom != null && dtFrom.Value.Millisecond > 0) dtFrom = dtFrom.Value.AddMilliseconds(dtFrom.Value.Millisecond * -1);       // Truncate on whole seconds

            // Move the dateTo to the next second and truncate the milliseconds so the we can do a < match and capture all
            // records with the same second even with varying miliseconds
            DateTimeOffset? dtTo = dateTo;
            if (dtTo != null) dtTo = dtTo.Value.AddMilliseconds(dtTo.Value.Millisecond * -1).AddSeconds(1);   // Round up to the next whole second

            return Logs.Where(l => l.LogSection == section &&
                                   (severity == (int)LogSeverity.All || l.Severity == severity) &&
                                   (dateFrom == null || l.LogDate >= dtFrom) &&
                                   (dateTo == null || l.LogDate < dtTo) &&
                                   (string.IsNullOrEmpty(search) ||
                                    (l.Message != null && l.Message.ToLower().Contains(search.ToLower())) ||
                                    (l.StackTrace != null && l.StackTrace.ToLower().Contains(search.ToLower())))
                              )
                       .OrderBy(l => l.LogDate);
        }

        public IQueryable<Log> FindLogsAfter(DateTime dt, LogSeverity severity) {
            return Logs.Where(l => l.LogDate >= dt &&
                                   l.Severity >= (int)severity)
                       .OrderBy(l => l.LogDate);
        }

        public int WriteLog(Exception ex, string rawUrl = "") {
            string msg = "Error: " + ex.Message;
            if (ex.InnerException != null) {
                if (ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message)) msg += "\r\n" + ex.InnerException.InnerException.Message;
                if (!string.IsNullOrEmpty(ex.InnerException.Message)) msg += "\r\n" + ex.InnerException.Message;
            }

            return WriteLog(LogSection.SystemError,
                            LogSeverity.Severe,
                            rawUrl,
                            msg,
                            ex.StackTrace);
        }

        public int WriteLog(LogSection section, LogSeverity severity, string url = "", string message = "", string stackTrace = "") {
            // The following uses a stored procedure to ensure that we can
            // always write to the Log even if EntityFramework is in conflict.
            // If we try to use EF to write to the log when EF is in conflict, the
            // log may not be written to.
            return WriteToLog((int)section, (int)severity, url, message, stackTrace).First().Value;
        }

        public int DeleteLogsOlderThan(int keepDays) {
            DateTimeOffset dt = DateTimeOffset.Now.AddDays(keepDays * -1);

            int rc = Logs.Where(l => l.LogDate < dt).Count();

            Logs.RemoveRange(Logs.Where(l => l.LogDate < dt));
            SaveChanges();

            return rc;
        }

        #endregion

        #region LOV table

        public IQueryable<LOV> FindLOVs(bool bMultiTenanted, bool bShowHidden = false) {
            return LOVs.Where(lov => lov.MultiTenanted == bMultiTenanted &&
                                     (bShowHidden == true || lov.Enabled == true))
            .OrderBy(lov => lov.LOVName);
        }

        public LOV FindLOV(int id) {
            return LOVs.Where(lov => lov.Id == id)
                       .SingleOrDefault();
        }

        public LOV FindLOV(string lovName) {
            return LOVs.Where(lov => lov.LOVName == lovName)
                       .SingleOrDefault();
        }

        public void InsertOrUpdateLOV(LOV lov) {
            bool bNew = lov.Id == 0;
            if (bNew) LOVs.Add(lov);
            SaveChanges();
        }

        #endregion

        #region LOVItem table

        public List<LOVItem> FindLOVItems(int? companyId, string lovName, bool bShowHidden = false) {
            var lov = FindLOV(lovName);
            return FindLOVItems(companyId, lov.Id, bShowHidden);
        }

        public List<LOVItem> FindLOVItems(int? companyId, int lovId, bool bShowHidden = false) {
            var lov = FindLOV(lovId);
            if (lov == null) {
                return new List<LOVItem>();

            } else if (lov.MultiTenanted) {
                // List is multi-tenanted, so return the items for the current tenant
                return lov.LOVItems.Where(li => li.CompanyId == companyId.Value &&
                                                (bShowHidden == true || li.Enabled == true))
                                   .OrderBy(li => li.OrderNo)
                                   .ThenBy(li => li.ItemText)
                                   .ToList();
            } else {
                // List is not multi-tenanted, so return all items
                return lov.LOVItems.Where(li => li.LovId == lovId &&
                                                (bShowHidden == true || li.Enabled == true))
                                   .OrderBy(li => li.OrderNo)
                                   .ThenBy(li => li.ItemText)
                                   .ToList();
            }
        }

        public LOVItem FindLOVItemByValue1(int? companyId, string lovName, string value) {
            var lov = FindLOV(lovName);
            return FindLOVItemByValue1(companyId, lov.Id, value);
        }

        public LOVItem FindLOVItemByValue1(int? companyId, int lovId, string value) {
            LOVItem result = null;
            var lov = FindLOV(lovId);
            if (lov != null) {
                if (lov.MultiTenanted) {
                    result = lov.LOVItems
                                .Where(li => li.CompanyId == companyId.Value &&
                                             li.ItemValue1 == value)
                                .FirstOrDefault();
                } else {
                    result = lov.LOVItems
                                .Where(li => li.ItemValue1 == value)
                                .FirstOrDefault();
                }
            }
            return result;
        }


        public LOVItem FindLOVItem(int id) {
            return LOVItems.Where(li => li.Id == id)
                           .SingleOrDefault();
        }

        public LOVItem FindLOVItem(int? companyId, int lovId, string itemText) {
            return LOVItems.Where(li => (companyId == null || li.CompanyId == companyId) &&
                                        li.LovId == lovId &&
                                        li.ItemText == itemText)
                           .FirstOrDefault();
        }

        public void MoveLOVItemUp(int? companyId, int lovId, int id) {
            List<LOVItem> lovItems = FindLOVItems(companyId, lovId, true).ToList();
            OrderLOVItems(lovItems);

            // Find the item to be moved
            int found = -1;
            for (int i = 0; found == -1 && i < lovItems.Count(); i++) if (lovItems[i].Id == id) found = i;
            if (found > 0) {
                // Found it, so move it
                int temp = lovItems[found].OrderNo;
                lovItems[found].OrderNo = lovItems[found - 1].OrderNo;
                lovItems[found - 1].OrderNo = temp; ;
            }
            SaveLOVItems(lovItems);
        }

        public void MoveLOVItemDown(int? companyId, int lovId, int id) {
            List<LOVItem> lovItems = FindLOVItems(companyId, lovId, true).ToList();
            OrderLOVItems(lovItems);

            // Find the item to be moved
            int found = -1;
            for (int i = 0; found == -1 && i < lovItems.Count(); i++) if (lovItems[i].Id == id) found = i;
            if (found != -1 && found < lovItems.Count() - 1) {
                // Found it, so move it
                int temp = lovItems[found].OrderNo;
                lovItems[found].OrderNo = lovItems[found + 1].OrderNo;
                lovItems[found + 1].OrderNo = temp; ;
            }
            SaveLOVItems(lovItems);
        }

        void OrderLOVItems(List<LOVItem> lovItems) {
            for (int i = 0; i < lovItems.Count(); i++) lovItems[i].OrderNo = i + 1;
        }

        void SaveLOVItems(List<LOVItem> lovItems) {
            foreach (var item in lovItems) InsertOrUpdateLOVItem(item);
        }

        public void InsertOrUpdateLOVItem(LOVItem item) {
            bool bNew = item.Id == 0;

            var lov = FindLOV(item.LovId);
            if (!lov.MultiTenanted) {
                item.Company = null;    // Multi-tenanted lists don't have a company
                item.CompanyId = null;
            }

            if (bNew) LOVItems.Add(item);
            SaveChanges();
        }

        public void DeleteLOVItem(int id) {
            LOVItems.RemoveRange(LOVItems.Where(li => li.Id == id));

            deleteLock(typeof(LOVItem).ToString(), id);
        }

        public void CopyLOVs(int sourceId, int targetId) {
            CopyListsOfValues(sourceId, targetId);
        }

        #endregion

        #region MarketingGroup table

        public IQueryable<MarketingGroup> FindMarketingGroups(int companyId, bool bShowHidden = false) {
            return MarketingGroups.Where(mg => mg.CompanyId == companyId &&
                                               (bShowHidden == true || mg.Enabled == true))
                                  .OrderBy(mg => mg.MarketingGroupName);
        }

        public MarketingGroup FindMarketingGroup(int id) {
            return MarketingGroups.Where(mg => mg.Id == id)
                                  .SingleOrDefault();
        }

        public MarketingGroup FindMarketingGroup(int companyId, string marketingGroupName) {
            return MarketingGroups.Where(mg => mg.CompanyId == companyId &&
                                               mg.MarketingGroupName == marketingGroupName)
                                  .FirstOrDefault();
        }

        public void InsertOrUpdateMarketingGroup(MarketingGroup marketingGroup) {
            bool bNew = marketingGroup.Id == 0;
            if (bNew) MarketingGroups.Add(marketingGroup);
            SaveChanges();
        }

        public void DeleteMarketingGroup(int id) {
            MarketingGroups.RemoveRange(MarketingGroups.Where(mg => mg.Id == id));

            deleteLock(typeof(MarketingGroup).ToString(), id);
        }

        #endregion

        #region MarketingGroupSubscription table

        public IQueryable<MarketingGroupSubscription> FindMarketingGroupSubscriptions(int customerId) {
            return MarketingGroupSubscriptions.Where(mgs => mgs.CustomerId == customerId)
                                              .OrderByDescending(mgs => mgs.DateFrom)
                                              .ThenByDescending(mgs => mgs.DateTo);
        }

        public MarketingGroupSubscription FindMarketingGroupSubscription(int id) {
            return MarketingGroupSubscriptions.Where(mgs => mgs.Id == id)
                                              .SingleOrDefault();
        }

        public void InsertOrUpdateMarketingGroupSubscription(MarketingGroupSubscription subscription) {
            bool bNew = subscription.Id == 0;
            if (bNew) MarketingGroupSubscriptions.Add(subscription);
            SaveChanges();
        }

        public void DeleteMarketingGroupSubscription(int id) {
            MarketingGroupSubscriptions.RemoveRange(MarketingGroupSubscriptions.Where(mgs => mgs.Id == id));

            deleteLock(typeof(MarketingGroupSubscription).ToString(), id);
        }

        #endregion

        #region Media table

        public IQueryable<Medium> FindMedias() {
            return Media;
        }

        public Medium FindMedia(int id) {
            return Media.Where(m => m.Id == id)
                        .SingleOrDefault();
        }

        public Medium FindMedia(string folderName, string fileName) {

            return Media.Where(m => m.FolderName == folderName &&
                                    m.FileName == fileName)
                        .FirstOrDefault();
        }

        public IQueryable<Medium> FindMediaForCustomer(int companyId, int customerId) {
            return NoteAttachments.Where(na => na.CompanyId == companyId &&
                                               na.Note.ParentId == customerId &&
                                               na.Note.NoteType == (int)NoteType.Customer)
                                  .Select(na => na.Medium)
                                  .OrderBy(na => na.Id);
        }

        public void InsertOrUpdateMedia(Medium media) {
            bool bNew = media.Id == 0;
            if (bNew) Media.Add(media);
            SaveChanges();
        }

        public void DeleteMedia(Medium media) {
            Media.Remove(media);
            SaveChanges();
        }

        public void DeleteMedia(int id) {
            Media.RemoveRange(Media.Where(m => m.Id == id));
            SaveChanges();
        }

        #endregion

        #region MediaType table

        public MediaType FindMediaType(int id) {
            return MediaTypes.Where(mt => mt.Id == id)
                             .SingleOrDefault();
        }

        public MediaType FindMediaType(string fileName) {
            MediaType mediaType = null;

            if (!string.IsNullOrEmpty(fileName)) {
                string extn = fileName.FileExtension();
                if (string.IsNullOrEmpty(extn)) extn = fileName;

                if (fileName.IsYouTubeUrl()) {
                    mediaType = MediaTypes.Where(mt => mt.Extension == "youtube")
                                          .FirstOrDefault();

                } else if (!string.IsNullOrEmpty(extn)) {
                    mediaType = MediaTypes.Where(mt => mt.Extension == extn)
                                          .FirstOrDefault();
                }

                if (mediaType == null) {
                    if (fileName.IsWebUrl()) {
                        mediaType = MediaTypes.Where(mt => mt.Extension == "ref")
                                              .FirstOrDefault();
                    }
                }
            }

            return mediaType;
        }

        public IQueryable<MediaType> FindMediaTypes() {
            return MediaTypes.Where(mt => mt.Enabled == true)
                             .OrderBy(mt => mt.Extension);
        }

        #endregion

        #region MenuOption table

        public MenuOption FindMenuOption(string screenTag, int requiredLogin, int requiredObjectFlags) {
            if (requiredObjectFlags == 0) {
                return MenuOptions.Where(mo => mo.OptionTag == screenTag &&
                                               (mo.RequiredLogin & requiredLogin) != 0)
                                  .FirstOrDefault();
            } else {
                return MenuOptions.Where(mo => mo.OptionTag == screenTag &&
                                               (mo.RequiredLogin & requiredLogin) != 0 &&
                                               (mo.RequiredObjectFlags & requiredObjectFlags) != 0)
                                  .FirstOrDefault();
            }
        }

        #endregion

        #region MessageTemplate table

        public IQueryable<MessageTemplate> FindMessageTemplates(int companyId, bool bShowHidden = false) {
            return MessageTemplates.Where(et => et.CompanyId == companyId &&
                                              (bShowHidden == true || et.Enabled == true))
                                 .OrderBy(et => et.Id);
        }

        public MessageTemplate FindMessageTemplate(int companyId, MessageTemplateType id) {
            return MessageTemplates.Where(et => et.CompanyId == companyId &&
                                              et.TemplateId == (int)id)
                                 .SingleOrDefault();
        }

        public MessageTemplate FindMessageTemplate(int id) {
            return MessageTemplates.Where(et => et.Id == id)
                                 .SingleOrDefault();
        }

        public void InsertOrUpdateMessageTemplate(MessageTemplate template) {
            bool bNew = template.Id == 0;
            if (bNew) MessageTemplates.Add(template);
            SaveChanges();
        }

        public void DeleteMessageTemplate(int id) {
            MessageTemplates.RemoveRange(MessageTemplates.Where(et => et.Id == id));

            deleteLock(typeof(MessageTemplate).ToString(), id);
        }

        #endregion

        #region MetadataTemplate table

        public MetadataTemplate FindMetadataTemplate(int id) {
            return MetadataTemplates.Where(mt => mt.Id == id)
                                    .SingleOrDefault();
        }

        public MetadataTemplate FindMetadataTemplate(int companyId, string templateName) {
            return MetadataTemplates.Where(mt => mt.CompanyId == companyId &&
                                                 mt.Name == templateName)
                                    .SingleOrDefault();
        }

        #endregion

        #region MethodSigned table

        public IQueryable<MethodSigned> FindMethodSigneds() {
            return MethodSigneds.OrderBy(ms => ms.MethodSigned1);
        }

        public MethodSigned FindMethodSigned(string methodSigned) {
            var method = MethodSigneds.Where(m => m.MethodSigned1 == methodSigned)
                                .FirstOrDefault();
            return method;
        }

        #endregion

        #region Note table

        public IQueryable<Note> FindNotes(NoteType noteType, int parentId) {
            return Notes.Where(n => n.NoteType == (int)noteType &&
                                    n.ParentId == parentId)
                        .OrderByDescending(n => n.DateCreated);
        }

        public Note FindNote(int id) {
            return Notes.Where(n => n.Id == id)
                        .SingleOrDefault();
        }

        public void InsertOrUpdateNote(Note note) {
            bool bNew = note.Id == 0;
            if (bNew) Notes.Add(note);
            SaveChanges();
        }

        public void DeleteNote(int id) {
            Notes.RemoveRange(Notes.Where(n => n.Id == id));

            deleteLock(typeof(Note).ToString(), id);
        }

        #endregion

        #region NoteAttachment table

        public void InsertOrUpdateNoteAttachment(NoteAttachment attachment) {
            bool bNew = attachment.Id == 0;
            if (bNew) NoteAttachments.Add(attachment);
            SaveChanges();
        }

        public void DeleteNoteAttachment(NoteAttachment attachment) {
            NoteAttachments.Remove(attachment);
            SaveChanges();
        }

        #endregion

        #region NuOrder table

        public IQueryable<NuOrderImportTemp> FindNuOrderImportTempRecord() {
            return NuOrderImportTemps;
        }

        public void InsertNuOrderImportOrderLine(NuOrderImportTemp orderLine) {
            if(orderLine.Id == 0) {
                NuOrderImportTemps.Add(orderLine);
                SaveChanges();
            }
        }

        public void SaveNuOrderData(List<SalesOrderHeader> soHeader) {
            foreach(var header in soHeader) {
                SalesOrderHeaders.Add(header);
                foreach(var detail in header.SalesOrderDetails) {
                    SalesOrderDetails.Add(detail);
                }
            }
            SaveChanges();
        }

        public void CleanNuOrderImportTempTable() {
            NuOrderImportTemps.RemoveRange(NuOrderImportTemps);
            SaveChanges();
        }
        #endregion

        #region OrderPayment table
        /*
        public IQueryable<OrderPayment> FindOrderPayments() {
            return null;
        }
        */
        #endregion

        #region PaymentTerm table

        public IQueryable<PaymentTerm> FindPaymentTerms(int companyId, bool bShowHidden = false) {
            return PaymentTerms.Where(pt => pt.CompanyId == companyId &&
                                            (bShowHidden == true || pt.Enabled == true))
                               .OrderBy(pt => pt.Id);
        }

        public PaymentTerm FindPaymentTerm(int id) {
            return PaymentTerms.Where(pl => pl.Id == id)
                              .SingleOrDefault();
        }

        public void InsertOrUpdatePaymentTerm(PaymentTerm paymentTerm) {
            bool bNew = paymentTerm.Id == 0;
            if (bNew) PaymentTerms.Add(paymentTerm);
            SaveChanges();
        }

        public void DeletePaymentTerm(int id) {
            PaymentTerms.RemoveRange(PaymentTerms.Where(pl => pl.Id == id));

            deleteLock(typeof(PaymentTerm).ToString(), id);
        }

        #endregion

        #region PepperiImport table

        public PepperiImportHeaderTemp FindPepperiImportHeaderTempRecord() {
            return PepperiImportHeaderTemps.FirstOrDefault();
        }

        public IQueryable<PepperiImportHeaderTemp> FindAllPepperiImportHeaderTempRecords() {
            return PepperiImportHeaderTemps;
        }

        public IQueryable<PepperiImportDetailTemp> FindAllPepperiImportDetailTempRecords() {
            return PepperiImportDetailTemps;
        }

        public PepperiImportHeaderTemp FindPepperiImportHeaderTemps(int wrntyId) {
            return PepperiImportHeaderTemps.Where(p => p.WrntyId == wrntyId)
                                            .FirstOrDefault();
        }

        public IQueryable<PepperiImportDetailTemp> FindPepperiImportDetailTemps(int wrntyId) {
            return PepperiImportDetailTemps.Where(p => p.TransactionWrntyId == wrntyId)
                                            .OrderBy(p => p.Id);
        }

        public void InsertPepperiImportFile(PepperiImportHeaderTemp pepperiImportHeaderTemp, List<PepperiImportDetailTemp> pepperiImportDetailsTemp) {
            if (pepperiImportHeaderTemp.Id == 0) {
                // Add Order Header
                PepperiImportHeaderTemps.Add(pepperiImportHeaderTemp);

                // Loop through and add each Order Line
                foreach (PepperiImportDetailTemp line in pepperiImportDetailsTemp) {
                    pepperiImportHeaderTemp.PepperiImportDetailTemps.Add(line);
                }
                SaveChanges();
            }
        }

        public void CleanPepperiImportTempTables() {
            PepperiImportDetailTemps.RemoveRange(PepperiImportDetailTemps);
            PepperiImportHeaderTemps.RemoveRange(PepperiImportHeaderTemps);
            SaveChanges();
        }

        public void SavePepperiData(SalesOrderHeader soHeader, ICollection<SalesOrderDetail> soDetails) {
            SalesOrderHeaders.Add(soHeader);
            foreach(var line in soDetails) {
                SalesOrderDetails.Add(line);
            }
            SaveChanges();
        }

        #endregion

        #region PickHeader table

        public IQueryable<PickHeader> FindPickHeaders(int companyId, string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            switch (sortColumn.ToLower()) {
                case "invoicenumber":
                    if (sortOrder == SortOrder.Desc) {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                         .OrderByDescending(ph => ph.InvoiceNumber);
                    } else {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                          .OrderBy(ph => ph.InvoiceNumber);
                    }
                case "customername":
                    if (sortOrder == SortOrder.Desc) {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                         .OrderByDescending(ph => ph.Customer.Name);
                    } else {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                          .OrderBy(ph => ph.Customer.Name);
                    }
                case "custpo":
                    if (sortOrder == SortOrder.Desc) {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                         .OrderByDescending(ph => ph.CustPO);
                    } else {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                          .OrderBy(ph => ph.CustPO);
                    }
                case "status":
                    if (sortOrder == SortOrder.Desc) {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                         .OrderByDescending(ph => ph.PickStatusId);
                    } else {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                          .OrderBy(ph => ph.PickStatusId);
                    }
                case "stwdateiso":
                    if (sortOrder == SortOrder.Desc) {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                         .OrderByDescending(ph => ph.STWDate);
                    } else {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                          .OrderBy(ph => ph.STWDate);
                    }
                case "invoicedateiso":
                    if (sortOrder == SortOrder.Desc) {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                         .OrderByDescending(ph => ph.InvoiceDate);
                    } else {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                          .OrderBy(ph => ph.InvoiceDate);
                    }
                case "locationname":
                    if (sortOrder == SortOrder.Desc) {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                         .OrderByDescending(ph => ph.LocationId);
                    } else {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                          .OrderBy(ph => ph.LocationId);
                    }
                case "id":
                default:
                    if(sortOrder == SortOrder.Desc) {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                         .OrderByDescending(ph => ph.Id);
                    } else {
                        return PickHeaders.Where(ph => ph.CompanyId == companyId)
                                                         .OrderBy(ph => ph.Id);
                    }
            }
        }

        public PickHeader FindPickHeader(int id) {
            return PickHeaders.Where(ph => ph.Id == id)
                              .SingleOrDefault();
        }

        public PickHeaderStatu FindPickStatus(int id) {
            return PickHeaderStatus.Where(phs => phs.Id == id)
                                   .SingleOrDefault();
        }

        public void InsertOrUpdatePickHeader(PickHeader ph) {
            if (ph.Id == 0) PickHeaders.Add(ph);
            SaveChanges();
        }

        public void DeletePickHeader(int id) {
            PickDetails.RemoveRange(PickDetails.Where(pd => pd.PickHeaderId == id));
            PickHeaders.RemoveRange(PickHeaders.Where(ph => ph.Id == id));
            SaveChanges();
        }

        #endregion

        #region PickDetail table

        public IQueryable<PickDetail> FindPickDetails(int companyId) {
            return PickDetails.Where(pd => pd.PickHeader.CompanyId == companyId)
                                          .OrderBy(pd => pd.Id);
        }

        public IQueryable<PickDetail> FindPickDetails(int companyId, int pickHeaderId) {
            return PickDetails.Where(ph => ph.CompanyId == companyId &&
                                           ph.PickHeaderId == pickHeaderId);
        }

        public void InsertOrUpdatePickDetail(PickDetail pd) {
            if (pd.Id == 0) PickDetails.Add(pd);
            SaveChanges();
        }

        #endregion

        #region Port table

        public IQueryable<Port> FindPorts(bool bShowHidden = false) {
            return Ports.Where(p => (bShowHidden == true || p.Enabled == true))
                        .OrderBy(p => p.PortName);
        }

        public Port FindPort(int id) {
            return Ports.Where(p => p.Id == id)
                              .SingleOrDefault();
        }

        public Port FindPort(string portName) {
            return Ports.Where(p => p.PortName == portName)
                        .FirstOrDefault();
        }

        public void InsertOrUpdatePort(Port port) {
            bool bNew = port.Id == 0;
            if (bNew) Ports.Add(port);
            SaveChanges();
        }

        public void DeletePort(int id) {
            Ports.RemoveRange(Ports.Where(p => p.Id == id));

            deleteLock(typeof(Port).ToString(), id);
        }

        #endregion

        #region PriceLevel table

        public IQueryable<PriceLevel> FindPriceLevels(int companyId, bool bShowHidden = false) {
            return PriceLevels.Where(pl => pl.CompanyId == companyId &&
                                           (bShowHidden == true || pl.Enabled == true))
                              .OrderBy(pl => pl.Mneumonic);
        }

        public PriceLevel FindPriceLevel(int id) {
            return PriceLevels.Where(pl => pl.Id == id)
                              .SingleOrDefault();
        }

        public PriceLevel FindPriceLevel(int companyId, string mneumonic) {
            return PriceLevels.Where(pl => pl.CompanyId == companyId &&
                                           pl.Mneumonic == mneumonic)
                              .FirstOrDefault();
        }

        public void InsertOrUpdatePriceLevel(PriceLevel priceLevel) {
            bool bNew = priceLevel.Id == 0;
            if (bNew) PriceLevels.Add(priceLevel);
            SaveChanges();
        }

        public void DeletePriceLevel(int id) {
            PriceLevels.RemoveRange(PriceLevels.Where(pl => pl.Id == id));

            deleteLock(typeof(PriceLevel).ToString(), id);
        }

        #endregion

        #region Product table

        public IQueryable<Product> FindProducts(bool bShowHidden = false) {
            return Products.Where(p => (bShowHidden == true || p.Enabled == true))
                          .OrderBy(p => p.ItemNumber)
                          .ThenBy(p => p.ItemName);
        }

        public IQueryable<Product> FindProductsForBrand(int brandId,
                                                        string sortColumn = "", SortOrder sortOrder = SortOrder.Asc,
                                                        bool bShowHidden = false) {
            switch (sortColumn.ToLower()) {
            case "itemname":
                if (sortOrder == SortOrder.Desc) {
                    return Products.Where(p => (p.BrandId == brandId || brandId == -1) &&
                                               (bShowHidden == true || p.Enabled == true))
                          .OrderByDescending(p => p.ItemName)
                          .ThenByDescending(p => p.ItemNumber);
                } else {
                    return Products.Where(p => (p.BrandId == brandId || brandId == -1) &&
                                               (bShowHidden == true || p.Enabled == true))
                          .OrderBy(p => p.ItemName)
                          .ThenBy(p => p.ItemNumber);
                }
            case "enabled":
                if (sortOrder == SortOrder.Desc) {
                    return Products.Where(p => (p.BrandId == brandId || brandId == -1) &&
                                              (bShowHidden == true || p.Enabled == true))
                          .OrderByDescending(p => p.Enabled);
                } else {
                    return Products.Where(p => (p.BrandId == brandId || brandId == -1) &&
                                               (bShowHidden == true || p.Enabled == true))
                          .OrderBy(p => p.Enabled);
                }
            case "itemnumber":
            default:
                if (sortOrder == SortOrder.Desc) {
                    return Products.Where(p => (p.BrandId == brandId || brandId == -1) &&
                                               (bShowHidden == true || p.Enabled == true))
                          .OrderByDescending(p => p.ItemNumber)
                          .ThenByDescending(p => p.ItemName);
                } else {
                    return Products.Where(p => (p.BrandId == brandId || brandId == -1) &&
                                               (bShowHidden == true || p.Enabled == true))
                          .OrderBy(p => p.ItemNumber)
                          .ThenBy(p => p.ItemName);
                }
            }
        }

        public IQueryable<Product> FindProductsForSupplier(int supplierId, bool bShowHidden = false) {
            return Products.Where(p => p.PrimarySupplierId == supplierId &&
                                       (bShowHidden == true || p.Enabled == true))
                           .OrderBy(p => p.ItemNumber)
                           .ThenBy(p => p.ItemName);
        }

        public Product FindProduct(int id) {
            return Products.Where(p => p.Id == id)
                           .SingleOrDefault();
        }

        public Product FindProduct(string itemNumber) {
            int pos = itemNumber.IndexOf(" ");
            string itemNo = (pos == -1 ? itemNumber : itemNumber.Substring(0, pos));

            return Products.Where(p => p.ItemNumber == itemNo)
                           .SingleOrDefault();
        }

        public BrandCategory FindProductBrandCategory(int companyId, int productId) {
            BrandCategory result = null;

            var product = Products.Where(p => p.Id == productId)
                                  .SingleOrDefault();
            if (product != null) {
                var temp = product.Brand
                                  .BrandBrandCategories
                                  .Where(bbc => bbc.CompanyId == companyId)
                                  .FirstOrDefault();
                if(temp != null) result = temp.BrandCategory;
            }
            return result;
        }


        public void InsertOrUpdateProduct(Product product) {
            bool bNew = product.Id == 0;
            if (bNew) Products.Add(product);
            SaveChanges();
        }

        public void DeleteProduct(int id) {
            ProductDatas.RemoveRange(ProductDatas.Where(pd => pd.ProductId == id));
            ProductAdditionalCategories.RemoveRange(ProductAdditionalCategories.Where(pc => pc.Id == id));

            Products.RemoveRange(Products.Where(p => p.Id == id));

            deleteLock(typeof(Product).ToString(), id);     // Does SaveChanges
        }

        #endregion

        #region ProductCompliance table

        public IQueryable<ProductCompliance> FindProductCompliances(int productId,
                                                                    string sortColumn = "", 
                                                                    SortOrder sortOrder = SortOrder.Asc) {
            switch (sortColumn.ToLower()) {
            case "MarketId":
            case "MarketNameText":
                if (sortOrder == SortOrder.Desc) {
                    return ProductCompliances.Where(pc => pc.ProductId == productId)
                                             .OrderByDescending(pc => pc.LOVItem_Market.ItemText);
                } else {
                    return ProductCompliances.Where(pc => pc.ProductId == productId)
                                             .OrderBy(pc => pc.LOVItem_Market.ItemText);
                }
            case "ComplianceCategoryId":
            case "ComplianceCategoryText":
            default:
                if (sortOrder == SortOrder.Desc) {
                    return ProductCompliances.Where(pc => pc.ProductId == productId)
                                             .OrderByDescending(pc => pc.LOVItem_ComplianceCategory.ItemText);
                } else {
                    return ProductCompliances.Where(pc => pc.ProductId == productId)
                                             .OrderBy(pc => pc.LOVItem_ComplianceCategory.ItemText);
                }
            }
        }

        public ProductCompliance FindProductCompliance(int id) {
            return ProductCompliances.Where(pc => pc.Id == id)
                                     .SingleOrDefault();
        }

        public void AddMediaToProductCompliance(int productComplianceId, int mediaId) {
            var attachment = ProductComplianceAttachments.Where(pca => pca.ProductComplianceId == productComplianceId &&
                                                                       pca.MediaId == mediaId)
                                                         .FirstOrDefault();
            if(attachment == null) {
                attachment = new ProductComplianceAttachment {
                    ProductComplianceId = productComplianceId,
                    MediaId = mediaId
                };
                ProductComplianceAttachments.Add(attachment);
                SaveChanges();
            }
        }

        public void InsertOrUpdateProductCompliance(ProductCompliance pc) {
            if (pc.Id == 0) ProductCompliances.Add(pc);
            SaveChanges();
        }

        public void DeleteProductCompliance(int id) {
            ProductCompliances.RemoveRange(ProductCompliances.Where(pc => pc.Id == id));
            SaveChanges();
        }

        #endregion

        #region ProductComplianceAttachment table

        public ProductComplianceAttachment FindProductComplianceAttachment(int id) {
            return ProductComplianceAttachments.Where(pca => pca.Id == id)
                                               .SingleOrDefault();
        }

        public IQueryable<ProductComplianceAttachment> FindProductComplianceAttachmentListModel(int productComplianceId) {
            return ProductComplianceAttachments.Where(pca => pca.ProductComplianceId == productComplianceId);
        }

        public void DeleteProductComplianceAttachment(int id) {
            ProductComplianceAttachments.RemoveRange(ProductComplianceAttachments.Where(pca => pca.Id == id));
            SaveChanges();
        }

        #endregion

        #region ProductIP table

        public IQueryable<ProductIP> FindProductIPs(int productId) {
            return ProductIPs.Where(pi => pi.ProductId == productId)
                             .OrderBy(pi => pi.LOVItem_Market.ItemText);
        }

        public ProductIP FindProductIP(int id) {
            return ProductIPs.Where(pi => pi.Id == id)
                             .SingleOrDefault();
        }

        public void InsertOrUpdateProductIP(ProductIP ip) {
            if (ip.Id == 0) ProductIPs.Add(ip);
            SaveChanges();
        }

        public void DeleteProductIPs(List<ProductIP> ips) {
            ProductIPs.RemoveRange(ips);
            SaveChanges();
        }

        public void DeleteProductIP(int id) {
            ProductIPs.RemoveRange(ProductIPs.Where(pi => pi.Id == id));
            SaveChanges();
        }

        #endregion

        #region ProductAdditionalCategory table

        public void InsertOrUpdateProductAdditionalCategory(ProductAdditionalCategory addCat) {
            var temp = ProductAdditionalCategories.Where(ac => ac.Id == addCat.Id)
                                                  .SingleOrDefault();
            if (temp == null) {
                ProductAdditionalCategories.Add(addCat);
            } else {
                temp.CategoryId = addCat.CategoryId;
                temp.FormatId = addCat.FormatId;
                temp.FormatTypeId = addCat.FormatTypeId;
                temp.SeasonId = addCat.SeasonId;
                temp.PackingTypeId = addCat.PackingTypeId;
                temp.KidsAdultsId = addCat.KidsAdultsId;
                temp.AgeGroupId = addCat.AgeGroupId;
                temp.DvlptTypeId = addCat.DvlptTypeId;
                temp.PCProductId = addCat.PCProductId;
                temp.PCDvlptId = addCat.PCDvlptId;
            }
            SaveChanges();
        }

        #endregion

        #region ProductData table

        public void InsertOrUpdateProductData(Product product, ProductData data) {
            var temp = product.ProductDatas
                              .Where(pd => pd.MetadataTemplateElementId == data.MetadataTemplateElementId)
                              .FirstOrDefault();
            if (temp == null) temp = new ProductData {
                ProductId = product.Id,
                MetadataTemplateElementId = data.MetadataTemplateElementId
            };

            temp.ValueText = data.ValueText;
            temp.ValueInt = data.ValueInt;
            temp.ValueDec = data.ValueDec;
            temp.ValueDate = data.ValueDate;

            bool bNew = temp.Id == 0;
            if (bNew) ProductDatas.Add(temp);
            SaveChanges();
        }

        #endregion

        #region ProductLocation table

        public ProductLocation FindProductLocation(int companyId, int productId, int locationId) {
            return ProductLocations.Where(pl => pl.CompanyId == companyId &&
                                                pl.ProductId == productId &&
                                                pl.LocationId == locationId)
                                   .SingleOrDefault();
        }

        public ProductLocation FindProductLocation(int id) {
            return ProductLocations.Where(pl => pl.Id == id)
                                   .SingleOrDefault();
        }

        public void InsertOrUpdateProductLocation(ProductLocation pl) {
            bool bNew = pl.Id == 0;
            if (bNew) ProductLocations.Add(pl);
            SaveChanges();
        }

        public void DeleteProductLocation(int id) {
            ProductLocations.RemoveRange(ProductLocations.Where(pl => pl.Id == id));

            deleteLock(typeof(ProductLocation).ToString(), id);
        }

        public void DeleteProductLocationsForProduct(int companyId, int productId) {
            ProductLocations.RemoveRange(ProductLocations.Where(pl => pl.CompanyId == companyId &&
                                                                      pl.ProductId == productId));
            SaveChanges();
        }

        #endregion

        #region ProductMedia table

        public IQueryable<ProductMedia> FindProductMedias(int productId) {
            return ProductMedias.Where(pm => pm.ProductId == productId);
        }

        public ProductMedia FindProductMedia(int id) {
            return ProductMedias.Where(pm => pm.Id == id)
                                .SingleOrDefault();
        }

        public void InsertOrUpdateProductMedia(ProductMedia prodMedia) {
            if(prodMedia.Id == 0) ProductMedias.Add(prodMedia);
            SaveChanges();

            if(prodMedia.Product.ProductMedia == null) {
                // Product has no primary media, so set it
                prodMedia.Product.ProductMedia = prodMedia;
                SaveChanges();
            }
        }

        public void DeleteProductMedia(int id) {
            var prodMedia = FindProductMedia(id);
            if (prodMedia != null) {
                if(prodMedia.Id == prodMedia.Product.PrimaryMediaId) {
                    // Deleting the current media
                    prodMedia.Product.ProductMedia = null;
                }

                ProductMedias.RemoveRange(ProductMedias.Where(pm => pm.Id == id));
                SaveChanges();
            }
        }

        #endregion

        #region ProductPrice table

        public IQueryable<ProductPrice> FindProductPrices(int productId) {
            return ProductPrices.Where(pp => pp.ProductId == productId)
                                .OrderBy(pp => pp.PriceLevelNameId)
                                .ThenBy(pp => pp.QuantityBreak);
        }

        #endregion

        #region PurchaseOrderHeader table

        public IQueryable<PurchaseOrderHeader> FindPurchaseOrderHeaders(int companyId, string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            switch (sortColumn.ToLower()) {
            case "orderdateiso":
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderByDescending(p => p.OrderDate);
                } else {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderBy(p => p.OrderDate);
                }
            case "salespersonname":
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderByDescending(p => p.User_SalesPerson.FirstName)
                                  .ThenByDescending(p => p.User_SalesPerson.LastName);
                } else {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderBy(p => p.User_SalesPerson.FirstName)
                                  .ThenBy(p => p.User_SalesPerson.LastName);
                }
            case "suppliername":
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderByDescending(p => p.Supplier.Name);
                } else {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderBy(p => p.Supplier.Name);
                }
            case "postatustext":
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderByDescending(p => p.PurchaseOrderHeaderStatu.StatusName);
                } else {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderBy(p => p.PurchaseOrderHeaderStatu.StatusName);
                }
            case "realisticrequireddateiso":
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderByDescending(p => p.RealisticRequiredDate);
                } else {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderBy(p => p.RealisticRequiredDate);
                }
            case "requireddateiso":
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderByDescending(p => p.RequiredDate);
                } else {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderBy(p => p.RequiredDate);
                }
            case "completeddateiso":
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderByDescending(p => p.CompletedDate);
                } else {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderBy(p => p.CompletedDate);
                }
            case "ordernumber":
            default:
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderByDescending(p => p.OrderNumber);
                } else {
                    return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId)
                                  .OrderBy(p => p.OrderNumber);
                }
            }
        }

        public IQueryable<PurchaseOrderHeader> FindUndeliveredPurchaseOrderHeaders(int companyId) {
            DateTimeOffset now = DateTimeOffset.Now;

            return PurchaseOrderHeaders.Where(p => p.CompanyId == companyId &&
                                                   p.RealisticRequiredDate < now &&
                                                   p.PurchaseOrderHeaderStatu.StatusValue != (int)PurchaseOrderStatus.Cancelled &&
                                                   p.PurchaseOrderHeaderStatu.StatusValue != (int)PurchaseOrderStatus.Closed &&
                                                   p.PurchaseOrderHeaderStatu.StatusValue != (int)PurchaseOrderStatus.Quote)
                                       .OrderByDescending(p => p.OrderNumber);
        }

        public PurchaseOrderHeader FindPurchaseOrderHeader(int id) {
            return PurchaseOrderHeaders.Where(p => p.Id == id)
                            .SingleOrDefault();
        }

        public double FindPurchaseOrderCBMs(int pohId) {
            double rc = 0;
            foreach(var item in PurchaseOrderDetails.Where(pod => pod.PurchaseOrderHeaderId == pohId)) {
                if(item.Product.UnitCBM != null) rc += (double)item.OrderQty * item.Product.UnitCBM.Value;
            }
            return rc;
        }

        public void InsertOrUpdatePurchaseOrderHeader(PurchaseOrderHeader purchase) {
            bool bNew = purchase.Id == 0;
            if (bNew) PurchaseOrderHeaders.Add(purchase);
            SaveChanges();
        }

        public void DeletePurchaseOrderHeader(int id) {
            PurchaseOrderDetails.RemoveRange(PurchaseOrderDetails.Where(pod => pod.PurchaseOrderHeaderId == id));
            SaveChanges();

            PurchaseOrderHeaders.RemoveRange(PurchaseOrderHeaders.Where(p => p.Id == id));

            deleteLock(typeof(PurchaseOrderHeader).ToString(), id);
        }

        #endregion

        #region PurchaseOrderHeaderStatus table

        public IQueryable<PurchaseOrderHeaderStatu> FindPurchaseOrderHeaderStatuses() {
            return PurchaseOrderHeaderStatus.OrderBy(pos => pos.Sequence);
        }

        public PurchaseOrderHeaderStatu FindPurchaseOrderHeaderStatus(int id) {
            return PurchaseOrderHeaderStatus.Where(pos => pos.Id == id)
                                            .SingleOrDefault();
        }

        public void InsertOrUpdatePurchaseOrderHeaderStatus(PurchaseOrderHeaderStatu pohs) {
            bool bNew = pohs.Id == 0;
            if (bNew) PurchaseOrderHeaderStatus.Add(pohs);
            SaveChanges();
        }

        public void DeletePurchaseOrderHeaderStatus(int id) {
            PurchaseOrderHeaderStatus.RemoveRange(PurchaseOrderHeaderStatus.Where(pl => pl.Id == id));

            deleteLock(typeof(PurchaseOrderHeaderStatu).ToString(), id);
        }

        #endregion

        #region PurchaseOrderDetail table

        public IQueryable<PurchaseOrderDetail> FindPurchaseOrderDetails(int companyId, int pohId) {
            return PurchaseOrderDetails.Where(pod => pod.CompanyId == companyId &&
                                                     pod.PurchaseOrderHeaderId == pohId)
                                       .OrderBy(pod => pod.Id);
        }

        public IQueryable<PurchaseOrderDetail> FindPurchaseOrderDetailsWithProduct(int productId) {
            return PurchaseOrderDetails.Where(pod => pod.ProductId == productId);
        }

        public PurchaseOrderDetail FindPurchaseOrderDetail(int id) {
            return PurchaseOrderDetails.Where(pod => pod.Id == id)
                                       .FirstOrDefault();
        }

        public void InsertOrUpdatePurchaseOrderDetail(PurchaseOrderDetail pod) {
            bool bNew = pod.Id == 0;
            if (bNew) PurchaseOrderDetails.Add(pod);
            SaveChanges();
        }

        public void DeletePurchaseOrderDetail(int id) {
            PurchaseOrderDetails.RemoveRange(PurchaseOrderDetails.Where(pod => pod.Id == id));
            SaveChanges();
        }

        #endregion

        #region PurchaseOrderDetailTemp table

        public IQueryable<PurchaseOrderDetailTemp> FindPurchaseOrderDetailTemps(int companyId, int purchaseOrderHeaderTempId, string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            switch (sortColumn.ToLower()) {
            case "productcode":
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.PurchaseOrderHeaderTempId == purchaseOrderHeaderTempId)
                                                   .OrderByDescending(podt => podt.Product.ItemNumber);
                } else {
                    return PurchaseOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.PurchaseOrderHeaderTempId == purchaseOrderHeaderTempId)
                                                   .OrderBy(podt => podt.Product.ItemNumber);
                }
            case "productdescription":
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.PurchaseOrderHeaderTempId == purchaseOrderHeaderTempId)
                                                   .OrderByDescending(podt => podt.ProductDescription);
                } else {
                    return PurchaseOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.PurchaseOrderHeaderTempId == purchaseOrderHeaderTempId)
                                                   .OrderBy(podt => podt.ProductDescription);
                }
            case "supplieritemnumber":
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.PurchaseOrderHeaderTempId == purchaseOrderHeaderTempId)
                                                   .OrderByDescending(podt => podt.Product.SupplierItemNumber);
                } else {
                    return PurchaseOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.PurchaseOrderHeaderTempId == purchaseOrderHeaderTempId)
                                                   .OrderBy(podt => podt.Product.SupplierItemNumber);
                }
            case "orderqty":
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.PurchaseOrderHeaderTempId == purchaseOrderHeaderTempId)
                                                   .OrderByDescending(podt => podt.OrderQty);
                } else {
                    return PurchaseOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.PurchaseOrderHeaderTempId == purchaseOrderHeaderTempId)
                                                   .OrderBy(podt => podt.OrderQty);
                }
            case "linenumber":
            default:
                if (sortOrder == SortOrder.Desc) {
                    return PurchaseOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.PurchaseOrderHeaderTempId == purchaseOrderHeaderTempId)
                                                   .OrderByDescending(podt => podt.LineNumber);
                } else {
                    return PurchaseOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.PurchaseOrderHeaderTempId == purchaseOrderHeaderTempId)
                                                   .OrderBy(podt => podt.LineNumber);
                }
            }
        }

        public PurchaseOrderDetailTemp FindPurchaseOrderDetailTemp(int id) {
            return PurchaseOrderDetailTemps.Where(podt => podt.Id == id)
                                           .FirstOrDefault();
        }

        public int GetNextPurchaseOrderDetailLineNumber(int headerId, bool bTempTable) {
            int? maxLineNo;
            // The next line number is the maximum currently on the order rounded up to the nearest 1000
            if (bTempTable) {
                maxLineNo = PurchaseOrderDetailTemps.Where(podt => podt.PurchaseOrderHeaderTempId == headerId)
                                                    .Select(podt => podt.LineNumber)
                                                    .DefaultIfEmpty(0)
                                                    .Max();
            } else {
                maxLineNo = PurchaseOrderDetails.Where(pod => pod.PurchaseOrderHeaderId == headerId)
                                                .Select(pod => pod.LineNumber)
                                                .DefaultIfEmpty(0)
                                                .Max();
            }
            if (maxLineNo == null) maxLineNo = 0;
            return (maxLineNo.Value + 1000) / 1000 * 1000;
        }

        public void InsertOrUpdatePurchaseOrderDetailTemp(PurchaseOrderDetailTemp podt) {
            bool bNew = podt.Id == 0;
            if (bNew) PurchaseOrderDetailTemps.Add(podt);
            SaveChanges();
        }

        public void DeletePurchaseOrderDetailTemp(int id) {
            PurchaseOrderDetailTemps.RemoveRange(PurchaseOrderDetailTemps.Where(podt => podt.Id == id));
            SaveChanges();
        }

        #endregion

        #region PurchaseOrderHeaderTemp table

        public IQueryable<PurchaseOrderHeaderTemp> FindPurchaseOrderHeaderTemps(int companyId) {
            return PurchaseOrderHeaderTemps.Where(poht => poht.CompanyId == companyId);
        }

        public PurchaseOrderHeaderTemp FindPurchaseOrderHeaderTemp(int id) {
            return PurchaseOrderHeaderTemps.Where(poht => poht.Id == id)
                                           .FirstOrDefault();
        }

        public void InsertOrUpdatePurchaseOrderHeaderTemp(PurchaseOrderHeaderTemp poht) {
            bool bNew = poht.Id == 0;
            if (bNew) PurchaseOrderHeaderTemps.Add(poht);
            SaveChanges();
        }

        public void DeletePurchaseOrderHeaderTemp(int companyId, int userId, int purchaseOrderHeaderId) {
            foreach(var temp in PurchaseOrderHeaderTemps.Where(poht => poht.CompanyId == companyId &&
                                                               poht.UserId == userId &&
                                                               poht.OriginalRowId == purchaseOrderHeaderId)
                                                        .ToList()) {

                PurchaseOrderDetailTemps.RemoveRange(PurchaseOrderDetailTemps.Where(podt => podt.PurchaseOrderHeaderTempId == temp.Id));
                SaveChanges();

                PurchaseOrderHeaderTemps.RemoveRange(PurchaseOrderHeaderTemps.Where(poht => poht.Id == temp.Id));
                SaveChanges();
            }
        }

        #endregion

        #region Region table

        public IQueryable<Region> FindRegions(int companyId, bool bShowHidden = false) {
            return Regions.Where(r => r.CompanyId == companyId &&
                                     (bShowHidden == true || r.Enabled == true))
                          .OrderBy(r => r.RegionName);
        }

        public Region FindRegion(int id) {
            return Regions.Where(r => r.Id == id)
                          .SingleOrDefault();
        }

        public Region FindRegion(int companyId, string regionName) {
            return Regions.Where(r => r.CompanyId == companyId &&
                                      r.RegionName == regionName)
                          .FirstOrDefault();
        }

        public void InsertOrUpdateRegion(Region region) {
            bool bNew = region.Id == 0;
            if (bNew) Regions.Add(region);
            SaveChanges();
        }

        public void DeleteRegion(int id) {
            Regions.RemoveRange(Regions.Where(r => r.Id == id));

            deleteLock(typeof(Region).ToString(), id);
        }

        #endregion

        #region SaleNextAction table

        public IQueryable<SaleNextAction> FindSaleNextActions() {
            return SaleNextActions.OrderBy(sna => sna.Id);
        }

        #endregion

        #region SalesOrderDetail table

        public SalesOrderDetail FindSalesOrderDetail(int id) {
            return SalesOrderDetails.Where(sod => sod.Id == id)
                                    .SingleOrDefault();
        }

        public IQueryable<SalesOrderDetail> FindSalesOrderDetails(int companyId) {
            return SalesOrderDetails.Where(sod => sod.CompanyId == companyId)
                                    .OrderBy(sod => sod.SalesOrderHeader.Customer.Name);
        }

        public IQueryable<SalesOrderDetail> FindSalesOrderDetails(int companyId, int salesOrderHeaderId) {
            return SalesOrderDetails.Where(sod => sod.CompanyId == companyId &&
                                                  sod.SalesOrderHeaderId == salesOrderHeaderId)
                                    .OrderBy(sod => sod.LineNumber);
        }

        public IQueryable<SalesOrderDetail> FindSalesOrderDetailsWithProduct(int productId) {
            return SalesOrderDetails.Where(sod => sod.ProductId == productId);
        }

        public void InsertOrUpdateSalesOrderDetail(SalesOrderDetail sod) {
            bool bNew = sod.Id == 0;
            if (bNew) SalesOrderDetails.Add(sod);
            SaveChanges();
        }

        public void DeleteSalesOrderDetail(int id) {
            SalesOrderDetails.RemoveRange(SalesOrderDetails.Where(sodt => sodt.Id == id));
            SaveChanges();
        }

        #endregion

        #region SalesOrderDetailTemp table

        public IQueryable<SalesOrderDetailTemp> FindSalesOrderDetailTemps(int companyId, int salesOrderHeaderTempId, string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            switch (sortColumn.ToLower()) {
            case "productcode":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.SalesOrderHeaderTempId == salesOrderHeaderTempId)
                                                   .OrderByDescending(podt => podt.Product.ItemNumber);
                } else {
                    return SalesOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.SalesOrderHeaderTempId == salesOrderHeaderTempId)
                                                   .OrderBy(podt => podt.Product.ItemNumber);
                }
            case "productdescription":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.SalesOrderHeaderTempId == salesOrderHeaderTempId)
                                                   .OrderByDescending(podt => podt.ProductDescription);
                } else {
                    return SalesOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.SalesOrderHeaderTempId == salesOrderHeaderTempId)
                                                   .OrderBy(podt => podt.ProductDescription);
                }
            case "supplieritemnumber":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.SalesOrderHeaderTempId == salesOrderHeaderTempId)
                                                   .OrderByDescending(podt => podt.Product.SupplierItemNumber);
                } else {
                    return SalesOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.SalesOrderHeaderTempId == salesOrderHeaderTempId)
                                                   .OrderBy(podt => podt.Product.SupplierItemNumber);
                }
            case "orderqty":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.SalesOrderHeaderTempId == salesOrderHeaderTempId)
                                                   .OrderByDescending(podt => podt.OrderQty);
                } else {
                    return SalesOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.SalesOrderHeaderTempId == salesOrderHeaderTempId)
                                                   .OrderBy(podt => podt.OrderQty);
                }
            case "linenumber":
            default:
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.SalesOrderHeaderTempId == salesOrderHeaderTempId)
                                                   .OrderByDescending(podt => podt.LineNumber);
                } else {
                    return SalesOrderDetailTemps.Where(podt => podt.CompanyId == companyId &&
                                                                  podt.SalesOrderHeaderTempId == salesOrderHeaderTempId)
                                                   .OrderBy(podt => podt.LineNumber);
                }
            }
        }

        public SalesOrderDetailTemp FindSalesOrderDetailTemp(int id) {
            return SalesOrderDetailTemps.Where(sodt => sodt.Id == id)
                                        .FirstOrDefault();
        }

        public int GetNextSalesOrderDetailLineNumber(int headerId, bool bTempTable) {
            int? maxLineNo;
            // The next line number is the maximum currently on the order rounded up to the nearest 1000
            if (bTempTable) {
                maxLineNo = SalesOrderDetailTemps.Where(sodt => sodt.SalesOrderHeaderTempId == headerId)
                                                 .Select(sodt => sodt.LineNumber)
                                                 .DefaultIfEmpty(0)
                                                 .Max();
            } else {
                maxLineNo = SalesOrderDetails.Where(sod => sod.SalesOrderHeaderId == headerId)
                                             .Select(sod => sod.LineNumber)
                                             .DefaultIfEmpty(0)
                                             .Max();
            }
            if (maxLineNo == null) maxLineNo = 0;
            return (maxLineNo.Value + 1000) / 1000 * 1000;
        }

        public void InsertOrUpdateSalesOrderDetailTemp(SalesOrderDetailTemp sodt) {
            bool bNew = sodt.Id == 0;
            if (bNew) SalesOrderDetailTemps.Add(sodt);
            SaveChanges();
        }

        public void DeleteSalesOrderDetailTemp(int id) {
            SalesOrderDetailTemps.RemoveRange(SalesOrderDetailTemps.Where(podt => podt.Id == id));
            SaveChanges();
        }

        #endregion

        #region SalesOrderHeader table

        public IQueryable<SalesOrderHeader> FindSalesOrderHeaders(int companyId, string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            switch (sortColumn.ToLower()) {
            case "custpo":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderByDescending(s => s.CustPO);
                } else {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderBy(s => s.CustPO);
                }
            case "orderdate":
            case "orderdateiso":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderByDescending(s => s.OrderDate);
                } else {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderBy(s => s.OrderDate);
                }
            case "customername":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderByDescending(s => s.Customer.Name);
                } else {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderBy(s => s.Customer.Name);
                }
            case "regionname":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderByDescending(s => s.Customer.Region.RegionName);
                } else {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderBy(s => s.Customer.Region.RegionName);
                }
            case "salespersonname":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                  .OrderByDescending(s => s.User_SalesPerson.FirstName)
                                  .ThenByDescending(s => s.User_SalesPerson.LastName);
                } else {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                  .OrderBy(s => s.User_SalesPerson.FirstName)
                                  .ThenBy(s => s.User_SalesPerson.LastName);
                }
            case "ordertypetext":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderByDescending(s => s.LOVItem_OrderType.ItemText);
                } else {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderBy(s => s.LOVItem_OrderType.ItemText);
                }
            case "deliverywindowopen":
            case "deliverywindowopeniso":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderByDescending(s => s.DeliveryWindowOpen);
                } else {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderBy(s => s.DeliveryWindowOpen);
                }
            case "deliverywindowclose":
            case "deliverywindowcloseiso":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderByDescending(s => s.DeliveryWindowClose);
                } else {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderBy(s => s.DeliveryWindowClose);
                }
            case "nextactiontext":
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderByDescending(s => s.SaleNextAction.NextActionDescription);
                } else {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderBy(s => s.SaleNextAction.NextActionDescription);
                }
            case "ordernumber":
            case "ordernumberurl":
            default:
                if (sortOrder == SortOrder.Desc) {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderByDescending(s => s.OrderNumber);
                } else {
                    return SalesOrderHeaders.Where(s => s.CompanyId == companyId)
                                            .OrderBy(s => s.OrderNumber);
                }
            }
        }

        public SalesOrderHeader FindSalesOrderHeader(int id) {
            return SalesOrderHeaders.Where(p => p.Id == id)
                                    .SingleOrDefault();
        }

        public void InsertOrUpdateSalesOrderHeader(SalesOrderHeader purchase) {
            bool bNew = purchase.Id == 0;
            if (bNew) SalesOrderHeaders.Add(purchase);
            SaveChanges();
        }

        public void DeleteSalesOrderHeader(int id) {
            SalesOrderDetails.RemoveRange(SalesOrderDetails.Where(sod => sod.SalesOrderHeaderId == id));
            SaveChanges();

            SalesOrderHeaders.RemoveRange(SalesOrderHeaders.Where(s => s.Id == id));

            deleteLock(typeof(SalesOrderHeader).ToString(), id);
        }

        #endregion

        #region SalesOrderHeaderStatus table

        public IQueryable<SalesOrderHeaderStatu> FindSalesOrderHeaderStatuses() {
            return SalesOrderHeaderStatus.OrderBy(sos => sos.Sequence);
        }

        public SalesOrderHeaderStatu FindSalesOrderHeaderStatus(int id) {
            return SalesOrderHeaderStatus.Where(sos => sos.Id == id)
                                            .SingleOrDefault();
        }

        public void InsertOrUpdateSalesOrderHeaderStatus(SalesOrderHeaderStatu pohs) {
            bool bNew = pohs.Id == 0;
            if (bNew) SalesOrderHeaderStatus.Add(pohs);
            SaveChanges();
        }

        public void DeleteSalesOrderHeaderStatus(int id) {
            SalesOrderHeaderStatus.RemoveRange(SalesOrderHeaderStatus.Where(pl => pl.Id == id));

            deleteLock(typeof(SalesOrderHeaderStatu).ToString(), id);
        }

        #endregion

        #region SalesOrderHeaderSubStatus table

        public IQueryable<SalesOrderHeaderSubStatu> FindSalesOrderHeaderSubStatuses() {
            return SalesOrderHeaderSubStatus.OrderBy(soss => soss.Sequence);
        }

        public SalesOrderHeaderSubStatu FindSalesOrderHeaderSubStatus(int id) {
            return SalesOrderHeaderSubStatus.Where(soss => soss.Id == id)
                                            .SingleOrDefault();
        }

        public SalesOrderHeaderSubStatu FindSalesOrderHeaderSubStatus(SalesOrderHeaderSubStatus status) {
            return SalesOrderHeaderSubStatus.Where(sohss => sohss.Id == (int)status)
                                            .FirstOrDefault();
        }

        #endregion

        #region SalesOrderHeaderTemp table

        public IQueryable<SalesOrderHeaderTemp> FindSalesOrderHeaderTemps(int companyId) {
            return SalesOrderHeaderTemps.Where(soht => soht.CompanyId == companyId);
        }

        public SalesOrderHeaderTemp FindSalesOrderHeaderTemp(int id) {
            return SalesOrderHeaderTemps.Where(soht => soht.Id == id)
                                        .FirstOrDefault();
        }

        public void InsertOrUpdateSalesOrderHeaderTemp(SalesOrderHeaderTemp soht) {
            bool bNew = soht.Id == 0;
            if (bNew) SalesOrderHeaderTemps.Add(soht);
            SaveChanges();
        }

        public void DeleteSalesOrderHeaderTemp(int companyId, int userId, int salesOrderHeaderId) {
            foreach (var temp in SalesOrderHeaderTemps.Where(soht => soht.CompanyId == companyId &&
                                                             soht.UserId == userId &&
                                                             soht.OriginalRowId == salesOrderHeaderId)
                                                      .ToList()) {

                SalesOrderDetailTemps.RemoveRange(SalesOrderDetailTemps.Where(sodt => sodt.SalesOrderHeaderTempId == temp.Id));
                SaveChanges();

                SalesOrderHeaderTemps.RemoveRange(SalesOrderHeaderTemps.Where(soht => soht.Id == temp.Id));
                SaveChanges();
            }
        }

        #endregion

        #region SalesOrderLineStatus table

        public IQueryable<SalesOrderLineStatu> FindSalesOrderLineStatuses() {
            return SalesOrderLineStatus.OrderBy(s => s.Sequence);
        }

        public SalesOrderLineStatu FindSalesOrderLineStatus(int id) {
            return SalesOrderLineStatus.Where(sols => sols.Id == id)
                                       .SingleOrDefault();
        }

        #endregion

        #region ScheduledTask table

        public IQueryable<ScheduledTask> FindScheduledTasks() {
            return ScheduledTasks.OrderByDescending(st => st.Enabled)
                                 .ThenBy(st => st.TaskName);
        }

        public ScheduledTask FindScheduledTask(int id) {
            return ScheduledTasks.Where(st => st.Id == id)
                                 .SingleOrDefault();
        }

        public ScheduledTask FindScheduledTask(string taskName, string cmdParameter = "") {
            return ScheduledTasks.Where(st => st.TaskName == taskName &&
                                              st.CmdParameter == cmdParameter)
                                 .FirstOrDefault();
        }

        public void InsertOrUpdateScheduledTask(ScheduledTask task) {
            bool bNew = task.Id == 0;
            if (bNew) ScheduledTasks.Add(task);
            SaveChanges();
        }

        public void DeleteScheduledTask(int id) {
            ScheduledTaskLogs.RemoveRange(ScheduledTaskLogs.Where(st => st.ScheduledTaskId == id));
            SaveChanges();

            ScheduledTasks.RemoveRange(ScheduledTasks.Where(st => st.Id == id));

            deleteLock(typeof(ScheduledTask).ToString(), id);
        }

        #endregion

        #region ScheduledTaskLog table

        public IQueryable<ScheduledTaskLog> FindScheduledTaskLogs(int taskId,
                                                                  string search,
                                                                  int severity,
                                                                  DateTimeOffset? dateFrom, DateTimeOffset? dateTo) {
            // If the dateFrom has milliseconds, truncate them so that we round down 
            // to the nearest second asthis is the grandularity the user can see
            DateTimeOffset? dtFrom = dateFrom;
            if (dtFrom != null && dtFrom.Value.Millisecond > 0) dtFrom = dtFrom.Value.AddMilliseconds(dtFrom.Value.Millisecond * -1);       // Truncate on whole seconds

            // Move the dateTo to the next second and truncate the milliseconds so the we can do a < match and capture all
            // records with the same second even with varying miliseconds
            DateTimeOffset? dtTo = dateTo;
            if (dtTo != null) dtTo = dtTo.Value.AddMilliseconds(dtTo.Value.Millisecond * -1).AddSeconds(1);   // Round up to the next whole second

            return ScheduledTaskLogs.Where(l => l.ScheduledTaskId == taskId &&
                                                (severity == (int)LogSeverity.All || l.Severity == severity) &&
                                                (dtFrom == null || l.LogDate >= dateFrom) &&
                                                (dtTo == null || l.LogDate < dtTo) &&
                                                (string.IsNullOrEmpty(search) ||
                                                 (l.Message != null && l.Message.ToLower().Contains(search.ToLower())) ||
                                                 (l.StackTrace != null && l.StackTrace.ToLower().Contains(search.ToLower())))
                                          )
                                    .OrderBy(l => l.LogDate);
        }

        public IQueryable<ScheduledTaskLog> FindScheduledTaskLogsAfter(DateTime dt, LogSeverity severity) {
            return ScheduledTaskLogs.Where(l => l.LogDate >= dt &&
                                                l.Severity >= (int)severity)
                                    .OrderBy(l => l.LogDate);
        }

        public ScheduledTaskLog FindScheduledTaskLog(int id) {
            return ScheduledTaskLogs.Where(stl => stl.Id == id)
                                    .SingleOrDefault();
        }

        public void InsertOrUpdateScheduledTaskLog(ScheduledTaskLog log) {
            // Logging isn't subjected to the test cleanup system
            if(log.Id == 0) ScheduledTaskLogs.Add(log);
            SaveChanges();
        }

        public int DeleteScheduledTaskLogsOlderThan(int keepDays) {
            DateTimeOffset dt = DateTimeOffset.Now.AddDays(keepDays * -1);

            int rc = ScheduledTaskLogs.Where(l => l.LogDate < dt).Count();

            ScheduledTaskLogs.RemoveRange(ScheduledTaskLogs.Where(l => l.LogDate < dt));
            SaveChanges();

            return rc;
        }

        #endregion

        #region Shipment table

        public IQueryable<Shipment> FindShipments(int companyId, bool bShowHidden = false) {
            return Shipments.Where(s => s.CompanyId == companyId)
                            .OrderBy(s => s.Id);
        }

        public Shipment FindShipment(int id) {
            return Shipments.Where(s => s.Id == id)
                            .SingleOrDefault();
        }

        public void InsertOrUpdateShipment(Shipment supplier) {
            bool bNew = supplier.Id == 0;
            if (bNew) Shipments.Add(supplier);
            SaveChanges();
        }

        public void DeleteShipment(int id) {
            ShipmentContents.RemoveRange(ShipmentContents.Where(sc => sc.ShipmentId == id));
            SaveChanges();

            Shipments.RemoveRange(Shipments.Where(s => s.Id == id));

            deleteLock(typeof(Shipment).ToString(), id);
        }

        #endregion

        #region ShipmentContent table

        public IQueryable<ShipmentContent> FindShipmentContents(int companyId) {
            return ShipmentContents.Where(sc => sc.CompanyId == companyId)
                                   .OrderBy(sc => sc.Id);
        }

        public IQueryable<ShipmentContent> FindShipmentContents(int companyId, int shipmentId) {
            return ShipmentContents.Where(sc => sc.CompanyId == companyId &&
                                                sc.ShipmentId == shipmentId)
                                   .OrderBy(sc => sc.Id);
        }

        public ShipmentContent FindShipmentContent(int id) {
            return ShipmentContents.Where(sc => sc.Id == id)
                            .SingleOrDefault();
        }

        public void InsertOrUpdateShipmentContent(ShipmentContent supplier) {
            bool bNew = supplier.Id == 0;
            if (bNew) ShipmentContents.Add(supplier);
            SaveChanges();
        }

        public void DeleteShipmentContent(int id, bool bDeleteShipmentIfNoContent) {
            var content = FindShipmentContent(id);
            if (content != null) {
                int companyId = content.CompanyId,
                    shipmentId = content.ShipmentId.Value;

                ShipmentContents.RemoveRange(ShipmentContents.Where(sc => sc.Id == id));
                deleteLock(typeof(ShipmentContent).ToString(), id);

                if (bDeleteShipmentIfNoContent &&
                    FindShipmentContents(companyId, shipmentId).Count() == 0) {
                    // Shipment has no content so delete it
                    DeleteShipment(shipmentId);
                }
            }
        }

        #endregion

        #region ShopifyImport table

        public IQueryable<ShopifyImportHeaderTemp> FindShopifyImportHeaderTempRecords() {
            return ShopifyImportHeaderTemps;
        }

        public IQueryable<ShopifyImportDetailTemp> FindShopifyImportDetailTempRecords() {
            return ShopifyImportDetailTemps;
        }

        public void InsertShopifyImportFile(ShopifyImportHeaderTemp order) {
            if (order.Id == 0) {
                // Add Order Header
                ShopifyImportHeaderTemps.Add(order);

                // Loop through and add each Order Line
                foreach (ShopifyImportDetailTemp line in order.ShopifyImportDetailTemps) {
                    ShopifyImportDetailTemps.Add(line);
                }
                SaveChanges();
            }
        }

        public void CleanShopifyImportTempTables() {
            ShopifyImportDetailTemps.RemoveRange(ShopifyImportDetailTemps);
            ShopifyImportHeaderTemps.RemoveRange(ShopifyImportHeaderTemps);
            SaveChanges();
        }

        public void SaveShopifyData(SalesOrderHeader soHeader) {
            SalesOrderHeaders.Add(soHeader);
            foreach (var line in soHeader.SalesOrderDetails) {
                SalesOrderDetails.Add(line);
            }
        }

        #endregion

        #region Supplier table

        public IQueryable<Supplier> FindSuppliers(int companyId, bool bShowHidden = false) {
            return Suppliers.Where(s => s.CompanyId == companyId &&
                                        (bShowHidden == true || s.Enabled == true))
                            .OrderBy(s => s.Name);
        }

        public Supplier FindSupplier(int id) {
            return Suppliers.Where(s => s.Id == id)
                            .SingleOrDefault();
        }

        public Supplier FindSupplier(string supplierName) {
            return Suppliers.Where(s => s.Name == supplierName)
                            .SingleOrDefault();
        }

        public void InsertOrUpdateSupplier(Supplier supplier) {
            bool bNew = supplier.Id == 0;
            if (bNew) Suppliers.Add(supplier);
            SaveChanges();
        }

        public void DeleteSupplier(int id) {
            SupplierAddresses.RemoveRange(SupplierAddresses.Where(sa => sa.SupplierId == id));
            SaveChanges();

            Suppliers.RemoveRange(Suppliers.Where(s => s.Id == id));

            deleteLock(typeof(Supplier).ToString(), id);
        }

        #endregion

        #region SupplierAddress table

        public IQueryable<SupplierAddress> FindSupplierAddresses(int supplierId) {
            return SupplierAddresses.Where(sa => sa.SupplierId == supplierId)
                                    .OrderBy(sa => sa.Street);
        }

        public SupplierAddress FindSupplierAddress(int supplierId) {
            return SupplierAddresses.Where(sa => sa.SupplierId == supplierId)
                                    .FirstOrDefault();
        }

        public SupplierAddress FindSupplierAddressById(int id) {
            return SupplierAddresses.Where(sa => sa.Id == id)
                                    .SingleOrDefault();
        }

        public void InsertOrUpdateSupplierAddress(SupplierAddress addrs) {
            bool bNew = addrs.Id == 0;
            if (bNew) SupplierAddresses.Add(addrs);
            SaveChanges();
        }

        public void DeleteSupplierAddress(int id) {
            SupplierAddresses.RemoveRange(SupplierAddresses.Where(sa => sa.Id == id));

            deleteLock(typeof(SupplierAddress).ToString(), id);
        }

        #endregion

        #region SupplierTerm table

        public IQueryable<SupplierTerm> FindSupplierTerms(int companyId, bool bShowHidden = false) {
            return SupplierTerms.Where(st => st.CompanyId == companyId &&
                                             (bShowHidden == true || st.Enabled == true))
                                .OrderBy(st => st.SupplierTermName);
        }

        public SupplierTerm FindSupplierTerm(int id) {
            return SupplierTerms.Where(st => st.Id == id)
                                    .SingleOrDefault();
        }

        public SupplierTerm FindSupplierTerm(int companyId, string supplierTermName) {
            return SupplierTerms.Where(st => st.CompanyId == companyId &&
                                             st.SupplierTermName == supplierTermName)
                                .FirstOrDefault();
        }

        public void InsertOrUpdateSupplierTerm(SupplierTerm term) {
            bool bNew = term.Id == 0;
            if (bNew) SupplierTerms.Add(term);
            SaveChanges();
        }

        public void DeleteSupplierTerm(int id) {
            SupplierTerms.RemoveRange(SupplierTerms.Where(st => st.Id == id));

            deleteLock(typeof(SupplierTerm).ToString(), id);
        }

        #endregion

        #region Task table

        public IQueryable<Task> FindTasks() {
            return Tasks.OrderBy(n => n.DueDate);
        }


        public IQueryable<Task> FindTasks(int companyId, int userId, int statusId = -1) {
            return Tasks.Where(t => t.CompanyId == companyId &&
                                    t.UserId == userId &&
                                    (statusId == -1 || t.StatusId == statusId))
                        .OrderBy(n => n.DueDate); ;
        }

        public Task FindTask(int id) {
            return Tasks.Where(n => n.Id == id)
                        .SingleOrDefault();
        }

        public void InsertOrUpdateTask(Task task) {
            bool bNew = task.Id == 0;
            if (bNew) Tasks.Add(task);
            SaveChanges();
        }

        #endregion

        #region TaxCode table

        public IQueryable<TaxCode> FindTaxCodes(int companyId, bool bShowHidden = false) {
            return TaxCodes.Where(tc => tc.CompanyId == companyId &&
                                        (bShowHidden == true || tc.Enabled == true))
                           .OrderBy(tc => tc.TaxCode1);
        }

        public TaxCode FindTaxCode(int id) {
            return TaxCodes.Where(tc => tc.Id == id)
                           .SingleOrDefault();
        }

        public TaxCode FindTaxCode(int companyId, string taxCode) {
            return TaxCodes.Where(tc => tc.CompanyId == companyId &&
                                        tc.TaxCode1 == taxCode)
                           .FirstOrDefault();
        }

        public void InsertOrUpdateTaxCode(TaxCode taxCode) {
            bool bNew = taxCode.Id == 0;
            if (bNew) TaxCodes.Add(taxCode);
            SaveChanges();
        }

        public void DeleteTaxCode(int id) {
            TaxCodes.RemoveRange(TaxCodes.Where(c => c.Id == id));

            deleteLock(typeof(TaxCode).ToString(), id);
        }

        #endregion

        #region TestFileLog table

        public IQueryable<TestFileLog> FindTestFileLogs() {
            return TestFileLogs.OrderByDescending(tfl => tfl.Id);
        }

        #endregion

        #region User table

        public User FindUser(string loginId) {
            if (!string.IsNullOrEmpty(loginId)) {
                //string[] user = loginId.Split('.');

                //string firstName = (user.Length > 0 ? user[0] : ""),
                //       lastName = (user.Length > 1 ? user[1] : "");
                return Users.Where(u => u.Name == loginId)
                            .FirstOrDefault();
            } else {
                return null;
            }
        }

        public User FindUser(int id) {
            return Users.Where(u => u.Id == id)
                        .FirstOrDefault();
        }

        public IQueryable<User> FindUsers() {
            return Users.OrderBy(u => u.LastName)
                        .ThenBy(u => u.FirstName);
        }

        public IQueryable<UserAlias> FindUserAliases() {
            return UserAlias.OrderBy(a => a.Name);
        }

        public IQueryable<UserAlias> FindUserAliases(string userName) {
            return UserAlias.Where(a => a.Name == userName);
        }

        public string MakeName(User user) {
            string rc = "";
            if (user != null) {
                rc = (string.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName);
                if (rc.IndexOf(user.LastName, StringComparison.InvariantCultureIgnoreCase) == -1) rc += " " + user.LastName;
                rc = rc.Replace(".", " ").Trim().WordCapitalise();
            }
            return rc;
        }

        public void InsertOrUpdateUser(User user) {
            bool bNew = user.Id == 0;
            if (bNew) Users.Add(user);
            SaveChanges();
        }

        public void DeleteUser(int id) {
            UserGroupUsers.RemoveRange(UserGroupUsers.Where(ugu => ugu.UserId == id));
            SaveChanges();

            Tasks.RemoveRange(Tasks.Where(n => n.UserId == id));
            SaveChanges();

            deleteLock(typeof(User).ToString(), id);

            Users.RemoveRange(Users.Where(u => u.Id == id));
            SaveChanges();
        }

        #endregion

        #region UserAlias table

        public UserAlias FindUserAlias(int id) {
            return UserAlias.Where(ua => ua.Id == id)
                            .FirstOrDefault();
        }

        public void InsertOrUpdateUserAlias(UserAlias userAlias) {
            if (userAlias.Id == 0) UserAlias.Add(userAlias);
            SaveChanges();
        }

        #endregion

        #region UserGroup table

        public UserGroup FindUserGroup(string groupName) {
            return UserGroups.Where(ug => ug.GroupName == groupName)
                             .FirstOrDefault();
        }

        public IQueryable<UserGroup> FindUserGroups() {
            return UserGroups.OrderBy(ug => ug.GroupName);
        }

        public void InsertOrUpdateUserGroup(UserGroup userGroup) {
            bool bNew = userGroup.Id == 0;
            if (bNew) UserGroups.Add(userGroup);
            SaveChanges();
        }

        #endregion

        #region UserGroupUsers table

        public UserGroupUser FindUserGroupUser(int userGroupId, int userId) {
            return UserGroupUsers.Where(ugu => ugu.UserGroupId == userGroupId &&
                                               ugu.UserId == userId)
                                 .FirstOrDefault();
        }

        public List<User> FindUserGroupUsers(UserGroup userGroup) {
            return userGroup.UserGroupUsers
                            .Select(ugu => ugu.User)
                            .OrderBy(u => u.FirstName)
                            .ThenBy(u => u.LastName)
                            .ToList();
        }

        public List<UserGroup> FindUserGroupsForUser(int userId) {
            return UserGroupUsers.Where(ugu => ugu.UserId == userId)
                                 .Select(ugu => ugu.UserGroup)
                                 .OrderBy(ug => ug.GroupName)
                                 .ToList();
        }

        public void InsertOrUpdateUserGroupUser(UserGroupUser userGroupUser) {
            bool bNew = userGroupUser.Id == 0;
            if (bNew) UserGroupUsers.Add(userGroupUser);
            SaveChanges();
        }

        #endregion

        #region UserSessionProperty table

        public UserSessionProperty FindUserSessionProperty(int userId, string propertyName) {
		    return UserSessionProperties.Where(usp => usp.UserId == userId &&
                                                      usp.PropertyName == propertyName)
			       .FirstOrDefault();
        }

        public UserSessionProperty InsertOrUpdateUserSessionProperty(int userId, string propertyName, string propertyValue) {
		    var prop = FindUserSessionProperty(userId, propertyName);
            if (prop == null) {
                prop = new UserSessionProperty {
                    UserId = userId,
                    PropertyName = propertyName,
                    PropertyValue = propertyValue
                };
                UserSessionProperties.Add(prop);
                SaveChanges();

            } else {
                prop.PropertyValue = propertyValue;
                SaveChanges();
            }

            return prop;
	    }

        #endregion
    }
}
