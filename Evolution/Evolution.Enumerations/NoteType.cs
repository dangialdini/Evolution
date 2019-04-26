using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Enumerations {
    // These values must be the same as the MediaFolder.cs enum (because enums can't be derived from each other)
    public enum NoteType {
        Customer = 3,
        Purchase = 4,
        Supplier = 5,
        Sale = 6,
        Product = 7,
        ProductCompliance = 8
    }
}
