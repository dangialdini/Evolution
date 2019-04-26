using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class MenuOptionModel {

        public const int TextOption = 1;
        public const int SeparatorOption = 2;
        public const int BrandImageOption = 3;
        public const int BrandTextOption = 4;

        public MenuOptionModel() {

        }

        public MenuOptionModel(string optionText, string url = "") {
            OptionText = optionText;
            URL = url;
        }

        public int OptionType { set; get; }
        public string OptionText { set; get; }
        public string Tooltip { set; get; }
        public string Image { set; get; }
        public string Alignment { set; get; }
        public string URL { set; get; }
        public string WindowName { set; get; }
        public bool Active { set; get; } = false;
        public MenuOptionModel ParentOption { set; get; } = null;
        public List<MenuOptionModel> Options { set; get; } = new List<MenuOptionModel>();
    }

    public class MenuModel {
        public MenuModel() {
            IsVertical = false;
        }
        public MenuModel(bool isVertical) {
            IsVertical = isVertical;
        }
        public bool IsVertical { set; get; }
        public List<MenuOptionModel> Options { set; get; } = new List<MenuOptionModel>();
    }

    public class MenuBreadCrumbItemModel {
        public string OptionText { set; get; }
        public string Tooltip { set; get; }
        public string URL { set; get; }
    }

    public class MenuBreadCrumbModel {
        public List<MenuBreadCrumbItemModel> Items = new List<MenuBreadCrumbItemModel>();
    }

    public class MenuDataModel {
        public MenuModel Menu1 = new MenuModel();
        public MenuModel Menu2 = new MenuModel();
        public MenuBreadCrumbModel BreadCrumb = new MenuBreadCrumbModel();
    }
}
