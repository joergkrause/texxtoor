using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.HtmlAgility.ToXml;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary.WordInterop;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models.Content;

namespace ImportWordHtmlConsole {
  internal class Program {

    [STAThread]
    private static void Main(string[] args) {
      var of = new OpenFileDialog();
      if (of.ShowDialog() == DialogResult.OK) {
        var name = of.FileName;
        var target = Path.GetFileNameWithoutExtension(name);
        var mapping = new Import();
        // Read File
        using (var s = File.Open(name, FileMode.Open)) {
          var content = ImportZip(s, target);
          var result = ImportSingleHtml(content, mapping, target);
          Console.WriteLine("{0} Kapitel", result.Children.Count());
        }
        Console.ReadLine();
      }
    }

    public static byte[] ImportZip(Stream file, string name) {
      file.Position = 0;
      var fileData = file.ReadToEnd();
      // take all images in the zip and convert to base64 encoded inline images
      var xDoc = ProcessZipFile(fileData);
      using (var memStream = new MemoryStream()) {
        var writer = XmlWriter.Create(memStream);
        xDoc.Save(writer);
        writer.Close();
        memStream.Position = 0;
        // overwrite filedata with converted HTML
        fileData = memStream.ToArray();
        // replace *.zip with *.html
        var fileName = String.Format("{0}.html", Path.GetFileNameWithoutExtension(name));
        // save the converted single file HTML to BLOB store
        return fileData;
      }
    }

    private static XDocument ProcessZipFile(byte[] fileData) {
      using (var ms = new MemoryStream(fileData)) {
        try {
          using (var gz = new ZipArchive(ms, ZipArchiveMode.Read)) {
            var entries = gz.Entries;
            var docFile = entries.First(z => Path.GetExtension(z.Name).StartsWith(".htm"));
            // copy resources so we can leave the zip safely
            var resourceFiles = entries.Except(new[] { docFile }).Select(r => {
              var name = r.FullName;
              var imageStream = new MemoryStream();
              r.Open().CopyTo(imageStream);
              var content = imageStream.ToArray();
              return new {
                Name = name,
                Content = content
              };
            }).ToList();
            Html2XmlUtil.imageAsBase64 = true;
            Html2XmlUtil.TreatExternalData += (sender, args) => {
              var images = resourceFiles.Where(f => f.Name.EndsWith(args.FileName)).ToList();
              if (!images.Any()) return;
              if (images.Count() > 1) {
                images = resourceFiles.Where(f => f.Name == Path.Combine(args.FilePath, args.FileName)).ToList();
              }
              var image = images.First();
              try {
                using (var imageStream = new MemoryStream(image.Content)) {
                  var img = System.Drawing.Image.FromStream(imageStream);
                  using (var pngImgMs = new MemoryStream()) {
                    img.Save(pngImgMs, ImageFormat.Png);
                    pngImgMs.Position = 0;
                    args.Data = pngImgMs.ToArray();
                  }
                }
              } catch (Exception) {
                args.Data = null;
              }
            };
            var result = new MemoryStream();
            docFile.Open().CopyTo(result);
            if (result.Length == 0) {
              throw new ArgumentOutOfRangeException();
            }
            string html;
            var bytes = result.ToArray();
            html = Encoding.UTF8.GetString(bytes); // unclear, even ISO docs do well in UTF8 IsUtf8(bytes) ? Encoding.UTF8.GetString(bytes) : Encoding.GetEncoding(1252).GetString(bytes);
            var xDoc = Html2XmlUtil.CleanUpHtmlWithResources(html); // make clean XHTML with embedded images
            return xDoc;
          }
        } catch (Exception ex) {
          throw;
        }
      }
    }

