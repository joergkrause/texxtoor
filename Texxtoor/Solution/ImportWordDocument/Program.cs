using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Texxtoor.BaseLibrary;
using Texxtoor.DataModels.Models.Content;

namespace ImportWordDocument {
  internal class Program {
    private static void Main(string[] args) {

      string path, html;
      var defKey = "y";
      XDocument xml;
      do {
        Console.Write("Import Word HTML (H), Word (W), XML (X), Cleanup (C) HTML? ");
        XDocument fixTableXslt;
        XDocument fixListingXslt;
        XDocument config = XDocument.Load("config.xml");
        switch (Console.ReadKey(false).KeyChar.ToString().ToLower()) {
          case "t":
            fixTableXslt = Html2XmlUtil.GenerateFixTableXslt(config);
            break;
          case "h":
            path = GetPath("Word HTML");
            // generate the transform files from config on-the-fly
            fixTableXslt = Html2XmlUtil.GenerateFixTableXslt(config);
            fixListingXslt = Html2XmlUtil.GenerateFixListingXslt(config);
            ImportSingleHtml(path, fixTableXslt, fixListingXslt);
            break;
          case "w":
            Console.WriteLine("");
            path = GetPath("Word DOCX");
            break;
          case "x":
            Console.WriteLine("");
            path = GetPath("texxtoor XML");
            break;
          case "c":
            Console.WriteLine("");
            path = GetPath("Word HTML (clean only)");
            break;
          default:
            Console.WriteLine("");
            Console.Write("Unknown key. ");
            break;
        }
        Console.Write("Repeat (y) or End (n)?");
        defKey = Console.ReadKey(false).KeyChar.ToString().ToLower();
        Console.WriteLine("");
      } while (defKey == "y");
      Console.WriteLine("done");
      // TODO: Check results by creating a preview in plain HTML
      Console.ReadLine();
    }

    private static string GetPath(string forType) {
      Console.WriteLine("");
      Console.WriteLine("Import {0}", forType);
      string ret = "";
      do {
        Console.Write("Path to file (type path + ENTER, clear + ENTER to end): ");
        ret = Console.ReadLine();
        if (!File.Exists(ret) && !String.IsNullOrEmpty(ret)) {
          Console.WriteLine("File '{0}' does not exist.", ret);
        }
        else {
          break;
        }
      } while (true);
      Console.WriteLine("");
      return ret;
    }

    private static void ImportSingleHtml(string path, XDocument tableFix, XDocument listingFix) {
      var html = File.ReadAllText(path, Encoding.UTF8);
      var name = Path.GetFileNameWithoutExtension(path);
      Console.WriteLine("Convert start");
      try {
        // convert prepared HTML into internal <Content> XML (backup and restore format)
        var xml = Html2XmlUtil.HtmlToOpusXsltParser(name, html, tableFix, listingFix);
        using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xml))) {
          Console.WriteLine("Parser");
          var xDoc = XDocument.Load(xmlStream);
        }
      }
      catch (Exception ex) {
        var cc = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        Console.ForegroundColor = cc;
      }
    }
  }
}
