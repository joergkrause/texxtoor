using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml;
using Texxtoor.BaseLibrary.Core.Extensions.ActionResults;
using Texxtoor.BaseLibrary;
using Texxtoor.BaseLibrary.Services;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Exceptions;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Cms;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.DataModels.ViewModels.Content;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Author;
using Texxtoor.ViewModels.Content;
using Texxtoor.ViewModels.Users;
using Texxtoor.ViewModels.Users.Demo;

namespace Texxtoor.Portal.Controllers {

  /// <summary>
  /// Base functions for the common areas, not specific for particular users.
  /// </summary>
  public class HomeController : ControllerExt {

    /// <summary>
    /// The project's landing page, contains sort of links to Index with or without entry points.
    /// </summary>
    /// <returns></returns>
    public ActionResult Index() {       
      if (User.Identity.IsAuthenticated) {      
        var hsi = UnitOfWork<UserProfileManager>().GetHomeScreenInfo(UserName);
        ViewBag.IsconnectedUser = UnitOfWork<UserManager>().IsConnectedUser(UserName);
        return View("Dashboard", hsi);
      } else {
        return View("LandingPage");
      }
    }

    public ActionResult AccessDenied() {
      return View();
    }

    public ImageResult GetAuthorThumbnail(int id) {
      UserProfile userProfile = Manager<UserProfileManager>.Instance.GetProfile(id);
      return new ImageResult(userProfile.Image);
    }

    public ActionResult News() {
      ViewBag.IsLandingPage = true;
      return View();
    }

    [OutputCache(Duration = 86400, VaryByParam = "id")]
    public ActionResult InfoPage(int id) {
      ViewBag.IsLandingPage = true;
      try {
        IEnumerable<CmsPage> pages = PageService.Instance.LoadAllForMenu(id);
        ViewBag.PageTitle = PageService.Instance.GetMenu(id).Title;
        return PartialView("_Page", pages);

      } catch (PageNotFoundException ex) {
        return View("LandingPage");
      }
    }

    //[OutputCache(Duration = 86400, VaryByParam = "name")]
    public ActionResult PageAlias(string name) {
      ViewBag.IsLandingPage = true;
      try {
        var page = PageService.Instance.LoadAlias(name);
        var menus = page.Menu.FirstOrDefault();
        ViewBag.PageTitle = menus != null ? PageService.Instance.GetMenu(menus.Id).Title : page.PageTitle;
        return View("Page", page);

      } catch (PageNotFoundException ex) {
        return View("LandingPage");
      }
    }

    //[OutputCache(Duration=86400, VaryByParam="id;name")]
    public ActionResult Page(int id, string name) {
      ViewBag.IsLandingPage = true;
      try {
        var page = PageService.Instance.LoadPage(id.ToString()) ?? PageService.Instance.LoadAlias(name);
        var menus = page.Menu.FirstOrDefault();
        ViewBag.PageTitle = menus != null ? PageService.Instance.GetMenu(menus.Id).Title : page.PageTitle;
        return View(page);
      } catch (PageNotFoundException ex) {        
        return View("LandingPage");
      }
    }

    [OutputCache(Duration = 86400)]
    public ActionResult About() {
      CmsPage page = PageService.Instance.LoadAlias("about");
      ViewBag.IsLandingPage = true;
      return View("Page", page);
    }

    [OutputCache(Duration = 86400)]
    public ActionResult Terms() {
      CmsPage page = PageService.Instance.LoadAlias("terms");
      ViewBag.IsLandingPage = true;
      return View("Page", page);
    }

    [OutputCache(Duration = 86400)]
    public ActionResult Press() {
      CmsPage page = PageService.Instance.LoadAlias("press");
      ViewBag.IsLandingPage = true;
      return View("Page", page);
    }

    public ActionResult Latest() {
      ViewBag.IsLandingPage = true;
      return View();
    }

    public ActionResult WelcomePackage() {
      ViewBag.IsLandingPage = true;
      return View();
    }

    public ActionResult AuthorHelp() {
      ViewBag.IsLandingPage = true;
      return View();
    }

    public ActionResult PubNews() {
      var url = WebConfigurationManager.AppSettings["texxtoor:NewsFeedUrl"];
      var reader = XmlReader.Create(url);
      var feed = SyndicationFeed.Load(reader);
      reader.Close();
      var model = feed.Items.ToDictionary(k => k.Title.Text, v => v.Summary.Text);
      ViewBag.BaseUrl = url;
      return View(model);
    }

    [OutputCache(Duration = 86400)]
    public ActionResult Contact() {
      return RedirectToAction("Contact", "Account");
    }

    [OutputCache(Duration = 3600)]
    public ActionResult Statistic() {
      ViewBag.Authors = UnitOfWork<UserManager>().GetUsersByRole(UserRole.Author).Count().ToString(CultureInfo.InvariantCulture);
      ViewBag.Works = UnitOfWork<ProjectManager>().GetPublishedWorks().Count().ToString(CultureInfo.InvariantCulture);
      return PartialView("_Stat");
    }

    public ActionResult Social() {
      // TODO map a social graph from current activity here
      @ViewBag.Subject = "texxtoor Community";
      @ViewBag.Activity = "Reading a Book";
      @ViewBag.Link = "http://www.texxtoor.com";
      return PartialView("_Social");
    }

    /// <summary>
    /// Global CMS search
    /// </summary>
    /// <param name="cmsSearch"></param>
    /// <returns></returns>
    public ActionResult SearchAll(string cmsSearch) {
      var pages = PageService.Instance.FindPage(cmsSearch, CurrentCulture)
        .Take(10)
        .OrderByDescending(p => p.ModifiedAt).ToList();
      return View(pages);
    }

