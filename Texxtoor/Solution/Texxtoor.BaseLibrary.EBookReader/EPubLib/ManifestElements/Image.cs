using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Drawing.Imaging;

namespace Texxtoor.BaseLibrary.EPub.Model {

  [Table("ManifestItem", Schema = "Epub")]
  public abstract class ImageFile : ManifestItem {
    protected ImageFile(string contentType)
      : base(contentType) {
    }

    public static ImageFile CreateImageFile(string type, byte[] raw) {
      using (var ms = new MemoryStream(raw)) {
        using (var data = Image.FromStream(ms)) {
          var rs = new MemoryStream();
          switch (type.ToLower()) {
            case "jpeg":
            case "jpg":
              data.Save(rs, ImageFormat.Jpeg);
              return new ImageJpeg {Data = rs.ToArray()};
            case "gif":
              data.Save(rs, ImageFormat.Gif);
              return new ImageGif {Data = rs.ToArray()};
            case "png":
              data.Save(rs, ImageFormat.Png);
              return new ImagePng {Data = rs.ToArray()};
          }
        }
      }
      throw new NotSupportedException("type");
    }
  }

  [Table("ManifestItem", Schema = "Epub")]
  public class ImageJpeg : ImageFile {
    internal ImageJpeg()
      : base("image/jpeg") {
      base.DefaultExtension = ".jpg";
    }
  }

  [Table("ManifestItem", Schema = "Epub")]
  public class ImageGif : ImageFile {
    internal ImageGif()
      : base("image/gif") {
      base.DefaultExtension = ".gif";
    }
  }

  [Table("ManifestItem", Schema = "Epub")]
  public class ImagePng : ImageFile {
    internal ImagePng()
      : base("image/png") {
      base.DefaultExtension = ".png";
    }
  }

}
