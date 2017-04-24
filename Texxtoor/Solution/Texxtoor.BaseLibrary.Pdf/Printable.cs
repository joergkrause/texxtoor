using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.HtmlAgility.Pack;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.BaseLibrary.Pdf {
  /// <summary>
  /// An abstract container that helps decoupling the various input objects (Opus, Published, OrderProduct)
  /// </summary>
  public class Printable {

    public static readonly XNamespace ConstNsTexxtoor = "http://www.texxtoor.de/2014/templating";

    public struct TemplatePartial {

      public const string BookCover = "frontcover.xml";
      public const string DocumentXml = "document.xml";

    }

    public class Pricing {
      public decimal NettoPrice { get; set; }
      public decimal Vat { get; set; }
      public string Currency { get; set; }
      public string CountryName { get; set; }

      public override string ToString() {
        return String.Format("{0}: {1} {2:0.00} ({3:0.00%})", CountryName, Currency, NettoPrice*Vat, Vat);
      }

    }

    public class ContentFile {
      public string Href { get; set; }
      public byte[] Content { get; set; }
      public string Name { get; set; }
      public string Identifier { get; set; }
    }

    public class ImageFile : ContentFile {
    }

    public class TocElement {

      public TocElement() {
        Children = new List<TocElement>();
      }

      public int OrderNr { get; set; }
      public string BuilderId { get; set; }
      public string Text { get; set; }
      public List<TocElement> Children { get; set; }
      public bool HasChildren {
        get {
          return Children.Any();
        }
      }
    }

    public Printable(IList<Template> templates) {
      Templates = templates;
      FrontCoverForeColor = "#000000";
      Version = "1.0";
      PrintDate = DateTime.Now.ToLongDateString();
      // the existance of an template is our default. Caller might override this, but only in skipping, not adding (otherwise boomconverter will fail)
      var document = Templates.SingleOrDefault(t => t.InternalName == TemplatePartial.DocumentXml);
      if (document != null) {
        var xDoc = LoadUtf8Safe(document.Content);
        var config = xDoc.Descendants(ConstNsTexxtoor + "configuration").FirstOrDefault();
        if (config != null) {
          HasImprint = config.Attribute("imprint").NullSafeBool().GetValueOrDefault();
          HasToc = config.Attribute("toc").NullSafeBool().GetValueOrDefault();
          HasFrontCover = config.Attribute("frontcover").NullSafeBool().GetValueOrDefault();
          HasAbout = config.Attribute("about").NullSafeBool().GetValueOrDefault();
          HasIndex = config.Attribute("index").NullSafeBool().GetValueOrDefault();
        }
      }
    }

    public bool HasImprint { get; set; }
    public bool HasToc { get; set; }
    public bool HasFrontCover { get; set; }
    public bool HasAbout { get; set; }
    public bool HasIndex { get; set; }

    public XElement GetTemplate(string partial) {
      var template = Templates.SingleOrDefault(t => t.InternalName == partial);
      if (template != null) {
        return LoadUtf8SafePartial(template.Content);
      }
      return null;
    }

    public string GetContent(string partial) {
      var template = Templates.SingleOrDefault(t => t.InternalName == partial);
      return template != null ? Encoding.ASCII.GetString(template.Content).Substring(3) : String.Empty;
    }


    public XDocument GetMainDocument(string templateName) {
        return LoadUtf8Safe(Templates.Single(t => t.InternalName == templateName).Content);
    }

    private static XDocument LoadUtf8Safe(byte[] content) {
      XDocument xml;
      using (var xmlStream = new MemoryStream(content)) {
        using (var xmlReader = new XmlTextReader(xmlStream)) {
          xml = XDocument.Load(xmlReader);
        }
      }
      return xml;
    }

    private static XElement LoadUtf8SafePartial(byte[] content) {
      XElement xml;
      using (var xmlStream = new MemoryStream(content)) {
        using (var xmlReader = new XmlTextReader(xmlStream)) {
          xml = XElement.Load(xmlReader);
        }
      }
      return xml;
    }

    # region Book Data
    public DateTime PublishingDate { get; set; }
    public string Isbn { get; set; }
    public bool HasIsbn { get { return !String.IsNullOrEmpty(Isbn); } }
    public string Title { get; set; }
    public string SubTitle { get; set; }
    public bool HasSubTitle { get { return !String.IsNullOrEmpty(SubTitle); } }
    public string AuthorNamesShort { get; set; }
    public string AuthorNamesVerbose { get; set; }
    public IList<string> AdditionalAuthorInfo { get; set; }
    public string AuthorBiography { get; set; }
    public IList<string> AdditionalAuthorBiographies { get; set; }
    public string Publisher { get; set; }
    public string AuthorAboutText { get; set; }
    public bool HasAuthorAboutText { get { return !String.IsNullOrEmpty(AuthorAboutText); } }
    # endregion
    # region Book Content
    /// <summary>
    /// One or more content pieces, each described with an identifier and a name. 
    /// </summary>
    public List<ContentFile> Chapters { get; set; }
    public string AllContent {
      get {
        var sb = new StringBuilder();
        Chapters.ForEach(c => sb.Append(Encoding.UTF8.GetString(c.Content)));
        return sb.ToString();
      }
    }
    public List<TocElement> ToC { get; set; }
    public List<ImageFile> Images { get; set; }
    # endregion
    # region Helper Data
    public string TemplateName { get; set; }
    public byte[] AuthorImage { get; set; }
    public byte[] CoverImage { get; set; }
    public string Keywords { get; set; }
    public string CoverDescription { get; set; }
    public string PermaLink { get; set; }
    public IList<Pricing> PriceTable { get; set; }
    public IList<Template> Templates { get; set; }
    # endregion
    # region Production Data
    public string BlobStorePath { get; set; }
    public string TempStorePath { get; set; }
    # endregion

    public string LocaleId { get; set; }

    public string Rights { get; set; }

    public string FrontCoverText { get; set; }

    public string FrontCoverForeColor { get; set; }

    public string CopyEditorNamesVerbose { get; set; }

    public string Version { get; set; }

    public string PrintDate { get; set; }

    public string Updatelink { get; set; }

    public byte[] QRImg { get; set; }

    public byte[] BarCodeImg { get; set; }

    public string CoverImg { get; set; }

    public string CallingUser { get; set; }

    /// <summary>
    /// The relative path used to store CSS in other locations as the current directory. Used in EPUB to organize doc containers properly.
    /// </summary>
    public string CssPath { get; set; }

    /// <summary>
    /// The relative path used to store scripts (ECMAScript) in other locations as the current directory. Used in EPUB to organize doc containers properly.
    /// </summary>
    public string ScriptPath { get; set; }

    /// <summary>
    /// The caller can provide an XML element and this method replaces all &lt;t:variables&gt; using the current printable's values.
    /// </summary>
    /// <param name="xml"></param>
    public void FillVariables(XElement xml) {
      var imageContainer = new Dictionary<string, string>();
      // design
      var cssPath = String.IsNullOrEmpty(CssPath) || CssPath.EndsWith("/") ? "" : CssPath + "/"; //trailing / vor path concatenation
      xml.Descendants(ConstNsTexxtoor + "style").ToList().ForEach(t => t.ReplaceWith(new XElement("link", new XAttribute("rel", "stylesheet"), new XAttribute("type", "text/css"), new XAttribute("href", cssPath + t.Attribute("src").Value))));
      // content
      imageContainer.Add("cover", Convert.ToBase64String(this.CoverImage ?? File.ReadAllBytes(HttpContext.Current.Server.MapPath("~/Content/images/blank.gif"))));
      imageContainer.Add("author", Convert.ToBase64String(this.AuthorImage ?? File.ReadAllBytes(HttpContext.Current.Server.MapPath("~/Content/images/blank.gif"))));
      imageContainer.Add("qr", Convert.ToBase64String(this.QRImg ?? File.ReadAllBytes(HttpContext.Current.Server.MapPath("~/Content/images/blank.gif"))));
      imageContainer.Add("barcode", Convert.ToBase64String(this.BarCodeImg ?? File.ReadAllBytes(HttpContext.Current.Server.MapPath("~/Content/images/blank.gif"))));
      xml.Descendants(ConstNsTexxtoor + "title").ToList().ForEach(t => t.ReplaceWith(this.Title));
      xml.Descendants(ConstNsTexxtoor + "subtitle").ToList().ForEach(t => t.ReplaceWith(this.SubTitle ?? String.Empty));
      xml.Descendants(ConstNsTexxtoor + "version").ToList().ForEach(t => t.ReplaceWith(this.Version));
      xml.Descendants(ConstNsTexxtoor + "printdate").ToList().ForEach(t => t.ReplaceWith(this.PrintDate));
      xml.Descendants(ConstNsTexxtoor + "biography").ToList().ForEach(t => t.ReplaceWith(this.AuthorBiography ?? String.Empty)); // TODO: type=lead
      xml.Descendants(ConstNsTexxtoor + "authorname").ToList().ForEach(t => t.ReplaceWith(this.AuthorNamesShort ?? String.Empty)); // TODO: type=lead
      xml.Descendants(ConstNsTexxtoor + "authornames").ToList().ForEach(t => t.ReplaceWith(this.AuthorNamesVerbose ?? String.Empty)); // TODO: type=lead
      xml.Descendants(ConstNsTexxtoor + "covertext").ToList().ForEach(t => t.ReplaceWith(this.FrontCoverText ?? String.Empty)); // TODO: maxlength, ellipsis
      xml.Descendants(ConstNsTexxtoor + "copyeditor").ToList().ForEach(t => t.ReplaceWith(this.CopyEditorNamesVerbose ?? String.Empty));
      xml.Descendants(ConstNsTexxtoor + "publisher").ToList().ForEach(t => t.ReplaceWith(this.Publisher ?? String.Empty));
      xml.Descendants(ConstNsTexxtoor + "isbn").ToList().ForEach(t => t.ReplaceWith(this.Isbn ?? String.Empty));
      xml.Descendants(ConstNsTexxtoor + "updatelink").ToList().ForEach(t => t.ReplaceWith(this.Updatelink ?? String.Empty));
      xml.Descendants(ConstNsTexxtoor + "keywords").ToList().ForEach(t => t.ReplaceWith(this.Keywords ?? String.Empty));
      if (this.PriceTable != null && this.PriceTable.Any()) {
        for (int i = 0; i < this.PriceTable.Count(); i++) {
          var priceElement = xml
            .Descendants(ConstNsTexxtoor + "pricetable")
            .SingleOrDefault(t => Int32.Parse(t.Attribute("item").Value) == i);
          if (priceElement != null) {
            priceElement.ReplaceWith(this.PriceTable[i].ToString());
          }
        }
      }
      xml.Descendants(ConstNsTexxtoor + "img").ToList().ForEach(t => t.ReplaceWith(new XElement("img", new XAttribute("src", "data:image/png;base64," + imageContainer[t.Attribute("src").Value]))));
    }

    public void FillContent(XElement xml, string content, Func<String, String> embedImagesAsData, Func<String, String> resolveHyperlinks) {
      var rawContent = content.ToHtmlNumericEntity(true);
      try {
        // we need to fix this because CKEdit will probably not expose clean XML
        var cleanUpHtml = new HtmlDocument() {
          OptionFixNestedTags = true,
          OptionOutputAsXhtml = true,
          OptionOutputAsXml = true,
        };
        cleanUpHtml.LoadHtml(rawContent);
        var sb = new StringBuilder();
        var settings = new XmlWriterSettings {OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment};
        using (XmlWriter xWriter = XmlWriter.Create(sb, settings)) {
          cleanUpHtml.Save(xWriter);
          rawContent = sb.ToString();
        }
      } catch (Exception ex) {
        rawContent = "Could not process text. Reason: " + ex.Message;
      } 
      // need content before adding images
      xml.Descendants(ConstNsTexxtoor + "content").ToList().ForEach(t => t.ReplaceWith(XElement.Parse("<div>" + rawContent + "</div>")));
      // replace references with inline images (to avoid temp files)
      var images = xml.Descendants("img").Where(e => e.Attribute("class").NullSafeString() == "builder");
      foreach (var image in images) {
        var src = image.Attribute("src").Value;
        if (src.StartsWith("data:")) continue;
        // convert only if source is not yet inline and requested
        if (embedImagesAsData == null) {
          try {
            var base64 = Convert.ToBase64String(this.Images.Single(i => i.Href == Path.GetFileName(src)).Content);
            image.Attribute("src").Value = "data:image/png;base64," + base64;
          }
          catch (Exception ex) {
          }
        }
        else {
          var newSrc = embedImagesAsData(src);
          image.Attribute("src").Value = newSrc;
        }
      }
      //  <a class="innerLink Section" href="#Section-8230" data-type="Section" data-snippet="8230">Einleitung</a>
      if (resolveHyperlinks != null) {
        var links = xml.Descendants("a").Where(e => e.Attribute("data-type").NullSafeString() == "Section");
        foreach (var link in links) {
          link.Attribute("href").Value = resolveHyperlinks(link.Attribute("data-snippet").Value);
        }
      }
    }
    
    public void CleanUpVariables(XElement xml) {
      xml.Descendants(ConstNsTexxtoor + "configuration").Remove();
    }

  }

}
