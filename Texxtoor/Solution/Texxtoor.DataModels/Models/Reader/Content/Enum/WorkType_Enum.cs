using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Texxtoor.DataModels.Models.Reader.Content {

  /// <summary>
  /// The navigation level setting for published documents.
  /// </summary>
  public enum WorkType {
    [Display(ResourceType = typeof (ModelResources), Name = "WorkType_Published")]
    Published = 0,
    [Display(ResourceType = typeof (ModelResources), Name = "WorkType_External")]
    External = 1,
    [Display(ResourceType = typeof (ModelResources), Name = "WorkType_Custom")]
    Custom = 2,
    [Display(ResourceType = typeof (ModelResources), Name = "WorkType_Other")]
    Other = 99

  }

}