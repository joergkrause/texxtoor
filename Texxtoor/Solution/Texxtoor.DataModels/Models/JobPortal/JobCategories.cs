using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.JobPortal {

  /// <summary>
  /// Categories in which job adds can be placed - limited to media world.
  /// </summary>
  [Table("JobCategory", Schema = "JobPortal")]
  public class JobCategory : LocalizedEntityBase {

    [Required]
    [StringLength(120)]
    [Display(ResourceType = typeof(ModelResources), Name = "JobCategory_Name_Category_Name", Description="JobCategory_Name_Category_Name_Helptext")]
    public string Name { get; set; }

    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "JobCategory_Description_Description", Description="JobCategory_Description_Description_Helptext")]
    public string Description { get; set; }

    [ScaffoldColumn(false)]
    public List<JobAdvertisment> JobAdvertisments { get; set; }

  }

}