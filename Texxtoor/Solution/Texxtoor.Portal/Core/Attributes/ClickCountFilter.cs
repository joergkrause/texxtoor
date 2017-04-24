using System.Linq;
using System.Web.Mvc;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary;
using Texxtoor.DataModels.Models.Marketing;

namespace Texxtoor.Portal.Core.Attributes {

  /// <summary>
  /// The purpose of this filter is the gathering of click information
  /// </summary>
  public class ClickCountFilter : ActionFilterAttribute {

    public ClickSourceType SourceType { get; set; }

    public int ClickValue { get; set; }

    public override void OnActionExecuted(ActionExecutedContext filterContext) {
      // only authenticated users
      if (filterContext.HttpContext.User.Identity.IsAuthenticated) {
        try {
          Manager<UserManager>.Instance.SaveChanges();
        } catch (System.Exception) {
        }
        try {
          Manager<UserManager>.Instance.ClickCount(ClickValue, SourceType, filterContext.ActionDescriptor.ActionName, filterContext.ActionDescriptor.ControllerDescriptor.ControllerName);
        } catch (System.Exception) {
        }
      }

      base.OnActionExecuted(filterContext);
    }

  }
}