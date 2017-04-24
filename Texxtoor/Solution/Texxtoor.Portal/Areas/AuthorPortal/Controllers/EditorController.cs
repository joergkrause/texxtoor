using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.Portal.ServiceApi.Services;
using Texxtoor.ViewModels.Common;
using Texxtoor.ViewModels.Editor;

namespace Texxtoor.Portal.Areas.AuthorPortal.Controllers {

  [Authorize]
  public class EditorController : ControllerExt {

    # region -= AuthorRoom =-

    public ActionResult AuthorRoom(int id, int? chapterId) {
      var opus = ProjectManager.Instance.GetOpus(id, UserName);
      // build all chapter, assume that first level are chapters
      var rms =
        opus.Children.OrderBy(e => e.OrderNr)
            .Select(
              chapter =>
              EditorManager.Instance.GetChapterModelForNewChapter(opus, chapter.Id,
                                                                  (s) => EditorManager.Instance.InsertElement(s)))
            .ToList();
      ViewBag.WorkspaceName = opus.Name;
      ViewBag.CreatedAtDate = String.Format("{0} {1}", DateTime.Now.ToShortDateString(),
                                            DateTime.Now.ToShortTimeString());
      ViewBag.BaseUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, "/Editor/");
      if (rms.Count > 1) {
        //Get all chapters with other nodes
        var rm = EditorManager.Instance.GetChapterModelForNewChapter(opus, chapterId, null);
        rm.CurrentElement = rm.ChapterElements.First();
        return View(rm);
      } else {
        // Get single chapter with other nodes
        var rm = chapterId.HasValue ? rms.First(r => r.CurrentChapter.Id == chapterId.Value) : rms.FirstOrDefault();
        rm.CurrentElement = rm.ChapterElements.First();
        return View(rm);
      }
    }

