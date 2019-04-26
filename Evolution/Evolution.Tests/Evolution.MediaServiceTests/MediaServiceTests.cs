using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.NoteService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using System.Drawing;

namespace Evolution.MediaServiceTests {
    [TestClass]
    public partial class MediaServiceTests : BaseTest {
        
        [TestMethod]
        public void GetSiteFolderTest() {
            string result = MediaServices.GetMediaFolder(-1, false).Trim();
            Assert.IsTrue(!string.IsNullOrEmpty(result), "Error: An empty string was returned when a folder name was expected");
            Assert.IsTrue(result.CountOf("\\") > 0, "Error: The folder name is expected to contain '\\' characters");
            Assert.IsTrue(result.CountOf("/") == 0, "Error: The folder name must not contain '/' characters");

            result = MediaServices.GetMediaFolder(-1, true).Trim();
            Assert.IsTrue(!string.IsNullOrEmpty(result), "Error: An empty string was returned when a URL was expected");
            Assert.IsTrue(result.CountOf("/") > 0, "Error: The URL is expected to contain '/' characters");
            Assert.IsTrue(result.CountOf("\\") == 0, "Error: The URL must not contain '\\' characters");
        }

        [TestMethod]
        public void GetProductImageFolderTest() {
            string result = MediaServices.GetProductImageFolder(false).Trim();
            Assert.IsTrue(!string.IsNullOrEmpty(result), "Error: An empty string was returned when a folder name was expected");
            Assert.IsTrue(result.CountOf("\\") > 0, "Error: The folder name is expected to contain '\\' characters");
            Assert.IsTrue(result.CountOf("/") == 0, "Error: The folder name must not contain '/' characters");

            result = MediaServices.GetProductImageFolder(true).Trim();
            Assert.IsTrue(!string.IsNullOrEmpty(result), "Error: An empty string was returned when a URL was expected");
            Assert.IsTrue(result.CountOf("/") > 0, "Error: The URL is expected to contain '/' characters");
            Assert.IsTrue(result.CountOf("\\") == 0, "Error: The URL must not contain '\\' characters");
        }
        
        [TestMethod]
        public void GetTempFileTest() {
            // Tested in all tests which create temporary files or note attachments
        }

        [TestMethod]
        public void SepCharTest() {
            // Tested in GetProductImageFolderTest, GetMediaFolderTest
        }
        
