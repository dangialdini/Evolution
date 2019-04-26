using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.TaskProcessor;

namespace Evolution.TaskProcessorTests {
    [TestClass]
    public partial class MailSenderTaskTests : BaseTest {
        [TestMethod]
        public void GetTaskNameTest() {
            var task = new MailSenderTask(db);
            string expected = TaskName.MailSenderTask,
                   actual = task.GetTaskName();
            Assert.IsTrue(actual == expected, $"Error: GetTaskName() returned {actual} when {expected} was expected. Check that the derived task class overrides the GetTaskName() method");
        }

        [TestMethod]
        public void DoProcessingTest() {
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

                var mailSenderTask = new MailSenderTask(db);
                string[] args = null;
                mailSenderTask.Run(args);

                mailSender = ts.FindScheduledTaskModel(TaskName.MailSenderTask);
                Assert.IsTrue(mailSender != null, "Error: MailSender Scheduled Task not found");
                ts.EnableTask(mailSender, enabled);
            }
        }

        [TestMethod]
        public void SendMessageToUserSmtpTest() {
            // Called by DoProcessingTest()
        }
    }
}
