using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.HtmlAgility.ToXml;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.HtmlAgility.Pack;
using System.Xml;

namespace Texxtoor.BaseLibrary.EPub {

  /// <summary>
  /// Create a stream from internal format.
  /// </summary>
  public static partial class EBookFactory {

    private static readonly XDeclaration XmlDeclaration = new XDeclaration("1.0", "UTF-8", "yes");

    public static Stream SaveBookToStream(EpubBook book) {
      var ms = new MemoryStream(SaveBook(book));
      return ms;
    }

    public static byte[] SaveBook(EpubBook book) {
      byte[] data = SaveBookInternal(book);
      return data;
    }

    private static byte[] SaveBookInternal(EpubBook book) {
      using (var ms = new MemoryStream()) {
        using (var gz = new ZipArchive(ms, ZipArchiveMode.Create, true, Encoding.UTF8)) {
          const string tconcx = "toc.ncx";
          // mimetype File
          var mtFile = gz.CreateEntry("mimetype", CompressionLevel.NoCompression);
          var stream = mtFile.Open();
          var bytes = Encoding.ASCII.GetBytes("application/epub+zip");
          stream.Write(bytes, 0, bytes.Length);
          stream.Close();
          // OPF File with three Elements: METADATA, MANIFEST, SPINE
          // container 
          var metaInf = GetContainerData(book.ContainerData);
          var metInfFile = gz.CreateEntry("META-INF/container.xml", CompressionLevel.Optimal);
          using (var sw = new StreamWriter(metInfFile.Open())) {
            metaInf.CopyTo(sw.BaseStream);
          }
          // ncx
          // <item id="ncxtoc" media-type="application/x-dtbncx+xml" href="toc.ncx"/>
          book.PackageData.Manifest.Items.Add(new ContentFile {
            Identifier = book.PackageData.Spine.Toc,
            Href = tconcx,
            MediaType = "application/x-dtbncx+xml"
          });
          // opf        
          var opfPath = book.ContainerData.Rootfiles.First().FullPath;
          var opfData = GetOpfData(book.PackageData);
          var opfFile = gz.CreateEntry(opfPath, CompressionLevel.Optimal);
          using (var sw = new StreamWriter(opfFile.Open())) {
            opfData.CopyTo(sw.BaseStream);
          }
          var ncxData = GetNcxData(book.GetTableOfContent(), book.PackageData.MetaData.Title.Text);
          var ncxPath = Path.GetDirectoryName(opfPath).CreatePath(tconcx);
          var ncxFile = gz.CreateEntry(ncxPath, CompressionLevel.Optimal);
          using (var sw = new StreamWriter(ncxFile.Open())) {
            ncxData.CopyTo(sw.BaseStream);
          }
          // take manifest items and create remaining files
          var oepbsPath = Path.GetDirectoryName(opfPath);
          XNamespace xmlns = "http://www.w3.org/1999/xhtml";
          var xrs = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Ignore, IgnoreWhitespace = false };
          foreach (var item in book.GetAllFiles().Where(file => file.Href != tconcx)) {
            // for HTML, caller needs to assure that it's XHTML !
            using (var fileData = new MemoryStream()) {
              fileData.Write(item.Data, 0, item.Data.Length);
              fileData.Position = 0;
              var filePath = String.Format("{0}/{1}", oepbsPath, item.Href.Replace(" ", "_"));
              var itemFile = gz.CreateEntry(filePath, CompressionLevel.Optimal);
              using (var sw = new StreamWriter(itemFile.Open())) {
                fileData.CopyTo(sw.BaseStream);
              }
            }
          }
        }
        ms.Position = 0;
        return ms.ToArray();
      }
    }

    // container.xml
    private static Stream GetContainerData(Container cnt) {
      var opfDoc = new XDocument();
      opfDoc.Declaration = XmlDeclaration;
      opfDoc.Add(Container.CreateXElement(cnt));
      var ms = new MemoryStream();
      opfDoc.Save(ms);
      ms.Position = 0;
      return ms;
    }

    // opfdata.opf
    private static Stream GetOpfData(OpfPackage package) {
      var opfDoc = new XDocument();
      opfDoc.Declaration = XmlDeclaration;
      var xe = OpfPackage.CreateXElement(package);
      xe.Add(MetaData.CreateXElement(package.MetaData));
      xe.Add(Manifest.CreateXElement(package.Manifest));
      xe.Add(Spine.CreateXElement(package.Spine));
      xe.Add(Guide.CreateXElement(package.Guide));
      opfDoc.Add(xe);
      var ms = new MemoryStream();
      opfDoc.Save(ms);
      ms.Position = 0;
      return ms;
    }

    // ncx.toc
    private static Stream GetNcxData(Navigation nav, string title) {
      var ncxDoc = new XDocument(new XDocumentType("ncx", "-//NISO//DTD ncx 2005-1//EN", "http://www.daisy.org/z3986/2005/ncx-2005-1.dtd", null));
      var ncx = Navigation.CreateXElement(nav, title);
      ncxDoc.Add(ncx);
      var ms = new MemoryStream();
      ncxDoc.Save(ms);
      ms.Position = 0;
      return ms;
    }


  }
}
