using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Texxtoor.BaseLibrary.WordInterop {

  [Serializable]
  public class MapExpression {

    public MapExpression() { }
    public MapExpression(string match, string replacement) {
      Match = match;
      Replacement = replacement;
    }

    public string Match { get; set; }
    public string Replacement { get; set; }
  }


  [Serializable]
  public class MapObject : IMapObject {

    /// <summary>
    /// Forces creation of a new fragment. New fragments have a type that delivers the HTML tag in an attribute.
    /// </summary>
    public bool FragmentSplit { get; set; }

    /// <summary>
    /// If set, the processor tries to retrieve the property in Replacement with the value pulled from Match property.
    /// </summary>
    /// <remarks>
    /// Assume the word file contains two styles, "figure" and "figurecaption". If "figure" preceedes the caption element, it has a look ahead instruction.
    /// The value contains the complete operation advice: "Match" contains "Content", hence the content of the next element is pulled. "Replacement" is
    /// the instruction to replace a certain property of the current item, such as "Title". Using a "Content|Title" copies the content from next element 
    /// into the Title of the current one.
    /// Assume the file contains the opposite direction, such as "tablecaption" followed by "table". Here one can set the properties "Content|Content". The
    /// content (the actual table) is pulled from next element but the current snippet knows that it has to keep the current content as the title.
    /// </remarks>
    public MapExpression LookAheadForFragmentType { get; set; }

    /// <summary>
    /// The type written to Elements table ((Text|Image|Video)Snippet, Section)
    /// </summary>
    public string FragmentTypeName { get; set; }

    /// <summary>
    /// Tag rendered aroung content. Used only inside text fragments where is now specific snippet type.
    /// </summary>
    public string ControlData { get; set; }

    /// <summary>
    /// Optional class used to decorate the element
    /// </summary>
    public string ControlAttributes { get; set; }

    /// <summary>
    /// A regular expression and a replacement expression to handle actual content (applied to the content, not the surrounding tag).
    /// </summary>
    public MapExpression ControlExpression { get; set; }

  }

}
