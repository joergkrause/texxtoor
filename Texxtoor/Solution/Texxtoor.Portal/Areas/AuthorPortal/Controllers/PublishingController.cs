using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary.Pdf;
using Texxtoor.BaseLibrary;
using Texxtoor.BaseLibrary.Mashup.Export;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.ViewModels.Content;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Author;
using Texxtoor.ViewModels.Common;
using Texxtoor.ViewModels.Content;
using Texxtoor.ViewModels.Utilities;

namespace Texxtoor.Portal.Areas.AuthorPortal.Controllers {

  [Authorize]
  public class PublishingController : ControllerExt {

    # region --== Publishing Preview For Authors ==--

    public ActionResult AuthorPreview(int id, string type) {
      var opus = ProjectManager.Instance.GetOpus(id, UserName);
      var templates = ProjectManager.Instance.GetTemplatesForTenant(opus).ToList();
      ViewBag.ProjectId = opus.Project.Id;
      ViewBag.OpusLang = String.IsNullOrEmpty(opus.LocaleId) ? null : new CultureInfo(opus.LocaleId).NativeName;
      var profile = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName);
      ViewBag.UserLang = profile != null ? new CultureInfo(profile.RunControl.UiLanguage).NativeName : ControllerResources.PublishingController_AuthorPreview_Not_Set;
      ViewBag.OpusId = id;
      ViewBag.PresetType = type;
      return View("AuthorPreview", templates);
    }

    [HttpPost]
    public JsonResult AuthorPreviewProduction(int id, GroupKind type, int? templateGroupId) {
      var prj = UnitOfWork<ProjectManager>();
      var prm = UnitOfWork<ProductionManager>();
      var opus = prj.GetOpus(id, UserName);
      prj.PrepareOpusForPublish(opus);
      var user = UnitOfWork<UserManager>().GetUserByName(UserName);
      var lnkTxt = String.Empty;
      var message = String.Empty;
      var href = String.Empty;
      Printable p = null;
      IBlob blob;
      var guid = Guid.NewGuid();
      switch (type) {
        case GroupKind.Pdf:
          if (templateGroupId.HasValue) {
            message = ControllerResources.PublishingController_Processing_PDF;
            lnkTxt = ControllerResources.PublishingController_Processing_PDFText;
            p = prm.CreatePrintable(opus, user, templateGroupId.Value);
            blob = BlobFactory.GetBlobStorage(guid, BlobFactory.Container.ProductionPreviews);
            blob.Content = prm.CreatePdfContent(p);
            blob["FileName"] = opus.Name;
            blob["Retention"] = DateTime.Now.AddDays(1); // file will be removed after one day
            blob.Save();
            href = Url.Action("AuthorPreviewFile", new {guid = guid, type = "pdf"});
          }
          break;
        case GroupKind.Epub:
          if (templateGroupId.HasValue) {
            message = ControllerResources.PublishingController_Processing_EPUB;
            lnkTxt = ControllerResources.PublishingController_Processing_EPUBText;
            p = prm.CreatePrintable(opus, user, templateGroupId.Value);
            blob = BlobFactory.GetBlobStorage(guid, BlobFactory.Container.ProductionPreviews);
            blob.Content = prm.CreateEpub(p);
            blob["FileName"] = opus.Name;
            blob["Retention"] = DateTime.Now.AddDays(1); // file will be removed after one day
            blob.Save();
            href = Url.Action("AuthorPreviewFile", new { guid = guid, type = "epub" });
          }
          break;
        case GroupKind.Html:
          if (templateGroupId.HasValue) {
            message = ControllerResources.PublishingController_Processing_HTML;
            lnkTxt = ControllerResources.PublishingController_Processing_HTMLText;
            href = Url.Action("AuthorPreviewFile", new {guid = guid, type = "html"});
            p = prm.CreatePrintable(opus, user, templateGroupId.Value);
            blob = BlobFactory.GetBlobStorage(guid, BlobFactory.Container.ProductionPreviews);
            blob.Content = Encoding.UTF8.GetBytes(prm.CreateHtml(p));
            blob["FileName"] = opus.Name;
            blob["Retention"] = DateTime.Now.AddDays(1); // file will be removed after one day
            blob.Save();
          }
          break;
        case GroupKind.Rss:
          message = ControllerResources.PublishingController_Processing_RSS;
          lnkTxt = ControllerResources.PublishingController_Processing_RSS;
          href = String.Format("/ServiceApi/Services/FeedService.svc/RssPreview/{1}/{0}", id,
                                 user.Id);
          break;
      }
      return Json(new {
        href = href,
        link = lnkTxt,
        msg = message,
        report = prm.IssueReport
      });
    }

