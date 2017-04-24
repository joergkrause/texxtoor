using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Web.Mvc;
using Texxtoor.BaseLibrary;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.Portal.Core.Extensions;
using System.Web;
using System.IO;
using System.Data.Entity.Validation;
using System.Xml.Linq;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {

  /// <summary>
  /// This is a per tenant approach.
  /// </summary>
  [Authorize(Roles = "Admin,TenantAdmin")]
  public class TemplateController : ControllerExt {


    public ActionResult Index(int? id) {
      var publicTemplates = !id.HasValue;
      var tenantId = id.GetValueOrDefault();
      var templates = AdminDb.TemplateGroups.Where(t => t.Owner.Id == tenantId || (t.Owner == null && publicTemplates));
      ViewBag.CurrentTab = 0;
      return View(templates);
    }

    public ActionResult WordTemplates(int? id) {
      TemplateGroup template = null;
      if (id.HasValue) {
        template = AdminDb.TemplateGroups.Find(id.Value);
      }
      return View("Word/WordTemplates", template);
    }

    public FileResult DownloadTemplate(int id) {
      var t = AdminDb.Templates.Find(id);
      return File(t.Content, "application/file", t.InternalName);
    }

    # region EPUB

    public ActionResult EditEpubTemplate(int? id) {
      Template template = null;
      if (id.HasValue) {
        template = AdminDb.Templates.Find(id.Value);
      }
      ViewBag.Users = new SelectList(AdminDb.Users, "Id", "UserName");
      ViewBag.CurrentTab = 1;
      return View("Epub/EditEpubTemplate", template);
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult EditEpubTemplate(int? id, int templateGroupId, string name, string content) {
      Template template = null;
      var msg = "";
      var err = false;
      try {
        if (id.HasValue) {
          template = AdminDb.Templates.Find(id);
          template.Content = Encoding.UTF8.GetBytes(content);
          template.InternalName = name;
        }
        else {
          var tt = AdminDb.TemplateGroups.Find(templateGroupId);
          template = new Template {
            InternalName = name,
            Content = Encoding.UTF8.GetBytes(content),
            Group = tt
          };
          tt.Templates.Add(template);
        }
        AdminDb.SaveChanges();
        msg = "Content Saved";
      } catch (DbEntityValidationException dbEx) {
        var error = dbEx.EntityValidationErrors.SelectMany(validationErrors => validationErrors.ValidationErrors).Aggregate("", (current, validationError) => current + string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
        err = true;
        msg = "Internal Error, content not yet saved properly. Please repeat. (" + error + ")";
      } catch (Exception ex) {
        msg = "Internal Error, content not yet saved properly. Please repeat. (" + ex.Message + ")";
        err = true;
      }
      ViewBag.CurrentTab = 1;
      return Json(new { msg, err });
    }

    public ActionResult CreateEpubGroup() {
      return View("Epub/CreateEpubGroup");
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult CreateEpubGroup(string name) {
      ViewBag.CurrentTab = 1;
      if (ModelState.IsValid && name != "Common Template") {
        var tt = AdminDb.TemplateGroups.Where(t => t.Name == name && t.Group == GroupKind.Epub);
        if (!tt.Any()) {
          AdminDb.TemplateGroups.Add(new TemplateGroup {
            Name = name,
            Admin = AdminDb.Users.Single(u => u.UserName == UserName),
            LocaleId = CurrentCulture,
            Group = GroupKind.Epub
          });
          AdminDb.SaveChanges();
          ViewBag.CurrentTab = 1;
        } else {
          ModelState.AddModelError("name", "Name not allowed or already in use");
        }
        return RedirectToAction("Index");
      }
      return View("Epub/CreateEpubGroup");
    }

    public ActionResult UploadEpubTemplate(int id, string name, string group) {
      ViewBag.Id = id;
      ViewBag.Name = name;
      ViewBag.Group = group;
      return View("Epub/UploadEpubTemplate");
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult UploadEpubTemplate(int? templateId, int templateGroupId, HttpPostedFileBase epubTemplate) {
      if (epubTemplate == null || epubTemplate.InputStream == null || epubTemplate.InputStream.Length == 0) {
        ModelState.AddModelError("epubTemplate", "You must choose a file, and the file must not be empty.");
      } else {
        var ms = new MemoryStream();
        epubTemplate.InputStream.CopyTo(ms);
        if (templateId.HasValue) {
          var template = AdminDb.Templates.Single(t => t.Id == templateId.Value);
          template.Content = ms.ToArray();
        } else {
          AdminDb.Templates.Add(new Template {
            InternalName = epubTemplate.FileName,
            Group = AdminDb.TemplateGroups.Find(templateGroupId),
            Content = ms.ToArray()
          });
        }
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      ViewBag.CurrentTab = 1;
      return View("Epub/UploadEpubTemplate");
    }

    public ActionResult CopyEpubTemplateGroup(int id) {
      CopyTemplateGroup(id, GroupKind.Epub);
      ViewBag.CurrentTab = 1;
      return RedirectToAction("Index");
    }

    public ActionResult DeleteEpubTemplateGroup(int id) {
      var tt = AdminDb.TemplateGroups.Find(id);
      if (tt != null && !tt.IsCommonTemplate) {
        AdminDb.TemplateGroups.Remove(tt);
        AdminDb.SaveChanges();
      }
      ViewBag.CurrentTab = 1;
      return RedirectToAction("Index");
    }

    public ActionResult DeleteEPubTemplate(int id) {
      ViewBag.CurrentTab = 1;
      return View("Epub/DeleteEpubTemplate", AdminDb.Templates.Find(id));
    }

    [HttpPost]
    [ActionName("DeleteEpubTemplate")]
    public ActionResult DeleteEPubTemplateInternal(int id) {
      var t = AdminDb.Templates.Find(id);
      if (t != null) {
        AdminDb.Templates.Remove(t);
        AdminDb.SaveChanges();
      }
      ViewBag.CurrentTab = 1;
      return RedirectToAction("Index");
    }

    public ActionResult RenameEpubTemplateGroup(string name) {
      ViewBag.OldName = name;
      ViewBag.CurrentTab = 1;
      return View("Epub/RenameEpubTemplateGroup");
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult RenameEpubTemplateGroup(int id, string name) {
      var tt = AdminDb.TemplateGroups.Find(id);
      ViewBag.CurrentTab = 1;
      if (ModelState.IsValid && tt != null && !tt.IsCommonTemplate) {
        tt.Name = name;
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      return View("Epub/RenameEpubTemplateGroup");
    }

    # endregion

    # region PDF

    public ActionResult EditPdfTemplate(int? id) {
      Template template = null;
      if (id.HasValue) {
        template = AdminDb.Templates.Find(id.Value);
      }
      ViewBag.CurrentTab = 0;
      ViewBag.Users = new SelectList(AdminDb.Users, "Id", "UserName");
      return View("Pdf/EditPdfTemplate", template);
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult EditPdfTemplate(int? id, int templateGroupId, string name, string content) {
      Template template = null;
      try {
        if (id.HasValue) {
          template = AdminDb.Templates.Find(id);
          template.Content = Encoding.UTF8.GetBytes(content);
          template.InternalName = name;
        } else {
          var tt = AdminDb.TemplateGroups.Find(templateGroupId);
          template = new Template {
            InternalName = name,
            Content = Encoding.UTF8.GetBytes(content),
            Group = tt
          };
          tt.Templates.Add(template);
        }
        AdminDb.SaveChanges();
        ViewBag.Message = "Content Saved";
      } catch (DbEntityValidationException dbEx) {
        var error = dbEx.EntityValidationErrors.SelectMany(validationErrors => validationErrors.ValidationErrors).Aggregate("", (current, validationError) => current + string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
        ViewBag.Message = "Internal Error, content not yet saved properly. Please repeat. (" + error + ")";
      } catch (System.Exception ex) {
        ViewBag.Message = "Internal Error, content not yet saved properly. Please repeat. (" + ex.Message + ")";
      }
      ViewBag.CurrentTab = 0;
      return View("Pdf/EditPdfTemplate", template);
    }

    public ActionResult CreatePdfGroup() {
      return View("Pdf/CreatePdfGroup");
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult CreatePdfGroup(string name) {
      ViewBag.CurrentTab = 0;
      if (ModelState.IsValid && name != "Common Template") {
        var tt = AdminDb.TemplateGroups.Where(t => t.Name == name && t.Group == GroupKind.Pdf);
        if (!tt.Any()) {
          AdminDb.TemplateGroups.Add(new TemplateGroup {
            Name = name,
            Admin = AdminDb.Users.Single(u => u.UserName == UserName),
            LocaleId = CurrentCulture,
            Group = GroupKind.Pdf
          });
          AdminDb.SaveChanges();
        } else {
          ModelState.AddModelError("name", "Name not allowed or already in use");
        }
        return RedirectToAction("Index");
      }
      return View("Pdf/CreatePdfGroup");
    }

    public ActionResult UploadPdfTemplate(int id, string name, string group) {
      ViewBag.Id = id;
      ViewBag.Name = name;
      ViewBag.Group = group;
      return View("Pdf/UploadPdfTemplate");
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult UploadPdfTemplate(int? templateId, int templateGroupId, HttpPostedFileBase pdfTemplate) {
      if (pdfTemplate == null || pdfTemplate.InputStream == null || pdfTemplate.InputStream.Length == 0) {
        ModelState.AddModelError("pdfTemplate", "You must choose a file, and the file must not be empty.");
      } else {
        var ms = new MemoryStream();
        pdfTemplate.InputStream.CopyTo(ms);
        if (templateId.HasValue) {
          var template = AdminDb.Templates.Single(t => t.Id == templateId.Value);
          template.Content = ms.ToArray();
        } else {
          var tt = AdminDb.TemplateGroups.Find(templateGroupId);
          tt.Templates.Add(new Template {
            InternalName = pdfTemplate.FileName,
            Content = ms.ToArray()
          });
        }
        AdminDb.SaveChanges();
        return RedirectToAction("Index");
      }
      ViewBag.CurrentTab = 0;
      return View("Pdf/UploadPdfTemplate");
    }

    public ActionResult CopyPdfTemplateGroup(int id) {
      CopyTemplateGroup(id, GroupKind.Pdf);
      ViewBag.CurrentTab = 0;
      return RedirectToAction("Index");
    }

    public ActionResult DeletePdfTemplateGroup(int id) {
      var tt = AdminDb.TemplateGroups.Find(id);
      if (tt != null && !tt.IsCommonTemplate) {
        AdminDb.TemplateGroups.Remove(tt);
        AdminDb.SaveChanges();
      }
      ViewBag.CurrentTab = 0;
      return RedirectToAction("Index");
    }

    public ActionResult DeletePdfTemplate(int id) {
      return View("Pdf/DeletePdfTemplate", AdminDb.Templates.Find(id));
    }

    [HttpPost]
    [ActionName("DeletePdfTemplate")]
    public ActionResult DeletePdfTemplateInternal(int id) {
      var t = AdminDb.Templates.Find(id);
      if (t != null) {
        AdminDb.Templates.Remove(t);
        AdminDb.SaveChanges();
      }
      ViewBag.CurrentTab = 0;
      return RedirectToAction("Index");
    }

    public ActionResult RenamePdfTemplateGroup(string name) {
      ViewBag.OldName = name;
      ViewBag.CurrentTab = 0;
      return View("Pdf/RenamePdfTemplateGroup");
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult RenamePdfTemplateGroup(int id, string name) {
      var tt = AdminDb.TemplateGroups.Find(id);
      if (ModelState.IsValid && tt != null && !tt.IsCommonTemplate) {
        if (tt.Name != name && AdminDb.TemplateGroups.Any(t => t.Name == name && t.Group == GroupKind.Pdf)) {
          ModelState.AddModelError("Name", "Cannot rename, the name is not valid or already in use");
        } else {
          tt.Name = name;
          AdminDb.SaveChanges();
          return RedirectToAction("Index");
        }
      }
      ViewBag.CurrentTab = 0;
      return View("Pdf/RenamePdfTemplateGroup");
    }

    # endregion

    # region Mail

    public ActionResult EditMailTemplate(int? id) {
      Template template = null;
      if (id.HasValue) {
        template = AdminDb.Templates.Find(id.Value);
      }
      ViewBag.Users = new SelectList(AdminDb.Users, "Id", "UserName");
      ViewBag.CurrentTab = 1;
      return View("Mail/EditMailTemplate", template);
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult EditMailTemplate(int id, string content) {
      Template template = null;
      try {
        template = AdminDb.Templates.Find(id);
        // content must be an XML fragment
        var xCheck = XElement.Load(new MemoryStream(Encoding.UTF8.GetBytes("<root>" + content + "</root>")));
        template.Content = Encoding.UTF8.GetBytes(String.Join(Environment.NewLine, xCheck.Elements().Select(e => e.ToString()).ToArray()));
        var group = AdminDb.TemplateGroups.Find(template.Group.Id);
        template.Group = group;
        AdminDb.SaveChanges();
        ViewBag.Error = false;
        ViewBag.Message = "Content Saved";
      } catch (DbEntityValidationException dbEx) {
        var error = dbEx.EntityValidationErrors.SelectMany(validationErrors => validationErrors.ValidationErrors).Aggregate("", (current, validationError) => current + string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
        ViewBag.Error = true;
        ViewBag.Message = "Internal Error, content not yet saved properly. Please repeat. (" + error + ")";
      } catch (System.Exception ex) {
        ViewBag.Error = true;
        ViewBag.Message = "Internal Error, content not yet saved properly. Please repeat. (" + ex.Message + ")";
      }
      ViewBag.CurrentTab = 1;
      return View("Mail/EditMailTemplate", template);
    }

    public ActionResult CreateMailTemplate(string name) {
      // mail is in one group for the common platform, we can use groups to make mail templates tenant aware
      var template = AdminDb.Templates.FirstOrDefault(t => t.InternalName == name);
      return View("Mail/CreateMailTemplate", template);
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult CreateMailTemplate(string name, string localeId) {
      var user = AdminDb.Users.Single(u => u.UserName == UserName);
      // check if locale already exists
      var template = AdminDb.Templates.FirstOrDefault(t => t.InternalName == name && t.Group.LocaleId == localeId && t.Group.Group == GroupKind.Email);
      if (template != null) {
        return View("Mail/CreateMailTemplate", template);
      }
      // take one to duplicate
      var tt = AdminDb.TemplateGroups.Single(t => t.Name == "texxtoor" && t.LocaleId == localeId);
      var newTemplate = new Template {
        InternalName = name,
        Group = tt,
      };
      tt.Templates.Add(newTemplate);
      AdminDb.SaveChanges();
      ViewBag.CurrentTab = 3;
      return RedirectToAction("EditMailTemplate", newTemplate);
    }

    public ActionResult DeleteMailTemplate(int id) {
      ViewBag.CurrentTab = 3;
      return View("Mail/DeleteMailTemplate", AdminDb.Templates.Find(id));
    }

    [HttpPost]
    [ActionName("DeleteMailTemplate")]
    public ActionResult DeleteMailTemplateInternal(int id) {
      var t = AdminDb.Templates.Find(id);
      AdminDb.Templates.Remove(t);
      AdminDb.SaveChanges();
      ViewBag.CurrentTab = 3;
      return RedirectToAction("Index");
    }

    # endregion

    private void CopyTemplateGroup(int id, GroupKind groupKind) {
      var tt = AdminDb.TemplateGroups.Find(id);
      var c = 1;
      var name = tt.Name;
      string n;
      do {
        if (name.Substring(name.Length - 2, 1) == "_") {
          Int32.TryParse(name.Substring(name.Length - 1, 1), out c);
        }
        n = String.Format("{0}_{1}", name, c++);
      } while (AdminDb.TemplateGroups.Any(t => t.Name == n && t.Group == groupKind));
      c--;
      var newGroup = new TemplateGroup();
      tt.CopyProperties<TemplateGroup>(newGroup,
        t => t.Admin,
        t => t.Name,
        t => t.Group,
        t => t.LocaleId,
        t => t.Owner);
      AdminDb.Templates.AddRange(tt.Templates.Select(t => {
        t.Id = 0;
        return t;
      }));
      AdminDb.SaveChanges();
    }


  }
}
