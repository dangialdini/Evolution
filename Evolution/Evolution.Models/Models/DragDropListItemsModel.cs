using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class DragDropListItemsModel {
        public string ControlPrefix { set; get; } = "";
        public string AvailableItemsLabel { set; get; } = "Available Items";
        public string SelectedItemsLabel { set; get; } = "Selected Items";

        public List<ListItemModel> SelectedItems = new List<ListItemModel>();
        public List<int> SelectedIds = new List<int>();

        public List<ListItemModel> AvailableItemList = new List<ListItemModel>();
        public List<ListItemModel> SelectedItemList = new List<ListItemModel>();

        public void SetAvailableItems(List<ListItemModel> items) {
            AvailableItemList = items;
        }

        public void SetSelectedItems(List<ListItemModel> items) {
            SelectedItems.Clear();
            SelectedItemList.Clear();
            foreach (var item in items) {
                int found = -1;
                for (int i = 0; found == -1 && i < AvailableItemList.Count(); i++) {
                    if (AvailableItemList[i].Id == item.Id) found = i;
                }
                if (found != -1) {
                    SelectedItems.Add(item);
                    SelectedItemList.Add(item);
                    AvailableItemList.RemoveAt(found);
                }
            }
        }

        public string ToIntString() {
            string rc = "";
            foreach(var item in SelectedItems) {
                if (!string.IsNullOrEmpty(rc)) rc += ",";
                rc += item.Id.ToString();
            }
            return rc;
        }
    }
}
