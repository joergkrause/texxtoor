using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Utilities;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Common;
using Texxtoor.ViewModels.Portal.Author;

namespace Texxtoor.Portal.Areas.AuthorPortal.Controllers {

  [Authorize]
  public class OpusController : ControllerExt {


    # region Opus Management
    public ActionResult Edit(int? id) {
      if (id.HasValue) {
        var opus = UnitOfWork<ProjectManager>().GetOpus(id.Value, UserName, o => o.Project, o => o.RequirementsResource, o => o.ProposedOutcomeResource);
        var teamMembers = UnitOfWork<ProjectManager>().GetTeamMembersOfOpus(opus).ToList();
        var isMember = teamMembers.FirstOrDefault(tm => tm.Member.UserName == UserName);
        if (isMember != null) {
          // this panel shows the editing options for the current user
          // 1. filter by user's contributing types
          // 2. filter by required roles in this particular project
          // 3. show in the view the tools available
          ViewBag.ContributorRole = isMember.Role.ContributorRoles;
          ViewBag.ContributorRoles = isMember.GetLocalizedContributorRoles();
          // 4. show all other members but the current
          ViewBag.TeamMembers = teamMembers.Except(teamMembers.Where(t => t.Member.UserName == UserName)).ToList();
          ViewBag.UserIsTeamlead = teamMembers.Any(t => t.TeamLead && t.Member.UserName == UserName);
          ViewBag.UserId = isMember.Member.Id;
          return View(opus);
        }
      }
      return RedirectToAction("ShowAll");
    }

    [HttpPost]
    public ActionResult Edit(int id, Opus opus,
      [Bind(Prefix = "Resource.RequirementsResource")]
      int? requirementsResource,
      [Bind(Prefix = "Resource.ProposedOutcomeResource")]
      int? proposedOutcomeResource) {
      ValidateModel(opus);
      if (ModelState.IsValid) {
        ProjectManager.Instance.EditOpus(id, opus.Name, opus.Version, opus.LocaleId, opus.Variation, opus.IsBoilerplate, opus.Requirements, opus.TargetAudience, opus.ProposedOutcome, requirementsResource, proposedOutcomeResource, true, null);
        return Json(new { msg = ControllerResources.OpusController_Edit_Text_information_saved });
      }
      return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ControllerResources.OpusController_Edit_An_error_occured_sending_data__Please_check_the_form_);
    }

    public ActionResult Restore(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpusWithTeam(id);
      return View(opus);
    }

    public ActionResult Repair(int id) {
      var opus = UnitOfWork<ProjectManager>().RepairEmptyDocument(id);
      return RedirectToAction("Edit", new { id = opus.Id });
    }


    [HttpPost]
    public PartialViewResult Restore(int id, HttpPostedFileBase file) {
      if (file == null || file.InputStream == null) {
        return PartialView("Restore/_Error", "File empty");
      }
      if (!file.FileName.EndsWith(".xml")) {
        return PartialView("Restore/_Error", "File must have extension *.xml");
      }
      try {
        var ms = new MemoryStream(file.InputStream.ReadToEnd());
        var xDoc = XDocument.Load(ms);
        UnitOfWork<ProjectManager>().RestoreOpusFromFile(id, xDoc, UserName);
        return PartialView("Restore/_Success");

      } catch (Exception ex) {
        return PartialView("Restore/_Error", "Error while importing file: " + ex.Message);
      }
    }

    public ActionResult ListHistory(int id, PaginationFormModel p) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      var history = UnitOfWork<ProjectManager>().GetResourceFiles(opus, TypeOfResource.Project, "application/opus-xml");
      return PartialView("Restore/_HistoryList", history.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public JsonResult RestoreFromHistory(int id, int resourceId) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      var history = UnitOfWork<ProjectManager>().GetResourceFiles(opus, TypeOfResource.Project, "application/opus-xml").First(r => r.Id == resourceId);
      using (var blob = BlobFactory.GetBlobStorage(history.ResourceId, BlobFactory.Container.Resources)) {
        var ms = new MemoryStream(blob.Content);
        var xDoc = XDocument.Load(ms);
        UnitOfWork<ProjectManager>().RestoreOpusFromFile(id, xDoc, UserName);
      }
      return Json(new { msg = ControllerResources.OpusController_Document_Restored });
    }

