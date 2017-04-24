using System.Web.Mvc;
using Texxtoor.BaseLibrary;
using Texxtoor.EasyAuthor.Core.Extensions;

namespace Texxtoor.EasyAuthor.Controllers
{

    public class CmsController : ControllerExt
    {
        public ActionResult UserPrivateMenu()
        {
            return this.PartialView(UserManager.Instance.GetCurrentUser(User.Identity.Name));
        }
    }
}