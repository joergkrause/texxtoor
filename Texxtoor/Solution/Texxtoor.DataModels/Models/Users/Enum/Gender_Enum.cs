using System.ComponentModel.DataAnnotations;

namespace Texxtoor.DataModels.Models {

  /// <summary>
  /// For profile dialog
  /// </summary>
  public enum Gender {
    [Display(ResourceType = typeof (ModelResources), Name = "Gender_Male_Male")]
    Male = 1,
    [Display(ResourceType = typeof (ModelResources), Name = "Gender_Female_Female")]
    Female = 2,
    [Display(ResourceType = typeof (ModelResources), Name = "Gender_Unknown_Unknown")]
    Unknown = 3,
    [Display(ResourceType = typeof (ModelResources), Name = "Gender_Unsure_Unsure")]
    Unsure = 4,
    [Display(ResourceType = typeof (ModelResources), Name = "Gender_Both_Both")]
    Both = 5
  }

}