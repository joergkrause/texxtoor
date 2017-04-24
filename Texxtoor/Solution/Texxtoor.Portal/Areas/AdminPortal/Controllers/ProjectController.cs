using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {
  [Authorize(Roles = "Admin")]
  public class ProjectController : ControllerExt {

    //
    // GET: /PortalAdmin/Project/

    public ViewResult Index() {
      return View(AdminDb.Projects.ToList());
    }

    //
    // GET: /PortalAdmin/Project/Details/5

    public ViewResult Details(int id) {
      Project project = AdminDb.Projects.Find(id);
      return View(project);
    }

    //
    // GET: /PortalAdmin/Project/Create

    public ActionResult Create() {
      return View();
    }

    //
    // POST: /PortalAdmin/Project/Create

    [HttpPost]
    public ActionResult Create(Project project) {
      if (ModelState.IsValid) {
        AdminDb.Projects.Add(project);
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(project);
    }

    //
    // GET: /PortalAdmin/Project/Edit/5

    public ActionResult Edit(int id) {
      Project project = AdminDb.Projects.Find(id);
      return View(project);
    }

    //
    // POST: /PortalAdmin/Project/Edit/5

    [HttpPost]
    public ActionResult Edit(Project project) {
      if (ModelState.IsValid) {
        AdminDb.Entry(project).State = EntityState.Modified;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(project);
    }

    //
    // GET: /PortalAdmin/Project/Delete/5

    public ActionResult Delete(int id) {
      Project project = AdminDb.Projects.Find(id);
      return View(project);
    }

    //
    // POST: /PortalAdmin/Project/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id) {
      Project project = AdminDb.Projects.Find(id);
      AdminDb.Projects.Remove(project);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }


  }
}