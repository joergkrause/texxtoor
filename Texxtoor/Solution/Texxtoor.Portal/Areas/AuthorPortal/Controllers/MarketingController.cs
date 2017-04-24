using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.Logging;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Marketing;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Author;
using Texxtoor.ViewModels.Common;

namespace Texxtoor.Portal.Areas.AuthorPortal.Controllers {

  /// <summary>
  /// Manage marketing packages, pricing, and other global project features regarding sales.
  /// </summary>
  public class MarketingController : ControllerExt {

    [Authorize]
    public ActionResult Index() {
      var contribRoles = Enum.GetNames(typeof(ContributorRole)).Select(c => (ContributorRole)Enum.Parse(typeof(ContributorRole), c)).ToArray();
      var projectLeader = UnitOfWork<ProjectManager>().GetProjectsWhereUserIsMember(UserName).ToList().Where(p => p.Team.TeamLead.UserName == UserName);
      var projectContrib = UnitOfWork<ProjectManager>().GetProjectForRoles(contribRoles, UserName);
      var projectLeaders = projectLeader as List<Project> ?? projectLeader.ToList();
      var model = new MarketingSummary {
        Projects = projectLeaders.ToList(),
        ProjectsAsLeader = projectLeaders.Count(),
        ProjectsAsMember = projectContrib.Count()
      };
      return View(model);
    }

    [Authorize]
    public ActionResult Revenues() {
      // SalesTracking Table
      var model = UnitOfWork<ProjectManager>().GetSalesTracking(UserName);
      return View(model);
    }

    // aggregate sales information
    [Authorize]
    public ActionResult RevenueFilterDate(object model, Revenues.DateFilter filter) {
      var sales = AccountingManager.Instance.GetTotalSales(filter, false, UserName);
      return PartialView("_RevenueFilterDate", sales);
    }

    [Authorize]
    public ActionResult Stats() {
      return View();
    }

    # region --== Marketing Settings ==--

    /// <summary>
    /// Settings for whole Project (can be overwritten during publishing, if project is used multiple times for publishing)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    public ActionResult MarketingPackage(int? id) {
      if (!id.HasValue) return View();
      var prj = UnitOfWork<ProjectManager>().GetProject(id.Value, UserName);
      return View(prj);
    }

    [Authorize]
    public ActionResult EditMarketingPackage(int id) {
      var pckg = UnitOfWork<ProjectManager>().GetMarketingPackage(id, UserName);
      return View("MarketingPackage/_Edit", pckg);
    }

    [HttpPost]
    public JsonResult EditMarketingPackage(MarketingPackage pckg, int[] package, string countryList) {
      UnitOfWork<ProjectManager>().EditMarketingPackage(pckg, package, countryList, UserName);
      return Json(new { msg = ControllerResources.MarketingController_Edit_MarketingPackage });
    }

    [Authorize]
    public ActionResult AddMarketingPackage() {
      var pckg = new MarketingPackage();
      pckg.MarketingType = MarketingPackageType.PublicDomain | MarketingPackageType.CreativeCommon | MarketingPackageType.RegularSale | MarketingPackageType.SubscriptionFee;
      return View("MarketingPackage/_Add", pckg);
    }

    [HttpPost]
    public JsonResult AddMarketingPackage(MarketingPackage pckg, int[] package, string countryList) {
      UnitOfWork<ProjectManager>().CreateMarketingPackage(pckg, package, countryList, UserName);
      return Json(new { msg = ControllerResources.MarketingController_Add_MarketingPackage });
    }


    [HttpPost]
    public JsonResult SetMarketingPackage(int id, int marketingId, bool? unassign) {
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      var pckg = UnitOfWork<ProjectManager>().SetMarketingPackage(prj, marketingId, unassign.GetValueOrDefault());
      return Json(new { msg = String.Format(ControllerResources.MarketingController_Set_MarketingPackage, pckg.Name, prj.Name) });
    }

