using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.EPub;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.BaseLibrary.Converters;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Helper;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.BaseLibrary.Pdf;

namespace Texxtoor.BusinessLayer {


  /// <summary>
  /// This class contains all methods to create actual content, such as PDF, EPub, iBook, and more.
  /// </summary>
  public partial class ProductionManager {

    # region EPUB Import

    public void ImportFromEpubSpine(EpubBook book, Published p, PortalContext ctx) {
      var converter = new EPubToFrozenFragments(EPubToFrozenFragments.Method.Spine);
      converter.Convert(book, p, ctx);
    }

    public void ImportFromEpubNcxToc(EpubBook book, Published p, PortalContext ctx) {
      var converter = new EPubToFrozenFragments(EPubToFrozenFragments.Method.NcxToc);
      converter.Convert(book, p, ctx);
    }

    public void ImportFromEpubHtml(EpubBook book, Published p, PortalContext ctx) {
      var converter = new EPubToFrozenFragments(EPubToFrozenFragments.Method.Html);
      converter.Convert(book, p, ctx);
    }

    # endregion  EPUB Import

    private Printable _printable = null;
    private Dictionary<string, string> _hyperlinkJumpTable = null;
    private Dictionary<string, string> _issueReport = null;
    private EpubBook book;

    /// <summary>
    /// Return all errors and warnings found while processing the data.
    /// </summary>
    public IDictionary<string, string> IssueReport {
      get { return _issueReport; }
    }

