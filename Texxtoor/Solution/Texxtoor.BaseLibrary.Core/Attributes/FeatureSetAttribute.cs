using System.IO.Compression;
using System.Web.Mvc;
using System.Web.Configuration;
using System;
using System.Collections.Generic;

namespace Texxtoor.BaseLibrary.Core {

  /// <summary>
  /// In certain environments some features are not allowed. If set, a distinct feature must correspond to the web.config settings.
  /// </summary>
  public class FeatureSetAttribute : ActionFilterAttribute {

    private string Set { get; set; }

    public FeatureSetAttribute()
      : base() {
    }

    public FeatureSetAttribute(string set)
      : this() {
      Set = set;
    }

    public override void OnResultExecuting(ResultExecutingContext filterContext) {

      //get request and response 
      var request = filterContext.HttpContext.Request;
      var response = filterContext.HttpContext.Response;


      base.OnResultExecuting(filterContext);
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext) {

      //get request and response 
      var request = filterContext.HttpContext.Request;
      var response = filterContext.HttpContext.Response;
      bool allowed = true;
      var controller = filterContext.Controller;
      if (controller != null) {
        var setRequest = WebConfigurationManager.AppSettings["ui:FeatureSet"];
        var allowedSets = new List<string>(setRequest.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
        allowed = allowedSets.Contains(Set);
      }

      if (!allowed) {
        filterContext.Result = new RedirectToRouteResult(
          new System.Web.Routing.RouteValueDictionary { 
            { "controller", "Home" },
            { "action", "FeatureNotAvailable" },
            { "area" , "" }
          });
      }

      base.OnActionExecuting(filterContext);
    }

  }
}

