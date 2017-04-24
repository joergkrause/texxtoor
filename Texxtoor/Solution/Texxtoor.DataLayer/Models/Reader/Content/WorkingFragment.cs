using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Reader.Content {

  /// <summary>
  /// Store of a relation to a frozen fragment for reference in several stages.
  /// </summary>
  /// <remarks>
  /// This class stores fragments in several hierarchies, related to the frozen fragments. References in Collections, Comments, Bookmarks etc.
  /// </remarks>
  [Table("WorkingFragments", Schema = "Reader")]
  public class WorkingFragment : HierarchyBase<WorkingFragment> {
    
    [NotMapped]
    [ScaffoldColumn(false)]
    public override string Name { get; set; }

    /// <summary>
    /// The entity in the work. While everything can be re-arranged, the frozen fragments remain static forever.
    /// </summary>
    [Required]
    public virtual FrozenFragment FrozenFragment { get; set; }

    # region --== Navigation Properties ==--

    /// <summary>
    /// Work this fragment belongs to. This is a 1:n relation, if the same fragment is needed twice, we'll copy it.
    /// </summary>
    public Work Work { get; set; }

    # endregion

  }


}