    public ActionResult ProofRoom(int id, int? chapterId) {
      var opus = ProjectManager.Instance.GetOpus(id, UserName);
      // build all chapter, assume that first level are chapters
      var rms =
        opus.Children.OrderBy(e => e.OrderNr)
            .Select(
              chapter =>
              EditorManager.Instance.GetChapterModelForNewChapter(opus, chapter.Id,
                                                                  (s) => EditorManager.Instance.InsertElement(s)))
            .ToList();
      ViewBag.WorkspaceName = opus.Name;
      ViewBag.CreatedAtDate = String.Format("{0} {1}", DateTime.Now.ToShortDateString(),
                                            DateTime.Now.ToShortTimeString());
      ViewBag.BaseUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, "/Editor/");
      if (rms.Count > 1) {
        //Get all chapters with other nodes
        var rm = EditorManager.Instance.GetChapterModelForNewChapter(opus, chapterId, null);
        rm.CurrentElement = rm.ChapterElements.First();
        return View(rm);
      } else {
        // Get single chapter with other nodes
        var rm = chapterId.HasValue ? rms.First(r => r.CurrentChapter.Id == chapterId.Value) : rms.FirstOrDefault();
        rm.CurrentElement = rm.ChapterElements.First();
        return View(rm);
      }
    }

    # endregion

    # region Common Functions for all Editors

    public ContentResult WordCount(int id) {
      return Content(UnitOfWork<EditorManager>().GetWordCount(id));

    }

    public ContentResult CharacterCount(int id) {
      return Content(UnitOfWork<EditorManager>().GetCharacterCount(id));
    }

    public ActionResult GetThumbnail(int id, int w, int h, string m) {
      var res = ProjectManager.Instance.GetResource(id) as ResourceFile;
      if (res != null) {
        using (var blob = BlobFactory.GetBlobStorage(res.ResourceId, BlobFactory.Container.Resources)) {
          switch (res.MimeType) {
            case "image/svg+xml":
              // for SVG we don't need to scale, it's a vector anyway
              return Content(Encoding.ASCII.GetString(blob.Content), res.MimeType);
            default:
              return File(UnitOfWork<EditorManager>().GetThumbnailImage(blob.Content, w, h), res.MimeType);
          }
        }
      }
      return null; // nothing in case of error TODO: consider default image
    }

    public FileResult DownloadCopy(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      var ms = UnitOfWork<ProjectManager>().CreateBackup(opus);
      return File(ms.ToArray(), "text/xml", opus.Name + ".xml");
    }

    # endregion

    # region -= Reviewer =-

    public ActionResult PeerReviewerRoom(int id) {
      var opus = ProjectManager.Instance.GetOpus(id, UserName);
      return View(opus);
    }

    public ActionResult ListReviews(int id, PaginationFormModel p) {
      var revs = ProjectManager.Instance.GetReviewsForOpus(id);
      ViewBag.UserIsTeamLead = ProjectManager.Instance.UserIsTeamLead(id, UserName);
      return PartialView("Reviews/_ListReviews", revs.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult AddReview(int id) {
      var opus = ProjectManager.Instance.GetOpus(id, UserName);
      var pr = new PeerReview();
      pr.PublishedWork = ProjectManager.Instance.GetPublished(opus.Published.Id);
      return PartialView("Reviews/_AddReview", pr);
    }

    /// <summary>
    /// Save form data
    /// </summary>
    /// <param name="id">Published Id</param>
    /// <param name="review">Form data</param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult AddReview(int id, int publishedId, PeerReview review) {
      try {
        var opus = ProjectManager.Instance.GetOpus(id, UserName);
        if (opus.Published.Id != publishedId) {
          throw new ArgumentOutOfRangeException("Committed form sent some wrong values");
        }
        ProjectManager.Instance.AddReviewForPublished(publishedId, review, UserName);
        return Json(new { msg = ControllerResources.OpusController_Review_Added });
      } catch (Exception ex) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
      }
    }

    public ActionResult EditReview(int id) {
      var review = ProjectManager.Instance.GetReview<PeerReview>(id);
      return PartialView("Reviews/_EditReview", review);
    }

    [HttpPost]
    public JsonResult EditReview(int id, PeerReview review) {
      ProjectManager.Instance.EditReview(id, review);
      return Json(new { msg = ControllerResources.OpusController_Review_Changed });
    }

    public ActionResult ApproveReview(int id) {
      var review = ProjectManager.Instance.GetReview<PeerReview>(id);
      return PartialView("Reviews/_ApproveReview", review);
    }

    [HttpPost]
    public JsonResult ApproveReview(int id, bool approved) {
      ProjectManager.Instance.ApproveReview(id, approved, UserName);
      return Json(new { msg = ControllerResources.OpusController_Review_Changed });
    }

    [HttpPost]
    public JsonResult DeleteReview(int id) {
      ProjectManager.Instance.DeleteReview(id, UserName);
      return Json(new { msg = ControllerResources.OpusController_Review_Deleted });
    }

    # endregion -= Reviewer Helper =-

    # region Upload Service

    public JsonResult UploadImage(string caption, int docId, int chapId, int? snipId, HttpPostedFileBase file) {
      var opus = ProjectManager.Instance.GetOpus(docId, UserName);
      var user = Manager<UserManager>.Instance.GetUserByName(UserName);
      var selectedImageId = 0;
      if (file.InputStream != null) {
        // Force converting all images into PNG for lazy content management
        var img = Image.FromStream(file.InputStream);
        var saveToMs = new MemoryStream();
        img.Save(saveToMs, ImageFormat.Png);
        // Write down to blob storage
        var resId = Guid.NewGuid();
        // Save reference in database
        var res = new ResourceFile {
          Name = caption ?? Path.GetFileNameWithoutExtension(file.FileName),
          Project = opus.Project,
          Owner = user,
          ResourceId = resId,
          TypesOfResource = TypeOfResource.Content,
          MimeType = "image/png"
        };
        using (var blob = BlobFactory.GetBlobStorage(resId, BlobFactory.Container.Resources)) {
          blob.Content = saveToMs.ToArray();
          blob.Save(() => {
            ResourceManager.Instance.AddResource(res);
            selectedImageId = ResourceManager.Instance.GetFile(res.ResourceId).Id;
          });
        }
        dynamic innerResponse;
        // can place and upload and storage was successfull
        if (snipId.HasValue && selectedImageId != 0) {
          // call from editor inserts in opus
          innerResponse = new EditorService().InsertSnippet(docId, chapId, snipId.Value, "img", String.Empty, selectedImageId.ToString());
        } else {
          innerResponse = JsonConvert.SerializeObject(new { id = selectedImageId });
        }
        var ct = !Request.Browser.Type.ToUpper().Contains("IE") ? "application/json" : "text/plain";
        return Json(innerResponse, ct);
      } else {
        return Json(new { error = "No Bytes" });
      }
    }


    # endregion

    # region -= Translator Room =-

    public ActionResult TranslatorRoom(int id, int? chapterId) {
      var opus = ProjectManager.Instance.GetOpus(id, UserName);
      // build all chapter, assume that first level are chapters
      var rms =
        opus.Children.OrderBy(e => e.OrderNr)
            .Select(
              chapter =>
              EditorManager.Instance.GetChapterModelForNewChapter(opus, chapter.Id,
                                                                  (s) => EditorManager.Instance.InsertElement(s)))
            .ToList();
      ViewBag.WorkspaceName = opus.Name;
      ViewBag.CreatedAtDate = String.Format("{0} {1}", DateTime.Now.ToShortDateString(),
                                            DateTime.Now.ToShortTimeString());
      ViewBag.BaseUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, "/Editor/");
      if (rms.Count > 1) {
        //Get all chapters with other nodes
        var rm = EditorManager.Instance.GetChapterModelForNewChapter(opus, chapterId, null);
        rm.CurrentElement = rm.ChapterElements.First();
        return View(rm);
      } else {
        // Get single chapter with other nodes
        var rm = chapterId.HasValue ? rms.First(r => r.CurrentChapter.Id == chapterId.Value) : rms.FirstOrDefault();
        rm.CurrentElement = rm.ChapterElements.First();
        return View(rm);
      }
    }

    # endregion

    # region -= Editor Room =-

    public ActionResult EditorRoom(int id, int? chapterId) {
      var opus = ProjectManager.Instance.GetOpus(id, UserName);
      // build all chapter, assume that first level are chapters
      var rms =
        opus.Children.OrderBy(e => e.OrderNr)
            .Select(
              chapter =>
              EditorManager.Instance.GetChapterModelForNewChapter(opus, chapter.Id,
                                                                  (s) => EditorManager.Instance.InsertElement(s)))
            .ToList();
      ViewBag.WorkspaceName = opus.Name;
      ViewBag.CreatedAtDate = String.Format("{0} {1}", DateTime.Now.ToShortDateString(),
                                            DateTime.Now.ToShortTimeString());
      ViewBag.BaseUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, "/Editor/");
      if (rms.Count > 1) {
        //Get all chapters with other nodes
        var rm = EditorManager.Instance.GetChapterModelForNewChapter(opus, chapterId, null);
        rm.CurrentElement = rm.ChapterElements.First();
        return View(rm);
      } else {
        // Get single chapter with other nodes
        var rm = chapterId.HasValue ? rms.First(r => r.CurrentChapter.Id == chapterId.Value) : rms.FirstOrDefault();
        rm.CurrentElement = rm.ChapterElements.First();
        return View(rm);
      }
    }

    # endregion

    # region -= Designer Room =-

    public ActionResult DesignerRoom(int id, int? opusId, int? resourceId) {
      var sdm = new SnippetDataModel {
        Referer = Request.UrlReferrer != null ? Request.UrlReferrer.AbsolutePath : String.Empty,
        DocumentId = opusId.GetValueOrDefault(),  // just to return to caller
        ProjectId = id,
        ResourceId = resourceId.GetValueOrDefault() // 0 = not associated with an active element, editor will save SVG to new file
      };
      return View(sdm);
    }

    [HttpPost]
    public JsonResult SaveSvg(int? id, int projectId, string svg, string filename)
    {
      int newId = 0;
      if (id.HasValue)
      {
        ResourceManager.Instance.SaveResource(id.Value, filename, svg, UserName);
      }
      else
      {
        byte[] content = Encoding.ASCII.GetBytes(svg);
        newId = ResourceManager.Instance.AddResource(projectId, TypeOfResource.Content, "SVG", filename, "image/svg+xml",
          content, UserName);
      }
      return Json(new
      {
        resourceId = newId
      });
    }

    [HttpPost]
    public JsonResult SaveImage(int? id, string svg, string filename) {
      return null;
    }

    /// <summary>
    /// This supports the library vie in SVG editor. The content appears in an iframe and hence we provide HTML.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public ActionResult ProjectLibrary(int id, string type) {
      var ribbonImages = EditorManager.Instance.GetRibbonImagesListFromProject(id, 100, 100);
      Func<RibbonImages, bool> clause;
      // we filter the list of images here because the caller must treat SVG different from IMG 
      switch (type) {
        case "img":
          clause = r => r.folder != "SVG Drawings" && r.folder != "SVG Exports";  // strings are consts used in Save methods
          break;
        case "svg":
          clause = r => r.folder == "SVG Drawings";
          break;
        default:
          clause = r => true;
          break;
      }
      var filteredList = ribbonImages.Where(clause).ToList();
      return View("Designer/ProjectLibrary", filteredList);
    }

    # endregion

  }
}