    public static Opus ImportSingleHtml(byte[] content, Import mapping, string name) {
      Opus opus = new Opus();
      var html = Encoding.UTF8.GetString(content);
      try {
        // convert prepared HTML into internal <Content> XML (backup and restore format)
        var parameters = new System.Collections.Specialized.NameValueCollection();
        if (mapping != null) {
          mapping.CharacterStyles.ForEach(c => parameters.Add(c.Key, c.Value));
          mapping.ParagraphStyles.ForEach(c => parameters.Add(c.Key, c.Value));
          mapping.NumberingStyles.ForEach(c => parameters.Add(c.Key, c.Value));
        }
        var xml = Html2XmlUtil.HtmlToOpusXsltParser(html, parameters);
        using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xml))) {
          var xDoc = XDocument.Load(xmlStream);          
          // use restore to create import, all content is in Opus, then
          RestoreOpusFromFile(opus, xDoc, null);
          // register the import information with the uploaded and converted HTML
        }
      }
      catch (Exception ex) {        
      }
      return opus;
    }

    private static void RestoreOpusFromFile(Opus opus, XDocument xDoc, string userName) {
      var saveElements = new List<Element>();
      Func<IEnumerable<XElement>, Element, List<Element>> helper = null;
      var currentChapter = opus.Name;
      var chapterOrder = 1;
      helper = (nodes, parent) => {
        var ret = new List<Element>();
        var orderNr = 1;
        foreach (var elm in nodes) {
          Element newElm = null;
          # region Detect Element Type
          switch (elm.Attribute("Type").Value.ToLower()) {
            case "opus":
              # region OPUS
              // do nothing as this import runs on opus level already, simply assign current as start
              opus.Name = elm.Attribute("Name").NullSafeString();
              ((Opus)opus).Version = ((Opus)opus).Version + 1;
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
              // only if import has an opus/parent part
              if (elm.Parent != null && elm.Parent.Name == "Content") {
                newElm.Parent = opus;
                currentChapter = elm.Attribute("Name").NullSafeString();
              }
              currentChapter = currentChapter ?? "Import Files";
              break;
              # endregion
            case "text":
              # region TEXT
              newElm = new TextSnippet {
                Content = Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Value.CleanUpString(15)
              };
              break;
              # endregion
            case "image":
              # region IMAGE
              var imgType = "png";
              var error = false;
              byte[] content = null;
              switch (elm.Attribute("Method").NullSafeString()) {
                case "Base64":
                  // assume the image is stored internally as Base64
                  try {
                    content = Convert.FromBase64String(elm.Value.Trim());
                  } catch (Exception ex) {
                    error = true;
                  }
                  break;
                default:
                  Console.ForegroundColor = ConsoleColor.Red;
                  Console.WriteLine("Error in Image: {0}", elm.ToString());
                  Console.ForegroundColor = ConsoleColor.Gray;
                  break;
              }
              if (error) break;
              // for this test console we don't save the images, add debug info here if needed
              break;
              # endregion
            case "listing":
              # region LISTING
              newElm = new ListingSnippet {
                Content = Encoding.UTF8.GetBytes(elm.Value.Trim()), //.Replace("\n", " "); - Causes problems with Listing widget. All data is displayed in one line
                Name = elm.Attribute("Name") == null ? "Listing" : elm.Attribute("Name").Value,
                Title = elm.Attribute("Name") == null ? "Listing" : elm.Attribute("Name").Value,
                Language = elm.Attribute("Language") == null ? "" : elm.Attribute("Language").Value,
                SyntaxHighlight = elm.Attribute("Highlight") == null || Boolean.Parse(elm.Attribute("Highlight").Value),
                LineNumbers = elm.Attribute("LineNumbers") == null || Boolean.Parse(elm.Attribute("LineNumbers").Value)
              };
              # endregion
              break;
            case "table":
              # region TABLE
              newElm = new TableSnippet {
                Content = System.Text.Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Attribute("Name") == null ? "Table" : elm.Attribute("Name").Value,
                Title = elm.Attribute("Name") == null ? "Table" : elm.Attribute("Name").Value,
                RepeatHeadRow = elm.Attribute("RepeatHeadRow") == null || Boolean.Parse(elm.Attribute("RepeatHeadRow").Value)
              };
              # endregion
              break;
            case "sidebar":
              # region SIDEBAR
              newElm = new SidebarSnippet {
                Content = Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Attribute("Name") == null ? "Sidebar" : elm.Attribute("Name").Value,
                SidebarType = elm.GetEnumAttribute<SidebarType>("SidebarType")
              };
              # endregion
              break;
            default:
              throw new NotSupportedException("Unknown snippet type found in source XML: " + elm.Attribute("Type").NullSafeString());
          }
          # endregion

          // take opus as existent, add else
          if (newElm == null) {
            opus.Children = helper(elm.Elements("Element"), opus);
          } else {
            if (elm.Elements("Element").Any()) {
              newElm.Children = helper(elm.Elements("Element"), newElm);
            }
            if (parent == null) {
              throw new InvalidOperationException("Parent must exist");
            }
            newElm.OrderNr = orderNr++;
            newElm.Parent = parent;
            ret.Add(newElm);
          }
        }
        return ret;
      };
      // invoke Content loader (assume each xml contains one Opus)
      try {
        if (xDoc.Root != null) {
          var restore = helper(from o in xDoc.Root.Elements("Element") select o, opus);
          opus.Children.AddRange(restore);
        }
        else {
          throw new ArgumentOutOfRangeException();
        }
      } catch (Exception ex) {
        throw;
      }
    }


  }
}