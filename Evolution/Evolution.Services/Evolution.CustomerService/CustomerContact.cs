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
using AutoMapper;

namespace Evolution.CustomerService {
    public partial class CustomerService : CommonService.CommonService {

        public List<ListItemModel> FindCustomerContactsListItemModel(int customerId,
                                                                     bool bIncludeRole = false,
                                                                     bool bShowHidden = false) {
            List<ListItemModel> model = new List<ListItemModel>();
            foreach (var contact in db.FindCustomerContacts(customerId)
                                      .Where(cc => cc.Enabled == true || bShowHidden == true)) {
                var newItem = new ListItemModel {
                    Id = contact.Id.ToString(),
                    Text = (contact.ContactFirstname + " " + contact.ContactSurname).Trim(),
                    ImageURL = ""
                };
                if(bIncludeRole) {
                    newItem.Text += " ";
                    if (contact.SendInvoice) newItem.Text += "(Invoice recipient)";
                    if (contact.SendStatement) newItem.Text += "(Statement recipient)";
                    if (contact.PrimaryContact) newItem.Text += "(Primary Contact)";
                }
                model.Add(newItem);
            }
            return model;
        }

        public CustomerContactListModel FindCustomerContactsListModel(int customerId, int index = 0, int pageNo = 1, int pageSize = Int32.MaxValue, string search = "") {
            var model = new CustomerContactListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindCustomerContacts(customerId)
                                .Where(cc => string.IsNullOrEmpty(search) ||
                                             (cc.ContactFirstname != null && cc.ContactFirstname.ToLower().Contains(search.ToLower())) ||
                                             (cc.ContactSurname != null && cc.ContactSurname.ToLower().Contains(search.ToLower())) ||
                                             (cc.ContactKnownAs != null && cc.ContactKnownAs.ToLower().Contains(search.ToLower())) ||
                                             (cc.ContactEmail != null && cc.ContactEmail.ToLower().Contains(search.ToLower())) ||
                                             (cc.Position != null && cc.Position.ToLower().Contains(search.ToLower())))
                                .OrderBy(cc => cc.ContactSurname)
                                .ThenBy(cc => cc.ContactFirstname);

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public CustomerContactModel MapToModel(CustomerContact item) {
            var newItem = Mapper.Map<CustomerContact, CustomerContactModel>(item);
            return newItem;
        }

        public void MapToEntity(CustomerContactModel model, CustomerContact entity) {
            Mapper.Map<CustomerContactModel, CustomerContact>(model, entity);
        }

        public CustomerContactModel FindCustomerContactModel(int id, CompanyModel company, CustomerModel customer, bool bCreateEmptyIfNotfound = true) {
            return FindCustomerContactModel(id, company, customer.Id, bCreateEmptyIfNotfound);
        }

        public CustomerContactModel FindCustomerContactModel(int id, CompanyModel company, int customerId, bool bCreateEmptyIfNotfound = true) {
            CustomerContactModel model = null;

            var a = db.FindCustomerContact(id);
            if (a == null) {
                if (bCreateEmptyIfNotfound) model = new CustomerContactModel { CompanyId = company.Id, CustomerId = customerId };
            } else {
                model = MapToModel(a);
            }

            return model;
        }

        public List<CustomerContactModel> FindPrimaryCustomerContactsModel(CustomerModel customer) {
            List<CustomerContactModel> model = new List<CustomerContactModel>();

            foreach(var item in db.FindCustomerContacts(customer.Id)
                                  .Where(cc => cc.PrimaryContact == true)) {
                model.Add(MapToModel(item));
            }
            return model;
        }

        public Error InsertOrUpdateCustomerContact(CustomerContactModel contact, UserModel user, string lockGuid) {
            var error = ValidateContactModel(contact);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(CustomerContact).ToString(), contact.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "ContactFirstname");

                } else {
                    CustomerContact temp = null;
                    if (contact.Id != 0) temp = db.FindCustomerContact(contact.Id);
                    if (temp == null) temp = db.FindCustomerContact(contact.CustomerId.Value, contact.ContactFirstname, contact.ContactSurname);
                    if (temp == null) temp = new CustomerContact();

                    MapToEntity(contact, temp);

                    db.InsertOrUpdateCustomerContact(temp);
                    contact.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteCustomerContact(int id) {
            db.DeleteCustomerContact(id);
        }

        public string LockCustomerContact(CustomerContactModel model) {
            return db.LockRecord(typeof(CustomerContact).ToString(), model.Id);
        }

        public Error ValidateContactModel(CustomerContactModel model) {
            var error = isValidRequiredString(getFieldValue(model.ContactFirstname), 50, "ContactFirstname", EvolutionResources.errFirstNameRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.ContactSurname), 60, "ContactSurname", EvolutionResources.errSurnameRequired);
            if (!error.IsError) error = isValidRequiredEMail(getFieldValue(model.ContactEmail), 100, "ContactEmail", EvolutionResources.errInvalidEMailAddress);

            return error;
        }
    }
}
