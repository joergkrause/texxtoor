using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml.Linq;
using Texxtoor.BaseLibrary;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Context;
using Texxtoor.Portal.Core.Attributes;
using Texxtoor.Portal.Core.Extensions;

namespace Texxtoor.Portal.Areas.AdminPortal.Controllers {

  /// <summary>
  /// Basic environment settings
  /// </summary>
  [Authorize(Roles = "Admin")]
  //[RequireHttps]
  public class HomeController : ControllerExt {

    public ActionResult Index() {
      return View();
    }

    public ActionResult Config() {
      var cfg = new Dictionary<string, object>();      
      WebConfigurationManager.AppSettings.CopyTo(cfg);
      var model = GetGroupedModel(cfg);
      return View(model);
    }

    private static IDictionary<string, IGrouping<string, KeyValuePair<string, object>>> GetGroupedModel(Dictionary<string, object> cfg) {
      return cfg
        .Where(c => c.Key.Contains(":"))
        .OrderBy(c => c.Key)
        .GroupBy(c => c.Key.Split(':')[0])
        .Where(c => !new[] { "owin", "aspnet", "webpages" }.Contains(c.Key))
        .ToDictionary(c => c.Key, c => c);
    }

    [HttpPost]
    public ActionResult Config(FormCollection form) {
      var configFile = WebConfigurationManager.OpenWebConfiguration("~");
      foreach (var key in form.AllKeys) {
        var val = form[key];
        configFile.AppSettings.Settings[key].Value = val.Replace(",,", ",").TrimEnd(','); // comma handling prevents empty collections being saved
      }
      configFile.Save(ConfigurationSaveMode.Minimal);
      var cfg = new Dictionary<string, object>();
      WebConfigurationManager.AppSettings.CopyTo(cfg);
      var model = GetGroupedModel(cfg);
      return View(model);
    }

    public ActionResult Info() {

      return View();
    }

    public class StatCounter {
      public string FullTableName { get; set; }
      public string TableName { get; set; }
      public string SchemaName { get; set; }

      public int Rows { get; set; }

      public DateTime Created { get; set; }
      public DateTime Modified { get; set; }
    }

    public class DbCounter {

      public string Databasename { get; set; }
      public decimal LogSize { get; set; }
      public decimal RowSize { get; set; }
      public decimal TotalSize { get; set; }

      public IEnumerable<StatCounter> StatCounters { get; set; }

    }

    public ActionResult Stat() {
      try {
        const string sql = @"SELECT '[' + SCHEMA_NAME(t.schema_id) + '].[' + t.name + ']' AS fullTableName, SCHEMA_NAME(t.schema_id) AS schemaName, t.name AS TableName, i.rows AS Rows, t.create_date AS Created, t.modify_date AS Modified
                            FROM sys.tables AS t INNER JOIN sys.sysindexes AS i ON t.object_id = i.id AND i.indid < 2
                            WHERE SCHEMA_NAME(t.schema_id) != 'dbo'
                            ORDER BY schemaName, fullTableName";
        var counters = AdminDb.ExecuteQuery<StatCounter>(sql).ToList();
        const string size = @"SELECT DatabaseName = DB_NAME(database_id), 
                            LogSize = CAST(SUM(CASE WHEN type_desc = 'LOG' THEN size END) * 8. / 1024 AS DECIMAL(8,2)),
                            RowSize = CAST(SUM(CASE WHEN type_desc = 'ROWS' THEN size END) * 8. / 1024 AS DECIMAL(8,2)),
                            TotalSize = CAST(SUM(size) * 8. / 1024 AS DECIMAL(8,2))
                            FROM sys.master_files WITH(NOWAIT)
                            WHERE database_id = DB_ID() -- for current db 
                            GROUP BY database_id";
        var model = AdminDb.ExecuteQuery<DbCounter>(size).ToList().Single();
        model.StatCounters = counters;
        return View(model);
      } catch (Exception ex) {
        return View(new DbCounter() { Databasename = "Function not available" });
      }
    }

    public FileResult BackupToFile(string table, string p) {      
      switch (table.ToLowerInvariant()) {
        case "localization":
          return File(BackupLocalization(p), "text/xml", String.Format("Localization{0}{1}.xml", String.IsNullOrEmpty(p) ? "" : ".", p));
      }
      return null;
    }

    private Stream BackupLocalization(string lang) {
      var ctx = UnitOfWork<ProjectManager>().Ctx;
      lang = lang ?? String.Empty;
      try {
        // try to save
        var xLoc = new XDocument(
          new XElement("localization",
                       ctx.Localization
                          .Where(l => l.LocaleId == lang)
                          .OrderBy(l => l.ResourceSet)
                          .ThenBy(l => l.ResourceId)
                          .ThenBy(l => l.LocaleId)
                          .ToList().Select(l => new XElement("loc",
                                                             new XAttribute("type", l.GetType().Name),
                                                             new XAttribute("localeid", l.LocaleId),
                                                             new XAttribute("resid", l.ResourceId),
                                                             new XAttribute("set", l.ResourceSet),
                                                             new XElement("data", l.Value))).ToList()
            )
          );
        var ms = new MemoryStream();
        xLoc.Save(ms);
        ms.Position = 0;
        return ms;
      }
      catch {
        return null;
      }
    }

    private Stream RestorLocalization(string lang) {
      var ctx = UnitOfWork<ProjectManager>().Ctx;
      lang = lang ?? String.Empty;
      try {
        // try to save
        var xLoc = new XDocument(
          new XElement("localization",
                       ctx.Localization
                          .Where(l => l.LocaleId == lang)
                          .OrderBy(l => l.ResourceSet)
                          .ThenBy(l => l.ResourceId)
                          .ThenBy(l => l.LocaleId)
                          .ToList().Select(l => new XElement("loc",
                                                             new XAttribute("type", l.GetType().Name),
                                                             new XAttribute("localeid", l.LocaleId),
                                                             new XAttribute("resid", l.ResourceId),
                                                             new XAttribute("set", l.ResourceSet),
                                                             new XElement("data", l.Value))).ToList()
            )
          );
        var ms = new MemoryStream();
        xLoc.Save(ms);
        ms.Position = 0;
        return ms;
      } catch {
        return null;
      }
    }

  }
}
