using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Extensions.ActionResults;
using Texxtoor.BaseLibrary.Core.HtmlAgility.Pack;
using Texxtoor.BaseLibrary;
using Texxtoor.BaseLibrary.Services;
using Texxtoor.BusinessLayer.Services;
using Texxtoor.DataModels.Models.Cms;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.BusinessLayer;

namespace Texxtoor.Portal.Controllers {

  [HandleError]
  public class ToolsController : ControllerExt {

    public ActionResult Error(int id, string aspxerrorpath) {
      Response.StatusCode = id;
      switch (id) {
        case 400:
          return Content("Something went wrong using last action on " + aspxerrorpath);
        case 401:
          return Content("You're not authorized for last action on " + aspxerrorpath);
        case 403:
          return Content("You're not allowed to access resources on " + aspxerrorpath);
        case 404:
          return Content("The item you attempt to get is not there");
      }
      return Content("Bad Request (custom)" + aspxerrorpath);
    }

    [CurrentUserFilter]
    public JsonResult CreateAlias(string title, string userName) {
      if (string.IsNullOrEmpty(title)) {
        title = "invalid value";
      }
      var page = new CmsPage { PageTitle = title, Alias = CmsPage.CreateAlias(title) };
      PageService.Instance.CreateEntry(page, Manager<UserManager>.Instance.GetCurrentUser(UserName));
      return Json(new { alias = page.Alias, id = page.Id });
    }

    [CurrentUserFilter]
    public JsonResult UpdateAlias(int id, string alias, string userName) {
      try {
        var page = PageService.Instance.LoadPage(id.ToString());
        var user = Manager<UserManager>.Instance.GetCurrentUser(UserName);
        if (null == page || string.IsNullOrEmpty(alias) || user == null) {
          throw new NullReferenceException("Page was not found, or user input is empty.");
        }

        page.Alias = alias;
        PageService.Instance.Save(page, user);
        return Json(new { alias = page.Alias, id = page.Id });
      } catch (Exception) {
        throw;
      }
    }

    public string FreeHtml(string htmlKey) {
      return htmlKey; //settingService.GetSettingValue(htmlKey, HttpContext.Cache);
    }

    public MvcHtmlString BreadCrumb(CmsPage currentPage) {
      const string seperator = " > ";
      var lst = new List<string>();
      var menuitems = MenuService.Instance.LoadMenuItemFromPage();
      foreach (var item in currentPage.MenuItem) {
        lst.Add(item.Title);
        if (menuitems.Keys.Contains(item.Id)) {
          string[] titles = menuitems[item.Id].Menu.Select(m => m.Title).ToArray();
          lst.Add(String.Join("|", titles));
        }
      }
      lst.AddRange(currentPage.Menu.Select(menu => menu.Title));
      var bc = string.Join(seperator, lst.ToArray().Reverse());
      return MvcHtmlString.Create(bc);
    }

    public ActionResult GetEpubTitlePage(int id) {
      var work = UnitOfWork<ReaderManager>().GetWork(id, true);
      var bytes = work.Extern == WorkType.External ? work.ExternalBook.CoverImage : null;
      if (bytes == null) {
        return Content("<html><body><h3>No Cover Found</h3></body></html>");
      }
      // assume bytes are HTML
      var html = Encoding.UTF8.GetString(bytes);
      var doc = new HtmlDocument();
      doc.LoadHtml(html);
      // HTML <img>
      var img = doc.DocumentNode.SelectNodes("//img");
      if (img != null) {
        img.ToList()
           .ForEach(
             n =>
             n.Attributes["src"].Value =
             String.Format("/Tools/GetImg/{0}?c=Epub&res=150x190&href={1}", id, n.Attributes["src"].Value));
      }
      // SVG <image>
      img = doc.DocumentNode.SelectNodes("//image");
      if (img != null) {
        img.ToList()
           .ForEach(
             n =>
             n.Attributes["xlink:href"].Value =
             String.Format("/Tools/GetImg/{0}?c=Epub&res=150x190&href={1}", id, n.Attributes["xlink:href"].Value));
      }
      return Content(doc.DocumentNode.InnerHtml);
    }

    public ImageResult GetFinderThumbnails(int id) {
      return GetImg(id, "FinderResource", "32x32", true);
    }

    [NoCache]
    [ValidateInput(false)]
    public ImageResult GetImg(int id, string c, string res, bool nc = true, string href = "") {
      var bytes = ProjectManager.Instance.GetImage(id, c, res, UserName, nc, href);
      const string mimeType = "image/png";
      return new ImageResult(bytes, mimeType);
    }

    public ImageResult GetImgQuickOrder(int id, string res) {
      return GetImg(id, "ProjectCover", res, false);
    }

    # region Template Helpers

    [OutputCache(VaryByParam = "id", Duration = 86400)]
    public JsonResult RegionForCountry(int id) {
      Func<string, string> encoder = (s) => {
        var iso = Encoding.GetEncoding("ISO-8859-1");
        var utf8 = Encoding.UTF8;
        var utfBytes = utf8.GetBytes(s);
        var isoBytes = Encoding.Convert(utf8, iso, utfBytes);
        var encoding = new UTF8Encoding();
        return encoding.GetString(isoBytes);
      };
      var q = UnitOfWork<UserProfileManager>().GetCountryList()
        .Single(c => c.Id == id)
        .Cities
        .GroupBy(c => c.District)
        .OrderBy(c => c.Key)
        .Select(c => encoder(c.Key));
      return Json(new { q }, JsonRequestBehavior.AllowGet);
    }

    # endregion

  }
}