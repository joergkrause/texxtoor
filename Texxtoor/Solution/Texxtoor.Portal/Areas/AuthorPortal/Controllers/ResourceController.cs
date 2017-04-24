using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AForge.Imaging.Filters;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Common;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using AForge;
using AForge.Imaging;
using Rectangle = System.Drawing.Rectangle;
using Texxtoor.BaseLibrary.Core.Extensions.ActionResults;

namespace Texxtoor.Portal.Areas.AuthorPortal.Controllers {

  [Authorize]
  public class ResourceController : ControllerExt {

    # region Common

    [NavigationPathFilter("Resources")]
    public ActionResult Index(int id) {
      // this is the project id
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
#if DEBUG
      ViewBag.IsDebug = true;
#else
      ViewBag.IsDebug = false;
#endif
      return View(prj);
    }

    public ActionResult CleanUpResources(int id) {
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      var rsm = UnitOfWork<ResourceManager>();
      var resIds = prj.Resources.OfType<ResourceFile>().Select(r => r.ResourceId).ToList();
      foreach (var resId in resIds) {
        using (var blob = BlobFactory.GetBlobStorage(resId, BlobFactory.Container.Resources)) {
          if (blob.Content == null || blob.Content.Length == 0) {
            rsm.Delete(resId);
          }
        }
      }
      return RedirectToAction("Index", new { id = id });
    }

    # endregion

    # region Resources

    public ActionResult LoadResourceTab(int id, string tab) {
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      switch (tab) {
        case "default-content":
          return PartialView("Finder/_Content", prj.Resources.Where(r => r.TypesOfResource == TypeOfResource.Content).ToList());
        case "project-content":
          var projectFiles = prj.Resources.Where(r => r.TypesOfResource == TypeOfResource.Project);
          foreach (var projectFile in projectFiles.OfType<ResourceFile>()) {
            UnitOfWork<ResourceManager>().Ctx.LoadProperty(projectFile, r => r.Published);
          }
          return PartialView("Finder/_Project", prj.Resources.Where(r => r.TypesOfResource == TypeOfResource.Project).ToList());
        case "import-content":
          return PartialView("Finder/_Import", prj.Resources.Where(r => r.TypesOfResource == TypeOfResource.Import).ToList());
        case "trash-content":
          return PartialView("Finder/_Trash", prj.Resources.Where(r => r.TypesOfResource == TypeOfResource.Trash).ToList());
      }
      throw new ArgumentOutOfRangeException();
    }
    public ActionResult LoadOrderedResources(int id, string tab, string dir) {
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      if (dir == "up") {
        switch (tab) {
          case "default-content":
            return PartialView("Finder/_Content", prj.Resources.Where(r => r.TypesOfResource == TypeOfResource.Content).OrderBy(r => r.Name).ToList());
          case "project-content":
            var projectFiles = prj.Resources.Where(r => r.TypesOfResource == TypeOfResource.Project);
            foreach (var projectFile in projectFiles.OfType<ResourceFile>()) {
              UnitOfWork<ResourceManager>().Ctx.LoadProperty(projectFile, r => r.Published);
            }
            return PartialView("Finder/_Project", projectFiles.OrderBy(r => r.Name).ToList());
        }
      } else {
        switch (tab) {
          case "default-content":
            return PartialView("Finder/_Content", prj.Resources.Where(r => r.TypesOfResource == TypeOfResource.Content).OrderByDescending(r => r.Name).ToList());
          case "project-content":
            var projectFiles = prj.Resources.Where(r => r.TypesOfResource == TypeOfResource.Project);
            foreach (var projectFile in projectFiles.OfType<ResourceFile>()) {
              UnitOfWork<ResourceManager>().Ctx.LoadProperty(projectFile, r => r.Published);
            }
            return PartialView("Finder/_Project", projectFiles.OrderByDescending(r => r.Name).ToList());
        }
      }
      throw new ArgumentOutOfRangeException();
    }
    public FileResult GetResource(int id) {
      var file = UnitOfWork<ResourceManager>().GetFile(id, UserName);
      using (var blob = BlobFactory.GetBlobStorage(file.ResourceId, BlobFactory.Container.Resources)) {
        var name = file.Name;
        // add an extension, because internally the name is the caption in case of figures
        if (String.IsNullOrEmpty(Path.GetExtension(name)) && file.MimeType.Contains("/")) {
          name = String.Format("{0}.{1}", name, file.MimeType.Split('/')[1]);
        }
        return File(blob.Content, file.MimeType, name);
      }
    }


