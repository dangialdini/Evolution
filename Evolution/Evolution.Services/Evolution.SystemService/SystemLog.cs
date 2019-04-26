using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Enumerations;
using AutoMapper;

namespace Evolution.SystemService {
    public partial class SystemService {

        #region Public members

        public LogListModel FindLogListModel(int index, int pageNo, int pageSize,
                                             string search, int section, int severity,
                                             DateTimeOffset? dateFrom, DateTimeOffset? dateTo) {
            var model = new LogListModel();

            model.GridIndex = index;
            var allItems = db.FindLogs(search, section, severity, dateFrom, dateTo)
                             .ToList();

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                         .Take(pageSize)) {
                LogModel newItem = mapToModel(item);

                switch ((LogSeverity)item.Severity) {
                case LogSeverity.Warning:
                    newItem.Colour = "#0000FF";
                    break;
                case LogSeverity.Severe:
                    newItem.Colour = "#FF0000";
                    break;
                default:
                    newItem.Colour = "#000000";
                    break;
                }

                model.Items.Add(newItem);
            }

            return model;
        }

        LogModel mapToModel(Log item) {
            var newItem = Mapper.Map<Log, LogModel>(item);

            newItem.SeverityText = Enum.GetName(typeof(LogSeverity), item.Severity);

            newItem.Message = item.Url;
            if (!string.IsNullOrEmpty(newItem.Message)) newItem.Message += "<br/><br/>";
            newItem.Message += item.Message;
            if (!string.IsNullOrEmpty(item.StackTrace)) newItem.Message += "<br/><br/>" + item.StackTrace;

            return newItem;
        }

        #endregion
    }
}
