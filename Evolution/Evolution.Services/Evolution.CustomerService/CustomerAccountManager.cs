using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;
using Evolution.Extensions;
using Evolution.Enumerations;
using AutoMapper;

namespace Evolution.CustomerService {

    #region Public members

    public partial class CustomerService : CommonService.CommonService {
        public BrandCategorySalesPersonListModel FindBrandCategorySalesPersonsListModel(int customerId, int index, int pageNo, int pageSize, string search) {
            var model = new BrandCategorySalesPersonListModel();

            model.GridIndex = index;
            var allItems = db.FindBrandCategorySalesPersons(customerId);

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = MapToModel(item);
                model.Items.Add(newItem);
            }
            return model;
        }

        public List<BrandCategorySalesPersonModel> FindBrandCategorySalesPersonsModel(CompanyModel company,
                                                                                      CustomerModel customer,
                                                                                      int brandCategoryId,
                                                                                      SalesPersonType salesPersonType) {

            List<BrandCategorySalesPersonModel> model = new List<BrandCategorySalesPersonModel>();

            foreach (var item in db.FindBrandCategorySalesPersons(customer.Id)
                                   .Where(bcsp => bcsp.BrandCategoryId == brandCategoryId &&
                                                  bcsp.LOVItem.ItemValue1 == ((int)salesPersonType).ToString())) {

                model.Add(MapToModel(item));
            }
            return model;
        }

        public BrandCategorySalesPersonModel MapToModel(BrandCategorySalesPerson item) {
            var newItem = Mapper.Map<BrandCategorySalesPerson, BrandCategorySalesPersonModel>(item);

            newItem.BrandCategoryName = item.BrandCategory.CategoryName;
            newItem.UserName = (item.User.FirstName + " " + item.User.LastName).Trim().WordCapitalise();
            newItem.SalesPersonTypeName = item.LOVItem.ItemText;

            return newItem;
        }

        public BrandCategorySalesPerson MapToEntity(BrandCategorySalesPerson entity) {
            var newItem = Mapper.Map<BrandCategorySalesPerson, BrandCategorySalesPerson>(entity);
            return newItem;
        }

        public void MapToEntity(BrandCategorySalesPersonModel model, BrandCategorySalesPerson entity) {
            Mapper.Map<BrandCategorySalesPersonModel, BrandCategorySalesPerson>(model, entity);
        }

        public BrandCategorySalesPersonModel FindBrandCategorySalesPersonModel(int id, CompanyModel company, CustomerModel customer, bool bCreateEmptyIfNotfound = true) {
            return FindBrandCategorySalesPersonModel(id, company, customer.Id, bCreateEmptyIfNotfound);
        }

        public BrandCategorySalesPersonModel FindBrandCategorySalesPersonModel(int id, CompanyModel company, int customerId, bool bCreateEmptyIfNotfound = true) {
            BrandCategorySalesPersonModel model = null;

            var sp = db.FindBrandCategorySalesPerson(id);
            if (sp == null) {
                if (bCreateEmptyIfNotfound) model = new BrandCategorySalesPersonModel {
                    CompanyId = company.Id,
                    CustomerId = customerId
                };
            } else {
                model = MapToModel(sp);
            }

            return model;
        }

        public Error InsertOrUpdateBrandCategorySalesPerson(BrandCategorySalesPersonModel acctMgr, UserModel user, string lockGuid) {
            var error = validateAddressModel(acctMgr);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(BrandCategorySalesPerson).ToString(), acctMgr.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "UserId");

                } else {
                    BrandCategorySalesPerson temp = null;
                    if (acctMgr.Id != 0) temp = db.FindBrandCategorySalesPerson(acctMgr.Id);
                    if (temp == null) temp = new BrandCategorySalesPerson();

                    var before = MapToEntity(temp);

                    MapToEntity(acctMgr, temp);

                    db.InsertOrUpdateBrandCategorySalesPerson(temp);
                    acctMgr.Id = temp.Id;

                    logChanges(before, temp, user);
                }
            }
            return error;
        }

        public void DeleteBrandCategorySalesPerson(int id) {
            db.DeleteBrandCategorySalesPerson(id);
        }

        public string LockBrandCategorySalesPerson(BrandCategorySalesPersonModel model) {
            return db.LockRecord(typeof(BrandCategorySalesPerson).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateAddressModel(BrandCategorySalesPersonModel model) {
            var error = new Error();

            return error;
        }

        private void logChanges(BrandCategorySalesPerson before, BrandCategorySalesPerson after, UserModel user) {
            AuditService.LogChanges(typeof(BrandCategorySalesPerson).ToString(), BusinessArea.CustomerAccountManager, user, before, after);
        }

        #endregion
    }
}
