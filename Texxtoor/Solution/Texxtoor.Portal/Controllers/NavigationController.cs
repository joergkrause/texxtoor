using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Linq;
using Texxtoor.BaseLibrary;
using Texxtoor.BaseLibrary.Services;
using Texxtoor.BusinessLayer;
using Texxtoor.BusinessLayer.Services;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Cms;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Controllers {

  /// <summary>
  /// Handle Navigation
  /// </summary>
  public class NavigationController : ControllerExt {

    private static readonly XNamespace Ns = "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0";

    public ActionResult BreadCrumb() {
      var xDoc = Request.RequestContext.HttpContext.Application["WebSiteMap"] as XDocument;
      var localPath = Request.Url.LocalPath.TrimEnd('/');
      // get all nodes in the sitemap      
      var nodes = xDoc.Root
        .Descendants(Ns + "siteMapNode")
        .Where(e => e.Attribute("url") != null && localPath.StartsWith(e.Attribute("url").Value))
        .ToList();
      // create an uplevel path for all of these nodes      
      Func<XElement, List<KeyValuePair<string, string>>> up = null;
      up = (e) => {
        var upPath = new List<KeyValuePair<string, string>>();
        if (e.Attribute("url") != null) {
          var title = (e.Attribute("title-" + CurrentCulture) != null)
                        ? e.Attribute("title-" + CurrentCulture).Value
                        : e.Attribute("title").Value;
          upPath.Add(new KeyValuePair<string, string>(title, e.Attribute("url").Value));
        }
        if (e.Parent != null) {
          upPath.AddRange(up(e.Parent));
        }
        return upPath;
      };
      var breadCrumbs = new List<List<KeyValuePair<string, string>>>();
      var nodeCnt = nodes.Count();
      foreach (var path in nodes.Select(node => up(node)).Where(path => nodeCnt <= 1 || path.Count != 1)) {
        path.Reverse();
        breadCrumbs.Add(path);
      }
      return PartialView("_BreadCrumbs", breadCrumbs);
    }

    //[OutputCache(CacheProfile = "Menu")]
    public ActionResult Show(string menuName) {
      if (String.IsNullOrEmpty(menuName)) {
        throw new ArgumentNullException("menuName");
      }
      var menu = new List<CmsMenu>();
      // nothing requested, proceed with role
      IEnumerable<UserRole> roles;
      roles = Boolean.Parse(WebConfigurationManager.AppSettings["ui:UseRolesInUI"]) ? UnitOfWork<UserManager>().GetRolesForUser(User.Identity).Select(r => (UserRole)Enum.Parse(typeof(UserRole), r, true)) : Enum.GetValues(typeof(UserRole)).OfType<UserRole>();
      var ueq = new UnionDistinctCompare();
      if (roles.Any()) {
        menu = roles.Aggregate(menu, (current, role) => current.Union(MenuService.Instance.LoadMenu(menuName, role, CurrentCulture), ueq).ToList());
      }
      else {
        menu = MenuService.Instance.LoadMenu(menuName, CurrentCulture).ToList();
      }
      // replace dynamic values
      MenuService.Instance.ResolveDynamicMenus(menu, UserName);
      return PartialView(menuName, menu);
    }

    class UnionDistinctCompare : IEqualityComparer<CmsMenu> {

      public bool Equals(CmsMenu x, CmsMenu y) {
        return x.Id == y.Id;
      }

      public int GetHashCode(CmsMenu obj) {
        return 0;
      }
    }

  }
}