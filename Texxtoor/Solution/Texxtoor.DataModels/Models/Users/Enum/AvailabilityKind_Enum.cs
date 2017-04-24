using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Texxtoor.DataModels.Models.Users {

  /// <summary>
  /// User can define what kind of availability time frame they want to define
  /// </summary>
  public enum AvailabilityKind {

    [Display(Name = "Free available for work")]
    FreeForWork,
    [Display(Name = "Partially available")]
    PartiallyAvailable,
    [Display(Name = "Regular worktime")]
    Worktime,
    [Display(Name = "Off for vacation")]
    OffForVacation,
    [Display(Name = "Off due to illness")]
    OffDueToIllness,
    [Display(Name = "Off, reason not specified")]
    OffNotSpecified,
    [Display(Name = "Available, reason not specified")]
    AvailableNotSpecified

  }
}