    [HttpPost]
    public JsonResult ImageCropManipulation(int id, int height, int width, double xpos, double ypos) {
      var file = UnitOfWork<ResourceManager>().GetFile(id, UserName);
      using (var blob = BlobFactory.GetBlobStorage(file.ResourceId, BlobFactory.Container.Resources)) {
        // save current state in the undo object
        blob.AddOrUpdateMetaData("Undo", blob.Content);
        // manipulate
        var imgTest = ByteArrayToImage(blob.Content);
        var cropcords = new Rectangle(Convert.ToInt32(xpos), Convert.ToInt32(ypos), width, height);
        var bmp = new Bitmap(cropcords.Width, cropcords.Height, imgTest.PixelFormat);
        bmp.SetResolution(imgTest.HorizontalResolution, imgTest.VerticalResolution);
        var Graphic = Graphics.FromImage(bmp);
        Graphic.DrawImage(imgTest, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), cropcords, GraphicsUnit.Pixel);
        using (var ms = new MemoryStream()) {
          bmp.Save(ms, imgTest.RawFormat);
          blob.Content = ms.ToArray();
          blob.Save();
        }
      }
      return Json(new { msg = "Ok" });

    }
    /// <summary>
    /// Image manipulation using AForge
    /// </summary>
    /// <param name="imgId">image id</param>
    /// <param name="option">selected type of manipulation</param>
    /// <returns>manipulated Image</returns>
    [HttpPost]
    public JsonResult ImageColorManipulation(int imgId, string option) {
      var file = UnitOfWork<ResourceManager>().GetFile(imgId, UserName);
      using (var blob = BlobFactory.GetBlobStorage(file.ResourceId, BlobFactory.Container.Resources)) {
        // save current state in the undo object
        blob.AddOrUpdateMetaData("Undo", blob.Content);
        // manipulate
        var blop = blob.Content;
        var imgTest = ByteArrayToImage(blop);
        using (var ms = new MemoryStream()) {
          var imgNew = ManipulateImageColor(option, imgTest);
          imgNew.Save(ms, ImageFormat.Png);
          blob.Content = ms.ToArray();
          blob.Save();
        }
      }
      return Json(new { msg = "Ok" });
    }

    private System.Drawing.Image ManipulateImageColor(string option, System.Drawing.Image tempImage) {
      switch (option) {
        case "Sepia":
          var spSepia = new Sepia();
          return spSepia.Apply((Bitmap)tempImage);
        case "Red":
          var cfChannelFilteringRed = new ChannelFiltering(new IntRange(0, 255), new IntRange(0, 0), new IntRange(0, 0));
          return cfChannelFilteringRed.Apply((Bitmap)tempImage);
        case "Yellow":
          var cfChannelFilteringYellow = new ChannelFiltering(new IntRange(0, 255), new IntRange(0, 255), new IntRange(0, 0));
          return cfChannelFilteringYellow.Apply((Bitmap)tempImage);
        case "Magenta":
          var cfChannelFilteringMegenta = new ChannelFiltering(new IntRange(0, 255), new IntRange(0, 0), new IntRange(0, 255));
          return cfChannelFilteringMegenta.Apply((Bitmap)tempImage);
        case "Cyon":
          var cfChannelFilteringCyon = new ChannelFiltering(new IntRange(0, 0), new IntRange(0, 255), new IntRange(0, 255));
          return cfChannelFilteringCyon.Apply((Bitmap)tempImage);
        case "Green":
          var cfChannelFilteringGreen = new ChannelFiltering(new IntRange(0, 0), new IntRange(0, 255), new IntRange(0, 0));
          return cfChannelFilteringGreen.Apply((Bitmap)tempImage);
        case "Blue":
          var cfChannelFilteringBlue = new ChannelFiltering(new IntRange(0, 0), new IntRange(0, 0), new IntRange(0, 255));
          return cfChannelFilteringBlue.Apply((Bitmap)tempImage);
        default:
          return tempImage;
      }
    }

