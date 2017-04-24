using System.Web.Mvc;
using System.Collections.Generic;
using System;
using System.Linq;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Cms;

namespace Texxtoor.Portal.Core.Extensions {

  public static class UrlHelperExtensions {

    public static string BuildMenuLink(this UrlHelper urlHelper, CmsMenu item) {
      if (item.IsExternalUrl) {
        return item.NavigateUrl;
      }
      if (item.Page == null) {
        return urlHelper.Action("Index", "Home");
      }
      return urlHelper.Action("Page", "Home", new { id = item.Page.Id, name = item.Page.Alias });
    }


    public static string BuildMenuLink(this UrlHelper urlHelper, CmsMenuItem item) {
      if (item.IsExternalUrl) {
        return item.NavigateUrl;
      }
      if (item.Page == null) {
        return urlHelper.Action("Index", "Home");
      }
      return urlHelper.Action("Page", "Home", new { id = item.Page.Id, name = item.Page.Alias });
    }

    public static string PageUrl(this UrlHelper helper, CmsPage page) {
      var context = helper.RequestContext.HttpContext;
      return helper.Action("Page", "Home", new { area = "", id = page.Id, name = page.Alias });
    }

    public static string GetGlobalLayout(this UrlHelper helper, string layoutFile = null, string area = null) {
      //var session = helper.RequestContext.HttpContext.Session;
      //var culture = String.IsNullOrEmpty(session["culture"] as string) ? String.Empty : session["culture"].ToString() + ".";
      var layout = layoutFile ?? "_Layout";
      return String.Format("{0}/Views/Shared/{1}.cshtml", area == null ? "~" : "~/Areas/" + area, layout);
    }

  }
}