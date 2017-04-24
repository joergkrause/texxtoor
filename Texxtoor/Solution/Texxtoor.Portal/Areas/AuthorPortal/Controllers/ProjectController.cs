using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Utilities;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BusinessLayer;
using Texxtoor.BaseLibrary.Core.HtmlAgility.ToXml;
using Texxtoor.DataModels;
using Texxtoor.DataModels.DataAnnotations;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModel.Project;
using Texxtoor.ViewModels.Author;
using Texxtoor.ViewModels.Common;
using Texxtoor.BaseLibrary.WordInterop;

namespace Texxtoor.Portal.Areas.AuthorPortal.Controllers {

  [Authorize]
  public class ProjectController : ControllerExt {

    # region Project Management

    [NavigationPathFilter("Projects")]
    public ActionResult Index() {
      return View();
    }

    public ActionResult ListProjects(PaginationFormModel p, bool deactivated = false) {
      var projects = UnitOfWork<ProjectManager>().GetUsersProjectsWithMembers(UserName, !deactivated).AsQueryable();
      ViewBag.TeamLead = UnitOfWork<ProjectManager>().GetProjectsTeamLeader(projects);
      ViewBag.CurrentUser = Manager<UserManager>.Instance.GetUserByName(UserName);
      ViewBag.ForDeactivated = deactivated;
      return PartialView("Projects/_List", projects.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    /// <summary>
    /// The projects centralized drop zone
    /// </summary>
    /// <returns></returns>
    public ActionResult DropZone(string type) {
      IEnumerable<DropCommand> commands = null;
      switch (type) {
        case "project":
          var typeCommands = typeof(Project).GetCustomAttributes(typeof(DropCommandAttribute), true).Cast<DropCommandAttribute>();
          commands = typeCommands.OrderBy(c => c.Order).Select(c => new DropCommand(c));
          commands.Last().IsLast = true; // Marker to help building UI
          break;
      }
      return PartialView("_DropZone", commands);
    }

    /// <summary>
    /// Form support for new project
    /// </summary>
    /// <param name="advanced">Set to false from SimpleProject wizard to suppress some fields.</param>
    /// <returns></returns>
    public ActionResult AddProject(bool? advanced = true) {
      var teams = new List<SelectListItem> { new SelectListItem { Text = ControllerResources.ProjectController_AddProject___Create_new_Team__, Value = "-1" } };
      // show all teams where this user is teamlead
      teams.AddRange(from team in UnitOfWork<ProjectManager>().GetTeamsWhereUserIsLead(UserName)
                     select new SelectListItem { Text = team.Name, Value = team.Id.ToString() });
      ViewBag.TeamModel = new ProjectDefaultViewModel {
        MyTeams = teams
      };
      var prj = new Project {
        Name = "",
        Active = true,
        ApproveTerms = false,
        Short = "",
        Description = ""
      };
      ViewBag.Advanced = advanced;
      var lead = new TeamMember { Member = UnitOfWork<UserManager>().GetUserByName(UserName) };
      var emptyOpus = new Opus();
      emptyOpus.Milestones = ProjectManager.Instance.CreateDefaultMileStones(lead, emptyOpus);
      ViewBag.DefaultOpusWithMilestones = emptyOpus;
      return PartialView("Projects/_AddProject", prj);
    }

    [HttpPost]
    [NavigationPathFilter("Create Project", Informational = true)]
    public ActionResult AddProject(Project p, HttpPostedFileBase addProjectImage, int? teamId, int? tpl, bool? useMilestones) {
      if (!teamId.HasValue) teamId = -1;
      var prj = UnitOfWork<ProjectManager>().CreateProject(UserName, p.Name, p.Short, p.Description, p.TermsAndConditions, p.ApproveTerms, teamId.Value, Request.Form, useMilestones.GetValueOrDefault());
      if (addProjectImage != null) {
        UnitOfWork<ProjectManager>().SaveProjectImage(prj.Id, addProjectImage, UserName);
      }
      return Json(new { msg = String.Format(ControllerResources.ProjectController_AddProject__0__Project_successfully_created_, p.Name) });
    }

    /// <summary>
    /// Form support for existing project
    /// </summary>
    /// <returns></returns>
    public ActionResult EditProject(int id) {
      var prj = ProjectManager.Instance.GetProject(id, UserName);
      var lead = new TeamMember { Member = UnitOfWork<UserManager>().GetUserByName(UserName) };
      var emptyOpus = new Opus();
      emptyOpus.Milestones = ProjectManager.Instance.CreateDefaultMileStones(lead, emptyOpus);
      ViewBag.DefaultOpusWithMilestones = emptyOpus;
      return PartialView("Projects/_EditProject", prj);
    }

    [HttpPost, ValidateInput(false)]
    public ActionResult EditProject(Project p, HttpPostedFileBase editProjectImage, bool? clearImage) {
      if (clearImage.GetValueOrDefault()) {
        UnitOfWork<ProjectManager>().RemoveProjectImage(p.Id, UserName);
      } else {
        if (editProjectImage != null) {
          UnitOfWork<ProjectManager>().SaveProjectImage(p.Id, editProjectImage, UserName);
        }
      }

      if (UnitOfWork<ProjectManager>().SaveProject(p, UserName)) {
        return Json(new { msg = String.Format("Properties for project {0} saved", p.Name) });
      } else {
        return new HttpNotFoundResult("Could not save changes or nothing changed.");
      }
    }


    [NavigationPathFilter("Deactivated Projects")]
    public ActionResult DeactivatedProjects() {
      return View();
    }

    public ActionResult ListDeactivated(int page) {
      var projects = UnitOfWork<ProjectManager>().GetUsersProjectsWithMembers(UserName, false).ToList();
      // show all deactivated projects where the user is teamlead
      var ledprojs = projects.Where(p => p.Active == false && p.Team.Members.Any(m => m.TeamLead && m.Member.UserName == UserName)).ToList();
      ViewBag.TeamLead = UnitOfWork<ProjectManager>().GetProjectsTeamLeader(projects);
      ViewBag.CurrentUser = UnitOfWork<UserManager>().GetUserByName(UserName);
      ViewBag.ForDeactivated = true;
      return PartialView("Projects/_List", new PagedList<Project>(ledprojs.AsQueryable(), page, 5));
    }

    [NavigationPathFilter("Project Dashboard")]
    public ActionResult Dashboard(int? id) {
      if (id.HasValue) {
        var prj = UnitOfWork<ProjectManager>().GetActiveProjectWithMembers(id.Value, UserName);
        if (prj != null) {
          // get information about contributing type from team member      
          ViewBag.TeamLead = UnitOfWork<ProjectManager>().GetTeamLeader(prj.Team.Id);
          ViewBag.CurrentUser = UnitOfWork<UserManager>().GetUserByName(UserName);
          ViewBag.Closed = false;
          return View(prj);
        }
      }
      return RedirectToAction("Index");
    }

    public ActionResult EditOpus(int id) {
      var project = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      var opus = project.Opuses.OrderBy(o => o.CreatedAt).Last(o => o.Active);
      return RedirectToAction("AuthorRoom", "Editor", new { id = opus.Id });
    }

    public ActionResult DetailsTeam(int id) {
      var prj = UnitOfWork<ProjectManager>().GetActiveProjectWithMembers(id, UserName);
      // calculate available contributing roles, those no longer supported, and those still available
      var allUserRoles = UnitOfWork<UserProfileManager>().GetContributorMatrixOfProjectTeam(prj);
      var assignedRoles = UnitOfWork<ProjectManager>().GetContributorMatrixAssigned(id);
      var rolesWeCanAssign = new Dictionary<string, List<ContributorMatrix>>();
      var rolesNoLongerAvailable = new Dictionary<string, List<ContributorMatrix>>();
      foreach (var item in allUserRoles) {
        // take all from allUserRoles that are not already assigned
        var canAssign = allUserRoles[item.Key].Where(role => !assignedRoles[item.Key].Contains(role)).ToList();
        rolesWeCanAssign.Add(item.Key, canAssign);
        // take all roles currently assigned and NOT available from user
        var notAssign = assignedRoles[item.Key].Where(role => !allUserRoles[item.Key].Contains(role)).ToList();
        rolesNoLongerAvailable.Add(item.Key, notAssign);
      }
      var tom = new TeamOverviewModel {
        Project = prj,
        AllUserRoleMatrix = allUserRoles,
        AssignedRoleMatrix = assignedRoles,
        AvailableRoleMatrix = rolesWeCanAssign,
        RemovedRoleMatrix = rolesNoLongerAvailable,
        Team = prj.Team
      };
      return PartialView("Dashboard/_Team", tom);
    }

    [NavigationPathFilter("Deactivate Project", Informational = true)]
    public ActionResult DeactivateProject(int id) {
      var deleted = UnitOfWork<ProjectManager>().DeactivateProject(id, UserName);
      return Json(new {
        msg = deleted
          ? ControllerResources.ProjectController_Project_Deactivated
          : ControllerResources.ProjectController_Project_NotDeactivated
      }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult ReactivateProject(int id) {
      var activated = UnitOfWork<ProjectManager>().ReactivateProject(id, UserName);
      return Json(new {
        msg = activated
          ? ControllerResources.ProjectController_Project_Activated
          : ControllerResources.ProjectController_Project_NotActivated
      }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteProject(int id) {
      UnitOfWork<ProjectManager>().DeleteProject(id, UserName);
      return Json(new {
        msg = ControllerResources.OpusController_Merge_Text_Ok
      }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult AssignToLeadAuthor(int id) {
      var project = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      return View(project);
    }

    [HttpPost]
    public ActionResult AssignToLeadAuthor(int id, bool keep, int userid) {
      if (UnitOfWork<ProjectManager>().AssignToLeadAuthor(id, keep, UserName, userid)) {
        return RedirectToAction("Index");
      } else {
        var project = UnitOfWork<ProjectManager>().GetProject(id, UserName);
        ViewBag.Error = "Something went wrong";
        return View(project);
      }
    }

    # endregion Project Management

    # region Messages

    public ActionResult CreateMessage(int id) {
      var msg = new WorkroomChat { Project = UnitOfWork<ProjectManager>().GetProject(id, UserName, p => p.Opuses) };
      return View("MessageBoard/_CreateMessage", msg);
    }

    [ValidateInput(false)]
    [HttpPost]
    public JsonResult CreateMessage(WorkroomChat msg, int projectId, int? parentId) {
      UnitOfWork<ProjectManager>().AddWorkroomMessage(msg, projectId, parentId, UserName);
      return Json(new { data = ControllerResources.ProjectController_Message_Created });
    }

    public ActionResult MessageBoard(int id) {
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      return View(prj);
    }

    public ActionResult TopMessage(int id, PaginationFormModel p) {
      // retrieve to level only, we ask for children recursively
      var msg = UnitOfWork<ProjectManager>().GetTopWorkroomMessages(id);
      ViewBag.ProjectId = id;
      return View("MessageBoard/_TopMessage", msg.ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult ChildMessage(IEnumerable<WorkroomChat> msg) {
      return PartialView("MessageBoard/_ChildMessage", msg);
    }

    # endregion

    # region Import

    /// <summary>
    /// Simple and easy upload using HTML files exported by word
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public ActionResult Import(int id) {
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      // currently we have only one "default mapping" for imports, once it exists, we can import
      var importModule = DocumentManager.Instance.LoadImport("Default Mapping", id);
      ViewBag.HasMapping = importModule != null;
      return View(prj);
    }

    public ActionResult ImportMapping(int id, int resourceId) {
      // This is forwarded to the drop downs, consider prepare different sets for particular publishing houses
      var pOptions = DocumentManager.Instance.GetMappingFor("Default Mapping", "P");
      var cOptions = DocumentManager.Instance.GetMappingFor("Default Mapping", "C");
      var nOptions = DocumentManager.Instance.GetMappingFor("Default Mapping", "N");
      ViewBag.ResourceId = resourceId;
      ViewBag.ProjectId = id;
      ViewBag.POptions = pOptions;
      ViewBag.COptions = cOptions;
      ViewBag.NOptions = nOptions;
      var importModule = DocumentManager.Instance.LoadImport("Default Mapping", id) ?? new Import { ProjectId = id, ImportName = "Default Mapping" };
      DocumentManager.Instance.SaveImport(importModule, resourceId, "Default Mapping");
      return View(importModule); // can be NULL on first attempt
    }

    [HttpPost, ValidateInput(false)]
    public ActionResult ImportMapping(int mappingResource) {
      var request = HttpContext.Request.Form;
      var importModule = DocumentManager.Instance.LoadImport("Default Mapping", Int32.Parse(RouteData.Values["id"].ToString()));
      // MapOption-StyleValue-T   [T = P|C|N, can be false or true]
      // MapValue-StyleValue-T    [T = P|C|N, same StyleValue as MapOption, is the selected Value (H1, P, etc.)
      var keys = (from object item in request.Keys select item.ToString()).ToList();
      foreach (var item in keys.Where(k => k.Contains("MapOption"))) {
        var s = item.Split('-');
        if (s.Length != 3) continue;
        var n = s[1]; // the style name from Word document normalized
        var t = s[2]; // The mapping store, such as "P", "C", "N"
        var v = request["MapValue-" + n + "-" + t]; // The value from dropdown list
        if (!String.IsNullOrEmpty(v)) {
          var i = request[item];
          var b = i.Contains(",") ? Boolean.Parse(i.Split(',')[1]) : Boolean.Parse(request[item]); // mapping checkbox
          switch (t) {
            case "P":
              bool p = false;
              if (request["MapSplit-" + n + "-" + t] != null) {
                p = Boolean.Parse(request["MapSplit-" + n + "-" + t]); // split forces later a new fragment for this style
              }
              var y = request["MapType-" + n + "-" + t]; // the type of snippet element we have to create
              DocumentManager.Instance.AssignParaMapping(importModule.ParagraphStylesMap, n, v, b, p, y);
              break;
            case "C":
              DocumentManager.Instance.AssignCharMapping(importModule.CharacterStylesMap, n, v, b);
              break;
            case "N":
              // AssignMapping(importModule.NumberingStylesMap, n, v, b);
              break;
          }
        }
      }
      // The Mapping is stored as a resource
      DocumentManager.Instance.SaveImport(importModule, mappingResource, "Default Mapping");
      ViewBag.POptions = DocumentManager.Instance.GetMappingFor("Default Mapping", "P");
      ViewBag.COptions = DocumentManager.Instance.GetMappingFor("Default Mapping", "C");
      ViewBag.NOptions = DocumentManager.Instance.GetMappingFor("Default Mapping", "N");
      ViewBag.ResourceId = mappingResource;
      return View(importModule);
    }

    # region Simple Import and File Upload

    public ActionResult ListImportFiles(int id, PaginationFormModel p) {
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      var allResFiles = UnitOfWork<ProjectManager>().GetResourceFiles(id, TypeOfResource.Import, null).ToList();
      var resFiles = allResFiles.Where(r => r.FullName.EndsWith("docx") || r.FullName.EndsWith("html")); // only those we can manage here
      ViewBag.HasMapping = allResFiles.Any(f => f.MimeType == "application/x-import");
      foreach (var resFile in resFiles) {
        var blob = BlobFactory.GetBlobStorage(resFile.ResourceId, BlobFactory.Container.Resources);
        resFile.Metadata = blob.MetaData;
      }
      return PartialView("Import/_List", resFiles.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult UploadWord(int id) {
      ViewBag.Type = "word";
      return PartialView("Import/Upload/_UploadWord", id);
    }

    public ActionResult UploadHtml(int id) {
      ViewBag.Type = "html";
      return PartialView("Import/Upload/_UploadHtml", id);
    }

    [HttpPost]
    public JsonResult Upload(int id, HttpPostedFileBase file, string upload) {
      UploadFileInfo f;
      var message = "";
      Guid resId = Guid.Empty;
      switch (upload) {
        case "word":
          resId = UnitOfWork<ProjectManager>().SaveImportFile(id, file, null, UserName);
          message = "Word file successfully uploaded. Proceed with style mapping to convert content.";
          break;
        case "html":
          try {
            switch (Path.GetExtension(file.FileName)) {
              case ".doc":
              case ".docx":
                message = ControllerResources.ProjectController_ImportSimple_DOC_DOCX_is_currently_not_supported;
                break;
              case ".htm":
              case ".html":
                message = ControllerResources.ProjectController_ImportSimple_Could_not_process_this_file_properly;
                resId = UnitOfWork<ProjectManager>().SaveImportFile(id, file, null, UserName);
                break;
              case ".gz":
              case ".zip":
                // check zip for consistency
                file.InputStream.Position = 0;
                var fileData = file.InputStream.ReadToEnd();
                // take all images in the zip and convert to base64 encoded inline images
                var xDoc = ProcessZipFile(fileData);
                using (var memStream = new MemoryStream()) {
                  var writer = XmlWriter.Create(memStream, new XmlWriterSettings(){ Encoding = Encoding.UTF8 });
                  xDoc.Save(writer);
                  writer.Close();
                  memStream.Position = 0;
                  // overwrite filedata with converted HTML
                  fileData = memStream.ToArray();
                  // replace *.zip with *.html
                  var fileName = String.Format("{0}.html", Path.GetFileNameWithoutExtension(file.FileName));
                  // save the converted single file HTML to BLOB store
                  resId = UnitOfWork<ProjectManager>().SaveImportFile(id, fileData, fileName, null, UserName);
                }
                break;
            }
          } catch (Exception ex) {
            message += String.Format(ControllerResources.ProjectController_ImportSimple__br_Internal_Error, ex.Message);
            throw new HttpException(400, message);
          }
          break;
      }
      f = new UploadFileInfo {
        ProjectId = id,
        Name = file.FileName,
        Size = file.ContentLength,
        Folder = "",
        ResourceId = resId
      };
      return Json(new { data = f, msg = message }, "text/html");
    }

    public JsonResult DeleteImportFile(int id, int projectId) {
      var p = UnitOfWork<ProjectManager>().GetProject(projectId, UserName);
      var result = false;
      if (p != null) {
        result = UnitOfWork<ResourceManager>().Delete(id, UserName);
      }
      return Json(new { msg = result ? "File deleted" : "File not found or not accessible" }, JsonRequestBehavior.AllowGet);
    }

    public FileResult DownloadImportFile(int id) {
      var resFile = UnitOfWork<ResourceManager>().GetFile(id);
      var blob = BlobFactory.GetBlobStorage(resFile.ResourceId, BlobFactory.Container.Resources);
      return File(blob.Content, "text/xml", blob["FileName"] + ".xml");
    }

    /// <summary>
    /// Extract the ZIP, take the HTML, convert all images into inline Base64 encoded format to have a single file HTML, then.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="fileData"></param>
    /// <param name="fileName"></param>
    /// <exception cref="InvalidOperationException">Could not process</exception>
    /// <returns></returns>
    private XDocument ProcessZipFile(byte[] fileData) {
      var message = "";
      Func<string, string> encoder = (s) => {
        var iso = Encoding.GetEncoding("ISO-8859-1");
        var utf8 = Encoding.UTF8;
        var utfBytes = utf8.GetBytes(s);
        var isoBytes = Encoding.Convert(utf8, iso, utfBytes);
        var encoding = new UTF8Encoding();
        return encoding.GetString(isoBytes);
      };
      using (var ms = new MemoryStream(fileData)) {
        message = ControllerResources.ProjectController_ImportSimple_Data_read_but_invalid_zip_file;
        try {
          using (var gz = new ZipArchive(ms, ZipArchiveMode.Read)) {
            var entries = gz.Entries;
            message = ControllerResources.ProjectController_ImportSimple_ZIP_loaded_but_entries_invalid;
            var docFile = entries.First(z => Path.GetExtension(z.Name).StartsWith(".htm"));
            // copy resources so we can leave the zip safely
            var resourceFiles = entries.Except(new[] { docFile }).Select(r => {
              var name = r.FullName;
              var imageStream = new MemoryStream();
              r.Open().CopyTo(imageStream);
              var content = imageStream.ToArray();
              return new {
                Name = name,
                Content = content
              };
            }).ToList();
            Html2XmlUtil.imageAsBase64 = true;
            Html2XmlUtil.TreatExternalData += (sender, args) => {
              var images = resourceFiles.Where(f => f.Name.EndsWith(args.FileName)).ToList();
              if (!images.Any()) return;
              if (images.Count() > 1) {
                images = resourceFiles.Where(f => f.Name == Path.Combine(args.FilePath, args.FileName)).ToList();
              }
              var image = images.First();
              try {
                using (var imageStream = new MemoryStream(image.Content)) {
                  var img = System.Drawing.Image.FromStream(imageStream);
                  using (var pngImgMs = new MemoryStream()) {
                    img.Save(pngImgMs, ImageFormat.Png);
                    pngImgMs.Position = 0;
                    args.Data = pngImgMs.ToArray();
                  }
                }
              } catch (Exception) {
                args.Data = null;
              }
            };
            var result = new MemoryStream();
            docFile.Open().CopyTo(result);
            if (result.Length == 0) {
              message = "The zip archive did not contain a valid html-file or has unexpected structure.";
            }
            string html;
            var bytes = result.ToArray();
            // unclear, even ISO docs do well in UTF8 
            html = Encoding.UTF8.GetString(bytes); // : Encoding.GetEncoding(1252).GetString(bytes);
            var xDoc = Html2XmlUtil.CleanUpHtmlWithResources(html); // make clean XHTML with embedded images
            return xDoc;
          }
        } catch (Exception ex) {
          message += ex.Message;
        }
      }
      // should never come here for regular files
      throw new InvalidOperationException(message);
    }

    private static bool IsUtf8(byte[] bytes, int offset = 0, int? length = null) {
      if (bytes == null) {
        throw new ArgumentNullException("bytes");
      }
      length = length ?? (bytes.Length - offset);
      if (offset < 0 || offset > bytes.Length) {
        throw new ArgumentOutOfRangeException("offset", "Offset is out of range.");
      } else if (length < 0) {
        throw new ArgumentOutOfRangeException("length");
      } else if ((offset + length) > bytes.Length) {
        throw new ArgumentOutOfRangeException("offset", "The specified range is outside of the specified buffer.");
      }
      var bytesRemaining = length.Value;
      while (bytesRemaining > 0) {
        var rank = GetUtf8MultibyteRank(bytes, offset, Math.Min(4, bytesRemaining));
        if (rank == MultibyteRank.None) {
          return false;
        } else {
          var charsRead = (int)rank;
          offset += charsRead;
          bytesRemaining -= charsRead;
        }
      }
      return true;
    }

    /// <summary>
    /// Determines whether the bytes in this buffer at the specified offset represent a UTF-8 multi-byte character.
    /// </summary>
    /// <remarks>
    /// It is not guaranteed that these bytes represent a sensical character - only that the binary pattern matches UTF-8 encoding.
    /// </remarks>
    /// <param name="bytes">This buffer.</param>
    /// <param name="offset">The position in the buffer to check.</param>
    /// <param name="length">The number of bytes to check, of 4 if not specified.</param>
    /// <returns>The rank of the UTF</returns>
    private static MultibyteRank GetUtf8MultibyteRank(byte[] bytes, int offset = 0, int length = 4) {
      if (bytes == null) {
        throw new ArgumentNullException("bytes");
      }
      if (offset < 0 || offset > bytes.Length) {
        throw new ArgumentOutOfRangeException("offset", "Offset is out of range.");
      } else if (length < 0 || length > 4) {
        throw new ArgumentOutOfRangeException("length", "Only values 1-4 are valid.");
      } else if ((offset + length) > bytes.Length) {
        throw new ArgumentOutOfRangeException("offset", "The specified range is outside of the specified buffer.");
      }
      // Possible 4 byte sequence
      if (length > 3 && IsLead4(bytes[offset])) {
        if (IsExtendedByte(bytes[offset + 1]) && IsExtendedByte(bytes[offset + 2]) && IsExtendedByte(bytes[offset + 3])) {
          return MultibyteRank.Four;
        }
      }
        // Possible 3 byte sequence
      else if (length > 2 && IsLead3(bytes[offset])) {
        if (IsExtendedByte(bytes[offset + 1]) && IsExtendedByte(bytes[offset + 2])) {
          return MultibyteRank.Three;
        }
      }
        // Possible 2 byte sequence
      else if (length > 1 && IsLead2(bytes[offset]) && IsExtendedByte(bytes[offset + 1])) {
        return MultibyteRank.Two;
      }
      if (bytes[offset] < 0x80) {
        return MultibyteRank.One;
      } else {
        return MultibyteRank.None;
      }
    }

    private static bool IsLead4(byte b) {
      return b >= 0xF0 && b < 0xF8;
    }

    private static bool IsLead3(byte b) {
      return b >= 0xE0 && b < 0xF0;
    }

    private static bool IsLead2(byte b) {
      return b >= 0xC0 && b < 0xE0;
    }

    private static bool IsExtendedByte(byte b) {
      return b > 0x80 && b < 0xC0;
    }

    private enum MultibyteRank {
      None = 0,
      One = 1,
      Two = 2,
      Three = 3,
      Four = 4
    }

    public JsonResult ImportSingleHtml(int id, int projectId, bool newOpus) {
      var resFile = UnitOfWork<ResourceManager>().GetFile(id);
      var blob = BlobFactory.GetBlobStorage(resFile.ResourceId, BlobFactory.Container.Resources);
      var html = Encoding.UTF8.GetString(blob.Content);
      var name = resFile.Name;
      var message = ControllerResources.ProjectController_ImportSimple_ZIP_file_has_been_processed__Return_to_Dashboard_to_view_results;
      try {
        // convert prepared HTML into internal <Content> XML (backup and restore format)
        var mapping = blob["Mapping"] as Import;
        var parameters = new System.Collections.Specialized.NameValueCollection();
        if (mapping != null) {
          mapping.CharacterStyles.ForEach(c => parameters.Add(c.Key, c.Value));
          mapping.ParagraphStyles.ForEach(c => parameters.Add(c.Key, c.Value));
          mapping.NumberingStyles.ForEach(c => parameters.Add(c.Key, c.Value));
        }
        var xml = Html2XmlUtil.HtmlToOpusXsltParser(html, parameters);
        using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xml))) {
          message = ControllerResources.ProjectController_ProcessSingleHtml_ZIP_file_extracted_but_content_not_convertable;
          var xDoc = XDocument.Load(xmlStream);
          Opus opus = null;
          if (blob.MetaData.ContainsKey("ImportToOpus") && !newOpus) {
            opus = UnitOfWork<ProjectManager>().GetOpus(Convert.ToInt32(blob["ImportToOpus"]), UserName);
          } else {
            opus = UnitOfWork<ProjectManager>().CreateOpusForProject(projectId, Path.GetFileNameWithoutExtension(name), null, null);
          }
          // use restore to create import
          UnitOfWork<ProjectManager>().RestoreOpusFromFile(opus.Id, xDoc, UserName);
          // register the import information with the uploaded and converted HTML
          blob["ImportToOpus"] = opus.Id;
          blob["ImportToOpusName"] = opus.Name;
          blob.Save();
          message = ControllerResources.ProjectController_ProcessSingleHtml_ZIP_processed_and_content_written_to_project;
          return Json(new {
            msg = message,
            name = opus.Name
          }, JsonRequestBehavior.AllowGet);
        }
      } catch (Exception ex) {
        message += ex.Message;
      }
      return Json(new {
        msg = message
      }, JsonRequestBehavior.AllowGet);
      //message = ControllerResources.ProjectController_ImportSimple_ZIP_file_extracted_but_couldn_t_parse;
    }

    public FileResult GetRestorableXml(int id) {
      try {
        string fileName;
        var xml = GetXmlForImportHtml(id, out fileName);
        return File(xml, "text/xml", fileName + ".xml");
      } catch (Exception ex) {
      }
      return null;
    }

    /// <summary>
    /// Just a viewer of the single file HTML for users convenience
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ContentResult PreviewSingleHtml(int id) {
      try {
        var resFile = UnitOfWork<ResourceManager>().GetFile(id);
        var blob = BlobFactory.GetBlobStorage(resFile.ResourceId, BlobFactory.Container.Resources);
        var html = Encoding.UTF8.GetString(blob.Content);
        return Content(html);
      } catch (Exception ex) {
        throw new HttpException(400, ex.Message);
      }
    }

    private byte[] GetXmlForImportHtml(int id, out string fileName) {
      var resFile = UnitOfWork<ResourceManager>().GetFile(id);
      var blob = BlobFactory.GetBlobStorage(resFile.ResourceId, BlobFactory.Container.Resources);
      var html = Encoding.UTF8.GetString(blob.Content);
      // check mapping
      if (!blob.MetaData.ContainsKey("Mapping")) {
        throw new ArgumentOutOfRangeException("no mapping");
      }
      var mapping = HttpUtility.ParseQueryString(blob["Mapping"].ToString());
      // combine multi styles (textPara1,2,3)
      mapping.Add("textPara", String.Format("{0}|{1}|{2}", mapping["textPara1"], mapping["textPara2"], mapping["textPara3"]));
      mapping.Remove("textPara1");
      mapping.Remove("textPara2");
      mapping.Remove("textPara3");
      // convert prepared HTML into internal <Content> XML (backup and restore format)
      var xml = Html2XmlUtil.HtmlToOpusXsltParser(html, mapping);
      using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xml))) {
        var xDoc = XDocument.Load(xmlStream);
        using (var stream = new MemoryStream()) {
          var writer = XmlWriter.Create(stream);
          xDoc.WriteTo(writer);
          writer.Flush();
          var bytes = stream.ToArray();
          fileName = blob["FileName"].ToString();
          return bytes;
        }
      }
    }

    /// <summary>
    /// THe mapper is a dialog that let the user map internal to word styles.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult MapHtmlStyles(int id) {
      ViewBag.FileId = id;
      var resFile = UnitOfWork<ResourceManager>().GetFile(id);
      var blob = BlobFactory.GetBlobStorage(resFile.ResourceId, BlobFactory.Container.Resources);
      var mapping = blob.MetaData.ContainsKey("Mapping") ? blob["Mapping"].ToString() : "";
      ViewBag.FormData = HttpUtility.ParseQueryString(mapping);
      var html = Encoding.UTF8.GetString(blob.Content);
      ViewBag.Error = String.Empty;
      try {
        var xml = Html2XmlUtil.HtmlToXDoc(html, "");
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(html));
        stream.Position = 0;
        var xDoc = XDocument.Load(stream);
        var classes = xDoc.Root.Descendants()
          .Where(e => e.Attribute("class") != null)
          .Distinct(new ClassComparer())
          .Select(e => new SelectListItem {
            Value = e.Attribute("class").Value,
            Text = e.Attribute("class").Value
          }).ToList();
        return PartialView("Import/Simple/_MapStyles", classes);
      } catch (Exception ex) {
        ViewBag.Error = ex.Message;
        return PartialView("Import/Simple/_MapStyles", null);
      }
    }

    class ClassComparer : IEqualityComparer<XElement> {

      public bool Equals(XElement x, XElement y) {
        if (x.Attribute("class") != null && y.Attribute("class") != null) {
          return x.Attribute("class").Value == y.Attribute("class").Value;
        }
        return false;
      }

      public int GetHashCode(XElement obj) {
        return 0;
      }
    }

    public JsonResult MapStyles(int id) {
      var resFile = UnitOfWork<ResourceManager>().GetFile(id);
      var blob = BlobFactory.GetBlobStorage(resFile.ResourceId, BlobFactory.Container.Resources);
      blob["Mapping"] = Request.Form.ToString();
      blob.Save();
      return Json(new { msg = "Mapping stored" });
    }

    # endregion Import

    public ActionResult ImportMapStyles(int id, int resourceId) {
      try {
        var importModule = DocumentManager.Instance.LoadImport("Default Mapping", id);
        var res = UnitOfWork<ResourceManager>().GetFile(resourceId);
        var b = BlobFactory.GetBlobStorage(res.ResourceId, BlobFactory.Container.Resources);
        importModule.ExtractAllStyles(b.Content);
        ViewBag.StyleCount = importModule.CharacterStyles.Count() + importModule.ParagraphStyles.Count() + importModule.NumberingStyles.Count();
        ViewBag.MapCount = importModule.CharacterStylesMap.Count() + importModule.ParagraphStylesMap.Count() + importModule.NumberingStylesMap.Count();
        DocumentManager.Instance.SaveImport(importModule, resourceId, "Default Mapping");
      } catch (Exception) {
        ViewBag.FileCount = -1;
        ViewBag.StyleCount = -1;
        ViewBag.MapCount = -1;
      }
      return PartialView("Import/Complex/_ImportMapStyles", id);
    }

    public ActionResult ImportFileSelection(int id) {
      return PartialView("Import/Complex/_ImportFileSelection", id);
    }

    public ActionResult SaveProjectFromImport(int id) {
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      ViewBag.ItemMissed = new List<string>();
      ViewBag.ItemProcessed = new List<string>();
      ViewBag.ImportFolders = DocumentManager.Instance.GetImportFolders();
      return View(prj);
    }

    [HttpPost]
    public ActionResult SaveProjectFromImport(int id, int? opusId, string[] resourceFile, bool importOverwrite, bool splitOpus) {
      List<string> itemsMissed, itemsProcessed;
      DocumentManager.Instance.SaveImportToProject(id, opusId, importOverwrite, splitOpus, out itemsMissed, out itemsProcessed);
      var importResult = new ImportResult {
        ItemsMissed = itemsMissed,
        ItemsProcessed = itemsProcessed,
        OpusId = opusId.GetValueOrDefault()
      };
      return PartialView("Import/Complex/_ImportResult", importResult);
    }

    public ActionResult Preview(int id, int opusId) {
      var res = UnitOfWork<ProjectManager>().GetOpusElementsForPreview(id, opusId);
      return PartialView("Import/Complex/_Preview", res);
    }


    # endregion

    public ActionResult MoveOpus(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      var projects = UnitOfWork<ProjectManager>().GetUsersProjectsWithMembers(UserName, true).ToList();
      ViewBag.OpusId = opus.Id;
      ViewBag.ProjectId = opus.Project.Id;
      ViewBag.OpusName = opus.Name;
      return View(projects);
    }

    [HttpPost]
    public ActionResult MoveOpus(int id, int projectId) {

      var result = UnitOfWork<ProjectManager>().MoveOpusToProject(id, projectId, true, UserName);
      if (result) {
        return RedirectToAction("Dashboard", new { id = projectId });
      } else {
        return RedirectToAction("MoveOpus", id);
      }
    }

  }
}
