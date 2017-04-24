using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texxtoor.DataModels.Models.Reader.Orders {

  /// <summary>
  /// Handle the states while producing and handling a product. This is an "all or nothing" state, if various media are involved.
  /// </summary>
  [Flags]
  public enum FullFillmentState {
    /// <summary>
    /// To get all
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "FullFillmentState_NoneOrAll_None")]
    NoneOrAll = 0,
    /// <summary>
    /// While user is in the middle of the order process
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "FullFillmentState_PreOrder_PreOrder")]
    PreOrder = 1,
    /// <summary>
    /// Done, but not yet ordered
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "FullFillmentState_Created_Created")]
    Created = 2,
    /// <summary>
    /// Payment done
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "FullFillmentState_Payed_Payed")]
    Payed = 4,    
    /// <summary>
    /// Started creating files
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "FullFillmentState_Producing_Producing")]
    Producing = 8,
    /// <summary>
    /// All instances done
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "FullFillmentState_Produced_Produced")]
    Produced = 16,
    /// <summary>
    /// Shipped
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "FullFillmentState_Shipped_Shipped")]
    Shipped = 32,
    /// <summary>
    /// Shipping provider confirmed delivery
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "FullFillmentState_Received_Received")]
    Received = 64,
    /// <summary>
    /// Cancelled by user or operator
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "FullFillmentState_Cancelled_Cancelled")]
    Cancelled = 1024

  }

}
