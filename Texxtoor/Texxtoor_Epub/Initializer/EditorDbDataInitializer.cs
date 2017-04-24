using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Texxtoor.Editor.Context;
using Texxtoor.Editor.Core.Extensions;
using Texxtoor.Editor.Models;
using Texxtoor.Editor.ViewModels;
using Texxtoor.Models;

namespace Texxtoor.Editor {

  public class EditorDbDataInitializer : DropCreateDatabaseAlways<EditorContext> {

    protected override void Seed(EditorContext context) {
      base.Seed(context);

      #region Author Portal

      Console.WriteLine("DemoContent");
      #region create a sample
      Func<IEnumerable<XElement>, List<Element>> helper = null;
      var currentChapter = String.Empty;
      var chapterOrder = 1;
      Document opus = null;
      helper = nodes => {
        var ret = new List<Element>();
        int orderNr = 1;
        foreach (var elm in nodes) {
          Element newElm = null;
          # region Detect Element Type
          switch (elm.Attribute("Type").Value.ToLower()) {
            case "opus":
              # region OPUS
              // create opus
              newElm = new Document {
                Name = elm.Attribute("Name").Value
              };
              opus = (Document)newElm;
              // this is an easy way to implement a workflow
              break;
              # endregion
            case "section":
              # region SECTION
              if (elm.FirstNode != null && elm.FirstNode.NodeType == System.Xml.XmlNodeType.Text) {
                newElm = new Section {
                  Content = System.Text.Encoding.UTF8.GetBytes(((XText)elm.FirstNode).Value.Trim())
                };
              } else {
                newElm = new Section {
                  Content = System.Text.Encoding.UTF8.GetBytes("Empty Section")
                };
              }
              if (elm.Attribute("Name") == null || String.IsNullOrEmpty(elm.Attribute("Name").Value)) {
                newElm.Name = System.Text.Encoding.UTF8.GetString(newElm.Content);
              } else {
                newElm.Name = elm.Attribute("Name").Value;
                if (elm.FirstNode == null) {
                  newElm.Content = System.Text.Encoding.UTF8.GetBytes(elm.Attribute("Name").Value);
                }
              }
              // Detect Chapter Elements to store resources in subfolders
              if (elm.Parent.Attribute("Type").Value == "Opus") {
                currentChapter = elm.Attribute("Name").Value;
              }
              break;
              # endregion
            case "text":
              # region TEXT
              newElm = new TextSnippet {
                Content = System.Text.Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Value.CleanUpString(15)
              };
              break;
              # endregion
            case "image":
              # region IMAGE
              var imgpath = elm.Value.Trim();
              Debug.Assert(Path.GetExtension(imgpath).Length > 0, "no extension " + imgpath);
              // get and optionally create folder after chapter
              //
              var res = new Resource {
                Name = elm.Attribute("Name").Value,
                OwnerDocument = opus,
                TypesOfResource = TypeOfResource.Content,
                MimeType = "image/" + Path.GetExtension(imgpath).Substring(1) // kick the leading "."
              };
              System.Drawing.Image img = null;
              var localPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "DemoContent", imgpath);
              if (File.Exists(localPath)) {
                var bytes = File.ReadAllBytes(localPath);
                res.Content = bytes;
              }
              context.Resources.Add(res);
              newElm = new ImageSnippet {
                Content = res.Content,
                Name = res.Name,
                Title = res.Name,
                MimeType = res.MimeType
              };
              var imgprops = new ImageProperties();
              if (elm.Attribute("Width") == null || elm.Attribute("Height") == null) {
                if (img != null) {
                  imgprops.ImageWidth = imgprops.OriginalWidth = img.Width;
                  imgprops.ImageHeight = imgprops.OriginalHeight = img.Height;
                } else {
                  imgprops.ImageWidth = imgprops.OriginalWidth = 100;
                  imgprops.ImageHeight = imgprops.OriginalHeight = 100;

                }
              } else {
                imgprops.ImageWidth = imgprops.OriginalWidth = Convert.ToInt32(elm.Attribute("Width").GetNullSafeValue());
                imgprops.ImageHeight = imgprops.OriginalHeight = Convert.ToInt32(elm.Attribute("Height").GetNullSafeValue());
              }
              ((ImageSnippet)newElm).Properties = new JavaScriptSerializer().Serialize(imgprops);
              break;
              # endregion
            case "listing":
              # region LISTING
              newElm = new ListingSnippet {
                Content = System.Text.UTF8Encoding.UTF8.GetBytes(elm.Value.Trim()), //.Replace("\n", " "); - Causes problems with Listing widget. All data is displayed in one line
                Name = elm.Attribute("Name") == null ? "Listing" : elm.Attribute("Name").Value,
                Title = elm.Attribute("Name") == null ? "Listing" : elm.Attribute("Name").Value,
                Language = elm.Attribute("Language") == null ? "" : elm.Attribute("Language").Value,
                SyntaxHighlight = elm.Attribute("Highlight") == null ? true : Boolean.Parse(elm.Attribute("Highlight").Value),
                LineNumbers = elm.Attribute("LineNumbers") == null ? true : Boolean.Parse(elm.Attribute("LineNumbers").Value)
              };
              # endregion
              break;
            case "table":
              # region TABLE
              newElm = new TableSnippet {
                Content = System.Text.Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Attribute("Name") == null ? "Table" : elm.Attribute("Name").Value,
                Title = elm.Attribute("Name") == null ? "Table" : elm.Attribute("Name").Value,
                RepeatHeadRow = elm.Attribute("RepeatHeadRow") == null ? true : Boolean.Parse(elm.Attribute("RepeatHeadRow").Value)
              };
              # endregion
              break;
            case "sidebar":
              # region SIDEBAR
              newElm = new SidebarSnippet {
                Content = System.Text.Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Attribute("Name") == null ? "Sidebar" : elm.Attribute("Name").Value,
                SidebarType = elm.GetEnumAttribute<SidebarType>("SidebarType")
              };
              # endregion
              break;
            default:
              throw new NotSupportedException("Unknown snippet type found in source XML: " + elm.Attribute("Type").GetNullSafeValue());
          }
          # endregion
          newElm.Children = helper(elm.Elements("Element"));
          newElm.OrderNr = orderNr++;
          ret.Add(newElm);
        }
        return ret;
      };
      // invoke Content loader (assume each xml contains one Opus)
      try {
        foreach (var importFile in Directory.GetFiles("DemoContent")) {
          var doc = XDocument.Load(importFile);
          helper(from o in doc.Root.Elements("Element") select o).ForEach(o => context.Elements.Add(o));
        }
        context.SaveChanges();
      } catch (Exception ex) {
        Console.WriteLine(ex.Message + " > " + ((ex.InnerException != null) ? ex.InnerException.Message : ""));
        throw;
      }
      # endregion

      #endregion
      Console.WriteLine("Terms");
      # region Terms
      context.Terms.Add(new Term { Text = "Visual Basic", Content = "VB", LocaleId = "en", TermType = TermType.Abbreviation});
      context.Terms.Add(new Term { Text = "Turbo Pascal", Content = "TP", LocaleId = "en", TermType = TermType.Abbreviation });
      context.Terms.Add(new Term { Text = "C Sharp", Content = "c#", LocaleId = "en", TermType = TermType.Abbreviation });
      context.Terms.Add(new Term { Text = "ASPHanser2010", Content = "ASP.NET 4 Carl Hanser Verlag 2010", LocaleId = "en", TermType = TermType.Cite });
      context.Terms.Add(new Term { Text = "ASPApress2009", Content = "ASP.NET Extensibility Apress 2009", LocaleId = "en", TermType = TermType.Cite });
      context.Terms.Add(new Term { Text = "UserName", Content = "Member.UserName", LocaleId = "", TermType = TermType.Variable }); // neutral culture
      context.Terms.Add(new Term { Text = "UserMail", Content = "Member.UserEmail", LocaleId = "", TermType = TermType.Variable }); // neutral culture
      # endregion
      context.SaveChanges();
    }

  }

}
