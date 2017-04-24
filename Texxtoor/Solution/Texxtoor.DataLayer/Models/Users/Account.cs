using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Models.Users {

  [Table("Account", Schema = "Common")]
  public class Account : EntityBase {

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Account_Owner_User", Description="Account_Owner_User_Helptext")]
    [ScaffoldColumn(false)]
    public User Owner { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Account_Amount_Amount", Description="Account_Amount_Amount_Helptext")]
    [DataType(DataType.Currency)]
    [AdditionalMetadata("Length", 6)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(256)]
    [Display(ResourceType = typeof(ModelResources), Name = "Account_Description_Description", Description="Account_Description_Description_Helptext")]
    public string Description { get; set; }

    [StringLength(256)]
    [Display(ResourceType = typeof(ModelResources), Name = "Account_TransactionNo_Transaction_Number", Description="Account_TransactionNo_Transaction_Number_Helptext")]
    [ScaffoldColumn(false)]
    public string TransactionNo { get; set; }

    [ScaffoldColumn(false)]
    public TransactionType TransactionType { get; set; }

  }

}