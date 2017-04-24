using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {
  [Authorize(Roles = "Admin")]
  public class TeamController : ControllerExt {

    //
    // GET: /PortalAdmin/Team/

    public ViewResult Index() {
      return View(AdminDb.Teams.ToList());
    }

    //
    // GET: /PortalAdmin/Team/Details/5

    public ViewResult Details(int id) {
      Team team = AdminDb.Teams.Find(id);
      return View(team);
    }

    //
    // GET: /PortalAdmin/Team/Create

    public ActionResult Create() {
      return View();
    }

    //
    // POST: /PortalAdmin/Team/Create

    [HttpPost]
    public ActionResult Create(Team team) {
      if (ModelState.IsValid) {
        AdminDb.Teams.Add(team);
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(team);
    }

    //
    // GET: /PortalAdmin/Team/Edit/5

    public ActionResult Edit(int id) {
      Team team = AdminDb.Teams.Find(id);
      return View(team);
    }

    //
    // POST: /PortalAdmin/Team/Edit/5

    [HttpPost]
    public ActionResult Edit(Team team) {
      if (ModelState.IsValid) {
        AdminDb.Entry(team).State = EntityState.Modified;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(team);
    }

    //
    // GET: /PortalAdmin/Team/Delete/5

    public ActionResult Delete(int id) {
      Team team = AdminDb.Teams.Find(id);
      return View(team);
    }

    //
    // POST: /PortalAdmin/Team/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id) {
      Team team = AdminDb.Teams.Find(id);
      AdminDb.Teams.Remove(team);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }

  }
}