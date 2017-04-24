using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {
  [Authorize(Roles = "Admin")]
  public class CatalogController : ControllerExt {

    //
    // GET: /PortalAdmin/Catalog/

    public ViewResult Index() {
      return View(AdminDb.Catalog.ToList());
    }

    //
    // GET: /PortalAdmin/Catalog/Details/5

    public ViewResult Details(int id) {
      Catalog catalog = AdminDb.Catalog.Find(id);
      return View(catalog);
    }

    //
    // GET: /PortalAdmin/Catalog/Create

    public ActionResult Create() {
      return View(new Catalog());
    }

    //
    // POST: /PortalAdmin/Catalog/Create

    [HttpPost]
    public ActionResult Create(Catalog catalog) {
      if (ModelState.IsValid) {
        AdminDb.Catalog.Add(catalog);
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(catalog);
    }

    //
    // GET: /PortalAdmin/Catalog/Edit/5

    public ActionResult Edit(int id) {
      Catalog catalog = AdminDb.Catalog.Find(id);
      return View(catalog);
    }

    //
    // POST: /PortalAdmin/Catalog/Edit/5

    [HttpPost]
    public ActionResult Edit(Catalog catalog) {
      if (ModelState.IsValid) {
        AdminDb.Entry(catalog).State = EntityState.Modified;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(catalog);
    }

    //
    // GET: /PortalAdmin/Catalog/Delete/5

    public ActionResult Delete(int id) {
      Catalog catalog = AdminDb.Catalog.Find(id);
      return View(catalog);
    }

    //
    // POST: /PortalAdmin/Catalog/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id) {
      Catalog catalog = AdminDb.Catalog.Find(id);
      AdminDb.Catalog.Remove(catalog);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }

  }
}