using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Author {

  /// <summary>
  /// This table keeps the ratio of all shares between authors and contributors.
  /// </summary>
  /// <remarks>
  /// This table is used twice: With a relation to a project the negotiated "base ratio" is stored here.
  /// Each publish procedure creates a new entry, which is either a copy or slightly changed one of the base ratio entry.
  /// 
  /// </remarks>
  [Table("ContributorRatio", Schema = "Author")]
  public class ContributorRatio : EntityBase {

    /// <summary>
    /// User for this share.
    /// </summary>
    [Required]
    public virtual User Contributor { get; set; }

    /// <summary>
    /// Optional set when the Opus gets published.
    /// </summary>
    public virtual Published Published { get; set; }

    /// <summary>
    /// Book to which this ratio applies
    /// </summary>
    public virtual Opus Opus { get; set; }

    /// <summary>
    /// User must confirm this ratio and we store this in this field. If it is not a share between lead and contributor, a fixed or h
    /// </summary>
    public ShareType ShareType { get; set; }

    /// <summary>
    /// The actual ratio in percent or the amount per hour, day, week, and so on (depends on <see cref="ShareType"/> property).
    /// </summary>
    [Range(0D, 10000D)]
    public decimal ValueOrRatio { get; set; }

    public bool Confirmed { get; set; }

    public string GetLocalizedShareType() {
      return typeof (ShareType).GetField(ShareType.ToString()).GetCustomAttributes(typeof (DisplayAttribute), true).Cast<DisplayAttribute>().Single().GetName();
    }
  }

}
