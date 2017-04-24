using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {
  /// <summary>
  /// Manage tenants, customers that have exclusive areas (publishers, companies etc)
  /// </summary>
  [Authorize(Roles = "Admin,TenantAdmin")]
  public class TenantController : ControllerExt {

    public ActionResult Index() {
      return View();
    }

    public ActionResult ShowTenants() {
      var result = AdminDb.Tenants.Include(t => t.Users).ToList();
      return PartialView("_ShowTenants", result);
    }

    public ViewResult Details(int id) {
      var tenant = AdminDb.Tenants.Include(t => t.Users).First(t => t.Id == id);
      return View(tenant);
    }

    public ActionResult Create() {
      return View();
    }

    [HttpPost]
    public ActionResult Create(Tenant tenant) {
      if (ModelState.IsValid) {
        AdminDb.Tenants.Add(tenant);
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(tenant);
    }

    public ActionResult Edit(int id) {
      var tenant = AdminDb.Tenants.Include(t => t.Users).First(t => t.Id == id);
      return View(tenant);
    }

    [HttpPost]
    public ActionResult Edit(Tenant tenant) {
      if (!String.IsNullOrEmpty(tenant.Name) && !String.IsNullOrEmpty(tenant.Description)) {
        var t = AdminDb.Tenants.Find(tenant.Id);
        t.Name = tenant.Name;
        t.Description = tenant.Description;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      ViewBag.Users = new SelectList(AdminDb.Users, "Id", "UserName");
      return View(tenant);
    }

    public ActionResult Delete(int id) {
      var tenant = AdminDb.Tenants.Find(id);
      return View(tenant);
    }

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id) {
      var tenant = AdminDb.Tenants.Find(id);
      AdminDb.Tenants.Remove(tenant);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }
  }
}