    public byte[] CreateEpub(Printable printable) {
      _printable = printable;
      _hyperlinkJumpTable = new Dictionary<string, string>();
      _issueReport = new Dictionary<string, string>();
      _issueReport.Add("Information", "Start at " + DateTime.Now.ToLongTimeString());
      var userObj = UserManager.Instance.GetCurrentUser(HttpContext.Current.User.Identity.Name).Profile;
      var user = String.Format("{0} {1}", userObj.FirstName, userObj.LastName);
      book = new EpubBook();

      List<ManifestItem> items = new List<ManifestItem>();
      var css = new CssFile {
        Identifier = "idtxxtCss",
        Href = "css/stylesheet.css",
        Data = printable.Templates.Single(t => t.InternalName == "stylesheet.css").Content
      };
      printable.CssPath = "css"; // forward the decision to keep css separetely to the data container
      /********** ADD CSS **********/
      items.Add(css);
      // check css for fonts and embedd accordingly      
      var fontCnt = 0;
      /********** ADD FONTS **********/
      printable.Templates.Where(t => t.InternalName.EndsWith(".otf")).ForEach(f =>
        items.Add(
        new FontFile {
          Identifier = String.Format("fntOtf{0:00}", fontCnt++),
          Href = String.Format("css/Fonts/{0}", f.InternalName),
          Data = f.Content
        }));
      // cover data
      var coverimgTpl = printable.Templates.FirstOrDefault(t => t.InternalName == "cover.jpg");
      ImageFile coverImg = null;
      if (coverimgTpl == null) {
        if (printable.CoverImage != null) {
          coverImg = ImageFile.CreateImageFile("jpg", printable.CoverImage);
        }
      } else {
        coverImg = ImageFile.CreateImageFile("jpg", coverimgTpl.Content);
      }
      if (coverImg == null) {
        _issueReport.Add("Error", "Template cover.png not provided and project does not has a default image");
      }
      coverImg.Identifier = "cover";
      coverImg.Href = CreateImagePath("cover.jpg");
      /********** ADD COVER IMG **********/
      items.Add(coverImg);
      /********** ADD HELPER IMG **********/
      var blankImg = ImageFile.CreateImageFile("gif", File.ReadAllBytes(HttpContext.Current.Server.MapPath("~/Content/images/blank.gif")));
      blankImg.Href = CreateImagePath("blank.gif");
      blankImg.Identifier = "blankgif";
      items.Add(blankImg);
      // Apart from what we do for PDF the EPUB module "thinks" in multiple documents". Hence, we have a separate file for cover. The xinclude function
      // is merely for convenience while creating complex document with repeating static sections 
      var coverTpl = printable.Templates.Single(t => t.InternalName == Printable.TemplatePartial.BookCover).Content;
      var coverHtml = new ContentFile {
        Href = "cover.xhtml",
        Identifier = "Cover",
        MediaType = "application/xhtml+xml",
        Data = NormalizeXml(coverTpl, "cover.xhtml", new byte[0]) // cover has default content only
      };
      /********** ADD COVER **********/
      items.Add(coverHtml);
      // content data
      int orderNr = 0;
      /********** ADD CONTENT **********/
      // CHAPTERS
      var order = 1;
      var chapterTpl = printable.Templates.Single(t => t.InternalName == Printable.TemplatePartial.DocumentXml).Content;
      printable.Chapters.ForEach(c => {
        var href = String.Format("{0}{1}.xhtml", "Chapter", order++);
        var cf = ContentFile.Create(FileItemType.XHTML);
        cf.Data = NormalizeXml(chapterTpl, href, c.Content);
        cf.Href = href;
        cf.Identifier = "id" + c.Identifier; // prefix to avoid pure numbers ==> repeat this for NCX as well!!!
        items.Add(cf);
      });
      // IMAGES
      printable.Images.ForEach(i => {
        var imgf = ImageFile.Create(i.Href);
        imgf.Data = i.Content;
        imgf.Href = CreateImagePath(i.Href);
        imgf.Identifier = i.Identifier; // 
        items.Add(imgf);
      });
      // OPF
      if (String.IsNullOrEmpty(printable.LocaleId)) {
        _issueReport.Add("Warning", "Locale not set");
      }
      if (String.IsNullOrEmpty(printable.Publisher)) {
        _issueReport.Add("Warning", "Publisher not set");
      }
      book.PackageData = new OpfPackage {
        MetaData = new MetaData {
          Identifier = new IdentifierElement { Text = String.Format("urn:uuid:{0}", Guid.NewGuid()), Identifier = "BookId" },
          Contributor = new ContributorElement { Text = user },
          Creator = new CreatorElement { Text = user },
          Description = new DescriptionElement { Text = printable.CoverDescription },
          Language = new LanguageElement { Text = String.IsNullOrEmpty(printable.LocaleId) ? "de" : printable.LocaleId },
          Publisher = new PublisherElement { Text = String.IsNullOrEmpty(printable.Publisher) ? "Augmented Content GmbH" : printable.Publisher },
          Rights = new RightsElement { Text = String.IsNullOrEmpty(printable.Rights) ? "(c) 2011-2014 by Augmented Content GmbH" : printable.Rights },
          Subject = new SubjectElement { Text = String.Format("Made for {0}", printable.Publisher) },
          Title = new TitleElement { Text = printable.Title },
          Date = new DateElement { Value = DateTime.Now }
        },
        Identifier = "BookId",
        Language = printable.LocaleId,
        // Data
        Manifest = new Manifest {
          Identifier = "idManifest",
          Items = items
        },
        // Navigation Order
        Spine = new Spine {
          Toc = "ncx",
          ItemRefs = items.Where(item => item.Href.EndsWith(".xhtml")).Select(item => new ItemRef {
            IdRef = item.Identifier
          }).ToList()
        },
        // Entry Point (TOC or Cover), we generate it here
        Guide = new Guide {
          ReferenceTitle = "Cover",
          ReferenceHref = "cover.xhtml",
          ReferenceType = "cover"
        }
      };
      var opf = new RootFile {
        FullPath = "content/content.opf",
        MediaType = "application/oebps-package+xml"
      };
      book.ContainerData = new Container {
        Rootfiles = new List<RootFile>(new[] { opf })
      };
      orderNr = 0;
      List<NavPoint> navmap = null;
      try {
        navmap = new List<NavPoint>(printable
          .ToC
          .Where(chapter => items.Any(item => item.Identifier == "id" + chapter.BuilderId)) // only if it is a real item
          .Select(chapter => new NavPoint {
            LabelText = chapter.Text,
            OrderNr = orderNr++,
            Content = items.Single(item => item.Identifier == "id" + chapter.BuilderId).Href,
            PlayOrder = orderNr,
            Identifier = String.Format("np-{0:000}", orderNr)
          }).ToArray());
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
      book.NavigationData = new Navigation {
        HeadMetaData = new Head {
          Creator = user,
          Identifier = book.PackageData.MetaData.Identifier.Text,
          MaxPageNumber = 100, // TODO: calculate these
          Depth = 1,
          TotalPageCount = 100 + 1
        },
        NavMap = navmap
      };
      return EBookFactory.SaveBook(book);
    }

    private static string CreateImagePath(string fileName) {
      return "images/" + fileName;
    }

    /// <summary>
    /// Internally we use some namespaces. EPUB, however, can't deal with namespaces other than the epub-related ones. This function removes texxtoor namespaces.
    /// </summary>
    /// <param name="templateXml">byte stream must contain valid XML/XHTML template</param>
    /// <param name="targetName">Chaptername that resolves for the jump table</param>
    /// <param name="content">the actual content data to be filled in. All other values are stored in the printable object.</param>
    /// <returns>byte stream must contains XML/XHTML</returns>
    private byte[] NormalizeXml(byte[] templateXml, string targetName, byte[] content) {
      string xml = Encoding.UTF8.GetString(templateXml);
      xml = xml.ToHtmlNumericEntity();
      templateXml = Encoding.UTF8.GetBytes(xml);
      var xDoc = XDocument.Load(new MemoryStream(templateXml));
      // provide content and resolve linked items (a, img)
      _printable.FillContent(xDoc.Root, Encoding.UTF8.GetString(content), AddContentImages, ResolveHyperLink);
      // fill in variable data
      _printable.FillVariables(xDoc.Root);
      // remove template support
      _printable.CleanUpVariables(xDoc.Root);
      // to support internal hyperlinks we create a "jump table"
      xDoc.Root.Descendants().Where(e => e.Attribute("id") != null).ForEach(e => _hyperlinkJumpTable.Add(e.Attribute("id").Value, targetName));
      // "clean up" to fullfill EPUB 3 standards
      var result = RemoveAllNamespaces(xDoc.Root);
      var cleanDoc = new XDocument(new XDocumentType("html", "-//W3C//DTD XHTML 1.1//EN", "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd", null), result);
      var settings = new XmlWriterSettings { OmitXmlDeclaration = false, Encoding = Encoding.UTF8 };
      using (var memoryStream = new MemoryStream())
      using (var xmlWriter = XmlWriter.Create(memoryStream, settings)) {
        cleanDoc.WriteTo(xmlWriter);
        xmlWriter.Flush();
        return memoryStream.ToArray();
      }
    }

    private string AddContentImages(string rawSrc) {
      // path adjustment to match relative package paths
      return CreateImagePath(rawSrc);
    }

    private string ResolveHyperLink(string targetId) {
      if (!_hyperlinkJumpTable.ContainsKey(targetId)) {
        _issueReport.Add("Warning", "Hyperlink to target " + targetId + " not set");
        return String.Empty; // kill links that will not work!
}
      // Get the chapter the target is in
      var chapter = _hyperlinkJumpTable[targetId];
      // return url plus hash
      return chapter + "#" + targetId;
    }

    private static XElement RemoveAllNamespaces(XElement e) {
      try {
        return new XElement(e.Name.LocalName,
      (from n in e.Nodes()
       select ((n is XElement) ? RemoveAllNamespaces(n as XElement) : n)),
          (e.HasAttributes) ?
            (from a in e.Attributes()
             where (!a.IsNamespaceDeclaration)
             select new XAttribute(a.Name.Namespace + a.Name.LocalName, a.Value)) : null);

      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        return null;
      }
    }

  }
}