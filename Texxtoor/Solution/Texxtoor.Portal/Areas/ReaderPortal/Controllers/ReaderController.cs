using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Extensions.ActionResults;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Functions;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Common;

namespace Texxtoor.Portal.Areas.ReaderPortal.Controllers {

  public class ReaderController : ControllerExt {

    # region -== Common ==-

    /// <summary>
    /// Index View, as we come from search, it's always published id (public book, the one available for free to everybody).
    /// </summary>
    /// <param name="id">publishedId</param>
    /// <returns></returns>
    [Authorize]
    public ActionResult Published(int id) {
      var published = UnitOfWork<ReaderManager>().GetPublishedById(id);
      ViewBag.IsMain = true;
      return View("Published", published);
    }

    [Authorize]
    public ActionResult PublishedDetails(int id, bool? main) {
      var published = UnitOfWork<ReaderManager>().GetPublishedById(id, p => p.ResourceFiles, p => p.Reviews);
      Func<ContributorRole, string> localizeRole = (cr) => {
        var attr = typeof (ContributorRole).GetField(cr.ToString()).GetCustomAttributes(typeof (DisplayAttribute), true).OfType<DisplayAttribute>().SingleOrDefault();
        if (attr != null) {
          return attr.GetName();
        }
        return cr.ToString();
      };
      published.AuthorInformation =
        String.Join(", ",
        published.Authors
        .Select(a => {
          UnitOfWork<ReaderManager>().AsUnitOfWork().LoadProperty(a.Profile, user => user.ContributorMatrix);
          var roles = String.Join(", ", a.Profile.ContributorMatrix.Select(c => String.Format("{0} [{1}]", localizeRole(c.ContributorRole), c.LocaleIdLocalName)));
          var names = String.Format("{0} {1} aka {3} ({2})", a.Profile.FirstName, a.Profile.LastName, roles, a.UserName);
          return names;
        }).ToArray());
      published.AuthorProfiles = published.Authors.Where(a => !String.IsNullOrEmpty(a.Profile.Application)).Select(a => a.Profile).ToList();
      ViewBag.IsMain = main.GetValueOrDefault();
      return View("Published/_Details", published);
    }

    [Authorize]
    public FileResult GetResourceFile(int id) {
      var res = UnitOfWork<ProjectManager>().GetResource(id) as ResourceFile;
      if (res == null) return null;
      using (var blob = BlobFactory.GetBlobStorage(res.ResourceId, BlobFactory.Container.Resources)) {
        return blob.Content == null ? null : File(blob.Content, res.MimeType, res.FullName);
      }
    }

    /// <summary>
    /// Popup with all reader functions (id is for a work)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    public ActionResult ReaderApp(int id) {
      // create an artifact session to let the reader operate on the service
      var currentSessionId = ReaderManager.Instance.CreateReaderServiceSession(UserName);
      ViewBag.Ssid = currentSessionId;
      ViewBag.BaseUrl = Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port + "/ServiceApi/Services/ReaderService.svc/json/";
      ReaderManager.Instance.RegisterSales(id);
      
      return View(id);
    }

    /// <summary>
    /// A quick preview of a single frozen fragment
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    public ActionResult MiniReader(int id) {
      var fragment = UnitOfWork<ReaderManager>().GetFrozenFragment(id);
      return PartialView("Library/Collections/_MiniReader", fragment);
    }

    public ImageResult MiniReaderImg(string href) {
      var data = UnitOfWork<ReaderManager>().GetFrozenFragmentHref(href);
      return new ImageResult(data);
    }

    [Authorize]
    public ActionResult ReaderAppForPublished(int id) {
      // this is the call from published, we create a work first, put in into the library, and call the reader, then
      var work = UnitOfWork<ReaderManager>().GetWorkForPublished(id, UserName);
      return RedirectToAction("ReaderApp", "Reader", new { id = work.Id });
    }

    public ActionResult PutIntoLibrary(int id) {
      UnitOfWork<ReaderManager>().GetWorkForPublished(id, UserName);
      return RedirectToAction("Library", "Reader");
    }

    public JsonResult TableOfContentPublished(int id) {
      var tree = ReaderManager.Instance.GetFragmentTreeForPublished(id, 2 /* just chapters */);
      return Json(tree, JsonRequestBehavior.AllowGet);
    }

