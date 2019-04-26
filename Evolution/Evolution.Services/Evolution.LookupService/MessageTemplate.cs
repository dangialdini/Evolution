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

        public List<MessageTemplateModel> FindMessageTemplatesModel(int companyId) {
            List<MessageTemplateModel> model = new List<MessageTemplateModel>();

            foreach(var template in db.FindMessageTemplates(companyId)) {
                model.Add(MapToModel(template));
            }

            return model;
        }

        public MessageTemplateListModel FindMessageTemplatesListModel(int companyId, int index, int pageNo, int pageSize, string search) {
            var model = new MessageTemplateListModel();

            int numValue = 0;
            bool bGotNum = int.TryParse(search, out numValue);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindMessageTemplates(companyId, true)
                             .Where(et => et.CompanyId == companyId &&
                                          (string.IsNullOrEmpty(search) ||
                                           (et.Subject != null && et.Subject.ToLower().Contains(search.ToLower())) ||
                                           (et.Message != null && et.Message.ToLower().Contains(search.ToLower()))));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                         .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }

            return model;
        }

        public MessageTemplateModel FindMessageTemplateModel(int id, bool bCreateEmptyIfNotfound = true) {
            MessageTemplateModel model = null;

            var et = db.FindMessageTemplate(id);
            if (et == null) {
                if (bCreateEmptyIfNotfound) model = new MessageTemplateModel();

            } else {
                model = MapToModel(et);
            }

            return model;
        }

        public MessageTemplateModel FindMessageTemplateModel(int companyId, MessageTemplateType templateId, bool bCreateEmptyIfNotfound = true) {
            MessageTemplateModel model = null;

            var et = db.FindMessageTemplate(companyId, templateId);
            if (et == null) {
                if(bCreateEmptyIfNotfound) model = new MessageTemplateModel();

            } else {
                model = MapToModel(et);
            }

            return model;
        }

        public MessageTemplateModel MapToModel(MessageTemplate item) {
            var newItem = Mapper.Map<MessageTemplate, MessageTemplateModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateMessageTemplate(MessageTemplateModel template, UserModel user, string lockGuid) {
            var error = validateModel(template);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(MessageTemplate).ToString(), template.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "Subject");

                } else {
                    MessageTemplate temp = null;
                    if (template.Id != 0) temp = db.FindMessageTemplate(template.Id);
                    if (temp == null) temp = new MessageTemplate();

                    Mapper.Map<MessageTemplateModel, MessageTemplate>(template, temp);

                    db.InsertOrUpdateMessageTemplate(temp);
                    template.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteMessageTemplate(int id) {
            db.DeleteMessageTemplate(id);
        }

        public string LockMessageTemplate(MessageTemplateModel model) {
            return db.LockRecord(typeof(MessageTemplate).ToString(), model.Id);
        }

        public void CopyMessageTemplates(CompanyModel source, CompanyModel target, UserModel user) {
            foreach (var template in FindMessageTemplatesModel(source.Id)) {
                var newItem = Mapper.Map<MessageTemplateModel, MessageTemplateModel>(template);
                newItem.Id = 0;
                newItem.CompanyId = target.Id;
                InsertOrUpdateMessageTemplate(newItem, user, "");
            }
        }

        #endregion

        #region Private methods

        private Error validateModel(MessageTemplateModel model) {
            string subject = getFieldValue(model.Subject);

            var error = isValidRequiredString(subject, 128, "Subject", EvolutionResources.errMessageTemplateSubjectRequired);

            return error;
        }

        #endregion
    }
}
