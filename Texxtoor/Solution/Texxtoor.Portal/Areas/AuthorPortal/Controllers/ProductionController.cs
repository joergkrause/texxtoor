using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Common;

namespace Texxtoor.Portal.Areas.AuthorPortal.Controllers {

  [Authorize]
  public class ProductionController : ControllerExt {

    # region HTML

    public FileResult GetHtml(Guid guid, string name) {
      using (var blob = BlobFactory.GetBlobStorage(guid, BlobFactory.Container.ProductionPreviews)) {
        return File(blob.Content, "text/html", name + ".html");
      }
    }

    # endregion

    # region Manual Production
    // immediate production without going through the order cycle

    public ActionResult Produce() {
      return View();
    }

    public ActionResult ListProjectsToProduce(PaginationFormModel p, bool deactivated = false) {
      var projects = UnitOfWork<ProjectManager>().GetUsersProjectsWithMembers(UserName, !deactivated).ToList().OrderByDescending(pr => pr.CreatedAt);
      ViewBag.TeamLead = UnitOfWork<ProjectManager>().GetProjectsTeamLeader(projects);
      ViewBag.CurrentUser = Manager<UserManager>.Instance.GetUserByName(UserName);
      ViewBag.ForDeactivated = deactivated;
      return PartialView("_List", projects.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult PdfProduction(int id) {
      var doc = ProjectManager.Instance.GetOpus(id, UserName);
      var templates = ProjectManager.Instance.GetTemplatesForTenant(doc).ToList();
      ViewBag.ProjectId = doc.Project.Id;
      ViewBag.OpusId = id;
      ViewBag.OpusLang = doc.LocaleId;
      ViewBag.UserLang = CurrentCulture;
      return View("PdfProduction", templates);
    }

    public ActionResult EpubProduction(int id) {
      var doc = ProjectManager.Instance.GetOpus(id, UserName);
      var templates = ProjectManager.Instance.GetTemplatesForTenant(doc).ToList();
      ViewBag.ProjectId = doc.Project.Id;
      ViewBag.OpusId = id;
      ViewBag.OpusLang = doc.LocaleId;
      ViewBag.UserLang = CurrentCulture;
      return View("EpubProduction", templates);
    }

    public ActionResult AppProduction(int id) {
      var doc = ProjectManager.Instance.GetOpus(id, UserName);
      var templates = ProjectManager.Instance.GetTemplatesForTenant(doc).ToList();
      ViewBag.ProjectId = doc.Project.Id;
      ViewBag.OpusId = id;
      ViewBag.OpusLang = doc.LocaleId;
      ViewBag.UserLang = CurrentCulture;
      return View("AppProduction", templates);
    }

    [HttpPost]
    public ActionResult ProduceMedia(int id, GroupKind type, int templateGroupId) {
      try {
        var prj = UnitOfWork<ProjectManager>();
        var prm = UnitOfWork<ProductionManager>();
        var opus = prj.GetOpus(id, UserName);
        var user = UnitOfWork<UserManager>().GetUserByName(UserName);
        var locale = opus.LocaleId;
        var p = prm.CreatePrintable(opus, user, templateGroupId);
        byte[] file = null;
        switch (type) {
          case GroupKind.Pdf:
            file = prm.CreatePdfContent(p);
            break;
          case GroupKind.Epub:
            file = prm.CreateEpub(p);
            break;
          case GroupKind.App:
            return new HttpNotFoundResult("App Production is currently not available.");
        }
        if (file != null) {
          var fileName = String.Format("{0}.{1}", opus.Name, type);
          prm.StoreMediaFile(fileName, type, file, UserName);
          return Json(new { msg = String.Format(ControllerResources.ProductionController_ProduceMedia_The_file__0__has_been_created, fileName) });
        }
      } catch (Exception ex) {
        return new HttpNotFoundResult(ex.Message);
      }
      return new HttpNotFoundResult(ControllerResources.ProductionController_ProduceMedia_An_error_occured_while_creating_the_file);
    }

    # endregion

  }
}
