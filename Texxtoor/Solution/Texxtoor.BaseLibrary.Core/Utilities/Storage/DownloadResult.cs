using System.IO;
using System.Web.Mvc;

namespace Texxtoor.BaseLibrary.Core.Utilities.Storage {
  public class DownloadResult : ActionResult {

    public string VirtualPath { get; set; }

    public override void ExecuteResult(ControllerContext context) {

      string filePath = context.HttpContext.Server.MapPath(this.VirtualPath);
      context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + Path.GetFileName(filePath));
      context.HttpContext.Response.TransmitFile(filePath);
    }
  }
}