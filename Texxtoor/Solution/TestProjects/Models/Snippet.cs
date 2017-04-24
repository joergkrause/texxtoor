using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;

namespace LinqDemo.Models {

  [Table("Elements")]
  public abstract class Snippet : Element {
    protected Snippet() {
      State = State.Unknown;
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

  }


}
