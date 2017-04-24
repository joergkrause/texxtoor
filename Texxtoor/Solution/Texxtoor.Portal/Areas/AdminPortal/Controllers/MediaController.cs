using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Cms;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {

  [Authorize(Roles = "Admin")]
  public class MediaController : ControllerExt {

    //
    // GET: /CmsAdmin/Media/

    public ViewResult Index() {
      return View(AdminDb.Media.ToList());
    }

    //
    // GET: /CmsAdmin/Media/Details/5

    public ViewResult Details(int id) {
      CmsMedia media = AdminDb.Media.Find(id);
      return View(media);
    }

    //
    // GET: /CmsAdmin/Media/Create

    public ActionResult Create() {
      return View();
    }

    //
    // POST: /CmsAdmin/Media/Create

    [HttpPost]
    public ActionResult Create(CmsMedia media) {
      if (ModelState.IsValid) {
        AdminDb.Media.Add(media);
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(media);
    }

    //
    // GET: /CmsAdmin/Media/Edit/5

    public ActionResult Edit(int id) {
      CmsMedia media = AdminDb.Media.Find(id);
      return View(media);
    }

    //
    // POST: /CmsAdmin/Media/Edit/5

    [HttpPost]
    public ActionResult Edit(CmsMedia media) {
      if (ModelState.IsValid) {
        AdminDb.Entry(media).State = EntityState.Modified;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(media);
    }

    //
    // GET: /CmsAdmin/Media/Delete/5

    public ActionResult Delete(int id) {
      CmsMedia media = AdminDb.Media.Find(id);
      return View(media);
    }

    //
    // POST: /CmsAdmin/Media/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id) {
      CmsMedia media = AdminDb.Media.Find(id);
      AdminDb.Media.Remove(media);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }

  }
}