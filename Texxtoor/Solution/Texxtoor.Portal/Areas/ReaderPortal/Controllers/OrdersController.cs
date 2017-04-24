using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary.Exceptions;
using Texxtoor.BaseLibrary.Mashup.Payment.ExpressCheckout;
using Texxtoor.BaseLibrary.Services;
using Texxtoor.BusinessLayer;
using Texxtoor.BusinessLayer.Workflows;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Common;
using Texxtoor.ViewModels.Users;

namespace Texxtoor.Portal.Areas.ReaderPortal.Controllers {

  /// <summary>
  /// This module manages new and existing orders from users.
  /// </summary>
  /// <remarks>
  /// Users order products by creating a specific work, either from existing content or by merging private and
  /// existing content. The content is stored in the private work table. Each portion of a work, called a snippet,
  /// references an item in the FrozenSnippets table. These snippets are save against changes by authors.
  /// From the work the user can create a product. The product is specific to the user and will be enriched with
  /// target specific data, such as cover information, dedications, behaviors, or whether a TOC is in it. The product
  /// is final from view of content. It's not final regarding a specific media.
  /// Media is iBook, PDF, EPub, and so on. The media is created and stored once user finally decided to buy (and pay).
  /// After the payment process is done (or the step skipped if the item is free), the processor creates the media.
  /// Final media is stored in the Blob storage for further reference. These items appear in the private download
  /// section for this particular user.
  /// </remarks>
  [Authorize]
  public class OrdersController : ControllerExt {


    # region Functions to create NEW Orders

    /// <summary>
    /// Create the order dialog, based on an existing product.
    /// </summary>
    /// <param name="id">id is ID of current product, we come here from product listing</param>
    /// <returns></returns> 
    public ActionResult Order(int? id) {
      if (id.HasValue) {
        var product = OrderManager.Instance.GetProductForUser(id.Value, UserName);
        if (product != null) {
          // prepare OrderProduct, which is, once checked out, the "real" product
          try {
            var orderProduct = OrderManager.Instance.CreateOrderProduct(product, UserName);
            return View(orderProduct);
          } catch (NoAddressException naex) {
            return RedirectToAction("DefaultAddress", "Orders", new { id = id, userId = naex.UserId });
          }
        }
      }
      return RedirectToAction("Products", "Products");
    }

    public ActionResult DefaultAddress(int id, int userId) {
      var user = UnitOfWork<UserManager>().GetUser(userId);
      ViewBag.OrderId = id;
      ViewBag.Referrer = Request.UrlReferrer == null ? Url.Action("Order", new { Id = id }) : Request.UrlReferrer.AbsoluteUri;
      return View(new AddressBook() { Name = String.Format("{0} {1}", user.Profile.FirstName.NullSafe(), user.Profile.LastName.NullSafe()) });
    }

    /// <summary>
    /// Create the order dialog, based on a previously ordered, but not yet fullfilled product.
    /// </summary>
    /// <param name="id">id is ID of current orderproduct, we come here from product listing</param>
    /// <returns></returns> 
    public ActionResult ReOrder(int id) {
      var op = OrderManager.Instance.GetOrderProduct(id);
      return View("Order", op);
    }

    /// <summary>
    /// Delete an pre-ordered product.
    /// </summary>
    /// <param name="id"><see cref="OrderProduct"/> id</param>
    /// <returns>Message</returns>
    public JsonResult DeletePreOrder(int id) {
      var name = OrderManager.Instance.DeleteOrderProduct(id);
      if (name == null) {
        return Json(new { message = ControllerResources.OrdersController_DeletePreOrder_Product_cannot_be_removed_ });
      } else {
        return Json(new { message = String.Format(ControllerResources.OrdersController_DeletePreOrder_Product___0___has_been_removed_, name) });
      }
    }

    public ActionResult OrderProduct(int id) {
      var op = OrderManager.Instance.GetOrderProduct(id);
      return PartialView("Order/_OrderProduct", op);
    }

