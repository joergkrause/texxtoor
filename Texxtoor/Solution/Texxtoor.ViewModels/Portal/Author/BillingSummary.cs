using System;
using System.Collections.Generic;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.ViewModels.Author {

  // everything we need to show sales data
  public class BillingSummary  {

    /// <summary>
    /// The opus this billing cycle applies to
    /// </summary>
    public Opus Opus { get; set; }

    public bool UserIsContributor { get; set; }

    public User TeamLead { get; set; }

    /// <summary>
    /// Number of invoices available
    /// </summary>
    public int OutgoingInvoices { get; set; }

    /// <summary>
    /// Number of invoices available
    /// </summary>
    public int IncomingInvoices { get; set; }

    public decimal CreditorBalanceDue { get; set; }

    public decimal CreditorBalance { get; set; }

    public decimal DebitorBalanceDue { get; set; }

    public decimal DebitorBalance { get; set; }


  }
}
