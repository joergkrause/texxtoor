using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.BaseLibrary.Core.HtmlAgility.Pack;
using Texxtoor.DataModels.Models.Reader.Content;

namespace Texxtoor.BaseLibrary.Converters {
  /// <summary>
  /// Converts an HTML document with clean hierarchy of headers into a hierarchical list of fragments. Resources must be provided.
  /// </summary>
  internal class HtmlToFrozenFragments {

    private static readonly string[] textElements = new string[] { "div", "p", "li", "ul", "ol", "span", "font" };
    private static readonly string[] contElements = new string[] { "table", "img" };
    private static readonly string[] headElements = new string[] { "h1", "h2", "h3", "h4", "h5", "h6", "header" };

    public FrozenFragment Convert(string html, IList<ManifestItem> resources) {
      var allHtml = new HtmlDocument { OptionAutoCloseOnEnd = true, OptionFixNestedTags = true, OptionOutputAsXml = true };
      allHtml.LoadHtml(html);

      // fragments form a hierarchy, beginning with <h1> on the first level
      var rootFragment = new FrozenFragment {
        Name = "Root",
        ItemHref = "root",
        TypeOfFragment = FragmentType.Meta,
        Children = new List<FrozenFragment>(),
        Published = null
      };
      var c = String.Empty; // current content for text snippet
      var body = allHtml.DocumentNode.SelectSingleNode("//body");
      ElementParser(body.ChildNodes, rootFragment, resources);
      return rootFragment;
    }

    private void ElementParser(IEnumerable<HtmlNode> elements, FrozenFragment rootFragment, IList<ManifestItem> data) {
      // loop through all first level children
      FrozenFragment activeFragment = rootFragment;
      string c = "";
      foreach (var element in elements) {
        string e = element.Name.ToLower();
        // Text elements are kept in a single fragment if they contain more than exactly one container
        if (textElements.Contains(e)) {
          if (element.HasChildNodes) {
            // if the only element is a container, keep the container and throw the element away
            if (element.ChildNodes.Count() == 1) {
              var subel = element.ChildNodes.Single();
              var subname = element.ChildNodes.Single().Name.ToLower();
              if (contElements.Contains(subname)) {
                CreateTextFragment(activeFragment, ref c, data);
                switch (subname) {
                  case "table":
                    Debug.WriteLine("inner table " + element.OuterHtml);
                    CreateTableFragment(activeFragment, subel, data);
                    break;
                  case "img":
                    Debug.WriteLine("inner image " + element.OuterHtml);
                    CreateImageFragment(activeFragment, subel, data);
                    break;
                }
                continue;
              }
            }
          }
          // collect element until we reach another container or header
          c += element.OuterHtml;
        }
        //
        if (headElements.Contains(e)) {
          // detect current header element
          int l = 0;
          // detect level where we already are
          int h = activeFragment.Level;
          // get where we need to be
# pragma warning disable CS0462
          if (e.StartsWith("h") && e.Length == 2 && Int32.TryParse(e.Substring(1), out l)) ;
# pragma warning restore CS0462
          l++; // zero based
          Debug.WriteLine("hx h:{0} l:{1}", h, l);
          // each section writes the current text 
          CreateTextFragment(activeFragment, ref c, data);
          if (l == 1) {
            // first level
            activeFragment = CreateSectionFragment(activeFragment, element);
            h = 0;
          } else {
            if (l == h) {
              // same level = add to current parent
              activeFragment = CreateSectionFragment(activeFragment.Parent, element);
            }
            if (l > h) {
              // one level deeper = add to last current
              activeFragment = CreateSectionFragment(activeFragment, element);
            }
            if (l < h) {
              // one level higher = add again to former parent
              while (activeFragment.Parent != null && activeFragment.Level >= l) {
                activeFragment = activeFragment.Parent;
              }
              activeFragment = CreateSectionFragment(activeFragment, element);
            }
          }
          continue;
        }
        if (e == "img") {
          Debug.WriteLine("outer image " + element.OuterHtml);
          CreateTextFragment(activeFragment, ref c, data);
          CreateImageFragment(activeFragment, element, data);
          continue;
        }
        if (e == "table") {
          Debug.WriteLine("outer table " + element.OuterHtml);
          CreateTextFragment(activeFragment, ref c, data);
          CreateTableFragment(activeFragment, element, data);
          continue;
        }
      }

    }

