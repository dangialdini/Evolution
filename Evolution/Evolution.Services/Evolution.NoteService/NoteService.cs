using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.MediaService;
using Evolution.Extensions;
using Evolution.Models.Models;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.NoteService {
    public partial class NoteService : CommonService.CommonService {

        #region Private members

        MediaService.MediaService _mediaServices = null;

        MediaService.MediaService MediaServices {
            get {
                if (_mediaServices == null) _mediaServices = new Evolution.MediaService.MediaService(db);
                return _mediaServices;
            }
        }

        #endregion

        #region Construction

        protected IMapper Mapper = null;

        public NoteService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<Note, NoteModel>();
                cfg.CreateMap<NoteModel, Note>();
                cfg.CreateMap<NoteAttachment, NoteAttachmentModel>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion

        #region Public methods

        public NoteListModel FindNotesListModel(NoteType noteType,
                                                int parentId, int index, int pageNo, int pageSize,
                                                string search,
                                                MediaSize thumbSize,
                                                int fitX = 0, int fitY = 0) {
            var model = new NoteListModel();

            model.GridIndex = index;
            var allItems = db.FindNotes(noteType, parentId)
                             .Where(n => string.IsNullOrEmpty(search) ||
                                         (n.Subject != null && n.Subject.ToLower().Contains(search.ToLower())) ||
                                         (n.Message != null && n.Message.ToLower().Contains(search.ToLower())))
                             .OrderByDescending(n => n.DateCreated);

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = mapToModel(item, thumbSize, fitX, fitY);
                model.Items.Add(newItem);
            }
            return model;
        }

        NoteModel mapToModel(Note item, MediaSize thumbSize, int fitX, int fitY) {
            var newItem = Mapper.Map<Note, NoteModel>(item);

            newItem.CreatedBy = (item.User.FirstName + " " + item.User.LastName).WordCapitalise();

            foreach (var attachment in item.NoteAttachments) {
                var att = mapToModel(attachment, thumbSize, fitX, fitY);
                if (!string.IsNullOrEmpty(newItem.Attachments)) newItem.Attachments += "<br/><br/>";
                newItem.Attachments += "<strong>" + attachment.Medium.Title + "</strong><br/>";
                newItem.Attachments += att.Media.MediaHtml;
            }
            return newItem;
        }

        NoteAttachmentModel mapToModel(NoteAttachment attachment, MediaSize thumbSize, int fitX, int fitY) {
            var attachmentModel = Mapper.Map<NoteAttachment, NoteAttachmentModel>(attachment);
            attachmentModel.Media = MediaServices.MapToModel(attachment.Medium, thumbSize, fitX, fitY);
            return attachmentModel;
        }

        public NoteModel FindNoteModel(int id, CompanyModel company, int parentId, NoteType noteType,
                                       bool bCreateEmptyIfNotfound = true,
                                       MediaSize thumbSize = MediaSize.Medium,
                                       int fitX = 0, int fitY = 0) {
            NoteModel model = null;

            var n = db.FindNote(id);
            if (n == null) {
                if (bCreateEmptyIfNotfound) model = new NoteModel {
                    CompanyId = company.Id,
                    NoteType = noteType,
                    ParentId = parentId
                };
            } else {
                model = mapToModel(n, thumbSize, fitX, fitY);
            }

            return model;
        }

        public List<NoteAttachmentModel> FindNoteAttachmentsModel(NoteModel note, MediaSize thumbSize, int fitX, int fitY) {
            List<NoteAttachmentModel> model = new List<NoteAttachmentModel>();

            var n = db.FindNote(note.Id);
            if (n != null) {
                foreach (var na in n.NoteAttachments) {
                    var newItem = mapToModel(na, thumbSize, fitX, fitY);
                    model.Add(newItem);
                }
            }

            return model;
        }

        public Error InsertOrUpdateNote(NoteModel note, UserModel user, string lockGuid) {
            var error = validateNoteModel(note);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Note).ToString(), note.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "");

                } else {
                    Note temp = null;
                    if (note.Id != 0) temp = db.FindNote(note.Id);
                    bool bNew = false;
                    if (temp == null) {
                        temp = new Note();
                        bNew = true;
                    }

                    Mapper.Map<NoteModel, Note>(note, temp);

                    if (bNew) {
                        temp.CreatedById = user.Id;
                        temp.DateCreated = DateTime.Now;
                    }

                    db.InsertOrUpdateNote(temp);
                    note.Id = temp.Id;

                    // Now save any attachments
                    if (note.Files != null) {
                        foreach (var attachment in note.Files) {
                            if (attachment != null && attachment.ContentLength > 0) {
                                string targetFolder = MediaServices.GetTempFolder();

                                string targetFile = targetFolder + "\\" + attachment.FileName.FileName();
                                try {
                                    attachment.SaveAs(targetFile);
                                    var err1 = AttachMediaItemToNote(note,
                                                                     user,
                                                                     targetFile,
                                                                     targetFile.FileName(),
                                                                     FileCopyType.Move);
                                    if (err1.IsError) {
                                        error.SetError(err1.Message);
                                        break;
                                    }

                                } catch (Exception e2) {
                                    error.SetError(e2);
                                    break;
                                }
                            }
                        }
                    }

                    // Finally, save any URLs
                    for (int i = 0; i < note.UrlReferences.Count(); i++) {
                        if (!string.IsNullOrEmpty(note.UrlReferences[i].Url)) {
                            var err2 = AttachMediaItemToNote(note, user,
                                                             note.UrlReferences[i].Url,
                                                             note.UrlReferences[i].Description,
                                                             FileCopyType.None);
                            if (err2.IsError) {
                                error.SetError(err2.Message);
                                break;
                            }
                        }
                    }
                }
            }
            return error;
        }

        public Error AttachMediaItemToNote(NoteModel note,
                                           UserModel user,
                                           string fileName,
                                           string description,
                                           FileCopyType copyType) {
            var error = new Error();

            var media = new MediaModel();
            var company = new CompanyModel { Id = note.CompanyId };

            string folderName = MediaServices.GetMediaFolder((MediaFolder)note.NoteType,
                                                             company.Id, note.ParentId, note.Id,
                                                             false, true);

            error = MediaServices.InsertOrUpdateMedia(media,
                                                      company,
                                                      user,
                                                      (MediaFolder)note.NoteType,
                                                      fileName,
                                                      "",
                                                      note.ParentId,
                                                      note.Id,
                                                      copyType);
            if (!error.IsError) {
                NoteAttachment attachment = new NoteAttachment {
                    Company = db.FindCompany(note.CompanyId),
                    NoteId = note.ParentId,
                    Note = db.FindNote(note.Id),
                    Medium = db.FindMedia(media.Id)
                };
                db.InsertOrUpdateNoteAttachment(attachment);
            }

            return error;
        }

        public Error AttachNoteToPurchaseOrder(PurchaseOrderHeader poh, UserModel sender,
                                               string subject, string message,
                                               List<string> attachments,
                                               FileCopyType copyType) {
            var error = new Error();

            var note = new NoteModel {
                CompanyId = poh.CompanyId,
                NoteType = NoteType.Purchase,
                ParentId = poh.Id,
                CreatedById = sender.Id,
                Subject = subject,
                Message = message
            };
            InsertOrUpdateNote(note, sender, "");
            error.Id = note.Id;

            foreach (var attachment in attachments) {
                AttachMediaItemToNote(note, sender, attachment, attachment.FileName(), copyType);
            }

            return error;
        }

        public Error AttachNoteToSalesOrder(SalesOrderHeaderModel soh, UserModel sender,
                                            string subject, string message,
                                            List<string> attachments = null,
                                            FileCopyType copyType = FileCopyType.Move) {
            var error = new Error();

            var note = new NoteModel {
                CompanyId = soh.CompanyId,
                NoteType = NoteType.Sale,
                ParentId = soh.Id,
                CreatedById = sender.Id,
                Subject = subject,
                Message = message
            };
            InsertOrUpdateNote(note, sender, "");

            error.Id = note.Id;
            if (!error.IsError && attachments != null) {
                foreach (var attachment in attachments) {
                    AttachMediaItemToNote(note, sender, attachment, attachment.FileName(), copyType);
                }
            }
            return error;
        }

        public void DeleteNote(int id) {
            // Delete attached files
            var note = db.FindNote(id);
            if (note != null) {
                foreach (var attachment in note.NoteAttachments.ToList()) {
                    MediaModel media = MediaServices.MapToModel(attachment.Medium, MediaSize.Small, 0, 0);

                    db.DeleteNoteAttachment(attachment);

                    MediaServices.DeleteMedia(media);
                }

                // Delete the note folder
                var folderName = MediaServices.GetMediaFolder((MediaFolder)note.NoteType, note.CompanyId, note.ParentId, note.Id);
                MediaService.MediaService.DeleteDirectory(folderName);

                // Delete the note
                db.DeleteNote(id);
            }
        }

        public string LockNote(NoteModel model) {
            return db.LockRecord(typeof(Note).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateNoteModel(NoteModel model) {
            int totalSize = 0,
                maxUploadSize = MediaService.MediaService.GetMaxUploadFileSize();

            var error = isValidRequiredString(getFieldValue(model.Subject), 128, "Subject", EvolutionResources.errSubjectRequired);

            if (!error.IsError) {
                // Validate upload file types
                if (model.Files != null) {
                    foreach (var attachment in model.Files) {
                        if (attachment != null && attachment.ContentLength > 0) {
                            // Check file size is not too large
                            totalSize += attachment.ContentLength;
                            if (totalSize > maxUploadSize) {
                                error.SetError(EvolutionResources.errFileSizeToLarge.Replace("%1", attachment.FileName).Replace("%2", maxUploadSize.ToString()), "Subject");
                                break;
                            } else {
                                string extn = Path.GetExtension(attachment.FileName).Replace(".", "");
                                if (!MediaServices.IsValidMediaType(attachment.FileName)) {
                                    // Not a valid upload type
                                    error.SetError(EvolutionResources.errFileTypeNotValidForUpload.Replace("%1", attachment.FileName), "Subject");
                                    break;
                                }
                            }
                        }
                    }
                }

                if(!error.IsError && model.UrlReferences != null) {
                    foreach(var reference in model.UrlReferences) {
                        if (!string.IsNullOrEmpty(reference.Url)) {
                            var url = reference.Url.Trim().TrimStart('/');

                            if (url.IndexOf("http://", StringComparison.InvariantCultureIgnoreCase) != 0 &&
                                url.IndexOf("https://", StringComparison.InvariantCultureIgnoreCase) != 0) {

                                url = "http://" + url;
                                reference.Url = url;
                            }
                        }
                    }
                }
            }

            return error;
        }

        #endregion
    }
}
