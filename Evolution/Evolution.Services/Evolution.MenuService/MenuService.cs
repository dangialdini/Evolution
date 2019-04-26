using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.MenuService {
    public class MenuService : CommonService.CommonService {

        #region Private members

        List<GetMenuTree_Result> options = null;

        const int OptionType_TextOption = 1;
        const int OptionType_SeparatorOption = 2;
        const int OptionType_BrandImageOption = 3;
        const int OptionType_BrandTextOption = 4;
        const int OptionType_LeftOption = 5;

        const int WhenLoggedIn = 1;
        const int WhenLoggedOut = 2;
        const int Everyone = 3;

        #endregion

        #region Construction

        public MenuService(EvolutionEntities dbEntities) : base(dbEntities) {
        }

        #endregion

        #region Public methods

        public MenuDataModel GetMenu(int menuRootId,
                                     bool isLoggedIn,
                                     string currentRoles,
                                     int objectFlags,
                                     string currentTag,
                                     Dictionary<string, string> dict) {

            var menuData = new MenuDataModel();

            options = db.GetMenuTree(menuRootId,
                                     (isLoggedIn ? 1 : 0),
                                     currentRoles,
                                     objectFlags,
                                     currentTag)
                        .ToList();

            if(options.Count() > 0)
                addOptions(options[0].ParentOptionId.Value, menuData.Menu1, menuData.Menu2, menuData.BreadCrumb, null, dict);

            return menuData;
        }

        void addOptions(int menuId, 
                        MenuModel topMenu, 
                        MenuModel leftMenu, 
                        MenuBreadCrumbModel breadCrumb,
                        MenuOptionModel parentOption, 
                        Dictionary<string, string> dict) {

            foreach(var menuOption in findOptions(menuId)) {
                var newOption = new MenuOptionModel {
                    OptionType = (menuOption.Name == "-" ? MenuOptionModel.SeparatorOption : menuOption.OptionType.Value), // Models.MenuOptionModel.TextOption),
                    OptionText = menuOption.Name.DoSubstitutions(dict),
                    Tooltip = menuOption.Tooltip.DoSubstitutions(dict),
                    Image = menuOption.Image,
                    Alignment = menuOption.Alignment,
                    URL = menuOption.Url.DoSubstitutions(dict),
                    WindowName = menuOption.WindowName,
                    Active = (menuOption.Active == 1 ? true : false)
                };
                if(newOption.Active) {
                    breadCrumb.Items.Add(new MenuBreadCrumbItemModel {
                        OptionText = newOption.OptionText,
                        Tooltip = newOption.Tooltip,
                        URL = newOption.URL
                    });
                }

                if (parentOption == null) {
                    // Root menu options
                    topMenu.Options.Add(newOption);
                    addOptions(menuOption.Id.Value, topMenu, leftMenu, breadCrumb, newOption, dict);

                } else {
                    // Non-root menu options or left menu options
                    if (menuOption.OptionType == OptionType_LeftOption) {
                        leftMenu.Options.Add(newOption);
                    } else {
                        parentOption.Options.Add(newOption);
                    }
                    addOptions(menuOption.Id.Value, topMenu, leftMenu, breadCrumb, newOption, dict);
                }
            }
        }

        List<GetMenuTree_Result> findOptions(int menuId) {
            return options.Where(opt => opt.ParentOptionId == menuId)
                                         .OrderBy(opt => opt.OrderNo)
                          .ToList();
        }

        public bool HasAccessRights(string pageId, bool isLoggedIn, string currentRoles, int requiredObjectFlags = 0) {
            bool bRc = false;
            var option = db.FindMenuOption(pageId, (isLoggedIn ? WhenLoggedIn : WhenLoggedOut), requiredObjectFlags);
            if (option != null && !string.IsNullOrEmpty(option.RequiredRoles)) bRc = option.RequiredRoles.IsItemShared(currentRoles);
            return bRc;
        }

        #endregion
    }
}
