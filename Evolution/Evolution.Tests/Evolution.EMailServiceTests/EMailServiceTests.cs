using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using CommonTest;
using Evolution.EMailService;
using Evolution.LookupService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.TaskProcessor;

namespace Evolution.EMailServiceTests {
    [TestClass]
    public class EMailServiceTests : BaseTest {
        [TestMethod]
        public void FindEMailQueueListModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompanyAU();

            EMailService.EMailService emailService = new EMailService.EMailService(db, testCompany);

            // Now create an email and try to search for it
            string  subject = RandomString(),
                    text    = LorumIpsum();
            var     sender  = new UserModel { EMail = RandomEMail() };

            var message = new EMailMessage(sender, testUser, subject, text);
            var result = emailService.SendEMail(message);
            Assert.IsTrue(!result.IsError, result.Message);

            // Perform a global test which should bring back the first page
            var emailList = emailService.FindEMailQueueListModel(0, 1, PageSize, "");
            int actual = emailList.Items
                                  .Where(e => e.MessageSubject == subject)
                                  .Count();
            int expected = 1;
            Assert.IsTrue(actual == expected, $"Error: {actual} records were found when {expected} was expected");

            // Delete and check again
            emailService.DeleteEMailQueue(result.Id);

            emailList = emailService.FindEMailQueueListModel(0, 1, PageSize, "");
            actual = emailList.Items
                              .Where(e => e.MessageSubject == subject)
                              .Count();
            expected = 0;
            Assert.IsTrue(actual == expected, $"Error: {actual} records were found when {expected} was expected");
        }

        [TestMethod]
        public void SendEMailTest() {
            // Get some test users and test company
            List<UserModel> users = new List<UserModel>();
            for (int i = 0; i < 10; i++) users.Add(GetTestUser());

            var testCompany = GetTestCompany(users[0]);

            EMailService.EMailService emailService = new EMailService.EMailService(db, testCompany);


            // Tests using email templates
            var sender = new UserModel { EMail = RandomEMail() };
            var message = new EMailMessage(sender, MessageTemplateType.TestMessage);

            // Single user
            var recipient = new UserModel { EMail = RandomEMail() };
            message.AddRecipient(users[0]);

            var result = emailService.SendEMail(message);
            Assert.IsTrue(!result.IsError, result.Message);

            var msgs = getMessages(sender);
            int actual = msgs.Count(),
                expected = 1;
            Assert.IsTrue(actual == expected, $"Error: {actual} records were found when {expected} were expected");

            var emailTemplate = LookupService.FindMessageTemplateModel(testCompany.Id, MessageTemplateType.TestMessage);
            checkEMailContent(msgs, sender, emailTemplate.Subject);

            // Multiple users
            sender = new UserModel { EMail = RandomEMail() };
            message = new EMailMessage(sender, MessageTemplateType.TestMessage);
            message.AddRecipients(users);

            result = emailService.SendEMail(message);
            Assert.IsTrue(!result.IsError, result.Message);

            msgs = getMessages(sender);
            actual = msgs.Count();
            expected = 1;       // One message is sent to all recipients/cc;s/bcc's
            Assert.IsTrue(actual == expected, $"Error: {actual} records were found when {expected} were expected");

            emailTemplate = LookupService.FindMessageTemplateModel(testCompany.Id, MessageTemplateType.TestMessage);
            checkEMailContent(msgs, sender, emailTemplate.Subject);


            // Tests using straight text
            string  subject = RandomString(),
                    text = LorumIpsum();

            // Single user
            sender = new UserModel { EMail = RandomEMail() };
            message = new EMailMessage(sender, subject, text);
            message.AddRecipient(users[0]);

            var result2 = emailService.SendEMail(message);
            Assert.IsTrue(!result2.IsError, result.Message);

            msgs = getMessages(sender);
            actual = msgs.Count();
            expected = 1;
            Assert.IsTrue(actual == expected, $"Error: {actual} records were found when {expected} were expected");
            checkEMailContent(msgs, sender, subject);

            // Multiple users
            sender = new UserModel { EMail = RandomEMail() };
            message = new EMailMessage(sender, subject, text);
            message.AddRecipients(users);

            result = emailService.SendEMail(message);
            Assert.IsTrue(!result.IsError, result.Message);

            msgs = getMessages(sender);
            actual = msgs.Count();
            expected = 1;       // One message is sent to all recipients/cc;s/bcc's
            Assert.IsTrue(actual == expected, $"Error: {actual} records were found when {expected} were expected");
            checkEMailContent(msgs, sender, subject);
        }

