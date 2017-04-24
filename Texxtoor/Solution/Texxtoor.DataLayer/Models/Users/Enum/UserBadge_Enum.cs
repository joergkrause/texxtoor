using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Texxtoor.BaseLibrary.Core;

namespace Texxtoor.DataModels.Models.Users {

  public enum UserBadge {
    [Display(ResourceType = typeof(ModelResources), Name = "UserBadge_Rookie_Rookie")]
    Rookie = 1,
    [Display(ResourceType = typeof(ModelResources), Name = "UserBadge_Member_Member")]
    Member = 2,
    [Display(ResourceType = typeof(ModelResources), Name = "UserBadge_Silver_Silver")]
    Silver = 3,
    [Display(ResourceType = typeof(ModelResources), Name = "UserBadge_Gold_Gold")]
    Gold = 4,
    [Display(ResourceType = typeof(ModelResources), Name = "UserBadge_Platinum_Platinum")]
    Platinum = 5
  }


}
