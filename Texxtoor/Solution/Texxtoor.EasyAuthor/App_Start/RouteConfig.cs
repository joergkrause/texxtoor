using System.Web.Mvc;
using System.Web.Routing;

namespace Texxtoor.EasyAuthor {
  public class RouteConfig {
    public static void RegisterRoutes(RouteCollection routes) {

      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
      routes.IgnoreRoute("{*extaxd}", new { extaxd = @".*ext\.axd(/.*)?" });

      routes.LowercaseUrls = true;

      routes.MapRoute(
          "Default", // Route name
          "{controller}/{action}/{id}", // URL with parameters
          new { controller = "Home", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
          new { id = @"\d{0,10}" },
          new string[] { "Texxtoor.EasyAuthor.Controllers" }
      );

         }
  }
}