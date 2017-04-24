using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.BaseLibrary.Core.HtmlAgility.Pack;
using Texxtoor.BaseLibrary.Converters;
using Texxtoor.DataModels.Helper;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.BaseLibrary.Pdf;

namespace Texxtoor.BusinessLayer {


  /// <summary>
  /// This class contains all methods to create actual content, such as PDF, EPub, iBook, and more.
  /// </summary>
  public partial class ProductionManager {

    /// <summary>
    /// Takes an uploaded HTML document and converts directly into a published work with frozen fragments.
    /// </summary>
    /// <param name="html">HTML</param>
    /// <param name="resources">The resources the HTML needs, such as images.</param>
    /// <returns></returns>
    public FrozenFragment ImportFromHtml(HtmlDocument html, IList<ManifestItem> resources) {
      var converter = new HtmlToFrozenFragments();
      return converter.Convert(html.DocumentNode.OuterHtml, resources);
    }

    public string CreateHtml(Printable printable) {
      _printable = printable;
      _issueReport = new Dictionary<string, string>();
      // first we get the container template
      var converter = new BoomConverter();
      converter.IssueReport += converter_IssueReport;
      // TODO: Add dynamic modifications to final document here
      var html = converter.GenerateHtml(printable, Printable.TemplatePartial.DocumentXml);
      return html;
    }
 
    /// <summary>
    /// Create deep level copy of an opus and export as plain HTML using the goven groups builder attributes.
    /// </summary>
    /// <param name="opus">Source</param>
    /// <param name="createImage">handler to store images</param>
    /// <param name="scaleImage">handler to scale/change images according to image properties</param>
    /// <param name="builderContent">Callback to get the content from caller</param>
    /// <param name="numbering">Instruction to create section numbers.</param>
    /// <param name="path">Path to savely store temporary files.</param>
    /// <param name="target">Build for HTML, RSS, PDF, and so  on.</param>
    /// <returns>HTML</returns>
    public string CreateDocumentHtml(Opus opus, CreateImageHandler createImage, ScaleImageHandler scaleImage, Func<string> builderContent, IDictionary<string, NumberingSchema> numbering, string path, GroupKind target) {
      opus.CreateImage += createImage;
      opus.ScaleImage += scaleImage;
      opus.TempPath = path;
      opus.Numbering = numbering;
      var builder = opus.GetType().GetCustomAttributes(typeof(SnippetBuilderAttribute), true).OfType<SnippetBuilderAttribute>().Single(sb => sb.Target == target);
      opus.BuiltContent = builderContent();
      return builder.BuildHtml(opus, numbering, null);
    }

    /// <summary>
    /// Create deep level copy of one chapter of an opus and export as plain HTML using the goven groups builder attributes.
    /// </summary>
    /// <param name="opus">Reference to parent opus</param>
    /// <param name="chapter">Source Chapter</param>
    /// <param name="createImage">handler to store images</param>
    /// <param name="scaleImage">handler to scale/change images according to image properties</param>
    /// <param name="builderContent">Callback to get the content from caller</param>
    /// <param name="numbering">Instruction to create section numbers.</param>
    /// <param name="path">Path to savely store temporary files.</param>
    /// <param name="target">Build for HTML, RSS, PDF, and so  on.</param>
    /// <returns>HTML</returns>
    public string CreateChapterHtml(Opus document, Section chapter, CreateImageHandler createImage, ScaleImageHandler scaleImage, string path, GroupKind target, bool withNumbers = true) {
      if (chapter.Parent.Id != document.Id) {
        throw new ArgumentOutOfRangeException("chapter");
      }
      document.CreateImage += createImage;
      document.ScaleImage += scaleImage;
      document.TempPath = path;
      document.Numbering = GetLocalizedNumberingSchema();
      var html = new StringBuilder();
      html.Append(CreateHtmlInner(new Snippet[] { chapter }, document.Numbering, target));
      return html.ToString();
    }

    /// <summary>
    /// Create the final HTML from a frozen fragment. There is no scaling or file handling as the frozen fragments are ready to go.
    /// </summary>
    /// <param name="fragment"></param>
    /// <param name="withNumbers"></param>
    /// <returns></returns>
    public string CreateFragmentHtml(FrozenFragment fragment, CreateImageHandler createImage, ScaleImageHandler scaleImage, bool withNumbers = true) {
      var flatFragments = fragment.Children.FlattenHierarchy()
        .Where(f => f.TypeOfFragment == FragmentType.Html)
        .ToList();
      var sb = new StringBuilder();
      sb.AppendLine(Encoding.UTF8.GetString(fragment.Content));
      flatFragments.Select(f => sb.AppendLine(Encoding.UTF8.GetString(f.Content)));
      var fragmentContent = sb.ToString();
      // assume children provide resources
      if (fragment.HasChildren()) {
        foreach (var resources in fragment.Children) {
          var imgRegex = new Regex(String.Format(@"src=""{0}""", resources.ItemHref));
          if (!imgRegex.Match(fragmentContent).Success) continue;
          var newPath = createImage(this, new CreateImageArguments {
            FileName = resources.ItemHref
          });
          fragmentContent = imgRegex.Replace(fragmentContent, String.Format(@"src=""{0}""", newPath));
        }
      }
      return fragmentContent;
    }

    private string CreateHtmlInner(IEnumerable<Snippet> source, IDictionary<string, NumberingSchema> numbering, GroupKind target) {      
      // each snippet has its very own numbering schema
      var flatContent = new StringBuilder();
      foreach (var elm in source) {
        // Get the final HTML
        var builder = elm.GetType().GetCustomAttributes(typeof(SnippetBuilderAttribute), true).OfType<SnippetBuilderAttribute>().Single(sb => sb.Target == target);
        var html = builder.BuildHtml(elm, numbering, null);
        flatContent.Append(html);
        if (numbering == null) continue;
        // add chapter and reset all other counters
        numbering["Section1"].Major = numbering["Section1"].Major + 1;
        numbering["Section2"].Major = numbering["Section1"].Major;
        numbering["Section3"].Major = numbering["Section1"].Major;
        numbering["Section4"].Major = numbering["Section1"].Major;
        numbering["ImageSnippet"].Major = numbering["Section1"].Major;
        numbering["TableSnippet"].Major = numbering["Section1"].Major;
        numbering["ListingSnippet"].Major = numbering["Section1"].Major;
        numbering["Section1"].Minor = 1;
        numbering["Section2"].Minor = 1;
        numbering["Section3"].Minor = 1;
        numbering["Section4"].Minor = 1;
        numbering["ImageSnippet"].Minor = 1;
        numbering["TableSnippet"].Minor = 1;
        numbering["ListingSnippet"].Minor = 1;
      }
      return flatContent.ToString().Replace("&shy;", "&#173;");
    }

  }
}