    private FrozenFragment CreateTextFragment(FrozenFragment parent, ref string c, IList<ManifestItem> data) {
      // if no text just proceed
      if (c.Length == 0) return parent;
      // close open tags or open closed tags
      var nodeCheck = new HtmlDocument {
        OptionAutoCloseOnEnd = true,
        OptionCheckSyntax = true,
        OptionFixNestedTags = true
      };
      nodeCheck.LoadHtml(c);
      if (nodeCheck.ParseErrors.Any()) {
        // handle parser errors
      }
      var f = new FrozenFragment {
        Name = "Paragraph",
        ItemHref = "para",
        TypeOfFragment = FragmentType.Html,
        Parent = parent,
        Published = null
      };
      CreateEmbeddedImages(f, nodeCheck.DocumentNode, data);
      var sb = new StringBuilder();
      var tw = new StringWriter(sb);
      nodeCheck.Save(tw);
      f.Content = Encoding.UTF8.GetBytes(sb.ToString());
      parent.Children.Add(f);
      c = String.Empty;
      return f;
    }

    private FrozenFragment CreateImageFragment(FrozenFragment parent, HtmlNode element, IEnumerable<ManifestItem> data) {
      var alt = element.Attributes["alt"] != null ? element.Attributes["alt"].Value : "Generic Image";
      var cnt = data.FirstOrDefault(d => d.Href == element.Attributes["src"].Value);
      // Create the Resource Fragment first, it becomes a child of the Content Fragment
      var r = new FrozenFragment {
        Name = alt,
        Content = cnt.Data,
        TypeOfFragment = FragmentType.Html,
        ItemHref = Guid.NewGuid().ToString()
      };
      var f = new FrozenFragment {
        Name = alt,
        Content = Encoding.UTF8.GetBytes(String.Format(@"<img src=""{0}"" alt=""{1}"" />", r.ItemHref, alt)),
        TypeOfFragment = FragmentType.Html,
        Children = new List<FrozenFragment>(new FrozenFragment[] { r }),
        Parent = parent
      };
      r.Parent = f;
      parent.Children.Add(f);
      return f;
    }

    private FrozenFragment CreateTableFragment(FrozenFragment parent, HtmlNode element, IList<ManifestItem> data) {
      var caption = element.NextSibling.Name == "caption" ? element.NextSibling.InnerText : "Generic Table";
      var f = new FrozenFragment {
        Name = caption,
        TypeOfFragment = FragmentType.Html,
        Parent = parent
      };
      CreateEmbeddedImages(f, element, data);
      f.Content = Encoding.UTF8.GetBytes(element.OuterHtml);
      parent.Children.Add(f);
      return f;
    }

    private FrozenFragment CreateSectionFragment(FrozenFragment parent, HtmlNode element) {
      var f = new FrozenFragment {
        Name = element.InnerText,
        Content = Encoding.UTF8.GetBytes(element.OuterHtml),
        TypeOfFragment = FragmentType.Html,
        Children = new List<FrozenFragment>(),
        Parent = parent
      };
      parent.Children.Add(f);
      return f;
    }

    private void CreateEmbeddedImages(FrozenFragment parent, HtmlNode element, IList<ManifestItem> data) {
      if (element.SelectNodes(".//img") == null) return;
      foreach (var img in element.SelectNodes(".//img")) {
        var alt = img.Attributes["alt"] != null ? img.Attributes["alt"].Value : "Generic Image";
        var cnt = data.FirstOrDefault(d => d.Href == img.Attributes["src"].Value);
        var r = new FrozenFragment {
          Name = alt,
          Content = cnt.Data,
          TypeOfFragment = FragmentType.Image,
          ItemHref = Guid.NewGuid().ToString(),
          Published = null
        };
        if (parent.Children == null) {
          parent.Children = new List<FrozenFragment>();
        }
        parent.Children.Add(r);
        img.Attributes["src"].Value = r.ItemHref;
      }
    }

  }

}

