using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Cms;

namespace Texxtoor.Portal.Core.Extensions {

  public static class HtmlHelperExtensions {

    public static MvcHtmlString PageLink(this HtmlHelper helper, CmsPage page, object htmlAttributes) {
      return PageLink(helper, null, page, htmlAttributes);
    }

    public static MvcHtmlString PageLink(this HtmlHelper helper, string linkText, CmsPage page,
                                         object htmlAttributes) {
      return helper.ActionLink(linkText ?? page.PageTitle, "Page", "Home",
                               new { area = "", id = page.Id, name = page.Alias }, htmlAttributes);
    }

    public static MvcHtmlString ActionPageLink(this HtmlHelper hh, CmsPage p) {
      return hh.ActionLink(hh.Raw(p.PageTitle).ToHtmlString(), "Page", "Home",
                           new { id = p.Id, name = p.Alias, area = "" },
                           new { title = p.PageTitle });
    }

    public static MvcHtmlString ActionPageLink(this HtmlHelper hh, string linkText, string alias) {
      return hh.ActionLink(linkText, "Page", "Home",
                           new { id = 0, name = alias, area = "" },
                           new { title = linkText });
    }

    public static MvcHtmlString ActionPageLink(this HtmlHelper hh, string linkText, int id) {
      return hh.ActionLink(linkText, "Page", "Home",
                           new { id = id, area = "" },
                           new { title = linkText });
    }

    public static MvcHtmlString DeleteLink(this HtmlHelper helper, string text, string action, string controller,
                                   object routeValues,
                                   object htmlAttributes) {
      return DeleteLink(helper, text, action, controller, new RouteValueDictionary(routeValues),
                        new RouteValueDictionary(htmlAttributes));
    }

    public static MvcHtmlString DeleteLink(this HtmlHelper helper, string text, string action, string controller,
                                           RouteValueDictionary routeValues,
                                           IDictionary<string, object> htmlAttributes) {
      if (htmlAttributes != null && !htmlAttributes.ContainsKey("onclick")) {
        htmlAttributes.Add("onclick", "deletePost(this.href); return false;");
      }

      return helper.ActionLink(text, action, controller, routeValues, htmlAttributes);
    }

    public static string PageLink(this HtmlHelper helper, CmsPage page) {
      if (null != page) {
        return page.GetUrl();
      }

      return string.Empty;
    }

    public static bool IsDefaultPage(this HtmlHelper helper) {
      return
          "Home".Equals(helper.ViewContext.RouteData.GetRequiredString("controller"),
                        StringComparison.InvariantCultureIgnoreCase) &&
          "Default".Equals(helper.ViewContext.RouteData.GetRequiredString("action"),
                           StringComparison.InvariantCultureIgnoreCase);
    }
  }
}