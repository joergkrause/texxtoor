using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.BaseLibrary.Core.Extensions;
using System;

namespace Texxtoor.BaseLibrary.EPub.Model {

  // TOC OPS 1.0 -- EPUB 2.0
  [Table("Navigation", Schema = "Epub")]
  public class Navigation : EntityBase {

    public static readonly XNamespace NcxNamespace = "http://www.daisy.org/z3986/2005/ncx/";
    public static readonly string Version = "2005-1";

    internal static Navigation CreateNavigation(Manifest mf, XDocument ncxDocument) {
      var ns = ncxDocument.Root.GetDefaultNamespace();
      var metaElements = ncxDocument.Root.Descendants(ns + "head").Elements(ns + "meta").ToArray();
      var head = new Head {
        Creator = metaElements.ReadAttributeFromElement("epub-creator", "content"),
        Identifier = metaElements.ReadAttributeFromElement("dtb:uid", "content"),
        Depth = metaElements.ReadAttributeFromElement("dtb:depth", "content").NullSafeInt32(),
        TotalPageCount = metaElements.ReadAttributeFromElement("dtb:totalPageCount", "content").NullSafeInt32(),
        MaxPageNumber = metaElements.ReadAttributeFromElement("dtb:maxPageNumber", "content").NullSafeInt32(),
      };
      var n = new Navigation {
        HeadMetaData = head,
        NavMap = GetNavMap(mf, ns, ncxDocument.Root.Descendants(ns + "navMap").Elements(ns + "navPoint"))
      };

      return n;
    }

    private static List<NavPoint> GetNavMap(Manifest mf, XNamespace ns, IEnumerable<XElement> navPoints) {
      var navList = new List<NavPoint>();
      foreach (var navPoint in navPoints) {
        var n = new NavPoint {
          Identifier = navPoint.Attribute("id").Value,
          LabelText = navPoint.Element(ns + "navLabel").Element(ns + "text").Value ?? "Label",
          Content = navPoint.Element(ns + "content").Attribute("src").Value,
          PlayOrder = navPoint.Attribute("playOrder").NullSafeInt32(),
          MetaId = GetIdFromManifest(mf, navPoint.Element(ns + "content").Attribute("src").Value)
        };
        if (navPoint.Elements(ns + "navPoint").Any()) {
          n.Children = GetNavMap(mf, ns, navPoint.Elements(ns + "navPoint"));
        }
        navList.Add(n);
      }
      return navList;
    }

    // this is a helper that makes the resolvation of paths easier, as the TOC does not use the same href as Manifest on Epub
    private static string GetIdFromManifest(Manifest mf, string src) {
      src = System.Web.HttpUtility.UrlDecode(src);
      var query = mf.Items.FirstOrDefault(m => m.Href == src);
      if (query == null) {
        // some epbus seem to use # as an path extension what the manifest cant resolve
        if (src.Contains("#")) {
          src = src.Split(new char[] { '#' })[0];
          query = mf.Items.FirstOrDefault(m => m.Href == src);
        }
      }
      if (query == null) {
        throw new ArgumentNullException("src");
      }
      return query.Identifier;
    }

    public static XElement CreateXElement(Navigation nav, string title) {
      var xe = new XElement(NcxNamespace + "ncx",
        new XAttribute("xmlns", NcxNamespace),
        new XAttribute(XNamespace.Xml + "lang", "en"),
        new XAttribute("version", Version),
        new XElement(NcxNamespace + "head",
          new XElement(NcxNamespace + "meta",
            new XAttribute("name", "epub-creator"), new XAttribute("content", nav.HeadMetaData.Creator)),
          new XElement(NcxNamespace + "meta",
            new XAttribute("name", "dtb:uid"), new XAttribute("content", nav.HeadMetaData.Identifier)),
          new XElement(NcxNamespace + "meta",
            new XAttribute("name", "dtb:depth"), new XAttribute("content", nav.HeadMetaData.Depth)),
          new XElement(NcxNamespace + "meta",
            new XAttribute("name", "dtb:totalPageCount"), new XAttribute("content", nav.HeadMetaData.TotalPageCount)),
          new XElement(NcxNamespace + "meta",
            new XAttribute("name", "dtb:maxPageNumber"), new XAttribute("content", nav.HeadMetaData.MaxPageNumber))
            ), // end head
        new XElement(NcxNamespace + "docTitle",
          new XElement(NcxNamespace + "text", title)
            ), // end docTitle
        new XElement(NcxNamespace + "navMap",
          CreateXNavPoints(nav.NavMap))
          );
      xe.Add();
      return xe;
    }

    private static XElement[] CreateXNavPoints(IList<NavPoint> navPoints) {
      var result =
        navPoints.Where(np => np.Children != null).Select(np =>
          new XElement(NcxNamespace + "navPoint",
            CreateXNavPoints(np.Children),
            new XElement(NcxNamespace + "navLabel", new XElement(NcxNamespace + "text", np.LabelText)),
            new XElement(NcxNamespace + "content", new XAttribute("src", np.Content)),
            new XAttribute("playOrder", np.PlayOrder),
            new XAttribute("id", np.Identifier)
            )
          )
        .Union(
        navPoints.Where(np => np.Children == null).Select(np =>
          new XElement(NcxNamespace + "navPoint",
            new XElement(NcxNamespace + "navLabel", new XElement(NcxNamespace + "text", np.LabelText)),
            new XElement(NcxNamespace + "content", new XAttribute("src", np.Content)),
            new XAttribute("playOrder", np.PlayOrder))
          )
          );
      return result.ToArray();
    }

    public Head HeadMetaData { get; set; }

    public IList<NavPoint> NavMap { get; set; }

  }

  [Table("Navigation_Head", Schema = "Epub")]
  public class Head : EntityBase {

    [StringLength(128)]
    [Required]
    public string Identifier { get; set; }
    [StringLength(512)]
    public string Creator { get; set; }
    [Range(-1, 99)]
    public int Depth { get; set; }
    [Range(0, 9999)]
    public int TotalPageCount { get; set; }
    [Range(0, 9999)]
    public int MaxPageNumber { get; set; }
  }

  [Table("Navigation_NavPoint", Schema = "Epub")]
  public class NavPoint : HierarchyBase<NavPoint> {

    [StringLength(128)]
    [Required]
    public string Identifier { get; set; }
    [StringLength(512)]
    public string LabelText { get; set; }
    [Required]
    public int PlayOrder { get; set; }
    [Required]
    [StringLength(256)]
    public string Content { get; set; }
    // this is reference to the manifest and is not defined in NavPoint element as of Epub 3.0
    [StringLength(128)]
    public string MetaId { get; set; }

    public Navigation Navigation { get; set; }

  }

}
