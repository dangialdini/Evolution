﻿using Evolution.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.ViewModels {
    public class SupplierAddressesListViewModel : ViewModelBase {
        public SupplierAddressListModel Addresses { get; set; }
        public List<ListItemModel> DateFormatList { get; set; }
    }
}
