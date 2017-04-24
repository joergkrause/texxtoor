using System.ComponentModel.DataAnnotations;

namespace Texxtoor.DataModels.Models.Users {

  public enum LanguageLevel {
    [Display(ResourceType = typeof(ModelResources), Name = "LanguageLevel_Native_Native_Speaker", Description="LanguageLevel_Native_Native_Speaker_Helptext")]
    Native = 0,
    [Display(ResourceType = typeof(ModelResources), Name = "LanguageLevel_Beginner_Beginner", Description="LanguageLevel_Beginner_Beginner_Helptext")]
    Beginner = 1,
    [Display(ResourceType = typeof(ModelResources), Name = "LanguageLevel_Intermediate_Intermediate", Description="LanguageLevel_Intermediate_Intermediate_Helptext")]
    Intermediate = 2,
    [Display(ResourceType = typeof(ModelResources), Name = "LanguageLevel_Advanced_Advanced", Description="LanguageLevel_Advanced_Advanced_Helptext")]
    Advanced = 3,
    [Display(ResourceType = typeof(ModelResources), Name = "LanguageLevel_Fluent_Fluent", Description="LanguageLevel_Fluent_Fluent_Helptext")]
    Fluent = 4
  }


}
