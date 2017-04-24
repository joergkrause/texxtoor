using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Texxtoor.BaseLibrary.EPub;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.BaseLibrary.Core.HtmlAgility.Pack;
using Texxtoor.DataModels.Models.Reader.Content;

namespace ImportEPubDocument {
  class Program {
    static void Main(string[] args) {
      LoadCatalog();
      Console.ReadLine();
    }

    private static void LoadCatalog() {
      // load epub
      foreach (var lang in Directory.GetDirectories("epubs")) {
        if (lang.StartsWith(".")) continue;
        Console.WriteLine(" - Load EPubs from language " + lang);
        var locale = Path.GetFileName(lang);
        // put all books in the first categorie of the suitable language
        foreach (var file in Directory.GetFiles(lang, "*.epub")) {
          string coverPath = Path.Combine(lang, Path.GetFileNameWithoutExtension(file) + ".jpg");
          byte[] content = File.ReadAllBytes(file);
          EpubBook book = EBookFactory.Create(content);
          book.CoverImage = File.Exists(coverPath) ? File.ReadAllBytes(coverPath) : null;
          book.CoverDescription = String.Format("{0} of {1} published at {2}",
            book.PackageData.MetaData.Title,
            book.PackageData.MetaData.Creator,
            book.PackageData.MetaData.Publisher);
          Console.WriteLine(" - EPub done " + file);
          // the published book simulates a book as it were written be an author, based on the uploaded EPUB
          Published p = new Published();
          SpineParser(book, p);
          WriteFragments(p, Path.GetFileNameWithoutExtension(file) + ".txt");
        } // epubs
      } // epub
      Console.WriteLine(" - Done");
    }

    private static void SpineParser(EpubBook book, Published p) {
      var spineIds = book.PackageData.Spine.ItemRefs.Select(i => i.IdRef).ToList();
      var data = book.PackageData.Manifest.Items;
      // 2. Get the content elements
      var content = data.Where(i => spineIds.Any(sid => sid == i.Identifier)).Select(i => new {
        Data = i.Data,
        Identifier = i.Href,
        Name = i.Identifier 
      });
      FrozenFragment rootFragment = new FrozenFragment {
        Name = "Root",
        TypeOfFragment = FragmentType.Meta,
        Children = new List<FrozenFragment>()
      };
      int orderNr = 1;
      foreach (var file in content) {
        var c = UTF8Encoding.UTF8.GetString(file.Data);
        CreateTextFragment(rootFragment, ref c, data);        
      }
      p.FrozenFragments = rootFragment.Children;
    }

    private static FrozenFragment CreateTextFragment(FrozenFragment parent, ref string c, IList<ManifestItem> data) {
      // if no text just proceed
      if (c.Length == 0) return parent;
      // close open tags or open closed tags
      HtmlDocument nodeCheck = new HtmlDocument();
      nodeCheck.OptionAutoCloseOnEnd = true;
      nodeCheck.OptionCheckSyntax = true;
      nodeCheck.OptionFixNestedTags = true;
      nodeCheck.LoadHtml(c);
      if (nodeCheck.ParseErrors.Count() > 0) {
        // handle parser errors
      }
      var f = new FrozenFragment {
        Name = "Paragraph",
        TypeOfFragment = FragmentType.Html,
        Parent = parent
      };
      var body = nodeCheck.DocumentNode.SelectSingleNode("//body");
      CreateEmbeddedImages(f, body, data);
      f.Content = UTF8Encoding.UTF8.GetBytes(body.InnerHtml);
      parent.Children.Add(f);
      c = String.Empty;
      return f;
    }

    private static void CreateEmbeddedImages(FrozenFragment parent, HtmlNode element, IList<ManifestItem> data) {
      if (element.SelectNodes(".//img") == null) return;
      foreach (var img in element.SelectNodes(".//img")) {
        var alt = img.Attributes["alt"] != null ? img.Attributes["alt"].Value : "Generic Image";
        var cnt = data.FirstOrDefault(d => img.Attributes["src"].Value.EndsWith(d.Href));
        var r = new FrozenFragment {
          Name = alt,
          Content = cnt.Data,
          TypeOfFragment = FragmentType.Image,
          ItemHref = Guid.NewGuid().ToString()
        };
        if (parent.Children == null) {
          parent.Children = new List<FrozenFragment>();
        }
        parent.Children.Add(r);
        img.Attributes["src"].Value = r.ItemHref;
      }
    }

    private static void WriteFragments(Published p, string fileName) {
      if (File.Exists(fileName)) File.Delete(fileName);
      var sb = new StringBuilder();
      WriteFragment(sb, p.FrozenFragments, 0);
      StreamWriter fs = null;
      try {
        fs = File.CreateText(fileName);
        fs.Write(sb.ToString());
      } finally {
        fs.Close();
        fs.Dispose();
      }
    }

    private static void WriteFragment(StringBuilder sb, IList<FrozenFragment> fragment, int indent) {
      foreach (var f in fragment) {
        sb.Append("".PadLeft(indent, ' '));
        sb.Append(indent);
        if (f.TypeOfFragment == FragmentType.Html) {
          sb.AppendLine(ASCIIEncoding.ASCII.GetString(f.Content));
        } else {
          sb.AppendLine("IMG!!" + f.Name);
        }
        if (f.HasChildren()) {
          indent++;
          WriteFragment(sb, f.Children, indent);
          indent--;
        }
      }
    }



  }
}