    [HttpPost]
    public JsonResult ImageBrightSatHueManipulation(int imgId, int saturation, int brightness) {
      var file = UnitOfWork<ResourceManager>().GetFile(imgId, UserName);
      using (var blob = BlobFactory.GetBlobStorage(file.ResourceId, BlobFactory.Container.Resources)) {
        // save current state in the undo object
        blob.AddOrUpdateMetaData("Undo", blob.Content);
        // manipulate
        var imgTest = ByteArrayToImage(blob.Content);
        System.Drawing.Image imgNew;
        //imgNew = ManipulateImage(option, tempImage);
        var bcBrightnessCorrection = new BrightnessCorrection();
        bcBrightnessCorrection.AdjustValue = brightness;
        imgTest = bcBrightnessCorrection.Apply((Bitmap) imgTest);
        var scSaturationCorrection = new SaturationCorrection();
        scSaturationCorrection.AdjustValue = saturation;
        imgNew = scSaturationCorrection.Apply((Bitmap) imgTest);
        using (var ms = new MemoryStream()) {
          imgNew.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
          blob.Content = ms.ToArray();
          blob.Save();
        }
      }
      return Json(new { msg = "Ok" });
    }

    private System.Drawing.Image ByteArrayToImage(byte[] byteArrayIn) {
      try {
        using (var ms = new MemoryStream(byteArrayIn)) {
          var returnImage = System.Drawing.Image.FromStream(ms, true);
          return returnImage;
        }
      } catch {
        return null;
      }
    }

    /// <summary>
    /// To get the original saved Image Back
    /// </summary>
    /// <param name="imgId">Image Id</param>
    /// <returns>JSON result</returns>
    [HttpPost]
    public JsonResult RevertBackToOriginal(int imgId) {
      var file = UnitOfWork<ResourceManager>().GetFile(imgId, UserName);
      using (var blob = BlobFactory.GetBlobStorage(file.ResourceId, BlobFactory.Container.Resources)) {
        if (blob.MetaData.ContainsKey("Undo")) {
          blob.Content = (byte[]) blob.MetaData["Undo"];
        }
        blob.Save();
      }
      return Json(new { msg = "Ok" });
    }

    [HttpPost]
    public JsonResult UploadResourcePortal(int id, string volume, string label, HttpPostedFileBase file) {
      var type = (TypeOfResource)Enum.Parse(typeof(TypeOfResource), volume, true);
      UnitOfWork<ResourceManager>().AddResource(id, type, label, file, UserName);
      return Json(new { msg = "Ok", fileName = file.FileName });
    }

    [HttpPost]
    public ActionResult DeleteResource(int id) {
      if (ResourceManager.Instance.Delete(id, UserName)) {
        return Json(new { msg = "Resource deleted" });
      } else {
        return Json(new { msg = "Resource not deleted: Not found or you're not the owner!" });
      }
    }

    [HttpPost]
    public ActionResult RenameResource(int id, string volume, string name, string label) {
      var type = (TypeOfResource)Enum.Parse(typeof(TypeOfResource), volume, true);
      if (ResourceManager.Instance.RenameResource(id, type, name, label, UserName)) {
        return Json(new { msg = "Ok" });
      } else {
        return Json(new { msg = "Error" });
      }
    }

    /// <summary>
    /// when user select the duplicate option, an ajax call will be made to this action
    /// </summary>
    /// <param name="projID"> the ID of the project</param>
    /// <param name="volume"> current tab </param>
    /// <param name="label"> name of the resource</param>
    /// <returns> creates a new resource with a different name and different resource ID</returns>
    [HttpPost]
    public JsonResult DuplicateResource(int projID, string volume, string label) {
      var id = Convert.ToInt32((Request.Params.GetValues(3)[0]));
      var file = ResourceManager.Instance.GetFile(id, UserName);
      var oldName = file.Name;
      using (var blob = BlobFactory.GetBlobStorage(file.ResourceId, BlobFactory.Container.Resources)) {
        var type = (TypeOfResource) Enum.Parse(typeof (TypeOfResource), volume, true);
        var allFiles = ResourceManager.Instance.GetAllFilesByProjectId(projID);
        var count = 0;
        var newName = "";
        foreach (var item in allFiles) {
          if (item.Name != null) {
            if (item.Name.Split('(')[0] == oldName.Split('(')[0] || item.Name == oldName) {
              count++;
            }
          }
          else {
          }
        }
        if (oldName.Split('(')[0] == null) {
          newName = oldName + "(" + count + ")";
        }
        else if (oldName.Split('(')[0] != null) {
          newName = oldName.Split('(')[0] + "(" + count + ")";

        }
        UnitOfWork<ResourceManager>().DuplicateResources(id, projID, type, label, blob.Content, UserName, newName, file.ResourceId, file.MimeType);
      }
      return Json(new { msg = "OK" });

    }

