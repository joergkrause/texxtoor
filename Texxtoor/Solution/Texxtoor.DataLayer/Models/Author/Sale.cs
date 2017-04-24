using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Author {

  /// <summary>
  /// Everything we need to show sales data to authors
  /// </summary> 
  [Table("Sales", Schema = "Author")]
  public class Sale : EntityBase {

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Sale_SourceOpus_Text_Name", Description = "Sale_SourceOpus_Text_Name_Helptext")]
    public virtual Opus SourceOpus { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Sale_OrderProduct_Purchase_Item", Description = "Sale_OrderProduct_Purchase_Item_Helptext")]
    public virtual OrderProduct OrderProduct { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Sale_ReaderCount_Item_was_read", Description = "Sale_ReaderCount_Item_was_read_Helptext")]
    public bool ReaderCount { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Sale_SubscriptionCount_Subscription_delivered", Description = "Sale_SubscriptionCount_Subscription_delivered_Helptext")]
    public bool SubscriptionCount { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Sale_DownloadCount_Download", Description = "Sale_DownloadCount_Download_Helptext")]
    public bool DownloadCount { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Sale_OrderCount_Regular_Order", Description = "Sale_OrderCount_Regular_Order_Helptext")]
    public bool OrderCount { get; set; }

    /// <summary>
    /// All revenues this item has with this particular transaction
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Sale_TotalRevenues_Total_Revenues", Description = "Sale_TotalRevenues_Total_Revenues_Helptext")]
    [DataType(DataType.Currency)]
    public decimal TotalRevenues { get; set; }

    /// <summary>
    /// Calculated share this item for the owner of this sale
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Sale_UserRevenues_User_Revenues", Description = "Sale_UserRevenues_User_Revenues_Helptext")]
    [DataType(DataType.Currency)]
    public decimal UserRevenues { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Sale_Owner_Owner", Description = "Sale_Owner_Owner_Helptext")]
    [Required]
    public virtual User Owner { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Sale_Day_Purchase_Day", Description = "Sale_Day_Purchase_Day_Helptext")]
    [DataType(DataType.Date)]
    public DateTime Day { get; set; }

  }
}
