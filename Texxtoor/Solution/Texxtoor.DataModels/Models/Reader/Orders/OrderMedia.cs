using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using System.ComponentModel;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.DataModels.Models.Reader.Orders {

  # region Order Management

  /// <summary>
  /// Add media specific data here
  /// </summary>
  [Table("OrderMedia", Schema = "Shop")]
  public class OrderMedia : EntityBase {

    public OrderMedia() {
      Available = true;
    }

    /// <summary>
    /// Name of media
    /// </summary>
    [Required]
    [StringLength(100)]
    [Description("Name of Media")]
    public string Name { get; set; }

    public decimal PriceBase { get; set; }

    public bool Available { get; set; }

    public System.Collections.Generic.ICollection<OrderProduct> Products { get; set; }

    public System.Collections.Generic.ICollection<Published> AllowsForPublished { get; set; }
  }

  # endregion

}