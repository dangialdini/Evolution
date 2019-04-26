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

        public CustomerModel FindCustomer(SalesOrderDetailTempModel sodt, CompanyModel company) {
            var soht = FindSalesOrderHeaderTempModel(sodt.SalesOrderHeaderTempId, company, false);
            return FindCustomer(soht, company);
        }

        public CustomerModel FindCustomer(SalesOrderHeaderTempModel soht, CompanyModel company) {
            CustomerModel cust = null;
            if(soht != null && soht.CustomerId != null) {
                cust = CustomerService.FindCustomerModel(soht.CustomerId.Value, company, false);
            }
            return cust;
        }
    }
}
