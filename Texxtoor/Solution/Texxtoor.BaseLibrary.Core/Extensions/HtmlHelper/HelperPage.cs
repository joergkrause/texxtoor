using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Texxtoor.BaseLibrary.Core.Extensions {
  public class HelperPage {
    public static HtmlHelper Html {
      get { return ((System.Web.Mvc.WebViewPage)WebPageContext.Current.Page).Html; }
    }

  }
}