    public ActionResult DemoWizard(string wizard) {
      var cl = CurrentCulture;
      var dw = Request.RequestContext.HttpContext.Application["DemoWizard"] as IList<DemoWizard>;
      var w = dw.Single(d => d.Id == wizard && d.Language == cl);
      return View(w);
    }

    [HttpPost]
    public async Task<ActionResult> DemoWizardCall(string wizard) {
      var cl = CurrentCulture;
      var dw = Request.RequestContext.HttpContext.Application["DemoWizard"] as IList<DemoWizard>;
      var w = dw.Single(d => d.Id == wizard && d.Language == cl);
      await UnitOfWork<UserManager>().ValidateUser(w.DemoData.UserName, w.DemoData.Password);
      UnitOfWork<UserManager>().LogSignIn(w.DemoData.UserName, false);
      var page = w.DemoData.Pages.First();
      return Redirect(page.Url);
    }

    public ActionResult AllWizards() {
      var dw = Request.RequestContext.HttpContext.Application["DemoWizard"] as IList<DemoWizard>;
      var cl = CurrentCulture;
      var w = dw.Where(d => d.Language == cl && d.Id == "CreateProject");
      return View(w);
    }

    public ActionResult FeatureNotAvailable() {
      return View();
    }

    [AllowAnonymous]
    public ActionResult Premium() {
      ViewBag.Message = "";
      return View(new Premium());
    }

    [HttpPost]
    [AllowAnonymous]
    public ActionResult Premium(Premium p) {
      if (!String.IsNullOrEmpty(p.Code)) {
        if (p.Code.Equals("b24k3cc")) {
          // TODO 
          return RedirectToAction("Register", "Account", new { name = p.Name, email = p.Email });
        }
      }
      ViewBag.Message = "Code invalid";
      return View(p);
    }

    public ActionResult Contract() {
      object contract = System.IO.File.ReadAllText(Server.MapPath(String.Format("~/App_Data/Templates/Contracts/AuthorContract-{0}.html", CurrentCulture)));
      return View("Contract", contract);
    }

    [HttpPost]
    public JsonResult QuickFormAuthor(QuickProject create, QuickProjectAdvanced advanced) {
      if (!String.IsNullOrEmpty(create.Name) && (create.UseDefaults || (!create.UseDefaults && !String.IsNullOrEmpty(advanced.ShortName) && !String.IsNullOrEmpty(advanced.Description)))) {
        string tn = String.Format(ControllerResources.HomeController_QuickFormAuthor_Team_for_project__0___Quick_Start_, create.Name);
        Team t = UnitOfWork<ProjectManager>().GetTeamsWhereUserIsLead(UserName).FirstOrDefault();
        t = t ?? UnitOfWork<ProjectManager>().CreateTeam(tn, "", UnitOfWork<UserManager>().GetCurrentUser(UserName));
        string sText = String.Format(ControllerResources.HomeController_QuickFormAuthor_Quickstart_project__0__created_at__1_, create.Name, DateTime.Now.ToShortDateString());
        string sDesc =
          String.Format(
            ControllerResources.HomeController_QuickFormAuthor_The_project__0___created_at__1__was_build,
            create.Name, DateTime.Now.ToShortDateString());
        if (!create.UseDefaults) {
          sText = advanced.ShortName;
          sDesc = advanced.Description;
        }
        var template = new NameValueCollection();
        template.Add("tpl", "1");
        template.Add("tpl-1-chapters", "2");
        Project p = UnitOfWork<ProjectManager>().CreateProject(UserName, create.Name, sText, sDesc, "", false, t.Id, template, true);
        return Json(new { forward = Url.Action("AuthorRoom", "Editor", new { area = "AuthorPortal", id = p.Opuses.First().Id }) });
      } else {
        return Json(new { forward = "" });
      }
    }

    [HttpPost]
    public JsonResult QuickFormReader(string search, string[] keywords, StageType[] stages, TargetType[] targets) {
      if (!String.IsNullOrEmpty(search) || keywords != null) {
        return Json(new { forward = Url.Action("Matrix", "Home", new { area = "ReaderPortal" }) });
      } else {
        return Json(new { forward = "" });
      }
    }

    public ActionResult Favorites() {
      UserProfile profile = null;
      if (!String.IsNullOrEmpty(UserName)) {
        profile = UnitOfWork<UserProfileManager>().GetProfileByUser(UserName);
      }
      return PartialView("_Favorites", profile == null ? null : profile.RunControl.Favorites);
    }

    [HttpPost]
    public ActionResult AddFavorite(string id, string title, string model) {
      var repository = UnitOfWork<UserProfileManager>();
      var profile = repository.GetProfileByUser(UserName);
      var dict = profile.RunControl.Favorites;
      var key = String.Format("{0}/{1}", model, id);
      if (dict.ContainsKey(key)) {
        return Json(new { msg = ControllerResources.HomeController_AddFavorite_Already_There });
      }
      dict.Add(key, title);
      profile.RunControl.Favorites = dict;
      repository.SaveChanges();
      return Json(new { msg = ControllerResources.HomeController_AddFavorite_Favorite_added });
    }

    [HttpPost]
    public ActionResult RemoveFavorite(string key) {
      var repository = UnitOfWork<UserProfileManager>();
      var profile = repository.GetProfileByUser(UserName);
      var dict = profile.RunControl.Favorites;
      if (dict.ContainsKey(key)) {
        dict.Remove(key);
      }
      profile.RunControl.Favorites = dict;
      repository.SaveChanges();
      return Json(new { msg = ControllerResources.HomeController_RemoveFavorite_Favorite_entry_removed });
    }


    

  }
}
