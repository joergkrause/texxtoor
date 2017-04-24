using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Model.Cms.Localization;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataBase.Initializer {

  internal class Program {
    public static User Author;

    private static void Main(string[] args) {
      System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
      var targetDeployment = "T";
      do {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Creating Portal context ... please select target");
        Console.WriteLine("Local    (L)");
        Console.WriteLine("Texxtoor (T)");
        Console.WriteLine("AC²      (2)");
        Console.WriteLine("Besit    (B)");
        Console.WriteLine("MyManuals(M)");
        Console.WriteLine("MyManuals(A) - Local");
        Console.Write("Enter: ");
        var target = Console.ReadKey().KeyChar.ToString().ToLower();
        var conn = "PortalContext";
        switch (target) {
          case "a":
            conn += "-MyManuals-Local";
            targetDeployment = "MM";
            break;
          case "l":
            conn += "-Local";
            targetDeployment = "T";
            break;
          case "2":
            conn += "-Ac2";
            targetDeployment = "AC2";
            break;
          case "b":
            conn += "-Ac2-Besit";
            targetDeployment = "AC2";
            break;
          case "t":
            conn += "-Texxtoor";
            targetDeployment = "T";
            break;
          case "m":
            conn += "-MyManuals";
            targetDeployment = "MM";
            break;
        }
        Console.WriteLine();
        Console.WriteLine("Provide Password, if necessary (all but local): ");
        var pw = Console.ReadLine();
        var connstring = ConfigurationManager.ConnectionStrings[conn].ConnectionString.Replace("***", pw);
        Console.WriteLine("Choosing Context Connection: {0}", conn);
        Console.WriteLine("Using this actual string: {0}", connstring);
        var ctx = new PortalContext(connstring);
        var pi = new PortalDbDataInitializer(ctx);
        pi.TargetDeployment = targetDeployment;
        Console.WriteLine("Create (C)");
        Console.WriteLine("Init (I)");
        Console.WriteLine("Function Call (F)");
        Console.WriteLine("Backup Localization (B)");
        Console.WriteLine("Restore Localization? (R)");
        Console.Write("Enter: ");
        var task = Console.ReadKey().KeyChar.ToString().ToLower();
        Console.WriteLine("");
        switch (task) {
          case "e":
            Export(ctx.Terms.ToList());
            Export(ctx.Users.ToList());
            Export(ctx.UserProfiles.ToList());
            Export(ctx.Applications.ToList());
            Export(ctx.Countries.ToList());            
            break;
          case "i":
            pi.Initialize(targetDeployment);
            break;
          case "f":
            string func = "";
            do {
              Console.WriteLine("");
              Console.WriteLine("What function? (LoadUsers, CreateMailtemplates, CreateJobcategories, AddIsbn, CreateTemplates, RefreshCms, LoadCatalog, CheckLocalres, loadCouNtries, AddDemoproject)");
              func = Console.ReadLine();
            } while (String.IsNullOrEmpty(func));
            Database.SetInitializer<PortalContext>(null);
            var app = ctx.Applications.SingleOrDefault(application => application.ApplicationName == "/") ?? pi.CreateRootApp();
            switch (func.ToLowerInvariant()) {
              case "loadusers":
              case "lu":
                pi.LoadUsers();
                break;
              case "loadcatalog":
              case "lc":
                pi.LoadCatalog(app);
                break;
              case "createmailtemplates":
              case "cm":
                pi.CreateMailTemplate();
                break;
              case "createjobcategories":
              case "cj":
                pi.CreateJobCategories();
                break;
              case "addisbn":
              case "ai":
                pi.AddIsbnNumbers();
                break;
              case "createtemplates":
              case "ct":
                var tAdmin = ctx.Users.FirstOrDefault(u => u.UserName == "Admin");
                pi.CreateTemplates(tAdmin, tAdmin, null, null);
                break;
              case "refreshcms":
              case "rc":
                var adminUser = ctx.Users.FirstOrDefault(u => u.UserName == "Admin");
                pi.LoadCmsData(app, adminUser, targetDeployment);
                break;
              case "checklocalres":
              case "cl":
                CheckLocalResources(ctx);
                break;
              case "loadcountries":
              case "cn":
                pi.LoadCountries();
                break;
              case "adddemoproject":
              case "ad":
                pi.AddDemoProjects();
                break;
            }
            break;
          case "c":
            // attempt to delete first
            Database.SetInitializer(pi);
            //try {
            //  if (ctx.Database != null) {
            //    if (ctx.Database.Connection.State == System.Data.ConnectionState.Open) {
            //      ctx.Database.Connection.Close();
            //    }
            //    ctx.Database.Delete();
            //  }
            //} catch (Exception) { }
            Console.WriteLine("Creating Database ...");
            break;
          case "b":
            BackupResources(ctx, targetDeployment);
            break;
          case "r":
            pi.LoadLocaleResources();
            break;
        }
        try {          
          var a = ctx.Applications.ToList();
          ctx.SaveChanges();
        } catch (Exception ex) {

          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine(ex.Message);
          Console.ForegroundColor = ConsoleColor.White;
        }
        Console.WriteLine("Done...x to exit or any key to repeat.");
      } while (Console.ReadKey().KeyChar.ToString().ToLower() != "x");
      Console.WriteLine("Terminating...any key to exit.");
      Console.ReadLine();
    }

    public static void BackupResources(PortalContext ctx, string targetDeployment) {
      Console.WriteLine("Backup Resources...");
      var c = ctx.Localization.Count();
      Console.WriteLine("{0} Resources detected", c);
      try {
        var fbData = ctx.Localization
                        .Where(l => l.LocaleId == "")
                        .OrderBy(l => l.ResourceSet)
                        .ThenBy(l => l.ResourceId)
                        .ThenBy(l => l.LocaleId)
                        .ToList();
        // try to save
        var xLoc = new XDocument(
          new XElement("localization",
                        fbData
                       .Select(l => new XElement("loc",
                                                             new XAttribute("type", l.GetType().Name),
                                                             new XAttribute("localeid", l.LocaleId),
                                                             new XAttribute("resid", l.ResourceId),
                                                             new XAttribute("set", l.ResourceSet),
                                                             new XElement("data", l.Value))).ToList()
            )
          );
        Console.WriteLine("Save Fallback...");
        xLoc.Save("../../localize/" + targetDeployment + "/Localization.xml", SaveOptions.None);
        var deData = ctx.Localization
                        .Where(l => l.LocaleId == "de")
                        .OrderBy(l => l.ResourceSet)
                        .ThenBy(l => l.ResourceId)
                        .ThenBy(l => l.LocaleId)
                        .ToList();
        xLoc = new XDocument(
          new XElement("localization",
                        deData
                        .Select(l => new XElement("loc",
                                                             new XAttribute("type", l.GetType().Name),
                                                             new XAttribute("localeid", l.LocaleId),
                                                             new XAttribute("resid", l.ResourceId),
                                                             new XAttribute("set", l.ResourceSet),
                                                             new XElement("data", l.Value))).ToList()
            )
          );
        Console.WriteLine("Save de...");
        xLoc.Save("../../localize/" + targetDeployment + "/Localization.de.xml", SaveOptions.None);
        Console.WriteLine("Get Delta");
        xLoc = new XDocument(
          new XElement("localization",
                        fbData
                        .Except(deData, new SoftComparer())
                        .Select(l => new XElement("loc",
                                                             new XAttribute("type", l.GetType().Name),
                                                             new XAttribute("localeid", l.LocaleId),
                                                             new XAttribute("resid", l.ResourceId),
                                                             new XAttribute("set", l.ResourceSet),
                                                             new XElement("data", l.Value))).ToList()
            )
          );
        xLoc.Save("../../localize/" + targetDeployment + "/Localization.missing.xml", SaveOptions.None);
        xLoc = new XDocument(
         new XElement("localization",
                deData
                .Except(fbData, new SoftComparer())
                .Select(l => new XElement("loc",
                                                     new XAttribute("type", l.GetType().Name),
                                                     new XAttribute("localeid", l.LocaleId),
                                                     new XAttribute("resid", l.ResourceId),
                                                     new XAttribute("set", l.ResourceSet),
                                                     new XElement("data", l.Value))).ToList()
    )
  );
        xLoc.Save("../../localize/" + targetDeployment + "Localization.overloaded.xml", SaveOptions.None);
      } catch (Exception ex) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: " + ex.Message);
        Console.ForegroundColor = ConsoleColor.White;
      }
      Console.WriteLine("Backup done.");
    }

    public static void CheckLocalResources(PortalContext context) {
      // retrieve all cshtml files recursively
      var path = @"D:\Apps\Firma\ABG\Project\Texxtoor\Texxtoor.Solution\Texxtoor.Portal";
      var flatFiles = new List<FileInfo>();
      Func<DirectoryInfo, FileInfo[]> getFiles = null;
      getFiles = d => {
        var files = d.GetFiles("*.cshtml");
        if (files.Length > 0) {
          flatFiles.AddRange(d.GetFiles("*.cshtml"));
        }
        var dirs = d.GetDirectories();
        foreach (var dir in dirs) {
          files = getFiles(dir);
          if (files.Length > 0) {
            flatFiles.AddRange(d.GetFiles("*.cshtml"));
          }
        }
        return files;
      };
      Console.WriteLine("");
      Console.Write("get files, ");
      getFiles(new DirectoryInfo(path));
      //@Html.Loc("title", "Change Password")
      var rx = new Regex(@"(@|Html\.)Loc\(""(?<key>.*?)"", ""(?<def>.*?)""\)|,", RegexOptions.Compiled | RegexOptions.IgnoreCase);
      var catchedPatterns = new List<Tuple<string, string, string>>();
      foreach (var flatFile in flatFiles) {
        var data = File.ReadAllText(flatFile.FullName);
        foreach (Match match in rx.Matches(data)) {
          if (match.Success) {
            var key = match.Groups["key"].Value;
            var val = flatFile.FullName.Replace(path, "~").Replace("\\", "/");
            if (catchedPatterns.Exists(p => p.Item1 == key && p.Item2 == val)) continue;
            var def = match.Groups["def"].Value; ;
            catchedPatterns.Add(new Tuple<string, string, string>(key, val, def));
          }
        }
      }
      Console.Write("locate {0} files, ", flatFiles.Count());
      // now we have all active matches, so we can check against the current Localization table
      var hits = new List<StringResource>();
      foreach (var catchedPattern in catchedPatterns) {
        var hit = context.Localization.OfType<StringResource>().Where(l => l.ResourceId == catchedPattern.Item1 && l.ResourceSet == catchedPattern.Item2);
        if (hit.Any()) {
          hits.AddRange(hit);
        }
      }
      Console.Write("get diff {0} hits, ", hits.Count());
      // get in DB but never used anywhere
      var existingEn = context.Localization.OfType<StringResource>().Where(l => l.LocaleId == "").ToList();
      var existingDe = context.Localization.OfType<StringResource>().Where(l => l.LocaleId == "de").ToList();
      var nohitsEn = existingEn.Except(hits).ToList();
      var nohitsDe = existingDe.Except(hits).ToList();
      Console.Write("remove {0} en, ", nohitsEn.Count());
      foreach (var resourceBase in nohitsEn) {
        //context.Localization.Remove(resourceBase);
        Console.WriteLine("{0} / {1}", resourceBase.ResourceId, resourceBase.ResourceId);
      }
      Console.Write("remove {0} de, ", nohitsDe.Count());
      foreach (var resourceBase in nohitsDe) {
        //context.Localization.Remove(resourceBase);
        Console.WriteLine("{0} / {1}", resourceBase.ResourceId, resourceBase.ResourceId);
      }
      Console.Write("write, ");
      //context.SaveChanges();
      existingEn = context.Localization.OfType<StringResource>().Where(l => l.LocaleId == "").ToList();
      Console.Write("adding missing, ");
      var missingEn = hits.Except(existingEn);
      foreach (var catchedPattern in catchedPatterns) {
        var hit = context.Localization.OfType<StringResource>().Where(l => l.ResourceId == catchedPattern.Item1 && l.ResourceSet == catchedPattern.Item2 && l.LocaleId == "");
        if (!hit.Any()) {
          //context.Localization.Add(new StringResource {
          //  LocaleId = "",
          //  ResourceId = catchedPattern.Item1,
          //  ResourceSet = catchedPattern.Item2,
          //  Value = catchedPattern.Item3
          //});
          Console.WriteLine("{0} / {1}", catchedPattern.Item1, catchedPattern.Item2);
        }
      }
      Console.Write("done.");
    }

    private static void Export<T>(List<T> data) {
      var ser = new XmlSerializer(typeof(List<T>));
      using (var writer = new StreamWriter(String.Format(@"..\..\export\{0}.xml", typeof (T).Name))) {
        ser.Serialize(writer, data);
      }
    }

  }

  public class SoftComparer : IEqualityComparer<ResourceBase> {

    public bool Equals(ResourceBase x, ResourceBase y) {
      return (x.ResourceId == y.ResourceId && x.ResourceSet == y.ResourceSet);
    }

    public int GetHashCode(ResourceBase obj) {
      return 0;
    }
  }

  public static class StaticCommand {
    public static string GrantAccess =
      @"USE [PortalDatabase] 
IF NOT EXISTS(SELECT principal_id FROM sys.server_principals WHERE name = 'IIS APPPOOL\ASP.NET v4.0') BEGIN
    EXEC('CREATE LOGIN [IIS APPPOOL\ASP.NET v4.0] FROM WINDOWS WITH DEFAULT_DATABASE=[PortalDatabase], DEFAULT_LANGUAGE=[us_english]')
END
IF NOT EXISTS(SELECT principal_id FROM sys.database_principals WHERE name = 'IISUser') BEGIN
    EXEC('CREATE USER [IISUser] FOR LOGIN [IIS APPPOOL\ASP.NET v4.0]')
END
EXEC('sp_addrolemember ""db_owner"", ""IISUser""')";
  }



}
