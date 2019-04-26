using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Enumerations {
    public class MenuOptionFlag {
        public const int RequiresCustomer = 1;
        public const int RequiresPurchase = 2;
        public const int RequiresShipment = 4;
        public const int RequiresSale = 8;
        public const int RequiresProduct = 16;
        public const int RequiresNoProduct = 32;
        public const int RequiresNoPurchase = 64;
        public const int RequiresNoSale = 128;
        public const int RequiresNoCustomer = 256;
        public const int RequiresNewSale = 512;
        public const int RequiresNewProduct = 1024;
        public const int RequiresNewPurchase = 2048;
        public const int RequiresSupplier = 4096;
        public const int RequiresNoSupplier = 8192;
    }
}
