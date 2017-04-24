using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Texxtoor.Portal.Areas.ReaderPortal.Controllers
{
  [Authorize]
  public class ShopController : Controller
    {
        //
        // GET: /ReaderPortal/Shop/

        public ActionResult Index()
        {
            return View();
        }

    }
}
