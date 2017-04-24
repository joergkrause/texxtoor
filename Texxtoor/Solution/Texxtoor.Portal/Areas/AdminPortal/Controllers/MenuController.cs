using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Cms;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {

  [Authorize(Roles = "Admin,CmsAdmin")]
  public class MenuController : ControllerExt {

    //
    // GET: /CmsAdmin/Menu/

    public ViewResult Index() {
      return View(AdminDb.Menus.ToList());
    }

    //
    // GET: /CmsAdmin/Menu/Details/5

    public ViewResult Details(int id) {
      CmsMenu cmsmenu = AdminDb.Menus.Find(id);
      return View(cmsmenu);
    }

    //
    // GET: /CmsAdmin/Menu/Create

    public ActionResult Create() {
      return View();
    }

    //
    // POST: /CmsAdmin/Menu/Create

    [HttpPost]
    public ActionResult Create(CmsMenu cmsmenu) {
      if (ModelState.IsValid) {
        AdminDb.Menus.Add(cmsmenu);
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(cmsmenu);
    }

    //
    // GET: /CmsAdmin/Menu/Edit/5

    public ActionResult Edit(int id) {
      CmsMenu cmsmenu = AdminDb.Menus.Find(id);
      return View(cmsmenu);
    }

    //
    // POST: /CmsAdmin/Menu/Edit/5

    [HttpPost]
    public ActionResult Edit(CmsMenu cmsmenu) {
      if (ModelState.IsValid) {
        AdminDb.Entry(cmsmenu).State = EntityState.Modified;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(cmsmenu);
    }

    //
    // GET: /CmsAdmin/Menu/Delete/5

    public ActionResult Delete(int id) {
      CmsMenu cmsmenu = AdminDb.Menus.Find(id);
      return View(cmsmenu);
    }

    //
    // POST: /CmsAdmin/Menu/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id) {
      CmsMenu cmsmenu = AdminDb.Menus.Find(id);
      AdminDb.Menus.Remove(cmsmenu);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }

  }
}