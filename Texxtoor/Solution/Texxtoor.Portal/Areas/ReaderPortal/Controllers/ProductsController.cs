using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DocumentFormat.OpenXml.Office2010.Word;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.BaseLibrary;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.ViewModels.Common;

namespace Texxtoor.Portal.Areas.ReaderPortal.Controllers {

  /// <summary>
  /// The assembler transforms public and private work into products one can buy. It "assembles" product information.
  /// </summary>
  [Authorize]
  public class ProductsController : ControllerExt {

    # region --== Product ==--

    /// <summary>
    /// Work related products, entry point if no work is specified.
    /// </summary>
    /// <returns></returns>
    public ActionResult Products() {
      return View("Basket");
    }

    public ActionResult PreviousProducts() {
      return View();
    }

    /// <summary>
    /// 3. Product (Auswahl Produkt)
    /// </summary>
    /// <remarks>
    /// The user can now select from the fragments of the temporarily created collection of this works</remarks>
    /// <param name="page"></param>
    /// <returns></returns>
    public ActionResult ListProducts(PaginationFormModel p) {
      var products = OrderManager.Instance.GetProductsForUser(UserName);
      return PartialView("Product/_ListProducts", products.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    /// <summary>
    /// Create a detached product object for specified work.
    /// </summary>
    /// <param name="id">Work Id</param>
    /// <returns>Detached product object, not in database yet.</returns>
    public ActionResult AddProduct(int id) {
      var p = ReaderManager.Instance.GetOrAddProductForWork(id, UserName);
      return PartialView("Product/_AddProduct", p);
    }

    /// <summary>
    /// Saves the detached product object to the database, personalizing the product.
    /// </summary>
    /// <param name="product"></param>
    /// <param name="workId"></param>
    /// <returns></returns>
    [HttpPost]
    public JsonResult AddProduct(Product product, int workId) {
      ReaderManager.Instance.AddProductForWorkId(product, workId, UserName);
      return Json(new { msg = String.Format("Product {0} added.", product.Title) });
    }

    // edit specific product
    // user adds personal information, private fragments or other stuff to the "Product"
    public ActionResult EditProduct(int id) {
      var product = ReaderManager.Instance.GetProduct(id);
      return PartialView("Product/_EditProduct", product);
    }

    [HttpPost]
    public JsonResult EditProduct(Product newProduct) {
      var product = ReaderManager.Instance.SaveProduct(newProduct);
      return Json(new { msg = String.Format("Product {0} changed.", product.Title) });
    }

    public JsonResult DeleteProduct(int id) {
      var product = ReaderManager.Instance.DeleteProduct(id, UserName);
      return Json(new { msg = String.Format("Product {0} deleted.", product.Title) });
    }

    # endregion --== Product ==--


    # region --== Work ==--

    /// <summary>
    /// Retrieve a Work and attached products. Creates a product, if it doesn't exists. Can optionally create a copy if it exists.
    /// </summary>
    /// <param name="id">Work Id</param>
    /// <returns></returns>
    public ActionResult BuyWork(int id) {
      // assembler MUST based on a specific work
      var work = ReaderManager.Instance.GetWorkWithProducts(id, UserName);
      // we need a work, if there isn't one, create one
      if (work == null) {
        throw new ArgumentOutOfRangeException("Either work must exists or published provided");
      }
      // we also assume that we want to create a Product, hence at least one (as a default) MUST exist           
      if (work.Products == null || !work.Products.Any()) {
        ReaderManager.Instance.GetOrAddProductForWork(id, UserName);
      }
      Session["CurrentWork"] = work;
      var products = ReaderManager.Instance.GetAllProducts(UserName);
      return View("Basket", new PagedList<Product>(products.AsQueryable(), 0, 5));
    }

    public ActionResult BuyProduct(int id) {
      // assembler MUST based on a specific work
      var work = ReaderManager.Instance.GetWorkForPublished(id, UserName);
      if (work == null) {
        throw new ArgumentOutOfRangeException("Either work must exists or published provided");
      }
      // we also assume that we want to create a Product, hence at least one (as a default) MUST exist           
      if (work.Products == null || !work.Products.Any()) {
        var p = ReaderManager.Instance.GetOrAddProductForWork(work.Id, UserName);
      }
      var profile = UserProfileManager.Instance.GetProfileByUser(UserName);
      if (profile.RunControl.Complexity == Complexity.Simple) {
        // let quick order retrieve all the objects again to support multiple entry points
        return RedirectToAction("QuickOrder", "Orders", new { id = work.Published.Id });
      } else {
        Session["CurrentWork"] = work;
        var products = ReaderManager.Instance.GetAllProducts(UserName);
        return View("Basket", new PagedList<Product>(products.AsQueryable(), 0, 5));
      }
    }


    # endregion --== Work ==--

    # region --== Order Support ==-

    /// <summary>
    /// Retrieve all orders stored in OrderProducts but not yet fullfilled (OrderStore not set).
    /// </summary>
    /// <returns></returns>
    public ActionResult PreOrders(PaginationFormModel p) {
      var result = OrderManager.Instance.GetOrderProductsForUser(UserName, false);
      return View("Product/_PreOrders", result.ToPagedList(p.Page, p.PageSize, p.FilterName, p.FilterName, p.Order, p.Dir));
    }

    # endregion

  }
}
