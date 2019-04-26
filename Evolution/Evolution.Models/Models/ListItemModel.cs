using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class ListItemModel {
        public string Id { set; get; }
        public string Text { set; get; }
        public string ImageURL { set; get; }

        public ListItemModel() {
        }

        public ListItemModel(string text, string id, string imageUrl = "") {
            Id = id;
            Text = text;
            ImageURL = imageUrl;
        }

        public ListItemModel(string text, int id, string imageUrl = "") {
            Id = id.ToString();
            Text = text;
            ImageURL = imageUrl;
        }
    }
}
