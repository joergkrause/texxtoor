using System.Net;
using System.Net.Http;
using System.Web.Http;
using Texxtoor.BaseLibrary;

namespace Texxtoor.EasyAuthor.Api {

  [Authorize]
  [RoutePrefix("api/marketing")]
  public class MarketingController : BaseApiController {
    private readonly ProjectManager _projectUnitOfWork;
    private readonly ReaderManager _readerUnitOfWork;
    private readonly ResourceManager _resourceUnitOfWork;

    public MarketingController() {
      _projectUnitOfWork = UnitOfWork<ProjectManager>();
      _readerUnitOfWork = UnitOfWork<ReaderManager>();
      _resourceUnitOfWork = UnitOfWork<ResourceManager>();
    }

    # region Marketing

    [Route("marketing/{id:int}/rss")]
    public HttpResponseMessage Rss(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      return Request.CreateResponse((HttpStatusCode)200, opus);
    }

    [Route("marketing/{id:int}/quicklink")]
    public HttpResponseMessage QuickLink(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      return Request.CreateResponse((HttpStatusCode)200, opus);
    }

    [Route("marketing/{id:int}/shop/{shop}")]
    public HttpResponseMessage Shop(int id, string shop) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      return Request.CreateResponse((HttpStatusCode)200, opus);
    }

    # endregion

  }
}