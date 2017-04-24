using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Reader.Functions {

  /// <summary>
  /// Save private and public comments for each fragment. Might build an hierarchy by itself to have forum like threads.
  /// </summary>
  [Table("Bookmarks", Schema = "Reader")]
  public class Bookmark : EntityBase {

    /// <summary>
    /// Fragment this bookmark is associated to.
    /// </summary>
    public string FragmentHref { get; set; }

    /// <summary>
    /// The associated work
    /// </summary>
    public Work Work { get; set; }

    /// <summary>
    /// The user who owns the bookmark.
    /// </summary>
    public User Owner { get; set; }

  }

}