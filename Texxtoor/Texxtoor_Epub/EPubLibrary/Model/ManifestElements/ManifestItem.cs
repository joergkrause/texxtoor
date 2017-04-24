using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.Models.BaseEntities.Epub;

namespace Texxtoor.BaseLibrary.EPub.Model {
  /// <summary>
  /// An abstract factory. Use Image.Create().
  /// </summary>
  [Table("ManifestItem", Schema = "Epub")]
  public abstract class ManifestItem : EntityBase {

    public ManifestItem() {
    }

    public ManifestItem(string contentType) {
      this.MediaType = contentType;
    }

    /// <summary>
    /// The actual content of the Item.
    /// </summary>
    [Column(TypeName="image")]
    public byte[] Data { get; set; }

    # region Properties according to Epub 3 spec
    /// <summary>
    /// he ID [XML] of this element, which must be unique within the document scope.
    /// </summary>
    [EPub("id")]
    [StringLength(128)]
    public virtual string Identifier { get; set; }
    /// <summary>
    /// An IRI [RFC3987] specifying the location of the Publication Resource described by this item.
    /// </summary>
    [EPub("href")]
    [StringLength(2048)]
    public virtual string Href { get; set; }
    /// <summary>
    /// An IDREF [XML] that identifies the fallback for a non-Core Media Type.
    /// </summary>
    [EPub("fallback")]
    [StringLength(128)]
    public string Fallback { get; set; }
    /// <summary>
    /// A media type [RFC2046] that specifies the type and format of the Publication Resource described by this item.
    /// </summary>
    [EPub("media-type")]
    [StringLength(128)]
    public virtual string MediaType { get; set; }
    /// <summary>
    /// A space-separated list of property values.
    /// </summary>
    [EPub("properties")]
    [StringLength(512)]
    public string Properties { get; set; }
    /// <summary>
    /// An IDREF [XML] that identifies the Media Overlay Document for the resource described by this item.
    /// </summary>
    [EPub("media-overlay")]
    [StringLength(128)]
    public string MediaOverLay { get; set; }
    # endregion
    
    [NotMapped]
    public virtual string DefaultExtension { get; protected set; }

    /// <summary>
    /// Creates a concrete type based on an enum. 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static ManifestItem Create(FileItemType type) {
      switch (type) {
        case FileItemType.JPEG:
          return new ImageJpeg();
        case FileItemType.GIF:
          return new ImageGif();
        case FileItemType.PNG:
          return new ImagePng();
        case FileItemType.XHTML:
          return new ContentFile();
        case FileItemType.OTF:
          return new FontFile();
        case FileItemType.TTF:
          return new FontFile("font/ttf");
        case FileItemType.CSS:
          return new CssFile();
        case FileItemType.XPGT:
          return new PageTemplateFile();
        default:
          throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Create a specific concrete type by filename. Usefule when you know you've done something like
    /// parse and &ltimg /&gt; tag--just pass the src in here. 
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static ManifestItem Create(string href) {
      ManifestItem mi = null;
      string filename = href.ToUpperInvariant();
      if (filename.EndsWith(".JPG") || filename.EndsWith(".JPEG")) {
        mi = ManifestItem.Create(FileItemType.JPEG);
      }
      if (filename.EndsWith(".GIF")) {
        mi = ManifestItem.Create(FileItemType.GIF);
      }
      if (filename.EndsWith(".PNG")) {
        mi = ManifestItem.Create(FileItemType.PNG);
      }
      if (filename.EndsWith("HTML") || filename.EndsWith("XHTML") || filename.EndsWith("XML") || filename.EndsWith("HTM")) {
        mi = ManifestItem.Create(FileItemType.XHTML);
      }
      if (filename.EndsWith("CSS")) {
        mi = ManifestItem.Create(FileItemType.CSS);
      }
      if (filename.EndsWith("OTF")) {
        mi = ManifestItem.Create(FileItemType.OTF);
      }
      if (filename.EndsWith("TTF")) {
        mi = ManifestItem.Create(FileItemType.TTF);
      }
      if (filename.EndsWith("XPGT")) {
        // http://wiki.mobileread.com/wiki/XPGT
        mi = ManifestItem.Create(FileItemType.XPGT);
      }
      if (mi == null) {
        throw new NotImplementedException();
      } else {
        mi.Href = href;
      }
      return mi;
    }

  }



}
