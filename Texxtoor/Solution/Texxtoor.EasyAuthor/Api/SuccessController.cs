using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Texxtoor.BaseLibrary;

namespace Texxtoor.EasyAuthor.Api {

  [Authorize]
  [RoutePrefix("api/success")]
  public class SuccessController : BaseApiController {
    private readonly ProjectManager _projectUnitOfWork;
    private readonly ReaderManager _readerUnitOfWork;
    private readonly ResourceManager _resourceUnitOfWork;

    public SuccessController() {
      _projectUnitOfWork = UnitOfWork<ProjectManager>();
      _readerUnitOfWork = UnitOfWork<ReaderManager>();
      _resourceUnitOfWork = UnitOfWork<ResourceManager>();
    }

    # region Success

    [Route("success/{id:int}/totals")]
    public HttpResponseMessage GetTotals(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      return Request.CreateResponse((HttpStatusCode)200, opus);
    }

    [Route("success/{id:int}/channels")]
    public HttpResponseMessage GetChannels(int id, DateTime from, DateTime to) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      return Request.CreateResponse((HttpStatusCode)200, opus);
    }

    [Route("success/{id:int}/export")]
    public HttpResponseMessage GetExport(int id, DateTime from, DateTime to) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      return Request.CreateResponse((HttpStatusCode)200, opus);
    }

    # endregion

  }
}