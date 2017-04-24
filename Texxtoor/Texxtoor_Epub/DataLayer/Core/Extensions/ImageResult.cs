using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Texxtoor.Editor.Extensions {

  /// <summary>
  /// This custom action returns an image stream. Used by the MvcResource handler approach.
  /// </summary>
  /// <remarks>
  /// the stream used to handover bytes is being disposed after reading.
  /// </remarks>
  public class ImageResult : ActionResult {
    public string SourceFilename { get; set; }
    public Stream SourceStream { get; set; }
    public string ContentType { get; set; }

    public ImageResult(string sourceFilename) {
      SourceFilename = sourceFilename;
      ContentType = String.Format("image/{0}", Path.GetExtension(sourceFilename));
    }

    public ImageResult(MemoryStream sourceStream, string contentType) {
      if (sourceStream == null) {
        SourceFilename = "~/Content/images/logo.png";
        ContentType = "image/png";
        return;
      }
      SourceStream = sourceStream;
      ContentType = contentType;
    }

    public ImageResult(byte[] data, string contentType) {
      if (data == null) {
        SourceFilename = "~/Content/images/logo.png";
        ContentType = "image/png";
        return;
      }
      SourceStream = new MemoryStream(data);
      ContentType = contentType;
    }

    public ImageResult(byte[] data) {
      SourceStream = new MemoryStream(data);
      ContentType = "image/png";
    }

    public ImageResult() {
      SourceFilename = "~/Content/images/logo.png";
      ContentType = "image/png";
    }

    public override void ExecuteResult(ControllerContext context) {
      var res = context.HttpContext.Response;
      res.Clear();
      res.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
      res.ContentType = ContentType;

      if (SourceStream is MemoryStream) {
        ((MemoryStream)SourceStream).WriteTo(res.OutputStream);
        SourceStream.Dispose();
      } else {
        res.TransmitFile(SourceFilename);
      }
    }

  }

}