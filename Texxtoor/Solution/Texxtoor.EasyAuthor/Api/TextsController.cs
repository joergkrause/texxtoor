using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.ViewModels.Content;
using Texxtoor.EasyAuthor.Models;
using Texxtoor.ViewModels.Content;

namespace Texxtoor.EasyAuthor.Api {

  [Authorize]
  [RoutePrefix("api/texts")]
  public class TextsController : BaseApiController {
    private readonly ProjectManager _projectUnitOfWork;
    private readonly ReaderManager _readerUnitOfWork;
    private readonly ResourceManager _resourceUnitOfWork;

    public TextsController() {
      _projectUnitOfWork = UnitOfWork<ProjectManager>();
      _readerUnitOfWork = UnitOfWork<ReaderManager>();
      _resourceUnitOfWork = UnitOfWork<ResourceManager>();
    }

    # region Dashboard

    [HttpGet]
    [Route("all")]
    public HttpResponseMessage GetAllText() {
      var opuses = _projectUnitOfWork.GetAllOpusForUser(this.UserName, false).ToList();
      var opusesDTO = opuses.Select(x => new OpusDto(this.UserName, x, _projectUnitOfWork, _readerUnitOfWork)).ToList();
      return Request.CreateResponse((HttpStatusCode)200, opusesDTO);
    }

    [HttpGet]
    [Route("published")]
    public HttpResponseMessage GetPublishedTexts() {
      var opuses = _projectUnitOfWork.GetAllOpusForUser(this.UserName, false)
        .ToList()
        .Where(o => o.IsPublished);
      var opusesDTO = opuses.Select(x => new OpusDto(this.UserName, x, _projectUnitOfWork, _readerUnitOfWork)).ToList();
      return Request.CreateResponse((HttpStatusCode)200, opusesDTO);
    }

    # endregion

    # region Overview

    [HttpGet]
    [Route("overview/{id:int}/gettext")]
    public HttpResponseMessage GetText(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      // we need default's of marketing package here, so we check this first and adjust database accordingly
      var ovDto = new OverviewDto(UserName, opus, _projectUnitOfWork, _readerUnitOfWork);
      return Request.CreateResponse((HttpStatusCode)200, ovDto);
    }


    [HttpGet]
    [Route("overview/{id:int}/metadata")]
    public HttpResponseMessage GetMetadata(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      var metadata = new MetadataDto {
        Title = opus.Name,
        Subtitle = opus.Project.Name,
        Description = opus.Project.Description,
        Publisher = opus.Published != null ? opus.Published.Imprint.Name : "",
        //PublishingDate = opus.Published != null ? opus.Published.CreatedAt : DateTime.MinValue,
        Categories = opus.Published != null ? opus.Published.Catalogs.Select(c => c.Name) : null,
        Language = opus.LocaleId        
      };
      return Request.CreateResponse((HttpStatusCode)200, metadata);
    }
    
    [HttpPut]
    [Route("overview/{id:int}/metadata")]
    public HttpResponseMessage SaveMetadata(int id, MetadataDto metaData) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      // TODO: Save a variaty of data to Published, Marketing, Project, Opus
      return Request.CreateResponse((HttpStatusCode)200, "");
    }

    [HttpGet]
    [Route("overview/{id:int}/content")]
    public HttpResponseMessage GetContent(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      var sections = opus.Children.FlattenHierarchy().OfType<Section>().Select(s => new SnippetDto(s));
      return Request.CreateResponse((HttpStatusCode)200, sections);
    }

