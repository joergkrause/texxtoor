using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Texxtoor.Editor.Core;
using Texxtoor.Editor.Core.Extensions;
using Texxtoor.Editor.ViewModels;
using Texxtoor.Models;

namespace Texxtoor.Editor.Core {

  /// <summary>
  /// Supports the author's editor
  /// </summary>
  public class EditorManager : Manager<EditorManager> {

    private readonly Func<Element, Element> _seekParent;
    private readonly Func<Element, Element> _seekChapter;

    public EditorManager() {
      _seekParent = e => (e is Section ? e : _seekParent(e.Parent));
      _seekChapter = e => (e is Section && ((Section)e).Deep == 1 ? e : _seekChapter(e.Parent));
    }

    public Element GetSection(Element e) {
      return _seekParent(e);
    }

    public Element GetParentSection(Element e) {
      var p = _seekParent(e);
      return (p.Parent == null || p.Parent is Document ? p : p.Parent);
    }

    public Element GetParentChapter(Element e) {
      return _seekChapter(e);
    }

    /// <summary>
    /// After inserting an element we have to reorder all elements after the insertion point
    /// </summary>
    /// <param name="ns"></param>
    /// <param name="newOrderNr"></param>
    public void ReorderLeafElementsAfterElement(Element ns, int newOrderNr) {
      // reorder: sn has the ordernr of current element. We insert always after this element
      var downList = ns.Parent.Children;
      if (downList.Any()) {
        ns.OrderNr = newOrderNr;
        downList
          .Where(e => e.OrderNr >= newOrderNr)
          .ToList()
          .ForEach(e => e.OrderNr = e.OrderNr + 1);
      } else {
        ns.OrderNr = 1;
      }
    }

    public void ReorderLeafElementsAll(IList<Element> elements, bool immediateSave = true) {
      int order = 1;
      elements.ToList().ForEach(e => e.OrderNr = order++);
      if (immediateSave) {
        SaveChanges();
      }
    }

    public void RelocateChildElementsToParent(Element element) {
      var isLeafSection = !element.HasChildren() || !element.Children.OfType<Section>().Any();
      if (isLeafSection) {
        element.Children.ToList().ForEach(e => e.Parent = element.Parent);
      }
    }

    public void RelocateChildElementsAfterElement(Element parent, Element ns, int newOrderNr) {
      // assume a section is being inserted among a list of paragraphs
      // 1. Insert New Element at current position
      ns.OrderNr = newOrderNr;
      // 2. Get all paragraphs AFTER the current one and up to the next
      var newChildList = parent.Children
        .Where(e => e.OrderNr >= newOrderNr)
        .ToList();
      // 3. Move all to new parent and reorder
      int order = 1;
      newChildList.ForEach(e => {
        e.Parent = ns;
        e.OrderNr = order++;
      });
    }

    public void MoveElementUp(Element element) {
      var moveCollection = element.Parent.Children.OrderBy(s => s.OrderNr).ToList();
      int currentPartnerIdx = moveCollection.FindIndex(s => s.Id == element.Id);
      var exChangePartner = moveCollection.ElementAtOrDefault(currentPartnerIdx - 1);
      if (exChangePartner != null) {
        int oldOrder = element.OrderNr;
        element.OrderNr = exChangePartner.OrderNr;
        exChangePartner.OrderNr = oldOrder;
      }
      SaveChanges();
    }

    public void MoveElementDown(Element element) {
      var moveCollection = element.Parent.Children.OrderBy(s => s.OrderNr).ToList();
      int currentPartnerIdx = moveCollection.FindIndex(s => s.Id == element.Id);
      var exChangePartner = moveCollection.ElementAtOrDefault(currentPartnerIdx + 1);
      if (exChangePartner != null) {
        int oldOrder = element.OrderNr;
        element.OrderNr = exChangePartner.OrderNr;
        exChangePartner.OrderNr = oldOrder;
      }
      SaveChanges();
    }

