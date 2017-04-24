using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.DataModels.Models.Content {

  /// <summary>
  /// A helper attribute that creates the final HTML for abstract content snippets.
  /// </summary>
  /// <remarks>
  /// This is used when building <see cref="FrozenFragment"/> objects. While snippets contain
  /// a partly abstract level of content the <see cref="FrozenFragment"/>s have already valid
  /// HTML 5 in it. To support the conversion the attribute helds the builder instructions.
  /// It's being invoked in the <see cref="FrozenFragment"/>s static helper methods called
  /// while the publishing procedure is running.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  public class SnippetBuilderAttribute : Attribute {

    public SnippetBuilderAttribute(GroupKind target, string pattern, params string[] properties) {
      Target = target;
      Properties = properties;
      HtmlPattern = pattern;
    }

    public GroupKind Target { get; set; }

    /// <summary>
    /// The HTML 5 template with placeholders.
    /// </summary>
    public string HtmlPattern { get; set; }

    /// <summary>
    /// The named properties from that the values are being inserted in the template.
    /// </summary>
    public string[] Properties { get; set; }

    /// <summary>
    /// Forces the builder to gather binary resources, such as images, from child data snippets.
    /// </summary>
    public bool CreateResource { get; set; }

    /// <summary>
    /// The builder itself, creates the HTML from property data using reflection.
    /// </summary>
    /// <param name="snippet">The source snippet</param>
    /// <param name="numbering"> </param>
    /// <param name="targetFragment"></param>
    /// <returns>The final HTML string</returns>
    public string BuildHtml(Element snippet, IDictionary<string, NumberingSchema> numbering, FrozenFragment targetFragment) {
      if (String.IsNullOrEmpty(HtmlPattern)) {
        return snippet.RawContent;
      }
      var t = snippet.WidgetName;
      if (numbering != null) {
        if (numbering.ContainsKey(t) && snippet is NumberedSnippet) {
          // set regular snippets
          ((NumberedSnippet)snippet).Divider = numbering[t].Divider;
          ((NumberedSnippet)snippet).Minor = numbering[t].Minor;
          ((NumberedSnippet)snippet).Label = numbering[t].Label;
          ((NumberedSnippet)snippet).Separator = numbering[t].Separator;
          ((NumberedSnippet)snippet).Major = numbering[t].Major;
          numbering[t].Minor = numbering[t].Minor + 1;
        } else {
          // special treatment for sections
          if (snippet is Section) {
            var d = snippet.Level - 1;
            var s = t + d;
            ((Section)snippet).SetCounterString(numbering[s].Separator);
          }
        }
      }
      // assign properties to the builder string
      var type = snippet.GetType();
      var propVals = new object[Properties.Length];
      for (var i = 0; i < propVals.Length; i++) {
        PropertyInfo prop = null;
        try {
          prop = type.GetProperty(Properties[i]);
          if (prop == null) throw new ArgumentOutOfRangeException("Builder Attribute provided wrong properties for object of type " + type);
          if (prop.GetIndexParameters().Length > 0) {
            propVals[i] = prop.GetValue(snippet, new object[] { Target, targetFragment });
          } else {
            propVals[i] = prop.GetValue(snippet);
          }
        } catch (Exception ex) {
          Debug.WriteLine(ex.Message, prop == null ? propVals[i] : prop.Name);
        }
      }
      return String.Format(HtmlPattern, propVals);
    }
  }


}
