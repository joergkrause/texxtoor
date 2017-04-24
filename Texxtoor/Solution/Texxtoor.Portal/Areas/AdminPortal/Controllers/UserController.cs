using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {
  [Authorize(Roles = "Admin,TenantAdmin")]
  public class UserController : ControllerExt {

    //
    // GET: /PortalAdmin/User/

    public ViewResult Index() {
      var users = AdminDb.Users.Include(u => u.Profile);
      return View(users.ToList());
    }

    //
    // GET: /PortalAdmin/User/Details/5

    public ViewResult Details(int id) {
      var user = AdminDb.Users
        .Include(u => u.Roles)
        .Single(u => u.Id == id);
      return View(user);
    }

    //
    // GET: /PortalAdmin/User/Create

    public ActionResult Create() {
      ViewBag.Id = new SelectList(AdminDb.UserProfiles, "Id", "Description");
      var user = new User {
        PasswordFormat = MembershipPasswordFormat.Hashed,
      };
      return View(user);
    }

    //
    // POST: /PortalAdmin/User/Create

    [HttpPost]
    public ActionResult Create(User user) {
      if (ModelState.IsValid) {
        throw new NotImplementedException("access BLL to add user");
        //if (status == MembershipCreateStatus.Success)
        //{
        //  return RedirectToAction("Index");
        //}
        //ModelState.AddModelError("", "Error creating user: " + Enum.GetName(typeof(MembershipCreateStatus), status));
      }
      ViewBag.Id = new SelectList(AdminDb.UserProfiles, "Id", "Description", user.Id);
      return View(user);
    }

    //
    // GET: /PortalAdmin/User/Edit/5

    public ActionResult Edit(int id) {
      var user = AdminDb.Users
        .Include(u => u.Roles)
        .Single(u => u.Id == id);
      ViewBag.Id = new SelectList(AdminDb.UserProfiles, "Id", "Description", user.Id);
      return View(user);
    }

    //
    // POST: /PortalAdmin/User/Edit/5

    [HttpPost]
    public ActionResult Edit(User user) {
      if (ModelState.IsValid) {
        AdminDb.Entry(user).State = EntityState.Modified;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      ViewBag.Id = new SelectList(AdminDb.UserProfiles, "Id", "Description", user.Id);
      return View(user);
    }

    //
    // GET: /PortalAdmin/User/Delete/5

    public ActionResult Delete(int id) {
      var user = AdminDb.Users.Find(id);
      return View(user);
    }

    //
    // POST: /PortalAdmin/User/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id) {
      var user = AdminDb.Users.Find(id);
      AdminDb.Users.Remove(user);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }

  }
}