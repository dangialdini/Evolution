using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Evolution.Resources;

namespace Evolution.Models.Models {
    public class NewCustomerModel {
        [Required]
        [StringLength(255)]
        [Display(Name = "lblName", ResourceType = typeof(EvolutionResources))]
        public string Name { set; get; } = "";
        [StringLength(30)]
        [Display(Name = "lblTaxId", ResourceType = typeof(EvolutionResources))]
        public string TaxId { set; get; } = "";
        [StringLength(255)]
        [Display(Name = "lblStreet", ResourceType = typeof(EvolutionResources))]
        public string Street { set; get; } = "";
        [StringLength(50)]
        [Display(Name = "lblCity", ResourceType = typeof(EvolutionResources))]
        public string City { set; get; } = "";
        [StringLength(20)]
        [Display(Name = "lblState", ResourceType = typeof(EvolutionResources))]
        public string State { set; get; } = "";
        [Required]
        [StringLength(10)]
        [Display(Name = "lblPostCode", ResourceType = typeof(EvolutionResources))]
        public string Postcode { set; get; } = "";
        [Required]
        [Display(Name = "lblCountryName", ResourceType = typeof(EvolutionResources))]
        public int CountryId { set; get; } = 0;
        public int CustomerTypeId { set; get; } = 0;
        public int RegionId { set; get; } = 0;
    }
}
