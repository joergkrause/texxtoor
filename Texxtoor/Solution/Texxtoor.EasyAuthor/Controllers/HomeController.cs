using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Texxtoor.BaseLibrary;
using Texxtoor.BaseLibrary.Services;
using Texxtoor.DataModels.Models;
using Texxtoor.EasyAuthor.Core.Extensions;

namespace Texxtoor.EasyAuthor.Controllers {

  public class HomeController : ControllerExt {


    [AllowAnonymous]
    public ActionResult Logon(string h) {
      // TODO: Take over a unique Hash
      if (!User.Identity.IsAuthenticated) {
        // TODO: Take over name
        if (UnitOfWork<UserManager>().CheckEncryptedPassword(h)) {
          UnitOfWork<UserManager>().LogSignIn(User.Identity.Name, true);
        }
      }
      return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public ActionResult Index() {
      ViewBag.Title = "Easy Author Portal";
      // TODO: Take over a unique Hash
      var menu = MenuService.LoadMenu("footer", UserRole.Unknown, this.CurrentCulture).ToList();
      ViewBag.FooterMenu = menu;
      return View();
    }



  }
}