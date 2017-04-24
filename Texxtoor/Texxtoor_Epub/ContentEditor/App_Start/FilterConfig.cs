using System.Web.Mvc;

namespace Texxtoor.Editor {
  public class FilterConfig {
    public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
      filters.Add(new HandleErrorAttribute());
    }
  }
}