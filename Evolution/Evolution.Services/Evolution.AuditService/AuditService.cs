using System;
using System.IO;
using System.Linq;
using Evolution.DAL;
using Evolution.Enumerations;
using Evolution.Models.Models;
using AutoMapper;

namespace Evolution.AuditService {
    public class AuditService {
        // This service is not derived from the common service because the common
        // service requires this service which would create a circular reference

        #region Protected members

        EvolutionEntities db;

        #endregion

        #region Construction

        protected IMapper Mapper = null;

        public AuditService(EvolutionEntities dbEntities) {
            db = dbEntities;

            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<AuditLog, AuditModel>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion

        #region Public methods

        public void LogChanges(string tableName, string businessArea, UserModel user,
                                Object before, Object after) {
            // The source and destination objects must be of the same type
            // as this method is comparing 'before' and 'after'
            int beforeRowId = 0,
                afterRowId = 0;
            string tempTable = normaliseTableName(tableName);

            if (before != null) {
                var srcT = before.GetType();
                var srcFId = srcT.GetProperties()
                                 .Where(f => f.Name == "Id")
                                 .FirstOrDefault();
                beforeRowId = Convert.ToInt32(srcFId.GetValue(before));
            }

            if (after != null) {
                var dstT = after.GetType();
                var dstFId = dstT.GetProperties()
                                 .Where(f => f.Name == "Id")
                                 .FirstOrDefault();
                afterRowId = Convert.ToInt32(dstFId.GetValue(after));
            }

            if (beforeRowId == 0) {
                if(afterRowId > 0) db.InsertAuditLog(tempTable, businessArea, afterRowId, user.Id, "", "", "Record created");

            } else if(after != null) {
                var srcT = before.GetType();
                var dstT = after.GetType();

                foreach (var f in srcT.GetFields()) {
                    if (f.FieldType.IsValueType || f.FieldType == typeof(string)) {
                        var dstF = dstT.GetField(f.Name);
                        if (dstF != null) {
                            var beforeValue = f.GetValue(before);
                            if (beforeValue == null) beforeValue = "";
                            var afterValue = dstF.GetValue(after);
                            if (afterValue == null) afterValue = "";

                            if (!beforeValue.Equals(afterValue)) {
                                // Value has changed
                                db.InsertAuditLog(tempTable, businessArea, afterRowId, user.Id, f.Name, beforeValue.ToString(), afterValue.ToString());
                            }
                        }
                    }
                }

                foreach (var f in srcT.GetProperties()) {
                    if (f.PropertyType.IsValueType || f.PropertyType == typeof(string)) {
                        var dstF = dstT.GetProperty(f.Name);
                        if (dstF != null) {
                            var beforeValue = f.GetValue(before);
                            if (beforeValue == null) beforeValue = "";
                            var afterValue = dstF.GetValue(after);
                            if (afterValue == null) afterValue = "";

                            if (!beforeValue.Equals(afterValue)) {
                                // Value has changed
                                db.InsertAuditLog(tempTable, businessArea, afterRowId, user.Id, f.Name, beforeValue.ToString(), afterValue.ToString());
                            }
                        }
                    }
                }
            }
        }

        public AuditListModel FindAuditListModel(string tableName, int rowId = -1) {
            var model = new AuditListModel();

            string tempTable = normaliseTableName(tableName);

            // Do a case-insensitive search
            var x = db.FindAuditLogs(tempTable, rowId).ToList();
            foreach (var item in db.FindAuditLogs(tempTable, rowId)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public AuditModel MapToModel(AuditLog item) {
            AuditModel newItem = Mapper.Map<AuditLog, AuditModel>(item);
            return newItem;
        }

        private string normaliseTableName(string tableName) {
            string tempTable = tableName;

            int pos = tableName.LastIndexOf(".");
            if (pos != -1) tempTable = tempTable.Substring(pos + 1);

            return tempTable;
        }

        #endregion
    }
}
