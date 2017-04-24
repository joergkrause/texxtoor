using System.Web.Mvc;

namespace Texxtoor.BaseLibrary.Core.Utilities.Storage {
  public class BlobResult : ActionResult {

    public IBlob Blob { get; set; }

    public override void ExecuteResult(ControllerContext context) {
      context.HttpContext.Response.ContentType = Blob["MimeType"] == null ? "unknown/unknown" : Blob["MimeType"].ToString();
      var fileBlob = Blob as FileBlob;
      if (fileBlob != null) {
        context.HttpContext.Response.TransmitFile(fileBlob.DataFilePath);
      }
      var blob = Blob as Blob;
      if (blob != null) {
        context.HttpContext.Response.TransmitFile(blob.BlobUri.AbsoluteUri);
      }
    }
  }
}