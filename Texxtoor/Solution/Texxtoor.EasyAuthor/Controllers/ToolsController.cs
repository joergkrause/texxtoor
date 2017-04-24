using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Caching;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Extensions.ActionResults;
using Texxtoor.BaseLibrary;
using Texxtoor.EasyAuthor.Core.Extensions;

namespace Texxtoor.EasyAuthor.Controllers {

  [HandleError]
  public class ToolsController : ControllerExt {

    public ActionResult SetCulture(string culture) {
      Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
      Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
      var url = Request.UrlReferrer != null ?
                Request.UrlReferrer.ToString() :
                Url.Action("Index", "Home");
      return Redirect(url);
    }

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


    public ImageResult GetFinderThumbnails(int id) {
      return GetImg(id, "FinderResource", "32x32", true);
    }

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