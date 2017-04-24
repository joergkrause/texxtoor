using System.ComponentModel.DataAnnotations;
using Texxtoor.BaseLibrary.Core;

namespace Texxtoor.DataModels.Models.Content {

  /// <summary>
  /// The type of sidebar, controls the render behavior.
  /// </summary>
  public enum SidebarType {
    [Display(ResourceType = typeof (ModelResources), Name = "SidebarType_Note_Note")]
    Note = 1,
    [Display(ResourceType = typeof (ModelResources), Name = "SidebarType_Tip_Tip")]
    Tip = 2,
    [Display(ResourceType = typeof (ModelResources), Name = "SidebarType_Warning_Warning")]
    Warning = 3,
    [Display(ResourceType = typeof (ModelResources), Name = "SidebarType_Information_Information")]
    Information = 4,
    [Display(ResourceType = typeof (ModelResources), Name = "SidebarType_Box_Box")]
    Box = 5,
    [Display(ResourceType = typeof (ModelResources), Name = "SidebarType_Advice_Advice")]
    Advice = 6,
    [Display(ResourceType = typeof (ModelResources), Name = "SidebarType_Custom_Custom")]
    Custom = 99
  }


 
}
