using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Image = System.Drawing.Image;

namespace Texxtoor.BaseLibrary.Pdf {


  public class BoomConverter : IDisposable {

    private Printable _printable;

    private static readonly XNamespace ConstNsInclude = "http://www.w3.org/2001/XInclude";
    private static readonly XNamespace ConstNsTexxtoor = Printable.ConstNsTexxtoor;
    private static readonly XNamespace ConstNsDefault = "http://www.w3.org/1999/xhtml";
    private const string ConstInclude = "include";
    private const string ConstHref = "href";

    public delegate void IssueReportHandler(object sender, KeyValuePair<string, string> e);

    public event IssueReportHandler IssueReport;

    private void OnIssueReport(KeyValuePair<string, string> e) {
      if (IssueReport != null) {
        IssueReport(this, e);
      }
    }

    public string GenerateHtml(Printable printable, string mainTemplate) {
      _printable = printable;
      // get all content, subsequent function may modify it, first turn all entities into numeric entities to prevent braking the xml parser
      OnIssueReport(new KeyValuePair<string, string>("Information", "Start at " + DateTime.Now.ToLongTimeString()));
      // we use the configurable main template to have different entry points, such as content and cover 
      var xml = ReplaceInclude(_printable.GetMainDocument(mainTemplate).Root);
      if (xml.Descendants(ConstNsTexxtoor + "style").Count() == 1) {
        var src = xml.Descendants(ConstNsTexxtoor + "style").First().Attribute("src").Value;
        var cssTemplate = printable.GetContent(src);
        var rxFonts = new Regex(@"url\('(?<font>Fonts/.*)'\);", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        foreach (Match match in rxFonts.Matches(cssTemplate)) {
          var rxFontPath = HttpContext.Current.Server.MapPath("~/App_Data/Templates/" + match.Groups["font"].Value).Replace("\\", "/");
          cssTemplate = cssTemplate.Replace(String.Format("url('{0}');", match.Groups["font"].Value), String.Format("url('{0}');", rxFontPath));
        }
        // generic content provides by style sheet
        cssTemplate = String.Format("<!--/*--><![CDATA[*><!--*/\n{0}\n/*]]>*/-->", cssTemplate);
        // first we read the templates and fill in data        
        xml.Descendants(ConstNsTexxtoor + "style").First().ReplaceWith(new XElement(ConstNsDefault + "style", new XRaw(cssTemplate)));
      }
      // common content functions
      printable.FillContent(xml, printable.AllContent, null, null);
      printable.FillVariables(xml);
      // PDF specific
      FillToc(xml);
      FillIndex(xml);
      printable.CleanUpVariables(xml);
      // replace the template descriptions with the real data
      var metaName = xml.Descendants(ConstNsDefault + "meta").FirstOrDefault(e => e.Attribute("name") != null && e.Attribute("name").Value == "name");
      if (metaName != null) {
        metaName.Attribute("content").Value = String.Format("{0} created at {1} by {2}", printable.Title, DateTime.Now, printable.CallingUser);
      }
      var descName = xml.Descendants(ConstNsDefault + "meta").FirstOrDefault(e => e.Attribute("name") != null && e.Attribute("name").Value == "description");
      if (descName != null) {
        descName.Attribute("content").Value = printable.CoverDescription ?? String.Empty;
      }
      var html = xml.ToString();
# if DEBUG
      // for debugging we save the final html, we use the temp path from images and suppress cleanup later on 
      File.WriteAllText(Path.Combine(printable.TempStorePath, String.Format("debug-{0}.html", printable.Title)), html);
# endif
      // call prince with result
      return html;
    }

    public class XRaw : XText {
      public XRaw(string text) : base(text) { }
      public XRaw(XText text) : base(text) { }

      public override void WriteTo(System.Xml.XmlWriter writer) {
        writer.WriteRaw(this.Value);
      }
    }

    protected virtual XElement ReplaceInclude(XElement include) {
      if (include == null) return null;
      while (include.Descendants(ConstNsInclude + ConstInclude).Any()) {
        var child = include.Descendants(ConstNsInclude + ConstInclude).First();
        var path = child.Attribute(ConstHref).Value;
        var subelement = _printable.GetTemplate(path);
        child.ReplaceWith(ReplaceInclude(subelement)); // the same for child XIncludes 
      }
      return include;
    }

   
    const string RgxIndx = @"<span[^>]*?data-type=""index""[^>]*?>(?<index>[^<]*?)</span>";
    const string RgxData = @"(?<name>\S+)=[""']?(?<value>(?:.(?![""']?\s+(?:\S+)=|[>""']))+.)[""']?";

    private void FillIndex(XElement xml) {
      if (_printable.HasIndex) {
        var idxBuilder = new StringBuilder();
        // look for <span title="Foundations" class="isindex" data-type="index">Foundations</span> and process these
        var indexMatches = Regex.Matches(xml.ToString(), RgxIndx, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var termList = new Dictionary<string, string>();
        // create HTML that the princexml css converter can process
        foreach (Match index in indexMatches) {
          var data = Regex.Matches(index.Value, RgxData, RegexOptions.Compiled | RegexOptions.IgnoreCase);
          string title = "";
          string builderid = "";
          foreach (Match match in data) {
            if (match.Groups[1].Value == "title") title = match.Groups[2].Value; // <span title="">, the visible/shown text
            if (match.Groups[1].Value == "id") builderid = match.Groups[2].Value; // <span id="">, the reference id
          }
          if (String.IsNullOrEmpty(builderid) || String.IsNullOrEmpty(title)) {
            continue;
          }
          termList.Add(builderid, title);
        }
        // group by leading letter
        var groupedIdx = termList
          .GroupBy(t => t.Value.Substring(0, 1).ToUpper())
          .OrderBy(t => t.Key)
          .ToDictionary(t => t.Key, t => t);
        foreach (var groupIdx in groupedIdx) {
          idxBuilder.AppendFormat("<li class='group'>{0}</li>", groupIdx.Key.ToUpper());
          foreach (var idx in groupIdx.Value) {
            idxBuilder.AppendFormat("<li><a href='#{0}'>{1}</a></li>", idx.Key, idx.Value);
          }
        }
        xml.Descendants(ConstNsTexxtoor + "index").ForEach(t => t.ReplaceWith(idxBuilder.ToString()));
      }
    }

    private void FillToc(XElement xml) {
      if (_printable.HasToc) {
        var tocDefinition = xml.Descendants(ConstNsTexxtoor + "toc").ToList().First();
        var outer = tocDefinition.Attribute("containerelement").NullSafeString("ul");
        var inner = tocDefinition.Attribute("buildelement").NullSafeString("li");
        var ul = new XElement(outer, new XAttribute("class", "toc"),
          new XElement(inner, new XAttribute("class", "frontmatter"),
            new XElement("a", new XAttribute("href", "#toc-h-1"), new XAttribute("class", "toc-header"))), // content set in CSS      
          _printable.ToC.OrderBy(c => c.OrderNr).Select(chapter => new XElement(inner,
            new XAttribute("class", "chapter"),
            new XElement("a", new XAttribute("href", String.Format("#{0}", chapter.BuilderId)), chapter.Text),
            !chapter.HasChildren ? null : new XElement(outer,
              chapter.Children.Select(section => new XElement(inner,
                new XAttribute("class", "section"),
                new XElement("a", new XAttribute("href", String.Format("#{0}", section.BuilderId)), section.Text),
                !section.HasChildren ? null : new XElement(outer,
                  chapter.Children.Select(subsection => new XElement(inner,
                    new XAttribute("class", "subsection"),
                    new XElement("a", new XAttribute("href", String.Format("#{0}", subsection.BuilderId)), subsection.Text)
                    ) // subsection li
                    )) // subsection ul
                ) // section li
                )) // section ul
            ) // chapter li
          )
        ); // toc ul
        if (_printable.HasIndex) {
          ul.Add(new XElement(inner,
            new XAttribute("class", "frontmatter"),
            new XElement("a",
              new XAttribute("href", "#index-h-1"),
              new XAttribute("class", "idx-header")
            )
          )); // content set in CSS
        }
        xml.Descendants(ConstNsTexxtoor + "toc").First().ReplaceWith(ul);
      }
    }


    public void Dispose() {
      _printable = null;
    }

  }
}
