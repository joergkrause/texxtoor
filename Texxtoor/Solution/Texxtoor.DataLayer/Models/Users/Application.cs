using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.DataModels.Models.Users {

  [Table("Applications", Schema = "Common")]
  public class Application : EntityBase {

    [Required]
    [StringLength(128)]
    [Display(ResourceType = typeof(ModelResources), Name = "Application_ApplicationName_Application", Description="Application_ApplicationName_Application_Helptext")]
    public string ApplicationName { get; set; }

    [StringLength(512)]
    [Display(ResourceType = typeof(ModelResources), Name = "Application_Description_Description", Description="Application_Description_Description_Helptext")]
    public string Description { get; set; }

    public IList<User> Users { get; set; }

    public IList<Catalog> Catalogs { get; set; }

  }

}