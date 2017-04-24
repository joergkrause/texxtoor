using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Texxtoor.Editor.Models;
using Texxtoor.Editor.Utilities;
using Texxtoor.Editor.ViewModels;
using Texxtoor.Models;
using Texxtoor.Models.Attributes;

namespace Texxtoor.Editor.Core
{

    public delegate string CreateImageHandler(object sender, CreateImageArguments e);

    /// <summary>
    /// All BLL functions for projects.
    /// </summary>
    public class ProjectManager : Manager<ProjectManager>
    {

        public Document GetDocumentFromSnippetId(int id)
        {
            var snippet = Ctx.Elements.Find(id);
            Func<Element, Element> findParent = null;
            findParent = e => (e.Parent is Document) ? e.Parent : findParent(e.Parent);
            return findParent(snippet) as Document;
        }

        public Document GetDocument(int docId)
        {
            var opus = Ctx.Documents.Find(docId);
            return opus;
        }

        private void AddSectionToChapter(Section chapter)
        {
            const string bt = @"Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";
            var s1 = new Section { Content = System.Text.Encoding.UTF8.GetBytes("First Section"), Name = "Section", OrderNr = 1, Parent = chapter };
            Ctx.Elements.Add(s1);
            var t1 = new TextSnippet { Content = System.Text.Encoding.UTF8.GetBytes(bt), Name = "Paragraph", OrderNr = 1, Parent = s1 };
            Ctx.Elements.Add(t1);
            s1.Children = new List<Element> { t1 };
            chapter.Children = new List<Element> { s1 };
        }

        public List<Resource> GetResourceFiles(int projectId, TypeOfResource type, string mimeType)
        {
            return GetResourceFiles(projectId, type, mimeType, "");
        }

        public List<Resource> GetResourceFiles(int opusId, TypeOfResource type, string mimeType, string namePattern)
        {
            var res = Ctx.Resources.OfType<Resource>().Where(r => r.Id == opusId && r.TypesOfResource == type).ToList();
            if (!String.IsNullOrEmpty(mimeType) && String.IsNullOrEmpty(namePattern))
            {
                return res.Where(r => r.MimeType.StartsWith(mimeType)).ToList();
            }
            if (!String.IsNullOrEmpty(namePattern) && String.IsNullOrEmpty(mimeType))
            {
                return res.Where(r => r.Name.EndsWith(namePattern)).ToList();
            }
            if (!String.IsNullOrEmpty(namePattern) && !String.IsNullOrEmpty(mimeType))
            {
                return res.Where(r => r.Name.EndsWith(namePattern) && r.MimeType.StartsWith(mimeType)).ToList();
            }
            return res;
        }

        public List<Resource> GetResourceFiles(Document opus, TypeOfResource type, string mimeType)
        {
            return GetResourceFiles(opus.Id, type, mimeType, null);
        }

        public List<Resource> GetResourceFiles(Document opus, TypeOfResource type, string mimeType, string namePattern)
        {
            return GetResourceFiles(opus.Id, type, mimeType, namePattern);
        }

        public Resource GetResource(int id)
        {
            return Ctx.Resources.FirstOrDefault(r => r.Id == id);
        }

        public SelectList GetSemanticListForDocument(int opusId, string typeOfList)
        {
            var ops = Ctx.Documents.Find(opusId);
            IEnumerable<KeyValuePair<int, string>> lst = null;
            var select = new Func<Term, KeyValuePair<int, string>>(t => new KeyValuePair<int, string>(t.Id, t.Content));
            // if project is NOT SET for termset it applies for ALL projects
            switch (typeOfList.ToLowerInvariant())
            {
                case "abbreviation":
                    lst = Ctx.Terms.Where(t => t.TermType == TermType.Abbreviation).Select(select);
                    break;
                case "cite":
                    lst = Ctx.Terms.Where(t => t.TermType == TermType.Cite).Select(select);
                    break;
                case "idiom":
                    lst = Ctx.Terms.Where(t => t.TermType == TermType.Idiom).Select(select);
                    break;
                case "variable":
                    lst = Ctx.Terms.Where(t => t.TermType == TermType.Variable).Select(select);
                    break;
                case "definition":
                    lst = Ctx.Terms.Where(t => t.TermType == TermType.Definition).Select(select);
                    break;
                case "link":
                    lst = Ctx.Terms.Where(t => t.TermType == TermType.Link).Select(select);
                    break;
            }
            var sl = new SelectList(lst, "Key", "Value");
            return sl;
        }

        public IEnumerable<Document> GetAllDocuments()
        {
            return Ctx.Documents.ToList();
        }

        public Document AddDocument()
        {
            var doc = new Document
            {
                Name = "Test Document",
                OrderNr = 1,
                Children = new List<Element>(new Element[] { 
          new Section {
           Name = "Chapter 1",
           Content = Encoding.UTF8.GetBytes("Chapter 1"),
           OrderNr = 1,
           Children = new List<Element>(new Element[] {
             new TextSnippet {
               Name = "Text 1",
               OrderNr = 1,
               Content = Encoding.UTF8.GetBytes("<p>Welcome to WYMIX Editor</p>")
             }
           })
          }
        })
            };
            Ctx.Documents.Add(doc);
            SaveChanges();
            return doc;
        }

        /// <summary>
        /// Create deep level copy of an opus
        /// </summary>
        /// <param name="document"></param>
        /// <param name="withNumbers"> </param>
        /// <returns></returns>
        public string CreateHtml(Document document, bool withNumbers = true)
        {
            var final = CreateFinalHtml(document, withNumbers);
            var path = Path.Combine(HttpContext.Current.Server.MapPath("~/data"), document.Name + ".html");
            using (var file = new StreamWriter(path, false, Encoding.UTF8))
            {
                file.Write(final);
                file.Close();
            }
            return Path.Combine("/data", document.Name + ".html");
        }

