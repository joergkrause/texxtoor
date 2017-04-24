using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Texxtoor.BaseLibrary;
using Texxtoor.EasyAuthor.Models;

namespace Texxtoor.EasyAuthor.Api {

  [Authorize]
  [RoutePrefix("api/preview")]
  public class PreviewController : BaseApiController {
    private readonly ProjectManager _projectUnitOfWork;
    private readonly ReaderManager _readerUnitOfWork;
    private readonly ResourceManager _resourceUnitOfWork;

    public PreviewController() {
      _projectUnitOfWork = UnitOfWork<ProjectManager>();
      _readerUnitOfWork = UnitOfWork<ReaderManager>();
      _resourceUnitOfWork = UnitOfWork<ResourceManager>();
    }


    # region Preview

    [HttpGet]
    [Route("preview/{id:int}/content")]
    public HttpResponseMessage GetPreviewContent(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      // TODO: Create a flat view of the content's sections
      var opusDto = new OpusDto {
        Title = opus.Name,
        SubTitle = opus.Project.Name,
        Description = opus.Project.Description,
        Authors = opus.Project.Team.Members.Select(m => new MemberDto { FullName = m.Member.UserName })
      };
      return Request.CreateResponse((HttpStatusCode)200, opusDto);
    }

    [HttpGet]
    [Route("preview/{id:int}/download/{type}")]
    public HttpResponseMessage Download(int id, string type) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      return Request.CreateResponse((HttpStatusCode)200, opus);
    }

    # endregion

  }
}