using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.BaseLibrary.EPub.Model {

  [Table("ManifestItem", Schema = "Epub")]
  public abstract class Image : ManifestItem {
    protected Image(string contentType)
      : base(contentType) {
    }
  }

  [Table("ManifestItem", Schema = "Epub")]
  public class ImageJpeg : Image {
    internal ImageJpeg()
      : base("image/jpeg") {
      base.DefaultExtension = ".jpg";
    }
  }

  [Table("ManifestItem", Schema = "Epub")]
  public class ImageGif : Image {
    internal ImageGif()
      : base("image/gif") {
      base.DefaultExtension = ".gif";
    }
  }

  [Table("ManifestItem", Schema = "Epub")]
  public class ImagePng : Image {
    internal ImagePng()
      : base("image/png") {
      base.DefaultExtension = ".png";
    }
  }

}
