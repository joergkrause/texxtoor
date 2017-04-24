using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Texxtoor.DataModels.Models.Reader.Content {

  /// <summary>
  /// The navigation level setting for published documents.
  /// </summary>
  [DefaultValue("NavLevel_Default")]
  public enum NavLevel {
    [Display(ResourceType = typeof (ModelResources), Name = "NavLevel_Document_Document")]
    Document = 0,
    [Display(ResourceType = typeof (ModelResources), Name = "NavLevel_Chapter_Chapter")]
    Chapter = 1,
    [Display(ResourceType = typeof (ModelResources), Name = "NavLevel_Section_Section")]
    Section = 2,
    [Display(ResourceType = typeof (ModelResources), Name = "NavLevel_SubSection_Sub_Section")]
    SubSection = 3

  }

}