using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.NoteServiceTests {
    [TestClass]
    public partial class NoteServiceTests : BaseTest {

        #region Private members

        private NoteService.NoteService _noteService = null;
        private NoteService.NoteService Noteservice { get {
                if (_noteService == null) _noteService = new NoteService.NoteService(db);
                return _noteService;
            }
        }

        #endregion

        #region Tests

        [TestMethod]
        public void FindNotesListModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add notes
            var note1 = CreateCustomerNote(testCompany.Id, testCustomer.Id, testUser, false);
            var note2 = CreateCustomerNote(testCompany.Id, testCustomer.Id, testUser, true);

            // Search for the notes
            var result = NoteService.FindNotesListModel(NoteType.Customer, testCustomer.Id, 0, 1, PageSize, "", MediaSize.Medium, 0, 0)
                                    .Items
                                    .OrderBy(m => m.Id)
                                    .ToList();
            int expectedResult = 2;
            Assert.IsTrue(result.Count == expectedResult, $"Error: {result.Count} records were found when {expectedResult} were expected");
            Assert.IsTrue(result[0].Id == note1.Id, $"Error: Conflict {result[0].Id} was returned when {note1.Id} was expected");
            Assert.IsTrue(result[1].Id == note2.Id, $"Error: Conflict {result[1].Id} was returned when {note2.Id} was expected");
        }

        [TestMethod]
        public void FindNoteModelTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add note
            var note1 = CreateCustomerNote(testCompany.Id, testCustomer.Id, testUser, true);

            var result = NoteService.FindNoteModel(note1.Id, testCompany, testCustomer.Id, NoteType.Customer);
            Assert.IsTrue(result != null, $"Error: 0 records were returned when 1 was expected");
            Assert.IsTrue(result.Id == note1.Id, $"Error: Customer {result.Id} was returned when {note1.Id} was expected");

            var excludes = new List<string>();
            excludes.Add("DateCreatedISO");     // Writing and reading drops a 100th of a second in conversion
            excludes.Add("Attachments");
            AreEqual(note1, result, excludes);

            // Now delete it and try to retrieve it
            NoteService.DeleteNote(note1.Id);

            result = NoteService.FindNoteModel(note1.Id, testCompany, testCustomer.Id, NoteType.Customer, false);
            Assert.IsTrue(result == null, $"Error: 1 record was returned when 0 were expected");
        }

        [TestMethod]
        public void InsertOrUpdateNoteTest() {
            // Tested by FindNoteModelTest
        }

        [TestMethod]
        public void DeleteNoteTest() {
            // Tested by FindNoteModelTest
        }

        [TestMethod]
        public void LockNoteTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add note
            var model = CreateCustomerNote(testCompany.Id, testCustomer.Id, testUser, false);

            // Get the current Lock
            string lockGuid = NoteService.LockNote(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = NoteService.InsertOrUpdateNote(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = NoteService.InsertOrUpdateNote(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = NoteService.LockNote(model);
            error = NoteService.InsertOrUpdateNote(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Delete the record and check that the lock was deleted
            int tempId = model.Id;
            NoteService.DeleteNote(tempId);
            var lockRecord = db.FindLock(typeof(Customer).ToString(), tempId);
            // The following is used because Assert.IsNull tried to evaluate the error message when lockrecord was NULL - its shouldn't!
            if (lockRecord != null) Assert.Fail($"Error: Lock record {lockRecord.Id} was returned when none were expected");
        }

        [TestMethod]
        public void AttachMediaItemToNoteTest() {
            // Tested by LockNoteTest
        }

        [TestMethod]
        public void FindNoteAttachmentsModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Create a note
            var note = new NoteModel {
                CompanyId = testCompany.Id,
                ParentId = testCustomer.Id,
                NoteType = NoteType.Customer,
                CreatedById = testUser.Id,
                Subject = RandomString(),
                Message = LorumIpsum()
            };
            var error = NoteService.InsertOrUpdateNote(note, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Add multiple attachment files to it
            int numAttachments = 10;
            List<string> fileNames = new List<string>();
            for (int i = 0; i < numAttachments; i++) {
                var tempFile = GetTempFile(".doc");

                using (var sw = new StreamWriter(tempFile)) {
                    sw.WriteLine(LorumIpsum());
                }
                error = NoteService.AttachMediaItemToNote(note, testUser, tempFile, tempFile.FileName(), FileCopyType.Move);
                Assert.IsTrue(!error.IsError, error.Message);
            }

            // Get the note attachments, check the names and make sure the files exist
            var attachments = NoteService.FindNoteAttachmentsModel(note, MediaSize.Medium, 0, 0);

            int expected = numAttachments,
                actual = attachments.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} attachment(s) were returned when {expected} were expected");

            for (int i = 0; i < numAttachments; i++) {
                var attachmentFile = MediaServices.GetMediaFileName(attachments[i].Media, false);
                Assert.IsTrue(attachmentFile.CountOf("/") == 0, "Error: The file name returned contained / characters when none were expected");
                Assert.IsTrue(attachmentFile.CountOf("\\") > 0, "Error: The file name returned contained no \\ characters when some were expected");
                Assert.IsTrue(File.Exists(attachmentFile), $"Error: Attachment file '{attachmentFile}' could not be found");

                var mediaFolder = GetAppSetting("MediaFolder", "");
                attachmentFile = attachmentFile.Substring(mediaFolder.Length + 1);

                // 936\\Customers\\0\\378\\tmpC3E3.doc"
                string[] parts = attachmentFile.Split('\\');
                Assert.IsTrue(parts[0] == testCompany.Id.ToString(), "Error: Part 0 is '{parts[0]}' when '{testCompanyId}' was expected");
                Assert.IsTrue(parts[1] == Enumerations.MediaFolder.Customer.ToString(), $"Error: Part 1 is '{parts[1]}' when 'Customers' was expected");
                Assert.IsTrue(parts[2] == testCustomer.Id.ToString(), $"Error: Part 2 is '{parts[2]}' when '{testCustomer.Id}' was expected");
                Assert.IsTrue(parts[3] == attachments[0].NoteId.ToString(), $"Error: Part 3 is '{parts[3]}' when '{attachments[0].NoteId}' was expected");
                Assert.IsTrue(parts[4] == attachmentFile.FileName(), $"Error: Part 4 is '{parts[4]}' when '{attachmentFile.FileName()}' was expected");
            }
        }

        [TestMethod]
        public void AttachNoteToPurchaseOrderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testPurchase = GetTestPurchaseOrderHeader(testCompany, testUser, 10);

            var notes = NoteService.FindNotesListModel(NoteType.Purchase, testPurchase.Id, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            int expected = 0,
                actual = notes.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Create a note
            var note = new NoteModel {
                CompanyId = testCompany.Id,
                ParentId = testPurchase.Id,
                NoteType = NoteType.Purchase,
                CreatedById = testUser.Id,
                Subject = RandomString(),
                Message = LorumIpsum()
            };
            var error = NoteService.InsertOrUpdateNote(note, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            notes = NoteService.FindNotesListModel(NoteType.Purchase, testPurchase.Id, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            expected = 1;
            actual = notes.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Delete it
            NoteService.DeleteNote(notes.Items[0].Id);

            notes = NoteService.FindNotesListModel(NoteType.Purchase, testPurchase.Id, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            expected = 0;
            actual = notes.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
        }

        [TestMethod]
        public void AttachNoteToSalesOrderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);
            var testSale = GetTestSalesOrderHeader(testCompany, testCustomer, testUser, 10);

            var notes = NoteService.FindNotesListModel(NoteType.Sale, testSale.Id, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            int expected = 0,
                actual = notes.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Create a note
            var note = new NoteModel {
                CompanyId = testCompany.Id,
                ParentId = testSale.Id,
                NoteType = NoteType.Sale,
                CreatedById = testUser.Id,
                Subject = RandomString(),
                Message = LorumIpsum()
            };
            var error = NoteService.InsertOrUpdateNote(note, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            notes = NoteService.FindNotesListModel(NoteType.Sale, testSale.Id, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            expected = 1;
            actual = notes.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Delete it
            NoteService.DeleteNote(notes.Items[0].Id);

            notes = NoteService.FindNotesListModel(NoteType.Sale, testSale.Id, 0, 1, 1000, "", MediaSize.Medium, 0, 0);
            expected = 0;
            actual = notes.Items.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
        }

        #endregion
    }
}
