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

        public List<ListItemModel> FindCustomerRecipients(SalesOrderHeaderTempModel soht,
                                                          CompanyModel company, UserModel user) {
            List<ListItemModel> recipients = new List<ListItemModel>();

            var customer = FindCustomerModel(soht.CustomerId == null ? 0 : soht.CustomerId.Value, company);

            // Add all the contacts
            recipients.AddRange(FindCustomerContactsListItemModel(customer.Id, true, false));

            // Myself
            recipients.Add(new ListItemModel(user.FullName + " (Myself)", user.Id * -1));

            // Account Manager
            foreach (var sp in FindBrandCategorySalesPersonsModel(company, customer, soht.BrandCategoryId.Value, SalesPersonType.AccountManager)) {
                recipients.Add(new ListItemModel(sp.UserName + " (Account Manager)", sp.UserId * -1));
            }

            // Account Admin manager for brand category
            foreach (var sp in FindBrandCategorySalesPersonsModel(company, customer, soht.BrandCategoryId.Value, SalesPersonType.AccountAdmin)) {
                recipients.Add(new ListItemModel(sp.UserName + " (Account Admin)", sp.UserId * -1));
            }

            return recipients;
        }
    }

    #endregion
}
