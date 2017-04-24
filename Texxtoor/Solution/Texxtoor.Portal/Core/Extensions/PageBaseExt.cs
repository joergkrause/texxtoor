using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Globalization.Extensions;

namespace Texxtoor.Portal.Core.Extensions {


  public abstract class PageBaseExt : WebViewPage {

    public IHtmlString Loc(string id, string fallBack, params object[] args){
      return Html.Loc(id, fallBack, args);
    }

  }

  public abstract class PageBaseExt<TModel> : WebViewPage<TModel> {

    public IHtmlString Loc(string id, string fallBack, params object[] args) {
      return Html.Loc(id, fallBack, args);
    }

  }

}