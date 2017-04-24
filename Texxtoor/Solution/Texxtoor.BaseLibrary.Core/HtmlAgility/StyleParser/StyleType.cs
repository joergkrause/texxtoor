namespace Texxtoor.BaseLibrary.Core.HtmlAgility.StyleParser {
  /// <summary>
  /// To check what type of attribute it is.
  /// </summary>
  public enum StyleType {
    /// <summary>
    /// Attribute is a color.
    /// </summary>
    Color = 0,
    /// <summary>
    /// Attribute is a unit.
    /// </summary>
    Unit = 1,
    /// <summary>
    /// Attribute is a list (any number of values, comma separated).
    /// </summary>
    List = 2,
    /// <summary>
    /// Attribute is a property (any string value).
    /// </summary>
    Property = 3,
    /// <summary>
    /// Attribute is a combination of Property, Color and Unit (in any order).
    /// </summary>
    Group = 4,
    /// <summary>
    /// Multiple values, such as "1px 0px 2px 5px" in padding or margin
    /// </summary>
    MultiGroup = 5
  }
}
