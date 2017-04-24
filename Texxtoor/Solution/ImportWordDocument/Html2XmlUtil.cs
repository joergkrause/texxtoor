using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.HtmlAgility.ToXml;
using Texxtoor.BaseLibrary.Core.HtmlAgility.ToXml.Nodes;

namespace ImportWordDocument {

  public class ExternalDataEventArgs : EventArgs {
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public byte[] Data { get; set; }
  }

  public class Html2XmlUtil {

    public static string basePath = "";
    public static bool imageAsBase64;
    public static List<string> callBackClasses = new List<string>();

    public static EventHandler TreatSpecialElement;
    public static EventHandler<ExternalDataEventArgs> TreatExternalData;
    private static Document docDes;
    private static void OnTreatSpecialElement(EventArgs e) {
      if (TreatSpecialElement != null) {
        TreatSpecialElement(docDes, e);
      }
    }

    private static void OnTreatExternalData(ExternalDataEventArgs e) {
      if (TreatExternalData != null) {
        TreatExternalData(docDes, e);
      }
    }

    private static string GetManifestResourceName(string folder, string file) {
      var resourceName = file;
      var resourcesFolderName = folder;
      return String.Format("{0}.{1}.{2}", typeof(Html2XmlUtil).Namespace, resourcesFolderName, resourceName);
    }

    private static Stream GetManifestResourceStream(string name) {
      var assembly = typeof(Html2XmlUtil).Assembly;
      return assembly.GetManifestResourceStream(GetManifestResourceName("Xslt", name));
    }

    public static XDocument GenerateFixTableXslt(XDocument config = null) {
      var configDoc = config ?? XDocument.Load(GetManifestResourceStream("config.xml"));
      return StreamTransform(configDoc, "generator.xsl");
    }

    public static XDocument GenerateFixListingXslt(XDocument config = null) {
      var configDoc = config ?? XDocument.Load(GetManifestResourceStream("config.xml"));
      return StreamTransform(configDoc, "generator.xsl");
    }

