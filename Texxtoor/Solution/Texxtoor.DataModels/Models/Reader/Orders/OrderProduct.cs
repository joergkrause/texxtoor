using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Orders;

namespace Texxtoor.DataModels.Models.Reader.Orders {

  /// <summary>
  /// This table stores the explicitly ordered products. These are products with media specific data attached.
  /// </summary>
  [Table("OrderProduct", Schema = "Shop")]
  public class OrderProduct : BaseProduct {

    public OrderProduct() {
      PriceBase = 0M;
      RuleFactor = 1M;
      RuleDistance = 0M;
      Subscription = false;
      Media = new List<OrderMedia>();
      Store = new OrderStore();
    }

    /// <summary>
    /// The store that keeps the order for later reference. Store contains actual fullfillment data.
    /// </summary>
    [ScaffoldColumn(false)]
    public OrderStore Store { get; set; }    

    /// <summary>
    /// Medias associated with this item
    /// </summary>
    [ScaffoldColumn(false)]
    [Display(ResourceType = typeof(ModelResources), Name = "OrderProduct_Media_Media", Description = "OrderProduct_Media_Media_Helptext")]
    public List<OrderMedia> Media { get; set; }

    /// <summary>
    /// Set to indicate that this is a subscription (different pricing applies here)
    /// </summary>
    /// 
    [ScaffoldColumn(false)]
    [Display(ResourceType = typeof(ModelResources), Name = "OrderProduct_Subscription_Subscription", Description = "OrderProduct_Subscription_Subscription_Helptext")]
    public bool Subscription { get; set; }

    /// <summary>
    /// Calculated price, production cost from XML.
    /// </summary>
    [Required]
    [ScaffoldColumn(false)]
    public decimal PriceBase { get; set; }

    /// <summary>
    /// Country or region specific adjustment.
    /// </summary>
    [Required]
    [ScaffoldColumn(false)]
    public decimal RuleFactor { get; set; }

    /// <summary>
    /// Adjustment for author, usually the marketing packages base price.
    /// </summary>
    [Required]
    [ScaffoldColumn(false)]
    public decimal RuleDistance { get; set; }

    [Required]
    public OrderShippingAddress ShippingAddress { get; set; }

    public OrderInvoiceAddress BillingAddress { get; set; }

    /// <summary>
    /// The product that has been ordered with this order
    /// </summary>
    public Product SourceProduct { get; set; }

    /// <summary>
    /// The real price is calculated from amount of content, country, time using a rule engine.
    /// </summary>
    [NotMapped]
    [Display(ResourceType = typeof(ModelResources), Name = "OrderProduct_RealPrice_Real_Price", Description = "OrderProduct_RealPrice_Real_Price_Helptext")]
    [ScaffoldColumn(false)]
    [UIHint("RealPrice")]
    public decimal RealPrice {
      get {
        return PriceBase * RuleFactor + RuleDistance;
      }
    }

  }
}