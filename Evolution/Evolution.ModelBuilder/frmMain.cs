using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace Evolution.ModelBuilder {
    public partial class frmMain : Form {

        #region Form initialisation

        public frmMain() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        #endregion

        #region Script creation

        private void btnCreate_Click(object sender, EventArgs e) {
            if (chkCreateServiceClass.Checked) CreateServiceClass();
            if (chkCreateModelClass.Checked) CreateModelClass();
            if (chkCreateViewModelClass.Checked) CreateViewModelClass();

            MessageBox.Show("Done!");
        }

        void CreateServiceClass() {
            string template = ReadTemplateFile("ServiceClassTemplate");
        }

        void CreateModelClass() {
            string template = ReadTemplateFile("ModelClassTemplate");
        }

        void CreateViewModelClass() {
            string template = ReadTemplateFile("ViewModelClassTemplate");
        }

        #endregion

        #region Support methods

        private string GetConfigurationSetting(string key, string defaultValue) {
            string result = defaultValue;
            try {
                result = ConfigurationManager.AppSettings[key];
            } catch { }
            return result;
        }

        private string ReadTemplateFile(string configKey) {
            string template = "";
            string fileName = GetConfigurationSetting(configKey, "");

            using (StreamReader sr = new StreamReader(fileName)) {
                template = sr.ReadToEnd();
            }

            return template;
        }

        #endregion
    }
}
