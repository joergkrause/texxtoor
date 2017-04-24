using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.HtmlAgility.ToXml.Nodes;

namespace Texxtoor.BaseLibrary.Core.HtmlAgility.ToXml {

  public class ExternalDataEventArgs : EventArgs {
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public byte[] Data { get; set; }
  }

  public class Html2XmlUtil {

    public static string basePath = "";
    public static bool imageAsBase64;
    public static List<string> callBackClasses = new List<string>();

    public static EventHandler TreatSpecialElement;
    public static EventHandler<ExternalDataEventArgs> TreatExternalData;
    private static Document docDes;
    private static void OnTreatSpecialElement(EventArgs e) {
      if (TreatSpecialElement != null) {
        TreatSpecialElement(docDes, e);
      }
    }

    private static void OnTreatExternalData(ExternalDataEventArgs e) {
      if (TreatExternalData != null) {
        TreatExternalData(docDes, e);
      }
    }

    public static string HtmlToOpusXsltParser(string html, NameValueCollection mapping) {
# if DEBUG
      System.IO.File.WriteAllText(@"c:\Temp\in.xml", html, Encoding.UTF8);
# endif
      var inStream = new MemoryStream(Encoding.UTF8.GetBytes(html));
      var doc = new XmlTextReader(inStream);
      var outStream = new MemoryStream();
      var writerSettings = new XmlWriterSettings {
        ConformanceLevel = ConformanceLevel.Fragment,
        Indent = true,
        IndentChars = " ",
        Encoding = Encoding.UTF8
      };
      var writer = XmlWriter.Create(outStream, writerSettings);
      var transform = new XslCompiledTransform();
      var settings = new XsltSettings {
        EnableScript = true
      };
      var sheetReader = XmlReader.Create(typeof(Html2XmlUtil).Assembly.GetManifestResourceStream("Texxtoor.BaseLibrary.Core.HtmlAgility.Xslt.HtmlToXml.xslt"));
      transform.Load(sheetReader, settings, null);
      var argsList = new XsltArgumentList();
      /*
          <xsl:param name="codeCharacter">ListingTextZchn</xsl:param>
          <xsl:param name="codePara">ListingText</xsl:param>
          <xsl:param name="listingCaption">Listingunterschrift</xsl:param>
          <xsl:param name="imageCaption">Bildunterschrift</xsl:param>
          <xsl:param name="tableCaption">Tabellenberschrift</xsl:param>
          <xsl:param name="sidebarHint">IconHinweisText</xsl:param>
          <xsl:param name="sidebarWarning">IconWarnungText</xsl:param>
          <xsl:param name="textPara">StandardAbsatz|AufzhlungEinrckung</xsl:param>
          <xsl:param name="bulletPara">Aufzhl1</xsl:param>
          <xsl:param name="numberPara">AufzhlNumber</xsl:param>
       * */
      foreach (var key in mapping.AllKeys) {
        if (key == null) continue;
        argsList.AddParam(key, "", mapping[key]);
      }
      transform.Transform(doc, argsList, writer);
      outStream.Position = 0;
      var xml = Encoding.UTF8.GetString(outStream.ToArray());
# if DEBUG
      System.IO.File.WriteAllText(@"c:\Temp\out-pre-fix.xml", xml, Encoding.UTF8);
# endif
      outStream.Position = 0;
      var xDoc = XDocument.Load(outStream);
      var currentType = "Text";
      XElement li = null;
      var allElements = xDoc.Root.Descendants().Where(e => e.Name == "Element").ToList();
      foreach (var le in allElements) {
        if (le.Attribute("Type").Value == "Listing" && currentType == "Text") {
          // enter listing sequence
          li = le;
        }
        if (li != null && le.Attribute("Type").Value == "Listing" && currentType == "Listing") {
          // listing sequence
          li.Value = li.Value + le.Value + Environment.NewLine;
          le.Remove();
        }
        // check current type
        currentType = le.Attribute("Type").Value;
      }
      foreach (var le in allElements) {
        if (le.Attribute("Type").Value == "Text" && currentType != "Text") {
          // enter text sequence
          li = le;
        }
        if (li != null && le.Attribute("Type").Value == "Text" && currentType == "Text") {
          // text sequence
          li.Add(le.Elements());
          le.Remove();
        }
        // check current type
        currentType = le.Attribute("Type").Value;
      }
# if DEBUG
      var outStream2 = new MemoryStream();
      var writer2 = XmlWriter.Create(outStream2);
      xDoc.WriteTo(writer2);
      writer2.Flush();
      xml = Encoding.UTF8.GetString(outStream2.ToArray());
      System.IO.File.WriteAllText(@"c:\Temp\out-after-fix.xml", xml, Encoding.UTF8);
# endif
      return xml;
    }

