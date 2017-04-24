using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Content {

  /// <summary>
  /// Users can Manage as many matrix values as they like and add to their profiles.
  /// </summary>
  /// <remarks>
  /// Each value contains a keyword, a level, and a target value. Authors can define the target and level. If the keyword is matched
  /// by the internal matching algorithm, the level and target values are used to order the results to give better matches first.
  /// While the level is used to order results, the target can be used like a filter in the production cycle.
  /// </remarks>
  [Table("ElementMatrix", Schema = "Content")]
  public class ElementMatrix : EntityBase {

    public ElementMatrix() {
      Stage = StageType.Novice;
      Target = TargetType.Professional;
    }

    /// <summary>
    /// A keyword, such as "C#". The level and target is applied to this keyword.
    /// </summary>
    [Required]
    [StringLength(256)]
    [Display(ResourceType = typeof (ModelResources), Name = "ElementMatrix_Keyword_Keyword")]
    public string Keyword { get; set; }
    /// <summary>
    /// Level. Say "Beginner" means that this user is beginner in the field "C#".
    /// </summary>
    [Required]
    [Display(ResourceType = typeof (ModelResources), Name = "ElementMatrix_Stage_Level")]
    public StageType Stage { get; set; }
    /// <summary>
    /// Target. Defines what this subject is used primarily. "Personal" means, the user uses "C#" at home, as a hobby, but not at work.
    /// </summary>
    /// <remarks>This are a flags value and can be combined. </remarks>
    [Required]
    [Display(ResourceType = typeof (ModelResources), Name = "ElementMatrix_Target_Target_Audience")]
    public TargetType Target { get; set; }

    [Required]
    public Element Element { get; set; }

  }

}
