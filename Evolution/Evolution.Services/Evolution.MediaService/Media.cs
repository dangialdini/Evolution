using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Configuration;
using System.Configuration;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;
using System.Drawing;

namespace Evolution.MediaService {
    public partial class MediaService : CommonService.CommonService {

        #region Media storage

        public MediaModel FindMediaModel(int? id, MediaSize thumbSize = MediaSize.Medium, int fitX = 0, int fitY = 0) {
            MediaModel model = null;
            if (id != null) {
                var media = db.FindMedia(id.Value);
                if (media != null) model = MapToModel(media, thumbSize, fitX, fitY);
            }
            return model;
        }

        public MediaModel MapToModel(Medium media, MediaSize thumbSize, int fitX, int fitY) {
            var model = Mapper.Map<Medium, MediaModel>(media);

            if (media.MediaType != null) {
                model.Lightboxable = media.MediaType.Lightboxable;

                int imageW = 0,
                    imageH = 0;
                model.MediaFile = GetMediaThumb(model, thumbSize, fitX, fitY, ref imageW, ref imageH);

                model.MediaHtml = GetMediaHtml(model, thumbSize, fitX, fitY);
            }

            return model;
        }

        public string GetMediaFileName(MediaModel media, bool bHttp = true) {
            string rc = "";
            if (media != null) {
                if (media.FileName.IsWebUrl()) {
                    rc = media.FileName;
                } else {
                    rc = GetMediaFolder(-1, bHttp);
                    rc += media.FolderName + media.FileName;
                }
                if (!bHttp) rc = rc.Replace("/", "\\");
            }
            return rc;
        }

        public Error InsertOrUpdateMedia(MediaModel model,
                                         CompanyModel company, UserModel user,
                                         MediaFolder mediaFolder,
                                         string sourceFile,         // Fully qualified file name or URL
                                         string lgs,
                                         int itemId,
                                         int subItemId,
                                         FileCopyType copyType) {
            // Filename must be a web URL or a fully qualified filename
            var error = new Error();

            if(string.IsNullOrEmpty(sourceFile)) {
                error.SetError(EvolutionResources.errInvalidFileName, "", sourceFile);

            } else if (!string.IsNullOrEmpty(sourceFile) && !IsValidMediaType(sourceFile)) {
                error.SetError(EvolutionResources.errFileTypeNotValidForUpload, "", sourceFile.FileExtension());

            } else {
                string tempFolderName = GetMediaFolder(mediaFolder,
                                                       company.Id,
                                                       itemId,
                                                       subItemId,
                                                       true,
                                                       true) + "/";
                string tempFileName = sourceFile.IsWebUrl() ? sourceFile : sourceFile.FileName();

                var media = db.FindMedia(tempFolderName, tempFileName);
                if (media == null) {
                    media = new Medium {
                        CreatedDate = DateTimeOffset.Now,
                        CreatedById = user.Id,
                        MediaType = db.FindMediaType(sourceFile),
                        Title = tempFileName,
                        FolderName = tempFolderName,
                        FileName = tempFileName,
                        ImageW = 640,
                        ImageH = 480
                    };
                    if (company != null && company.Id != -1) media.CompanyId = company.Id;
                    db.InsertOrUpdateMedia(media);
                } else {
                    if (!db.IsLockStillValid(typeof(Medium).ToString(), model.Id, lgs)) {
                        error.SetError(EvolutionResources.errRecordChangedByAnotherUser);
                    } else {
                        media.ModifiedDate = DateTimeOffset.Now;
                        media.ModifiedById = user.Id;
                        media.MediaType = db.FindMediaType(sourceFile);
                        media.Title = tempFileName;
                        media.ImageW = 640;
                        media.ImageH = 480;
                        db.InsertOrUpdateMedia(media);
                    }
                }

                if (!error.IsError) {
                    // Now move the file if it exists
                    int actualW = 0,
                        actualH = 0,
                        thumbW = 0,
                        thumbH = 0;

                    if (File.Exists(sourceFile)) {
                        string targetFolder = GetMediaFolder() + media.FolderName.Replace("/", "\\");
                        string targetFile = targetFolder + media.FileName;

                        error = MediaService.CopyOrMoveFile(sourceFile, targetFile, copyType);

                        if (!error.IsError) {
                            // Create thumbnails
                            if (media.MediaType.CreateThumb) {
                                CreateThumbNail(targetFile, GetThumbFileName(targetFile, MediaSize.Small), (int)MediaSize.SmallW, (int)MediaSize.SmallH, true, ref actualW, ref actualH, ref thumbW, ref thumbH);
                                media.ImageW = actualW;
                                media.ImageH = actualH;
                                db.InsertOrUpdateMedia(media);

                                CreateThumbNail(targetFile, GetThumbFileName(targetFile, MediaSize.Medium), (int)MediaSize.MediumW, (int)MediaSize.MediumH, true, ref actualW, ref actualH, ref thumbW, ref thumbH);
                                CreateThumbNail(targetFile, GetThumbFileName(targetFile, MediaSize.Large), (int)MediaSize.LargeW, (int)MediaSize.LargeH, true, ref actualW, ref actualH, ref thumbW, ref thumbH);
                            }
                        }

                    } else if(media.MediaType != null &&
                              media.MediaType.CreateThumb &&
                              media.FileName.IsWebUrl()) {
                        // Media is a url to a thumbable media item
                        error = GetImageSize(sourceFile, ref actualW, ref actualH);
                        if(!error.IsError) {
                            media.ImageW = actualW;
                            media.ImageH = actualH;
                            db.InsertOrUpdateMedia(media);
                        }
                    }

                    Mapper.Map<Medium, MediaModel>(media, model);
                    model.Lightboxable = media.MediaType.Lightboxable;
                    model.MediaHtml = GetMediaHtml(model, MediaSize.Small, (int)MediaSize.SmallW, (int)MediaSize.SmallH);
                }
            }
            return error;
        }

        public Error DeleteMedia(MediaModel media) {
            var error = new Error();
            var product = db.FindProducts(true)
                            .Where(p => p.ProductMedia != null &&
                                        p.ProductMedia.MediaId == media.Id)
                            .FirstOrDefault();
            if (product != null) {
                error.SetError(EvolutionResources.errCantDeleteMediaWhenSetAsProductPrimaryImage, "", media.FileName, product.ItemNumber + " " + product.ItemName);

            } else {
                string fileName = (GetMediaFolder() + media.FolderName + media.FileName).Replace("/", "\\");

                db.DeleteMedia(media.Id);

                DeleteFile(fileName);
                DeleteFile(GetThumbFileName(fileName, MediaSize.Small));
                DeleteFile(GetThumbFileName(fileName, MediaSize.Medium));
                DeleteFile(GetThumbFileName(fileName, MediaSize.Large));
            }
            return error;
        }

        public string LockMedia(MediaModel media) {
            return db.LockRecord(typeof(Medium).ToString(), media.Id);
        }

        #endregion
    }
}
