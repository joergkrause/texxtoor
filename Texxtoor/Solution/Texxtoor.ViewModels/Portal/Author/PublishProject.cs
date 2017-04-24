using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.ViewModels.Author {
  
  /// <summary>
  /// Build and manage the publishing form.
  /// </summary>
  public class PublishProject {
  
    [Required]
    public bool CanPublish { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "PublishProject_PublishableOpus_Publishable_Parts_of_Project", Description = "PublishProject_PublishableOpus_Publishable_Parts_of_Project_Helptext")]
    public Project Project { get; set; }

    //public bool HasManyProjects {
    //  get {
    //    return Projects != null && Projects.Count() > 1;
    //  }
    //}
    
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "PublishProject_OpusId_Selected_Part", Description="PublishProject_OpusId_Selected_Part_Helptext")]
    public int? OpusId { get; set; }
    
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "PublishProject_QuickPublish_Quick_Publish_Only", Description="PublishProject_QuickPublish_Quick_Publish_Only_Helptext")]
    public bool QuickPublish { get; set; }

  }
}
