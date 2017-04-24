using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Content {

  [Table("Templates", Schema = "Content")]
  public class Template : EntityBase {

    [Required]
    [StringLength(120)]
    [Display(ResourceType = typeof(ModelResources), Name = "Template_InternalName_Internal_Name", Description="Template_InternalName_Internal_Name_Helptext")]
    public string InternalName { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Template_Content_Content", Description="Template_Content_Content_Helptext")]
    public byte[] Content { get; set; }

    [ScaffoldColumn(false)]
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Template_Group_Template_Group", Description="Template_Group_Template_Group_Helptext")]
    public virtual TemplateGroup Group { get; set; }

  }

}
