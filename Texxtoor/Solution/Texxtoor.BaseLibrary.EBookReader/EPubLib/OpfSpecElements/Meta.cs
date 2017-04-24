using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.BaseLibrary.EPub.Model {


  /// <summary>
  /// As child of the metadata element. Repeatable.
  /// </summary>
  [Table("Meta", Schema = "Epub")]
  public class Meta : EntityBase {

    /// <summary>
    /// A property.
    /// </summary>
    [StringLength(1024)]
    public string Property { get; set; }
    /// <summary>
    /// Identifies the expression or resource augmented by this element. The value of the attribute must be a relative IRI [RFC3987] pointing to the resource or element it describes.
    /// </summary>
    [StringLength(1024)]
    public string Refines { get; set; }
    /// <summary>
    /// The ID [XML] of this element, which must be unique within the document scope.
    /// </summary>
    [StringLength(128)]
    public string Identifier { get; set; }
    /// <summary>
    /// A property data type value indicating the source the value of the element is drawn from.
    /// </summary>
    [StringLength(1024)]
    public string Scheme { get; set; }

    public MetaData MetaData { get; set; }

  }

}
