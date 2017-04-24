using System.Web.Mvc;
using System.Web.Routing;

namespace Texxtoor.Portal {
  public class RouteConfig {
    public static void RegisterRoutes(RouteCollection routes) {

      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
      routes.IgnoreRoute("{*extaxd}", new { extaxd = @".*ext\.axd(/.*)?" });

      routes.LowercaseUrls = true;

      //var easyRoute = routes.MapHttpRoute(
      //    name: "EasyAuthorAPI",
      //    routeTemplate: "api/{controller}/{action}/{id}",
      //    defaults: new { id = RouteParameter.Optional }          
      //);
      //easyRoute.DataTokens = new RouteValueDictionary();
      //easyRoute.DataTokens["Namespaces"] = new string[] { "Texxtoor.Portal.Areas.EasyAuthor.Controllers" };

      routes.MapRoute(
        "Premium",
        "premium",
        new { controller = "Home", action = "Premium" }, // Parameter defaults
        new string[] { "Texxtoor.Portal.Controllers" }
        );

      var r = routes.MapRoute(
        "ReaderPortal_QuickOrder",
        "qo/{id}",
        new { controller = "Orders", action = "QuickOrder" },
        new string[] { "Texxtoor.Portal.Areas.ReaderPortal.Controllers" }
      );
      r.DataTokens["area"] = "ReaderPortal";

      // string c, string res, bool nc = false
      routes.MapRoute(
            "EmbeddedHtml_For_Author_Provide_QuickOrder",
            "qoi/{id}/{c}/{res}/{nc}",
            new { controller = "Tools", action = "GetImgQuickOrder", c = "ProjectCover", res= "80x120", nc = false }
          );

      routes.MapRoute(
          "AuthorHelp", // Route name
          "AuthorHelp", // URL, no parameters
          new { controller = "Home", action = "AuthorHelp" }, // Parameter defaults
          new string[] { "Texxtoor.Portal.Controllers" }
      );

      routes.MapRoute(
          "Pages", // Route name
          "Home/Page/{id}", // URL with parameters
          new { controller = "Home", action = "Page", id = 0 }, // Parameter defaults
          new { id = @"\d+" },
          new string[] { "Texxtoor.Portal.Controllers" }
      );
      
      routes.MapRoute(
          "AliasPages", // Route name
          "Home/Page/{name}", // URL with parameters
          new { controller = "Home", action = "PageAlias", name= "" }, // Parameter defaults
          new string[] { "Texxtoor.Portal.Controllers" }
      );

      
      routes.MapRoute(
          "Default", // Route name
          "{controller}/{action}/{id}", // URL with parameters
          new { controller = "Home", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
          new { id = @"\d{0,10}" },
          new string[] { "Texxtoor.Portal.Controllers" }
      );

      routes.MapRoute(
          "Editor", // Route name
          "{controller}/{action}/{path}/{key}/{culture}", // URL with parameters for editing resource
          new { controller = "Home", action = "Index", path = "", key = "", culture = UrlParameter.Optional } // Parameter defaults
      );

      routes.MapRoute(
          "CMS", // Route name
          "CmsAdmin/{controller}/{action}/{id}", // URL with parameters for CMS actions
          new { controller = "Home", action = "Index", culture = UrlParameter.Optional }, // Parameter defaults
          new string[] { "Texxtoor.Portal.Areas.CmsAdmin.Controllers" }
      );

      routes.MapRoute(
          "Portal", // Route name
          "rp/{controller}/{action}/{id}", // URL with parameters regular actions
          new { controller = "Home", action = "Index", culture = UrlParameter.Optional }, // Parameter defaults
          new string[] { "Texxtoor.Portal.Areas.ReaderPortal.Controllers" }
      );

      routes.MapRoute(
          "PortalResource", // Route name
          "rp/{controller}/{action}/{restype}/{resource}", // URL with parameters for embedded images etc.
          new { controller = "Reader", action = "Resource" }, // Parameter defaults
          new string[] { "Texxtoor.Portal.Areas.ReaderPortal.Controllers" }
          );


      //var sr = new System.ServiceModel.Activation.ServiceRoute("Services/ReaderService", new ServiceHostFactory(), typeof(Texxtoor.Services.Mobile.ReaderService));
      //routes.Add(sr);
    }
  }
}