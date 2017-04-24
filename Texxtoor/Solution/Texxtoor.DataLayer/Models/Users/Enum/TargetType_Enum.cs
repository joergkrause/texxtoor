using System;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.DataModels.Models.Users {

  [Flags]
  public enum TargetType {
    [Display(ResourceType = typeof(ModelResources), Name = "TargetType_Personal_Personal", Description="TargetType_Personal_Personal_Helptext")]
    Personal = 1,
    [Display(ResourceType = typeof(ModelResources), Name = "TargetType_School_School", Description="TargetType_School_School_Helptext")]
    School = 2,
    [Display(ResourceType = typeof(ModelResources), Name = "TargetType_Study_Study", Description="TargetType_Study_Study_Helptext")]
    Study = 4,
    [Display(ResourceType = typeof(ModelResources), Name = "TargetType_Professional_Professional", Description="TargetType_Professional_Professional_Helptext")]
    Professional = 8,
    [Display(ResourceType = typeof(ModelResources), Name = "TargetType_Qualification_Qualification", Description="TargetType_Qualification_Qualification_Helptext")]
    Qualification = 16
  }

}