        List<EMailQueue> getMessages(UserModel sender) {
            return db.FindEMailQueues()
                     .Where(eq => eq.SenderAddress == sender.EMail)
                     .ToList();
        }

        [TestMethod]
        public void AddOrganisationDetailsTest() {
            // Tested in all SendEMailTests as SendEMail calls EMailService.AddOrganisationDetails
        }

        [TestMethod]
        public void AddUserDetailsTest() {
            var testUser = GetTestUser(true, false);

            Dictionary<string, string> dict = new Dictionary<string, string>();

            int expected = 0,
                actual = dict.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            EMailService.EMailService emailService = new EMailService.EMailService(db);
            emailService.AddUserDetails(testUser, dict);

            expected = 2;
            actual = dict.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            string keyName = "USERNAME";
            try {
                var test = dict[keyName].ToString();
                Assert.IsTrue(testUser.FullName == test, $"Error: {test} was returned when {testUser.FullName} was expected");
            } catch {
                Assert.Fail($"Error: Item '{keyName}' was not found in the dictionary");
            }

            keyName = "EMAIL";
            try {
                var test = dict[keyName].ToString();
                Assert.IsTrue(testUser.EMail == test, $"Error: {test} was returned when {testUser.EMail} was expected");
            } catch {
                Assert.Fail($"Error: Item '{keyName}' was not found in the dictionary");
            }
        }

        [TestMethod]
        public void GetQueueCountTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            EMailService.EMailService emailService = new EMailService.EMailService(db, testCompany);

            int expected = emailService.GetQueueCount();

            // Create a message
            string subject = RandomString(),
                    text = LorumIpsum();
            var     sender = new UserModel { EMail = RandomEMail() };
            var     message = new EMailMessage(sender, testUser, subject, text);

            var result = emailService.SendEMail(message);
            Assert.IsTrue(!result.IsError, result.Message);

            int actual = emailService.GetQueueCount();
            expected++;
            Assert.IsTrue(actual == expected, $"Error: SendEMail returned error code {result} when 0 was expected");

            // Delete the email
            emailService.DeleteEMailQueue(result.Id);

