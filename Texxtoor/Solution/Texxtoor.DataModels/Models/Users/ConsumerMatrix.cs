using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Users {

  /// <summary>
  /// Users can Manage as many matrix values as they like and add to their profiles.
  /// </summary>
  /// <remarks>
  /// Each value contains a keyword, a level, and a target value. Authors can define the target and level. If the keyword is matched
  /// by the internal matching algorithm, the level and target values are used to order the results to give better matches first.
  /// While the level is used to order results, the target can be used like a filter in the production cycle.
  /// </remarks>
  [Table("ConsumerMatrix", Schema = "Common")]
  public class ConsumerMatrix : EntityBase {

    public ConsumerMatrix() {
      Stage = StageType.Novice;
      Target = TargetType.Professional;
      Temporary = false;
    }

    /// <summary>
    /// A keyword, such as "C#". The level and target is applied to this keyword.
    /// </summary>
    [Required]
    [StringLength(256)]
    public string Keyword { get; set; }
    /// <summary>
    /// Level. Say "Beginner" means that this user is beginner in the field "C#".
    /// </summary>
    [Required]
    public StageType Stage { get; set; }
    /// <summary>
    /// Target. Defines what this subject is used primarily. "Personal" means, the user uses "C#" at home, as a hobby, but not at work.
    /// </summary>
    /// <remarks>This are a flags value and can be combined. </remarks>
    [Required]
    public TargetType Target { get; set; }

    [Required]
    public UserProfile Profile { get; set; }

    /// <summary>
    /// Used in search algorithms to identify values for search only.
    /// </summary>
    public bool Temporary { get; set; }

  }

}
