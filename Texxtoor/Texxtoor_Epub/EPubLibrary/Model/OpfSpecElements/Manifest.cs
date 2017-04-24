using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Texxtoor.Editor.Core.Extensions.Epub;
using Texxtoor.Models.BaseEntities.Epub;

namespace Texxtoor.BaseLibrary.EPub.Model {

  /// <summary>
  /// The manifest element provides an exhaustive list of the Publication Resources that constitute the EPUB Publication, each represented by an item element.
  /// </summary>
  [Table("Manifest", Schema = "Epub")]
  public class Manifest : EntityBase {

    internal static Manifest CreateManifest(ZipStorer gz, XElement e, string ncxToExclude, string opsFolder) {
      Manifest mf = new Manifest();
      // Id
      mf.Identifier = Helper.NullSaveString(e.Attribute("id"));
      mf.Items = new List<ManifestItem>();
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
      var xe = new XElement("manifest",
        mf.Items.Select(item =>
            new XElement("item",
              new XAttribute("id", item.Identifier),
              new XAttribute("href", item.Href),
              new XAttribute("media-type", item.MediaType)
              )
            )
          );
      return xe;
    }

    private static ManifestItem ReadFileContent(ZipStorer gz, XElement item, string opsFolder) {
      string href = Helper.NullSaveString(item.Attribute(Helper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.Href)), true);
      ManifestItem fi = ManifestItem.Create(href);
      fi.Identifier = Helper.NullSaveString(item.Attribute(Helper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.Identifier)));
      fi.MediaOverLay = Helper.NullSaveString(item.Attribute(Helper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.MediaOverLay)));
      fi.Fallback = Helper.NullSaveString(item.Attribute(Helper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.Fallback)));
      fi.MediaType = Helper.NullSaveString(item.Attribute(Helper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.MediaType)));
      using (MemoryStream ms = new MemoryStream()) {
        var entry = gz.ReadCentralDir().First(file => file.FilenameInZip == Helper.CreatePath(opsFolder, href));
        gz.ExtractFile(entry, ms);
        ms.Position = 0;
        fi.Data = ms.ToArray();
      }
      fi.Properties = Helper.NullSaveString(item.Attribute(Helper.GetAttributeName<ManifestItem, EPubAttribute>(m => m.Properties)));
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
    public IList<ManifestItem> Items { get; set; }

  }

}