    public ActionResult EmptyTrash(int id) {
      var result = UnitOfWork<ResourceManager>().EmptyVolume(id, TypeOfResource.Trash, UserName);
      return Json(new {msg = result ? ControllerResources.ResourceController_EmptyTrash_Files_deleted : ControllerResources.ResourceController_EmptyTrash_No_files_deleted}, JsonRequestBehavior.AllowGet);
    }


    /// <summary>
    /// will move the selected resource to the other tab 
    /// </summary>
    /// <param name="id">id of the selected record</param>
    /// <param name="volume">tab of the record</param>
    /// <param name="label">name of the resource</param>
    /// <returns>Moves the resource to the other tab</returns>
    [HttpPost]
    public ActionResult MoveResource(int id, string volume, string label) {
      var type = (TypeOfResource)Enum.Parse(typeof(TypeOfResource), volume, true);
      UnitOfWork<ResourceManager>().MoveFile(id, type, label, UserName);
      return Json(new { msg = "moved to " + volume });
    }

    [HttpPost]
    public ActionResult LabelResource(int id, string volume, string label) {
      var type = (TypeOfResource)Enum.Parse(typeof(TypeOfResource), volume, true);
      UnitOfWork<ResourceManager>().MoveFile(id, type, label, UserName);
      return Json(new { msg = "moved to " + label });
    }

    public ActionResult ImageData(int id) {
      var file = UnitOfWork<ResourceManager>().GetFile(id);
      using (var blob = BlobFactory.GetBlobStorage(file.ResourceId, BlobFactory.Container.Resources)) {
        if (blob.Content != null) {
          using (var s = new MemoryStream(blob.Content)) {
            var img = Bitmap.FromStream(s);
            return Json(new {
              w = img.Width,
              h = img.Height,
              dpih = img.HorizontalResolution,
              dpiv = img.VerticalResolution,
              warn = img.VerticalResolution < 300F ? "Resolution may not satisfy print" : "No, looks perfect",
              px = Enum.GetName(typeof (PixelFormat), img.PixelFormat)
            }, JsonRequestBehavior.AllowGet);
          }
        }
        else {
          return null;
        }
      }
    }

    public ImageResult RawImage(int id) {
      var file = UnitOfWork<ResourceManager>().GetFile(id);
      using (var blob = BlobFactory.GetBlobStorage(file.ResourceId, BlobFactory.Container.Resources)) {
        return new ImageResult(blob.Content, "image/png");
      }
    }

    # endregion

    # region TermSets

    private readonly Func<TermType, string> _localizeEnum = e => {
      var disp =
        typeof(TermType).GetField(Enum.GetName(typeof(TermType), e))
                         .GetCustomAttributes(true)
                         .OfType<DisplayAttribute>()
                         .Single();
      return disp.ResourceType.GetProperty(disp.Name).GetValue(null).ToString();
    };


    [NavigationPathFilter("Semantic Data")]
    public ActionResult Termsets(int id) {
      var prj = ProjectManager.Instance.GetProject(id, UserName);
      // need: { 1: "A", 2: "B" }
      var types =
        String.Join(", ",
          Enum.GetValues(typeof(TermType))
              .OfType<TermType>()
              .OrderByDescending(t => t)
              .Select(t => String.Format("{0}: '{1}'", (int)t, _localizeEnum(t)))
              .ToArray());
      ViewBag.TermTypesAsJsonForSearch = types + ", 99: 'All'";
      ViewBag.TermTypesAsJsonForEdit = types;
      return View(prj);
    }

