using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using Texxtoor.DataModels.Context;

namespace Texxtoor.Database.BackupAll {
  class Program {
    private static void Main(string[] args) {
      Console.WriteLine("Start");
      var ctx = new PortalContext("PortalContext-Local");
      var c = ctx.Applications.Count();
      Console.WriteLine("Done. Press key...");
      Console.ReadLine();
    }
  }
}