    public void DecreaseSectionLevel(Element section) {
      // if there is a section before the parent of parent we can go a step deeper, all children move too (deep tree move)
      /* 
       * 1. The element in the list before the current becomes the new parent
       * 2. Get the new parent element
       * 3. Re-parent the element (step down in hierarchy)
       * 4. Re-order the new children collection 
       * 5. Re-order remaining children for old parent (as we lost an element in hierarchy)
       * */
      var oldParent = section.Parent;
      var sectionIndex = oldParent.Children.OrderBy(s => s.OrderNr).ToList().IndexOf(section);          // 1.
      var newParent = oldParent.Children.OrderBy(s => s.OrderNr).ToList().ElementAt(sectionIndex - 1);      // 2.
      section.Parent = newParent;                                                                       // 3.
      int order = 1;
      section.Parent.Children.OrderBy(s => s.OrderNr).ToList().ForEach(e => e.OrderNr = order++);       // 4.
      order = 1;
      oldParent.Children.OrderBy(s => s.OrderNr).ToList().ForEach(e => e.OrderNr = order++);            // 5.
      SaveChanges();
    }

    public void IncreaseSectionLevel(Element section) {
      // if there is a section before the parent of parent we can go a step higher, all children move too (deep tree move)
      /*
       * 1. Get new parent
       * 2. All Sections AFTER the current parent move one on the order
       * 3. The parent of parent becomes the new parent
       * 4. Re-order new collection
       * */
      var oldParent = section.Parent;
      var newParent = section.Parent.Parent;                                                                // 1.
      var startReorder = oldParent.OrderNr + 1;                                                         // 2.
      var upperBound = oldParent.Parent.Children.OrderBy(s => s.OrderNr).Where(e => e.OrderNr >= startReorder).ToList();
      int order = startReorder + 1;
      upperBound.ForEach(e => e.OrderNr = order++);
      section.Parent = newParent;                                                                       // 3.
      section.OrderNr = startReorder;                                                                   // 4.
      SaveChanges();
    }

    public Element CreateTextSnippet(Element after) {
      int newOrderNr = 1;
      var ns = new TextSnippet { Name = "Paragraph", Content = "<p>Enter text here...</p>".GetBytes() };
      ns.Parent = GetSection(after);
      if (after is TextSnippet) {
        newOrderNr = after.OrderNr + 1; // inserting always after so new element gets the position of current element
      } // else: keep 1 as default, as if not invoked from another textsnippet assume we're inserting after section
      this.ReorderLeafElementsAfterElement(ns, newOrderNr);
      return ns;
    }

    public Element CreateSidebarSnippet(Element after) {
      int newOrderNr = 1;
      var type = SidebarType.Note;
      var ns = new SidebarSnippet { Name = "Sidebar", Content = String.Format("<header contenteditable='{1}'>{0}</header><aside>Side Bar Content goes here...</aside>", type.ToString(), type == SidebarType.Box).GetBytes(), SidebarType = type };
      ns.Parent = GetSection(after);
      if (after is SidebarSnippet) {
        newOrderNr = after.OrderNr + 1; // inserting always after so new element gets the position of current element
      } // else: keep 1 as default, as if not invoked from another textsnippet assume we're inserting after section
      this.ReorderLeafElementsAfterElement(ns, newOrderNr);
      return ns;
    }

    public Element CreateImageSnippet(Element after, Resource res) {
      int newOrderNr = after.OrderNr + 1;
      ImageSnippet ns;
      // assume this is an image!
      using (var ms = new MemoryStream(res.Content)) {
        var img = System.Drawing.Image.FromStream(ms);
        using (var imgMs = new MemoryStream()) {
          img.Save(imgMs, ImageFormat.Png);
          ns = new ImageSnippet {
            Name = res.Name, // remember the Name (will become the title once the user overwrites this
            Content = imgMs.ToArray(), // copy of image
            Properties = System.Web.Helpers.Json.Encode(new ImageProperties {
              OriginalWidth = img.Width,
              OriginalHeight = img.Height,
              ImageWidth = img.Width,
              ImageHeight = img.Height
            }),
            MimeType = res.MimeType,
            OrderNr = newOrderNr
          };
        }
      }
      ns.Parent = GetSection(after);
      this.ReorderLeafElementsAfterElement(ns, newOrderNr);
      return ns;
    }

