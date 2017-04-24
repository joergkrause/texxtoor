namespace Texxtoor.Models {

  /// <summary>
  /// Classifies HTML fragments
  /// </summary>
  public enum FragmentType {
    /// <summary>
    /// Regular HTML, text, tables, listings etc.
    /// </summary>
    Html,
    /// <summary>
    /// Image element, such as &lt;img&gt; the child fragment of type Data contains the binary content.
    /// </summary>
    Image,
    /// <summary>
    /// Video element, such as &lt;img&gt; the child fragment of type Data contains the binary content.
    /// </summary>
    Video,
    /// <summary>
    /// Audio element, such as &lt;img&gt; the child fragment of type Data contains the binary content.
    /// </summary>
    Audio,
    /// <summary>
    /// The data element, that acts solely as a child fragment for images, videos etc.
    /// </summary>
    Data,
    /// <summary>
    /// Supporting CSS that applies to all elements. Shall be on top level.
    /// </summary>
    Css,
    /// <summary>
    /// Treat this like HTML, but expect it contains Script executed on the reader device.
    /// </summary>
    Script,
    /// <summary>
    /// Supporting FONT that applies to all elements. Shall be on top level.
    /// </summary>
    Font,
    /// <summary>
    /// Meta data, usually the exclusive root element for the content entity, such as an opus, book, article etc.
    /// </summary>
    Meta
  }

}