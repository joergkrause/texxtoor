using System.IO.Compression;
using System.Web.Mvc;

namespace Texxtoor.BaseLibrary.Core {
  public class CompressAttribute : ActionFilterAttribute {

    public override void OnActionExecuting(ActionExecutingContext filterContext) {

      //get request and response 
      var request = filterContext.HttpContext.Request;
      var response = filterContext.HttpContext.Response;

      //get requested encoding 

      if (!string.IsNullOrEmpty(request.Headers["Accept-Encoding"])) {
        string enc = request.Headers["Accept-Encoding"].ToUpperInvariant();

        //preferred: gzip or wildcard 
        if (enc.Contains("GZIP") || enc.Contains("*")) {
          response.AppendHeader("Content-encoding", "gzip");
          response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
        }

        //deflate 
        else if (enc.Contains("DEFLATE")) {
          response.AppendHeader("Content-encoding", "deflate");
          response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
        }
      }
      base.OnActionExecuting(filterContext);
    }

  }
}

