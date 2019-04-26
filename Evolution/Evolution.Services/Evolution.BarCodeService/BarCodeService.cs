using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.MediaService;
using BarcodeLib;

namespace Evolution.BarCodeService {
    public class BarCodeService : CommonService.CommonService {

        public BarCodeService(EvolutionEntities dbEntities) : base(dbEntities) { }

        public string CreateBarCode(string barCode, bool bHtml = false, bool bRegisterForDelete = false) {
            string  barCodeFile = "",
                    tempBc = barCode.Trim();
            if (!string.IsNullOrEmpty(tempBc)) {
                using (var bc = new Barcode()) {
                    try {
                        bc.IncludeLabel = true;
                        var image = bc.Encode(TYPE.CODE128, barCode, 220, 50);

                        var mediaService = new MediaService.MediaService(db);
                        var barCodeFolder = GetConfigurationSetting("MediaFolder", "") + "\\Barcodes";

                        var error = MediaService.MediaService.CreateDirectory(barCodeFolder);
                        if (!error.IsError) {
                            barCodeFile = $"{tempBc}.png";
                            var temp = $"{barCodeFolder}\\{barCodeFile}";

                            if (bRegisterForDelete) mediaService.AddFileToLog(temp, 20);        // So it gets cleaned up later

                            image.Save(temp, ImageFormat.Png);

                            if (bHtml) {
                                barCodeFolder = GetConfigurationSetting("MediaHttp", "") + "/Barcodes";
                                temp = $"{barCodeFolder}/{barCodeFile}";
                            }
                            barCodeFile = temp;
                        }
                    } catch { }
                }
            }
            return barCodeFile;
        }

        public string GetBarCode(string barCode, bool bHtml = false, bool bRegisterForDelete = false) {
            if (barCode != null) {
                string bc = barCode.Trim();
                if (!string.IsNullOrEmpty(bc)) {
                    string barCodeFile = GetConfigurationSetting("MediaFolder", "") + $"\\Barcodes\\{bc}.png";
                    if (File.Exists(barCodeFile)) {
                        if (bHtml) barCodeFile = GetConfigurationSetting("MediaHttp", "") + $"/Barcodes/{bc}.png";
                        return barCodeFile;
                    } else {
                        return CreateBarCode(barCode, bHtml, bRegisterForDelete);
                    }
                } else {
                    return "";
                }
            }
            return "";
        }
    }
}