    public static string HtmlToOpusXsltParser(string name, string html, XDocument tableFix, XDocument listingFix) {
# if DEBUG
      var basePath = Path.Combine(@"C:\Temp\", name);
      if (!Directory.Exists(basePath)) {
        Directory.CreateDirectory(basePath);
      }
      //******** STEP 0 *********//
      // The input document
      File.WriteAllText(Path.Combine(basePath, "word-html-step-0.html"), html, Encoding.UTF8);
# endif
      var step0output = XDocument.Parse(html);
      //******** STEP 1 *********//
      // Fix the Listing / Code blocks      
      var step1output = StreamTransform(step0output, listingFix);
      # region
      /*
      var argsList = new XsltArgumentList();
      /*
          <xsl:param name="codeCharacterTemplate">ListingTextZchn</xsl:param>
          <xsl:param name="codeListingTemplate">ListingText</xsl:param>
          <xsl:param name="listingCaptionTemplate">Listingunterschrift</xsl:param>
          <xsl:param name="figureCaptionTemplate">Bildunterschrift</xsl:param>
          <xsl:param name="tableCaptionTemplate">Tabellenberschrift</xsl:param>
       * 
      var mapping = new NameValueCollection();
      foreach (var key in mapping.AllKeys) {
        argsList.AddParam(key, "", mapping[key]);
      } */
      # endregion
# if DEBUG
      WriteDocumentToFile(step1output, "word-html-step-1.html");
# endif
      //******** STEP 2 *********//
      // General Transform
      var step2output = StreamTransform(step1output, "transformHtml.xsl");
# if DEBUG
      WriteDocumentToFile(step2output, "word-html-step-2.html");
# endif
      //******** STEP 3 *********//
      // Fix Tables
      var step3output = StreamTransform(step2output, tableFix);
# if DEBUG
      WriteDocumentToFile(step3output, "word-html-step-3.html");
# endif
      //******** STEP 4 *********//
      // Conjunct text
      var step4output = StreamTransform(step3output, "normalizeText.xsl");
# if DEBUG
      WriteDocumentToFile(step4output, "word-html-step-4.html");
# endif
      var ms = new MemoryStream();
      step4output.Save(ms);
      var xml = Encoding.UTF8.GetString(ms.ToArray());
      return xml;
    }

    public static XDocument HtmlToXDoc(string html, string name) {
      var docSrc = NSoupClient.Parse(html);
      var elements = docSrc.GetElementsByTag("body");
      var body = elements.First;
      var xDoc = new XDocument();
      xDoc.Add(new XElement("html",
        new XElement("head",
          new XElement("title", name)),
        XElement.Parse(body.OuterHtml())));
      return xDoc;
    }

    /// <summary>
    /// Make Html XHTML compliant and convert Images to inline base64 encoded strings to get single file result.
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static XDocument CleanUpHtmlWithResources(string html) {
      var docSrc = NSoupClient.Parse(html);
      var elements = docSrc.GetElementsByTag("body");
      var body = elements.First;
      var xDoc = new XDocument();
      xDoc.Add(new XElement("html",
        new XElement("head",
          new XElement("title", String.Format("Created from import at {0} by texxtoor", DateTime.Now.ToFileTime()))),
        XElement.Parse(body.OuterHtml())));
      // treat external data (we're looking for <img src> and replace this with inline base64 encoded stuff
      if (xDoc.Root != null) {
        xDoc.Root.Descendants("img")
          .ForEach(e => {
            var src = e.Attribute("src").Value;
            var ea = new ExternalDataEventArgs {
              FileName = Path.GetFileName(src),
              FilePath = Path.GetDirectoryName(src)
            };
            OnTreatExternalData(ea); // let the caller figure out how to retrieve the image
            e.Attribute("src").SetValue(String.Format("data:image/png;base64,{0}", Convert.ToBase64String(ea.Data)));
          });
      }
      // after this we remove WordSection divs to further normalize the document
      return StreamTransform(xDoc, "NormalizeWordXml.xslt");
    }

    private static XDocument StreamTransform(XDocument inputXml, string transformXsltName) {
      var transform = new XslCompiledTransform();
      var settings = new XsltSettings {
        EnableScript = true        
      };     
      var writerSettings = new XmlWriterSettings {
        Indent = true,
        ConformanceLevel = ConformanceLevel.Auto,
        OmitXmlDeclaration = false
      };
      var sb = new StringBuilder();
      using (var writer = XmlWriter.Create(sb, writerSettings)) {
        
        var sheetReader = XmlReader.Create(GetManifestResourceStream(transformXsltName));
        transform.Load(sheetReader, settings, null);        
        transform.Transform(inputXml.CreateReader(), writer); 
      }
      var targetDoc = XDocument.Parse(sb.ToString());
      return targetDoc;
    }

    private static XDocument StreamTransform(XDocument inputXml, XDocument transformXslt) {
      var transform = new XslCompiledTransform();
      var settings = new XsltSettings {
        EnableScript = true
      };
      var writerSettings = new XmlWriterSettings {
        Indent = true,
        ConformanceLevel = ConformanceLevel.Auto,
        OmitXmlDeclaration = false
      };
      var sb = new StringBuilder();
      using (var writer = XmlWriter.Create(sb, writerSettings)) {

        var sheetReader = transformXslt.CreateReader();
        transform.Load(sheetReader, settings, null);
        transform.Transform(inputXml.CreateReader(), writer);
      }
      var targetDoc = XDocument.Parse(sb.ToString());
      return targetDoc;
    }

    private static void WriteDocumentToFile(XDocument xDoc, string name) {
      var writer = XmlWriter.Create(name, new XmlWriterSettings());
      writer.Settings.Indent = true;
      xDoc.Save(writer);
    }


  }
}
