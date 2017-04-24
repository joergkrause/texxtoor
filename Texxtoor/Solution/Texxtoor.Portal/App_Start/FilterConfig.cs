using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Texxtoor.Portal.Core.Attributes;

namespace Texxtoor.Portal {
  public class FilterConfig {
    public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
#if DEBUG
        // no anonymous acces in the release version
      if (Boolean.Parse(WebConfigurationManager.AppSettings["environment:UseHttps"])) {
        filters.Add(new RequireHttpsAttribute());
      }
#endif
      filters.Add(new HandleErrorAttribute());
      filters.Add(new ClickCountFilter() { ClickValue = 1, SourceType = DataModels.Models.Marketing.ClickSourceType.Portal });
    }
  }
}