        [TestMethod]
        public void GetMediaFolderTest() {
            // Tests an API which gets the location of the media folder, using combinations of parameters

            // Media folder root - http/relative
            string result = MediaServices.GetMediaFolder(-1, false, false);
            string expectedValue = MediaFolder;
            Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (1)");

            result = MediaServices.GetMediaFolder(-1, false, true);
            expectedValue = @"";
            Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (2)");

            result = MediaServices.GetMediaFolder(-1, true, false);
            expectedValue = MediaHttp;
            Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (3)");

            result = MediaServices.GetMediaFolder(-1, true, true);
            expectedValue = @"";
            Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (4)");

            // Media folder root for a company - http/relative
            var user = GetTestUser();
            var company = GetTestCompany(user);

            result = MediaServices.GetMediaFolder(company.Id, false, false);
            expectedValue = MediaFolder + @"\" + company.Id.ToString();
            Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (5)");

            result = MediaServices.GetMediaFolder(company.Id, false, true);
            expectedValue = @"\" + company.Id.ToString();
            Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (6)");

            result = MediaServices.GetMediaFolder(company.Id, true, false);
            expectedValue = MediaHttp + "/" + company.Id.ToString();
            Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (7)");

            result = MediaServices.GetMediaFolder(company.Id, true, true);
            expectedValue = @"/" + company.Id.ToString();
            Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (8)");

            // Check the different media folders for a customer
            var customer = GetTestCustomer(company, user);

            int i = 1;
            string folderName = ((Enumerations.MediaFolder)i).ToString();
            while (folderName.Length > 2) {
                result = MediaServices.GetMediaFolder((MediaFolder)i, company.Id, customer.Id, -1, false, false);
                switch(i) {
                case (int)Enumerations.MediaFolder.Temp:
                    expectedValue = $"{MediaFolder}\\{company.Id}\\{folderName}";
                    break;
                case (int)Enumerations.MediaFolder.User:
                    result = MediaServices.GetMediaFolder((MediaFolder)i, -1, user.Id, -1, false, false);
                    expectedValue = $"{MediaFolder}\\{folderName}\\{user.Id}";
                    break;
                case (int)Enumerations.MediaFolder.Product:
                    expectedValue = $"{MediaFolder}\\{folderName}";
                    break;
                case (int)Enumerations.MediaFolder.ProductCompliance:
                    expectedValue = $"{MediaFolder}\\ProductCompliance\\{customer.Id}\\-1";
                    break;
                default:
                    expectedValue = $"{MediaFolder}\\{company.Id}\\{folderName}\\{customer.Id}";
                    break;
                }
                Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (9)");

                result = MediaServices.GetMediaFolder((MediaFolder)i, company.Id, customer.Id, -1, false, true);
                switch (i) {
                case (int)Enumerations.MediaFolder.Temp:
                    expectedValue = $"\\{company.Id}\\{folderName}";
                    break;
                case (int)Enumerations.MediaFolder.User:
                    result = MediaServices.GetMediaFolder((MediaFolder)i, -1, user.Id, -1, false, true);
                    expectedValue = $"\\{folderName}\\{user.Id}";
                    break;
                case (int)Enumerations.MediaFolder.Product:
                    expectedValue = $"\\{folderName}";
                    break;
                case (int)Enumerations.MediaFolder.ProductCompliance:
                    expectedValue = $"\\ProductCompliance\\{customer.Id}\\-1";
                    break;
                default:
                    expectedValue = $"\\{company.Id}\\{folderName}\\{customer.Id}";
                    break;
                }
                Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (10)");

                result = MediaServices.GetMediaFolder((MediaFolder)i, company.Id, customer.Id, -1, true, false);
                switch (i) {
                case (int)Enumerations.MediaFolder.Temp:
                    expectedValue = $"{MediaHttp}/{company.Id}/{folderName}";
                    break;
                case (int)Enumerations.MediaFolder.User:
                    result = MediaServices.GetMediaFolder((MediaFolder)i, -1, user.Id, -1, true, false);
                    expectedValue = $"{MediaHttp}/{folderName}/{user.Id}";
                    break;
                case (int)Enumerations.MediaFolder.Product:
                    expectedValue = $"{MediaHttp}/{folderName}";
                    break;
                case (int)Enumerations.MediaFolder.ProductCompliance:
                    expectedValue = $"{MediaHttp}/ProductCompliance/{customer.Id}/-1";
                    break;
                default:
                    expectedValue = $"{MediaHttp}/{company.Id}/{folderName}/{customer.Id}";
                    break;
                }
                Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (11)");

                result = MediaServices.GetMediaFolder((MediaFolder)i, company.Id, customer.Id, -1, true, true);
                switch (i) {
                case (int)Enumerations.MediaFolder.Temp:
                    expectedValue = $"/{company.Id}/{folderName}";
                    break;
                case (int)Enumerations.MediaFolder.User:
                    result = MediaServices.GetMediaFolder((MediaFolder)i, -1, user.Id, -1, true, true);
                    expectedValue = $"/{folderName}/{user.Id}";
                    break;
                case (int)Enumerations.MediaFolder.Product:
                    expectedValue = $"/{folderName}";
                    break;
                case (int)Enumerations.MediaFolder.ProductCompliance:
                    expectedValue = $"/ProductCompliance/{customer.Id}/-1";
                    break;
                default:
                    expectedValue = $"/{company.Id}/{folderName}/{customer.Id}";
                    break;
                }
                Assert.IsTrue(result == expectedValue, $"Error: {result} was returned when {expectedValue} was expected (12)");

                i++;
                folderName = ((Enumerations.MediaFolder)i).ToString();
            }
        }
        
        [TestMethod]
        public void FindCompanyLogosTest() {
            var logoList = MediaServices.FindCompanyLogos();
            Assert.IsTrue(logoList.Count() > 0, "Error: No compny logos were returned when some were expected");
        }
        
