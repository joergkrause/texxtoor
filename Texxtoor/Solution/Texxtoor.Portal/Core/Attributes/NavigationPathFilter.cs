using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;

namespace Texxtoor.Portal.Core.Attributes {

  /// <summary>
  /// Used to track the path the user navigates. This creates a row of buttons in the user's menu.
  /// </summary>
  public class NavigationPathFilter : ActionFilterAttribute {

    private const string KEY = "NavigationPath";
    private const int LIMIT = 8;

    /// <summary>
    /// The name to display or resource key.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// This item is informational, no direct links possible (usually Script element)
    /// </summary>
    public bool Informational { get; set; }

    public NavigationPathFilter(string name)
      : base() {
        Name = name;
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext) {

      if (filterContext.HttpContext.Session[KEY] == null) {
        filterContext.HttpContext.Session[KEY] = new Queue<KeyValuePair<string, string>>(LIMIT);
      }
      // limit queue to 8 items
      Queue<KeyValuePair<string, string>> q = (Queue<KeyValuePair<string, string>>)filterContext.HttpContext.Session[KEY];
      if (q.Count == LIMIT) {
        q.Dequeue();
      }
      var url = (Informational ? "" : filterContext.HttpContext.Request.Url.PathAndQuery);
      // enqueue only if not the same again
      var last = q.Any() ? q.ToArray().Last() : new KeyValuePair<string, string>();
      if (q.Count == 0 || (last.Key != Name || last.Value != url)) {
        q.Enqueue(new KeyValuePair<string, string>(Name, url));
      }
      base.OnActionExecuting(filterContext);
    }

  }
}