using System.Web;
using System.Web.Mvc;
using Texxtoor.BusinessLayer.Core.Utilities.Storage;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.BusinessLayer.Finder {
  internal class DownloadFileResult : ActionResult {
    public ResourceFile File { get; private set; }
    public bool IsDownload { get; private set; }
    public DownloadFileResult(ResourceFile file, bool isDownload) {
      File = file;
      IsDownload = isDownload;
    }

    public override void ExecuteResult(ControllerContext context) {
      var response = context.HttpContext.Response;
      string fileName;
      var fileNameEncoded = HttpUtility.UrlEncode(File.Name);

      if (context.HttpContext.Request.UserAgent.Contains("MSIE")) {
        // IE < 9 do not support RFC 6266 (RFC 2231/RFC 5987)
        fileName = "filename=\"" + fileNameEncoded + "\"";
      } else {
        fileName = "filename*=UTF-8\'\'" + fileNameEncoded; // RFC 6266 (RFC 2231/RFC 5987)
      }
      string mime;
      string disposition;
      if (IsDownload) {
        mime = "application/octet-stream";
        disposition = "attachment; " + fileName;
      } else {
        mime = Helper.GetMimeType(File.Name) ?? File.MimeType;
        disposition = (mime.Contains("image") || mime.Contains("text") || mime == "application/x-shockwave-flash" ? "inline; " : "attachment; ") + fileName;
      }
      using (var blob = BlobFactory.GetBlobStorage(File.ResourceId, BlobFactory.Container.Resources)) {
        response.ContentType = mime;
        response.AppendHeader("Content-Disposition", disposition);
        response.AppendHeader("Content-Location", File.Name);
        response.AppendHeader("Content-Transfer-Encoding", "binary");
        response.AppendHeader("Content-Length", blob.Content != null ? blob.Content.Length.ToString() : "0");
        if (blob.Content != null) {
          response.BinaryWrite(blob.Content);
        }
      }
      response.End();
      response.Flush();
    }
  }
}
