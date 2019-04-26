using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.AuditService;
using Evolution.Enumerations;
using Evolution.Resources;

namespace Evolution.PurchasingService {
    public partial class PurchasingService {

        #region Public methods

        public List<UserModel> FindOrderPurchasers(PurchaseOrderHeaderModel poh, 
                                                   CompanyModel company,
                                                   decimal poNumber, 
                                                   ref string errorMsg) {
            // Given a PurchaseOrderHeader model, finds the sales person (purchaser) 
            // for the order and returns a list of all the users in the same user 
            // group as the sales person, including the sales person.
            List<UserModel> users = null;

            UserModel salesPerson = null;
            if (poh.SalespersonId != null) salesPerson = MembershipManagementService.FindUserModel(poh.SalespersonId.Value);
            if (salesPerson != null) {
                // Found the sales person
                BrandCategoryModel brandCat = null;
                if (poh.BrandCategoryId != null) brandCat = ProductService.FindBrandCategoryModel(poh.BrandCategoryId.Value, company, false);
                if (brandCat != null) {
                    string groupName = brandCat.CategoryName.ToLower() + " purchasing";
                    var userGroup = MembershipManagementService.FindGroupsForUser(salesPerson)
                                                           .Where(ug => ug.GroupName.ToLower().Contains(groupName))
                                                           .FirstOrDefault();
                    if (userGroup != null) {
                        // Found the group, so get all the users in the group, including the sales person
                        users = MembershipManagementService.FindUsersInGroup(userGroup);
                        if (users.Count() == 0) {
                            errorMsg = $"Error: Active Directory User Group '{groupName}' has no members!";
                            users = null; ;
                        }

                    } else {
                        errorMsg = $"Error: Failed to find Active Directory Group '{groupName}' !";
                    }

                } else {
                    errorMsg = $"Error: Failed to find a Brand Catgeory for Purchase Order Number {poNumber} !";
                }

            } else {
                errorMsg = $"Error: Failed to find a Sales Person for Purchase Order Number {poNumber} !";
            }

            return users;
        }

        public List<ListItemModel> FindPurchasersListItemModel(CompanyModel company) {
            List<ListItemModel> model = new List<ListItemModel>();
            foreach (var item in db.FindPurchaseOrderHeaders(company.Id)
                                   .Where(poh => poh.User_SalesPerson != null)
                                   .Select(poh => poh.User_SalesPerson)
                                   .Distinct()) { 
                model.Add(new ListItemModel {
                    Id = item.Id.ToString(),
                    Text = db.MakeName(item),
                    ImageURL = ""
                });
            }
            return model.OrderBy(m => m.Text)
                        .ToList();
        }

        #endregion
    }
}
