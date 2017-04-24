using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Extensions.ActionResults;
using Texxtoor.BaseLibrary.Globalization.Provider;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.Portal.Services;
using ResourceManager = Texxtoor.BaseLibrary.Globalization.Provider.ResourceManager;

namespace Texxtoor.Portal.Controllers {
  public class ThemeSwitcherController : ControllerExt {

    public ActionResult SetCulture(string culture) {
      if (User.Identity.IsAuthenticated) {
        var rm = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName).RunControl;
        rm.UiLanguage = culture;
        UnitOfWork<UserProfileManager>().SaveChanges();
      }
      Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
      Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
      var url = Request.UrlReferrer != null ?
                Request.UrlReferrer.ToString() :
                Url.Action("Index", "Home");
      return Redirect(url);
    }

    // Change from resource editor temporarily
    public JsonResult SwitchCulture(string culture) {
      if (User.Identity.IsAuthenticated) {
        var rm = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName).RunControl;
        rm.UiLanguage = culture ?? "de";
        UnitOfWork<UserProfileManager>().SaveChanges();
      }
      return Json(new { data = culture });
    }

    /// <summary>
    /// Returns the images stored as embedded resource in the resource handler library.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ImageResult GetImage(string id) {
      // d containes the websource : assembly|type
      var a = Encoding.ASCII.GetString(HttpServerUtility.UrlTokenDecode(id)).Split('|');
      var ass = ResourceManager.AssemblyInfoCache[a[0]];
      var ms = new MemoryStream();
      var manifestResourceStream = ass.GetManifestResourceStream(a[1]);
      if (manifestResourceStream != null) manifestResourceStream.CopyTo(ms);
      var contentType = String.Format("image/{0}", Path.GetExtension(a[1]));
      return new ImageResult(ms, contentType);
    }

    [HttpPost]
    public JsonResult ReadResource(string path, string key, string culture) {
      var c = String.IsNullOrEmpty(culture) ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(culture);
      var data = HttpContext.GetLocalResourceObject(path, key, c);

      return Json(data);
    }

    [ValidateInput(false)]
    [HttpPost]
    public JsonResult SaveResource(string path, string key, string culture, string data) {
      try {
        var c = String.IsNullOrEmpty(culture) ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(culture);
        culture = c.TwoLetterISOLanguageName;
        c = new CultureInfo(culture);        
        // remove unwanted para elements
        if (data.StartsWith("<p>") && data.EndsWith("</p>")) {
          data = data.Substring(3, data.Length - 7);
        }
        DbResourceDataManager.UpdateOrAdd(key, data, c, path);
        // UNDO
        Stack q;
        if (Session[String.Format("Undo_{0}_{1}_{2}", path, key, culture)] == null) {
          q = new Stack();
          q.Push(data);
        } else {
          q = (Stack)Session[String.Format("Undo_{0}_{1}_{2}", path, key, culture)];
          q.Push(data);
        }
        Session[String.Format("Undo_{0}_{1}_{2}", path, key, culture)] = q;
        return Json(new {
          msg = "OK (" + culture + ")", data
        }); // 0 (added) or 1 (updated)
      } catch (Exception ex) {
        return Json(new {
          msg = ex.Message, data
        }); // 0 (added) or 1 (updated)
      }
    }

    // trying to retrieve the last save value
    [HttpPost]
    public JsonResult UndoResource(string path, string key, string culture) {
      var q = Session[String.Format("Undo_{0}_{1}_{2}", path, key, culture)] as Stack;
      string data = null;
      if (q != null) {
        //retrieve the one before the last one (if any)
        if (q.Count > 1) {
          q.Pop();
          data = q.Peek() as string;
        }
        if (q.Count == 1) {
          data = q.Pop() as string;
        }
      }
      return Json(data);
    }

    [HttpPost]
    public JsonResult TranslateText(string type, string text, string from, string to) {
      var translatorType = (Translators)Enum.Parse(typeof(Translators), type, true);
      var t = TranslatorFactory.GetTranslator(translatorType);
      var sb = new StringBuilder();
      if (text.Contains('.')) {
        // Engines cannot translate more than one sentence (auto cut after period sign).
        var sentences = text.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var tt in sentences.Select(txt => t.Translate(text, @from, to))) {
          sb.AppendFormat("{0}{1}", tt, tt.EndsWith(".") ? "" : "."); // some engines remove the ".", some not, so we attach it if necessary
        }
      }
      else {
        // single word or phrase
        sb.Append(t.Translate(text, from, to));
      }
      return Json(sb.ToString());
    }

    public ContentResult ResourceSummary(string view) {
      var cc = new CultureInfo(CurrentCulture);
      var cp = new CultureInfo(CurrentCulture).Parent;
      var ci = CultureInfo.InvariantCulture;
      var resourceCulture = DbResourceDataManager.GetResourceSet(cc, view);
      var resourceNeutral = DbResourceDataManager.GetResourceSet(cp, view);
      var resourceInvariant = DbResourceDataManager.GetResourceSet(ci, view);
      return Content(String.Format(@"<ul><li>{3} = {0}</li><li>{4} = {1}</li><li>{5} = {2}</li>",
        resourceCulture.Count,
        resourceNeutral.Count,
        resourceInvariant.Count,
        cc.TwoLetterISOLanguageName,
        cp.TwoLetterISOLanguageName,
        "INV"));
    }

  }
}