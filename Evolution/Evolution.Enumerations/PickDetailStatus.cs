using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Enumerations {
    public enum PickDetailStatus {
        // This list must be synchronised with the PickDetailStatus table
        ToBePicked = 1,
        Picked = 2,
        Flagged = 3
    }
}