            actual = emailService.GetQueueCount();
            expected--;
            Assert.IsTrue(actual == expected, $"Error: SendEMail returned error code {result} when 0 was expected");
        }

        [TestMethod]
        public void DeleteEMailQueueTest() {
            // Tested in GetEMailQueueCountTest
        }

        [TestMethod]
        public void EmptyEMailQueueTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            EMailService.EMailService emailService = new EMailService.EMailService(db, testCompany);

            var sender = new UserModel { EMail = GetAppSetting("SenderAddress", "") };
            UserModel recipient = new UserModel { EMail = GetAppSetting("RecipientAddress", "") };

            // Create messages
            int numMsgs = 10;
            for (int i = 0; i < numMsgs; i++) {
                var error = new Error();

                var message = new EMailMessage(sender, recipient, MessageTemplateType.TestMessage);
                message.AddProperty("MESSAGECONTENT", $"This message is {i + 1} of {numMsgs} and has been sent by an Evolution outomated test - please ignore.");
                error = emailService.SendEMail(message);

                Assert.IsTrue(!error.IsError, error.Message);
            }

            // Get the queue count
            int expected = 10,
                actual = emailService.GetQueueCount();
            Assert.IsTrue(actual >= expected, $"Error: {actual} queue messages were found when {expected} were expected");

            // Clean the queue
            emailService.EmptyEMailQueue();

            // Chekc it again
            expected = 0;
            actual = emailService.GetQueueCount();
            Assert.IsTrue(actual == expected, $"Error: {actual} queue messages were found when {expected} were expected");
        }

        [TestMethod]
        public void SendEMailThroughTaskServiceTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            EMailService.EMailService emailService = new EMailService.EMailService(db, testCompany);
            MediaService.MediaService mediaService = new Evolution.MediaService.MediaService(db);

            var sender = new UserModel { EMail = GetAppSetting("SenderAddress", "") };
            UserModel recipient = new UserModel { EMail = GetAppSetting("RecipientAddress", "") };

            // Create messages, the first 10 without attachments, the rest with attachments
            int numMsgs = 20;
            for (int i = 0; i < numMsgs; i++) {
                var error = new Error();
                var message = new EMailMessage(sender, recipient, MessageTemplateType.TestMessage);

                if (i >= 10) {
                    message.AddProperty("MESSAGECONTENT", $"This message is {i + 1} of {numMsgs} and has been sent with an attachment by an Evolution outomated test - please ignore.");

                    string tempFile = MediaService.MediaService.GetTempFile(".txt");
                    db.LogTestFile(tempFile);
                    message.AddAttachment(tempFile, FileCopyType.Move);

                    using (StreamWriter sw = new StreamWriter(tempFile)) {
                        sw.WriteLine("Attachment " + (i - 9).ToString());
                        sw.WriteLine("");
                        sw.WriteLine(LorumIpsum());
                    }

                    error = emailService.SendEMail(message);
                } else {
                    message.AddProperty("MESSAGECONTENT", $"This message is {i + 1} of {numMsgs} and has been sent by an Evolution outomated test - please ignore.");

                    error = emailService.SendEMail(message);
                }
                Assert.IsTrue(!error.IsError, error.Message);
            }

            if (GetAppSetting("EMailSendingServiceInTests", false)) {
                TaskService.TaskService ts = new TaskService.TaskService(db);

                var mailSender = ts.FindScheduledTaskModel(TaskName.MailSenderTask);
                Assert.IsTrue(mailSender != null, "Error: MailSender Scheduled Task not found");

                var enabled = mailSender.Enabled;
                if (!enabled) ts.EnableTask(mailSender, true);

                //ts.RunTask(mailSender.Id, true);  
                var mailSenderTask = new MailSenderTask(db);
                string[] args = null;
                mailSenderTask.Run(args);

                mailSender = ts.FindScheduledTaskModel(TaskName.MailSenderTask);
                Assert.IsTrue(mailSender != null, "Error: MailSender Scheduled Task not found");
                ts.EnableTask(mailSender, enabled);
            }
        }

        void checkEMailContent(List<EMailQueue> emails, UserModel sender, string messageSubject) {
            foreach(var email in emails) {
                Assert.IsTrue(email.QueuedDate != null, $"Error: A NULL QueuedDate was found when a valid date was expected");
                Assert.IsTrue(email.SenderAddress == sender.EMail, $"Error: SenderAddress is {email.SenderAddress} when {sender.EMail} was expected");
                Assert.IsTrue(email.ReplyToAddress == sender.EMail, $"Error: ReplyToAddress is {email.ReplyToAddress} when {sender.EMail} was expected");
                Assert.IsTrue(!string.IsNullOrEmpty(email.RecipientAddress), $"Error: RecipientAddress is {email.RecipientAddress} when a non-NULL value was expected");
                Assert.IsTrue(!string.IsNullOrEmpty(email.MessageSubject), $"Error: MessageSubject is {email.MessageSubject} when a {messageSubject} was expected");
                Assert.IsTrue(!string.IsNullOrEmpty(email.MessageText), $"Error: MessageText is {email.MessageText} when a non-NULL value was expected");
                Assert.IsTrue(email.Retries < 1, $"Error: Retries is {email.Retries} when a value >1 was expected");
            }
        }
    }
}
