using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Enumerations {
    // The values here must correspond with values in MediaService.cs, GetFolderName()
    // and also the NoteType.cs enum (the later is because enums can't be derived from each other)
    public enum MediaFolder {
        Temp = 1,
        User = 2,
        Customer = 3,
        Purchase = 4,
        Supplier = 5,
        Sale = 6,
        Product = 7,
        ProductCompliance = 8
    }
}
