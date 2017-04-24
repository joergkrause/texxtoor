using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Reader.Orders {

  /// <summary>
  /// Stores all information required to fullfill an order. Stores permanently even if not yet fullfilled. Acts like a perma basket.
  /// </summary>
  [ComplexType]
  public class OrderStore {

    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Store the transaction data from payment provider
    /// </summary>
    [StringLength(1024)]
    [ScaffoldColumn(false)]
    public string TransactionBag { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "OrderStore_FullFillment_Fullfillment_State", Description = "OrderStore_FullFillment_Fullfillment_State_Helptext")]
    public FullFillmentState FullFillment { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? BeginSubscription { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? EndSubscription { get; set; }

    [NotMapped]
    public bool HasSubscription {
      get {
        return BeginSubscription.HasValue;
      }
    }

    [NotMapped]
    public bool HasActiveSubscription {
      get {
        return HasSubscription && BeginSubscription.GetValueOrDefault() > DateTime.Now && EndSubscription.GetValueOrDefault() != DateTime.MinValue && EndSubscription.GetValueOrDefault() < DateTime.Now;
      }
    }

  }

}