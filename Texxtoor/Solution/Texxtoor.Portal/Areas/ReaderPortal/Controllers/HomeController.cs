using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.Portal.Core.Extensions;
using Texxtoor.ViewModels.Common;
using Texxtoor.ViewModels.Utilities;

namespace Texxtoor.Portal.Areas.ReaderPortal.Controllers {
  public class HomeController : ControllerExt {

    public ActionResult Index() {
      return View();
    }
    public class DynaTreeModel
    {

        public string title;
        public JsTreeAttribute attr;
        public DynaTreeModel[] children;

    }
    /// <summary>
    /// Create catalog tree, JSON call from partial view _Catalog, filter is optional (not mapped)
    /// </summary>
    /// <param name="filter"></param>
    /// /// <param name="language"></param>
    /// <returns></returns>
    [HttpGet]
    public JsonResult Catalog(string filter, string language) {
      if (String.IsNullOrEmpty(filter)) {
        var query = ReaderManager.Instance.GetCatalog(true, null, String.IsNullOrEmpty(language) ? null : language);
        var tree = TreeService.GetNavigationTreeModel(query, c => c.Name, c => c.PublishedCount, c => c.Id);
        string str = Newtonsoft.Json.JsonConvert.SerializeObject(tree);
        str = str.Replace("\"data\":", "\"title\":");
        List<DynaTreeModel> lstTree=Newtonsoft.Json.JsonConvert.DeserializeObject<List<DynaTreeModel>>(str);
        return Json(lstTree, JsonRequestBehavior.AllowGet);

      }
      var filterQuery = ReaderManager.Instance.GetCatalog(false, filter, language);
      var flat = filterQuery.ToList()
                         .Select(c => new DynaTreeModel
                         {
                             title = c.Name,
                             attr = new JsTreeAttribute { id = c.Id.ToString(CultureInfo.InvariantCulture), rel = "file" },
                             children = null
                         });
      return Json(flat, JsonRequestBehavior.AllowGet);
    }

    private IEnumerable<Published> GetRecommendations() {
      var pmatrix = ReaderManager.Instance.GetConsumerMatrix(UserName, false).ToList();
      var tmatrix = ReaderManager.Instance.GetConsumerMatrix(UserName).ToList();
      var query = ReaderManager.Instance.GetRecommendations(UserName, pmatrix, tmatrix);      
      return query;
    }

    public ActionResult Recommendations(PaginationFormModel p) {
      if (p == null) {
        p = new PaginationFormModel {
          FilterValue = "",
          FilterName = "",
          Order = "Rating",
          Dir = false
        };
      }
      var rec = GetRecommendations();
      return PartialView("_BrowseCatalog", rec.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    // Search within catalog (browsing)
    public ActionResult BrowseCatalog(int? id, PaginationFormModel p) {
      var catId = (id.HasValue) ? id.Value : -1;
      var rec = GetRecommendations();
      var ic = new IdComparer();
      var publ = ReaderManager.Instance.GetPublished(catId).Union(rec).Distinct(ic).OrderByDescending(pb => pb.Rating);
      // get pricing previews
      var medias = OrderManager.Instance.GetMedias();
      var pricingPreview = new Dictionary<int, Dictionary<string, decimal>>();
      foreach (var pb in publ) {
        decimal subscriptionPrice;
        decimal price;
        var pricing = new Dictionary<string, decimal>();
        foreach (var media in medias) {
          var m = new List<OrderMedia>(new[] {media});
          OrderManager.Instance.CalculateProductionCost(
            m,
            ControllerContext.HttpContext.Application["ProductionCalc"] as XDocument,
            true,
            pb.Marketing.BasePrice,
            out price,
            out subscriptionPrice);
          pricing.Add(media.Name, price);
        }        
        pricingPreview.Add(pb.Id, pricing);
      }
      ViewBag.PricingPreview = pricingPreview;
      return PartialView("_BrowseCatalog", publ.AsQueryable().ToPagedList(p.Page, p.PageSize, p.FilterValue, p.FilterName, p.Order, p.Dir));
    }

    private class IdComparer : IEqualityComparer<Published> {

      public bool Equals(Published x, Published y) {
        return x.Id == y.Id;
      }

      public int GetHashCode(Published obj) {
        return 0;
      }
    }


    // creates the global search form, prefilled with data
    public ActionResult GlobalSearch() {
      return PartialView("_GlobalSearch", ReaderManager.Instance.GetCatalog(true, null));
    }

    // full text search result (handle both seacrh forms)
    [HttpPost]
    public ActionResult Search(string profiBtn, string stdBtn, string search_profi, string search_term, int? stdCategory, int? profiCategory, int? rating, bool? auditing, int? auditinglevel) {
      // filter full text
      var profiSearch = !String.IsNullOrEmpty(profiBtn);
      var search = profiSearch ? search_profi : search_term;
      var category = profiSearch ? profiCategory : stdCategory;

      // query
      var query = ReaderManager.Instance.GetPublished(profiSearch, search, auditing, auditinglevel, rating);
        
      ViewBag.QueryCount = query.Count();
      // remove categories
      var catalog = ReaderManager.Instance.GetCatalog(category);
      var result = query.Where(p => p.Catalogs.Contains(catalog) || !category.HasValue);
      return View(result);
    }

    [Authorize]
    public ActionResult Matrix() {
      var pmatrix = ReaderManager.Instance.GetConsumerMatrix(UserName, false);
      return View(pmatrix);
    }

    [Authorize]
    public ActionResult MatrixSearch() {
      var pmatrix = ReaderManager.Instance.GetConsumerMatrix(UserName, false);
      return View("Search");
    }

    [HttpGet]
    public JsonResult GetMatrixValues() {
      if (!User.Identity.IsAuthenticated || String.IsNullOrEmpty(UserName)) {
        return Json(new{data = ""}, JsonRequestBehavior.AllowGet);
      }
      var gmatrix = ReaderManager.Instance.GetConsumerMatrix(UserName);
      if (gmatrix != null) {
        var tmatrix = ReaderManager.Instance.GetConsumerMatrix(UserName)
          .Select(t => new {
            Id = t.Id,
            Keyword = t.Keyword,
            Stage = t.Stage.ToString(),
            Target = t.Target.ToString()
          });
        return Json(new { data = tmatrix }, JsonRequestBehavior.AllowGet);
      }
      // no profile and hence no default matrix
      return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult AddMatrixValue(ConsumerMatrix matrix) {      
      var id = ReaderManager.Instance.AddConsumerMatrix(matrix, UserName);
      return Json(new { msg = "OK", data = id });
    }

    [HttpPost]
    public JsonResult RemoveMatrixValue(int id) {
      ReaderManager.Instance.RemoveConsumerMatrix(id, UserName);
      return Json(new { msg = "OK" });
    }

  }
}
