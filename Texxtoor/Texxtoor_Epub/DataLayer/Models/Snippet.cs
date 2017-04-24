using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.Editor.Models;

namespace Texxtoor.Models {

  /// <summary>
  /// An abstract marker class
  /// </summary>
  [Table("Elements")]
  public abstract class Snippet : Element {
  }

 [Table("Elements")]
  public abstract class TitledSnippet : NumberedSnippet {
    // used to have titles for images, video, audio, listings, and everything else numerable
    public string Title { get; set; }

  }
}
