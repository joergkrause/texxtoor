using System.Web.Mvc;

namespace Texxtoor.EasyAuthor.Controllers
{

    public class DialogsController : Controller
    {
        public ActionResult CreateNewText()
        {
            return this.PartialView();
        }

        public ActionResult EditMetadata() {
          return this.PartialView();
        }

        public ActionResult EditImprint() {
          return this.PartialView();
        }

        public ActionResult ShowImageFile() {
          return this.PartialView();
        }
    }
}