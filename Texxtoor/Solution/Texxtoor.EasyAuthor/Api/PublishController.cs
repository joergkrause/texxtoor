using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Texxtoor.BaseLibrary;

namespace Texxtoor.EasyAuthor.Api {

  [Authorize]
  [RoutePrefix("api/publish")]
  public class PublishController : BaseApiController {
    private readonly ProjectManager _projectUnitOfWork;
    private readonly ReaderManager _readerUnitOfWork;
    private readonly ResourceManager _resourceUnitOfWork;

    public PublishController() {
      _projectUnitOfWork = UnitOfWork<ProjectManager>();
      _readerUnitOfWork = UnitOfWork<ReaderManager>();
      _resourceUnitOfWork = UnitOfWork<ResourceManager>();
    }

    # region Publish

    [Route("publish/{id:int}/teamshares")]
    public HttpResponseMessage TeamShares(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      return Request.CreateResponse((HttpStatusCode)200, opus);
    }

    [Route("publish/{id:int}/channels")]
    public HttpResponseMessage Channels(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      return Request.CreateResponse((HttpStatusCode)200, opus);
    }

    [Route("publish/{id:int}/check")]
    public HttpResponseMessage CheckAndPublish(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      return Request.CreateResponse((HttpStatusCode)200, opus);
    }

    [HttpPost]
    [Route("publish/{id:int}/publish")]
    public HttpResponseMessage Publish(int id, PublishDto publish) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      return Request.CreateResponse((HttpStatusCode)200, opus);
    }

    # endregion

  }
}