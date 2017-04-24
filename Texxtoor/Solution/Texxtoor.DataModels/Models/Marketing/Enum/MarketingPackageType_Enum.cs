using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Texxtoor.BaseLibrary.Core;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.DataModels.Models.Marketing {
  /// <summary>
  /// Type of package. Manages the behavior of content within the platform.
  /// </summary>
  [Flags]
  public enum MarketingPackageType {
    /// <summary>
    /// Content is free (Public Domain)
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "MarketingPackageType_PublicDomain_Public_Domain")]
    PublicDomain = 1,
    /// <summary>
    /// Content is free (Creative Common)
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "MarketingPackageType_CreativeCommon_Creative_Common")]
    CreativeCommon = 2,
    /// <summary>
    /// Content is free (Ad Supported)
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "MarketingPackageType_AdSupported_Ad_supported")]
    AdSupported = 4,
    /// <summary>
    /// Regular sale on a "per piece" base
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "MarketingPackageType_RegularSale_Regular_Sale")]
    RegularSale = 8,
    /// <summary>
    /// Content can participate in a subscription model
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "MarketingPackageType_SubscriptionFee_With_Subscription_Fee")]
    SubscriptionFee = 16,
    /// <summary>
    /// Content is provided under some sort of custom level defined elsewhere
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "MarketingPackageType_CustomLevel_Custom_Level")]
    CustomLevel = 32,
    /// <summary>
    /// Content is closed and does not participate in any sales model
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "MarketingPackageType_Closed_Closed_Inactive")]
    Closed = 64
  }

}
