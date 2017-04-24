using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core;

namespace Texxtoor.DataModels.Models {

  public enum TransactionType {
    /// <summary>
    /// Payment from sales activity (reader buys = negative amount, author gets revenue = positive amount)
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "TransactionType_Sale_Sale")]
    Sale = 1,
    /// <summary>
    /// Transfer to external account, such as PayPal
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "TransactionType_ToBankAccount_To_Bank_Account")]
    ToBankAccount = 2,
    /// <summary>
    /// Transfer from external account
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "TransactionType_FromBankAccount_From_Bank_Account")]
    FromBankAccount = 3,
    /// <summary>
    /// Transfer from or to internally, to or from another texxtoor account 
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "TransactionType_Transfer_Transfer")]
    Transfer = 4,
    /// <summary>
    /// Adjust, a system generated payment, used to cover currency exchange errors
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "TransactionType_Adjust_Adjustment")]
    Adjust = 5,
    /// <summary>
    /// Other
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "TransactionType_Other_Other")]
    Other = 29,
    /// <summary>
    /// All, for selection and search purpose.
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "TransactionType_All_All")]
    All = 99

  }

}