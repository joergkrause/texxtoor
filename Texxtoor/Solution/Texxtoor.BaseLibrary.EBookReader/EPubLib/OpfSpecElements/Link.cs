using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.BaseLibrary.EPub.Model {


  /// <summary>
  /// The link element is used to associate resources with a Publication, such as metadata records.
  /// </summary>
  [Table("Link", Schema = "Epub")]
  public class Link : EntityBase {
    public string Href { get; set; }
    public string Rel { get; set; }
    public string Identifier { get; set; }
    public string Refines { get; set; }
    public string MediaType { get; set; }
    public MetaData MetaData { get; set; }
  }
}
