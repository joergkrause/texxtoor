using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.BaseLibrary.EPub.Model {

  [Table("ManifestItem", Schema = "Epub")]
  public class CssFile : ManifestItem {
    public CssFile()
      : base("text/css") {

    }
  }

  [Table("ManifestItem", Schema = "Epub")]
  public class PageTemplateFile : ManifestItem {
    public PageTemplateFile()
      : base("application/adobe-page-template+xml") {

    }
  }

}
