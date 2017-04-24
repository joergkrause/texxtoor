using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.Models.BaseEntities.Epub;

namespace Texxtoor.BaseLibrary.EPub.Model {


  [Table("Bindings", Schema = "Epub")]
  public class Binding : EntityBase {

    public IList<MediaTypeElement> MediaTypes { get; set; }

  }


  [Table("MediaTypeElements", Schema = "Epub")]
  public class MediaTypeElement : EntityBase {

    [StringLength(128)]
    public string MediaType { get; set; }
    
    [StringLength(512)]
    public string Handler { get; set; }

  }
}
