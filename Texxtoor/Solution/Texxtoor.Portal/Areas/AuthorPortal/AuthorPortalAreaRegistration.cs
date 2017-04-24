using System.Web.Mvc;

namespace Texxtoor.Portal.Areas.AuthorPortal {
  public class AuthorPortalAreaRegistration : AreaRegistration {
    public override string AreaName {
      get {
        return "AuthorPortal";
      }
    }

    public override void RegisterArea(AreaRegistrationContext context) {
      context.MapRoute(
          "AuthorPortal_default",
          "AuthorPortal/{controller}/{action}/{id}",
          new { action = "Index", id = UrlParameter.Optional }
      );
    }
  }
}
