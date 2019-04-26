using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.Models.Models {
    public class FileTransferConfigurationModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public FileTransferType TransferType { set; get; } = FileTransferType.Send;
        public int DataTypeId { set; get; } = 0;
        public DateTimeOffset CreatedDate { set; get; }
        public int CreatedById { set; get; } = 0;
        public string TransferName { set; get; } = "";
        public string FTPHost { set; get; } = "";
        public string UserId { set; get; } = "";
        public string Password { set; get; } = "";
        public FTPProtocol Protocol { set; get; } = FTPProtocol.FTP;
        public string SourceFolder { set; get; } = "";
        public string TargetFolder { set; get; } = "";
        public string ArchiveFolder { set; get; } = "";
        public string ErrorFolder { set; get; } = "";
        public string ConfigurationTemplate { set; get; } = "";
        public int? LocationId { set; get; } = null;            // For a warehouse
        public int? FreightForwarderId { set; get; } = null;    // For a freight forwarder
        public string PostTransferCommand { set; get; }
        public string PostTransferParameters { set; get; }
        public string TargetFileNameFormat { set; get; }
        public bool Enabled { set; get; } = false;

        // Additional fields
        public string CreatedDateISO { get { return CreatedDate.ISODate(); } }
        public string CreatedByText { set; get; } = "";
        public string TransferTypeText { set; get; } = "";
        public string DataTypeText { set; get; } = "";
        public FileTransferDataType DataType { set; get; }
        public string ProtocolText { set; get; } = "";
        public string DataTransferFolder { set; get; } = "";
    }

    public class FileTransferConfigurationListModel : BaseListModel {
        public List<FileTransferConfigurationModel> Items { set; get; } = new List<FileTransferConfigurationModel>();
    }
}
