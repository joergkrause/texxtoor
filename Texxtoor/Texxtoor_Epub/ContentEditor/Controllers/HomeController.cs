using System.Web.Mvc;

namespace Texxtoor.Editor.Controllers {
  public class HomeController : Controller {
    public ActionResult Index() {
      ViewBag.Message = "Demo.";

      return View();
    }

    public ActionResult About() {
      ViewBag.Message = "About me.";

      return View();
    }

    public ActionResult Contact() {
      ViewBag.Message = "Contact.";

      return View();
    }

    
  }
}
