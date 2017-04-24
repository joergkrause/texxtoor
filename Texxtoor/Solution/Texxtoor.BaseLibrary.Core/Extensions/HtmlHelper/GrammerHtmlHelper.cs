using System;
using System.Web;
using System.Web.Mvc;

namespace Texxtoor.BaseLibrary.Core.Extensions {
    public static class GrammerHtmlHelper {

      public static MvcHtmlString Pluralize(this HtmlHelper html, int value, IHtmlString singular, IHtmlString plural) {
        return new MvcHtmlString(String.Format((value == 1) ? singular.ToString() : plural.ToString(), value));
      }

      public static MvcHtmlString Pluralize(this HtmlHelper html, int value, string singular, string plural) {
        return new MvcHtmlString(String.Format((value == 1) ? singular : plural, value));
      }

      public static MvcHtmlString Pluralize(this HtmlHelper html, int value, string singular, string plural, string nullWord) {
        var val = (value == 0) ? nullWord : value.ToString();
        return new MvcHtmlString(String.Format((value == 1 || value == 0) ? singular : plural, val));
      }

    }
   
}
