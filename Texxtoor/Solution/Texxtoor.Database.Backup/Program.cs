using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using Texxtoor.DataModels.Context;

namespace Texxtoor.Database.Backup {
  class Program {
    static void Main(string[] args) {
# if !DEBUG
      var ctx = new PortalContext("PortalContext");
# else
      var ctx = new PortalContext();
# endif
      Console.WriteLine(ctx.Countries.Count());
      Console.WriteLine("Save Resources...");
      var c = ctx.Localization.Count();
      Console.WriteLine("Save {0} Resources...", c);
      try {
        // try to save
        var xLoc = new XDocument(
          new XElement("localization",
            ctx.Localization
            .Where(l => l.LocaleId == "")
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
        xLoc.Save("../../Localization.xml", SaveOptions.None);
        xLoc = new XDocument(
          new XElement("localization",
            ctx.Localization
            .Where(l => l.LocaleId == "de")
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
        xLoc.Save("../../Localization.de.xml", SaveOptions.None);
        if (File.Exists("../../Localization.xml")) {
          if (File.Exists("../../../DatabaseTest/Localization.xml")) {
            File.Delete("../../../DatabaseTest/Localization.xml");
          }
          File.Copy("../../Localization.xml", "../../../DatabaseTest/Localization.xml");
        }
      } catch {
      }
    }
  }
}