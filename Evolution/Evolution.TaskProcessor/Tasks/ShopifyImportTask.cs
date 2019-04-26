using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.ShopifyImportService;

namespace Evolution.TaskProcessor {
    public class ShopifyImportTask : TaskBase {
        public ShopifyImportTask(EvolutionEntities dbEntities) : base(dbEntities) { }

        public override string GetTaskName() { return TaskName.ShopifyImportTask; }

        public override int DoProcessing(string[] args) {
            var shopifyImportService = new ShopifyImportService.ShopifyImportService(_db);
            DataTransferService.DataTransferService dts = new DataTransferService.DataTransferService(_db);

            int i = 1;
            var profileName = GetTaskParameter($"DataTransfer{1}", "");
            var taskUser = GetTaskUser();
            string errorMsg = "";

            var config = dts.FindDataTransferConfigurationModel(profileName);
            if (config == null) {
                TaskService.WriteTaskLog(this.Task, $"Error: Failed to find Data Transfer configuration '{profileName}' !");

            } else {
                string fileLoc = config.TargetFolder;
                while (!string.IsNullOrEmpty(fileLoc)) {
                    string businessName = GetTaskParameter($"BusinessName{i}", "");
                    string[] files = null;
                    try {
                        files = Directory.GetFiles(fileLoc);
                    } catch (Exception ex) {
                        TaskService.WriteTaskLog(this.Task, $"Error: There was a problem getting files from '{fileLoc}'\r\n" + ex, LogSeverity.Severe);
                    }

                    if (files.Length > 0 && files != null) {
                        foreach (string fileName in files) {

                            // Process File
                            TaskService.WriteTaskLog(this.Task, $"Success: Processing file '{fileName}'", LogSeverity.Normal);
                            ShopifyImportTempModel.Data shopifyTempModel = null;
                            try {
                                shopifyTempModel = shopifyImportService.ProcessXml(fileName, businessName);
                            } catch (Exception ex) {
                                TaskService.WriteTaskLog(this.Task, $"Error: Could not process file '{fileName}'\r\n" + ex, LogSeverity.Severe);
                                //MoveFileToErrorFolder(dts, config, fileName);
                            }

                            if (shopifyTempModel != null) {
                                List<ShopifyImportHeaderTemp> sihtList = new List<ShopifyImportHeaderTemp>();

                                // Map each order to TEMP
                                foreach (var order in shopifyTempModel.Order) {
                                    try {
                                        Dictionary<string, string> configDetails = LookupConfigDetails(order.StoreName);
                                        if (configDetails.Count > 0) {
                                            ShopifyImportHeaderTemp siht = new ShopifyImportHeaderTemp();
                                            siht = shopifyImportService.MapOrderToTemp(businessName, configDetails, order, taskUser);
                                            sihtList.Add(siht);
                                        } else {
                                            TaskService.WriteTaskLog(this.Task, $"Error: Configuration Setting are not setup", LogSeverity.Severe);
                                        }
                                    } catch (Exception ex) {
                                        TaskService.WriteTaskLog(this.Task, $"Error: Could not map order (#{order.OrderNumber}) details to temp table\r\n" + ex, LogSeverity.Severe);
                                        //MoveFileToErrorFolder(dts, config, fileName);
                                        break;
                                    }
                                }

                                if (sihtList.Count > 0 && sihtList.Count == shopifyTempModel.Order.Length) { // AND COUNT = THE ACTUAL NUMBER OF ORDERS IN THE XML FILE
                                    // Save temp to db
                                    try {
                                        shopifyImportService.SaveDataToTempTables(sihtList);
                                        TaskService.WriteTaskLog(this.Task, $"Success: Saved '{fileName}' temp table", LogSeverity.Normal);

                                        // Get Temp table data
                                        try {
                                            sihtList = shopifyImportService.GetShopifyTempTableData();
                                            if (sihtList.Count > 0) {
                                                try {
                                                    // CopyTempDataToSalesModel
                                                    List<SalesOrderHeader> orders = shopifyImportService.CopyTempDataToSalesModel(sihtList, businessName);
                                                    if (orders.Count > 0) {
                                                        if (shopifyImportService.SaveSalesOrders(orders)) {
                                                            TaskService.WriteTaskLog(this.Task, $"Success: Data Saved to Sales tables", LogSeverity.Normal);

                                                            // Move file to Archive folder
                                                            if (dts.MoveToArchive(config, fileName, ref errorMsg)) {
                                                                TaskService.WriteTaskLog(this.Task, $"Failed to move to Archive folder\r\n{errorMsg}", LogSeverity.Severe);
                                                            } else {
                                                                TaskService.WriteTaskLog(this.Task, $"Successfully moved file '{fileName}' to Archive folder", LogSeverity.Normal);
                                                            }
                                                        } else {
                                                            // Move file to Error folder
                                                            MoveFileToErrorFolder(dts, config, fileName);
                                                        }
                                                    } else {
                                                        TaskService.WriteTaskLog(this.Task, $"Error: Could not retrieve any orders from the temp table/s", LogSeverity.Severe);
                                                        MoveFileToErrorFolder(dts, config, fileName);
                                                    }
                                                } catch (Exception ex) {
                                                    TaskService.WriteTaskLog(this.Task, $"Error: Could not copy data to sales table/s\r\n" + ex, LogSeverity.Severe);
                                                    MoveFileToErrorFolder(dts, config, fileName);
                                                }
                                            }
                                        } catch (Exception ex) {
                                            TaskService.WriteTaskLog(this.Task, $"Error: Failed to get temp data from database\r\n" + ex, LogSeverity.Severe);
                                            MoveFileToErrorFolder(dts, config, fileName);
                                        }
                                    } catch (Exception ex) {
                                        TaskService.WriteTaskLog(this.Task, $"Error: Failed to save data to the temp table/s\r\n" + ex, LogSeverity.Severe);
                                        MoveFileToErrorFolder(dts, config, fileName);
                                    }
                                }
                            } else {
                                TaskService.WriteTaskLog(this.Task, $"Error: The file '{fileName}' was empty", LogSeverity.Severe);
                                MoveFileToErrorFolder(dts, config, fileName);
                            }
                            MoveFileToErrorFolder(dts, config, fileName);
                        }
                    } else {
                        TaskService.WriteTaskLog(this.Task, $"INFO: There were no files to process.'", LogSeverity.Normal);
                    }
                    i++;
                    profileName = GetTaskParameter($"DataTransfer{i}", "");
                    config = dts.FindDataTransferConfigurationModel(profileName);
                    fileLoc = (config != null) ? config.TargetFolder : "";
                }
            }
            return 0;
        }

