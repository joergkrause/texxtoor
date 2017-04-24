using System;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.DataModels.Models.JobPortal {

  [Flags]
  public enum JobStatuteType {
    [Display(ResourceType = typeof(ModelResources), Name = "JobStatuteType_FullTime_Full_Time", Description="JobStatuteType_FullTime_Full_Time_Helptext")]
    FullTime = 1,
    [Display(ResourceType = typeof(ModelResources), Name = "JobStatuteType_PartTime_Part_Time", Description="JobStatuteType_PartTime_Part_Time_Helptext")]
    PartTime = 2,
    [Display(ResourceType = typeof(ModelResources), Name = "JobStatuteType_OnRequest_On_Request", Description="JobStatuteType_OnRequest_On_Request_Helptext")]
    OnRequest = 4
  }

}