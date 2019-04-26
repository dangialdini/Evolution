using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.LookupService {
    public partial class LookupService {

        #region Public members    

        public List<DocumentTemplateModel> FindDocumentTemplatesModel() {
            List<DocumentTemplateModel> model = new List<DocumentTemplateModel>();

            foreach(var item in db.FindDocumentTemplates()) {
                model.Add(MapToModel(item));
            }
            return model;
        }

        public List<ListItemModel> FindDocumentTemplatesListItemModel(DocumentTemplateCategory templateCategory) {
            List<ListItemModel> model = new List<ListItemModel>();

            foreach (var st in db.FindDocumentTemplates()
                                 .Where(t => t.TemplateCategory == (int)templateCategory)) {
                var newModel = new ListItemModel {
                    Id = st.Id.ToString(),
                    Text = st.Name,
                    ImageURL = ""
                };
                model.Add(newModel);
            }
            return model;
        }

        public List<ListItemModel> FindDocumentTemplatesListItemModel(DocumentTemplateCategory templateCategory,
                                                                      DocumentTemplateType templateType) {
            List<ListItemModel> model = new List<ListItemModel>();

            foreach (var st in db.FindDocumentTemplates()
                                 .Where(t => t.TemplateCategory == (int)templateCategory &&
                                             t.TemplateType == (int)templateType)) {
                var newModel = new ListItemModel {
                    Id = st.Id.ToString(),
                    Text = st.Name,
                    ImageURL = ""
                };
                model.Add(newModel);
            }
            return model;
        }

        public DocumentTemplateListModel FindDocumentTemplatesListModel(int index, int pageNo, int pageSize, string search) {
            var model = new DocumentTemplateListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindDocumentTemplates(true)
                            .Where(st => (string.IsNullOrEmpty(search) ||
                                          (st.Name != null && st.Name.ToLower().Contains(search.ToLower())) ||
                                          (st.Description != null && st.Description.ToLower().Contains(search.ToLower()))));

            model.TotalRecords = allItems.Count();
            foreach(var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public DocumentTemplateModel FindDocumentTemplateModel(int id) {
            return MapToModel(db.FindDocumentTemplate(id));
        }

        public DocumentTemplateModel FindDocumentTemplateModel(DocumentTemplateCategory templateCategory,
                                                               DocumentTemplateType templateType) {
            var template = db.FindDocumentTemplates()
                             .Where(dt => dt.TemplateCategory == (int)templateCategory &&
                                          dt.TemplateType == (int)templateType)
                             .FirstOrDefault();
            return MapToModel(template);
        }

        public DocumentTemplateModel MapToModel(DocumentTemplate item) {
            DocumentTemplateModel model = null;
            if (item != null) {
                model = Mapper.Map<DocumentTemplate, DocumentTemplateModel>(item);
                model.QualTemplateFile = GetConfigurationSetting("SiteFolder", "") + $"\\App_Data{item.TemplateFile}";
            }
            return model;
        }

        public DocumentTemplateModel Clone(DocumentTemplateModel item) {
            var newItem = Mapper.Map<DocumentTemplateModel, DocumentTemplateModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateDocumentTemplate(DocumentTemplateModel template, UserModel user, string lockGuid) {
            var error = validateModel(template);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(DocumentTemplate).ToString(), template.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "TemplateName");

                } else {
                    DocumentTemplate temp = null;
                    if (template.Id != 0) temp = db.FindDocumentTemplate(template.Id);
                    if (temp == null) temp = new DocumentTemplate();

                    Mapper.Map<DocumentTemplateModel, DocumentTemplate>(template, temp);

                    db.InsertOrUpdateDocumentTemplate(temp);
                    template.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteDocumentTemplate(int id) {
            db.DeleteDocumentTemplate(id);
        }

        public string LockDocumentTemplate(DocumentTemplateModel model) {
            return db.LockRecord(typeof(DocumentTemplate).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(DocumentTemplateModel model) {
            var error = new Error();

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var template = db.FindDocumentTemplate(model.Name);
                if (template != null &&                 // Record was found
                    ((template.Id == 0 ||               // when creating new or
                      template.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "Name");
                }
            }

            return error;
        }

        #endregion
    }
}
