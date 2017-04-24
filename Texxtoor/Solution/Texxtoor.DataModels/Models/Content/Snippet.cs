using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.DataModels.Models.Content {

  [Table("Elements", Schema = "Content")]
  public abstract class Snippet : Element {
    protected Snippet() {
      State = Models.Content.State.Unknown;
    }

    /// <summary>
    /// This element has alternatives or variations, new versions or other special occurences. 
    /// </summary>
    /// <remarks>
    /// The variations are stored as children of this element. Usually the type "Snippet" is a leaf element. If this property is true,
    /// it's allowed to add children. The UI will still stay on top level but may allow the author to switch between variations.
    /// For production it's not allowed to use many of these, moreover, some sort of filter must peek exactly on value from the 
    /// downlevel hierarchy. See also VariationType enum for values that determine the sort of variation for a leaf element's children.
    /// </remarks>
    [NotMapped]
    public bool HasVariations { get { return HasChildren(); } }

    /// <summary>
    /// Several UI functions need to know that the work on this element is in progress or done.
    /// </summary>
    /// <remarks>
    /// The translation tool uses this to remember that an automated translation has been applied and does not need to be invoked again.
    /// </remarks>
    public State State { get; set; }

    /// <summary>
    /// The relation of an element's content to its predecessors. 
    /// </summary>
    /// <remarks>
    /// On opus level it's used to organize documents. On snippet level it's used to help the translation tool to manage relations between translated snippets.
    /// </remarks>
    public virtual Snippet Predecessor { get; set; }


    /// <summary>
    /// This supports the builder by providiung a way to create content different for target channels.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="targetFragment">Optionally attach the content of resources to frozen fragment. Can be <c>null</c> if called from preview.</param>
    /// <returns></returns>
    [NotMapped]
    public abstract string this[GroupKind target, FrozenFragment targetFragment] { get; }

    /// <summary>
    /// Create this Element as HTML and process down with all children.
    /// </summary>
    /// <param name="snippets">Create HTML for these snippets in distinct order, section resolve their children independently.</param>
    /// <param name="target">The current production target the builder is working for.</param>
    /// <param name="targetFragment"></param>
    /// <returns></returns>
    protected string CreateChildren(IEnumerable<Snippet> snippets, GroupKind target, FrozenFragment targetFragment) {
      Element opus = this;
      do {
        opus = opus.Parent;
      } while (!(opus is Opus));
      var numbering = ((Opus)opus).Numbering;
      var flatContent = new StringBuilder();
      Action<Snippet> convert = null;
      convert = e => {
        var builder = e.GetType().GetCustomAttributes(typeof(SnippetBuilderAttribute), true).OfType<SnippetBuilderAttribute>().First(sb => sb.Target == target);
        flatContent.Append(builder.BuildHtml(e, numbering, targetFragment));
      };
      snippets.OrderBy(s => s.OrderNr).ToList().ForEach(c => convert(c));
      return flatContent.ToString();
    }

    /// <summary>
    /// Supports creation of TOC and Index as well as hyperlink references.
    /// </summary>
    [NotMapped]
    public virtual string BuilderId {
      get { return String.Format("{1}-{0}", Id, WidgetName.ToLower()); }
    }

  }

  [Table("Elements", Schema = "Content")]
  public abstract class TitledSnippet : NumberedSnippet {
    // used to have titles for images, video, audio, listings, and everything else numerable
    public string Title { get; set; }

  }
}