    public JsonResult TableOfContentWork(int id) {
      var tree = ReaderManager.Instance.GetFragmentTreeForWork(id, 2 /* just chapters */);
      return Json(tree, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// Rate published works
    /// </summary>
    /// <param name="id">Published Id</param>
    /// <param name="v"></param>
    /// <returns></returns>
    public JsonResult RateContent(int id, int v) {
      UnitOfWork<ReaderManager>().SetRating(id, v, UserName);
      return Json(new { msg = "Thanks for rating this content." }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult RateContentStars(int id) {
      var publ = UnitOfWork<ReaderManager>().GetPublishedById(id);
      return PartialView("Published/_RatingStars", publ.Starred);
    }

    # endregion -== Common ==-

    # region -== Fragment Handling ==-

    [Authorize]
    public ActionResult ChangeCollection(int id, int target) {
      var source = ReaderManager.Instance.GetWork(id, UserName);
      var trgt = ReaderManager.Instance.GetWork(target, UserName);
      return PartialView("Library/Collections/_ChangeCollection", new Tuple<Work, Work>(source, trgt));
    }

    [Authorize]
    public ActionResult GetWorkCollection(int id) {
      var work = ReaderManager.Instance.GetWork(id, UserName);
      return PartialView("Library/Collections/_WorkCollection", work);
    }

    [HttpPost]
    public JsonResult AssignFragmentToWork(int id, int[] fragmentIds) {
      try {
        // we need the source work, because we need to know the fragments origin (pull either from FrozenFragments or WorkingFragments)
        ReaderManager.Instance.AssignFragmentsToWork(id, fragmentIds.ToList(), UserName);
        return Json(new { msg = "Success" });
      } catch (Exception ex) {
        return Json(new { msg = "Error" + ex.Message });
      }
    }

    /// <summary>
    /// Show all works of current user as source of fragment selection
    /// </summary>
    /// <returns></returns>
    [Authorize]
    public ActionResult SelectWork() {
      var works = UnitOfWork<ReaderManager>().GetWorksForUser(UserName)
        .OrderBy(b => b.Name);
      return PartialView("Library/Collections/_SelectWork", works);
    }

    # endregion -== Fragment Handling ==-

    # region -== Library Handling ==-

    /// <summary>
    /// Work view
    /// </summary>
    /// <returns></returns>
    [Authorize]
    public ActionResult Library() {
      ViewBag.IsMain = false;
      return View();
    }

    [Authorize]
    public ActionResult WorkDetails(int id) {
      var work = UnitOfWork<ReaderManager>().GetWork(id);
      return PartialView("Library/Works/_Details", work);
    }

    /// <summary>
    /// Create a full copy and decouple from Published
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    public ActionResult CopyWork(int id) {
      ReaderManager.Instance.CopyWork(id, UserName);
      return RedirectToAction("Library");
    }

    /// <summary>
    /// All private books (work)
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    [Authorize]
    public ActionResult ListLibraryWorks(PaginationFormModel p) {
      var works = UnitOfWork<ReaderManager>().GetWorksForUser(UserName);
      ViewBag.HasOrders = OrderManager.Instance.GetOrdersForWorks(works, UserName).GroupBy(w => w).ToDictionary(w => w.Key.Key.Id, o => o.Count());
      return PartialView("Library/Works/_List", works.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    [Authorize]
    public ActionResult AddWork() {
      return PartialView("Library/Works/_Add");
    }

    [HttpPost]
    [Authorize]
    public JsonResult AddWork(Work newWork) {
      if (!String.IsNullOrEmpty(newWork.Name)) {
        var work = ReaderManager.Instance.AddWork(newWork, UserName);
        return Json(new { msg = String.Format(ControllerResources.ReaderController_EditWork_Work_changed, work.Name) });
      }
      return Json(new { msg = "Error" });
    }

    [Authorize]
    public ActionResult EditWork(int id) {
      var work = ReaderManager.Instance.GetWork(id, UserName);
      return PartialView("Library/Works/_Edit", work);
    }

    [HttpPost]
    [Authorize]
    public JsonResult EditWork(Work newWork) {
      if (!String.IsNullOrEmpty(newWork.Name)) {
        var work = ReaderManager.Instance.EditWork(newWork, UserName);
        return Json(new { msg = String.Format(ControllerResources.ReaderController_EditWork_Work_changed, work.Name) });
      }
      return Json(new { msg = "Error" });
    }

    [HttpPost]
    public ActionResult DeleteWork(int id) {
      var name = ReaderManager.Instance.DeleteBookFromLibrary(id, UserName);
      if (!String.IsNullOrEmpty(name)) {
        return Json(new { msg = String.Format(ControllerResources.ReaderController_DeleteWork_Work_deleted, name) });
      }
      return Json(new { msg = "Cannot delete. Probably pending orders associated." });
    }

    // Let user upload private work to work table
    public FileUploadJsonResult AjaxBookUpload(HttpPostedFileBase file) {
      var extension = Path.GetExtension(file.FileName);
      var mimeType = MimeTypeHelper.GetFromExtension(extension);
      var fileName = Path.GetFileName(file.FileName);
      if (mimeType.Contains("epub") || extension == ".epub") {
        try {
          using (var ms = new MemoryStream()) {
            file.InputStream.CopyTo(ms);
            ms.Position = 0;
            ReaderManager.Instance.AddWorkFromEPub(UserName, ms);
          }
          // Return JSON             
          return new FileUploadJsonResult { Data = new { msg = String.Format("{0} uploaded successfully.", fileName) } };
        } catch (Exception ex) {
          // Return JSON             
          return new FileUploadJsonResult { Data = new { msg = ex.Message } };
        }
      }
      return new FileUploadJsonResult { Data = new { msg = "You're not allowed to upload a file of type " + mimeType + " here." } };
    }

    # endregion -== Library Handling ==-

    # region -== Comment Handling ==-

    /*
     * Grouping of Subject assumes that we use unique strings here:
     *  - COMM:subject    Regular comment with no other meaning and optional subject text
     *  - NOTE:#lime      Tagged section with specific dialog color (Note) ???
     *  - PUBL:lname      Self defined list of names for public notes (list is shared among all works)
     *  - PRIV:lname      Self defined list of names for private notes (list is exclusive for this work)
    */

    [Authorize]
    public ActionResult Comments(int id) {
      var hasComments = ReaderManager.Instance.BookHasComments(id, UserName);
      var work = ReaderManager.Instance.GetWorkWithProducts(id, UserName);
      ViewBag.WorkName = work.Name;
      ViewBag.WorkId = id;
      return View(hasComments);
    }

    public ActionResult ListComments(int id) {
      var groupedComments = ReaderManager.Instance.GetAllComments(id, UserName);
      return PartialView("Comments/_List", groupedComments);
    }

    [HttpPost]
    public JsonResult DeleteComments(List<int> ids) {
      var result = ReaderManager.Instance.DeleteComments(ids, UserName);
      return Json(new { msg = String.Format(ControllerResources.ReaderController_DeleteComments__0__comments_deleted, result) });
    }

    public JsonResult DeleteComment(int id) {
      var result = ReaderManager.Instance.DeleteComments(new[] { id }.ToList(), UserName);
      return Json(new { msg = String.Format(ControllerResources.ReaderController_DeleteComment__0__comment_deleted, result) }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult PrintComments(int id) {
      // id is workId
      var comments = ReaderManager.Instance.GetAllComments(id, UserName);
      ViewBag.WorkId = id;
      return View("Comments/_Print", comments);
    }

    // get thread of current fragment
    public JsonResult ThreadNavigation(string fragmentName) {
      var comm = ReaderManager.Instance.GetComments(fragmentName, UserName);
      JsTreeModel[] tree;
      if (comm != null) {
        tree = GetCommentTreeModel(comm.ToList());
        tree = new[] { new JsTreeModel { attr = new JsTreeAttribute { id = "0", rel = "folder" }, children = tree, data = "Comments" } };
      } else {
        tree = new[] { new JsTreeModel { attr = new JsTreeAttribute { id = "0", rel = "folder" }, children = null, data = "No Comments" } };
      }
      return Json(tree, JsonRequestBehavior.AllowGet);
    }

    private static JsTreeModel[] GetCommentTreeModel(IEnumerable<Comment> comments) {
      if (comments == null) return null;
      return comments.Select(n => new JsTreeModel {
        data = n.Name.Ellipsis(50).ToHtmlString(),
        attr = new JsTreeAttribute {
          id = n.Id.ToString(CultureInfo.InvariantCulture),
          rel = (n.Children != null && n.Children.Any()) ? "folder" : "file"
        },
        children = GetCommentTreeModel(n.Children)
      }).ToArray();
    }

    [HttpPost]
    public ActionResult OpenThread(string fragmentName) {
      var comments = ReaderManager.Instance.GetComments(fragmentName, UserName);
      return PartialView("Social/_EditThread", comments);
    }

    [HttpPost]
    public JsonResult AddComment(Comment newComment, int? parentCommentGroup) {
      ReaderManager.Instance.AddComment(newComment, parentCommentGroup, UserName);
      return Json(new {
        message = "Comment saved"
      });
    }

    [HttpPost]
    public JsonResult EditComment(Comment comment) {
      ReaderManager.Instance.EditComment(comment, UserName);
      return Json(new {
        message = "Comment saved"
      });
    }

    # endregion

    # region -== Bookmarks Handling ==-

    [Authorize]
    public ActionResult Bookmarks(int id) {
      var work = ReaderManager.Instance.GetWork(id, UserName);
      return View(work);
    }

    [Authorize]
    public ActionResult ListBookmarks(int id, int? page) {
      var work = ReaderManager.Instance.GetWork(id, UserName);
      var bookMarks = ReaderManager.Instance.GetBookmarksForWork(id, UserName);
      ViewBag.WorkName = work.Name;
      if (!page.HasValue) {
        page = 0;
      }
      return PartialView("Bookmarks/_List", new PagedList<Bookmark>(bookMarks.AsQueryable(), page.Value, 10));
    }

    [Authorize]
    public JsonResult DeleteBookmarks(int[] ids) {
      var cnt = ids.Sum(id => UnitOfWork<ReaderManager>().DeleteBookmark(id, UserName) ? 1 : 0);
      return Json(new {msg = cnt.ToString()});
    }

    # endregion

    # region -== Messages ==-

    /// <summary>
    /// Publis board for a published text.
    /// </summary>
    /// <param name="id">Published Id</param>
    /// <returns></returns>
    [Authorize]
    public ActionResult MessageBoard(int id) {
      var published = ReaderManager.Instance.GetPublishedById(id);
      return View(published);
    }

    [Authorize]
    public ActionResult CreateMessage(string name) {
      var msg = new WorkChat { Name = name };
      return View("MessageBoard/_CreateMessage", msg);
    }

    [ValidateInput(false)]
    [HttpPost]
    public JsonResult CreateMessage(WorkChat msg, int publishedId, int? parentId) {
      ReaderManager.Instance.CreateMessage(msg, publishedId, parentId, UserName);
      return Json(new { data = "OK" });
    }


    public ActionResult TopMessage(int id, PaginationFormModel p) {
      var publ = ReaderManager.Instance.GetPublishedById(id);
      var work = ReaderManager.Instance.GetWorkForPublished(publ.Id, UserName);
      if (work == null) return View("MessageBoard/_TopMessage", null);
      // retrieve to level only, we ask for children recursively
      var msg = ReaderManager.Instance.GetTopMessages(work.Id).AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir);
      ViewBag.PublishedId = id;
      return View("MessageBoard/_TopMessage", msg);
    }

    public ActionResult ChildMessage(IEnumerable<WorkChat> msg) {
      return PartialView("MessageBoard/_ChildMessage", msg);
    }

    # endregion

    # region -= Reviewer =-

    public ActionResult Review(int id) {
      var publ = ReaderManager.Instance.GetPublishedById(id);
      return View(publ);
    }

    public ActionResult ListReviews(int id, PaginationFormModel p) {
      var revs = ProjectManager.Instance.GetReviewsForPublished<ReaderReview>(id).ToList();
      return View("Reviews/_ListReviews", revs.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult AddReview(int id) {
      var publ = ReaderManager.Instance.GetPublishedById(id);
      var pr = new ReaderReview { PublishedWork = publ };
      return PartialView("Reviews/_AddReview", pr);
    }

    [HttpPost]
    public JsonResult AddReview(int id, ReaderReview review) {
      ProjectManager.Instance.AddReviewForPublished(id, review, UserName);
      return Json(new { msg = ControllerResources.OpusController_Review_Added });
    }

    public ActionResult EditReview(int id) {
      var review = ProjectManager.Instance.GetReview<PeerReview>(id);
      return PartialView("Reviews/_EditReview", review);
    }

    [HttpPost]
    public JsonResult EditReview(int id, ReaderReview review) {
      ProjectManager.Instance.EditReview(id, review);
      return Json(new { msg = ControllerResources.OpusController_Review_Changed });
    }

    [HttpPost]
    public JsonResult DeleteReview(int id) {
      ProjectManager.Instance.DeleteReview(id, UserName);
      return Json(new { msg = ControllerResources.OpusController_Review_Deleted });
    }

    # endregion -= Reviewer Helper =-

  }
}