    public ActionResult OrderMedia(int id) {
      var op = OrderManager.Instance.GetOrderProduct(id);
      // prepare media selection
      ViewBag.Medias = OrderManager.Instance.GetMediaSelectList(op);
      // create form
      return PartialView("Order/_OrderMedia", op);
    }

    [HttpPost]
    public JsonResult OrderMedia(int[] ids, int orderProductId, bool subscription) {
      decimal price = 0M;
      decimal subscriptionPrice = 0M;
      // set media
      OrderManager.Instance.SetMediaForOrderProduct(orderProductId, ids, UserName);
      // pricing
      var op = OrderManager.Instance.GetOrderProduct(orderProductId);
      // this is the total of all baseprices as set in the marketiung package ===> AUTHOR driven
      var basePrice = OrderManager.Instance.CalculateProductPrice(orderProductId);
      // this is the total for all medias according to price relation table   ===> PRODUCTION driven
      OrderManager.Instance.CalculateProductionCost(op.Media, HttpContext.Application["ProductionCalc"] as XDocument, op.Colored, basePrice, out price, out subscriptionPrice);
      // ones we have it we store it in the roderproduct for further reference
      var countryAndRegionAdjustment = 0M;
      OrderManager.Instance.SetFinalPrice(orderProductId, basePrice, subscription ? subscriptionPrice : price, countryAndRegionAdjustment, UserName);
      // subscription and naming for same if required
      OrderManager.Instance.SetSubscriptionForOrder(orderProductId, UserName, subscription, DateTime.Now, DateTime.Now.AddYears(1));
      // return result to show user
      return Json(new {
        data = String.Format(ControllerResources.OrdersController_OrderMedia_Product_has_been_set_to_types___0___, String.Join(", ", op.Media.Select(m => m.Name).ToArray())),
        price = String.Format("{0:C}", price),
        subscription = String.Format("{0:C}", subscriptionPrice)
      });
    }

