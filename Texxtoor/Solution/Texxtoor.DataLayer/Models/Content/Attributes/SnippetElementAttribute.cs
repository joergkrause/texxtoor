using System;
using System.Collections.Generic;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.DataModels.Attributes {

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
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class SnippetBuilderAttribute : Attribute {

    public SnippetBuilderAttribute(string pattern, params string[] properties) {
      Properties = properties;
      HtmlPattern = pattern;
    }

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
    /// <returns>The final HTML string</returns>
    public string BuildHtml(Element snippet, IDictionary<string, NumberingSchema> numbering) {
      if (String.IsNullOrEmpty(HtmlPattern)) {
        return snippet.RawContent;
      }
      var t = snippet.GetType().BaseType == null ? snippet.GetType().Name : snippet.GetType().BaseType.Name;
      if (numbering != null && snippet is NumberedSnippet) {
        if (numbering.ContainsKey(t)) {
          // set regular snippets
          ((NumberedSnippet)snippet).Divider = numbering[t].Divider;
          ((NumberedSnippet)snippet).Minor = numbering[t].Minor;
          ((NumberedSnippet)snippet).Label = numbering[t].Label;
          ((NumberedSnippet)snippet).Separator = numbering[t].Separator;
          ((NumberedSnippet)snippet).Major = numbering[t].Major;
          numbering[t].Minor = numbering[t].Minor + 1;
        } else {
          // special treatment for sections
          if (t == "Section") {
            var d = ((Section)snippet).Level - 1;
            if (numbering.ContainsKey(t + d)) {
              var s = t + d;
              // we have a definition for this level
              ((NumberedSnippet)snippet).Divider = numbering[s].Divider;
              ((NumberedSnippet)snippet).Minor = numbering[s].Minor;
              ((NumberedSnippet)snippet).Label = numbering[s].Label;
              ((NumberedSnippet)snippet).Separator = numbering[s].Separator;
              ((NumberedSnippet)snippet).Major = numbering[s].Major;
              var pn = String.Empty;
              var o = d;
              while (d > 2) {
                if (numbering[t + d].IncludeParent) {
                  d--;
                  pn = String.Format("{0}{1}", numbering[t + d].Minor - 1, numbering[t + d].Separator) + pn;
                }
                else {
                  break;
                }
              }
              if (o > 1) {
                ((Section)snippet).ParentNumbering = String.Format("{0}{1}{2}", numbering[s].Major, numbering[s].Separator, pn);
              } else {
                ((NumberedSnippet) snippet).Minor = 0;
                ((Section)snippet).ParentNumbering = String.Format("{0}", numbering[s].Major);
              }
              numbering[s].Minor = numbering[s].Minor + 1;              
            }
          }
        }
      }

      var type = snippet.GetType();
      var propVals = new object[Properties.Length];
      for (var i = 0; i < propVals.Length; i++) {
        var prop = type.GetProperty(Properties[i]);
        propVals[i] = prop.GetValue(snippet);
      }
      return String.Format(HtmlPattern, propVals);
    }
  }


}
