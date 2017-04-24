using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Author {

  /// <summary>
  /// Used to manage direct payments between contributors.
  /// </summary>
  [Table("Invoice", Schema = "Author")]
  public class Invoice : EntityBase {

    public Invoice() {
      Positions = new List<InvoicePosition>();
    }

    /// <summary>
    /// The sender of the invoice
    /// </summary>
    [Required]
    public virtual User Debitor { get; set; }

    /// <summary>
    /// The receiver of the invoice
    /// </summary>
    [Required]
    [ScaffoldColumn(false)]
    public virtual User Creditor { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Invoice_WithTax_With_Tax", Description="Invoice_WithTax_With_Tax_Helptext")]
    public bool WithTax { get; set; }

    [Range(0, 100)]
    [Display(ResourceType = typeof(ModelResources), Name = "Invoice_TaxPercentage_Value_added_Tax", Description="Invoice_TaxPercentage_Value_added_Tax_Helptext")]
    [ScaffoldColumn(false)]
    public decimal TaxPercentage { get; set; }

    [Column(TypeName = "datetime2")]
    [Display(ResourceType = typeof(ModelResources), Name = "Invoice_BillingDate_Billing_Date", Description="Invoice_BillingDate_Billing_Date_Helptext")]
    public DateTime BillingDate { get; set; }

    [Range(0, 365)]
    [Display(ResourceType = typeof(ModelResources), Name = "Invoice_DueDays_Due_in__days_", Description="Invoice_DueDays_Due_in__days__Helptext")]
    public int DueDays { get; set; }

    public virtual List<InvoicePosition> Positions { get; set; }

    [StringLength(500)]
    [Display(ResourceType = typeof(ModelResources), Name = "Invoice_Text_Text", Description="Invoice_Text_Text_Helptext")]
    public string Text { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Invoice_Paid_Has_been_paid", Description="Invoice_Paid_Has_been_paid_Helptext")]
    [ScaffoldColumn(false)]
    public bool Paid { get; set; }

    /// <summary>
    /// Tag as deleted so it doesn't appear in public lists.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "Invoice_Deleted_Deleted", Description="Invoice_Deleted_Deleted_Helptext")]
    [ScaffoldColumn(false)]
    public bool Deleted { get; set; }

    /// <summary>
    /// Check for a name whether for this invoice this particial user is the creditor.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public bool IsIncomingFor(string userName) {
      return Creditor.UserName == userName;
    }

    public decimal Total {
      get {
        return Positions.Sum(p => p.Amount);
      }
    }

  }

  [Table("InvoicePosition", Schema = "Author")]
  public class InvoicePosition : EntityBase {

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "InvoicePosition_Amount_Amount", Description="InvoicePosition_Amount_Amount_Helptext")]
    public decimal Amount { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "InvoicePosition_Text_Text", Description="InvoicePosition_Text_Text_Helptext")]
    [StringLength(300)]
    public string Text { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "InvoicePosition_Position_Position", Description="InvoicePosition_Position_Position_Helptext")]
    public int Position { get; set; }

  }
}
