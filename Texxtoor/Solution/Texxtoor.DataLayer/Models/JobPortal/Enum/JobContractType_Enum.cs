using System;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.DataModels.Models.JobPortal {

  [Flags]
  public enum JobContractType {
    [Display(ResourceType = typeof(ModelResources), Name = "JobContractType_Permanent_Permanent", Description="JobContractType_Permanent_Permanent_Helptext")]
    Permanent = 1,
    [Display(ResourceType = typeof(ModelResources), Name = "JobContractType_Freelancer_Freelancer", Description="JobContractType_Freelancer_Freelancer_Helptext")]
    Freelancer = 2,
    [Display(ResourceType = typeof(ModelResources), Name = "JobContractType_Offsite_Offsite", Description="JobContractType_Offsite_Offsite_Helptext")]
    Offsite = 4
  }


}