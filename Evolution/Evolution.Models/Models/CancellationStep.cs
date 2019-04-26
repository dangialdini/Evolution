using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class ProductStatusValue {
        public bool Value { set; get; } = false;
    }

    public class CancellationStep1 {
        public bool DeliveryWindowClosed { set; get; } = false;
        public int CancelAll { set; get; } = 1;

        public List<ProductStatusValue> ProductStatus { set; get; } = new List<ProductStatusValue>();
        public List<ListItemModel> ProductStatusList { set; get; } = new List<ListItemModel>();

        public int BrandCategoryId { set; get; } = 0;
        public List<ListItemModel> BrandCategoryList { set; get; } = new List<ListItemModel>();
    }

    public class CancellationStep2 {
        [UIHint("DragDropListBox")]
        public DragDropListItemsModel CustomerList { set; get; } = new DragDropListItemsModel();
    }

    public class CancellationStep3 {
        [UIHint("DragDropListBox")]
        public DragDropListItemsModel OrderList { set; get; } = new DragDropListItemsModel();
    }

    public class CancellationStep4 {
        [UIHint("DragDropListBox")]
        public DragDropListItemsModel ProductList { set; get; } = new DragDropListItemsModel();
    }

    public class CancellationStep5 {
        [UIHint("DragDropListBox")]
        public DragDropListItemsModel WarehouseList { set; get; } = new DragDropListItemsModel();
    }

    public class CancellationStep6 {
        [UIHint("DragDropListBox")]
        public DragDropListItemsModel AccountManagerList { set; get; } = new DragDropListItemsModel();
    }

    public class CancellationStep7 {
    }
}
