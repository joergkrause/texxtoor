using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.BusinessLayer.Workflows;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Common;

namespace Texxtoor.Portal.Areas.AuthorPortal.Controllers {

  /// <summary>
  /// Services that support authors in many ways.
  /// </summary>
  public class ServiceController : ControllerExt {


    public ActionResult Index() {
      return View();
    }

    public ActionResult Score() {
      var profile = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName);
      return PartialView("_Score", profile);
    }

    public ActionResult AllPublishables(PaginationFormModel pg) {
      var ids = UnitOfWork<ProjectManager>().GetProjectsWhereUserIsMember(UserName)
        .Where(p => p.Team.Members.Any(t => t.TeamLead && t.Member.UserName == UserName))
        .Select(p => p.Id)
        .ToList();
      List<KeyValuePair<Opus, string>> misses;
      var publishables = UnitOfWork<ProjectManager>().GetPublishables(ids, UserName, out misses);
      return PartialView("Lists/_Publishables", publishables.AsQueryable().ToPagedList(pg.Page, pg.PageSize, pg.FilterValue, pg.FilterName, pg.Order, pg.Dir));
    }

    /// <summary>
    /// Author selected a text and requests help from another author with higher reputation to unfreeze the publishing procedure.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult UnfreezeText(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      return View(opus);
    }

    [HttpPost]
    public ActionResult UnfreezeText(int? user, IEnumerable<int> usernames) {
      var ask = new List<int>();
      if (usernames == null && user.HasValue) {
        ask.Add(user.Value);
      }
      if (usernames != null && user.HasValue) {
        ask = usernames.ToList();
        ask.Add(user.Value);
      }
      InvokeReputation();
      return View("Index");
    }

    private void InvokeReputation() {

      object state = "";
      var invoker = new WorkflowInvoker(new UnfreezeTextWorkflow());
      var inputs = new Dictionary<string, object> {
      };
      // Wait for the workflow to complete.
      Task.Factory.FromAsync<IDictionary<string, object>>(
        invoker.BeginInvoke(inputs, AsyncCall, state),
        invoker.EndInvoke);
    }

    private void AsyncCall(IAsyncResult args) {
      if (args.IsCompleted) {
        var user = UnitOfWork<UserManager>().GetCurrentUser(UserName);
        UnitOfWork<UserManager>().SendExternalMail(null, user, "ProductionDone", true, null);
      }
    }

    /// <summary>
    /// Search for users that have minimum rep and exclude caller.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="q"></param>
    /// <returns></returns>
    public JsonResult SearchUsers(int id, string q) {
      var users = UnitOfWork<UserManager>().SearchUsersForReputation(q, 1000)
        .Where(u => u.name != UserName)
        .ToArray();
      return Json(users, JsonRequestBehavior.AllowGet);
    }

    public ActionResult ReputationRecommandation() {
      var users = UnitOfWork<UserManager>().SearchUsersTopForReputation(1000, 5);
      return PartialView("Lists/_ReputationRecommandation", users);
    }

  }
}
