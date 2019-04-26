﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditCustomerConflictViewModel : ViewModelBase {
        public CustomerConflictModel CustomerConflict { set; get; }
        public string SelectedItems { set; get; } = "";
    }
}
