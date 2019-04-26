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
using System.Net;

namespace Evolution.MediaService {
    public partial class MediaService : CommonService.CommonService {

        public string GetMediaHtml(MediaModel media, MediaSize thumbSize, int fitX, int fitY) {
            string htmlText = "";
            int     imageW = (media.ImageW != null ? media.ImageW.Value : 0),
                    imageH = (media.ImageH != null ? media.ImageH.Value : 0);
            string  sizeText = "",
                    titleText = media.Title,
                    mediaFileName = GetMediaFileName(media, true),
                    mediaThumb = GetMediaThumb(media, thumbSize, fitX, fitY, ref imageW, ref imageH);

            if (fitX > 0 && fitY > 0) {
                sizeText = GetMediaSizeString(imageW, imageH);

                var mediaType = db.FindMediaType(media.MediaTypeId);
                if (mediaType.CreateThumb) {
                    // This item type does have a thumb
                    if (mediaType.Lightboxable) {
                        htmlText = "<a class=\"fancybox\" href=\"" + mediaFileName + "\" data-fancybox-group=\"gallery\" title=\"" + titleText + "\">";
                        htmlText += "<img src=\"";
                        if (media.FileName.IsWebUrl()) {
                            htmlText += media.FileName;
                        } else {
                            htmlText += mediaThumb;
                        }
                        htmlText += "\"" + sizeText + "/></a>";

                    } else {
                        htmlText = "<a href=\"";
                        htmlText += mediaFileName;
                        htmlText += "\" target=\"MediaViewer\">";
                        htmlText += "<img src=\"" + mediaThumb;
                        htmlText += "\" border=\"0\"" + sizeText + "/></a>";
                    }

                } else {
                    // This item type doesn't have a thumb so use a system thumb image
                    // eg MP3, MP4
                    if (mediaType.Lightboxable) {
                        // This item type doesn't have a thumb, but it is lightboxable
                        switch ((Enumerations.MediaType)media.MediaTypeId) {
                        case Enumerations.MediaType.YOUTUBE:
                            htmlText = "<a class=\"fancybox-media\" href=\"" + mediaFileName + "\">";
                            htmlText += "<img src=\"" + mediaThumb;

                            htmlText += "\" style=\"width:" + fitX.ToString() + ";height:auto\" border=\"0\"/></a>";
                            break;

                        case Enumerations.MediaType.MPG3:
                            htmlText = "<a class=\"fancybox-mp3\" rel=\"audio\" title=\"" + titleText + "\" href=\"#data_" + media.Id.ToString() + "_X\">";
                            htmlText += "<img src=\"" + mediaThumb + "\"";
                            htmlText += " border=\"0\" alt=\"" + titleText + "\"/></a>";
                            htmlText += "<div style=\"display:none\">";
                            htmlText += "    <div id=\"data_" + media.Id.ToString() + "_X\">";
                            htmlText += "        <audio controls>";
                            htmlText += "          <source src=\"" + mediaFileName + "\" type=\"audio/mpeg\">";
                            htmlText += "        <p>Your browser does not support the audio element.</p>";
                            htmlText += "        </audio>";
                            htmlText += "    </div>";
                            htmlText += "</div>";
                            break;

                        case Enumerations.MediaType.MPG4:
                            htmlText = "<a class=\"fancybox-mp4\" rel=\"video\" title=\"" + titleText + "\" href=\"#data_" + media.Id.ToString() + "_X\">";
                            htmlText += "<img src=\"" + mediaThumb + "\"";
                            htmlText += " border=\"0\" alt=\"" + titleText + "\"/></a>";
                            htmlText += "<div style=\"display:none\">";
                            htmlText += "    <div id=\"data_" + media.Id.ToString() + "_X\">";
                            htmlText += "        <video controls>";
                            htmlText += "            <source src=\"" + mediaFileName + "\" type=\"video/mp4\" codecs=\"avc1.42E01E, mp4a.40.2\"/>";
                            htmlText += "            <p>Your browser does not support the video element.</p>";
                            htmlText += "        </video>";
                            htmlText += "    </div>";
                            htmlText += "</div>";
                            break;

                        default:
                            htmlText = "<a class=\"fancybox\" href=\"" + mediaFileName + "\" data-fancybox-group=\"gallery\" title=\"" + titleText + "\">";
                            htmlText += "<img src=\"" + mediaThumb + "\"";
                            htmlText += sizeText;
                            htmlText += " border=\"0\"/></a>";
                            break;
                        }

                    } else {
                        // This item type doesn't have a thumb and it isn't lightboxable
                        htmlText = "<a href=\"";
                        htmlText += mediaFileName;
                        htmlText += "\" target=\"MediaViewer\">";
                        htmlText += "<img src=\"" + mediaThumb + "\" border=\"0\"/></a>";
                    }
                }
            }
            return htmlText;
        }

