using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class AllocationInfoViewModel : ViewModelBase {
        public ProductModel Product { set; get; }
        public int AvailableNow { set; get; }

        // Availability Summary
        public int TotalStockOnHandAndOnOrder { set; get; }
        public int TotalAllocatedStock { set; get; }
        public int TotalAvailableStock { set; get; }

        // Sales Order Summary
        public int TotalStockOnSalesOrders { set; get; }
        public int TotalAllocatedStockOnSalesOrders { set; get; }
        public int TotalRequiredStockOnSalesOrders { set; get; }
    }
}