    public ActionResult DownloadHistory(int id) {
      var history = UnitOfWork<ProjectManager>().GetResource(id);
      using (var blob = BlobFactory.GetBlobStorage(history.ResourceId, BlobFactory.Container.Resources)) {
        if (blob.Content != null && blob.Content.Length > 0) {
          return File(blob.Content, "application/opus-xml", history.FullName);
        }
        return new EmptyResult();
      }
    }

    public JsonResult DeleteHistory(int id) {
      var history = UnitOfWork<ProjectManager>().GetResource(id);
      ResourceManager.Instance.Delete(history.ResourceId);
      return Json(new { msg = ControllerResources.OpusController_Document_HistoryDeleted }, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// Action show either active or closed Opus' from given project.
    /// </summary>
    /// <param name="id">Project Id</param>
    /// <param name="p"> </param>
    /// <param name="closed">Option to set search condition</param>
    /// <returns></returns>
    public ActionResult List(int id, PaginationFormModel p, bool? closed) {
      var prj = ProjectManager.Instance.GetProject(id, UserName);
      if (prj == null) {
        return RedirectToAction("Index", "Project");
      }
      var cls = closed.GetValueOrDefault(); // either give explicit value or show all
      var ops = prj.Opuses.Where(o => o.Active != cls).OrderByDescending(o => o.CreatedAt);
      ViewBag.Closed = cls;
      ViewBag.ProjectId = id;
      ViewBag.UserIsLead = prj.Team.TeamLead.UserName == UserName;
      return PartialView("Opus/_List", ops.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult ShowAll() {
      var ops = ProjectManager.Instance.GetAllOpusForUser(UserName, false);
      return View(ops.Any());
    }

    public ActionResult ListAll(PaginationFormModel p, bool? closed) {
      var cls = closed.GetValueOrDefault();
      var ops = UnitOfWork<ProjectManager>().GetAllOpusForUser(UserName, cls);
      ViewBag.Closed = cls;
      ViewBag.UserIsLead = false; // can't know for all, so we disable the new button here
      return PartialView("Opus/_List", ops.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult CreateProjectAndText() {
      var team = UnitOfWork<ProjectManager>().GetTeamsWhereUserIsLead(UserName).FirstOrDefault() ?? UnitOfWork<ProjectManager>().CreateTeam(
        String.Format(ControllerResources.OpusController_CreateFromTemplateSimple__0_s_Team, UserName),
        String.Format(ControllerResources.OpusController_CreateFromTemplateSimple__0_s_Team_for_a_simple_project, UserName),
        UnitOfWork<UserProfileManager>().GetCurrentUser(UserName),
        false);
      var template = new NameValueCollection();
      template.Add("tpl", "1");
      template.Add("tpl-1-chapters", "2");
      var prj = UnitOfWork<ProjectManager>().CreateProject(UserName,
        ControllerResources.OpusController_CreateFromTemplateSimple_Simple_Project,
        ControllerResources.OpusController_CreateFromTemplateSimple_A_simple_project_created_implicitly,
        "", "", false, team.Id, template, true);
      var newOpus = new Opus {
        Project = prj,
        Version = 0,
        Active = true,
        LocaleId = CurrentCulture,
      };
      var lead = prj.Team.Members.First(m => m.TeamLead);
      var mstn = UnitOfWork<ProjectManager>().CreateDefaultMileStones(lead, newOpus);
      newOpus.Milestones = mstn;
      return View("CreateFromTemplate", newOpus);
    }

    public ActionResult CreateFromTemplate(int id) {
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      var newOpus = new Opus {
        Project = prj,
        Version = 0,
        Active = true,
        LocaleId = CurrentCulture,
      };
      var lead = prj.Team.Members.First(m => m.TeamLead);
      var mstn = UnitOfWork<ProjectManager>().CreateDefaultMileStones(lead, newOpus);
      newOpus.Milestones = mstn;
      return View("CreateFromTemplate", newOpus);
    }

    [HttpPost]
    public ActionResult CreateFromTemplate() {
      // read all values from form
      var values = ControllerContext.RequestContext.HttpContext.Request.Form;
      var newOpus = ProjectManager.Instance.CreateOpusFromTemplate(values, UserName);
      return RedirectToAction("Edit", new { id = newOpus.Id });
    }

    public ActionResult TargetAudience(int id) {
      var opus = ProjectManager.Instance.GetOpus(id, UserName, o => o.MatchMatrix);
      var mm = opus.MatchMatrix;
      return PartialView("Opus/_TargetAudience", mm);
    }

    [HttpPost]
    public JsonResult TargetAudienceAdd(int id) {
      ProjectManager.Instance.AddMatchMatrixToElement(id);
      return Json(new { msg = ControllerResources.OpusController_Audience_Added });
    }

    [HttpPost]
    public JsonResult TargetAudienceEdit(int id, int[] matrix, string[] keywords, int[] targets, int[] stages) {
      if (matrix == null || keywords == null || targets == null || stages == null) {
        throw new HttpException(400, "No proper form values");
      }
      ProjectManager.Instance.SaveMatchMatrixToElement(id, matrix, keywords, targets, stages);
      return Json(new { msg = ControllerResources.OpusController_Audience_Changed });
    }

    [HttpPost]
    public ActionResult TargetAudienceRemove(int id, int matrix) {
      ProjectManager.Instance.RemoveMatchMatrixFromElement(matrix);
      return Json(new { msg = ControllerResources.OpusController_Audience_Removed });
    }

    # endregion

    # region Milestone Managament

    public ActionResult FinishMilestone(int id) {
      var opus = ProjectManager.Instance.GetOpus(id, UserName);
      return View(opus);
    }

    public ActionResult ManageMilestones(int id) {
      var opus = ProjectManager.Instance.GetOpus(id, UserName);
      return View(opus);
    }

    public ActionResult ListMilestones(int id, PaginationFormModel p) {
      var mstn = ProjectManager.Instance.GetMileStonesOfOpus(id);
      // this parameter changes the presentation (the method is called twice)
      ViewBag.OpusId = id;
      ViewBag.UserId = UnitOfWork<UserManager>().GetUserByName(UserName).Id;
      MilestoneProgress(mstn);
      return PartialView("Milestones/_ListOpusMilestones", mstn.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult ListMilestonesSimple(int id) {
      var mstn = ProjectManager.Instance.GetMileStonesOfOpus(id);
      // this parameter changes the presentation (the method is called twice)
      ViewBag.OpusId = id;
      ViewBag.UserId = UnitOfWork<UserManager>().GetUserByName(UserName).Id;
      MilestoneProgress(mstn);
      return PartialView("Milestones/_ListOpusMilestonesSimple", mstn.ToPagedList(0, 100));
    }

    public ActionResult ListMilestonesForFinish(int id, PaginationFormModel p) {
      var mstn = ProjectManager.Instance.GetMileStonesOfOpus(id);
      // this parameter changes the presentation (the method is called twice)
      ViewBag.OpusId = id;
      ViewBag.TeamLeadId = UnitOfWork<ProjectManager>().GetTeamLeadForOpus(id, UserName).TeamLead.Id;
      ViewBag.UserId = UnitOfWork<UserManager>().GetUserByName(UserName).Id;
      MilestoneProgress(mstn);
      return PartialView("Milestones/_ListOpusMilestonesForFinish", mstn.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult ListMilestonesForManager(int id) {
      var mstn = ProjectManager.Instance.GetMileStonesOfOpus(id).ToList();
      var hasNext = mstn.Where(m => m.NextMilestone != null).ToList();
      var isNext = mstn.Where(m => hasNext.Any(i => i.NextMilestone == m)).ToList();
      var connectors = isNext.Union(hasNext).Distinct().ToList();
      var model = mstn.Where(m => m.NextMilestone == null).Except(connectors).AsQueryable();
      // this parameter changes the presentation (the method is called twice)
      ViewBag.OpusId = id;
      ViewBag.UserId = UnitOfWork<UserManager>().GetUserByName(UserName).Id;
      ViewBag.TeamLeadId = UnitOfWork<ProjectManager>().GetTeamLeadForOpus(id, UserName).TeamLead.Id;
      return PartialView("Milestones/_ListOpusMilestonesForManager", model.ToPagedList(0, 100));
    }

    public ActionResult ListMilestonesForManagerChained(int id) {
      var mstn = ProjectManager.Instance.GetMileStonesOfOpus(id).ToList();
      var hasNext = mstn.Where(m => m.NextMilestone != null).ToList();
      var isNext = mstn.Where(m => hasNext.Any(i => i.NextMilestone == m)).ToList();
      var chainStart = hasNext.Except(isNext);
      var stackedChain = new List<Stack<Milestone>>();
      foreach (var milestone in chainStart) {
        var m = milestone;
        var s = new Stack<Milestone>();
        do {
          s.Push(m);
          m = m.NextMilestone;
        } while (m != null);
        s = new Stack<Milestone>(s); // reverse
        stackedChain.Add(s);
      }
      // this parameter changes the presentation (the method is called twice)
      ViewBag.OpusId = id;
      ViewBag.UserId = UnitOfWork<UserManager>().GetUserByName(UserName).Id;
      ViewBag.TeamLeadId = UnitOfWork<ProjectManager>().GetTeamLeadForOpus(id, UserName).TeamLead.Id;
      return PartialView("Milestones/_ListOpusMilestonesForManagerChained", stackedChain);
    }

    public JsonResult MoveMilestone(int id, string dir) {
      var d = String.IsNullOrEmpty(dir) ? "d" : ((dir == "d") ? "d" : "u");
      ProjectManager.Instance.MoveMileStoneOrder(id, d);
      return Json(new { msg = ControllerResources.OpusController_Milestone_Moved });
    }

    [HttpPost]
    public JsonResult ChangeMilestone(int id, string comment, int progress) {
      progress = progress > 100 ? 100 : progress;
      ProjectManager.Instance.ChangeMileStone(id, comment, progress);
      return Json(new { msg = ControllerResources.OpusController_Milestone_Changed });
    }

    public ActionResult AddMileStone(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      var lead = UnitOfWork<ProjectManager>().GetTeamLeaderAsMember(opus.Project.Team.Id);
      var mstn = new Milestone { Opus = opus, DateDue = DateTime.Now.AddDays(30), Owner = lead };
      ViewBag.TeamMembers = new SelectList(UnitOfWork<ProjectManager>().GetTeamMembersOfOpus(opus), "Id", "Member.UserName");
      ViewBag.Milestones = new SelectList(UnitOfWork<ProjectManager>().GetMileStonesOfOpus(opus.Id), "Id", "Name");
      return PartialView("Milestones/_AddMilestone", mstn);
    }

    [HttpPost]
    public JsonResult AddMileStone(int id, Milestone mstn, [Bind(Prefix = "NextMilestone.NextMilestone")] int? nextMileStone, int Owner) {
      if (!String.IsNullOrEmpty(mstn.Name)) {
        UnitOfWork<ProjectManager>().CreateMilestone(id, mstn, nextMileStone.GetValueOrDefault(), Owner);
        return Json(new { msg = ControllerResources.OpusController_Milestone_Added });
      }
      return Json(new { msg = ControllerResources.OpusController_Milestone_CannotAdd_NameInvalid });
    }

    public ActionResult EditMileStone(int id) {
      var mstn = UnitOfWork<ProjectManager>().GetMilestone(id);
      var opus = mstn.Opus;
      ViewBag.TeamMembers = new SelectList(UnitOfWork<ProjectManager>().GetTeamMembersOfOpus(opus), "Id",
                                           "Member.UserName");
      // exclude parent and self (mstn)
      var all = UnitOfWork<ProjectManager>().GetMileStonesOfOpus(opus.Id).ToList();
      var exceptions = new List<Milestone>();
      // exclude self
      exceptions.Add(mstn);
      // exclude parent to avoid circular reference
      var parent = all.SingleOrDefault(m => m.NextMilestone != null && m.NextMilestone.Id == id);
      if (parent != null) {
        exceptions.Add(parent);
      }
      var available = all.Except(exceptions);
      ViewBag.Milestones = new SelectList(available, "Id", "Name", mstn.NextMilestone != null ? mstn.NextMilestone.Id : 0);
      return PartialView("Milestones/_EditMilestone", mstn);
    }

    [HttpPost]
    public JsonResult EditMileStone(int id, Milestone mstn, [Bind(Prefix = "NextMilestone.NextMilestone")] int? nextMileStone, int Owner) {
      if (!String.IsNullOrEmpty(mstn.Name)) {
        var next = nextMileStone.GetValueOrDefault();
        UnitOfWork<ProjectManager>().EditMilestone(id, mstn, next, Owner);
        return Json(new { msg = ControllerResources.OpusController_Milestone_Edited });
      }
      return Json(new { msg = ControllerResources.OpusController_Milestone_Edited_InvalidName });
    }

    [HttpPost]
    public JsonResult DeleteMileStone(int id) {
      if (UnitOfWork<ProjectManager>().DeleteMilestone(id, UserName)) {
        return Json(new { msg = ControllerResources.OpusController_Milestone_Deleted });
      } else {
        return Json(new { msg = ControllerResources.OpusController_Milestone_DeleteError });
      }
    }

    private void MilestoneProgress(IEnumerable<Milestone> mstn) {
      const string myTheme = @"<Chart BackColor=""Transparent"">
                        <ChartAreas>
                           <ChartArea Name=""Default"" BackColor=""Wheat""></ChartArea>
                        </ChartAreas>
                     </Chart>";
      foreach (var milestone in mstn) {
        var cnt = 0;
        var data = new Dictionary<string, int>();
        if (!String.IsNullOrEmpty(milestone.Comment)) {
          var fields = milestone.Comment.Split('|');
          foreach (var field in fields) {
            var values = field.Split('^');
            if (values.Length != 4) {
              continue;
            }
            ++cnt;
            data.Add(cnt.ToString(), Int32.Parse(values[3]));
          }
        }
        var chart = new Chart(100, 100, myTheme);

        chart.AddSeries("Progress", "Line", xValue: data.Keys, yValues: data.Values);
        chart.SaveToCache("Progress-" + milestone.Id);
      }
    }

    public ActionResult ProgressChart(int id) {
      return File(Chart.GetFromCache("Progress-" + id.ToString()).GetBytes("png"), "image/png");
    }

    # endregion

    #region -= Tree Helper =-
    [HttpPost]
    public JsonResult GetTreeData(int documentId) {
      var opus = ProjectManager.Instance.GetOpus(documentId, UserName);

      var tree = (new List<Opus> { opus }).RecursiveSelect<Element, JsTreeModel>(
        c => c.Children.OrderBy(e => e.OrderNr),
        (e, c) => new JsTreeModel {
          data = e.Name,
          attr = new JsTreeAttribute {
            id = e.Id.ToString(),
            rel = (e is Snippet) ? "snippet" : ((e is Section) ? "section" : "opus")
          },
          children = c.ToArray()
        });

      return Json(tree);
    }

    #endregion

    # region Tools

    public ActionResult Open(int id) {
      var opus = ProjectManager.Instance.GetAndActivateOpus(id, true);
      return View("ShowClosed", opus.Project);
    }

    public ActionResult Close(int id) {
      var opus = ProjectManager.Instance.GetAndActivateOpus(id, false);
      return View("ShowClosed", opus.Project);
    }

    public ActionResult ShowClosed(int id) {
      var prj = ProjectManager.Instance.GetProject(id, UserName);
      return View(prj);
    }

    public ActionResult CreateFrom(int id) {
      var opus = ProjectManager.Instance.GetOpus(id, UserName);
      return View("CreateFrom", opus);
    }

    [HttpPost]
    public JsonResult CreateFrom(int id, string name, int version, [Bind(Prefix = "Culture")] string localeId, bool isBoilerplate, VariationType variation, bool? copyContent, bool? useMilestones) {
      var form = Request.Form;
      if (!String.IsNullOrEmpty(name)) {
        var milestones = new List<Milestone>();
        // copy always without milestones and add selected milestones later
        var opus = ProjectManager.Instance.CreateOpusFromExisting(id, copyContent.GetValueOrDefault(), false);
        if (useMilestones.GetValueOrDefault()) {
          // read the old text's milestones as a template for the new copy
          milestones.AddRange(from milestone in UnitOfWork<ProjectManager>().GetMileStonesOfOpus(id)
                              where form[String.Format("Opus.Milestones[{0}].Active", milestone.Id)] != null && Boolean.Parse(form[String.Format("Opus.Milestones[{0}].Active", milestone.Id)])
                              let teamMember = Int32.Parse(form[String.Format("Opus.Milestones[{0}].Owner", milestone.Id)])
                              select new Milestone {
                                Name = milestone.Name,
                                Owner = UnitOfWork<ProjectManager>().GetTeamMember(teamMember),
                                DateDue = DateTime.Parse(form[String.Format("Opus.Milestones[{0}].DateDue", milestone.Id)]),
                                Progress = 0,
                                Description = milestone.Description,
                                DateAssigned = DateTime.Now,
                                Opus = opus
                              });
        }
        if (String.IsNullOrEmpty(localeId)) {
          localeId = CurrentCulture;
        }
        UnitOfWork<ProjectManager>().EditOpus(opus.Id, name, version, localeId, variation, isBoilerplate, useMilestones.GetValueOrDefault(), milestones);
        return Json(new { msg = "OK" });
      }
      return Json(new { msg = "Error" });
    }

    public ActionResult ChangeOpusVariation(int id, VariationType variation) {
      var opus = UnitOfWork<ProjectManager>().ChangeOpusVariation(id, variation, UserName);
      return RedirectToAction("Edit", new { id = id });
    }

    public ActionResult MergeWith(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      return View(opus);
    }

    public ActionResult MergeBoilerplates(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      return View(opus);
    }


    /// <summary>
    /// Calls view to let user merge any chapters of the current project's active opuses into the selected one.
    /// </summary>
    /// <param name="id">Target opus' id</param>
    /// <returns></returns>
    public PartialViewResult MergeWithContent(int id) {
      var targetOpus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      var sourceOpuses = targetOpus.Project.Opuses.Where(o => o.Active).Except(new[] { targetOpus });
      ViewBag.WithBoilerplates = false;
      return PartialView("Opus/_MergeWithContent", new Tuple<IEnumerable<Opus>, Opus>(sourceOpuses, targetOpus));
    }

    /// <summary>
    /// Calls view to let user merge any chapters of the current project's active opuses into the selected one.
    /// </summary>
    /// <param name="id">Target opus' id</param>
    /// <returns></returns>
    public PartialViewResult MergeWithBoilerplate(int id) {
      var targetOpus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      var projects = UnitOfWork<ProjectManager>().GetProjectsWhereUserIsMember(UserName)
        .ToList()
        .Where(p => p.Team.TeamLead.UserName == UserName);
      var sourceOpuses = projects
        .SelectMany(p => p.Opuses)
        .Where(o => o.Active)
        .Except(new[] { targetOpus })
        .ToList() // need to get because next condition is not supported by EF
        .Where(o => o.IsBoilerplate)
        .ToList();
      ViewBag.WithBoilerplates = true;
      return PartialView("Opus/_MergeWithContent", new Tuple<IEnumerable<Opus>, Opus>(sourceOpuses, targetOpus));
    }

    [HttpPost]
    public JsonResult SaveMergedContent(int id, int[] fragmentIds) {
      UnitOfWork<ProjectManager>().MergeChaptersToOpus(id, fragmentIds, UserName);
      return Json(new { msg = ControllerResources.OpusController_Merge_Text_Ok });
    }

    [HttpPost]
    public ActionResult SaveMergedBoilerplate(int id, IEnumerable<int> fragmentIds) {
      var result = UnitOfWork<ProjectManager>().CopyChaptersToOpus(id, fragmentIds.ToList(), UserName);
      if (result) {
        return Json(new { msg = ControllerResources.OpusController_Merge_Text_Ok });
      }
      return new HttpNotFoundResult("Could not add boilerplate. Check document and chapter order.");
    }


    # endregion

    # region Publishing Preset

    public ActionResult PublishingPreset(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName, o => o.Published);
      var publ = opus.Published ?? new Published();
      var p = new PublishingPreset {
        Title = publ.Title,
        Description = publ.ExternalPublisher.Description,
        SubTitle = publ.SubTitle,
        Keywords = publ.ExternalPublisher.Keywords,
        NavLevel = publ.NavLevel,
        SourceOpus = opus,
        SupportedMedia = null,
        Publisher = "texxtoor"
      };
      ViewBag.View = false;
      return PartialView("Opus/_PublishingPreset", p);
    }

    public ActionResult PublishingPresetView(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName, o => o.Published);
      var publ = opus.Published ?? new Published();
      ViewBag.View = true;
      var p = new PublishingPreset {
        Title = publ.Title,
        Description = publ.ExternalPublisher.Description,
        SubTitle = publ.SubTitle,
        Keywords = publ.ExternalPublisher.Keywords,
        NavLevel = publ.NavLevel,
        SourceOpus = opus,
        SupportedMedia = null,
        Publisher = "texxtoor"
      };
      return PartialView("Opus/_PublishingPreset", p);
    }

    [HttpPost]
    public ActionResult PublishingPreset(int id, PublishingPreset publ, string keywords) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName, o => o.Published);
      publ.SourceOpus = opus; // assure connection
      var p = opus.Published ?? new Published();
      p.Title = publ.Title;
      p.SubTitle = publ.SubTitle;
      p.ExternalPublisher.Description = publ.Description;
      p.Publisher = publ.Publisher;
      p.NavLevel = publ.NavLevel;
      p.SourceOpus = opus;
      p.ExternalPublisher.Keywords = keywords;      
      UnitOfWork<ProjectManager>().SavePublished(p);
      return Json(new { msg = "OK" });
    }


    # endregion

  }
}