    public Element CreateListingSnippet(Element after) {
      int newOrderNr = 1;
      var ns = new ListingSnippet { Name = "Listing", Content = "public class Code {\n//Code goes here...\n}\n".GetBytes() };
      ns.Parent = GetSection(after);
      if (after is TextSnippet || after is ListingSnippet) {
        newOrderNr = after.OrderNr + 1; // inserting always after so new element gets the position of current element
      } // else: keep 1 as default, as if not invoked from another textsnippet assume we're inserting after section
      this.ReorderLeafElementsAfterElement(ns, newOrderNr);
      return ns;
    }

    public void DeleteChildrenResursively(List<Element> list) {
      foreach (var item in list) {
        if (item.HasChildren()) {
          DeleteChildrenResursively(item.Children);
        }
        Ctx.Elements.Remove(item);
      }
      SaveChanges();
    }

    public void InsertElement(Element ns, bool immediateSave = true) {
      Ctx.Elements.Add(ns);
      if (immediateSave) {
        SaveChanges();
      }
    }

    public Element GetElement(int id) {
      return Ctx.Elements.Find(id);
    }

    public T GetElement<T>(int id) where T : Element {
      return Ctx.Elements.OfType<T>().First(f => f.Id == id);
    }

    public void DeleteElement(Element sn) {
      Ctx.Elements.Remove(sn);
      SaveChanges();
    }

    public IList<Element> GetElementsForParent(int parentId) {
      return Ctx.Elements
          .Where(e => e.Parent.Id == parentId)
          .OrderBy(e => e.OrderNr)
          .ToList();
    }


    public void ReorderLeafElementsAfterOrderNr(int orderNr, int parentId) {
      var nextChapters = Ctx.Elements.Where(e => e.OrderNr >= orderNr && e.Parent.Id == parentId).ToList();
      int order = orderNr + 1;
      nextChapters.ForEach(c => c.OrderNr = order++);
      SaveChanges();
    }


    public void AddElement(Element element) {
      Ctx.Elements.Add(element);
      SaveChanges();
    }


    public bool SaveContent(int snippetId, System.Collections.Specialized.NameValueCollection frm, string content) {
      if (snippetId < 1498) return false;
      var res = GetElement(snippetId);
      bool refreshSection = false;
      var baseType = res.GetType().BaseType ?? res.GetType();
      switch (baseType.Name) {
        // images are saved through the Blob store 
        case "ImageSnippet":
          // images just save the figure's title here. All other operations are managed through the pane
          ((ImageSnippet)res).Title = content.StripTags();
          ((ImageSnippet)res).Properties = frm["properties-" + res.Id];
          break;
        case "ListingSnippet":
          ((ListingSnippet)res).Content = content.StripTags().GetBytes();
          ((ListingSnippet)res).Title = frm["caption-" + res.Id].StripTags();
          ((ListingSnippet)res).Language = frm["language-" + res.Id];
          bool b;
          ((ListingSnippet)res).LineNumbers = (Boolean.TryParse(frm["linenumbers-" + res.Id], out b) && b);
          ((ListingSnippet)res).SyntaxHighlight = (Boolean.TryParse(frm["syntaxhighlight-" + res.Id], out b) && b);
          break;
        case "TableSnippet":
          ((TableSnippet)res).Content = content.GetBytes();
          ((TableSnippet)res).Title = frm["caption-" + res.Id].StripTags();
          break;
        case "Section":
          ((Section)res).Content = content.StripTags().GetBytes();
          break;
        case "SidebarSnippet":
          ((SidebarSnippet)res).Content = content.GetBytes();
          ((SidebarSnippet)res).SidebarType = (SidebarType)Int32.Parse(frm["sidebartype-" + res.Id].StripTags());
          break;
        default:
          if (res is Section && (res.Parent == null || res.Parent.Parent == null)) {
            res.Name = content;
            refreshSection = true;
          }
          res.Content = content.GetBytes();
          break;
      }
      SaveChanges();
      return refreshSection;
    }

  }
}