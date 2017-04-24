using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.DataModels.Models.Reader.Orders;

namespace Texxtoor.DataModels.Models.Marketing {

  # region Marketing Management

  /// <summary>
  /// Add media specific data here
  /// </summary>
  [Table("SalesTracking", Schema = "Marketing")]
  public class SalesTracking : EntityBase {


    public SalesTracking() {
      Part = 100F;
    }

    // the user the sales action is assigned to
    [Required]
    public User Author { get; set; }

    // what sale was the reason for this sales entry
    [Required]
    public OrderProduct Source { get; set; }

    // a percentage value
    [Range(0, 100)]
    public float Part { get; set; }

  }

  # endregion
  
}