    public FileResult AuthorPreviewFile(Guid guid, string type) {
      var blob = BlobFactory.GetBlobStorage(guid, BlobFactory.Container.ProductionPreviews);
      return File(blob.Content, MimeTypeHelper.GetFromExtension(type), blob["FileName"] + "." + type);
    }

    # region EPub

    # endregion

    # region PDF

    //private async Task GetPdf(int id, string name) {
    //  NotificationService.Instance.SetProductionProgress(15, "Started");
    //  var applicationPath = System.Web.HttpContext.Current.Server.MapPath("~/Download/");
    //  var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
    //  object state = "";
    //  var syncEvent = new AutoResetEvent(false);
    //  var invoker = new WorkflowInvoker(new PreviewCreationWorkflow());
    //  invoker.InvokeCompleted += delegate(object sender, InvokeCompletedEventArgs args) {
    //    if (args.Cancelled == true) {
    //      Debug.WriteLine("Workflow was cancelled.");
    //    } else if (args.Error != null) {
    //      Debug.WriteLine("Exception: {0}\n{1}",
    //                      args.Error.GetType().FullName,
    //                      args.Error.Message);
    //    } else {
    //      //args.Outputs["D1"], args.Outputs["D2"]);
    //    }
    //    syncEvent.Set();
    //  };
    //  IDictionary<string, object> inputs = new Dictionary<string, object> {
    //    {"type", TemplateGroup.Pdf},
    //    {"opus", opus},
    //    {"templateName", name},
    //    {"applicationPath", applicationPath},
    //    {"userName", UserName},
    //    {"locale", new CultureInfo(CurrentCulture) },
    //    {"notificationService", NotificationService.Instance}
    //  };      
    //  await Task.Factory.FromAsync<IDictionary<string, object>>(
    //     invoker.BeginInvoke(inputs, AsyncCall, state),
    //     invoker.EndInvoke);
    //}

    # endregion

    # endregion --== Publishing Preview For Authors ==--

    #region --== Publishing Procedure ==--

