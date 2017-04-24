using System.Web.Mvc;

namespace Texxtoor.EasyAuthor.Controllers
{
  
    public class PartialsController : Controller
    {
        public ActionResult Index()
        {
            return this.RedirectToAction("Dashboard");
        }

        # region

        public ActionResult Dashboard()
        {
            return this.PartialView();
        }

        # endregion

        public ActionResult Overview()
        {
            return this.PartialView();
        }

        public ActionResult OverviewContent() {
          return this.PartialView();
        }

        public ActionResult OverviewSemantics() {
          return this.PartialView();
        }

        public ActionResult OverviewFiles() {
          return this.PartialView();
        }

        # region Preview

        public ActionResult Preview()
        {
            return this.PartialView();
        }

        # endregion

        # region Publish

        public ActionResult Publish() {
          return this.PartialView();
        }

        public ActionResult CheckAndPublish() {
          return this.PartialView();
        }

        # endregion

        # region Marketing

        public ActionResult Marketing() {
          return this.PartialView();
        }

        # endregion

        # region Success

        public ActionResult Success() {
          return this.PartialView();
        }

        # endregion

    }
}