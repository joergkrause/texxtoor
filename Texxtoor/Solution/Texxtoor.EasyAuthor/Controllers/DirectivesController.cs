using System.Web.Mvc;

namespace Texxtoor.EasyAuthor.Controllers
{

    public class DirectivesController : Controller
    {
        public ActionResult TextItem()
        {
            return this.PartialView();
        }
    }
}