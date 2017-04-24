using System.Web.Mvc;

namespace Texxtoor.EasyAuthor.Api {
  using System.IO;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Web.Http;

  using Texxtoor.BaseLibrary;
  using Texxtoor.EasyAuthor.Utilities;

  [RoutePrefix("api/images")]
  public class ImagesController : BaseApiController {
    [Route("{id:int}/{type:alpha}/{resolution:regex(^\\d{1,4}x\\d{1,4}$)?}")]
    public HttpResponseMessage Get(int id, string type, string resolution = Constants.Images.StandardResolutions._150x200) {
      var response = new HttpResponseMessage {
        Content = new StreamContent(new MemoryStream(ProjectManager.Instance.GetImage(id, type, resolution, UserName, true, null)))
      };
      response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
      return response;
    }
  }
}