        public Error GetImageSize(string sourceFile, ref int ActualWidth, ref int ActualHeight) {
            var error = new Error();
            System.Drawing.Image img = null;

            try {
                if (sourceFile.ToLower().IndexOf("http://") == 0 ||
                    sourceFile.ToLower().IndexOf("https://") == 0) {
                    WebRequest req = WebRequest.Create(sourceFile);
                    req.Credentials = new NetworkCredential(GetConfigurationSetting("LdapUsername", ""),
                                                            GetConfigurationSetting("LdapPassword", ""));
                    WebResponse response = req.GetResponse();
                    if (response != null) {
                        Stream stream = response.GetResponseStream();

                        img = System.Drawing.Image.FromStream(stream);
                        stream.Close();
                    }
                } else {
                    img = System.Drawing.Image.FromFile(sourceFile);
                }
            } catch (Exception e1) {
                error.SetError(e1);
            }

            if (!error.IsError) {
                ActualWidth = img.Size.Width;
                ActualHeight = img.Size.Height;
            }

            return error;
        }

        public Error CreateThumbNail(string sourceFile, string targetFile, int fitWidth, int fitHeight, bool bUseOriginalIfSmaller,
                                     ref int ActualWidth, ref int ActualHeight, ref int ThumbWidth, ref int ThumbHeight) {
            // Returns: 0=Success, <>0 = Error
            // Values are returned in the ref parameters:
            //      ActualWidth/Height = size of orginal image
            //      ThumbWidth/Height  = size of thumbnaail

            var error = new Error();
            System.Drawing.Image img = null;

            db.LogTestFile(targetFile);     // So tests clean it up

            ThumbWidth = ThumbHeight = 0;

            if (fitWidth < 1 || fitHeight < 1) {
                // No thumbnail to be created
                try {
                    File.Delete(targetFile);
                } catch { }

            } else {
                // A thumbnail size has been specified

                try {
                    if (sourceFile.ToLower().IndexOf("http://") == 0 ||
                        sourceFile.ToLower().IndexOf("https://") == 0) {

                        WebRequest req = WebRequest.Create(sourceFile);
                        WebResponse response = req.GetResponse();
                        if (response != null) {
                            Stream stream = response.GetResponseStream();

                            img = System.Drawing.Image.FromStream(stream);
                            stream.Close();
                        }
                    } else {
                        img = System.Drawing.Image.FromFile(sourceFile);
                    }
                } catch (Exception e1) {
                    error.SetError(e1);
                }

                if (!error.IsError) {
                    // Prevent using image's internal thumbnail
                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);

                    ActualWidth = img.Size.Width;
                    ActualHeight = img.Size.Height;

                    if (img.Size.Width <= fitWidth && img.Size.Height <= fitHeight && bUseOriginalIfSmaller) {
                        // Image is smaller than the required size, so copy it as is
                        ThumbWidth = img.Size.Width;
                        ThumbHeight = img.Size.Height;
                        img.Dispose();
                        try {
                            if (!string.IsNullOrEmpty(targetFile)) File.Copy(sourceFile, targetFile, true);
                        } catch { }

                    } else {
                        byte[] buffer = null;
                        try {
                            Size fitSize = CalculateFitSize(img.Size.Width, img.Size.Height, fitWidth, fitHeight);

                            ThumbWidth = fitSize.Width;
                            ThumbHeight = fitSize.Height;

                            System.Drawing.Image newImg = img.GetThumbnailImage(fitSize.Width, fitSize.Height, null, new System.IntPtr());
                            try {
                                if (!string.IsNullOrEmpty(targetFile)) {
                                    using (MemoryStream ms = new MemoryStream()) {
                                        if (sourceFile.FileExtension().ToLower() == "jpg") {
                                            newImg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                                        } else if (sourceFile.FileExtension().ToLower() == "png") {
                                            newImg.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                                        } else if (sourceFile.FileExtension().ToLower() == "gif") {
                                            newImg.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);

                                        } else {	// Default to bmp
                                            newImg.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                                        }
                                        buffer = ms.ToArray();
                                    }
                                }
                            } finally {
                                newImg.Dispose();
                            }
                        } catch (Exception e2) {
                            error.SetError(e2);
                        } finally {
                            img.Dispose();
                        }

                        if (buffer != null) {
                            FileStream olStr = null;
                            BinaryWriter olBinWriter = null;
                            try {
                                olStr = File.Create(targetFile);
                                olBinWriter = new BinaryWriter(olStr);
                                olBinWriter.Write(buffer);
                            } catch (Exception e3) {
                                error.SetError(e3);
                            }
                            if (olBinWriter != null) olBinWriter.Close();
                            if (olStr != null) olStr.Close();
                        }
                        if (buffer == null || (buffer != null && buffer.Length < 5)) {
                            // Now check the resulting file - if it is zero bytes it means that the
                            // graphic was too small to shrink, therefore, we use the original file
                            // and copy it as the thumbnail.
                            try {
                                if (!string.IsNullOrEmpty(targetFile)) File.Copy(sourceFile, targetFile, true);
                            } catch { }
                        }
                    }
                }
            }

