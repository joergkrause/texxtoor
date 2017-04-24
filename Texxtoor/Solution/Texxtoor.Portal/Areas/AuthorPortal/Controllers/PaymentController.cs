using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Mashup.Payment.Adaptive;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AuthorPortal.Controllers {
  public class PaymentController : ControllerExt {

    private readonly Func<TransactionType, string> _localizeEnum = e => {
      var disp =
        typeof(TransactionType).GetField(Enum.GetName(typeof(TransactionType), e))
                         .GetCustomAttributes(true)
                         .OfType<DisplayAttribute>()
                         .Single();
      return disp.ResourceType.GetProperty(disp.Name).GetValue(null).ToString();
    };

    # region --== Account Management ==--



    # endregion --== Account Management ==--

    # region --== Payment ==--

    [NavigationPathFilter("Payments")]
    public ActionResult Index() {
      var types = String.Join(", ",
          Enum.GetValues(typeof(TransactionType))
              .OfType<TransactionType>()
              .OrderByDescending(t => t)
              .Select(t => String.Format("{0}: '{1}'", (int)t, _localizeEnum(t)))
              .ToArray());
      var profile = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName);
      ViewBag.NoPaypal = String.IsNullOrEmpty(profile.PayPalUserId); 
      ViewBag.TransactionTypesAsJsonForSearch = types + ", 99: 'All'";
      ViewBag.Balance = UnitOfWork<AccountingManager>().GetCurrentCreditAmountForUser(UserName);
      return View();
    }
    /// <summary>
    /// Retrieve all transaction stored in Accounts.
    /// </summary>
    /// <returns></returns>
    public JsonResult AccountTransaction([Bind(Prefix = "_search")] bool? search, TransactionType? transactiontype, string amount, string description, int? rows, int? page, string sidx, string sord) {
      var p = page.HasValue ? page.Value : 1;
      var r = rows.HasValue ? rows.Value : 20;
      decimal amnt = 0m;
      Decimal.TryParse(amount, out amnt);
      var acc = search.GetValueOrDefault() ? UnitOfWork<AccountingManager>().GetTransactionsForUser(UserName, transactiontype.GetValueOrDefault(), amnt, description) : UnitOfWork<AccountingManager>().GetAllTransactionsForUser(UserName);

      var model = new {
        total = Math.Ceiling((decimal)acc.Count() / r),
        page = p,
        records = acc.Count(),
        rows = acc
          .Select(t => new {
            id = t.Id,
            cell = new[] { _localizeEnum(t.TransactionType), t.Amount.ToString(new CultureInfo(CurrentCulture)), t.CreatedAt.ToShortDateString(), t.Description, t.TransactionNo }
          })
      };
      return Json(model, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// this view display user's current credit amount.
    /// </summary>
    /// <returns></returns>
    public ActionResult WithdrawAmount() {
      var acc = new Account { Amount = UnitOfWork<AccountingManager>().GetCurrentCreditAmountForUser(UserName) };
      return PartialView("Lists/_WithdrawAmount", acc);
    }

    /// <summary>
    /// Withdraw credit amount.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public JsonResult WithdrawAmount(Account acc) {
      var profile = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName);
      var currentcreditamount = UnitOfWork<AccountingManager>().GetCurrentCreditAmountForUser(UserName);

      if (!String.IsNullOrEmpty(profile.PayPalUserId)) {
        if (acc.Amount > 0 && acc.Amount <= currentcreditamount) {

          var withdrawal = new AdaptivePayPalAPI();

          var senderPayPalId = WebConfigurationManager.AppSettings["paypal:PayPalUserId"].ToString(CultureInfo.InvariantCulture); //set Texxtoor PayPal User Id which add in web.config file.
          var receiverPayPalId = profile.PayPalUserId.Trim();
          var widthrawalAmount = acc.Amount.ToString(CultureInfo.InvariantCulture);
          var retMsg = "";
          var returnUrl = Url.Action("Index");
          var cancelUrl = Url.Action("Index");
          //implicitly transfer withdrawal amount to user's paypal account
          var isSucceed = withdrawal.AdaptivePay(senderPayPalId, receiverPayPalId, widthrawalAmount, returnUrl, cancelUrl, ref retMsg);

          if (isSucceed) {
            //Log the Internal transaction.
            acc.Amount = -acc.Amount; //we stored negetive amount for debit transactions.
            UnitOfWork<AccountingManager>().AddAccountTransactions(acc.Amount, TransactionType.ToBankAccount, UserName);
            return Json(new { msg = ControllerResources.PaymentController_WithdrawAmount_SuccessTransfer });
          }
          return Json(new { msg = ControllerResources.PaymentController_WithdrawAmount_AmountInvalid, detail = retMsg });
        }
        return Json(new { msg = ControllerResources.PaymentController_WithdrawAmount_ErrorOccured });
      }
      return Json(new { msg = ControllerResources.PaymentController_WithdrawAmount_NoAccount });
    }
    # endregion --== Payment ==--

  }
}
