using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.BaseLibrary.EPub.Model {
  [Table("ManifestItem", Schema = "Epub")]
  public class ContentFile : ManifestItem, IComparable<ContentFile> {
    public ContentFile()
      : base("application/xhtml+xml") {
    }

    /// <summary>
    /// The title of the chapter
    /// </summary>
    [StringLength(512)]
    public string Title { get; set; }

    /// <summary>
    /// The chapter number, starting from 1.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// The HTML body, as UTF-8.
    /// </summary>
    [Column(TypeName = "ntext")]
    public string Document {
      get { return Data != null ? Encoding.UTF8.GetString(Data) : String.Empty; }
      set { Data = Encoding.UTF8.GetBytes(value); }
    }

    #region IComparable<Content> Members

    /// <summary>
    /// Used for sorting by chapter number
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(ContentFile other) {
      return Number.CompareTo(other.Number);
    }

    #endregion
  }


}
