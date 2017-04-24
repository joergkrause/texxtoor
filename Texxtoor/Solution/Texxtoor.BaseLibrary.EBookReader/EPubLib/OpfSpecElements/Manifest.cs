using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.BaseLibrary.Core.Extensions;

namespace Texxtoor.BaseLibrary.EPub.Model {

  /// <summary>
  /// The manifest element provides an exhaustive list of the Publication Resources that constitute the EPUB Publication, each represented by an item element.
  /// </summary>
  [Table("Manifest", Schema = "Epub")]
  public class Manifest : EntityBase {

    internal static Manifest CreateManifest(ZipArchive gz, XElement e, string ncxToExclude, string opsFolder) {
      var mf = new Manifest{Identifier = e.Attribute("id").NullSafeString(), Items = new List<ManifestItem>()};
      // Id
      // get items, exclude ncx (will be processed separately)
      XNamespace ns = e.Document.Root.GetDefaultNamespace();
      var items = e.Elements(ns + "item")
          .Where(i => i.Attribute("id").Value != ncxToExclude)
          .Select(i => i).ToList();
      // store
      items.ForEach(item => ((List<ManifestItem>)mf.Items).Add(ReadFileContent(gz, item, opsFolder)));
      return mf;
    }

    internal static XElement CreateXElement(Manifest mf) {
      var xe = new XElement(OpfPackage.OpfNameSpace + "manifest",
        mf.Items.Select(item =>
            new XElement(OpfPackage.OpfNameSpace + "item",
              new XAttribute("id", item.Identifier),
              new XAttribute("href", item.Href.Replace(" ", "_")),
              new XAttribute("media-type", item.MediaType)
              )
            )
          );
      return xe;
    }

    private static ManifestItem ReadFileContent(ZipArchive gz, XElement item, string opsFolder) {
      string href = item.Attribute(XmlHelper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.Href)).NullSafeString(true);
      ManifestItem fi = ManifestItem.Create(href);
      fi.Identifier = item.Attribute(XmlHelper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.Identifier)).NullSafeString();
      fi.MediaOverLay = item.Attribute(XmlHelper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.MediaOverLay)).NullSafeString();
      fi.Fallback = item.Attribute(XmlHelper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.Fallback)).NullSafeString();
      fi.MediaType = item.Attribute(XmlHelper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.MediaType)).NullSafeString();      
        var entry = gz.Entries.First(file => file.Name == opsFolder.CreatePath(href));
      using (var ms = entry.Open()) {
        ms.Position = 0;
        var buffer = new byte[(int)ms.Length];
        ms.Read(buffer, 0, buffer.Length);
        fi.Data = buffer;
      }
      fi.Properties = item.Attribute(XmlHelper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.Properties)).NullSafeString();
      return fi;
    }
    
    /// <summary>
    /// The ID [XML] of this element, which must be unique within the document scope.
    /// </summary>
    [StringLength(128)]
    [EPub("id")]
    public string Identifier { get; set; }

    /// <summary>
    /// One or more item elements [required].
    /// </summary>
    public virtual IList<ManifestItem> Items { get; set; }

  }

}
