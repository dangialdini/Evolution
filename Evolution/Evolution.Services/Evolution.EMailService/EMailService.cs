using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.EMailService {
    public class EMailService : CommonService.CommonService {

        #region Private members

        CompanyModel _company = new CompanyModel { Id = 0 };

        #endregion

        #region Construction

        protected IMapper Mapper = null;

        public EMailService(EvolutionEntities dbEntities, CompanyModel company = null) : base(dbEntities) {
            if (company != null) {
                // Company supplied as a parameter
                _company = company;
            } else {
                // No company, so get the default 'head office' company
                var temp = db.FindParentCompany();
                if (temp != null) {
                    _company = new CompanyModel {
                        Id = temp.Id,
                        CompanyName = temp.CompanyName
                    };
                } else {
                    throw new Exception("Error: No Parent Company is configured!");
                }
            }

            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<EMailQueue, EMailQueueModel>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion

        #region Public members - sending EMails

        public Error SendEMail(EMailMessage message) {
            var error = new Error();

            string subject = message.Subject,
                   text = message.Message;

            // Find/validate the message template (if specified)
            if(message.TemplateId != MessageTemplateType.None) {
                var messageTemplate = db.FindMessageTemplate(_company.Id, message.TemplateId);
                if(messageTemplate == null) {
                    error.SetError(EvolutionResources.errEMailTemplateNotFound);

                } else {
                    subject = messageTemplate.Subject;
                    text = messageTemplate.Message;
                }
            }

            if (!error.IsError) {
                // Validate the sender
                if (message.Sender == null ||
                   string.IsNullOrEmpty(message.Sender.EMail) ||
                   !message.Sender.EMail.IsValidEMail()) {
                    error.SetError(EvolutionResources.errInvalidSenderEMailAddress, "", (message.Sender == null ? "[Invalid EMail]" : message.Sender.EMail), (message.Sender == null ? "[No Name]" : message.Sender.FullName));

                } else {
                    // Now validate the recipients
                    if (message.Recipients.Count() == 0) {
                        error.SetError(EvolutionResources.errNoToRecipientsSelected);

                    } else {
                        string  recList = "",
                                copyList = "";

                        foreach (var user in message.Recipients) {
                            if (string.IsNullOrEmpty(user.EMail) || !user.EMail.IsValidEMail()) {
                                if (string.IsNullOrEmpty(error.Message)) error.SetError(EvolutionResources.errInvalidEMailsForUsers);
                                error.Message += $"<br/>{user.FullName} ({user.EMail})";
                            } else {
                                if (!string.IsNullOrEmpty(recList)) recList += ",";
                                recList += user.EMail;
                            }
                        }
                        foreach (var user in message.Copies) {
                            if (string.IsNullOrEmpty(user.EMail) || !user.EMail.IsValidEMail()) {
                                if (string.IsNullOrEmpty(error.Message)) error.SetError(EvolutionResources.errInvalidEMailsForUsers);
                                error.Message += $"<br/>{user.FullName} ({user.EMail})";
                            } else {
                                if (!string.IsNullOrEmpty(copyList)) copyList += ",";
                                copyList += user.EMail;
                            }
                        }

                        // Even though we can get errors above, we still send the message
                        // to all the valid addresses
                        if (!string.IsNullOrEmpty(recList)) {
                            AddOrganisationDetails(message.Dict);
                            AddUserDetails(message.Sender, message.Dict);

                            var newEMail = new EMailQueue {
                                QueuedDate = DateTime.Now,
                                SenderAddress = message.Sender.EMail,
                                ReplyToAddress = message.Sender.EMail,
                                RecipientAddress = recList,
                                CopyAddress = copyList,
                                BCCAddress = _company.EmailAddressBCC,
                                MessageSubject = subject.DoSubstitutions(message.Dict),
                                MessageText = text.DoSubstitutions(message.Dict)
                            };
                            db.InsertOrUpdateEMailQueue(newEMail);

                            error = addAttachments(newEMail, message.Attachments, message.FileCopyType);
                            error.Id = newEMail.Id;
                        }
                    }
                }
            }
            return error;
        }

        public void AddOrganisationDetails(Dictionary<string, string> dict) {
            var header = db.FindMessageTemplate(_company.Id, MessageTemplateType.EMailHeader);
            string templateText = (header == null ? "" : header.Message);
            dict.AddProperty("MESSAGEHEADER", templateText);

            var footer = db.FindMessageTemplate(_company.Id, MessageTemplateType.EMailFooter);
            templateText = (footer == null ? "" : footer.Message);
            dict.AddProperty("MESSAGEFOOTER", templateText);

            if(_company != null) dict.AddProperty("COMPANYNAME", _company.CompanyName);

            dict.AddProperty("APPNAME", EvolutionResources.lblApplicationName);
            dict.AddProperty("SITEHTTP", GetConfigurationSetting("SiteHttp", ""));
            dict.AddProperty("MEDIAHTTP", GetConfigurationSetting("MediaHttp", ""));
        }

        public void AddUserDetails(UserModel user, Dictionary<string, string> dict) {
            dict.AddProperty("USERNAME", user.FullName);
            dict.AddProperty("EMAIL", user.EMail);
        }

        private Error addAttachments(EMailQueue email, List<string> attachments, FileCopyType fileCopyType) {
            var error = new Error();
            if (attachments != null && attachments.Count() > 0) {
                // Queue the attachments
                int orderNo = 1;
                foreach (var fileName in attachments) {
                    // Copy or move the file to a temporary location for sending. After sending, we delete it.
                    string tempFolder = GetConfigurationSetting("MediaFolder", "") + "\\EMailQueue\\" + email.Id.ToString();
                    try {
                        Directory.CreateDirectory(tempFolder);
                    } catch { }
                    db.LogTestFolder(tempFolder);

                    // Copy/move the file
                    string targetFile = tempFolder + "\\" + fileName.FileName();

                    try {
                        if (fileCopyType == FileCopyType.Move) {
                            File.Move(fileName, targetFile);
                        } else {
                            File.Copy(fileName, targetFile, true);
                        }

                        var attachment = new EMailQueueAttachment {
                            EMailQueue = email,
                            FileName = targetFile,
                            OrderNo = orderNo++
                        };
                        db.InsertOrUpdateEMailQueueAttachment(attachment);

                    } catch (Exception e1) {
                        error.SetError(e1);
                        break;
                    }
                }
            }
            return error;
        }

        #endregion

        #region Public members - queue management

        public int GetQueueCount() {
            return db.FindEMailQueues().Count();
        }

        public EMailQueueListModel FindEMailQueueListModel(int index, int pageNo, int pageSize, string search) {
            var model = new EMailQueueListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindEMailQueues()
                             .Where(eq => string.IsNullOrEmpty(search) ||
                                          (eq.MessageSubject != null && eq.MessageSubject.ToLower().Contains(search.ToLower())) ||
                                          (eq.MessageText != null && eq.MessageText.ToLower().Contains(search.ToLower())));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = mapToModel(item);
                newItem.AttachmentCount = item.EMailQueueAttachments.Count();
                model.Items.Add(newItem);
            }
            return model;
        }

        EMailQueueModel mapToModel(EMailQueue item) {
            var newItem = Mapper.Map<EMailQueue, EMailQueueModel>(item);
            return newItem;
        }

        public void DeleteEMailQueue(int id) {
            db.DeleteEMailQueue(id);
        }

        public void EmptyEMailQueue() {
            db.EmptyEMailQueue();
        }

        #endregion
    }
}
