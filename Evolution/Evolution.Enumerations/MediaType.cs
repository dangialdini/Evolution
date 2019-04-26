using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Enumerations {
    // This list must match Installation.MediaTypes.sql
    public enum MediaType {
        UNKNOWN = 0,
        PNG = 1,
        JPG = 2,
        JPEG = 3,
        GIF = 4,
        XLS = 5,
        XLSX = 6,
        DOC = 7,
        DOCX = 8,
        PPT = 9,
        PPTX = 10,
        PDF = 11,
        YOUTUBE = 12,
        AVI = 13,
        WMV = 14,
        MPG3 = 15,
        MPG4 = 16,
        ZIP = 17,
        TXT = 18,
        MOV = 19,
        WAV = 20,
        REFERENCE = 21
    }

    public enum MediaSize {
        Original = 0,

        Small = 1,
        SmallW = 16,
        SmallH = 16,

        Medium = 2,
        MediumW = 64,
        MediumH = 64,

        Large = 3,
        LargeW = 150,
        LargeH = 150
    }
}
