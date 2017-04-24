using System;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Functions;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Common;

namespace Texxtoor.Portal.Areas.ReaderPortal.Controllers {

  /// <summary>
  /// Manage user's reading and learning groups.
  /// </summary>
  [Authorize]
  public class GroupsController : ControllerExt {

    public ActionResult AddMessageToGroup(int id){
      var group = UnitOfWork<ReaderManager>().GetGroup(id);
      ViewBag.Receivers = String.Join(",", group.Members.Select(u => u.Id).ToArray());
      var msg = new Message {
        Sender = Manager<UserManager>.Instance.GetUserByName(UserName),
        Subject = "",
        Body = ""
      };
      return PartialView("Groups/_AddMessage", msg);
    }

    public ActionResult AddMessageToUser(int id){
      ViewBag.Receivers = id;
      var msg = new Message {
        Sender = Manager<UserManager>.Instance.GetUserByName(UserName),
        Subject = "",
        Body = ""
      };
      return PartialView("Groups/_AddMessage", msg);
    }

    public JsonResult SearchUsers(int groupId, string q) {
      var users = UnitOfWork<UserManager>().SearchUsersForGroup(groupId, q).ToArray();      
      return Json(users, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="receivers">Comma separated list of id</param>
    /// <param name="msg"></param>
    /// <returns></returns>
    [HttpPost, ValidateInput(false)]
    public JsonResult SendMessage(int[] receivers, Message msg) {
      UnitOfWork<UserManager>().AddMessage(receivers, msg, UserName);
      return Json(new { msg = ControllerResources.MessagingController_AddMessage_Successfully_send__ });
    }

    /// <summary>
    ///  shows member, either all relevant or for the selected group.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult Member(int id) {
      var group = UnitOfWork<ReaderManager>().GetGroup(id);
      return View(group);
    }

    public ActionResult ListMember(int groupId, PaginationFormModel p) {
      var group = UnitOfWork<ReaderManager>().GetGroup(groupId);
      ViewBag.GroupAdmin = group.Owner.Id;
      ViewBag.ReaderGroup = group.Id;
      return PartialView("Member/_ListMembers", group.Members.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    [MinimumAwardScoreFilter(true, 1)]
    public ActionResult AddMember(int groupId) {
      var group = UnitOfWork<ReaderManager>().GetGroup(groupId);
      return PartialView("Member/_AddMember", group);
    }

    [HttpPost]
    [MinimumAwardScoreFilter(true, 1)]
    public ActionResult AddMember(int id, int[] usernames) {
      var result = UnitOfWork<ReaderManager>().AddMemberToGroup(id, usernames, UserName);
      return Json(new { msg = result ? "OK" : "Error" }, JsonRequestBehavior.AllowGet);
    }

    [MinimumAwardScoreFilter(true, 1)]
    public JsonResult DeleteMember(int id, int groupId) {
      // id is here the variable part, as the Dialog.js class forces us to use this
      var result = UnitOfWork<ReaderManager>().RemoveMemberFromGroup(groupId, id, UserName);
      return Json(new{msg = result ? "OK" : "Error"}, JsonRequestBehavior.AllowGet);
    }

    # region Group Management

    public ActionResult Groups() {
      var query = UnitOfWork<ReaderManager>().GetGroups(UserName, true);
      return View(query);
    }

    public ActionResult ListGroups(PaginationFormModel p, bool @public) {
      var grps = UnitOfWork<ReaderManager>().GetGroups(UserName, @public);
      return View("Groups/_ListGroups", grps.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    [MinimumAwardScoreFilter(true, 1)]
    public ActionResult AddGroupButton() {
      return PartialView("Groups/_AddGroupButton");
    }
    
    [MinimumAwardScoreFilter(true, 1)]
    public ActionResult AddQuestionButton() {
      return PartialView("Questions/_AddQuestionButton");
    }

    [MinimumAwardScoreFilter(true, 1)]
    public ActionResult AddMemberButton() {
      return PartialView("Member/_AddMemberButton");
    }

    [MinimumAwardScoreFilter(true, 1)]
    public ActionResult AddGroup() {
      return PartialView("Groups/_AddGroup");
    }

    [HttpPost]
    [MinimumAwardScoreFilter(true, 1)]
    public JsonResult AddGroup(ReaderGroup group) {
      UnitOfWork<ReaderManager>().AddGroupForUser(group, UserName);
      return Json(new { msg = ControllerResources.GroupsController_AddGroup_Group_created });
    }

    public ActionResult EditGroup(int id) {
      var group = UnitOfWork<ReaderManager>().GetGroup(id);
      return PartialView("Groups/_EditGroup", group);
    }

    [HttpPost]
    public JsonResult EditGroup(int id, ReaderGroup newGroup) {
      UnitOfWork<ReaderManager>().EditGroup(id, newGroup);
      return Json(new { msg = ControllerResources.GroupsController_EditGroup_Group_changed });
    }

    [HttpPost]
    public JsonResult DeleteGroup(int id) {
      UnitOfWork<ReaderManager>().DeleteGroup(id, UserName);
      return Json(new { msg = ControllerResources.GroupsController_DeleteGroup_Group_deleted });
    }

    public ActionResult SharedContent(int id, PaginationFormModel p) {
      var group = UnitOfWork<ReaderManager>().GetGroup(id);
      var works = UnitOfWork<ReaderManager>().GetWorksForUsers(group.Members, true);
      ViewBag.TotalCount = works.Count();
      return PartialView("Groups/_SharedContent", works.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    # endregion

    # region Questions

    public ActionResult Questions() {
      var query = ReaderManager.Instance.GetAllQuestions();
      return View(query);
    }

    public ActionResult ListQuestionsForUser(PaginationFormModel p) {
      var model = ReaderManager.Instance.GetOwnOrAuthorQuestions(UserName);
      var user = model.Where(m => m.Key == "User").Select(m => m.Value).AsQueryable();
      ViewBag.WorkCollection = ReaderManager.Instance.GetWorksForUser(UserName).ToList();
      ViewBag.QAndAType = "User";
      return View("Questions/_ListQuestions", user.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult ListQuestionsForAuthor(PaginationFormModel p) {
      var model = ReaderManager.Instance.GetOwnOrAuthorQuestions(UserName);
      var user = model.Where(m => m.Key == "Author").Select(m => m.Value).AsQueryable();
      ViewBag.QAndAType = "Author";
      return View("Questions/_ListQuestions", user.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult AddQuestion() {
      ViewBag.Themes = new SelectList(ReaderManager.Instance.GetGroupThemes(), "Id", "Name");
      ViewBag.WorkCollection = new SelectList(ReaderManager.Instance.GetWorksForUser(UserName), "Id", "Name");
      return PartialView("Questions/_AddQuestion");
    }

    [HttpPost]
    public JsonResult AddQuestion(QuestionsAnswers qanda, [Bind(Prefix = "Work")] int workId) {
      ReaderManager.Instance.AddQuestion(qanda, workId, UserName);
      return Json(new { msg = ControllerResources.GroupsController_AddQuestion_Question_created });
    }

    public ActionResult AddAnswer(int id) {
      var q = UnitOfWork<ReaderManager>().GetQuestion(id);
      return PartialView("Questions/_AddAnswer", q);
    }

    [HttpPost]
    public JsonResult AddAnswer(QuestionsAnswers qanda, [Bind(Prefix = "Work")] int workId) {
      ReaderManager.Instance.AddQuestion(qanda, workId, UserName);
      return Json(new { msg = ControllerResources.GroupsController_AddAnswer_Answer_added });
    }

    public ActionResult EditQuestion(int id) {
      var q = UnitOfWork<ReaderManager>().GetQuestion(id);
      return PartialView("Questions/_EditQuestion", q);
    }

    [HttpPost]
    public JsonResult EditQuestion(QuestionsAnswers newQandA) {
      ReaderManager.Instance.EditQuestion(newQandA);
      return Json(new { msg = ControllerResources.GroupsController_EditQuestion_Question_changed });
    }

    # endregion

  }

}