using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Marketing {

  [Table("Imprints", Schema = "Common")]
  public class Imprint : EntityBase {

    [Required]
    [StringLength(64)]
    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_Name_Name", Description = "Imprint_Name_Name_Helptext")]
    public string Name { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_Description_Description", Description = "Imprint_Description_Description_Helptext")]
    [StringLength(512)]
    public string Description { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_AboutUs_About_Us", Description = "Imprint_AboutUs_About_Us_Helptext")]
    [StringLength(2048)]
    public string AboutUs { get; set; }

    [ScaffoldColumn(false)]
    public List<IsbnStore> Isbns { get; set; }

    [Required]
    public virtual User Owner { get; set; }

    [Required]
    public virtual AddressBook Address { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_Firm_Firm", Description = "Imprint_Firm_Firm_Helptext")]
    [Required]
    [StringLength(128)]
    public string Firm { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_Url_Website", Description = "Imprint_Url_Website_Helptext")]
    [StringLength(128)]
    public string Url { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Imprint_CompanyLogo_Logo", Description = "Imprint_CompanyLogo_Logo_Helptext")]
    public byte[] CompanyLogo { get; set; }

    [ScaffoldColumn(false)]
    public Published Published { get; set; }

  }
}
