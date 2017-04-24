using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Users {

  /// <summary>
  /// Stores activity points to calculate the gold, silver, and member status' of the users
  /// </summary>
  [Table("UserActivity", Schema = "Common")]
  public class UserActivity : EntityBase {

    public UserActivity() {
      Aggregated = false;
      OperationValue = 1;
      OperationReason = "Common Platform Activity";
    }

    // The user to this activity applies
    [Required]
    public User ActivityUser { get; set; }

    [Required]
    [Range(1,100)]
    public int OperationValue { get; set; }

    [Required]
    [StringLength(80)]
    public string OperationReason { get; set; }

    /// <summary>
    /// Set to true if the value is an aggregated (summarized) value from recent activity. Used to reduced table work load.
    /// </summary>
    public bool Aggregated { get; set; }

  }

}