using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;

namespace Evolution.SalesService {
    public partial class SalesService {

        public Error PrintSale(SalePrintOptionsViewModel model, 
                               CompanyModel company, UserModel currentUser, string selectedIds) {
            // Prints the sale according to the user selections
            var error = new Error();

            var soh = FindSalesOrderHeaderModelFromTempId(model.SalesOrderHeaderTempId, company, false);
            if (soh != null) {
                var customer = CustomerService.FindCustomerModel(soh.CustomerId.Value, company, false);
                if (customer != null) {
                    // Check the recipients and new contacts
                    var recipients = new List<UserModel>();
                    var copies = new List<UserModel>();
                    error = BuildRecipientLists(selectedIds.ToLower(),
                                                model.CustomerContact,
                                                company,
                                                customer,
                                                model.SaveAsContact,
                                                recipients,
                                                copies);
                    if (!error.IsError) {
                        // Create the required document PDF
                        string outputFile = "";
                        string pdfFile = MediaServices.GetMediaFolder(MediaFolder.Temp, soh.CompanyId) +
                                            "\\" + soh.OrderNumber + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                        error = CreateSalesDocumentPdf(soh, model.TemplateId, pdfFile, model.ShowCancelledItems, ref outputFile);
                        if (!error.IsError) {
                            string url = MediaServices.GetMediaFolder(MediaFolder.Temp, soh.CompanyId, -1, -1, true) + "/" + outputFile.FileName();

                            if (model.SaveInSaleNotesAttachments) {
                                // Save it against sale notes/attachments
                                error = NoteService.AttachNoteToSalesOrder(soh,
                                                                            currentUser,
                                                                            (string.IsNullOrEmpty(model.Subject) ? "Sale Preview Document" : model.Subject),
                                                                            model.Message,
                                                                            outputFile.ToStringList(),
                                                                            FileCopyType.Copy);
                            }
                            if (!error.IsError) {
                                if (model.SendAsEMail) {
                                    // Send it as an email attachment
                                    Dictionary<string, string> dict = new Dictionary<string, string>();

                                    if (!error.IsError) {
                                        var message = new EMailMessage(currentUser, recipients, model.Subject, model.Message);
                                        message.AddCopies(copies);
                                        message.AddProperties(dict);
                                        message.AddAttachment(outputFile, FileCopyType.Copy);

                                        EMailService.EMailService emailService = new Evolution.EMailService.EMailService(db, company);
                                        error = emailService.SendEMail(message);
                                        if (!error.IsError) {
                                            error.SetInfo(EvolutionResources.infEMailSuccessfullySent);

                                            if (model.ViewCreatedDocument) error.URL = url;
                                        }
                                    }

                                } else if (model.ViewCreatedDocument) {
                                    error.URL = url;
                                } else {
                                    error.SetInfo(EvolutionResources.infDocumentSuccessfullyCreated);
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(outputFile)) MediaServices.AddFileToLog(outputFile, 30);
                    }

                } else {
                    error.SetRecordError("Customer", soh.CustomerId.Value);
                }
            } else {
                error.SetError(EvolutionResources.errFailedToFindSOGForSOHT, "", model.SalesOrderHeaderTempId.ToString());
            }
            model.SetError(error.Icon, error.Message, null, null, null, null, true);

            return error;
        }

        private Error BuildRecipientLists(string selectedIds,
                                          CustomerContactModel contact,
                                          CompanyModel company, CustomerModel customer,
                                          bool bSaveAsContact,
                                          List<UserModel> toUsers, List<UserModel> ccUsers) {
            // SelectedIds is supplied as a comma separated list:
            //  To:123,CC:-345,To:OTH
            // Where:
            //  A prefix of 'To' denotes a recipient
            //  A prefix of 'Cc' denotes a user to 'cc' to
            //  A positive number represents an id of a CustomerContact record
            //  A negative number represents the id of a User record (multiply it by -1 to get the positive value)
            //  OTH indicates that the details for 'Other User' should be retrieved from parameter: contact

            // The method returns two lists of UserModels, one for 'To' and one for 'cc' recipients.
            var error = new Error();

            var selected = selectedIds.ToLower().Split(',');
            foreach (var recipient in selected) {
                var pos = recipient.IndexOf(":");
                if (pos != -1) {
                    UserModel user = null;
                    var recType = recipient.Substring(0, pos).ToLower();
                    var recId = recipient.Substring(pos + 1).ToLower();

                    if (recId == "oth") {
                        // New contact
                        if (bSaveAsContact) {
                            contact.CompanyId = company.Id;
                            contact.Enabled = true;
                            error = CustomerService.InsertOrUpdateCustomerContact(contact, user, "");
                        } else {
                            error = CustomerService.ValidateContactModel(contact);
                        }
                        if(error.IsError) {
                            break;
                        } else {
                            user = new UserModel {
                                FirstName = contact.ContactFirstname,
                                LastName = contact.ContactSurname,
                                EMail = contact.ContactEmail
                            };
                        }

                    } else {
                        var id = Convert.ToInt32(recId);
                        if (Convert.ToInt32(id) < 0) {
                            // User
                            user = MembershipManagementService.FindUserModel(Math.Abs(id));

                        } else {
                            // Contact
                            var temp = CustomerService.FindCustomerContactModel(id, company, customer, false);
                            if (temp != null) user = new UserModel {
                                FirstName = temp.ContactFirstname,
                                LastName = temp.ContactSurname,
                                EMail = temp.ContactEmail
                            };
                        }
                    }
                    if (user != null) {
                        if (recType == "to") {
                            toUsers.Add(user);
                        } else {
                            ccUsers.Add(user);
                        }
                    }
                }
            }
            return error;
        }
    }
}
