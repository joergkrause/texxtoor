using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.DataModels.Context;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {

  [Authorize(Roles = "Admin")]
  public class AddressController : ControllerExt {

    // GET: /AdminPortal/Address/
    public ActionResult Index() {
      return View(AdminDb.AddressBook.ToList());
    }

    // GET: /AdminPortal/Address/Details/5
    public ActionResult Details(int? id) {
      if (id == null) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      AddressBook addressbook = AdminDb.AddressBook.Find(id);
      if (addressbook == null) {
        return HttpNotFound();
      }
      return View(addressbook);
    }

    // GET: /AdminPortal/Address/Create
    public ActionResult Create() {
      return View();
    }

    // POST: /AdminPortal/Address/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "Id,Name,StreetNumber,Zip,City,Region,Default,Invoice,CreatedAt,ModifiedAt")] AddressBook addressbook) {
      if (ModelState.IsValid) {
        AdminDb.AddressBook.Add(addressbook);
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(addressbook);
    }

    // GET: /AdminPortal/Address/Edit/5
    public ActionResult Edit(int? id) {
      if (id == null) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      AddressBook addressbook = AdminDb.AddressBook.Find(id);
      if (addressbook == null) {
        return HttpNotFound();
      }
      return View(addressbook);
    }

    // POST: /AdminPortal/Address/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "Id,Name,StreetNumber,Zip,City,Region,Default,Invoice,CreatedAt,ModifiedAt")] AddressBook addressbook) {
      if (ModelState.IsValid) {
        AdminDb.Entry(addressbook).State = EntityState.Modified;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(addressbook);
    }

    // GET: /AdminPortal/Address/Delete/5
    public ActionResult Delete(int? id) {
      if (id == null) {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      AddressBook addressbook = AdminDb.AddressBook.Find(id);
      if (addressbook == null) {
        return HttpNotFound();
      }
      return View(addressbook);
    }

    // POST: /AdminPortal/Address/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id) {
      AddressBook addressbook = AdminDb.AddressBook.Find(id);
      AdminDb.AddressBook.Remove(addressbook);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }

  }
}
