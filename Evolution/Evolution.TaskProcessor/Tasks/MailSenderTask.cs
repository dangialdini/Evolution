using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Evolution.DAL;
using Evolution.TaskService;
using Evolution.Models.Models;
using Evolution.Enumerations;
using Evolution.EMailService;
using Evolution.Extensions;
using System.Net.Mail;
using System.Net;
using AutoMapper;

namespace Evolution.TaskProcessor {
    public class MailSenderTask : TaskBase {

        #region Private members

        private int     _smtpPort = 0;
        private string  _smtpServer = "",
                        _smtpUserName,
                        _smtpPassword;

        #endregion

        #region Construction

        protected IMapper Mapper = null;

        public MailSenderTask(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<User, UserModel>();
            }));

            Mapper = config.CreateMapper();
        }

        public override string GetTaskName() { return TaskName.MailSenderTask; }

        #endregion

        #region Main processing loop

        public override int DoProcessing(string[] args) {

            _smtpServer = GetTaskParameter("SMTPServer", "");
            _smtpPort = GetTaskParameter("SMTPPort", 25);
            _smtpUserName = GetTaskParameter("SMTPUserName", "");
            _smtpPassword = GetTaskParameter("SMTPPassword", "");

            ScanLogsForErrors();
            SendMessages();

            return 0;
        }

        #endregion

        void ScanLogsForErrors() {

            LogSeverity severity = (LogSeverity)GetTaskParameter("ReportErrorsAboveSeverity", (int)LogSeverity.Severe);

            DateTime lastRun = GetTaskParameter("LastRun", DateTime.Now);
            SetTaskParameter("LastRun", DateTime.Now);

            string  systemErrors = "",
                    taskErrors = "";

            foreach(var logRecord in _db.FindLogsAfter(lastRun, severity)) {
                systemErrors += "<tr><td style=\"width:120px\">Date:</td><td style=\"width:auto\">" + logRecord.LogDate.ToString() + "</td></tr>\r\n";
                systemErrors += "<tr><td style=\"vertical-align:top\">URL:</td><td>" + logRecord.Url + "</td></tr>\r\n";
                systemErrors += "<tr><td style=\"vertical-align:top\">Error:</td><td>" + logRecord.Message + "</td></tr>\r\n";
                systemErrors += "<tr><td style=\"vertical-align:top\">Stack Trace:</td><td>" + logRecord.StackTrace + "</td></tr>\r\n";
            }

            foreach (var logRecord in _db.FindScheduledTaskLogsAfter(lastRun, severity)) {
                taskErrors += "<tr>";
                taskErrors += "<td style=\"width:120px\">" + logRecord.LogDate.ToString() + "</td>";
                taskErrors += "<td style=\"vertical-align:top\">" + logRecord.ScheduledTask.TaskName + "</td>";
                taskErrors += "<td style=\"vertical-align:top\">" + logRecord.Message;
                if (!string.IsNullOrEmpty(logRecord.StackTrace)) taskErrors += "<br/><br/>" + logRecord.StackTrace;
                taskErrors += "</td>";
                taskErrors += "</tr>\r\n";
            }

            if (!string.IsNullOrEmpty(systemErrors) || !string.IsNullOrEmpty(taskErrors)) {
                if (string.IsNullOrEmpty(systemErrors)) systemErrors = "<tr><td>None></td><td></td></tr>";
                if (string.IsNullOrEmpty(taskErrors)) taskErrors = "<tr><td>None></td><td></td><td></td></tr>";

                string groupName = GetTaskParameter("SendErrorsToGroup", "");
                UserGroup userGroup = _db.FindUserGroup(groupName);
                if (userGroup == null) {
                    WriteTaskLog($"Error: User group '{groupName}' could not be found!");

                } else {
                    var sender = new UserModel { EMail = GetTaskParameter("SenderEMail", "noreply@placeholder.com.au") };
                    var message = new EMailMessage(sender, MessageTemplateType.SystemError);

                    message.AddProperty("SYSTEMERRORS", systemErrors);
                    message.AddProperty("TASKERRORS", taskErrors);

                    foreach(var user in _db.FindUserGroupUsers(userGroup)) {
                        var userModel = Mapper.Map<User, UserModel>(user);
                        message.AddRecipient(userModel);
                    }

                    EMailService.EMailService emailService = new EMailService.EMailService(_db);
                    emailService.SendEMail(message);
                }
            }
        }

        void SendMessages() {

            int totalEMails = 0,
                successCount = 0,
                errorCount = 0;

            // Get the EMail style sheet
            string cssFile = GetConfigSetting("EMailCssFile", @"C:\Development\Evolution\Evolution\Evolution\Content\EMailStyles.css");
            string cssContent = "";

            try {
                using (StreamReader sr = new StreamReader(cssFile)) {
                    cssContent = sr.ReadToEnd();
                }
            } catch(Exception e1) {
                WriteTaskLog($"Failed to load EMailCssFile: '{cssFile}' : {e1.Message}", LogSeverity.Severe);
            }

            // Send each email in the queue
            var emailList = _db.FindEMailQueues()
                               .Where(eq => eq.Retries < 5)
                               .ToList();
            //Parallel.ForEach(emailList, (email) => {
            foreach (var email in emailList) {
                totalEMails++;

                WriteTaskLog($"Sending Message '{email.MessageSubject}' to '{email.RecipientAddress}'");

                string emailText = email.MessageText;
                if(emailText.IsHtml()) {
                    emailText = "<html>";
                    emailText += "<head>";
                    emailText += "<style>";
                    emailText += cssContent;
                    emailText += "</style>";
                    emailText += "</head>";
                    emailText += "<body>";
                    emailText += email.MessageText;
                    emailText += "</body>";
                    emailText += "<html>";
                }

                // Use the SMTP server to send the email with any attachments
                List<string> attachments = email.EMailQueueAttachments
                                                .OrderBy(eqa => eqa.OrderNo)
                                                .Select(eqa => eqa.FileName)
                                                .Distinct()
                                                .ToList();

                string errorMsg = "";
                if (!SendMessageToUserSmtp(email.SenderAddress, email.SenderAddress, email.ReplyToAddress,
                                           email.RecipientAddress, email.CopyAddress, email.BCCAddress,
                                           email.MessageSubject, email.MessageText,
                                           attachments,
                                           ref errorMsg)) {
                    // Successfully sent, so delete from queue
                    _db.DeleteEMailQueue(email);
                    WriteTaskLog($"Message successfully send to '{email.RecipientAddress}'");
                    if(!string.IsNullOrEmpty(email.CopyAddress)) WriteTaskLog($"Message successfully copied to '{email.CopyAddress}'");
                    successCount++;

                } else {
                    // Error, so re-queue for retry later
                    email.Retries++;
                    _db.InsertOrUpdateEMailQueue(email);

                    WriteTaskLog("Failed to send message - queued for retry");
                    errorCount++;
                }
            }

            WriteTaskLog($"{totalEMails} message(s) processed, {successCount} sent successfully, {errorCount} error(s)");
        }

        public bool SendMessageToUserSmtp(string fromName, string fromEMail, string replyToEMail,
                                          string recipientEMail, string ccEMail, string bccEMail, 
                                          string messageSubject, string messageText,
                                          List<string> attachments,
                                          ref string errorMsg) {
            bool    bRc = false;

            // The recipient/copy lists can be a comma delimeted list of addresses
            try {
                // Send an HTML message
                MailMessage msg = new MailMessage();

                // Remove some headers before adding them again
                try {
                    msg.Headers.Remove("Reply-To");
                } catch { }
                try {
                    msg.Headers.Remove("Return-path");
                } catch { }
                try {
                    msg.Headers.Remove("X-Sender");
                } catch { }
                try {
                    msg.Headers.Remove("List-Id");
                } catch { }
                try {
                    msg.Headers.Remove("Message-ID");
                } catch { }
                try {
                    msg.Headers.Remove("In-Reply-To");
                } catch { }

                // Now set all the header information
                msg.From = new MailAddress(fromEMail, fromName);
                msg.Headers.Add("Reply-To", replyToEMail);
                msg.Headers.Add("Return-path", replyToEMail);

                var recList = new SeparatedString(recipientEMail);
                string recipient = "";
                while((recipient = recList.NextString()) != null) {
                    msg.To.Add(new MailAddress(recipient));
                }

                recList = new SeparatedString(ccEMail);
                while ((recipient = recList.NextString()) != null) {
                    msg.CC.Add(new MailAddress(recipient));
                }

                recList = new SeparatedString(bccEMail);
                while ((recipient = recList.NextString()) != null) {
                    msg.Bcc.Add(new MailAddress(recipient));
                }

                msg.Subject = messageSubject;

                msg.IsBodyHtml = messageText.IsHtml();
                msg.Body = messageText;

                msg.Priority = MailPriority.Normal;

                // Add the attachments
                if(attachments != null) {
                    foreach(string fileName in attachments) {
                        msg.Attachments.Add(new Attachment(fileName));
                    }
                }

                // Send the message
                WriteTaskLog("Sending as '" + fromEMail + "' using: " + _smtpServer);

                SmtpClient smtpClient = new SmtpClient(_smtpServer, _smtpPort);
                NetworkCredential credentials = new NetworkCredential(_smtpUserName, _smtpPassword);
                smtpClient.Credentials = credentials;
                //smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                smtpClient.Send(msg);

            } catch (Exception e) {
                // Failed to send message, so report error
                if (errorMsg != "") errorMsg += " (" + errorMsg + ")<br/>";
                errorMsg += e.Message;

                WriteTaskLog(errorMsg, LogSeverity.Severe);
                bRc = true;
            }

            return bRc;
        }
    }
}
