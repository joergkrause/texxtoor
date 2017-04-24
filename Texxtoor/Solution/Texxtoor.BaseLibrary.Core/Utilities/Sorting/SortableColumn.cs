using System;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace Texxtoor.BaseLibrary.Core.Utilities.Sorting {
  public static class SortableExt {

    public static string SortableColumn(this HtmlHelper htmlHelper, string linkText, string columnName, object routeValues) {
      //automatically determine the current action
      RouteData data = htmlHelper.ViewContext.Controller.ControllerContext.RouteData;
      string actionName = data.GetRequiredString("action");

      var sb = new StringBuilder();
      var vals = new RouteValueDictionary(routeValues);

      string sidx = String.Empty;
      if (System.Web.HttpContext.Current.Request["sidx"] != null) {
        sidx = System.Web.HttpContext.Current.Request["sidx"];
      }

      //modify the sidx
      if (vals.ContainsKey("sidx") == false) {
        vals.Add("sidx", columnName);
      } else {
        vals["sidx"] = columnName;
      }

      //get the sort order from the request variable if it exists
      string sord = String.Empty;
      if (System.Web.HttpContext.Current.Request["sord"] != null) {
        sord = System.Web.HttpContext.Current.Request["sord"];
      }

      //add the sord key if needed
      if (vals.ContainsKey("sord") == false) {
        vals.Add("sord", String.Empty);
      }

      //if column matches
      if (sidx.Equals(columnName, StringComparison.CurrentCultureIgnoreCase)) {
        if (sord.Equals("asc", StringComparison.CurrentCultureIgnoreCase)) {
          //draw the ascending sort indicator using the wingdings font. 
          sb.Append(" <font face='Wingdings 3'>&#112;</font>");
          vals["sord"] = "desc";
        } else {
          sb.Append(" <font face='Wingdings 3'>&#113;</font>");
          vals["sord"] = "asc";
        }
      }
      else {
        //if the column does not match then force the next sort to ascending order
        vals["sord"] = "asc";
      }

      //Use the ActionLink to build the link and insert it into the string
      sb.Insert(0, System.Web.Mvc.Html.LinkExtensions.ActionLink(htmlHelper, linkText, actionName, vals));
      return sb.ToString();
    }
  }
}
