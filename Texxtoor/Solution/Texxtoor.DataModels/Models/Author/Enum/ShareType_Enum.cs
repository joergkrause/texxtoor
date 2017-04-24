using System.ComponentModel.DataAnnotations;
using Texxtoor.BaseLibrary.Core;

namespace Texxtoor.DataModels.Models.Author {

  /// <summary>
  /// Defines how a contributor is being paid for specific work
  /// </summary>
  public enum ShareType {
    /// <summary>
    /// No payment
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "ShareType_Free_Free")]
    Free = 1,
    /// <summary>
    /// The Amount field contains the hourly rate
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "ShareType_Hourly_Hourly")]
    Hourly = 2,
    /// <summary>
    /// The Amount field contains the daily rate
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "ShareType_Daily_Daily")]
    Daily = 3,
    /// <summary>
    /// The Amount field contains the weekly rate
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "ShareType_Weekly_Weekly")]
    Weekly = 4,
    /// <summary>
    /// The Amount field contains the monthly rate
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "ShareType_Monthly_Monthly")]
    Monthly = 5,
    /// <summary>
    /// The Amount field contains a fixed one-time payment amount
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "ShareType_Fixed_Fixed")]
    Fixed = 6,
    /// <summary>
    /// The Amount field contains a rate (share) in percent
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "ShareType_Ratio_Ratio")]
    Ratio = 7
  }

  
}