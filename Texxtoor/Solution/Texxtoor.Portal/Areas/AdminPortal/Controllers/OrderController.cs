using System.Data.Entity;
using System.Web.Mvc;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {
  [Authorize(Roles = "Admin")]
  public class OrderController : ControllerExt {

    //
    // GET: /PortalAdmin/Order/

    public ViewResult Index() {
      var orderstores = AdminDb.OrderProducts.Include(o => o.ShippingAddress).Include(o => o.BillingAddress);
      return View(orderstores);
    }

    //
    // GET: /PortalAdmin/Order/Details/5

    public ViewResult Details(int id) {
      var orderstore = AdminDb.OrderProducts.Find(id);
      return View(orderstore);
    }

    //
    // GET: /PortalAdmin/Order/Edit/5

    public ActionResult Edit(int id) {
      var product = AdminDb.OrderProducts.Find(id);
      ViewBag.DeliveryAddressId = new SelectList(AdminDb.AddressBook, "Id", "Name", product.ShippingAddress.Id);
      ViewBag.BillingAddressId = new SelectList(AdminDb.AddressBook, "Id", "Name", product.BillingAddress.Id);
      return View(product);
    }

    //
    // POST: /PortalAdmin/Order/Edit/5

    [HttpPost]
    public ActionResult Edit(OrderProduct orderproduct) {
      if (ModelState.IsValid) {
        AdminDb.Entry(orderproduct).State = EntityState.Modified;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      ViewBag.DeliveryAddressId = new SelectList(AdminDb.AddressBook, "Id", "Name", orderproduct.ShippingAddress.Id);
      ViewBag.BillingAddressId = new SelectList(AdminDb.AddressBook, "Id", "Name", orderproduct.BillingAddress.Id);
      return View(orderproduct);
    }

    //
    // GET: /PortalAdmin/Order/Delete/5

    public ActionResult Delete(int id) {
      var orderstore = AdminDb.OrderProducts.Find(id);
      return View(orderstore);
    }

    //
    // POST: /PortalAdmin/Order/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id) {
      var orderstore = AdminDb.OrderProducts.Find(id);
      AdminDb.OrderProducts.Remove(orderstore);
      AdminDb.SaveChanges();
      return RedirectToAction("Index");
    }

  }
}