    /// <summary>
    /// Packages are defined per user (owner, e.g. lead author), and used per Opus and for Publishing
    /// </summary>
    /// <returns></returns>
    [Authorize]
    public ActionResult ListMarketingPackages(int id, PaginationFormModel p) {
      var currentPrj = UnitOfWork<ProjectManager>().GetProject(id, UserName, pr => pr.Marketing);
      var pckgs = UnitOfWork<ProjectManager>().GetMarketingPackages(UserName).ToList();
      if (currentPrj.Marketing != null) {
        // package might be created but not yet assigned
        ViewBag.AssignedPackage = currentPrj.Marketing.Id;
      }
      var userProjects = UnitOfWork<ProjectManager>().GetProjectsWhereUserIsMember(UserName)
                .ToList();
      foreach (var marketingPackage in pckgs) {
        marketingPackage.AssignedProjects = userProjects.Where(mp => mp.Marketing != null && mp.Marketing.Id == marketingPackage.Id).ToList();
      }
      return View("MarketingPackage/_List", pckgs.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    /// <summary>
    /// Packages are defined per user (owner, e.g. lead author), and used per Opus and for Publishing
    /// </summary>
    /// <returns></returns>
    [Authorize]
    public ActionResult ListAllMarketingPackages(PaginationFormModel p) {
      var pckgs = UnitOfWork<ProjectManager>().GetMarketingPackages(UserName).ToList();
      var userProjects = UnitOfWork<ProjectManager>().GetProjectsWhereUserIsMember(UserName)
                .ToList();
      foreach (var marketingPackage in pckgs) {
        marketingPackage.AssignedProjects = userProjects.Where(mp => mp.Marketing != null && mp.Marketing.Id == marketingPackage.Id).ToList();
      }
      return View("MarketingPackage/_List", pckgs.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    public JsonResult CountryLookup(string q) {
      var result = UnitOfWork<ProjectManager>().GetCountries(q).ToArray();
      return Json(result, "text/html", JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetPackageCountries(int id) {
      var pckg = UnitOfWork<ProjectManager>().GetMarketingPackage(id, UserName);
      if (pckg != null) {
        var countries = UnitOfWork<ProjectManager>().GetCountries(pckg.LimitCountries);
        return Json(countries, JsonRequestBehavior.AllowGet);
      }
      return Json(null, JsonRequestBehavior.AllowGet);
    }

    [Authorize]
    public ActionResult ProjectMarketingState(int id) {
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName, p => p.Marketing);
      return View("MarketingPackage/_MarketingState", prj);
    }

    [HttpPost]
    public JsonResult DeleteMarketingPackage(int id) {
      var pckg = UnitOfWork<ProjectManager>().GetMarketingPackage(id, UserName);
      if (pckg == null) {
        return Json(new { msg = ControllerResources.MarketingController_Delete_MarketingPackage_Cannot });
      }
      // check whether the package is in use (at least one)
      var inuse = UnitOfWork<ProjectManager>().DeleteMarketingPackage(pckg, UserName);
      return Json(!inuse
        ? new { msg = ControllerResources.MarketingController_Delete_MarketingPackage_Success }
        : new { msg = ControllerResources.MarketingController_Delete_MarketingPackage_InUse });
    }


    # endregion --== Marketing Settings ==--

    # region --== Pricing and Shares ==--

    [Authorize(Roles = "TeamLead")]
    public ActionResult SetBasePrice(int id)
    {
      var model = ProjectManager.Instance.GetProject(id, UserName, project => project.Marketing);
      return View(model);
    }

    [HttpPost]
    public ActionResult SetBasePrice(int id, decimal price) {
      var result = UnitOfWork<ProjectManager>().SetBasePrice(id, price, UserName);
      if (result.Kind != ResultKind.Error) {
        return Json(new { msg = result.Text });
      } else {
        return new HttpNotFoundResult(result.Text);
      }
    }

    [HttpPost]
    public JsonResult GetBasePrice(int id, decimal? val) {
      if (val.HasValue) {
        return Json(new {
          Print = String.Format("{0:0.00}", val * 4 + 5),
          Epub = String.Format("{0:0.00}", val * 2),
          Ibook = String.Format("{0:0.00}", val * 3)
        });
      }
      var prj = UnitOfWork<ProjectManager>().GetProject(id, UserName);
      var basePrice = prj.Marketing == null ? 0M : prj.Marketing.BasePrice;
      return Json(new {
        Print = String.Format("{0:0.00}", basePrice * 4 + 5),
        Epub = String.Format("{0:0.00}", basePrice * 2),
        Ibook = String.Format("{0:0.00}", basePrice * 3)
      });
    }

    # endregion --== Pricing ==--

  }
}