        private Dictionary<string, string> LookupConfigDetails(string storeNameToLookup) {
            Dictionary<string, string> returnedData = new Dictionary<string, string>();
            int i = 1;
            var storeName = GetTaskParameter($"StoreName{i}", "");

            while (!string.IsNullOrEmpty(storeName)) {
                if (storeNameToLookup.ToLower() == storeName.ToLower()) {
                    returnedData.Add("StoreId", i.ToString());
                    returnedData.Add("CustomerName", GetTaskParameter($"CustomerName{i}", ""));
                    returnedData.Add("BrandCategory", GetTaskParameter($"BrandCategory{i}", ""));
                    returnedData.Add("DataSource", GetTaskParameter($"DataSource{i}", "OrderHive"));
                    return returnedData;
                } else {
                    i++;
                    storeName = GetTaskParameter($"StoreName{i}", "");
                }
            }
            return returnedData;
        }

        private void MoveFileToErrorFolder(DataTransferService.DataTransferService dts, FileTransferConfigurationModel config, string fileName) {
            string errorMsg = "";
            if(dts.MoveToError(config, fileName, ref errorMsg)) {
                TaskService.WriteTaskLog(this.Task, $"Failed to move to Error folder\r\n{errorMsg}", LogSeverity.Severe);
            } else {
                TaskService.WriteTaskLog(this.Task, $"File '{fileName}' has been moved to the Error folder", LogSeverity.Severe);
            }
        }

    }
}
