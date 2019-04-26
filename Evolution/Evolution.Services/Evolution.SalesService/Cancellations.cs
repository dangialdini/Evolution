using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.ViewModels;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.AuditService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.SalesService {
    public partial class SalesService {

        private int getEOLProductFlags(CancellationStep1 data) {
            int rc = 0;
            for (int i = 0; i < data.ProductStatusList.Count(); i++) {
                if (data.ProductStatus[i].Value) rc |= Convert.ToInt32(data.ProductStatusList[i].Id);
            }
            if (rc == 0) rc = 0xffff;   // No flags set by user, so switch everything on otherwise nothing will be found
            return rc;
        }

        public List<ListItemModel> FindCancellationCustomersListItemModel(CompanyModel company, CancellationViewModel data) {
            List<ListItemModel> model = new List<ListItemModel>();

            foreach (var cust in db.FindCancellationCustomersList(company.Id,
                                                                 data.Step1.BrandCategoryId,
                                                                 data.Step1.DeliveryWindowClosed,
                                                                 getEOLProductFlags(data.Step1))) {
                model.Add(new ListItemModel {
                    Id = cust.Id.ToString(),
                    Text = cust.Name
                });
            }

            return model;
        }

        public List<ListItemModel> FindCancellationOrdersListItemModel(CompanyModel company,
                                                                       CancellationViewModel data) {
            List<ListItemModel> model = new List<ListItemModel>();

            foreach (var order in db.FindCancellationOrderList(company.Id,
                                                               data.Step1.BrandCategoryId,
                                                               data.Step2.CustomerList.ToIntString(),
                                                               data.Step1.DeliveryWindowClosed,
                                                               getEOLProductFlags(data.Step1))) {
                model.Add(new ListItemModel {
                    Id = order.Id.ToString(),
                    Text = order.OrderNumber + (order.OrderDate == null ? "" : " " + order.OrderDate.Value.ToString(company.DateFormat))
                });
            }
            return model;
        }

        public List<ListItemModel> FindCancellationProductListItemModel(CompanyModel company,
                                                                        CancellationViewModel data) {
            List<ListItemModel> model = new List<ListItemModel>();

            foreach (var product in db.FindCancellationProductList(company.Id,
                                                                   data.Step1.BrandCategoryId,
                                                                   data.Step2.CustomerList.ToIntString(),
                                                                   data.Step3.OrderList.ToIntString(),
                                                                   data.Step1.DeliveryWindowClosed,
                                                                   getEOLProductFlags(data.Step1))) {
                model.Add(new ListItemModel {
                    Id = product.Id.ToString(),
                    Text = product.ItemNumber + " " + product.ItemName
                });
            }
            return model;
        }

        public List<ListItemModel> FindCancellationWarehouseListItemModel(CompanyModel company,
                                                                          CancellationViewModel data) {
            List<ListItemModel> model = new List<ListItemModel>();

            foreach (var warehouse in db.FindCancellationWarehouseList(company.Id,
                                                                       data.Step1.DeliveryWindowClosed,
                                                                       getEOLProductFlags(data.Step1))) {
                model.Add(new ListItemModel {
                    Id = warehouse.LocationId.ToString(),
                    Text = warehouse.LocationName
                });
            }
            return model;
        }

        public List<ListItemModel> FindCancellationAccountManagerListItemModel(CompanyModel company,
                                                                               CancellationViewModel data) {
            List<ListItemModel> model = new List<ListItemModel>();

            foreach (var acctMgr in db.FindCancellationAccountManagerList(company.Id,
                                                                           data.Step2.CustomerList.ToIntString(),
                                                                           data.Step3.OrderList.ToIntString(),
                                                                           data.Step1.DeliveryWindowClosed,
                                                                           getEOLProductFlags(data.Step1))) {
                model.Add(new ListItemModel {
                    Id = acctMgr.Id.ToString(),
                    Text = acctMgr.FullName
                });
            }
            return model;
        }

        public CancellationSummaryListModel FindCancellationSummaryListModel(CompanyModel company,
                                                                             CancellationViewModel data,
                                                                             int index) {
            var model = new CancellationSummaryListModel();

            model.GridIndex = index;
            foreach (var item in db.FindCancellationSummaryList(company.Id,
                                                                data.Step2.CustomerList.ToIntString(),
                                                                data.Step3.OrderList.ToIntString(),
                                                                data.Step4.ProductList.ToIntString(),
                                                                data.Step5.WarehouseList.ToIntString(),
                                                                data.Step6.AccountManagerList.ToIntString(),
                                                                data.Step1.DeliveryWindowClosed,
                                                                getEOLProductFlags(data.Step1),
                                                                data.Step1.CancelAll)) {
                model.Items.Add(Mapper.Map<FindCancellationSummaryList_Result, CancellationSummaryModel>(item));
            }
            return model;
        }

        public Error DoCancellations(CompanyModel company,
                                          CancellationViewModel data) {
            var error = new Error();
            int numItems = 0;

            foreach (var item in db.FindCancellationSummaryList(company.Id,
                                                                data.Step2.CustomerList.ToIntString(),
                                                                data.Step3.OrderList.ToIntString(),
                                                                data.Step4.ProductList.ToIntString(),
                                                                data.Step5.WarehouseList.ToIntString(),
                                                                data.Step6.AccountManagerList.ToIntString(),
                                                                data.Step1.DeliveryWindowClosed,
                                                                getEOLProductFlags(data.Step1),
                                                                data.Step1.CancelAll)
                                   .ToList()) {
                // Drop a line's alloactions
                db.DeleteAllocationsForSaleLine(company.Id, item.SodId);

                // Cancel the line
                var line = db.FindSalesOrderDetail(item.SodId);
                line.LineStatusId = db.FindSalesOrderHeaderSubStatus(SalesOrderHeaderSubStatus.PartiallyCompletePartiallyCancelled).Id;
                line.AllocQty = 0;
                line.DateModified = DateTimeOffset.Now;
                db.InsertOrUpdateSalesOrderDetail(line);

                numItems++;
            }

            if(numItems == 0) {
                error.SetError("No items were selected for Cancellation!");

            } else {
                error.Message = numItems.ToString() + " Item(s) successfully cancelled.";
            }

            return error;
        }
    }
}
