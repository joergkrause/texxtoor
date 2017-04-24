using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Cms;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {
  
  [Authorize(Roles = "Admin,CmsAdmin")]
  public class PageController : ControllerExt {

    //
    // GET: /CmsAdmin/Page/

    public ViewResult Index() {
      return View(AdminDb.Pages.ToList());
    }

    //
    // GET: /CmsAdmin/Page/Details/5

    public ViewResult Details(int id) {
      CmsPage cmspage = AdminDb.Pages.Find(id);
      return View(cmspage);
    }

    //
    // GET: /CmsAdmin/Page/Create

    public ActionResult Create() {
      return View();
    }

    //
    // POST: /CmsAdmin/Page/Create

    [HttpPost]
    public ActionResult Create(CmsPage cmspage) {
      if (ModelState.IsValid) {
        AdminDb.Pages.Add(cmspage);
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(cmspage);
    }

    //
    // GET: /CmsAdmin/Page/Edit/5

    public ActionResult Edit(int id) {
      CmsPage cmspage = AdminDb.Pages.Find(id);
      return View(cmspage);
    }

    //
    // POST: /CmsAdmin/Page/Edit/5

    [HttpPost]
    public ActionResult Edit(CmsPage cmspage) {
      if (ModelState.IsValid) {
        AdminDb.Entry(cmspage).State = EntityState.Modified;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(cmspage);
    }

    //
    // GET: /CmsAdmin/Page/Delete/5

    public ActionResult Delete(int id) {
      CmsPage cmspage = AdminDb.Pages.Find(id);
      return View(cmspage);
    }

    //
    // POST: /CmsAdmin/Page/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id) {
      CmsPage cmspage = AdminDb.Pages.Find(id);
      AdminDb.Pages.Remove(cmspage);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }

  }
}