using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Texxtoor.BaseLibrary.Core.Extensions {

  public static class ViewHelper {

    public static MvcHtmlString InteractiveHeader(this UrlHelper url, string path) {
      // Take web.sitemap
      // look for url
      // get backlinks
      var sm = url.RequestContext.HttpContext.Application["WebSiteMap"] as XDocument;
      var node = sm.Descendants("siteMapNode")
        .Where(e => e.Attribute("url").Value == path)
        .Select(e => new {
          title = e.Attribute("title").Value,
          backLinks = e.Attribute("backLinks") == null ? null : e.Attribute("backLinks").Value.Split(',')
        })
        .First();
      var links = new List<string>();      
      foreach (var item in node.backLinks) {
        var n = sm.Descendants("siteMapNode").First(e => e.Attribute("id").Value == item);
        var t = n.Attribute("title").Value;
        var u = n.Attribute("url").Value.Split('/');
        var area = u[0];
        var ctrl = u[1];
        var actn = u[2];
        var clss = "";
        switch (area) {
          case "AuthorPortal":
            clss = "texxtoorClr1 backArrowRed";
            break;
          case "ReaderPortal":
            clss = "texxtoorClr2 backArrowYellow";
            break;
          case "TeamPortal":
            clss = "texxtoorGreen backArrowGreen";
            break;
          case "":
            clss = "texxtoorBlue backArrowBlue";
            break;
        }
        object r = null;
        links.Add(String.Format(@"<a href=""{0}"" class=""backNavButton {2}"">{1}</a>", url.Action(actn, ctrl, r), t, clss));
      }
      return new MvcHtmlString(String.Format("<h1>{0}{1}</h1>", node.title));
    }


  }
}
