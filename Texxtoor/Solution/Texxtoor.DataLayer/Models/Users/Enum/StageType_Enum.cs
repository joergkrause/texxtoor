using System;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.DataModels.Models.Users {

  [Flags]
  public enum StageType {
    [Display(ResourceType = typeof(ModelResources), Name = "StageType_Novice_Novice", Description="StageType_Novice_Novice_Helptext")]
    Novice = 1,
    [Display(ResourceType = typeof(ModelResources), Name = "StageType_Beginner_Beginner", Description="StageType_Beginner_Beginner_Helptext")]
    Beginner = 2,
    [Display(ResourceType = typeof(ModelResources), Name = "StageType_Mediocre_Mediocre", Description="StageType_Mediocre_Mediocre_Helptext")]
    Mediocre = 4,
    [Display(ResourceType = typeof(ModelResources), Name = "StageType_Advanced_Advanced", Description="StageType_Advanced_Advanced_Helptext")]
    Advanced = 8,
    [Display(ResourceType = typeof(ModelResources), Name = "StageType_Expert_Expert", Description="StageType_Expert_Expert_Helptext")]
    Expert = 16
  }

}
