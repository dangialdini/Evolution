using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.ViewModels {

    public class GridViewItemModel {

    }

    public class GridViewModel {
        public List<GridViewItemModel> Items { set; get; } = new List<GridViewItemModel>();
    }
}
