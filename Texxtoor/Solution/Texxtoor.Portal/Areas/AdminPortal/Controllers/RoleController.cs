using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {
  [Authorize(Roles = "Admin")]
  public class RoleController : ControllerExt {

    //
    // GET: /CmsAdmin/Role/

    public ViewResult Index() {
      return View(AdminDb.Roles.ToList());
    }

    //
    // GET: /CmsAdmin/Role/Details/5

    public ViewResult Details(int id) {
      Role role = AdminDb.Roles.Find(id);
      return View(role);
    }

    //
    // GET: /CmsAdmin/Role/Create

    public ActionResult Create() {
      return View();
    }

    //
    // POST: /CmsAdmin/Role/Create

    [HttpPost]
    public ActionResult Create(Role role) {
      if (ModelState.IsValid) {
        AdminDb.Roles.Add(role);
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(role);
    }

    //
    // GET: /CmsAdmin/Role/Edit/5

    public ActionResult Edit(int id) {
      Role role = AdminDb.Roles.Find(id);
      return View(role);
    }

    //
    // POST: /CmsAdmin/Role/Edit/5

    [HttpPost]
    public ActionResult Edit(Role role) {
      if (ModelState.IsValid) {
        AdminDb.Entry(role).State = EntityState.Modified;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(role);
    }

    //
    // GET: /CmsAdmin/Role/Delete/5

    public ActionResult Delete(int id) {
      Role role = AdminDb.Roles.Find(id);
      return View(role);
    }

    //
    // POST: /CmsAdmin/Role/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id) {
      Role role = AdminDb.Roles.Find(id);
      AdminDb.Roles.Remove(role);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }

  }
}