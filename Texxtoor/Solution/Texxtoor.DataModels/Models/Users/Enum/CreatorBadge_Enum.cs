using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Texxtoor.BaseLibrary.Core;

namespace Texxtoor.DataModels.Models.Users {

  public enum CreatorBadge {
    [Display(ResourceType = typeof (ModelResources), Name = "CreatorBadge_Beginner_Beginner")]
    Beginner = 1,
    [Display(ResourceType = typeof (ModelResources), Name = "CreatorBadge_Author_Author")]
    Author = 2,
    [Display(ResourceType = typeof (ModelResources), Name = "CreatorBadge_Maven_Maven")]
    Maven = 3,
    [Display(ResourceType = typeof (ModelResources), Name = "CreatorBadge_Specialist_Specialist")]
    Specialist = 4,
    [Display(ResourceType = typeof (ModelResources), Name = "CreatorBadge_Expert_Expert")]
    Expert = 5
  }


}
