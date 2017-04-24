using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Marketing {

  # region Marketing Management

  /// <summary>
  /// Add media specific data here
  /// </summary>
  [Table("ClickCount", Schema = "Marketing")]
  public class ClickCount : EntityBase {

    // where does the click comes from
    [Required]
    public ClickSourceType SourceType { get; set; }

    // simple value based counter
    // each click is "valued" with an internal currency, the click value
    // we define somewhere the value of this currency in the real world, e.g. 10 values = 1 cent
    public int Value { get; set; }

    // user this click is assigned to
    [Required]
    public User User { get; set; }

    [StringLength(100)]
    public string Action { get; set; }

    [StringLength(100)]
    public string Controller { get; set; }

  }

  # endregion

}