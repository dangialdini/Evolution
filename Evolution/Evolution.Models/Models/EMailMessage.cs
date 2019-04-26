using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.Models.Models {
    public class EMailMessage {

        #region Public properties

        public UserModel Sender { set; get; }
        public MessageTemplateType TemplateId { set; get; } = MessageTemplateType.None;
        public string Subject { set; get; }
        public string Message { set; get; }
        public List<UserModel> Recipients { set; get; } = new List<UserModel>();
        public List<UserModel> Copies { set; get; } = new List<UserModel>();
        public Dictionary<string, string> Dict { set; get; } = new Dictionary<string, string>();
        public List<string> Attachments { set; get; } = new List<string>();
        public FileCopyType FileCopyType { set; get; } = FileCopyType.Copy;

        #endregion

        #region Construction

        public EMailMessage() { }

        public EMailMessage(UserModel sender) {
            initialiseMessage(sender);
        }

        public EMailMessage(UserModel sender, MessageTemplateType templateId) {
            initialiseMessage(sender, null, null, templateId);
        }

        public EMailMessage(UserModel sender, List<UserModel> recipients, MessageTemplateType templateId) {
            initialiseMessage(sender, null, recipients, templateId);
        }

        public EMailMessage(UserModel sender, UserModel recipient, MessageTemplateType templateId) {
            initialiseMessage(sender, recipient, null, templateId);
        }

        public EMailMessage(UserModel sender, string subject, string message = "") {
            initialiseMessage(sender, null, null, MessageTemplateType.None, subject, message);
        }

        public EMailMessage(UserModel sender, List<UserModel> recipients, string subject, string message = "") {
            initialiseMessage(sender, null, recipients, MessageTemplateType.None, subject, message);
        }

        public EMailMessage(UserModel sender, UserModel recipient, string subject, string message = "") {
            initialiseMessage(sender, recipient, null, MessageTemplateType.None, subject, message);
        }

        private void initialiseMessage(UserModel sender, 
                                       UserModel recipient = null, 
                                       List<UserModel> recipients = null,
                                       MessageTemplateType templateId = MessageTemplateType.None,
                                       string subject = "",
                                       string message = "") {
            Sender = sender;

            if (recipient != null) AddRecipient(recipient);
            if (recipients != null) AddRecipients(recipients);

            if(templateId != MessageTemplateType.None) {
                TemplateId = templateId;
                Subject = Message = "";
            } else {
                TemplateId = MessageTemplateType.None;
                Subject = subject;
                Message = message;
            }
        }

        #endregion

        #region Public methods

        public void AddRecipients(List<UserModel> recipients) {
            if(recipients != null) addUsers(Recipients, recipients);
        }
        public void AddRecipient(UserModel user) {
            if (user != null) addUsers(Recipients, user);
        }
        public void AddRecipient(string email) {
            if (!string.IsNullOrEmpty(email)) {
                var user = new UserModel("", "", email);
                addUsers(Recipients, user);
            }
        }

        public void AddCopies(List<UserModel> copies) {
            if (copies != null) addUsers(Copies, copies);
        }
        public void AddCopy(UserModel user) {
            if (user != null) addUsers(Copies, user);
        }
        public void AddCopy(string email) {
            if (!string.IsNullOrEmpty(email)) {
                var user = new UserModel("", "", email);
                addUsers(Copies, user);
            }
        }

        public void AddProperty(string key, string value) {
            Dict.AddProperty(key, value);
        }

        public void AddProperty(string key, int value) {
            Dict.AddProperty(key, value);
        }

        public void AddProperty(string key, decimal value) {
            Dict.AddProperty(key, value);
        }

        public void AddProperties(Dictionary<string, string> props) {
            if (props != null) {
                foreach (var prop in props) {
                    Dict.AddProperty(prop.Key, prop.Value);
                }
            }
        }

        public void AddAttachment(string fileName, FileCopyType copyType = FileCopyType.Copy) {
            if (!string.IsNullOrEmpty(fileName)) {
                FileCopyType = copyType;
                Attachments.Add(fileName);
            }
        }

        public void AddAttachments(List<string> fileNames, FileCopyType copyType = FileCopyType.Copy) {
            if (fileNames != null) {
                FileCopyType = copyType;
                Attachments.AddRange(fileNames);
            }
        }

        #endregion

        #region Private methods

        private void addUsers(List<UserModel> userList, List<UserModel> recipients) {
            // Adds users to recipient list, ensuring no duplicates
            foreach(var user in recipients) {
                if (userList.Where(ul => ul.EMail == user.EMail)
                           .FirstOrDefault() == null) userList.Add(user);
            }
        }

        private void addUsers(List<UserModel> userList, UserModel recipient) {
            // Adds users to recipient list, ensuring no duplicates
            if (userList.Where(ul => ul.EMail == recipient.EMail)
                        .FirstOrDefault() == null) userList.Add(recipient);
        }

        #endregion
    }
}