    public static XDocument HtmlToXDoc(string html, string name) {
      var docSrc = NSoupClient.Parse(html);
      var elements = docSrc.GetElementsByTag("body");
      var body = elements.First;
      var xDoc = new XDocument();
      xDoc.Add(new XElement("html",
        new XElement("head",
          new XElement("title", name)),
        XElement.Parse(body.OuterHtml())));
      return xDoc;
    }

    /// <summary>
    /// Make Html XHTML compliant and convert Images to inline base64 encoded strings to get single file result.
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static XDocument CleanUpHtmlWithResources(string html) {
      var docSrc = NSoupClient.Parse(html);
      var elements = docSrc.GetElementsByTag("body");
      var body = elements.First;
      var xDoc = new XDocument();
      xDoc.Add(new XElement("html", 
        new XElement("head", 
          new XElement("title", String.Format("Created from import at {0} by texxtoor", DateTime.Now.ToFileTime()))),
        XElement.Parse(body.OuterHtml())));
      // treat external data (we're looking for <img src> and replace this with inline base64 encoded stuff
      if (xDoc.Root != null) {
        xDoc.Root.Descendants("img")
          .ForEach(e => {
            var src = e.Attribute("src").Value;
            var ea = new ExternalDataEventArgs {
              FileName = Path.GetFileName(src),
              FilePath = Path.GetDirectoryName(src)
            };
            OnTreatExternalData(ea); // let the caller figure out how to retrieve the image
            if (ea.Data != null) {
              e.Attribute("src").SetValue(String.Format("data:image/png;base64,{0}", Convert.ToBase64String(ea.Data)));
            }
            else {
              e.Attribute("src").SetValue(String.Format("Error! File '{0}' missing from legacy path '{1}'", ea.FileName, ea.FilePath));
            }
          });
      }
      // after this we remove WordSection divs to further normalize the document
      var targetDoc = new XDocument();
      using (var writer = targetDoc.CreateWriter()) {
        var transform = new XslCompiledTransform();
        var settings = new XsltSettings {
          EnableScript = true
        };
        var sheetReader = XmlReader.Create(typeof(Html2XmlUtil).Assembly.GetManifestResourceStream("Texxtoor.BaseLibrary.Core.HtmlAgility.Xslt.NormalizeWordXml.xslt"));
        transform.Load(sheetReader, settings, null);
        transform.Transform(xDoc.CreateReader(), writer);
      }
      return targetDoc;
    }

    # region Traditional Parser, Code Pased

    public static string HtmlToOpusParser(string html) {
      var docSrc = NSoupClient.Parse(html);
      var elements = docSrc.GetElementsByTag("body");
      var body = elements.First;
      docDes = NSoupClient.Parse("");
      var content = docDes.CreateElement("Content");
      Process(docDes, body, content);
      return content.ToString();
    }