        [TestMethod]
        public void IsValidMediaTypeTest() {
            // Check valid file types
            checkValidMediaType("filename.png");
            checkValidMediaType("filename.jpg");
            checkValidMediaType("filename.jpeg");
            checkValidMediaType(".gif");
            checkValidMediaType(".xls");
            checkValidMediaType(".xlsx");
            checkValidMediaType(".doc");
            checkValidMediaType(".docx");
            checkValidMediaType(".ppt");
            checkValidMediaType(".pptx");
            checkValidMediaType(".pdf");

            // Check invalid file types
            checkInvalidMediaType("filename.exe");
            checkInvalidMediaType("filename.bin");
            checkInvalidMediaType("filename.htm");
            checkInvalidMediaType("filename.html");
            checkInvalidMediaType("filename.dll");
            checkInvalidMediaType("filename.js");
            checkInvalidMediaType("filename.css");
        }
        
        void checkValidMediaType(string fileName) {
            Assert.IsTrue(MediaServices.IsValidMediaType(fileName), $"Error: {fileName} was reported as an invalid type when is should be valid");
        }
        
        void checkInvalidMediaType(string fileName) {
            Assert.IsFalse(MediaServices.IsValidMediaType(fileName), $"Error: {fileName} was reported as a valid type when is should be invalid");
        }
        
        [TestMethod]
        public void GetMediaTypeTest() {
            // Tested in IsValidMediaTypeTest
        }
        
        [TestMethod]
        public void GetMaxUploadFileSizeTest() {
            int maxFileSize = MediaService.MediaService.GetMaxUploadFileSize();
            Assert.IsTrue(maxFileSize > 0, $"Error: {maxFileSize} was returned when a non-zero value was expected - check the app.Config or web.config");
        }
        
        [TestMethod]
        public void InsertOrUpdateMediaTest() {
            // Tested in FindMediaModeltest below
        }
        /*
        [TestMethod]
        public void AddNoteAttachmentTest() {
            // Tested by NoteTests.FindNoteModelTest - it creates a note with attachment
        }
        */
        [TestMethod]
        public void FindMediaModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a media item
            var media = new MediaModel();
            var error = createMedia(testCompany, testUser, "", media);
            Assert.IsTrue(error.IsError, "Error: Success was returned when a blank sourceFile should have caused an error");

            // Now supply a valid source file
            string sourceFile = MediaService.MediaService.GetTempFile(".txt");
            File.WriteAllText(sourceFile, LorumIpsum());

            error = createMedia(testCompany, testUser, sourceFile, media);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now find it
            var checkMedia = MediaServices.FindMediaModel(media.Id);
            Assert.IsTrue(checkMedia != null, "Error: A NULL value was returned when a non-NULL value was expected");

            var excludes = new List<string>();
            excludes.Add("MediaHtml");              // MediaHtml is not known up-front
            AreEqual(media, checkMedia, excludes);
        }
        
        [TestMethod]
        public void GetDataTransferFolderTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            string folderName = MediaServices.GetDataTransferFolder(testCompany.Id);
            Assert.IsTrue(!string.IsNullOrEmpty(folderName), "Error: A NULL/empty value was returned when a text string was expected");
            Assert.IsTrue(folderName.CountOf("\\") > 0, $"Error: {folderName} was returned when a string containing \\ characters was expected");
            Assert.IsTrue(folderName.CountOf("/") == 0, $"Error: {folderName} was returned when a string containing no / characters was expected");

