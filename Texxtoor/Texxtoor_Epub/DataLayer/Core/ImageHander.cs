using System.Collections.Generic;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;

namespace Texxtoor.Editor.Core {
  public class ImageHander : IImageProvider {
    public string BaseUri;
    public Image GetImage(string src, IDictionary<string, string> h, ChainedProperties cprops, IDocListener doc) {
      string imgPath = string.Empty;

      if (src.ToLower().Contains("http://") == false) {
        imgPath = HttpContext.Current.Request.Url.Scheme + "://" +

                  HttpContext.Current.Request.Url.Authority + src;
      } else {
        imgPath = src;
      }

      return iTextSharp.text.Image.GetInstance(imgPath);
    }
  }
}