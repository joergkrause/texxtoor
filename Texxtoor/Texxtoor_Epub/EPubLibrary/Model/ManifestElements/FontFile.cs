using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.BaseLibrary.EPub.Model {

  [Table("ManifestItem", Schema = "Epub")]
  public class FontFile : ManifestItem {
    public FontFile()
      : base("font/otf") {
    }
    public FontFile(string mimeType)
      : base(mimeType) {

    }

  }

}
