using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Configuration;
using Texxtoor.BusinessLayer.Properties;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Cms;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.BusinessLayer.Services {

  public class MenuService : Manager<MenuService> {

    public void SaveMenuItem(CmsMenuItem item) {
      if (item.Page != null && item.Page.Id == 0) {
        item.Page = null;
      }
      Ctx.MenuItems.Add(item);
      Ctx.SaveChanges();
    }

    public IEnumerable<CmsMenu> LoadAllMenues() {
      return Ctx.Menus.ToList();
    }

    public IEnumerable<CmsMenu> LoadMenu(string menuType, string localeId) {
      if (String.IsNullOrEmpty(localeId) || localeId == "iv") {
        localeId = "en";
      }
      var authorMenu = LoadMenu(menuType, UserRole.Unknown, new CultureInfo(localeId));
      return authorMenu;
    }

    public IEnumerable<CmsMenu> LoadMenu(string menuType) {
      var authorMenu = LoadMenu(menuType, UserRole.Unknown, System.Threading.Thread.CurrentThread.CurrentUICulture);
      return authorMenu;
    }

    public IEnumerable<CmsMenu> LoadMenu(string menuType, UserRole role, string culture) {
      var ci = (culture == "iv") ? CultureInfo.InvariantCulture : new CultureInfo(culture);
      return LoadMenu(menuType, role, ci);
    }

    public IEnumerable<CmsMenu> LoadMenu(string menuType, UserRole role, CultureInfo c) {
      var cName = c.Name.ToLower();
      var cParentName = (Equals(c.Parent, CultureInfo.InvariantCulture) ? c : c.Parent).Name.ToLowerInvariant();
      var menuTypeLowered = menuType.ToLowerInvariant();
      var application = HttpContext.Current.Session["Application"] == null ? "/" : Convert.ToBoolean(HttpContext.Current.Session["Application"]) ? "ac2" : "/";
      var featureSetValue = WebConfigurationManager.AppSettings["ui:FeatureSet"];
      var featureSets = new List<string>(featureSetValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
      var query = Ctx.Menus
          .Include(m => m.MenuItems)
          .Include(m => m.Application)
          .Include(m => m.Roles)
          .Include(m => m.Page)
          .Where(m => m.Type.Equals(menuTypeLowered) && (m.LocaleId.Equals(cName) || m.LocaleId.Equals(cParentName)))
          .Where(m => String.IsNullOrEmpty(m.FeatureSet) || featureSets.Contains(m.FeatureSet));
      var menus = query
          .ToList()
          // Unknown Role is a hint that the role manager shall not do anything, we show this menu regardless the user's roles
          .Where(m => m.Roles.Any(r => r.UserRole == role) /* if role provided */ || m.Roles.All(r => r.UserRole == UserRole.Unknown) /* no roles other than unknown */ || role == UserRole.Unknown /* no role requested/anonym */)
          .Where(m => m.Application.ApplicationName == "/" || m.Application.ApplicationName == application)
          .OrderBy(m => m.Order)
          .Select(m => {
            var m2 = new CmsMenu();
            m.CopyProperties<CmsMenu>(m2,
              p => p.Id,
              p => p.Application,
              p => p.Description,
              p => p.DynamicData,
              p => p.FeatureSet,
              p => p.NavigateUrl,
              p => p.Order,
              p => p.Page,
              p => p.Roles,
              p => p.Style,
              p => p.Title,
              p => p.Type);
            m2.MenuItems =
              m.MenuItems.Where(
                mi => mi.Visible && mi.Application == null || (mi.Application !=null && mi.Application.ApplicationName == "/") || (mi.Application != null && mi.Application.ApplicationName == application)).ToList();
            return m2;
          });
      // return all if all roles are applied
      return menus.ToList();
    }

    public CmsMenu LoadMenu(int id) {
      return Ctx.Menus.FirstOrDefault(m => m.Id == id);
    }

    public CmsMenuItem LoadMenuItem(int id) {
      return Ctx.MenuItems.FirstOrDefault(m => m.Id == id);
    }

    public void SaveMenu(CmsMenu menu) {
      if (!Ctx.Menus.Contains(menu)) {
        Ctx.Menus.Add(menu);
      }
      Ctx.SaveChanges();
    }

    public IEnumerable<CmsMenuItem> LoadMenuItems(int menuItemId) {
      return Ctx.MenuItems.Where(m => m.Menu.Any(p => p.Id == menuItemId));
    }

    # region Dynamic Menus

    private static Dictionary<string, object> CreatePropertyBag(string userName) {
      var propertyBag = new Dictionary<string, object>();
      var hsi = Manager<UserProfileManager>.Instance.GetHomeScreenInfo(userName);
      propertyBag.Add("ProjectsCount", hsi.ProjectsCount);
      propertyBag.Add("ProjectsEditable", hsi.ProjectsEditable);
      propertyBag.Add("BooksEditable", hsi.BooksEditable);
      propertyBag.Add("ProjectsAsLeader", hsi.ProjectsAsLeader);
      propertyBag.Add("TeamMemberCount", hsi.Memberships);
      propertyBag.Add("BooksCount", hsi.TextsPublished);
      propertyBag.Add("EditableCount", hsi.Editables);
      propertyBag.Add("Publishable", hsi.TextsPublishable);
      propertyBag.Add("ProductCount", hsi.Products);
      propertyBag.Add("WorkCount", hsi.Works);
      propertyBag.Add("ProfileExists", hsi.ProfileExists);
      propertyBag.Add("MessageCount", hsi.MessageCount);
      propertyBag.Add("ArchiveCount", hsi.ArchiveCount);
      propertyBag.Add("OrderCount", hsi.OrderCount);
      return propertyBag;
    }

    /// <summary>
    /// Loop through all public properties and try to replace {propname} in template
    /// </summary>
    /// <param name="template"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private static string ResolveProperties(string template, object data) {
      var result = template;
      foreach (var pi in data.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)) {
        try {
          object pv = pi.GetValue(data, null);
          if (pv == null) continue;
          var pn = pi.Name;
          result = result.Replace("{" + pn + "}", pv.ToString());
        } catch {
        }
      }
      return result;
    }

    public void ResolveDynamicMenus(List<CmsMenu> menu, string userName) {
      var propertyBag = CreatePropertyBag(userName);

      foreach (var m in menu) {
        // transform menu entries
        if (!String.IsNullOrEmpty(m.Description)) {
          if (!String.IsNullOrEmpty(m.DynamicData)) {
            var requestedValues = m.DynamicData.Split(',');
            for (int i = 0; i < requestedValues.Length; i++) {
              if (!propertyBag.ContainsKey(requestedValues[i])) continue;
              m.Description = m.Description.Replace("{" + i + "}", propertyBag[requestedValues[i]].ToString());
              m.DynamicDataResolved = propertyBag[requestedValues[i]].ToString();
            }
          }
        }
        // transform items
        foreach (var item in m.MenuItems) {
          if (!String.IsNullOrEmpty(item.Description)) {
            if (!String.IsNullOrEmpty(item.DynamicData)) {
              var requestedValues = item.DynamicData.Split(',');
              for (var i = 0; i < requestedValues.Length; i++) {
                if (!propertyBag.ContainsKey(requestedValues[i])) continue;
                item.Description = item.Description.Replace("{" + i + "}", propertyBag[requestedValues[i]].ToString());
                item.DynamicDataResolved = propertyBag[requestedValues[i]].ToString();
              }
            }
          }
          // build a completely dynamic menu, when the title is set the menu is filled
          if (!String.IsNullOrEmpty(item.DynaTitle)) {
            var val = propertyBag[item.DynaData] as IEnumerable; // assume this is a collection of some enum type
            if (val != null) {
              item.DynaMenuItems = new List<Tuple<string, string>>();
              foreach (var v in val) {
                item.DynaMenuItems.Add(new Tuple<string, string>(ResolveProperties(item.DynaTitle, v), ResolveProperties(item.DynaNavi, v)));
              }
            }
          }
        }
      }
    }

    # endregion

    public Dictionary<int, CmsMenuItem> LoadMenuItemFromPage() {
      return Ctx.MenuItems
          .Include(m => m.Menu)
          .Select(m => m).ToDictionary(k => k.Id);
    }
  }
}