    public JsonResult CalculateShippingAndHandling(int id, int amount) {
      amount = amount < 1 ? 1 : amount;
      // id is orderProduct
      var op = OrderManager.Instance.GetOrderProduct(id);
      var shipping = op.RealPrice / 100 * amount;
      return Json(new {
        basePrice = op.RealPrice,
        shipping = 0,
        total = String.Format("{0:C}", shipping),
        amount
      }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult OrderAddress() {
      var address = UnitOfWork<UserProfileManager>().GetInvoiceAddressForUser(UserName);
      var profile = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName);
      if (profile == null) {
        ViewBag.Message = ControllerResources.OrdersController_OrderAddress_No_user_profile__purchase_cannot_be_stored_permanently_;
      } else {
        if (address == null) {
          // user has forgotten to set address in profile
          address = new AddressBook();
        }
        if (String.IsNullOrEmpty(address.Name)) {
          address.Name = String.Format("{0} {1}", profile.FirstName.NullSafe(), profile.LastName.NullSafe());
        }
      }
      return PartialView("Order/_OrderAddress", address);
    }

    [HttpPost]
    public JsonResult OrderAddress(int? id, AddressBook newAddress, int country, string region) {
      try {
        if (id.HasValue) {
          newAddress.Id = id.Value;
        }
        UnitOfWork<UserProfileManager>().SetOrAddAddressForUser(newAddress, country, region, UserName);
        return Json(new { msg = ControllerResources.OrdersController_OrderAddress_Address_changed_ });
      } catch (Exception ex) {
        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return Json(ex.Message);
      }
    }

    # endregion Functions to create NEW Orders

    # region Quick Order

    [AllowAnonymous]
    public ActionResult QuickOrder(int id) {
      var publ = UnitOfWork<ProjectManager>().GetPublished(id);
      ViewBag.PublishedId = id;
      return View(publ);
    }

    [AllowAnonymous]
    public ActionResult QuickLogOn(int id) {
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
      ViewBag.PublishedId = id;
      return PartialView("QuickOrder/_LogOn", new LogOn());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<JsonResult> QuickLogOn(LogOn model) {
      if (ModelState.IsValid) {
        var result = await UnitOfWork<UserManager>().ValidateUser(model.UserName, model.Password);
        if (!result) return Json(new { r = "error", msg = "Not logged in" });
        await UnitOfWork<UserManager>().SignInAsync(new User { UserName = model.UserName }, model.RememberMe);
        return Json(new { r = "success", msg = "OK" });
      }
      UnitOfWork<UserManager>().RegisterFailedAttempt(model.UserName);
      // If we got so far, something failed, redisplay form
      return Json(new { r = "invalid", msg = "Missing data in form" });
    }

    [AllowAnonymous]
    public ActionResult QuickRegister(int id) {
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;
      ViewBag.PublishedId = id;
      return PartialView("QuickOrder/_Register", new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<JsonResult> QuickRegister(RegisterViewModel model) {
      ViewBag.HasError = false;
      if (UnitOfWork<UserManager>().IsExistingEmail(model.Email)) {
        ModelState.AddModelError("", ControllerResources.AccountController_Register_An_account_with_this_email_already_exists_);
      }
      if (ModelState.IsValid) {
        // let's user refine this in the profile
        var user = new User { UserName = model.UserName };
        var result = await UnitOfWork<UserManager>().Usermanager.CreateAsync(user, model.Password);
        if (result.Succeeded) {
          await UnitOfWork<UserManager>().SignInAsync(user, false);
          UnitOfWork<UserManager>().LogSignIn(model.UserName, true);
        }
        ViewBag.Status = TexxtoorMembershipService.ErrorCodesToString(result.Errors);
      }
      // If we got this far, something failed, redisplay form
      ViewBag.HasError = true;
      ViewBag.PasswordLength = UnitOfWork<UserManager>().MinPasswordLength;

      // If we got this far, something failed, redisplay form
      return Json(new { r = "invalid", msg = UnitOfWork<UserManager>().MinPasswordLength });
    }

    [AllowAnonymous]
    public ActionResult QuickAddress(int id) {
      AddressBook addr = null;
      var uow = UnitOfWork<UserProfileManager>();
      if (User.Identity.IsAuthenticated) {
        addr = uow.GetInvoiceAddressForUser(UserName);
        if (addr != null) {
          var profile = uow.GetProfileByUser(UserName);
          if (profile == null) {
            ViewBag.Message =
              ControllerResources.OrdersController_OrderAddress_No_user_profile__purchase_cannot_be_stored_permanently_;
          } else {
            if (String.IsNullOrEmpty(addr.Name)) {
              addr.Name = String.Format("{0} {1}", profile.FirstName.NullSafe(), profile.LastName.NullSafe());
            }
          }
        }
      }
      ViewBag.PublishedId = id;
      return PartialView("QuickOrder/_Address", addr ?? new AddressBook { Default = true, Invoice = true });
    }

    [HttpPost]
    public ActionResult QuickAddress(int id, AddressBook newAddress, int country, string region) {
      try {
        newAddress.Id = id;
        UnitOfWork<UserProfileManager>().SetOrAddAddressForUser(newAddress, country, region, UserName);
        return Json(new { msg = "OK" });
      } catch (Exception ex) {
        return new HttpNotFoundResult(ex.Message);
      }
    }

    public ActionResult QuickMedia(int id) {
      // prepare media selection, make op indep
      ViewBag.Medias = OrderManager.Instance.GetMediaSelectList(null);
      ViewBag.PublishedId = id;
      return PartialView("QuickOrder/_Media", new OrderProduct());
    }

    // id is Published Id driven from outside
    [HttpPost]
    public JsonResult QuickMedia(int id, int[] ids, uint amount, bool subscription) {
      /** in Quick we just have published texts, it's not possible to come here with Custom or External
       *  Hence the calculation is different as easier than in the regular order procedure */
      var published = UnitOfWork<ReaderManager>().GetPublishedById(id, p => p.Marketing);
      if (ids == null) {
        // nothing selected, hence we return always 0.00
        return Json(new {
          r = "invalid",
          msg = "Error",
          data = ControllerResources.OrdersController_QuickMedia_There_are_no_media_selected__Select_at_least_one_,
          price = "N/A",
          subscription = "N/A"
        });
      }
      var medias = OrderManager.Instance.GetMedias(ids).ToList();
      decimal subscriptionPrice, price;
      // this way we assure that user cannot manipulate payment settings
      dynamic preset = new {
        Id = id,
        Ids = ids,
        Subscription = subscription
      };
      Session["Preset"] = preset;
      // ===> AUTHOR driven
      var basePrice = published.Marketing.BasePrice;
      // ===> PRODUCTION driven
      OrderManager.Instance.CalculateProductionCost(medias, HttpContext.Application["ProductionCalc"] as XDocument, true, basePrice, out price, out subscriptionPrice);
      return Json(new {
        r = "success",
        msg = "OK",
        data = String.Format(ControllerResources.OrdersController_OrderMedia_Product_has_been_set_to_types___0___, String.Join(", ", medias.Select(m => m.Name).ToArray())),
        price = String.Format("{0:C}", price / 100),
        subscription = String.Format("{0:C}", subscriptionPrice / 12 / 100)
      });
    }

    // id is Published Id driven from outside
    public ActionResult QuickPayment() {
      var success = false;
      // this way we assure that user cannot manipulate payment settings
      var preset = (dynamic)Session["Preset"];
      if (preset == null) return new EmptyResult();
      int id = preset.Id;
      int[] ids = preset.Ids;
      bool subscription = preset.Subscription;
      var payPalAmount = 0M;
      OrderProduct op = null;
      using (var scope = UnitOfWork<ReaderManager>().AsUnitOfWork().BeginTransaction()) {
        try {
          // if user came from regular order procedure (through catalog entry) and runs in simple mode we probably have a work object already
          // create a work that is the container for private content
          var w = ReaderManager.Instance.GetWorkForPublished(id, UserName);
          if (w.Fragments.Any()) {
            // copy the public fragments into a private space
            var wf = ReaderManager.Instance.CopyPublishedToWorkFragments(id);
            w.Fragments = wf.ToList();
          }
          // create a product to store the users attempt to order
          var p = ReaderManager.Instance.GetOrAddProductForWork(w.Id, UserName);

          // create an order entry to fix the embracing data
          op = OrderManager.Instance.CreateOrderProduct(p, UserName);
          // Refresh Media Entry in Store, if one is already there
          var medias = OrderManager.Instance.SetMediaForOrderProduct(op.Id, ids, UserName);
          // store the individually calculated price and assign to the order
          decimal subscriptionPrice, price;
          /** in Quick we just have published texts, it's not possible to come here with Custom or External
           *  Hence the calculation is different as easier than in the regular order procedure */
          var published = UnitOfWork<ReaderManager>().GetPublishedById(id);
          // ===> AUTHOR driven
          var basePrice = published.Marketing.BasePrice;
          // ===> PRODUCTION driven
          OrderManager.Instance.CalculateProductionCost(medias, HttpContext.Application["ProductionCalc"] as XDocument, true, basePrice, out price, out subscriptionPrice);
          // adjustment for country or region
          var countryOrRegionAdjustment = 0M;
          // keep results
          OrderManager.Instance.SetFinalPrice(op.Id, basePrice, subscription ? subscriptionPrice : price, countryOrRegionAdjustment, UserName);
          // subscription and naming
          OrderManager.Instance.SetSubscriptionForOrder(op.Id, UserName, subscription, DateTime.Now, DateTime.Now.AddYears(1));
          scope.Commit();
          payPalAmount = op.RealPrice / 100;
          success = true;
        } catch (Exception) {
          scope.Rollback();
        }
      }
      // we got so far, so everything is in the database
      if (success) {
        // Paypal checkout
        RedirectToAction("ExpressCheckout", new { id = op.Id });
      }
      return RedirectToAction("PayPalError", new { id = id, error = "Something went wrong" });
    }

    # endregion

    # region Functions to handle previous fullfilled Orders

    public ActionResult PreviousOrders() {
      return View();
    }

    public ActionResult PreOrders(PaginationFormModel p) {
      string userName = UserName;
      var model = OrderManager.Instance.GetOrders(userName);
      return PartialView("Order/_PreOrders", model.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    # endregion Functions to handle OLD Orders

    #region Express PayPal
    /// <summary>
    /// Redirect to the PayPal for credentials.
    /// </summary>
    /// <returns></returns> 
    public ActionResult ExpressCheckout(int id) {
      var op = OrderManager.Instance.GetOrderProduct(id);
      if (op.RealPrice == 0) {
        // no pricing, product is free, so we provide it immediately to the user
        OrderManager.Instance.SetFullfillmentState(id, FullFillmentState.Payed);
        if ((op.Store.FullFillment & FullFillmentState.Produced) != FullFillmentState.Produced) {
          try {
            ProducePersonalMedia(id);
            return View("PayPal/Confirmation", op);
          } catch (Exception ex) {
            ViewBag.Error = ex.Message;
            return View("PayPal/PayPalError", op);
          }
        }
      }
      // Call the Express Checkout through API
      var returnUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Url.Action("Receipt", new { id });
      var cancelUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Url.Action("Cancel", new { id });
      var checkout = new NVPExpressAPICaller(returnUrl, cancelUrl);
      var redirectUrl = "";
      var token = "";
      // amount as string in US format
      var amount = Math.Round(op.RealPrice / 100, 2).ToString(new CultureInfo("en-us")); //rounding off Real Price, cause paypal accept upto 2 decimal for order amount.
      // execute
      var isSucceed = checkout.SetExpressCheckout(amount, op.Name, CurrentCulture, ref token, ref redirectUrl);
      // store for last step protected from URLs
      Session["amount"] = amount;
      if (isSucceed) {
        Session["token"] = token;
        return Redirect(redirectUrl);
      } else {
        return RedirectToAction("PayPalError", new { id, error = redirectUrl });
      }
    }

    /// <summary>
    /// PayPal redirect to here, after user reviewing the their shipping address, payment method etc (this setting done in web.config file).
    /// </summary>
    /// <returns></returns> 
    public ActionResult Receipt(int id, string token, [Bind(Prefix = "PayerID")] string payerId) {
      var op = OrderManager.Instance.GetOrderProduct(id);
      OrderManager.Instance.SetFullfillmentState(id, FullFillmentState.PreOrder);
      var shipping = new NVPExpressAPICaller();
      var returnMsg = String.Empty;
      var shippingAddressDecoder = new NVPCodec();
      // get the paypal shipping details.
      var isSucceed = shipping.GetExpressCheckoutDetails(token, ref payerId, ref shippingAddressDecoder, ref returnMsg);
      // if it's different we refresh the provided address, only if Paypal has been confirmed
      if (shippingAddressDecoder["ADDRESSSTATUS"].ToLower() == "confirmed") {
        var iso2 = shippingAddressDecoder["SHIPTOCOUNTRYCODE"];
        var country = UnitOfWork<UserProfileManager>().GetCountryList().Single(c => c.Iso2Code == iso2).Name;
        var newShippingAddress = new OrderShippingAddress {
          City = shippingAddressDecoder["SHIPTOCITY"],
          StreetNumber = shippingAddressDecoder["SHIPTOSTREET"],
          Country = country,
          Name = shippingAddressDecoder["SHIPTONAME"]
        };
        OrderManager.Instance.SetShippingAddressFromPayment(op.Id, newShippingAddress);
      }
      if (!isSucceed) {
        return RedirectToAction("PayPalError", new { id, error = returnMsg });
      }
      Session["token"] = token;
      Session["payerid"] = payerId;
      op = OrderManager.Instance.GetOrderProduct(id);
      return View("PayPal/Receipt", op);
    }

    public ActionResult Confirmation(int id) {
      var op = OrderManager.Instance.GetOrderProduct(id);
      // pay only if this order is not yet payed
      if ((op.Store.FullFillment & FullFillmentState.Payed) != FullFillmentState.Payed) {
# if !DEBUG
      var checkout = new NVPExpressAPICaller();
      var returnMsg = String.Empty;
      var amount = op.RealPrice.ToString();
      var payerId = Session["payerid"].ToString();
      var token = Session["token"].ToString();
      var finalDecoder = new NVPCodec();
      var isSucceed = checkout.DoExpressCheckoutPayment(amount, op.Name, token, payerId, ref finalDecoder, ref returnMsg);

      if (!isSucceed) {
        return RedirectToAction("PayPalError", new { id, error = returnMsg });
      }      
# endif
        try {
          // revenues only once when payment is made
          UnitOfWork<AccountingManager>().HandleRevenueShare(id);
        } catch (Exception) {
        }
        OrderManager.Instance.SetFullfillmentState(id, FullFillmentState.Payed);
      }
      if ((op.Store.FullFillment & FullFillmentState.Produced) != FullFillmentState.Produced) {
        try {
          ProducePersonalMedia(id);
        } catch (Exception) {

        }
      }

      return View("PayPal/Confirmation", op);
    }

    /// <summary>
    /// If user cancel the transaction 
    /// then PayPal redirect to this screen (this setting done in web.config file).
    /// </summary>
    /// <returns></returns> 
    public ActionResult Cancel(string token) {
      return View("PayPal/Cancel", new { token });
    }

    /// <summary>
    /// Handling the unexpected errors during the Express checkout process.
    /// </summary>
    /// <returns></returns> 
    public ActionResult PayPalError(int id, string errors) {
      var op = OrderManager.Instance.GetOrderProduct(id);
      ViewBag.Errors = errors;
      return View("PayPal/PayPalError", op);
    }
    #endregion

    # region Workflow

    private void ProducePersonalMedia(int orderProductId) {
      var product = OrderManager.Instance.GetOrderProductForProduction(orderProductId);
      // TODO: Add language management here
      var input = new Dictionary<string, object>();
      var syncEvent = new AutoResetEvent(false);
      // Invoke Workflow Here      
      var productionWorkflow = new ProductCreationWorkflow();
      if (product.Work.Extern == WorkType.Published) {
        input.Add("lang", product.Work.Published.SourceOpus.LocaleId);
        // TODO: modify workflow
        input.Add("templateGroupIdPdf", product.Work.Published.PreferredTemplateGroup.Single(t => t.Group == GroupKind.Pdf).Id);
        input.Add("templateGroupIdEPub", product.Work.Published.PreferredTemplateGroup.Single(t => t.Group == GroupKind.Epub).Id);
        input.Add("templateGroupIdHTML", product.Work.Published.PreferredTemplateGroup.Single(t => t.Group == GroupKind.Html).Id);
      } else {
        // because it's user generated we pull the default (common) template
        input.Add("lang", product.LocaleId);
        input.Add("templateGroupId", UnitOfWork<ProjectManager>().GetTemplateGroup(product.LocaleId).Id);
      }
      input.Add("userName", UserName);
      input.Add("orderProduct", product);
      var instance = new WorkflowApplication(productionWorkflow, input) {
        Completed = e => syncEvent.Set(),
        OnUnhandledException = e => UnhandledExceptionAction.Terminate,
        Aborted = e => syncEvent.Set()
      };
      // TODO: Add Error Handling, Store Error state per user
      instance.Run();
      // method returns immediately
    }

    # endregion
  }
}
