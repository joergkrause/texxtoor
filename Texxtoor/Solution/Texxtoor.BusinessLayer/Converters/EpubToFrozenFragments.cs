using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.BaseLibrary.Core.HtmlAgility.Pack;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.BaseLibrary.Converters {
  /// <summary>
  /// Convert an EPUB to list of hierarchical FrozenFragments similar to a publish procedure.
  /// </summary>
  internal class EPubToFrozenFragments {

    /// <summary>
    /// Conversion method
    /// </summary>
    public enum Method {
      /// <summary>
      /// Spine is simple chapter based method, robust and easy
      /// </summary>
      Spine,
      /// <summary>
      /// Try to extract the hierarchy from Ncx file
      /// </summary>
      NcxToc,
      /// <summary>
      /// Collapse whole document into one HTML and extract based on h1, h2, ..., tags
      /// </summary>
      Html
    }

    private Method _method;
    public Published Published { get; set; }
    public PortalContext Context { get; set; }
    public EPubToFrozenFragments(Method method) {
      _method = method;
    }

    public void Convert(EpubBook book, Published p, PortalContext context) {
      Published = p;
      Context = context;
      // 1. The spine is used to create one single document as a source
      var spineIds = book.PackageData.Spine.ItemRefs.Select(i => i.IdRef).ToList();
      // all elements
      var data = book.PackageData.Manifest.Items;
      // 2. Get the content elements only
      var content = data.Where(i => spineIds.Any(sid => sid == i.Identifier)).Select(i => i).ToList();
      // fragments form a hierarchy, beginning with <h1> on the first level
      FrozenFragment rootFragment = new FrozenFragment {
        Name = book.PackageData.MetaData.Title.Text,
        ItemHref = "Opus (Import)",
        Published = p,
        TypeOfFragment = FragmentType.Meta,
        Children = new List<FrozenFragment>()
      };
      switch (_method) {
        case Method.NcxToc:
        case Method.Html:
          // 3. Create source
          var complete = new StringBuilder();
          complete.AppendLine("<body>");
          foreach (var html in content) {
            complete.AppendFormat(@"<a name=""{0}"" ></a>", html.Identifier);
            complete.Append(GetBodyFromManifest(html.Data));
          }
          complete.AppendLine("</body>");
          // 5.
          HtmlDocument allHtml = new HtmlDocument();
          allHtml.OptionAutoCloseOnEnd = true;
          allHtml.OptionFixNestedTags = true;
          allHtml.OptionOutputAsXml = true;
          allHtml.LoadHtml(complete.ToString());
          var body = allHtml.DocumentNode.SelectSingleNode("//body");
          switch (_method) {
            case Method.NcxToc:
              NcxParser(book, body, rootFragment);
              break;
            case Method.Html:
              var htmlConverter = new HtmlToFrozenFragments();
              htmlConverter.Convert(body.InnerHtml, data);
              break;
          }
          break;
        case Method.Spine:
          SpineParser(content, rootFragment, data);
          break;
      }
      //Context.SaveChanges();      
    }

    # region Spine Method

    private void SpineParser(IList<ManifestItem> content, FrozenFragment root, IList<ManifestItem> data) {
      foreach (var file in content) {
        CreateTextFragment(root, file, data);
      }
    }

    private FrozenFragment CreateTextFragment(FrozenFragment parent, ManifestItem file, IList<ManifestItem> data) {
      var c = UTF8Encoding.UTF8.GetString(file.Data);
      // close open tags or open closed tags
      HtmlDocument nodeCheck = new HtmlDocument();
      nodeCheck.OptionAutoCloseOnEnd = true;
      nodeCheck.OptionCheckSyntax = true;
      nodeCheck.OptionFixNestedTags = true;
      nodeCheck.LoadHtml(c);
      if (nodeCheck.ParseErrors.Count() > 0) {
        // handle parser errors
      }
      var f = new FrozenFragment {
        Name = "Chapter:" + file.Identifier,
        ItemHref = file.Href,
        TypeOfFragment = FragmentType.Html,
        Parent = parent,
        Published = Published
      };
      Context.FrozenFragments.Add(f);
      var body = nodeCheck.DocumentNode.SelectSingleNode("//body");
      CreateEmbeddedImages(f, body, data);
      f.Content = UTF8Encoding.UTF8.GetBytes(body.InnerHtml);
      parent.Children.Add(f);
      c = String.Empty;
      return f;
    }

    private void CreateEmbeddedImages(FrozenFragment parent, HtmlNode element, IList<ManifestItem> data) {
      if (element.SelectNodes(".//img") == null) return;
      foreach (var img in element.SelectNodes(".//img")) {
        var alt = img.Attributes["alt"] != null ? img.Attributes["alt"].Value : "Generic Image";
        var cnt = data.FirstOrDefault(d => img.Attributes["src"].Value.EndsWith(d.Href));
        var r = new FrozenFragment {
          Name = alt,
          Content = cnt.Data,
          TypeOfFragment = FragmentType.Image,
          ItemHref = Guid.NewGuid().ToString(),
          Published = Published
        };
        if (parent.Children == null) {
          parent.Children = new List<FrozenFragment>();
        }
        parent.Children.Add(r);
        img.Attributes["src"].Value = r.ItemHref;
        Context.FrozenFragments.Add(r);
      }
    }

    # endregion

    # region NCX Method

    private void NcxParser(EpubBook book, HtmlNode content, FrozenFragment rootFragment) {
      var data = book.PackageData.Manifest.Items;
      var nav = book.NavigationData.NavMap;
      RecurseNavigation(nav, content, rootFragment);
    }

    private void RecurseNavigation(IList<NavPoint> navPoints, HtmlNode content, FrozenFragment parent) {
      foreach (var np in navPoints) {
        var f = new FrozenFragment {
          Name = np.LabelText,
          ItemHref = np.Identifier,
          OrderNr = np.PlayOrder,
          Parent = parent,
          Published = Published
        };
        // the html is a flat document. To get the navigation content as hierarchy we retrieve the current segment up to the next one
        // can be of format manifestFile#Id
        string npNextId = "";
        // 1. current Ids
        var npFileId = GetFileIdFromNcxId(np.Content);
        var npHashId = GetHashIdFromNcxId(np.Content);
        // 2. next Ids
        var idx = navPoints.IndexOf(np);
        var nxt = navPoints.Count() > idx ? navPoints.ElementAt(idx + 1) : null;
        if (nxt == null) {
          // either end of NCX or in a sublevel
          if (navPoints.Last() != np && np.HasChildren()) {
            // not end, look deeper
            npNextId = np.Children.First().Content;
          }
        } else {
          npNextId = nxt.Content;
        }
        var nxFileId = GetFileIdFromNcxId(npNextId);
        var nxHashId = GetHashIdFromNcxId(npNextId);
        var startNode = content.SelectSingleNode(String.Format("//*[@name='{0}']", npFileId));
        var startHash = startNode.SelectSingleNode(String.Format("//*[@*='{0}']", npHashId));

        var stopNode = content.SelectSingleNode(String.Format("//*[@name='{0}']", nxFileId));
        var stopHash = stopNode.SelectSingleNode(String.Format("//*[@*='{0}']", nxHashId));

        var snippet = "";
        foreach (var s in startHash.AncestorsAndSelf()) {
          snippet += s.OuterHtml;
          if (s == stopHash) {
            break;
          }
        }

      }
    }

    private string GetFileIdFromNcxId(string id) {
      if (id.Contains('#')) {
        var s = id.Split('#');
        return s[0];
      } else {
        return id;
      }
    }

    private string GetHashIdFromNcxId(string id) {
      if (id.Contains('#')) {
        var s = id.Split('#');
        return s[1];
      } else {
        return id;
      }
    }

    private string GetBodyFromManifest(byte[] data) {
      HtmlDocument doc = new HtmlDocument();
      var html = UTF8Encoding.UTF8.GetString(data);
      doc.LoadHtml(html);
      if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0) {
        // Handle any parse errors as required
      } else {
        if (doc.DocumentNode != null) {
          HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("//body");
          if (bodyNode != null) {
            // 4.
            return bodyNode.InnerHtml;
          }
        }
      }
      return "";
    }

    # endregion
  }

}