    /// <summary>
    /// Parse and Convert to internal format.
    /// </summary>
    /// <param name="docDes"></param>
    /// <param name="current"></param>
    /// <param name="parent"></param>
    private static void Process(Document docDes, Node current, Node parent) {
      var stack = new Stack<Node>();
      var nodes = current.ChildNodesAsArray();
      Node elem = null;
      for (var i = 0; i < nodes.Length; i++) {
        var n = nodes[i];
        var name = n.NodeName;
        var match = Regex.Match(name, @"[hH]\d");
        if (match.Success) {
          // Header
          elem = docDes.CreateElement("Element");
          elem.Attr("Type", "Section");
          elem.Attr("Level", name.Substring(1));
          while (stack.Count > 0) {
            Node top = stack.Peek();
            if (top != null) {
              string topname = top.Attr("Level");
              int toplevel = Int32.Parse(topname);
              int currentlevel = Int32.Parse(name.Substring(1));
              if (toplevel >= currentlevel) {
                stack.Pop();
              } else {
                top.AddChildren(elem);
                break;
              }
            }

          }
          Process(docDes, n, elem);
          if (stack.Count == 0) {
            parent.AddChildren(elem);
          }
          stack.Push(elem);
        } else {
          // if there is at least one element we can add content
          if (n is TextNode) {
            TextNode text = (TextNode)n;
            if (!text.IsBlank) {
              string t = text.Text();
              if (t != null && !t.Equals("")) {
                elem = new TextNode(t, "");
              }
            }
          } else {
            string nodename = n.NodeName;
            if (nodename.Equals("div", StringComparison.OrdinalIgnoreCase)) {
              // must start with a header, we throw away all text at the beginning
              if (parent.NodeName != "content") {
                elem = docDes.CreateElement("Element");
                elem.Attr("Type", "Section");
              }
              else {
                Process(docDes, n, parent);
              }
            } else if (nodename.Equals("p", StringComparison.OrdinalIgnoreCase)) {
              if (callBackClasses.Contains(n.Attr("class"))) {
                OnTreatSpecialElement(new EventArgs());
              }
              elem = docDes.CreateElement("p");
              if (parent.NodeName.Equals("td", StringComparison.OrdinalIgnoreCase)) {

              } else if (parent.NodeName.Equals("Element", StringComparison.OrdinalIgnoreCase) && parent.ChildNodes.Count > 0 && parent.Attr("Type").Equals("Text")) {

              } else if (parent.NodeName.Equals("Element", StringComparison.OrdinalIgnoreCase) && parent.ChildNodes.Count > 0 && parent.ChildNodes.Last().Attr("Type").Equals("Text")) {
                //elem = docDes.CreateElement("p");
                parent = parent.ChildNodes.Last();
              } else {
                Node elemP = docDes.CreateElement("Element");
                elemP.Attr("Type", "Text");
                AddToParent(new Stack<Node>(), elem, docDes, elemP, n);
                elem = elemP;
                n = null;
                //elem = docDes.CreateElement("p");
              }
              //parent = elem;
            } else if (nodename.Equals("table", StringComparison.OrdinalIgnoreCase)) {

              Node tableElement = docDes.CreateElement("Element");
              tableElement.Attr("Type", "Table");
              elem = docDes.CreateElement("table");
              //tableElement.AddChildren(elem);
              AddToParent(new Stack<Node>(), elem, docDes, tableElement, n);
              elem = tableElement;
              n = null;
              //elem = tableElement;

            } else if (nodename.Equals("b", StringComparison.OrdinalIgnoreCase)) {
              elem = docDes.CreateElement("b");

            } else if (nodename.Equals("a", StringComparison.OrdinalIgnoreCase)) {
              if (!n.Attr("href").Equals("") && n.Attr("href").IndexOf("_Toc") < 0) {
                elem = docDes.CreateElement("a");
                elem.Attr("Href", n.Attr("href"));

              } else {
                if (stack.Count > 0) {
                  elem = stack.Peek();
                } else {
                  elem = parent;
                }
              }

            } else if (nodename.Equals("span", StringComparison.OrdinalIgnoreCase)) {
              if (stack.Count > 0) {
                elem = stack.Peek();
              } else {
                elem = parent;
              }
            } else if (nodename.Equals("img", StringComparison.OrdinalIgnoreCase)) {
              elem = docDes.CreateElement("Element");
              elem.Attr("Type", "Image");
              TextNode src = null;
              if (imageAsBase64) {
                elem.Attr("Src", n.Attr("src"));
                elem.Attr("Method", "Base64");
                elem.Attr("Format", Path.GetExtension(n.Attr("src")).Substring(1));
                elem.Attr("Name", Path.GetFileNameWithoutExtension(n.Attr("src")));
                var extPath = Path.Combine(basePath, HttpUtility.UrlDecode(n.Attr("src")));
                var lessPath = Path.GetFileName(n.Attr("src"));
                var args = new ExternalDataEventArgs {
                  FilePath = extPath,
                  FileName = lessPath
                };
                OnTreatExternalData(args);
                src = new TextNode(args.Data == null ? "" : Convert.ToBase64String(args.Data), "");
              } else {
                elem.Attr("Method", "RefPath");
                src = new TextNode(HttpUtility.UrlDecode(n.Attr("src")), "");
              }
              elem.AddChildren(src);
              //elem.Attr("SRC", n.Attr("src"));
              if (n.Attr("caption") != null && !n.Attr("caption").Equals("")) {
                elem.Attr("Name", n.Attr("name"));
              } else if (n.Attr("name") != null && !n.Attr("name").Equals("")) {
                elem.Attr("Name", n.Attr("caption"));
              }
            } else {
              elem = docDes.CreateElement(nodename);
            }
            // 
          }
          AddToParent(stack, elem, docDes, parent, n);
        }
      }
    }

