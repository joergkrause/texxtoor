using System.Web;
using System.Web.Mvc;

namespace Texxtoor.EasyAuthor {
  public class FilterConfig {
    public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
#if !DEBUG
        // no anonymous acces in the release version
        //filters.Add(new RequireAuthenticationAttribute());
#endif
      filters.Add(new HandleErrorAttribute());
    }
  }
}