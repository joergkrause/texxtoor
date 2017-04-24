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
  public class MenuItemController : ControllerExt {

    //
    // GET: /CmsAdmin/MenuItem/

    public ViewResult Index() {
      return View(AdminDb.MenuItems.ToList());
    }

    //
    // GET: /CmsAdmin/MenuItem/Details/5

    public ViewResult Details(int id) {
      CmsMenuItem cmsmenuitem = AdminDb.MenuItems.Find(id);
      return View(cmsmenuitem);
    }

    //
    // GET: /CmsAdmin/MenuItem/Create

    public ActionResult Create() {
      return View();
    }

    //
    // POST: /CmsAdmin/MenuItem/Create

    [HttpPost]
    public ActionResult Create(CmsMenuItem cmsmenuitem) {
      if (ModelState.IsValid) {
        AdminDb.MenuItems.Add(cmsmenuitem);
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(cmsmenuitem);
    }

    //
    // GET: /CmsAdmin/MenuItem/Edit/5

    public ActionResult Edit(int id) {
      CmsMenuItem cmsmenuitem = AdminDb.MenuItems.Find(id);
      return View(cmsmenuitem);
    }

    //
    // POST: /CmsAdmin/MenuItem/Edit/5

    [HttpPost]
    public ActionResult Edit(CmsMenuItem cmsmenuitem) {
      if (ModelState.IsValid) {
        AdminDb.Entry(cmsmenuitem).State = EntityState.Modified;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(cmsmenuitem);
    }

    //
    // GET: /CmsAdmin/MenuItem/Delete/5

    public ActionResult Delete(int id) {
      CmsMenuItem cmsmenuitem = AdminDb.MenuItems.Find(id);
      return View(cmsmenuitem);
    }

    //
    // POST: /CmsAdmin/MenuItem/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id) {
      CmsMenuItem cmsmenuitem = AdminDb.MenuItems.Find(id);
      AdminDb.MenuItems.Remove(cmsmenuitem);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }

  }
}