    private static void AddToParent(Stack<Node> stack, Node elem, Document docDes, Node parent, Node n) {
      Node top = null;
      if (stack.Count > 0) {
        top = stack.Peek();
      }
      //Node[] childs = n.ChildNodesAsArray();
      //foreach (Node child in n.ChildNodes)
      //{
      if (elem != null && n != null) {
        Process(docDes, n, elem);
      }
      if (elem != null && elem.Attr("Type").Equals("Table")) {
        Console.Write("");
      }
      if (elem != null && elem.Attr("Type").Equals("Text")) {
        Console.Write("");
      }
      //}
      if (elem == null || (elem.ChildNodes.Count <= 0 && !elem.Attr("Type").Equals("Image") && !elem.Attr("Type").Equals("Table") && !(elem is TextNode))) return;
      if (top != null) {
        if (Equals(elem, top)) return;
        if (elem is TextNode) {
          var text = (TextNode)elem;
          top.Attr("NAME", text.Text());
        } else if (elem.NodeName.Equals("p", StringComparison.OrdinalIgnoreCase) || elem.Attr("Type").Equals("Text")) {
          var list = new ArrayList();
          foreach (var childP in elem.ChildNodes) {
            if (childP.Attr("Type").Equals("Image") || childP.Attr("Type").Equals("Table")) {
              list.Add(childP);
            }
          }
          foreach (Node childP in list) {
            elem.RemoveChild(childP);
            top.AddChildren(childP);
          }
          if (top.ChildNodes.Count > 0) {
            var childs = top.ChildNodesAsArray();
            var last = childs[childs.Length - 1];
            if (elem.Attr("Type").Equals("Text") && last.Attr("Type").Equals("Text")) {
              if (!top.ChildNodes.Last().Equals(elem)) {
                top.ChildNodes.Last().AddChildren(elem.ChildNodesAsArray());
              }
              elem = null;
            }
          }
        }
        if (elem != null && (elem.ChildNodes.Count > 0 || elem.Attr("Type").Equals("Image") || elem is TextNode)) {
          top.AddChildren(elem);
        }
      } else {
        if (Equals(elem, parent)) return;
        if (elem is TextNode && parent.Attr("Type").Equals("Section")) {
          var text = (TextNode)elem;
          parent.Attr("Name", parent.Attr("Name") + text.Text());
        } else if (elem.NodeName.Equals("p", StringComparison.OrdinalIgnoreCase) || elem.Attr("Type").Equals("Text")) {
          var list = new ArrayList();
          foreach (var childP in elem.ChildNodes) {
            if (childP.Attr("Type").Equals("Image") || childP.Attr("Type").Equals("Table")) {
              list.Add(childP);
            }
          }
          foreach (Node childP in list) {
            elem.RemoveChild(childP);
            //parent.AddChildren(childP);
            AddToParent(childP, parent);
          }
        }
        if (elem.ChildNodes.Count > 0 || elem.Attr("Type").Equals("Image") || elem is TextNode) {
          //parent.AddChildren(elem);
          AddToParent(elem, parent);
        }
      }
    }

    private static void AddToParent(Node elem, Node parent) {
      if ((elem.Attr("Type").Equals("Image") || elem.Attr("Type").Equals("Table") || elem.Attr("Type").Equals("Text")) && (parent.Attr("Type").Equals("Image") || parent.Attr("Type").Equals("Table") || parent.Attr("Type").Equals("Text")) && parent.ParentNode != null) {
        AddToParent(elem, parent.ParentNode);
      } else {
        parent.AddChildren(elem);
      }
    }

    # endregion

  }
}