        public string CreateFinalHtml(Document document, bool withNumbers = true)
        {
            var flatContent = new StringBuilder();
            flatContent.Append(CreateHtmlInner(document.Children.OfType<Snippet>(), withNumbers));
            var html = flatContent.ToString();

            var final = String.Format(@"<!DOCTYPE HTML>
<html>
  <head>
    <link rel=""stylesheet"" href=""document.css"" />
  </head>
  <body>
    {0}
  </body>
</html>
        ",
              html);
            return final;
        }

        public string CreateHtmlInner(IEnumerable<Snippet> source, bool withNumbers)
        {
            // each snippet has its very own numbering schema
            var numbering = new Dictionary<string, NumberingSchema> {        
        {"Section1", new NumberingSchema { Major = 1, Separator = '.', Divider = "", Label = "&nbsp;&nbsp;"}}, 
        {"Section2", new NumberingSchema { Major = 1, Minor = 1, Separator = '.', Divider = "", Label = "&nbsp;&nbsp;", IncludeParent = true}},
        {"Section3", new NumberingSchema { Major = 1, Minor = 1, Separator = '.', Divider = "", Label = "&nbsp;&nbsp;", IncludeParent = true}},
        {"Section4", new NumberingSchema { Major = 1, Minor = 1, Separator = '.', Divider = "", Label = "&nbsp;&nbsp;", IncludeParent = true}},
        {"ImageSnippet", new NumberingSchema { Major = 1, Minor = 1, Separator = '-', Divider = ": ", Label = "Figure " }}, 
        {"TableSnippet", new NumberingSchema { Major = 1, Minor = 1, Separator = '-', Divider = ": ", Label = "Table " }},
        {"ListingSnippet", new NumberingSchema { Major = 1, Minor = 1, Separator = '-', Divider = ": ", Label = "Listing " }}
      };
            var flatContent = new StringBuilder();
            foreach (var elm in source)
            {
                // Get the final HTML
                var builder = elm.GetType().GetCustomAttributes(typeof(SnippetElementAttribute), true).OfType<SnippetElementAttribute>().Single();
                var html = builder.BuildHtml(elm, numbering);
                // Resource based elements get the resource's content as child element
                if (builder.CreateResource)
                {
                    throw new NotImplementedException();
                }
                flatContent.Append(html);
                if (elm.HasChildren())
                {
                    flatContent.Append(CreateHtmlInnerRecursively(elm.Children.OfType<Snippet>(), numbering));
                }
                // add chapter and reset all other counters
                numbering["Section1"].Major = numbering["Section1"].Major + 1;
                numbering["Section2"].Major = numbering["Section1"].Major;
                numbering["Section3"].Major = numbering["Section1"].Major;
                numbering["Section4"].Major = numbering["Section1"].Major;
                numbering["ImageSnippet"].Major = numbering["Section1"].Major;
                numbering["TableSnippet"].Major = numbering["Section1"].Major;
                numbering["ListingSnippet"].Major = numbering["Section1"].Major;
                numbering["Section1"].Minor = 1;
                numbering["Section2"].Minor = 1;
                numbering["Section3"].Minor = 1;
                numbering["Section4"].Minor = 1;
                numbering["ImageSnippet"].Minor = 1;
                numbering["TableSnippet"].Minor = 1;
                numbering["ListingSnippet"].Minor = 1;
            }
            return flatContent.ToString();
        }

        public event CreateImageHandler CreateImage;

        private string CreateHtmlInnerRecursively(IEnumerable<Snippet> snippets, IDictionary<string, NumberingSchema> numbering)
        {
            var flatContent = new StringBuilder();
            Action<Snippet> convert = null;
            convert = e =>
            {
                var builder = e.GetType().GetCustomAttributes(typeof(SnippetElementAttribute), true).OfType<SnippetElementAttribute>().Single();
                if (builder.CreateResource)
                {
                    string itemHref = String.Empty;
                    // BaseType because it's a proxy element; if database context changes behavior, change this
                    switch (e.GetType().BaseType.Name)
                    {
                        case "ImageSnippet":
                            // pull resource from BLOB storage
                            if (e.Content != null)
                            {
                                itemHref = ((ImageSnippet)e).ItemHref;
                                var properties = System.Web.Helpers.Json.Decode<ImageProperties>(((ImageSnippet)e).Properties);
                                var image = ImageUtil.ApplyImageProperties(e.Content, properties);
                                if (image != null)
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        image.Save(ms, ImageFormat.Png);
                                        byte[] bytes = ms.ToArray();
                                        ((ImageSnippet)e).ItemHref = CreateImage(this, new CreateImageArguments { Content = bytes, FileName = itemHref });
                                    }
                                }
                                else
                                {
                                    // create a placeholder image
                                    var fs = File.ReadAllBytes(HttpContext.Current.Server.MapPath("~/Images/bullet.png"));
                                    ((ImageSnippet)e).ItemHref = CreateImage(this, new CreateImageArguments { Content = fs, FileName = itemHref });
                                }
                            }
                            break;
                    }
                    flatContent.Append(builder.BuildHtml(e, numbering));
                }
                else
                {
                    flatContent.Append(builder.BuildHtml(e, numbering));
                    // if resource do not add again
                    if (e.HasChildren())
                    {
                        e.Children.OfType<Snippet>().ToList().ForEach(c => convert(c));
                    }
                }
            };
            snippets.ToList().ForEach(c => convert(c));
            return flatContent.ToString();
        }


        public void ClearDataFolder(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                if (file.EndsWith("css")) continue;
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}