using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.ViewModels.Content {

  /// <summary>
  /// ViewModel for first time quickstart
  /// </summary>
  public class QuickProject {

    public QuickProject() {
      UseDefaults = true;
    }

    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "QuickCreate_Name_You_must_provide_a_name")]
    [Display(ResourceType = typeof(ModelResources), Name = "QuickCreate_Name_Content_Name", Description = "QuickCreate_Name_Content_Name_Helptext")]
    public string Name { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "QuickCreate_UseDefaults_Use_default_settings", Description="QuickCreate_UseDefaults_Use_default_settings_Helptext")]
    public bool UseDefaults { get; set; }

  }

  public class QuickProjectAdvanced {

    [StringLength(255)]
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Project_Short_Short_Description", Description = "Project_Short_Short_Description_Helptext")]
    [AdditionalMetadata("Length", 55)]
    public string ShortName { get; set; }

    [StringLength(2500)]
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Project_Description_Verbose_Description", Description = "Project_Description_Verbose_Description_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 55)]
    public string Description { get; set; }

  }

}
