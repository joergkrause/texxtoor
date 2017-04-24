using System.Web.Mvc;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;

namespace Texxtoor.Portal.Core.Attributes {

  public class CurrentUserFilter : ActionFilterAttribute {
    public override void OnActionExecuting(ActionExecutingContext filterContext) {
      const string Key = "userName";

      if (filterContext.ActionParameters.ContainsKey(Key)) {
        if (filterContext.HttpContext.User.Identity.IsAuthenticated) {
          filterContext.ActionParameters[Key] = Manager<UserManager>.Instance.GetCurrentUserName();
        }
      }

      base.OnActionExecuting(filterContext);
    }

  }
}