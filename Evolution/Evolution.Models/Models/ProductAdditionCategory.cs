using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class ProductAdditionalCategoryModel {
        public int Id { set; get; }
        public int? CategoryId { set; get; }
        public int? FormatId { set; get; }
        public int? FormatTypeId { set; get; }
        public int? SeasonId { set; get; }
        public int? PackingTypeId { set; get; }
        public int? KidsAdultsId { set; get; }
        public int? AgeGroupId { set; get; }
        public int? DvlptTypeId { set; get; }
        public int? PCProductId { set; get; }
        public int? PCDvlptId { set; get; }
    }
}
