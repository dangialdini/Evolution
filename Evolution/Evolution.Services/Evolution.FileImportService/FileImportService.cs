using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CSVFileService;
using Evolution.Enumerations;
using Evolution.Extensions;
using AutoMapper;

namespace Evolution.FileImportService {
    public class FileImportService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public FileImportService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<FileImportRow, FileImportRowModel>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion

        #region Public members

        public Error UploadFile(CompanyModel company, UserModel user, string fileName,
                                bool bFirstLineContainsHeader) {
            var error = new Error();
            int maxFields = 64;

            // Load the file into the database
            CSVReader sr = new CSVReader();

            var fieldList = new List<CSVField>();
            for (int i = 1; i <= maxFields; i++) fieldList.Add(new CSVField {
                FieldName = "Field" + i.ToString(),
                FieldType = CSVFieldType.String
            });

            try {
                db.ClearFileImportRows(company.Id, user.Id);

                sr.OpenFile(fileName, false, fieldList);

                Dictionary<string, string> lineFields;

                int rowNo = 1;
                string importLine;

                while ((lineFields = sr.ReadLine()) != null) {
                    if(rowNo == 1) {
                        // First line, so count the fields
                        var tempMaxFields = 0;
                        foreach(var item in lineFields) {
                            if (string.IsNullOrEmpty(item.Value)) {
                                break;
                            } else {
                                tempMaxFields++;
                            }
                        }
                        maxFields = tempMaxFields;

                        if (!bFirstLineContainsHeader) {
                            importLine = "";

                            for (int i = 0; i < maxFields; i++) {
                                if (i > 0) importLine += "\t";
                                importLine += fieldList[i].FieldName;
                            }
                            db.InsertFileImportFile(company.Id, user.Id, importLine);
                        }
                    }

                    // Import a row
                    importLine = "";

                    int fldNo = 0;
                    foreach (var item in lineFields) {
                        if(fldNo < maxFields) {
                            if (fldNo > 0) importLine += "\t";
                            importLine += item.Value;
                        } else {
                            break;
                        }
                        fldNo++;
                    }
                    db.InsertFileImportFile(company.Id, user.Id, importLine);

                    rowNo++;
                }

                sr.Close();

                try {
                    // Cleanup the temp file
                    System.IO.File.Delete(fileName);
                } catch { }

                // Now move the lines from FileImportFile into FileImport[Row|Field]
                db.ImportFileLines(company.Id, user.Id);

            } catch (Exception e1) {
                error.SetError(e1);
            }
            sr.Close();

            return error;
        }

        public void InsertOrUpdateFileImportRow(FileImportRowModel row) {
            FileImportRow tempRow = null;
            if (row.Id != 0) {
                tempRow = db.FindFileImportRow(row.CompanyId, row.UserId, row.Id);
                if (tempRow == null) row.Id = 0;
            }
            if (row.Id == 0) {
                // New row
                tempRow = new FileImportRow {
                    CompanyId = row.CompanyId,
                    UserId = row.UserId,
                    ProductId = row.ProductId,
                    SupplierId = row.SupplierId
                };
                db.InsertOrUpdateFileImportRow(tempRow);
                row.Id = tempRow.Id;

                foreach (var field in row.Fields) {
                    var tempField = new FileImportField {
                        CompanyId = row.CompanyId,
                        FileImportRowId = tempRow.Id,
                        Value = field.Value
                    };
                    db.InsertOrUpdateFileImportField(tempField, false);
                }
                db.SaveChanges();

            } else {
                // Update existing row
                tempRow.ErrorMessage = row.ErrorMessage;
                tempRow.ProductId = row.ProductId;
                tempRow.SupplierId = row.SupplierId;
                db.InsertOrUpdateFileImportRow(tempRow);

                int fldNo = 0;
                foreach (var field in tempRow.FileImportFields)  {
                    field.Value = row.Fields[fldNo].Value;
                    fldNo++;
                }
                db.InsertOrUpdateFileImportRow(tempRow);
            }
        }

        public DataMappingModel GetData(CompanyModel company, UserModel user) {
            DataMappingModel model = new DataMappingModel();

            int lineNo = 0;
            foreach (var row in db.FindFileImportRows(company.Id, user.Id)) {
                if (lineNo == 0) {
                    foreach(var field in row.FileImportFields) {
                        model.Headings.Add(field.Value);
                    }
                } else {
                    var newRow = new FileImportRowModel();
                    foreach (var field in row.FileImportFields) {
                        var newField = new FileImportFieldModel { Value = (string.IsNullOrEmpty(field.Value) ? "" : field.Value) };
                        newRow.Fields.Add(newField);
                    }
                    newRow.ErrorMessage = row.ErrorMessage;
                    model.Lines.Add(newRow);
                }
                lineNo++;
            }
            return model;
        }

        public List<ListItemModel> GetHeadingList(string importTemplateFile, bool bSort = true, bool bIncludeBlank = true) {
            string fileName = GetConfigurationSetting("SiteFolder", "") +
                              @"\App_Data\FileImportTemplates\" + importTemplateFile;

            List<ListItemModel> list = new List<ListItemModel>();

            try {
                using(StreamReader sr = new StreamReader(fileName)) {
                    string line;
                    while((line = sr.ReadLine()) != null) {
                        line = line.Trim();
                        if (!string.IsNullOrEmpty(line) && line.Left(2) != "//") {
                            string[] data = line.Split(',');
                            if (data.Length >= 2) {
                                if (bIncludeBlank || (!bIncludeBlank && !string.IsNullOrEmpty(data[0]))) {
                                    var newItem = new ListItemModel(data[1], data[0]);
                                    list.Add(newItem);
                                }
                            }
                        }
                    }
                }
            } catch { }

            if (bSort) {
                return list.OrderBy(l => l.Text)
                           .ToList();
            } else {
                return list;
            }
        }

        #endregion
    }
}