            string[] parts = folderName.Split('\\');
            int idx = parts.Length - 1;
            Assert.IsTrue(parts[idx] == testCompany.Id.ToString(), $"Error: A folder name ending in '{parts[idx]}' was returned when '{testCompany.Id}' was expected");
        }
        
        [TestMethod]
        public void CreateCompanyFoldersTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Creating the test company above creates the company folders,
            // so check that they have been created
            string companyfolder = MediaServices.GetMediaFolder(testCompany.Id, false);

            string testFolder = companyfolder;
            Assert.IsTrue(Directory.Exists(testFolder), $"Error: Folder {testFolder} could not be found");

            testFolder = companyfolder + "\\" + Enumerations.MediaFolder.Customer;
            Assert.IsTrue(Directory.Exists(testFolder), $"Error: Folder {testFolder} could not be found");

            testFolder = companyfolder + "\\" + Enumerations.MediaFolder.Purchase;
            Assert.IsTrue(Directory.Exists(testFolder), $"Error: Folder {testFolder} could not be found");

            testFolder = companyfolder + "\\" + Enumerations.MediaFolder.Sale;
            Assert.IsTrue(Directory.Exists(testFolder), $"Error: Folder {testFolder} could not be found");

            // Create the folders for the company data transfers
            string dataTransferFolder = MediaServices.GetDataTransferFolder(testCompany.Id);

            testFolder = dataTransferFolder;
            Assert.IsTrue(Directory.Exists(testFolder), $"Error: Folder {testFolder} could not be found");

            testFolder = dataTransferFolder + "\\UnpackSlips";
            Assert.IsTrue(Directory.Exists(testFolder), $"Error: Folder {testFolder} could not be found");

            testFolder = dataTransferFolder + "\\UnpackSlips\\Errors";
            Assert.IsTrue(Directory.Exists(testFolder), $"Error: Folder {testFolder} could not be found");
        }
        
        [TestMethod]
        public void GetMediaFileNameTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Create a note
            NoteService.NoteService noteService = new NoteService.NoteService(db);

            var note = new NoteModel {
                CompanyId = testCompany.Id,
                ParentId = testCustomer.Id,
                NoteType = NoteType.Customer,
                CreatedById = testUser.Id,
                Subject = RandomString(),
                Message = LorumIpsum()
            };
            var error = noteService.InsertOrUpdateNote(note, testUser, "");
            Assert.IsTrue(!error.IsError, error.Message);

            // Add a reference attachment to it
            var tempFile1 = GetTempFile(".doc");
            using (var sw = new StreamWriter(tempFile1)) {
                sw.WriteLine(LorumIpsum());
            }
            error = NoteService.AttachMediaItemToNote(note, testUser, tempFile1, tempFile1.FileName(), FileCopyType.Move);
            Assert.IsTrue(!error.IsError, error.Message);

            // Add a second attachment - an actual file
            var tempFile2 = GetTempFile(".doc");
            using (var sw = new StreamWriter(tempFile2)) {
                sw.WriteLine(LorumIpsum());
            }
            error = NoteService.AttachMediaItemToNote(note, testUser, tempFile2, tempFile2.FileName(), FileCopyType.Move);
            Assert.IsTrue(!error.IsError, error.Message);

            // Get the note attachments and check the names
            var attachments = NoteService.FindNoteAttachmentsModel(note, MediaSize.Medium, 0, 0);

            int expected = 2,
                actual = attachments.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} attachment(s) were returned when {expected} were expected");

            var attachmentFile = MediaServices.GetMediaFileName(attachments[0].Media, false);
            Assert.IsTrue(attachmentFile.CountOf("/") == 0, "Error: The file name returned contained / characters when none were expected");
            Assert.IsTrue(attachmentFile.CountOf("\\") > 0, "Error: The file name returned contained no \\ characters when some were expected");

            var mediaFolder = GetAppSetting("MediaFolder", "");
            attachmentFile = attachmentFile.Substring(mediaFolder.Length + 1);

            // 936\\Customer\\0\\378\\tmpC3E3.doc"
            string[] parts = attachmentFile.Split('\\');
            Assert.IsTrue(parts[0] == testCompany.Id.ToString(), "Error: Part 0 is '{parts[0]}' when '{testCompanyId}' was expected");
            Assert.IsTrue(parts[1] == Enumerations.MediaFolder.Customer.ToString(), $"Error: Part 1 is '{parts[1]}' when 'Customers' was expected");
            Assert.IsTrue(parts[2] == testCustomer.Id.ToString(), $"Error: Part 2 is '{parts[2]}' when '{testCustomer.Id}' was expected");
            Assert.IsTrue(parts[3] == attachments[0].NoteId.ToString(), $"Error: Part 3 is '{parts[3]}' when '{attachments[0].NoteId}' was expected");
            Assert.IsTrue(parts[4] == attachmentFile.FileName(), $"Error: Part 4 is '{parts[4]}' when '{attachmentFile.FileName()}' was expected");

            // Check the second note attachment
            attachmentFile = MediaServices.GetMediaFileName(attachments[1].Media, false);
            Assert.IsTrue(attachmentFile.CountOf("/") == 0, "Error: The file name returned contained / characters when none were expected");
            Assert.IsTrue(attachmentFile.CountOf("\\") > 0, "Error: The file name returned contained no \\ characters when some were expected");
            Assert.IsTrue(File.Exists(attachmentFile), $"Error: Attachment file '{attachmentFile}' could not be found");

            attachmentFile = attachmentFile.Substring(mediaFolder.Length + 1);

            // 936\\Customer\\0\\378\\tmpC3E3.doc"
            parts = attachmentFile.Split('\\');
            Assert.IsTrue(parts[0] == testCompany.Id.ToString(), "Error: Part 0 is '{parts[0]}' when '{testCompanyId}' was expected");
            Assert.IsTrue(parts[1] == Enumerations.MediaFolder.Customer.ToString(), $"Error: Part 1 is '{parts[1]}' when 'Customers' was expected");
            Assert.IsTrue(parts[2] == testCustomer.Id.ToString(), $"Error: Part 2 is '{parts[2]}' when '{testCustomer.Id}' was expected");
            Assert.IsTrue(parts[3] == attachments[0].NoteId.ToString(), $"Error: Part 3 is '{parts[3]}' when '{attachments[0].NoteId}' was expected");
            Assert.IsTrue(parts[4] == attachmentFile.FileName(), $"Error: Part 4 is '{parts[4]}' when '{attachmentFile.FileName()}' was expected");
        }
        
        [TestMethod]
        public void GetImageSizeTest() {
            string[] data = { "FileTypeIcons.png", "800", "816",
                              "SydneyHarbour.jpg", "300", "168" };

            int row = 0;
            for(int i = 0; i < data.Length; i+= 3) { 
                string sourceFile = GetAppSetting("SourceFolder", "") + @"\Evolution.Tests\Evolution.MediaServiceTests\TestData\" + data[i];

                int actualW = 0,
                    actualH = 0;
                var error = MediaServices.GetImageSize(sourceFile, ref actualW, ref actualH);

                Assert.IsTrue(actualW == Convert.ToInt32(data[i + 1]), $"Error: Width {actualW} was returned when {data[i + 1]} was expected (test row {row})");
                Assert.IsTrue(actualH == Convert.ToInt32(data[i + 2]), $"Error: Width {actualW} was returned when {data[i + 2]} was expected (test row {row})");
            }
        }
        
        [TestMethod]
        public void CreateThumbNailTest() {
            string[] fileNames = { "FileTypeIcons.png",
                                   "SydneyHarbour.jpg" };

            foreach (var fileName in fileNames) {
                string sourceFile = GetAppSetting("SourceFolder", "") + @"\Evolution.Tests\Evolution.MediaServiceTests\TestData\" + fileName;

                string targetFile = MediaServices.GetThumbFileName(sourceFile, MediaSize.Medium);
                db.LogTestFile(targetFile);

                int actualW = 0,
                    actualH = 0,
                    thumbW = 0,
                    thumbH = 0;
                var error = MediaServices.CreateThumbNail(sourceFile, targetFile, 160, 120, true, ref actualW, ref actualH, ref thumbW, ref thumbH);
                Assert.IsTrue(!error.IsError, error.Message);
                Assert.IsTrue(File.Exists(targetFile), $"Error: {targetFile} could not be found");
            }
        }
        
        [TestMethod]
        public void CalculateFitSizeTest() {
            //              Original  Required  Expected
            int[] sizes = { 640, 480, 320,  240, 320, 240,      // Fit area smaller than the image
                            640, 480, 1024, 768, 640, 480};     // Fit area larger than the image (uses original image size)
            int row = 0;

            for(int i = 0; i < sizes.Length; i += 6) {
                Size size = MediaServices.CalculateFitSize(sizes[i], sizes[i + 1], sizes[i + 2], sizes[i + 3], true);
                Assert.IsTrue(size.Width == sizes[i + 4], $"Error: Width {size.Width} was returned when {sizes[i + 4]} was expected (test row {row})");
                Assert.IsTrue(size.Height == sizes[i + 5], $"Error: Width {size.Height} was returned when {sizes[i + 5]} was expected (test row {row})");
                row++;
            }
        }
        
        [TestMethod]
        public void GetThumbFileNameTest() {
            string[] data1 = { "FileTypeIcons.png", "FileTypeIcons_ThumbSmall.png",
                               "FileTypeIcons.png", "FileTypeIcons_ThumbMedium.png",
                               "FileTypeIcons.png", "FileTypeIcons_ThumbLarge.png"};
            MediaSize[] data2 = { MediaSize.Small,
                                  MediaSize.Medium,
                                  MediaSize.Large };

            int row = 0;
            for(int i = 0; i < data1.Length; i += 2) {
                string result = MediaServices.GetThumbFileName(data1[i], data2[i / 2]);
                Assert.IsTrue(result == data1[i + 1], $"Error: {result} was returned when {data1[i + 1]} was expected (test row {row})");
                row++;
            }
        }

        [TestMethod]
        public void GetMediaThumbTest() {
            // Tested in GetMediaHtmlTest() and all tests which cause MapToModel as this calls GetMediaThumb
        }
        
        [TestMethod]
        public void GetMediaSizeStringTest() {
            string result = MediaServices.GetMediaSizeString(0, 0),
                   expected = "";
            Assert.IsTrue(result == expected, $"Error: '{result}' was returned when '{result}' was expected");

            result = MediaServices.GetMediaSizeString(640, 480);
            expected = " width=\"640\" height=\"480\"";
            Assert.IsTrue(result == expected, $"Error: '{result}' was returned when '{result}' was expected");
        }
        
        [TestMethod]
        public void GetValidMediaTypesTest() {
            // Tested in IsValidMediaUploadTypeTest below
        }
        
        [TestMethod]
        public void IsValidMediaUploadTypeTest() {
            var validTypes = MediaServices.GetValidMediaTypes();
            Assert.IsTrue(validTypes != null, "Error: A NULL value was returned when a string was expected");

            string[] typeList = validTypes.ToLower().Replace(" ", "").Split(',');
            Assert.IsTrue(typeList != null, "Error: A NULL value was returned when a list was expected");

            foreach(var fileType in typeList) {
                Assert.IsTrue(MediaServices.IsValidMediaUploadType("." + fileType), $"Error: {fileType} was returned as an invalid file type when GetValidMediaTypes returned it as a valid type");
            }
        }

        [TestMethod]
        public void GetValidOrderImportTypesTest() {
            // This test will fail if the valid types are changed, therefore, it serves
            // mainly as a regression test
            string validTypes = MediaServices.GetValidOrderImportTypes();
            Assert.IsTrue(validTypes != null, "Error: A NULL value was returned when a string was expected");

            string[] typeList = validTypes.ToLower().Replace(" ", "").Split(',');
            Assert.IsTrue(typeList != null, "Error: A NULL value was returned when a list was expected");
            Assert.IsTrue(typeList.Length == 2, $"Error: {typeList.Length} types were returned when 2 were expected");
            if (!((typeList[0] == "csv" && typeList[1] == "txt") ||
               (typeList[0] == "txt" && typeList[1] == "csv"))) {
                Assert.Fail($"Error: Types {typeList[0]} and {typeList[1]} were returned when csv and txt were expected");
            }
        }

        [TestMethod]
        public void GetValidImageTypesTest() {
            // This test will may if the valid types are changed, therefore, it serves
            // mainly as a regression test
            string validTypes = MediaServices.GetValidImageTypes();
            Assert.IsTrue(validTypes != null, "Error: A NULL value was returned when a string was expected");

            string[] typeList = validTypes.ToLower().Replace(" ", "").Split(',');
            Assert.IsTrue(typeList != null, "Error: A NULL value was returned when a list was expected");
            Assert.IsTrue(typeList.Length == 4, $"Error: {typeList.Length} types were returned when 4 were expected");
            string expected = "gif, jpeg, jpg, png";
            Assert.IsTrue(validTypes == expected, $"Error: '{validTypes}' was returned when '{expected}' was expected");
        }

        [TestMethod]
        public void IsValidOrderImportTypeTest() {
            var result = MediaServices.IsValidOrderImportType(".TXT");
            Assert.IsTrue(result == true, "Error: TXT was returned as invalid when it should be valid");
            result = MediaServices.IsValidOrderImportType(".txt");
            Assert.IsTrue(result == true, "Error: txt was returned as invalid when it should be valid");

            result = MediaServices.IsValidOrderImportType(".CSV");
            Assert.IsTrue(result == true, "Error: CSV was returned as invalid when it should be valid");
            result = MediaServices.IsValidOrderImportType(".csv");
            Assert.IsTrue(result == true, "Error: csv was returned as invalid when it should be valid");

            result = MediaServices.IsValidOrderImportType(".DAT");
            Assert.IsTrue(result == false, "Error: DAT was returned as valid when it should be invalid");
            result = MediaServices.IsValidOrderImportType(".dat");
            Assert.IsTrue(result == false, "Error: dat was returned as valid when it should be invalid");
        }
        
        [TestMethod]
        public void MapToModelTest() {
            var media = new Medium {
                Id = RandomInt(),
                CompanyId = RandomInt(),
                CreatedDate = DateTimeOffset.Now,
                CreatedById = RandomInt(),
                ModifiedDate = DateTimeOffset.Now,
                ModifiedById = RandomInt(),
                MediaType = db.FindMediaType("jpg"),
                Title = RandomString(),
                FolderName = RandomString(),
                FileName = RandomString(),
                ImageW = RandomInt(),
                ImageH = RandomInt()
            };
            int fitX = RandomInt(),
                fitY = RandomInt();
            var mappedResult = MediaServices.MapToModel(media, MediaSize.Medium, fitX, fitY);

            Assert.IsTrue(mappedResult.Id == media.Id, $"Error: {mappedResult.Id} was returned for Id when {media.Id} was expected");
            Assert.IsTrue(mappedResult.CompanyId == media.CompanyId, $"Error: {mappedResult.CompanyId} was returned for Id when {media.CompanyId} was expected");
            Assert.IsTrue(mappedResult.MediaTypeId == media.MediaTypeId, $"Error: {mappedResult.MediaTypeId} was returned for Id when {media.MediaType.Id} was expected");
            Assert.IsTrue(mappedResult.Title == media.Title, $"Error: {mappedResult.Title} was returned for Id when {media.Title} was expected");
            Assert.IsTrue(mappedResult.FolderName == media.FolderName, $"Error: {mappedResult.FolderName} was returned for Id when {media.FolderName} was expected");
            Assert.IsTrue(mappedResult.FileName == media.FileName, $"Error: {mappedResult.FileName} was returned for Id when {media.FileName} was expected");
        }

        [TestMethod]
        public void CopyOrMoveFileTest() {
            // Test copying
            string sourceFile = MediaService.MediaService.GetTempFile();
            LogTestFile(sourceFile);
            string targetFile = MediaService.MediaService.GetTempFile();
            LogTestFile(targetFile);

            File.WriteAllText(sourceFile, LorumIpsum());
            Assert.IsTrue(File.Exists(sourceFile), $"Error: Source file {sourceFile} could not be found");

            Assert.IsTrue(!File.Exists(targetFile), $"Error: Target file {targetFile} already exists before a COPY when it should not");
            var error = MediaService.MediaService.CopyOrMoveFile(sourceFile, targetFile, FileCopyType.Copy);
            Assert.IsTrue(!error.IsError, error.Message);
            Assert.IsTrue(File.Exists(sourceFile), $"Error: Source file {sourceFile} has been removed when it should not have");
            Assert.IsTrue(File.Exists(targetFile), $"Error: Target file {targetFile} could not be found after a COPY");

            // Test moving
            sourceFile = MediaService.MediaService.GetTempFile();
            LogTestFile(sourceFile);
            targetFile = MediaService.MediaService.GetTempFile();
            LogTestFile(targetFile);

            File.WriteAllText(sourceFile, LorumIpsum());
            Assert.IsTrue(File.Exists(sourceFile), $"Error: Source file {sourceFile} could not be found");

            Assert.IsTrue(!File.Exists(targetFile), $"Error: Target file {targetFile} already exists before a MOVE when it should not");
            error = MediaService.MediaService.CopyOrMoveFile(sourceFile, targetFile, FileCopyType.Move);
            Assert.IsTrue(!error.IsError, error.Message);
            Assert.IsTrue(!File.Exists(sourceFile), $"Error: Source file {sourceFile} still exists after a MOVE when it should have been deleted");
            Assert.IsTrue(File.Exists(targetFile), $"Error: Target file {targetFile} could not be found after a MOVE");
        }

        [TestMethod]
        public void DeleteFileTest() {
            // Tested in DeleteDirectoryTest()
        }

        [TestMethod]
        public void CreateDirectoryTest() {
            // Tested in DeleteDirectoryTest()
        }

        [TestMethod]
        public void DeleteDirectoryTest() {
            var tempFolder = MediaServices.GetTempFolder() + "\\" + RandomString();
            var error = MediaService.MediaService.CreateDirectory(tempFolder);
            Assert.IsTrue(!error.IsError, error.Message);

            var tempFile = tempFolder + RandomString() + ".txt";
            File.WriteAllText(tempFile, LorumIpsum());
            Assert.IsTrue(File.Exists(tempFile), $"Error: File {tempFile} could not be found");

            error = MediaService.MediaService.DeleteFile(tempFile);
            Assert.IsTrue(!error.IsError, error.Message);

            error = MediaService.MediaService.DeleteDirectory(tempFolder);
            Assert.IsTrue(!error.IsError, error.Message);

            Assert.IsTrue(!File.Exists(tempFile), $"Error: File {tempFile} still exists - DeleteDirectory() has failed");
        }

        [TestMethod]
        public void GetTempFolderTest() {
            // Tested in DeleteDirectoryTest()
        }

        [TestMethod]
        public void DeleteMediaTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            string sourceFile = MediaService.MediaService.GetTempFile(".txt");
            LogTestFile(sourceFile);
            File.WriteAllText(sourceFile, LorumIpsum());

            var model = new MediaModel();
            var error = createMedia(testCompany, testUser, sourceFile, model);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Check that the media exists
            var mediaFile = MediaServices.GetMediaFileName(model, false);
            Assert.IsTrue(File.Exists(mediaFile), $"Error: File '{mediaFile}' could not be found");

            // Now delete the media
            error = MediaServices.DeleteMedia(model);

            // Check that the media has been deleted
            var check = db.FindMedias()
                          .Where(m => m.Id == model.Id)
                          .FirstOrDefault();
            Assert.IsTrue(check == null, "Error: Media record was found when it should have been deleted");
            Assert.IsTrue(!File.Exists(mediaFile), $"Error: File '{mediaFile}' exists when it should have been deleted");
        }

        [TestMethod]
        public void LockMediaTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            string sourceFile = MediaService.MediaService.GetTempFile(".txt");
            LogTestFile(sourceFile);
            File.WriteAllText(sourceFile, LorumIpsum());

            var model = new MediaModel();
            var error = createMedia(testCompany, testUser, sourceFile, model);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = MediaServices.LockMedia(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = MediaServices.InsertOrUpdateMedia(model, testCompany, otherUser, Enumerations.MediaFolder.User, sourceFile, lockGuid, -1, -1, FileCopyType.Copy);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = MediaServices.InsertOrUpdateMedia(model, testCompany, testUser, Enumerations.MediaFolder.User, sourceFile, lockGuid, -1, -1, FileCopyType.Copy);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = MediaServices.LockMedia(model);
            error = MediaServices.InsertOrUpdateMedia(model, testCompany, testUser, Enumerations.MediaFolder.User, sourceFile, lockGuid, -1, -1, FileCopyType.Copy);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void GetMediaHtmlTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            string testImage = MediaServices.GetSiteFolder() + @"\\Content\\EvolutionLogo.png";
            string sourceFile = MediaServices.GetTempFolder() + testImage.FileName();
            LogTestFile(sourceFile);
            var error = MediaService.MediaService.CopyOrMoveFile(testImage, sourceFile, Enumerations.FileCopyType.Copy);
            Assert.IsTrue(!error.IsError, error.Message);

            var model = new MediaModel();
            error = createMedia(testCompany, testUser, sourceFile, model);
            Assert.IsTrue(!error.IsError, error.Message);

            string html = MediaServices.GetMediaHtml(model, MediaSize.Medium, (int)MediaSize.MediumW, (int)MediaSize.MediumH);
            Assert.IsTrue(!string.IsNullOrEmpty(html), "Error: An empty string was returned");

            // TBD: Need to add further checks to make sure the html is valid
        }

        private Error createMedia(CompanyModel testCompany, UserModel testUser, string sourceFile, 
                                  MediaModel media) {
            media.CompanyId = testCompany.Id;

            var mediaType = db.FindMediaType(sourceFile);
            if(mediaType == null) mediaType = db.FindMediaType((int)Enumerations.MediaType.UNKNOWN);
            media.MediaTypeId = mediaType.Id;
            media.MediaFile = "/Content/MediaThumbs/" + mediaType.ThumbMedium;

            var error = MediaServices.InsertOrUpdateMedia(media,
                                                          testCompany,
                                                          testUser,
                                                          Enumerations.MediaFolder.User,
                                                          sourceFile,
                                                          "",
                                                          testUser.Id,
                                                          -1,
                                                          FileCopyType.Move);
            return error;
        }
    }
}

