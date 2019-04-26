using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditPaymentTermViewModel : ViewModelBase {
        public PaymentTermModel PaymentTerm { set; get; }
    }
}