    public ActionResult ListTermsets(int id, PaginationFormModel p) {
      var tsets = ProjectManager.Instance.GetTermSetsForProject(CurrentCulture, UserName, id);
      ViewBag.IsGlobal = false;
      return PartialView("Termset/_List", tsets.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult ListPublicTermsets(PaginationFormModel p) {
      var tsets = ProjectManager.Instance.GetTermSetsForProject(CurrentCulture, UserName);
      ViewBag.IsGlobal = true;
      return PartialView("Termset/_List", tsets.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    [HttpPost]
    public JsonResult GetTerms(int id, [Bind(Prefix = "_search")] bool? search, string type, string key, string value, int? rows, int? page, string sidx, string sord) {
      var p = page.HasValue ? page.Value : 1;
      var r = rows.HasValue ? rows.Value : 50;
      string tsName;
      var ts = ProjectManager.Instance.GetTermSetForGrid(id, search.GetValueOrDefault(), type, key, value, r, p, sidx, sord, out tsName).ToList();
      // jqGrid JSON Syntax
      var model = new {
        total = Math.Ceiling((decimal)ts.Count() / r),
        page = p,
        records = ts.Count(),
        rows = ts
                  .Select(t => new {
                    id = t.Id,
                    cell = new[] { _localizeEnum(t.TermType), t.Text, t.Content }
                  })
      };
      return Json(model, JsonRequestBehavior.AllowGet);
    }

    public ActionResult AddTermset(int id) {
      var ts = ProjectManager.Instance.CreateTermsetForProject(id, CurrentCulture);
      return PartialView("Termset/_AddTermset", ts);
    }

    [HttpPost]
    public JsonResult AddTermset(int id, TermSet ts) {
      try {
        ProjectManager.Instance.AddTermsetForProject(id, ts);
        return Json(new { msg = ControllerResources.ResourceController_Termset_Added });
      } catch (Exception ex) {
        return Json(new { msg = String.Format(ControllerResources.ResourceController_Termset_AddedError, ex.Message) });
      }
    }

    public ActionResult EditTermset(int id) {
      var ts = ProjectManager.Instance.GetTermSet(id);
      return PartialView("Termset/_EditTermset", ts);
    }

    [HttpPost]
    public JsonResult EditTermset(int id, TermSet ts) {
      try {
        ProjectManager.Instance.EditTermSet(id, ts);
        return Json(new { msg = ControllerResources.ResourceController_Termset_Changed });
      } catch (Exception ex) {
        return Json(new { msg = String.Format(ControllerResources.ResourceController_Termset_ChangedError, ex.Message) });
      }
    }

    public JsonResult DeleteTermset(int id) {
      try {
        ProjectManager.Instance.DeleteTermSet(id);
        return Json(new { msg = ControllerResources.ResourceController_Termset_Removed }, JsonRequestBehavior.AllowGet);
      } catch (Exception ex) {
        return Json(new { msg = String.Format(ControllerResources.ResourceController_Termset_RemovedError, ex.Message) }, JsonRequestBehavior.AllowGet);
      }
    }

    # endregion

    # region Terms

    // immediate creation of term lists from view (simplified one time call)
    public ActionResult GetTermListForTermset(IEnumerable<Term> terms, string name) {
      ViewBag.TermType = name;
      return PartialView("Termset/_Termlist", terms);
    }

    // update through ajax calls, gets same result as 'GetTermListForTermset'
    public ActionResult UpdateTermListForTermset(int id, TermType type) {
      var terms = ProjectManager.Instance.GetTermsSetOfTypeName(id, type);
      ViewBag.TermType = type.ToString();
      return PartialView("Termset/_Termlist", terms.ToList());
    }

    public ActionResult ChangeTermForTermset(int id, int type, string key, string value) {
      ProjectManager.Instance.ChangeTermsSetOfTypeName(id, (TermType)type, key, value, CurrentCulture);
      return Json(new { msg = ControllerResources.ResourceController_Term_Changed });
    }

    public ActionResult AddTermToTermset(int id, int type, string key, string value) {
      ProjectManager.Instance.AddTermsSetOfTypeName(id, (TermType)type, key, value, CurrentCulture);
      return Json(new { msg = ControllerResources.ResourceController_Term_Added });
    }

    public JsonResult RemoveTermFromTermset(int id) {
      var type = ProjectManager.Instance.RemoveTerm(id);
      return Json(new { msg = ControllerResources.ResourceController_Term_Removed });
    }


    # endregion
  }

}