    [HttpPost]
    [Route("overview/{id:int}/addsection")]
    public HttpResponseMessage AddSection(int id, int parentId, string title) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      // TODO:
      return Request.CreateResponse((HttpStatusCode)200, "");
    }

    [HttpPut]
    [Route("overview/{id:int}/renamesection")]
    public HttpResponseMessage RenameSection(int id, int sectionId, string title) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      // TODO:
      return Request.CreateResponse((HttpStatusCode)200, "");
    }

    [HttpDelete]
    [Route("overview/{id:int}/deletesection")]
    public HttpResponseMessage DeleteSection(int id, int sectionId) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      // TODO: 
      return Request.CreateResponse((HttpStatusCode)200, "");
    }

    [HttpGet]
    [Route("overview/{id:int}/imprint")]
    public HttpResponseMessage GetImprint(int id) {
      var imprint = _projectUnitOfWork.GetImprint(UserName);
      var iadto = new ImprintAddressDto {
        AboutUs = imprint.AboutUs,
        Firm = imprint.Firm,
        Name = imprint.Name,
        ImprintId = imprint.Id,
        Description = imprint.Description,
        City = imprint.Address.City,
        StreetNumber = imprint.Address.StreetNumber,
        Zip = imprint.Address.Zip,
        Region = imprint.Address.Region,
        OwnerId = imprint.Owner.Id
      };
      return Request.CreateResponse((HttpStatusCode)200, iadto);
    }

    [HttpPut]
    [Route("overview/{id:int}/imprint")]
    public HttpResponseMessage EditImprint(ImprintAddressDto iadto) {
      var user = UserManager.Instance.GetUserByName(UserName);
      var profile = user.Profile.Addresses.FirstOrDefault();
      var imprint = new ImprintAddress {
        AboutUs = iadto.AboutUs,
        Firm = iadto.Firm,
        Name = iadto.Name,
        OwnerId = user.Id,
        Description = iadto.Description
      };
      if (profile != null) {
        imprint.City = profile.City;
        imprint.StreetNumber = profile.StreetNumber;
        imprint.Zip = profile.Zip;
        imprint.Region = profile.Region;
        imprint.AddressId = profile.Id;
      }
      _projectUnitOfWork.AddOrUpdateImprint(imprint, UserName);
      return Request.CreateResponse((HttpStatusCode)200, imprint);
    }

    [HttpPut]
    [Route("overview/{id:int}/content")]
    public HttpResponseMessage SaveSection(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      // TODO: Create a flat view of the content's sections
      return Request.CreateResponse((HttpStatusCode)200, "");
    }

    [HttpGet]
    [Route("overview/{id:int}/semantics")]
    public HttpResponseMessage GetSemantics(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      // this creates a default termset that we use here without ever changing it
      var ts = _projectUnitOfWork.GetTermSetsForProject(opus.LocaleId, UserName, opus.Project.Id).Single();
      var terms = ts.Terms.Select(t => new TermDto {
        Id = t.Id,
        Type = t.GetLocalizedTermType(),
        Data = t.Content,
        Name = t.Text
      });
      return Request.CreateResponse((HttpStatusCode)200, terms);
    }

    [HttpPost]
    [Route("overview/{id:int}/semantics")]
    public HttpResponseMessage AddSemantic(int id, TermDto term) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      // this creates a default termset that we use here without ever changing it
      var ts = _projectUnitOfWork.GetTermSetsForProject(opus.LocaleId, UserName, opus.Project.Id).Single();
      var terms = ts.Terms.Select(t => new TermDto {
        Type = t.GetLocalizedTermType(),
        Data = t.Content,
        Name = t.Text
      });
      return Request.CreateResponse((HttpStatusCode)200, "");
    }

    [HttpPut]
    [Route("overview/{id:int}/semantics")]
    public HttpResponseMessage EditSemantic(int id, TermDto term) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      // this creates a default termset that we use here without ever changing it
      var ts = _projectUnitOfWork.GetTermSetsForProject(opus.LocaleId, UserName, opus.Project.Id).Single();
      var terms = ts.Terms.Select(t => new TermDto {
        Type = t.GetLocalizedTermType(),
        Data = t.Content,
        Name = t.Text
      });
      return Request.CreateResponse((HttpStatusCode)200, "");
    }

    [HttpDelete]
    [Route("overview/{id:int}/semantics")]
    public HttpResponseMessage DeleteSemantic(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      // this creates a default termset that we use here without ever changing it
      var ts = _projectUnitOfWork.GetTermSetsForProject(opus.LocaleId, UserName, opus.Project.Id).Single();
      var terms = ts.Terms.Select(t => new TermDto {
        Type = t.GetLocalizedTermType(),
        Data = t.Content,
        Name = t.Text
      });
      return Request.CreateResponse((HttpStatusCode)200, "");
    }

    [HttpGet]
    [Route("overview/{id:int}/files")]
    public HttpResponseMessage Files(int id) {
      var opus = _projectUnitOfWork.GetOpus(id, UserName);
      var resContent = _projectUnitOfWork.GetResourceFiles(opus, TypeOfResource.Content, null, null, true);
      var resProject = _projectUnitOfWork.GetResourceFiles(opus, TypeOfResource.Project, null, null, true);

      Predicate<int> isPublished = fileId => false;

      Func<ResourceFile, FileDto> getFiles = (file => new FileDto {
        Name = file.Name,
        Id = file.Id,
        Size = file.FileSizeString,
        Date = file.CreatedAt.ToString("dd.MM.YYYY HH:mm"),
        MimeType = file.MimeType,
        IsImage = file.MimeType.StartsWith("image"),
        IsPublished = isPublished(file.Id),
        Label = file.Parent == null ? "" : file.Parent.Name,
        Labels = file.Parent == null ? null : file.Parent.Parent.Children.Select(c => c.Name).ToList(),
        Volume = file.TypesOfResource.ToString()
      });

      var res = resContent
        .Union(resProject)
        .ToList()
        .Select(getFiles)
        .OrderBy(r => r.Volume)
        .ThenBy(r => r.Label)
        .ThenBy(r => r.Name)
        .ToList();

      return Request.CreateResponse((HttpStatusCode)200, res);
    }

    [HttpPost]
    [Route("overview/{id:int}/files")]
    public HttpResponseMessage AddFile(int id, Object upload) {
      return Request.CreateResponse((HttpStatusCode)200, "");
    }

    [HttpDelete]
    [Route("overview/{id:int}/files")]
    public HttpResponseMessage DeleteFile(int id) {
      return Request.CreateResponse((HttpStatusCode)200, "");
    }

    [HttpGet]
    [Route("overview/{id:int}/image")]
    public HttpResponseMessage GetImage(int id) {
      var file = _projectUnitOfWork.GetResource(id);

      using (var blob = BlobFactory.GetBlobStorage(file.ResourceId, BlobFactory.Container.Resources)) {
        var res = new ImageDto();
        using (var s = new MemoryStream(blob.Content)) {
          using (var img = Image.FromStream(s)) {
            res.Src = String.Format("/tools/getImg/{0}?c=ResourceFile&res=500x300", file.Id);
            res.Width = img.Width;
            res.Height = img.Height;
            res.ResH = img.HorizontalResolution;
            res.ResV = img.VerticalResolution;
            res.Warn = img.VerticalResolution < 300F ? "Resolution may not satisfy print" : "No, looks perfect";
            res.Px = Enum.GetName(typeof (PixelFormat), img.PixelFormat);
          }
        }
        return Request.CreateResponse((HttpStatusCode) 200, res);
      }
    }

    [HttpPut]
    [Route("overview/{id:int}/file")]
    public HttpResponseMessage UpdateFile(FileDto file) {
      var res = _resourceUnitOfWork.GetFile(file.Id);
      res.Name = file.Name;
      res.TypesOfResource = (TypeOfResource)Enum.Parse(typeof(TypeOfResource), file.Volume, true);
      // TODO: Label (change Parent)
      var result = _resourceUnitOfWork.UpdateResource(res);
      return Request.CreateResponse((HttpStatusCode)200, result);
    }

    # endregion

    }
}