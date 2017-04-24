using System.Web.Mvc;

namespace Texxtoor.Portal.Areas.ReaderPortal {
  public class ReaderPortalAreaRegistration : AreaRegistration {
    public override string AreaName {
      get {
        return "ReaderPortal";
      }
    }

    public override void RegisterArea(AreaRegistrationContext context) {
      context.MapRoute(
            "ReaderPortal_default",
            "ReaderPortal/{controller}/{action}/{id}",
            new { action = "Index", area= "ReaderPortal", id = UrlParameter.Optional }
        );
    }
  }
}