            return error;
        }

        public Size CalculateFitSize(int ipOrigWidth, int ipOrigHeight, int ipReqWidth, int ipReqHeight, bool bUseOriginalIfSmaller = false) {
            // Scales a rectangle to fit it inside a bounding rectangle.
            // Used to calculate the size of thumbnails to fit a parameter rectangle.
            Size source = new Size(ipOrigWidth, ipOrigHeight),
                 target = new Size(ipReqWidth, ipReqHeight);

            if (ipOrigWidth <= ipReqWidth && ipOrigHeight <= ipReqHeight && bUseOriginalIfSmaller) {
                return source;

            } else {
                float ratio = (float)(Math.Max((float)source.Width / (float)target.Width, (float)source.Height / (float)target.Height));
                Size fitSize = new Size((int)Math.Round(source.Width / ratio), (int)Math.Round(source.Height / ratio));

                return fitSize;
            }
        }

        public string GetThumbFileName(string fileName, MediaSize thumbSize = MediaSize.Small) {

            int ilPos = fileName.LastIndexOf(".");
            string result = fileName;

            if (ilPos != -1) {
                string extn = fileName.Substring(ilPos);

                result = fileName.Substring(0, ilPos);

                switch (thumbSize) {
                case MediaSize.Large:
                    result += "_ThumbLarge" + extn;
                    break;
                case MediaSize.Medium:
                    result += "_ThumbMedium" + extn;
                    break;
                case MediaSize.Small:
                default:
                    result += "_ThumbSmall" + extn;
                    break;
                }
            }
            return result;
        }

        public string GetMediaThumb(MediaModel media, MediaSize thumbSize, int fitX, int fitY, ref int imageW, ref int imageH) {
            string thumbImage;

            imageW = imageH = 0;

            var mediaType = db.FindMediaType(media.MediaTypeId);
            if (mediaType.CreateThumb) {
                if (media.ImageW <= fitX && media.ImageH <= fitY) {
                    // Image is smaller than the fit area, so use it 'as is'
                    thumbImage = GetMediaFileName(media, true);
                } else {
                    // Image is larger than the fit area, so shrink it to fit
                    if (media.ImageW != null && media.ImageH != null) {
                        Size size = CalculateFitSize(media.ImageW.Value, media.ImageH.Value, fitX, fitY);
                        imageW = size.Width;
                        imageH = size.Height;
                    }
                    thumbImage = GetMediaFileName(media, true);
                }

            } else {
                // Media item which doesn't have thumbs created from itself eg MP3, MP4, PDF
                thumbImage = "/Content/MediaThumbs/";

                switch (thumbSize) {
                case MediaSize.Large:
                    thumbImage += mediaType.ThumbLarge;
                    break;
                case MediaSize.Medium:
                    thumbImage += mediaType.ThumbMedium;
                    break;

                default:
                    thumbImage += mediaType.ThumbSmall;
                    break;
                }
            }
            return thumbImage;
        }

        public string GetMediaSizeString(int mediaW, int mediaH) {
            if (mediaW > 0 && mediaH > 0) {
                return " width=\"" + mediaW.ToString() + "\" height=\"" + mediaH.ToString() + "\"";
            } else {
                return "";
            }
        }
    }
}
