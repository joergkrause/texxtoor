using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.ViewModels.Reader {
  public class PayPalConfirm {

    [Required]
    public string TransactionId { get; set; }

    /// <summary>
    /// The type of transaction Possible values: l  cart l  express-checkout 
    /// </summary>
    public string TransactionType { get; set; }

    /// <summary>
    /// Indicates whether the payment is instant or delayed. Possible values: l  none l  echeck l  instant 
    /// </summary>
    public string PaymentType { get; set; }

    /// <summary>
    /// Time/date stamp of payment
    /// </summary>
    public string OrderTime { get; set; }

    /// <summary>
    /// The final amount charged, including any shipping and taxes from your Merchant Profile.
    /// </summary>
    public string Amount { get; set; }

    /// <summary>
    /// A three-character currency code for one of the currencies listed in PayPay-Supported Transactional Currencies. Default: USD.    
    /// </summary>
    public string CurrencyCode { get; set; }

    /// <summary>
    /// PayPal fee amount charged for the transaction    
    /// </summary>
    public string FeeAmount { get; set; }

    /// <summary>
    /// Amount deposited in your PayPal account after a currency conversion.    
    /// </summary>
    public string SettleAmount { get; set; }

    /// <summary>
    /// Tax charged on the transaction.    
    /// </summary>
    public string TaxAmount { get; set; }

    /// <summary>
    /// Exchange rate if a currency conversion occurred. Relevant only if your are billing in their non-primary currency.
    /// </summary>
    public string ExchangeRate { get; set; }
  }
}
