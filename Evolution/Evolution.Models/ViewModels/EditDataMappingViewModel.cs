﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditDataMappingViewModel : ViewModelBase {
        public DataMappingModel Data { set; get; } = new DataMappingModel();

        public List<ListItemModel> HeadingList { set; get; } = new List<ListItemModel>();
    }
}
