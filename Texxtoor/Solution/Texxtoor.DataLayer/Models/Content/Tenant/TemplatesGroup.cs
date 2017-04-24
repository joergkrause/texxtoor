using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Content {

  [Table("TemplateGroups", Schema = "Content")]
  public class TemplateGroup : LocalizedEntityBase {

    public TemplateGroup() {
      Templates = new List<Template>();
    }

    [Required]
    [StringLength(120)]
    [Display(ResourceType = typeof(ModelResources), Name = "Template_Name_Template_Name", Description="Template_Name_Template_Name_Helptext")]
    [System.ComponentModel.ReadOnly(true)]
    public string Name { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Template_Admin_Responsible_Administrator", Description="Template_Admin_Responsible_Administrator_Helptext")]
    public virtual User Admin { get; set; }

    /// <summary>
    /// Tenant is optional. Templates that have no tenant relation are public for the platform.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Template_Owner_Template_Owner", Description="Template_Owner_Template_Owner_Helptext")]
    public virtual Tenant Owner { get; set; }

    [ScaffoldColumn(false)]
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Template_Group_Template_Group", Description="Template_Group_Template_Group_Helptext")]
    public GroupKind Group { get; set; }

    public virtual IList<Template> Templates { get; set; }

    public virtual IList<Published> PublishedUsings { get; set; }
    
    [ScaffoldColumn(false)]
    [NotMapped]
    public bool IsCommonTemplate {
      get { return Name == "Common Template"; }

    }

    [StringLength(800)]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 5)]
    [AdditionalMetadata("Cols", 55)]
    [Display(ResourceType = typeof(ModelResources), Name = "Template_Description_Template_Name", Description = "Template_Description_Template_Name_Helptext")]
    public string Description { get; set; }
  }

}
