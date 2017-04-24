using System;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Notifications;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models.Common;

namespace Texxtoor.EasyAuthor.Core.Extensions {

  public class ControllerExtAsync : AsyncController {

    //protected IHubConnectionContext hubContext = GlobalHost.ConnectionManager.GetHubContext<NavigationTilesHub>().Clients;

    public string UserName { get { return User == null ? String.Empty : User.Identity.Name; } }

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
      return UnitOfWork<T>();
    }

  }
  
  public class ControllerExt : Controller {

    //protected IHubConnectionContext hubContext = GlobalHost.ConnectionManager.GetHubContext<NavigationTilesHub>().Clients;

    public string UserName { get { return User == null ? String.Empty : User.Identity.Name; } }

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
        requestContext.HttpContext.Session["RunControl"] = true;
      }
      base.Initialize(requestContext);
    }

    protected T UnitOfWork<T>() where T : class, IManager, new() {
      return UnitOfWork<T>(null);
    }

    protected T UnitOfWork<T>(INotificationService n) where T : class, IManager, new() {
      var rep = Manager<T>.Instance as T; 
      rep.Notification = n;
      return rep;
    }

  }
}