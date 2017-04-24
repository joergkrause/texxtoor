using System.Web.Mvc;

namespace Texxtoor.Portal.Areas.AdminPortal {
  public class PortalAdminAreaRegistration : AreaRegistration {
    public override string AreaName {
      get {
        return "AdminPortal";
      }
    }

    public override void RegisterArea(AreaRegistrationContext context) {
      context.MapRoute(
          "AdminPortal_default",
          "AdminPortal/{controller}/{action}/{id}",
          new { action = "Index", id = UrlParameter.Optional }
      );
    }
  }
}
