using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Enumerations {
    public class UserRole {
        public const string Administrator = "Administrator";
        public const string SuperUser = "SuperUser";
        public const string Sales = "Sales";
        public const string Purchasing = "Purchasing";
        public const string Finance = "Finance";
        public const string PurchasingSuper = "PurchasingSuper";
        public const string Logistics = "Logistics";

        public const string AllUsers = Administrator + "," +
                                       SuperUser + "," +
                                       Sales + "," +
                                       Purchasing + "," +
                                       Finance + "," +
                                       PurchasingSuper + "," +
                                       Logistics;

        public const string AllPurchasing = Purchasing + "," +
                                            PurchasingSuper;
    }
}
