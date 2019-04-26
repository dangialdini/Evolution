using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Models.ViewModels {
    public class EditSalesOrderDetailTempViewModel : ViewModelBase {
        public SalesOrderDetailTempModel SalesOrderDetailTemp { set; get; }
    }
}
