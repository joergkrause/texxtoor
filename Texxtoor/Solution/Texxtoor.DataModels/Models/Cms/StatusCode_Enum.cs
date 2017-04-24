using Texxtoor.BaseLibrary.Core;
using System;

namespace Texxtoor.DataModels.Models.Cms {
  /// <summary>
  /// State of the content concerning the review and approval process.
  /// </summary>
  public enum StatusCode {
    /// <summary>
    /// Draft, internally only and not visible.
    /// </summary>
    Draft = 1,
    /// <summary>
    /// Visible to the public
    /// </summary>
    Published = 2,
    /// <summary>
    /// Not visible, usually replaced by something new
    /// </summary>
    Dropped = 3
  }

}
