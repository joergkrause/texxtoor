using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.BaseLibrary.Core.Notifications;
using Texxtoor.DataModels.Context;

namespace Texxtoor.Portal.Core.Extensions
{

    [DebuggerStepThrough]
  public class ControllerExt : Controller {

    private string _userName = null;

    public string UserName {
      get {
        if (_userName == null) {
          if (User != null) {
            if (User.Identity.IsAuthenticated) {
              _userName = User.Identity.Name;
              //var leadingAccountId = UnitOfWork<UserManager>().GetUserByName(User.Identity.Name).LeadingAccountId;
              //if (leadingAccountId != null) {
              //  _userName = UnitOfWork<UserManager>().GetUser(Convert.ToInt32(leadingAccountId)).UserName;
              //}
              //else {
              //  _userName = User.Identity.Name;
              //}
            }
          }          
        }
        return _userName;
      }
    }

    public string CurrentCulture {
      get {
        var cult = Thread.CurrentThread.CurrentUICulture;
        return Equals(cult.Parent, CultureInfo.InvariantCulture)
                 ? cult.TwoLetterISOLanguageName
                 : cult.Parent.TwoLetterISOLanguageName;
      }
    }

    protected override void Initialize(System.Web.Routing.RequestContext requestContext) {
      if (!String.IsNullOrEmpty(UserName) && requestContext.HttpContext.Session != null) {
        requestContext.HttpContext.Session["RunControl"] = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName).RunControl;
      }
      if (requestContext.HttpContext.Session != null && requestContext.HttpContext.Session["RunControl"] != null) {
        var rm = (RunControl)requestContext.HttpContext.Session["RunControl"];
        Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = new CultureInfo(rm.UiLanguage);
      }
      base.Initialize(requestContext);
    }

    protected bool GetAc2() {
      return ((RunControl)Session["RunControl"]).RunMode == RunMode.Business;
    }

    protected T UnitOfWork<T>() where T : class, IManager, new() {
      var repository = Manager<T>.Instance as T;
      repository.UserName = UserName;
      return repository;
    }

    // For Admin Area only, we want to move this to regular repositories!
    protected PortalContext AdminDb {
      get { return UnitOfWorkFactory.GetIUnitOfWorkContext<PortalContext>(); }
    }

  }
}