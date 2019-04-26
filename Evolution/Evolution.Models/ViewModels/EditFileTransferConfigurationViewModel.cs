using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditFileTransferConfigurationViewModel : ViewModelBase {
        public FileTransferConfigurationModel FileTransferConfiguration { set; get; }

        public List<ListItemModel> CompanyList = new List<ListItemModel>();
        public List<ListItemModel> TransferTypeList = new List<ListItemModel>();
        public List<ListItemModel> DataTypeList = new List<ListItemModel>();
        public List<ListItemModel> FTPProtocolList = new List<ListItemModel>();
        public List<ListItemModel> LocationList = new List<ListItemModel>();
        public List<ListItemModel> FreightForwarderList = new List<ListItemModel>();
        public List<ListItemModel> ConfigurationTemplateList = new List<ListItemModel>();
    }
}
