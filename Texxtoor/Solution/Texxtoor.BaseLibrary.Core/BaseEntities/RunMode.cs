using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texxtoor.DataModels;

namespace Texxtoor.DataModels.Models.Common {

  public enum Complexity {

    /// <summary>
    /// Simplified mode with minimum viable set of features.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Complexity_Enum_Simple")]
    Simple = 0,
    /// <summary>
    /// Regular plattform mode with all features.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Complexity_Enum_Full")]
    Full = 1
  }

  /// <summary>
  /// The mode the platform runs in. Set through the <see cref="RunControl"/> object.
  /// </summary>
  public enum RunMode {

    /// <summary>
    /// Texxtoor.de public plattform.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "RunMode_Enum_Texxtoor")]
    Texxtoor = 0,

    /// <summary>
    /// B2B platform for internal usage (AC² behavior)
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "RunMode_Enum_Business")]
    Business = 1,

    /// <summary>
    /// MyManuals Platform
    /// </summary>
    MyManuals = 2
  }
}