    public ContentResult GetContract(int? id) {
      var contract = System.IO.File.ReadAllText(Server.MapPath(String.Format("~/App_Data/Templates/Contracts/AuthorContract-{0}.html", CurrentCulture)));
      if (id.HasValue) {
        var publ = UnitOfWork<ProjectManager>().GetPublished(id.Value, UserName);
        var profile = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName);
        var author = String.Format("{0}, {1}",
                                   profile.FullName,
                                   profile.FullDefaultAddress);
        contract = contract.Replace(@"<span id=""muster-only"">(MUSTER)</span>", "");
        contract = contract.Replace(@"<span id=""authorFullAddress"">Max Mustermann, Musterstraße 10, 12345 Berlin</span>", author);
        contract = contract.Replace(@"<span class=""ebook-price"">0.00</span>", publ.Marketing.BasePrice.ToString(CurrentCulture));
        contract = contract.Replace(@"<span class=""texxtoor-price"">0.00</span>", publ.Marketing.BasePrice.ToString(CurrentCulture));
        contract = contract.Replace(@"<span class=""amazon-price"">0.00</span>", publ.Marketing.BasePrice.ToString(CurrentCulture));
        contract = contract.Replace(@"<span class=""apple-price"">0.00</span>", publ.Marketing.BasePrice.ToString(CurrentCulture));
        contract = contract.Replace(@"<span class=""author-name"">Bernd Mustermann</span>", publ.Owner.Profile.FirstName + " " + publ.Owner.Profile.LastName);
      }
      return Content(contract);
    }

    /// <summary>
    /// Entry Point for Publish Form. handles many or one project. Calls <see cref="RecentlyPublished"/> and <see cref="Publishables"/>.
    /// </summary>
    /// <param name="id">Opus Id</param>
    /// <returns></returns>
    public ActionResult Index(int? id) {
      PublishProject pp = null;
      ViewBag.NoAddressForPublish = false;
      if (id.HasValue) {
        var prj = UnitOfWork<ProjectManager>().GetProject(id.Value, UserName);
        pp = new PublishProject {
          CanPublish = prj.CanPublish() && CheckProfileAddress(prj.Team),
          Project = prj,
          QuickPublish = false
        };
      }
      if (!CheckProfileAddress(UserName)) {
        ViewBag.NoAddressForPublish = true;
      }
      return View("Index", pp);
    }

    private bool CheckProfileAddress(Team team) {
      return team.Members.First(m => m.TeamLead == true).Member.Profile.HasDefaultAddress();
    }

    private bool CheckProfileAddress(string userName) {
      return UnitOfWork<UserProfileManager>().GetProfileByUser(userName).HasDefaultAddress();
    }

    public ActionResult PublishableProjects() {
      var projects = UnitOfWork<ProjectManager>().GetUsersProjectsWithMembers(UserName, true)
        .ToList()
        .Where(p => UnitOfWork<ProjectManager>().MemberIsTeamLead(p.Team.Id, UserName))
        .Where(p => p.CanPublish())
        .OrderByDescending(p => p.CreatedAt)
        .ToList();
      projects = projects.Where(p => CheckProfileAddress(p.Team)).ToList();
      return PartialView("Lists/_PublishableProjects", projects);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult FullPublish(int id) {
      UnitOfWork<ProjectManager>().GetOrCreatePublished(id, false, UserName);
      return RedirectToAction("PublishedMarketing", new { id });
    }

    /// <summary>
    /// Create an RSS feed.
    /// </summary>
    /// <param name="id">Opus ID</param>
    /// <returns></returns>
    public ActionResult QuickPublished(int id) {
      var feed = UnitOfWork<ProjectManager>().QuickPublish(id, UserName);
      return View(feed);
    }

    /// <summary>
    /// Save the finally set data and publish to frozen elements. Provide public publishing data.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult ConfirmPublished(int id) {
      var publ = UnitOfWork<ProjectManager>().GetPublished(id, UserName);      
      try {
        UnitOfWork<ProjectManager>().PublishContentToFrozenState(publ.Id, publ.SourceOpus.Id, publ.NavLevel.GetValueOrDefault());
      } catch (Exception ex) {
        ViewBag.Error = ex.Message;
      }
      if (!publ.ExternalPublisher.RequestsPublishing) return View("ConfirmPublished", publ);
      if (publ.Marketing.AssignIsbn) {
        // Assign ISBN
        UnitOfWork<ProjectManager>().FreezeIsbn(publ);
      }
      if (publ.Marketing.RegisterForLibraries) {
        // Create ONIX
        var onix = new Onix(Onix.OnixVersion.V21);
        onix.GetProduct("de").Title = new ProductTitle {
          TitleText = new ProductTitleTitleText { textcase = 0, Value = publ.Title },
          Subtitle = new ProductTitleTitleText { textcase = 0, Value = publ.SubTitle },
          TitleType = 1
        };
        onix.GetProduct("de").Language = new ProductLanguage {
          LanguageCode = new CultureInfo("de").ThreeLetterISOLanguageName,
          LanguageRole = 1
        };
        onix.GetProduct("de").PublicationDate = (ushort)DateTime.Now.Year;
        onix.GetProduct("de").EditionNumber = 1;
        onix.GetProduct("de").ProductIdentifier = new ProductProductIdentifier {
          IDValue = 1u,
          ProductIDType = 1
        };
        onix.GetProduct("de").BasicMainSubject = publ.SubTitle;
        var sequence = 1;
        onix.GetProduct("de").Contributor =
          publ.Authors.Select(a =>
            new ProductContributor {
              BiographicalNote = a.Profile.Description,
              ContributorRole = "", // TODO: String.Join(", ", a.Roles.Select(r => r.UserRole.ToString()).ToArray()),
              PersonNameInverted = String.Format("{1}, {0}", a.Profile.FirstName, a.Profile.LastName),
              SequenceNumber = (byte)sequence++
            }).ToArray();
        onix.GetProduct("de").Publisher = new ProductPublisher {
          PublisherName = publ.Publisher,
          PublishingRole = 1
        };
        onix.GetProduct("de").Imprint = new ProductImprint {
          ImprintName = publ.Imprint.Name ?? publ.Publisher
        };
        onix.GetProduct("de").ProductForm = "SC"; // Soft Cover
        onix.GetProduct("de").AudienceCode = 3;
        onix.GetProduct("de").NumberOfPages = 100;
        onix.GetProduct("de").Price = new Price {
          CountryCode = new CultureInfo("de").TwoLetterISOLanguageName,
          CurrencyCode = "EUR",
          PriceAmount = publ.Marketing.BasePrice,
          PriceStatus = 2,
          PriceTypeCode = 4,
          TaxRateCode1 = "S" // S = Standard 19%
        };
        // TODO: Send ONIX to Admin
      }
      if (publ.Marketing.PackageBasePrice > 0) {
        // Package has a price and hence we withdraw from author's account
        UnitOfWork<AccountingManager>().AddAccountTransactions(publ.Marketing.PackageBasePrice, TransactionType.Sale, UserName);
        // TODO: Send Mail to user
      }
      return View("ConfirmPublished", publ);
    }

    [HttpPost]
    public JsonResult UnShare(int id, int[] isSingular) {
      UnitOfWork<ProjectManager>().UnsetSingularFragments(id, isSingular, UserName);
      return Json(new { msg = "OK" });
    }

    public FileResult CreateEpubPreview(int id, int templateGroupId) {
      var pjm = UnitOfWork<ProjectManager>();
      var prm = UnitOfWork<ProductionManager>();
      var publ = pjm.GetPublishedWithProps(id, UserName, p => p.SourceOpus, p => p.Marketing);
      var templateGroup = UnitOfWork<ProjectManager>().GetTemplateGroup(templateGroupId);
      // in preview set the templategroup temporarily
      var printable = prm.CreatePrintable(publ, templateGroup);
      var bytes = prm.CreateEpub(printable);
      return File(bytes, "application/epub+zip", publ.SourceOpus.Name + ".epub");
    }

    public FileResult CreateCoverPreview(int id, int templateGroupId) {
      var pjm = UnitOfWork<ProjectManager>();
      var prm = UnitOfWork<ProductionManager>();
      var publ = pjm.GetPublishedWithProps(id, UserName, p => p.SourceOpus, p => p.Marketing);
      var templateGroup = UnitOfWork<ProjectManager>().GetTemplateGroup(templateGroupId);
      // in preview set the templategroup temporarily
      var printable = prm.CreatePrintable(publ, templateGroup);
      var bytes = prm.CreatePdfCover(printable);
      return File(bytes, "application/pdf", publ.SourceOpus.Name + "_Cover.pdf");
    }

    /// <summary>
    /// Preview for several templates for author, only content, no cover.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="templateGroupId"></param>
    /// <returns></returns>
    public ActionResult CreateContentPreview(int id, int templateGroupId) {
      var pjm = UnitOfWork<ProjectManager>();
      var prm = UnitOfWork<ProductionManager>();
      var publ = pjm.GetPublishedWithProps(id, UserName, p => p.FrozenFragments, p => p.SourceOpus);
      var templateGroup = UnitOfWork<ProjectManager>().GetTemplateGroup(templateGroupId);
      // in preview set the templategroup temporarily
      var printable = prm.CreatePrintable(publ, templateGroup);
      var bytes = prm.CreatePdfContent(printable);
      return File(bytes, "application/pdf", publ.SourceOpus.Name + ".pdf");
    }

    /// <summary>
    /// Shows all published work by this author
    /// </summary>
    /// <returns></returns>
    public ActionResult RecentlyPublished() {
      var projects = UnitOfWork<ProjectManager>().GetUsersProjectsWithMembers(UserName, true)
        .ToList()
        .Where(p => UnitOfWork<ProjectManager>().MemberIsTeamLead(p.Team.Id, UserName))
        .Where(p => p.CanPublish())
        .OrderByDescending(p => p.CreatedAt);
      return View(projects);
    }

    public ActionResult ListRecentlyPublished(int[] ids, PaginationFormModel p) {
      if (ids == null) {
        return PartialView("Lists/_RecentlyPublished");
      }
      var published = new List<Published>();
      foreach (var id in ids) {
        published.AddRange(UnitOfWork<ProjectManager>().GetAllPublished(id, UserName));
      }
      return PartialView("Lists/_RecentlyPublished", published.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public ActionResult Publishables(int id, PaginationFormModel p) {
      List<KeyValuePair<Opus, string>> misses;
      var published = UnitOfWork<ProjectManager>().GetPublishables(new List<int> { id }, UserName, out misses);
      ViewBag.ProjectId = id;
      ViewBag.Misses = misses;
      return PartialView("Lists/_Publishables", published.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    [MinimumAwardScoreFilter(false, 50)]
    public ActionResult AddPublishMarketingButton(int id) {
      return PartialView("Lists/_PublishedMarketingButton", id);
    }

    [MinimumAwardScoreFilter(false, 50)]
    public ActionResult AddFullPublishButton(int id) {
      return PartialView("Lists/_FullPublishButton", id);
    }

    /// <summary>
    /// Main publishing procedure for not yet published texts.
    /// </summary>
    /// <param name="id">Opus' Id</param>
    /// <returns></returns>
    public ActionResult PublishedMarketing(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      var publ = UnitOfWork<ProjectManager>().GetPublishedWithProps(opus.Published.Id, UserName, p => p.Catalogs, p => p.Owner, p => p.Marketing, p => p.ResourceFiles, p => p.Imprint);
      var kdl = publ.ExternalPublisher;
      kdl.Title = kdl.Title ?? publ.Title;
      kdl.Description = kdl.Description ?? publ.SubTitle;
      if (String.IsNullOrEmpty(kdl.Authors)) {
        kdl.Authors = String.Join(",", publ.Authors.Select(u => u.Id.ToString()));
      }
      kdl.KindleLanguage = kdl.KindleLanguage ?? publ.SourceOpus.LocaleId;
      // ISBN is claimed here, but will not removed from store until finally published
      kdl.Isbn = UnitOfWork<ProjectManager>().ClaimIsbn(publ);

      // temporarily attach Imprint if available, user need to save this to connect Imprint with Published
      ViewBag.Imprint = UnitOfWork<ProjectManager>().GetImprint(UserName);

      ViewBag.IsValid = true; // assume model from DB is always valid
      ViewBag.ResourceFiles = UnitOfWork<ProjectManager>().GetResourceFiles(opus, TypeOfResource.Project, null, null, true);
      return View("PublishedMarketing", publ);
    }

    // Save Forms
    [HttpPost]
    public ActionResult SavePublishedCommon(int id, string title, string subtitle, int navLevel,
          [Bind(Prefix = "ExternalPublisher.KindleLanguage")] string kindleLanguage,
          [Bind(Prefix = "ExternalPublisher.Keywords")] string keywords,
          [Bind(Prefix = "ExternalPublisher.Description")] string description,
          int[] contrib, int[] about) {
      UnitOfWork<ProjectManager>().SavePublishedCommon(id, title, subtitle, navLevel, kindleLanguage, keywords, description, contrib, about, UserName);
      return Json(new { msg = "Ok" });
    }

    [HttpPost]
    public ActionResult SavePublishedCatalogue(int id, int[] Catalogs) {
      UnitOfWork<ProjectManager>().SavePublishedCatalogue(id, Catalogs, UserName);
      return Json(new { msg = "Ok" });
    }

    [HttpPost]
    public ActionResult SavePublishedCover(int id, string foreColor, string backColor, string fontFamily, float fontSize, int[] templateGroupId, string backTemplate, bool? usebackTemplate) {
      UnitOfWork<ProjectManager>().SavePublishedCover(id, foreColor, backColor, fontFamily, fontSize, backTemplate, usebackTemplate.GetValueOrDefault(), UserName);
      UnitOfWork<ProjectManager>().SavePublishedTemplate(id, templateGroupId, UserName);
      return Json(new { msg = "Ok" });
    }

    [HttpPost]
    public ActionResult TargetSelection(int id, bool publisher, bool? globalValid) {
      UnitOfWork<ProjectManager>().SetPublishingTarget(id, publisher, globalValid.GetValueOrDefault(), UserName);      
      return Json(new { msg = "Ok" });
    }

    public class MarketingModel {
      public bool? DrmRequired { get; set; }
      public bool? CreativeCommon { get; set; }
      public bool? ShareContent { get; set; }
      public bool? RegisterForLibraries { get; set; }
      public bool? CreateRssFeed { get; set; }
      public bool? AssignIsbn { get; set; }
      public string BasePrice { get; set; }
    }

    [HttpPost]
    public ActionResult SavePublishedMarketing(int id, MarketingModel m) {
      decimal price = 0;
      Decimal.TryParse(m.BasePrice, out price);
      UnitOfWork<ProjectManager>().SavePublishedMarketing(id,
        m.DrmRequired.GetValueOrDefault(),
        m.CreativeCommon.GetValueOrDefault(),
        m.ShareContent.GetValueOrDefault(),
        m.RegisterForLibraries.GetValueOrDefault(),
        m.CreateRssFeed.GetValueOrDefault(),
        m.AssignIsbn.GetValueOrDefault(), price, UserName);
      return Json(new { msg = "Ok" });
    }

    private static bool GetBoolForm(string val) {
      return val.Contains("true");
    }

    [HttpPost]
    public ActionResult SavePublishedResources(int id, List<int> targetId = null) {
      var publ = UnitOfWork<ProjectManager>().GetPublished(id, UserName);
      UnitOfWork<ProjectManager>().AddResourceFilesToPublished(publ, targetId == null ? null : targetId.ToArray());
      return Json(new { msg = "Ok" });
    }

    public JsonResult ClaimIsbn(int id, bool assignIsbn) {
      var pub = UnitOfWork<ProjectManager>().GetPublished(id, UserName);
      pub.Marketing.AssignIsbn = assignIsbn;
      var result = UnitOfWork<ProjectManager>().ClaimIsbn(pub);
      return Json(new { data = (result == null) ? "" : pub.ExternalPublisher.Isbn.Isbn10 });
    }

    public class DynaTreeModel {

      public string title;
      public JsTreeAttribute attr;
      public DynaTreeModel[] children;

    }

    /// <summary>
    /// Create catalog tree, JSON call from partial view _AssignCatalog, filter is optional (not mapped)
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="lang"></param>
    /// <param name="preSet"></param>
    /// <returns></returns>
    [HttpGet]
    public JsonResult Catalog(string filter, string lang, IEnumerable<Catalog> preSet) {
      if (String.IsNullOrEmpty(lang)) {
        lang = CurrentCulture;
      }
      if (String.IsNullOrEmpty(filter)) {
        var cat = UnitOfWork<ProjectManager>().GetCatalogForLanguage(lang, filter);
        if (preSet != null) {
          cat.ToList().ForEach(c => c.Selected = preSet.Any(p => p.Id == c.Id));
        }
        var tree = TreeService.GetNavigationTreeModel(cat, c => c.Name, c => c.Id);
        string str = Newtonsoft.Json.JsonConvert.SerializeObject(tree);
        str = str.Replace("\"data\":", "\"title\":");
        var lstTree = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DynaTreeModel>>(str);
        return Json(lstTree, JsonRequestBehavior.AllowGet);
      }
      var filterQuery = ReaderManager.Instance.GetCatalog(false, filter, lang);
      //var flat = filterQuery.ToList()
      //                      .Select(c => new JsTreeModel {
      //                        data = c.Name,
      //                        attr = new JsTreeAttribute { id = c.Id.ToString(CultureInfo.InvariantCulture), rel = "file" },
      //                        children = null
      //                      });
      var flat = filterQuery.ToList()
                         .Select(c => new DynaTreeModel {
                           title = c.Name,
                           attr = new JsTreeAttribute { id = c.Id.ToString(CultureInfo.InvariantCulture), rel = "file" },
                           children = null
                         });
      return Json(flat, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult SetCover(int id, HttpPostedFileBase file) {
      UnitOfWork<ProjectManager>().SaveCoverImage(id, file, UserName);
      // Return JSON             
      return new ContentResult { Content = ControllerResources.PublishingController_SetCover_Click_Image_to_upload_custom };
    }

    [HttpPost]
    public ActionResult RemoveCover(int id) {
      UnitOfWork<ProjectManager>().SaveCoverImage(id, UserName);
      // Return JSON             
      return new ContentResult { Content = ControllerResources.PublishingController_SetCover_Click_Image_to_upload_custom };
    }

    public ActionResult TemplateGroups(int id, GroupKind group) {
      var publ = UnitOfWork<ProjectManager>().GetPublished(id, UserName);
      var model = UnitOfWork<ProjectManager>().GetTemplatesForTenant(publ.SourceOpus, group)
        .Distinct()
        .OrderBy(t => t.Name)
        .ToList();
      ViewBag.GroupKind = group;
      return PartialView("Options/_TemplateGroup", model);
    }



    #endregion --== Publishing Procedure ==--

    # region Update / subscription

    public ActionResult UpdatePublishing(int id) {
      var publ = UnitOfWork<ProjectManager>().GetPublished(id, UserName);
      return View(publ);
    }

    [HttpPost]
    public ActionResult UpdatePublishing(int id, string subtitle, string description) {
      var publ = UnitOfWork<ProjectManager>().GetPublished(id, UserName);
      // set values
      // Copy Frozen Fragments
      return View(publ);
    }

    public ActionResult CreateNewVersion(int id) {
      var opus = UnitOfWork<ProjectManager>().GetOpus(id, UserName);
      return View(opus);
    }

    [HttpPost]
    public ActionResult RedirectToNewVersion(int id, Opus opus) {
      if (ModelState.IsValid) {
        var oldOpus = UnitOfWork<ProjectManager>().CreateOpusFromExisting(id, true);
        var newOpus = ProjectManager.Instance.EditOpus(id, opus.Name, opus.Version, opus.LocaleId, opus.Variation, opus.IsBoilerplate, oldOpus.HasMilestones(), oldOpus.Milestones);
        return RedirectToAction("Edit", "Opus", new {id = newOpus.Id});
      }
      return View("CreateNewVersion", opus);
    }

    # endregion

    # region Imprint

    public ActionResult Imprint() {
      var imprint = UnitOfWork<ProjectManager>().GetImprintForUser(UserName);
      return View(imprint);
    }

    [HttpPost]
    public JsonResult Imprint(ImprintAddress imprintaddress, [Bind(Prefix = "Country.Id")] int countryId) {
      imprintaddress.CountryId = countryId;
      var result = UnitOfWork<ProjectManager>().AddOrUpdateImprint(imprintaddress, UserName);
      return Json(new { msg = result != null ? "Imprint Saved" : "Something went wrong, check fields", imprintId = result != null ? result.ImprintId : 0 });
    }

    [HttpPost]
    public JsonResult ImprintImage(int id, HttpPostedFileBase file) {
      var result = UnitOfWork<ProjectManager>().SaveImprintLogo(id, file);
      return Json(new { msg = result != null ? "Image Saved" : "Something went wrong", size = result });
    }


    public ActionResult SaveIsbn(int id, string isbns) {
      var result = UnitOfWork<ProjectManager>().SaveIsbnToImprint(id, isbns, UserName);
      return Json(new { msg = result ? "Imprint Saved" : "Something went wrong, check fields" });
    }

    public ActionResult LoadIsbn(int id) {
      var isbn = UnitOfWork<ProjectManager>().GetIsbnForImprint(id, UserName);
      return PartialView("Imprint/_Isbn", isbn);
    }

    # endregion
  }
}