using System.Collections.Generic;
using System.Security.Policy;
using System.Web.Mvc;
using System.Web.Routing;
using DocumentFormat.OpenXml.Spreadsheet;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.Portal.Core.Attributes {

  public class MinimumAwardScoreFilter : ActionFilterAttribute {

    public MinimumAwardScoreFilter(bool forUser, int score) {
      ForUser = forUser;
      if (forUser)
        UserScore = score;
      else
        CreatorScore = score;
    }

    public bool ForUser { get; set; }

    public int UserScore { get; set; }

    public int CreatorScore { get; set; }

    private bool forceEmptyResult = false;

    private int hasUserScore;
    private int hasCreatorScore;

    public override void OnActionExecuting(ActionExecutingContext filterContext) {
      var rm = UserProfileManager.Instance.GetProfileByUser(filterContext.HttpContext.User.Identity.Name).RunControl;
      if (rm.RunMode == RunMode.Texxtoor) {
        var p = Manager<UserProfileManager>.Instance.GetProfileByUser(filterContext.HttpContext.User.Identity.Name);
        if (filterContext.IsChildAction) {
          hasUserScore = p.UserScore;
          hasCreatorScore = p.CreatorScore;
          forceEmptyResult = ForUser ? hasUserScore < UserScore : hasCreatorScore < CreatorScore;
        } else {
          if (ForUser) {
            if (p.UserScore < UserScore) {
              filterContext.Result = new RedirectToRouteResult("Default",
                new RouteValueDictionary(new Dictionary<string, object>() {
                  {"controller", "Home"},
                  {"action", "Error"},
                  {"id", UserScore}
                }));
            }
          } else {
            if (p.CreatorScore < CreatorScore) {
              filterContext.Result = new RedirectToRouteResult("Default",
                new RouteValueDictionary(new Dictionary<string, object>() {
                  {"controller", "Home"},
                  {"action", "Error"},
                  {"id", CreatorScore}
                }));
            }
          }
        }
      }
      base.OnActionExecuting(filterContext);
    }

    public override void OnActionExecuted(ActionExecutedContext filterContext) {
      var result = filterContext.Result;
      var viewResult = result as ViewResultBase;
      if (viewResult != null){
        if (forceEmptyResult){
          viewResult.ViewData.Add("MinScore", ForUser ? UserScore : CreatorScore);
        }
        else{
          viewResult.ViewData.Add("MinScore", 0);
        }
        viewResult.ViewData.Add("HasScore", ForUser ? hasUserScore : hasCreatorScore);
        filterContext.Result = viewResult;
      }
      base.OnActionExecuted(filterContext);
    }
  }
}