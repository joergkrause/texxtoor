using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Logging;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary.Mashup.Payment.ExpressCheckout;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.JobPortal;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Author;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.ViewModels.Common;
using Texxtoor.ViewModels.Users;

namespace Texxtoor.Portal.Areas.AuthorPortal.Controllers {

  /// <summary>
  /// Manage all team functions, add members, remove members, check changes.
  /// </summary>
  [Authorize]
  public class TeamController : ControllerExt {

    #region --== Team ==--

    [NavigationPathFilter("Teams")]
    public ActionResult Index() {
      return View();
    }

    public ActionResult ListTeams(PaginationFormModel p) {
      var user = Manager<UserManager>.Instance.GetCurrentUser(UserName);
      var members = UnitOfWork<ProjectManager>().GetTeamMembersOfAllTeamsForUser(user);
      var teamOverviewModels = members as List<TeamOverviewModel> ?? members.ToList();
      ViewBag.ContributorRole = Manager<UserManager>.Instance.GetContributorTypesByMembers(teamOverviewModels, user);
      return PartialView("Teams/_List", teamOverviewModels.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult Details(int id, int? projectId) {
      var user = Manager<UserManager>.Instance.GetCurrentUser(UserName);
      var members = UnitOfWork<ProjectManager>().GetTeamMembersOfAllTeamsForUser(user);
      var team = members.FirstOrDefault(t => t.Team.Id == id);
      // editable by team leader only
      ViewBag.Editable = UnitOfWork<ProjectManager>().MemberIsTeamLead(id, UserName);
      // TODO: Filter drop downs
      ViewBag.FilterToProjectId = projectId;
      return View(team);
    }

    [MinimumAwardScoreFilter(false, 100)]
    public ActionResult AddTeamButton() {
      return PartialView("Teams/_AddTeamButton");
    }

    [MinimumAwardScoreFilter(false, 100)]
    public ActionResult AddTeam() {
      var t = new Team {
        Name = "",
        Description = ""
      };
      return PartialView("Teams/_AddTeam", t);
    }

    public ActionResult EditTeam(int id) {
      var team = UnitOfWork<ProjectManager>().GetTeam(id);
      return PartialView("Teams/_EditTeam", team);
    }

    /// <summary>
    /// Support refresh overview on team details page.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult GetTeam(int id) {
      var team = UnitOfWork<ProjectManager>().GetTeam(id);
      return Json(new
      {
        team.Name,
        team.Description
      }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult AddTeam(string name, string description) {
      var user = Manager<UserManager>.Instance.GetCurrentUser(UserName);
      UnitOfWork<ProjectManager>().CreateTeam(name, description, user);
      return Json(new { msg = String.Format(ControllerResources.TeamController_AddTeam__0__Team_successfully_created_, name) });
    }

    [HttpPost]
    public ActionResult EditTeam(int id, string name, string description, HttpPostedFileBase editTeamImage, bool? clearImage = false) {
      UnitOfWork<ProjectManager>().ChangeTeamInfo(id, "teamName", name, UserName);
      UnitOfWork<ProjectManager>().ChangeTeamInfo(id, "teamDescription", description, UserName);
      if (editTeamImage != null && !clearImage.GetValueOrDefault()) {
        UnitOfWork<ProjectManager>().SaveTeamImage(id, editTeamImage, UserName);
      } else {
        UnitOfWork<ProjectManager>().RemoveTeamImage(id, UserName);
      }
      return Json(new { msg = String.Format(ControllerResources.TeamController_EditTeam_Team_name_changed_to, name) });
    }

    public ActionResult DeleteTeam(int id) {
      var result = UnitOfWork<ProjectManager>().DeleteTeam(id, UserName);
      if (result.Kind == ResultKind.Error) {
        return new HttpNotFoundResult(result.Text);
      } else {
        return Json(new { msg = result.Text }, JsonRequestBehavior.AllowGet);
      }
    }

    public ActionResult DeactivateTeam(int id) {
      var result = UnitOfWork<ProjectManager>().DeactivateTeam(id, UserName);
      if (result.Kind == ResultKind.Error) {
        return new HttpNotFoundResult(result.Text);
      } else {
        return Json(new { msg = result.Text }, JsonRequestBehavior.AllowGet);
      }
    }

    # endregion

    # region --== Member Handling ==--

    [HttpPost]
    public ActionResult SearchMembers(int ct, PaginationFormModel p, bool? avail, bool? shared, bool? hourly, decimal from = 0, decimal to = 0) {
      var contribRole = (ContributorRole)ct;
      var users = UnitOfWork<ProjectManager>().GetContributorWithCriteria(contribRole, avail.GetValueOrDefault(), shared.GetValueOrDefault(), hourly.GetValueOrDefault(), from, to, p.Order, p.Dir);
      return PartialView("Members/_SearchResults", users.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult AddMember(int id) {
      var team = UnitOfWork<ProjectManager>().GetTeamForTeamLead(id, UserName);
      ViewBag.TeamId = id;
      return PartialView(team);
    }

    public ActionResult MemberBidding(int id) {
      ViewBag.TeamId = id;
      return View(new JobAdvertisment());
    }

    [HttpPost]
    public ActionResult MemberBidding(JobAdvertisment jobAd, int teamId) {
      if (ModelState.IsValid) {
        JobPortalManager.Instance.CreateJobAdvertisment(jobAd, UserName);
        // TODO: Send Mail
      }
      ViewBag.TeamId = teamId;
      return View(jobAd);
    }

    // id is member id
    public ActionResult RateMember(int id, int? teamId) {
      var user = UnitOfWork<UserManager>().GetUser(id);
      // set a default rating
      var prjs = UnitOfWork<ProjectManager>().GetProjectsWhereUserIsMember(user.UserName);
      var rating = new UserRating {
        Friendlyness = 5,
        Communication = 5,
        Punctuality = 5,
        Quality = 5,
        Reliability = 5
      };
      ViewBag.ProjectsForMember = prjs.ToList();
      ViewBag.TeamId = teamId;
      return View(rating);
    }

    [HttpPost]
    public ActionResult RateMember(int id, int projectId, int userId, string comment, int? fn, int? co, int? pu, int? qu, int? re) {
      // rate
      UnitOfWork<ProjectManager>().RateMember(id, UserName, projectId, userId, comment, fn, co, pu, qu, re);
      // get all ratings
      var ratings = UnitOfWork<ProjectManager>().GetRatingsForUser(id);
      return View("MemberRatings", ratings);
    }

    [HttpPost]
    public ActionResult AssignRole(int id, int roles) {
      UnitOfWork<ProjectManager>().SetTeamMemberRoles(id, roles);
      return Json(new { msg = ControllerResources.TeamController_AssignRole_Role_for_team_member_assigned });
    }

    public ActionResult MembersRoles(int id) {
      var tm = UnitOfWork<ProjectManager>().GetTeamMemberWithRoles(id);
      return PartialView("Members/_MembersRoles", tm);
    }

    public ActionResult SearchUsers(int id, string q, bool teamOnly = true) {
      var proj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      var teamId = proj.Team.Id;
      ManagerResults<IEnumerable<EditUser>> result;
      if (!teamOnly) {
        result = UnitOfWork<UserManager>().SearchUsersForTeam(teamId, q);
      } else {
        result = UnitOfWork<UserManager>().SearchUsersInTeam(teamId, q);
      }
      if (result.Kind == ResultKind.Error) {
        return HttpNotFound(result.Text);
      } else {
        return Json(result.ViewModel.ToArray(), JsonRequestBehavior.AllowGet);
      }
    }

    [HttpPost]
    public ActionResult AddMember(int id, string usernames) {
      foreach (var username in usernames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
        int userId;
        if (Int32.TryParse(username, out userId)) {
          UnitOfWork<ProjectManager>().AddMemberToTeam(id, userId);
        }
      }
      return Json(new { msg = ControllerResources.TeamController_AddMember_Member_added });
    }

    [HttpPost]
    public ActionResult RemoveMember(int id) {
      var result = UnitOfWork<ProjectManager>().RemoveMemberFromTeam(id, UserName, true, true);
      return Json(new { msg = result ? ControllerResources.TeamController_RemoveMember_Member_removed : ControllerResources.TeamController_RemoveMember_Cannot_remove_member });
    }

    [HttpPost, ValidateInput(false)]
    public ActionResult Save(int id, string elementId, string value) {
      UnitOfWork<ProjectManager>().ChangeTeamInfo(id, elementId, value, UserName);
      return new ContentResult { Content = value, ContentEncoding = Encoding.UTF8, ContentType = "text/html" };
    }
    #endregion

    #region --== Member List ==--

    /// <summary>
    /// Lists all team members
    /// </summary>
    /// <param name="id">the team id</param>
    /// <param name="projectId"> </param>
    /// <returns>a partial view with all team members in an div as ul</returns>
    public ActionResult ListMembers(int id, int? projectId) {
      var members = UnitOfWork<ProjectManager>().GetTeamMembersWithRoles(id, projectId);
      ViewBag.Editable = UnitOfWork<ProjectManager>().MemberIsTeamLead(id, UserName);
      ViewBag.TeamId = id;
      return PartialView("Members/_ListMembers", members);
    }

    public ActionResult ListUsers(string term, int teamId) {
      var users = UnitOfWork<ProjectManager>().GetUserNotAlreadyInTeam(term, teamId);
      return PartialView("Members/_ListUsers", users);
    }

    public ActionResult ConfirmMembership(int id) {
      // id is TeamMembers Id
      var tm = UnitOfWork<ProjectManager>().GetContributorProposal(id);
      return View(tm);
    }

    public ActionResult ConfirmProposal(int id, int opusId) {
      UnitOfWork<ProjectManager>().ConfirmProposal(id, opusId, true, UserName);
      return RedirectToAction("ConfirmMembership", new { id });
    }

    public ActionResult DeclineProposal(int id, int opusId) {
      UnitOfWork<ProjectManager>().ConfirmProposal(id, opusId, false, UserName);
      return RedirectToAction("ConfirmMembership", new { id });
    }

    #endregion

    # region --== Invoices ==--

    [NavigationPathFilter("Billing")]
    public ActionResult Billing(int id) {
      // id is OpusId -- Invoices are on a per milestone/work base      
      var bs = UnitOfWork<AccountingManager>().CreateBillingSummary(id, UserName);
      return View("Billing/Billing", bs);
    }

    [NavigationPathFilter("Incoming Invoices")]
    public ActionResult InvoicesIncoming() {
      return View("Billing/InvoicesIncoming");
    }

    [NavigationPathFilter("Outgoing Invoices")]
    public ActionResult InvoicesOutgoing() {
      return View("Billing/InvoicesOutgoing");
    }

    public ActionResult ListInvoices(int page, bool outgoing) {
      var invs = UnitOfWork<AccountingManager>().GetInvoices(outgoing, UserName);
      return PartialView("Billing/Invoices/_ListInvoices", new PagedList<Invoice>(invs.AsQueryable(), page, 10));
    }

    /// <summary>
    /// Create new invoice. Curently we support one position per invoice only.
    /// </summary>
    /// <remarks>Always raise an invoice to a teamlead, always strictly related to an opus.</remarks>
    /// <param name="id">Opus Id</param>
    /// <returns></returns>
    public ActionResult CreateInvoice(int id) {
      var inv = UnitOfWork<AccountingManager>().CreateInvoice(id, UserName);
      return PartialView("Billing/Invoices/_CreateInvoice", inv);
    }

    [HttpPost]
    public JsonResult CreateInvoice(Invoice inv, [Bind(Prefix = "Creditor.Id")] int cid, [Bind(Prefix = "Debitor.Id")] int did, string amount, string text) {
      var newInv = new Invoice();
      inv.CopyProperties<Invoice>(newInv,
        i => i.BillingDate,
        i => i.DueDays,
        i => i.Paid,
        i => i.Positions,
        i => i.TaxPercentage,
        i => i.Text,
        i => i.WithTax
        );
      newInv.Creditor = Manager<UserManager>.Instance.GetUser(cid);
      newInv.Debitor = Manager<UserManager>.Instance.GetUser(did);
      var position = new InvoicePosition {
        Amount = Decimal.Parse(amount),
        Text = text,
        Position = 0
      };
      newInv.Positions.Add(position);
      UnitOfWork<AccountingManager>().CreateInvoice(newInv, true);
      return Json(new { message = "Invoice Created and Send" });
    }

    public ActionResult EditInvoice() {
      return PartialView("Billing/Invoices/_EditInvoice");
    }

    [HttpPost]
    public ActionResult EditInvoice(Invoice inv) {
      return PartialView("Billing/Invoices/_EditInvoice");
    }

    public ActionResult ShowInvoice(int id) {
      var inv = UnitOfWork<AccountingManager>().GetInvoice(id, UserName);
      return PartialView("Billing/Invoices/_ShowInvoice", inv);
    }

    [HttpPost]
    public JsonResult DeleteInvoice(Invoice inv) {
      var r = UnitOfWork<AccountingManager>().DeleteInvoice(inv.Id, UserName);
      return Json(new { message = r ? "Invoice removed" : "Invoice can't be deleted" });
    }

    # endregion

    public ActionResult PayInvoice(int id) {
      var inv = UnitOfWork<AccountingManager>().GetInvoice(id, UserName);
      //Call the Express Checkout through API
      var returnUrl = String.Format("http://{0}/AuthorPortal/Team/ConfirmInvoice", Request.Url.Host);
      var cancelUrl = String.Format("http://{0}/AuthorPortal/Team/CancelInvoice", Request.Url.Host);
      var checkout = new NVPExpressAPICaller(returnUrl, cancelUrl);
      var redirectUrl = "";
      var token = "";

      //stored invoice Id and amount in session before sending to the PayPal
      Session["invoice_amt"] = Math.Round(inv.Total, 2);
      Session["invoice_id"] = id;

      //Distribute virtual Credit balance

      decimal creditBalance = UnitOfWork<AccountingManager>().GetCurrentCreditAmountForUser(UserName);
      if (creditBalance <= 0) {
        Session["invoice_amt"] = Math.Round(inv.Total, 2);
        bool isSucceed = checkout.SetExpressCheckout(Session["invoice_amt"].ToString(), inv.Positions.First().Text, CurrentCulture, ref token, ref redirectUrl);

        if (isSucceed) {
          Session["token"] = token;
          return Redirect(redirectUrl);
        }
        return Redirect("InvoicePayError");
      }
      if (creditBalance < Math.Round(inv.Total, 2)) {
        //Session["invoice_amt"] = Math.Round(inv.Total, 2) - CreditBalance;
        Session["invoice_amt"] = Math.Round(inv.Total, 2);
        //bool result = UnitOfWork<ProjectManager>().CreditAmount(CreditBalance, inv.Debitor.UserName, inv.Creditor.UserName, TransactionType.Transfer);
        bool isSucceed = checkout.SetExpressCheckout(Session["invoice_amt"].ToString(), inv.Positions.First().Text, CurrentCulture, ref token, ref redirectUrl);

        if (isSucceed) {
          Session["token"] = token;
          return Redirect(redirectUrl);
        }
        return Redirect("InvoicePayError");
      }
      //Payment will be done from virtual credit and no need to go for Paypal
      Session["invoice_amt"] = Math.Round(inv.Total, 2);
      UnitOfWork<AccountingManager>().CreditAmount(Math.Round(inv.Total, 2), inv.Debitor.UserName, inv.Creditor.UserName, TransactionType.Transfer, id.ToString(CultureInfo.InvariantCulture));
      UnitOfWork<AccountingManager>().UpdateInvoiceStatus(inv, Convert.ToInt32(Session["invoice_id"].ToString()), UserName);
      return View("Billing/InvoiceStatus", inv);
    }

    /// <summary>
    /// PayPal redirect to here, after user reviewing the their shipping address, payment method etc. 
    /// </summary>
    /// <returns></returns> 
    public ActionResult ConfirmInvoice() {
      var inv = UnitOfWork<AccountingManager>().GetInvoice(Convert.ToInt32(Session["invoice_id"].ToString()), UserName);

      var shipping = new NVPExpressAPICaller();
      var returnMsg = string.Empty;
      var payerId = string.Empty;
      var shippingAddressDecoder = new NVPCodec();
      // get the paypal shipping details.
      var isSucceed = shipping.GetExpressCheckoutDetails(Session["token"].ToString(), ref payerId, ref shippingAddressDecoder, ref returnMsg);
      // override the shipping address 
      if (isSucceed) {
        //stored the payer id
        Session["payerid"] = payerId;
      } else {
        return Redirect("InvoicePayError");
      }
      return View("Billing/Payment/InvoiceConfirm", inv);
    }

    /// <summary>
    /// If user cancel the Payment
    /// then PayPal redirect to this screen. 
    /// </summary>
    /// <returns></returns> 
    public ActionResult CancelInvoice() {
      var inv = UnitOfWork<AccountingManager>().GetInvoice(Convert.ToInt32(Session["invoice_id"].ToString()), UserName);
      return View("Billing/Payment/InvoiceCancel", inv);
    }

    public ActionResult InvoicePayment() {
      var inv = UnitOfWork<AccountingManager>().GetInvoice(Convert.ToInt32(Session["invoice_id"].ToString()), UserName);

      var trasactions = new NVPExpressAPICaller();
      var returnMsg = string.Empty;
      NVPCodec decoder = null;

      //do the Payment 
      var isSucceed = trasactions.DoExpressCheckoutPayment(Session["invoice_amt"].ToString(), "", Session["token"].ToString(), Session["payerid"].ToString(), ref decoder, ref returnMsg);
      if (!isSucceed) {
        return Redirect("InvoicePayError");
      }
      /*
      // Unique transaction ID of the payment. Note:  If the PaymentAction of the request was Authorization or Order, this value is your AuthorizationID for use with the Authorization & Capture APIs. 
      var transactionId = decoder["TRANSACTIONID"];

      // The type of transaction Possible values: l  cart l  express-checkout 
      var transactionType = decoder["TRANSACTIONTYPE"];

      // Indicates whether the payment is instant or delayed. Possible values: l  none l  echeck l  instant 
      var paymentType = decoder["PAYMENTTYPE"];

      // Time/date stamp of payment
      var orderTime = decoder["ORDERTIME"];

      // A three-character currency code for one of the currencies listed in PayPay-Supported Transactional Currencies. Default: USD.    
      var currencyCode = decoder["CURRENCYCODE"];

      // PayPal fee amount charged for the transaction    
      var feeAmt = decoder["FEEAMT"];

      // Amount deposited in your PayPal account after a currency conversion.    
      var settleAmt = decoder["SETTLEAMT"];

      // Tax charged on the transaction.    
      var taxAmt = decoder["TAXAMT"];

      //' Exchange rate if a currency conversion occurred. Relevant only if your are billing in their non-primary currency. If 
      var exchangeRate = decoder["EXCHANGERATE"];
      */
      // The final amount charged, including any shipping and taxes from your Merchant Profile.
      var amt = decoder["AMT"];

      //Update Invoice Paid status to true

      //Credit Distribution based on available credit balance.
      TransactionType type;
      var creditBalance = UnitOfWork<AccountingManager>().GetCurrentCreditAmountForUser(UserName);
      if (creditBalance < 0) {
        type = TransactionType.FromBankAccount;
      } else {
        type = creditBalance < Math.Round(inv.Total, 2) ? TransactionType.FromBankAccount : TransactionType.Transfer;
      }
      //Log the Transaction Details 
      var result = UnitOfWork<AccountingManager>().CreditAmount(decimal.Parse(amt), inv.Debitor.UserName, inv.Creditor.UserName, type,
                                    Session["invoice_id"].ToString());
      // Send Payment Confirmation Email
      if (result) {
        var msg = new Message {
          Body = "Your Payment has been confirmed",
          Subject = "Your Payment has been confirmed",
          Sender = inv.Creditor,
          Receiver = inv.Debitor
        };
        UnitOfWork<UserManager>().AddMessage(new List<int>(new[] { inv.Debitor.Id }), msg, inv.Creditor.UserName);
      }
      UnitOfWork<AccountingManager>().UpdateInvoiceStatus(inv, Convert.ToInt32(Session["invoice_id"].ToString()), UserName);
      return View("Billing/InvoiceStatus", inv);
      //return View("Billing/Payment/InvoicePayment");
    }

    /// <summary>
    /// Handling the unexpected errors during the Express checkout process.
    /// </summary>
    /// <returns></returns> 
    public ActionResult InvoicePayError() {
      var inv = UnitOfWork<AccountingManager>().GetInvoice(Convert.ToInt32(Session["invoice_id"].ToString()), UserName);
      return View("Billing/Payment/InvoiceError", inv);
    }

    public ActionResult CurrentMembers(int id) {
      var teamMembers = UnitOfWork<ProjectManager>().GetTeamMembersWithRoles(id, null);
      return PartialView("Members/_CurrentMembers", teamMembers);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">id is Project.Id</param>
    /// <param name="opusId">Filter the list to opus if we come from dashboard</param>
    /// <returns></returns>
    [Authorize]
    public ActionResult ManageShares(int id, int? opusId) {
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName, p => p.Marketing);
      ViewBag.TeamLead = UnitOfWork<ProjectManager>().GetTeamLeader(prj.Team.Id).UserName;
      ViewBag.FilterToOpus = opusId;
      return View(prj);
    }

    /// <summary>
    /// Returns all active opus for a project.
    /// </summary>
    /// <param name="id">Opus Id</param>
    /// <param name="opusId">Filter</param>
    /// <param name="p">current page</param>
    /// <param name="tl">Forwarded TeamLead to manage buttons in view</param>
    /// <returns></returns>
    public ActionResult ListOpus(int id, string tl, int? opusId, PaginationFormModel p) {
      // id is Project.Id
      var opus = UnitOfWork<ProjectManager>().GetOpuses(id, UserName, true);
      if (opusId.HasValue) {
        opus = opus.Where(o => o.Id == opusId);
      }
      ViewBag.TeamLead = tl;
      return PartialView("Shares/_List", opus.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult EditShares(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpusWithTeam(id);
      var cs = new ContributorShares {
        TeamMembers = opus.Project.Team.Members,
        Book = opus
      };
      return PartialView("Shares/_Edit", cs);
    }

    [HttpPost]
    public ActionResult EditShares(int id, string[] user, int[] ratio, bool[] use, int[] shareType) {
      var shareTypes = new ShareType[shareType.Length];
      for (var i = 0; i < shareType.Length; i++) {
        shareTypes[i] = (ShareType)Enum.ToObject(typeof(ShareType), shareType[i]);
      }
      var result = UnitOfWork<ProjectManager>().SetSharesForOpus(id, user.ToList(), ratio.ToList(), use.ToList(), shareTypes.ToList());
      if (result.Kind != ResultKind.Error) {
        return Json(new { msg = result.Text });
      } else {
        return new HttpNotFoundResult(result.Text);
      }
    }


  }
}
