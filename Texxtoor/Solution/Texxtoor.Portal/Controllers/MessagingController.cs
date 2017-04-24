using System;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Common;

namespace Texxtoor.Portal.Controllers {

  [Authorize]
  public class MessagingController : ControllerExt {

    # region Mail

    [NavigationPathFilter("Mail")]
    public ActionResult Index(string filter) {
      if (!String.IsNullOrEmpty(filter)) {
        // check valid filter values
        if (!(new string[] { "In", "Out" }).Contains(filter)) {
          filter = "In";
        }
      }
      ViewBag.Filter = filter;
      return View("Mail/Index", new Message());
    }

    public ActionResult ListMessages(string filter, PaginationFormModel p) {
      filter = filter ?? String.Empty;
      var msgs = Manager<UserManager>.Instance.GetMessagesByFilter(filter, UserName).ToList();
      ViewBag.Filter = filter;
      ViewBag.Count = msgs.Count();
      return PartialView("Mail/_List", msgs.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public JsonResult GetMessageReceivers(string q) {
      var result = Manager<UserManager>.Instance.GetUsersByName(q, UserName)
               .Select(u => new {
                 name = u.UserName,
                 id = u.Id
               });
      return Json(result, JsonRequestBehavior.AllowGet);
    }

    public ActionResult AddMessage() {
      var msg = new Message {
        Sender = Manager<UserManager>.Instance.GetUserByName(UserName),
        Subject = "",
        Body = ""
      };
      return PartialView("Mail/_AddMessage", msg);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="receivers">Comma separated list of id</param>
    /// <param name="msg"></param>
    /// <returns></returns>
    [HttpPost, ValidateInput(false)]
    public JsonResult AddMessage(string receivers, Message msg) {
      var ids = receivers.Split(',').Select(v => Convert.ToInt32(v)).ToList();
      Manager<UserManager>.Instance.AddMessage(ids, msg, UserName);
      return Json(new { msg = ControllerResources.MessagingController_AddMessage_Successfully_send__ });
    }

    # endregion

    public ActionResult DeleteMessage(int id) {
      var result = Manager<UserManager>.Instance.DeleteMessage(id, UserName);
      if (result) {
        return Json(new {msg = ControllerResources.MessagingController_DeleteMessage_Message_deleted__}, JsonRequestBehavior.AllowGet);
      }
      else {
        return new HttpNotFoundResult("Could not delete message.");
      }
    }

    public ActionResult ShowMessage(int id) {
      var msg = Manager<UserManager>.Instance.GetMessageForUser(id, UserName);
      return PartialView("Mail/_ShowMessage", msg);
    }

    public ActionResult ReportAsSpam(int id) {
      var msg = Manager<UserManager>.Instance.GetMessageForUser(id, UserName);
      return View("Mail/ReportAsSpam", msg);
    }

    public ActionResult Forward(int id) {
      var msg = Manager<UserManager>.Instance.GetMessageForUser(id, UserName);
      return View("Mail/Forward", msg);
    }

    public ActionResult SendMail(int id, int senderId) {
      var receiver = UnitOfWork<UserManager>().GetUser(id);
      var sender = UnitOfWork<UserManager>().GetUser(senderId);
      return View("Mail/SendMail", new Tuple<User, User>(receiver, sender));
    }

    public ActionResult SendMessage(User receiver, User sender) {
      var msg = new Message {
        Receiver =  receiver,
        Sender = sender
      };
      return PartialView("Mail/_SendMessage", msg);
    }

    [HttpPost]
    public ActionResult SendMessage(string receivers, Message msg) {
      if (ModelState.IsValid && !String.IsNullOrEmpty(receivers)) {
        var ids = receivers.Split(',').Select(v => Convert.ToInt32(v)).ToList();
        Manager<UserManager>.Instance.AddMessage(ids, msg, UserName);
      }
      if (String.IsNullOrEmpty(receivers))
      {
        ModelState.AddModelError("receivers", ControllerResources.MessagingController_SendMessage_You_must_at_least_select_one_user);
        return new HttpNotFoundResult(ControllerResources.MessagingController_SendMessage_You_must_at_least_select_one_user);
      }
      return Json(new { msg = "Message sent" });
    }

  }
}
