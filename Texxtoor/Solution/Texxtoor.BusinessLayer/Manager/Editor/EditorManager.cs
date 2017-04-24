using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Drawing.Charts;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Utilities.Imaging;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.DataModels.Attributes;
using Texxtoor.DataModels.Helper;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.ViewModels.Common;
using Texxtoor.ViewModels.Editor;

namespace Texxtoor.BusinessLayer {

  /// <summary>
  /// Supports the author's editor
  /// </summary>
  public class EditorManager : Manager<EditorManager> {

    public EditorManager() {
    }

    # region Simple Operations

    public void InsertElement(Element element, bool immediateSave = true) {
      Ctx.Elements.Add(element);
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

    public List<Element> DeleteElement(Element sn) {
      var level = sn.Level;
      var changedSectionsOnly = new List<Element>();
      // Checking if current element is a section and has children
      if (sn.HasChildren() && sn.Children.Count > 0) {
        var reparentableItems = sn.Children;
        // Try to get the previous section on the same level
        var thisChapter = SectionOperations.GetParentChapter(sn);
        var flatChapterElements = SectionOperations.GetFlattenElements(thisChapter);
        // Here we will check if a user is trying to delete a chapter
        if (sn.Id == thisChapter.Id) {
          // Check if user is not trying to delete the first chapter. This chapter must be intact
          if (thisChapter.OrderNr > 1) {
            var doc = ProjectManager.Instance.GetOpusInternal(thisChapter.Parent.Id);
            var chapters = doc.Children.OfType<Section>().OrderBy(c => c.OrderNr).Where(e => e.OrderNr < thisChapter.OrderNr).ToList();
            if (chapters.Any()) {
              var nextChapter = chapters.Last();
              var nextChapterSections = nextChapter.Children.OrderBy(c => c.OrderNr).Where(e => e.Level == 3).ToList();
              int orderNr = 0;
              if (nextChapterSections.Any())
                orderNr = nextChapterSections.Last().OrderNr;
              var nextParentForNonSectionSnippet = SectionOperations.GetLastSectionByLevel(nextChapter)[0];
              var orderNrForNonSectionSnippet = ((nextParentForNonSectionSnippet.Children.Count > 0) ? nextParentForNonSectionSnippet.Children.Last().OrderNr : 0);
              reparentableItems.ForEach(e => {
                if (e is Section) {
                  e.Parent = nextChapter;
                  e.OrderNr = ++orderNr;
                } else {
                  e.Parent = nextParentForNonSectionSnippet;
                  e.OrderNr = ++orderNrForNonSectionSnippet;
                }
              });
            }
          } else
            return null;
        } else {
          // Check if there is a previous section on the same level from which we are going to delete the current section
          var remainingElements = flatChapterElements.Where(e => (e.Level == sn.Level && e.OrderNr == sn.OrderNr - 1 && e is Section && e.Parent == sn.Parent)).ToList();
          // Also you can say top to bottom delete strategy
          if (remainingElements.Any()) {
            var nextParentForNonSectionSnippet = SectionOperations.GetLastSectionByLevel(remainingElements[0])[0];
            var nextParent = remainingElements[0];
            // Gettting the order of last child in the previous section
            // We have to manage the sections and all other snippet differently because section needs to placed according to the level and other snippets will be
            // just added to the end of the last section, so that user will not get frustrated with the new position of the snippet other than section..
            var orderNr = ((nextParent.Children.Count > 0) ? nextParent.Children.Last().OrderNr : 0);
            var orderNrForNonSectionSnippet = ((nextParentForNonSectionSnippet.Children.Count > 0) ? nextParentForNonSectionSnippet.Children.Last().OrderNr : 0);
            // Set the order number to the orphaned children
            reparentableItems.ForEach(e => {
              if (e is Section) {
                e.Parent = nextParent;
                e.OrderNr = ++orderNr;
              } else {
                e.Parent = nextParentForNonSectionSnippet;
                e.OrderNr = ++orderNrForNonSectionSnippet;
              }
            });
          }
            // If there no previous sections available on the same level to which we can rebind the deleted section children, we will have to look for its parent 
            //to rebind the deleted section children
            // Also you can say bottom to top deleting strategy
          else {
            var deletingElementParent = sn.Parent;
            int orderNr = 0;
            if (deletingElementParent.HasChildren()) {
              // Get some section on the same level with common parent
              var parentChildren = deletingElementParent.Children.Where(e => e.Level == sn.Level && e is Section && e.Id != sn.Id).ToList();
              if (!parentChildren.Any())
                // if no such section level is found then associate the deleting section children to parent directly
                parentChildren.Add(deletingElementParent);
              // Just add the parent of the deleted section as the next parent
              var nextParent = deletingElementParent;
              // check if the there is no next parent then deletingelementparent will be our choice
              if (parentChildren.Any(e => e.OrderNr <= sn.OrderNr))
                // Just handle the condition here to change the level of the child section
                nextParent = parentChildren.Last(e => e.OrderNr <= sn.OrderNr);
              // Check if the next parent has some children so that we can assign some order to the newly adding children, other wise consider that the
              // deleted section children are the only children so sarting the order no from 1
              if (nextParent.HasChildren())
                if (nextParent.Children.Count() != 1 || nextParent.Children[0].Id != sn.Id)
                  orderNr = nextParent.Children.OrderBy(c => c.OrderNr).Where(e => e.Id != sn.Id && e.OrderNr <= sn.OrderNr).Last().OrderNr;
                else
                  orderNr = 0;
              else
                orderNr = 1;

              var reparentableSectionsOnly = reparentableItems.Where(e => e is Section).ToList();
              if (reparentableSectionsOnly.Any()) {
                // Calculating the level differences
                var levelDifference = reparentableSectionsOnly.First().Level - nextParent.Level;
                // Releveling the sections
                changedSectionsOnly = SectionOperations.ReLevelSections(reparentableSectionsOnly, levelDifference);
              }

              reparentableItems.ForEach(e => {
                e.Parent = nextParent;
                e.OrderNr = ++orderNr;
              });
              // Now check if levels need to be updated as well
              // First of all getting all sections
              //var reparentableSectionsOnly = reparentableItems.Where(e => e is Section).ToList();
              //if (reparentableSectionsOnly.Count() > 0)
              //{
              //    // Calculating the level differences
              //    var levelDifference = reparentableSectionsOnly.First().Level - nextParent.Level;
              //    // Releveling the sections
              //    SectionOperations.ReLevelSections(reparentableSectionsOnly,levelDifference);
              //}
            }
          }
        }
        // We are not willing to remove the children with the sections
        sn.Children = null;
      }
      //var changedElements = GetSectionsAfterLevel(sn, level);
      var changedElements = changedSectionsOnly;
      Ctx.Elements.Remove(sn);
      SaveChanges();
      return changedElements;
    }

    public void DeleteElementWithChildren(List<Element> list) {
      DeleteElementWithChildrenInner(list, null);
      SaveChanges();
    }

    public void DeleteElementWithChildren(List<Element> list, List<int> childrenRemoved) {
      DeleteElementWithChildrenInner(list, childrenRemoved);
      SaveChanges();
    }

    private void DeleteElementWithChildrenInner(IEnumerable<Element> list, ICollection<int> childrenRemoved) {
      foreach (var item in list.ToList()) {
        if (childrenRemoved != null) {
          childrenRemoved.Add(item.Id);
        }
        if (item.HasChildren()) {
          DeleteElementWithChildrenInner(item.Children, childrenRemoved);
        }
        Ctx.Elements.Remove(item);
      }
    }

    # endregion

    # region Create Operations

    /// <summary>
    /// Inserts a chapter. Current elements is from chapter before the new one.
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="currentElement"></param>
    /// <returns></returns>
    public Snippet CreateChapterSnippet(int documentId, Element currentElement) {
      var doc = ProjectManager.Instance.GetOpusInternal(documentId);
      var chapters = doc.Children.OfType<Section>().OrderBy(c => c.OrderNr).ToList();
      var prevChapter = currentElement == null ? doc.Children.OfType<Section>().FirstOrDefault() : SectionOperations.GetParentChapter(currentElement);
      if (prevChapter == null) {
        return null;
      }
      var parent = prevChapter.Parent;
      if (!(parent is Opus)) {
        return null;
      }
      var ns = new Section { Name = "Chapter", Content = doc.ChapterDefault.GetBytes(), Parent = parent };
      ns.Name = "Chapter";
      //ns.OrderNr = prevChapter.OrderNr + 1;
      int newElementOrderNr;
      ns.OrderNr = newElementOrderNr = prevChapter.OrderNr + 1;
      //ns.OrderNr = prevChapter.Children.Last().OrderNr + 1;
      //int newElementOrderNr = prevChapter.Children.Last().OrderNr + 1;
      // Get all elements with respect to first chapter, but then show other chapters on proper places in next lines of code
      //ReOrderOperations.ReorderLeafElementsAfterElement(ns, ns.OrderNr);
      // ReOrdering all the elements by their order no
      //foreach (var chapter in chapters) {
      chapters
          .Where(e => e.OrderNr >= newElementOrderNr)
         .ForEach(e => e.OrderNr = ++newElementOrderNr);
      //.Where(e => e.Level >= level)
      //.ForEach(e => e.OrderNr = ++newElementOrderNr);
      //}
      /*flatChapterElements
          .Where(e => (e.OrderNr >= (newElementOrderNr) && (e.Id != ns.Id)))
         .ForEach(e => e.OrderNr = ++newElementOrderNr);*/
      InsertElement(ns);
      return ns;
    }

    public Section CreateSectionSnippet(int opusId, Element currentElement, int level) {
      var doc = ProjectManager.Instance.GetOpusInternal(opusId);
      var chapters = doc.Children.OfType<Section>().OrderBy(c => c.OrderNr).ToList();
      var newOrderNr = 1;
      var thisChapter = SectionOperations.GetParentChapter(currentElement);
      var flatChapterElements = SectionOperations.GetFlattenElements(thisChapter);
      // 2. take all after current with requested level
      var remainingElements = SectionOperations.GetRemainingElementsOfLevel(flatChapterElements, currentElement.Id, level);      // 3. everything that is parented outside the list will become reparented to new element, but not other sections
      var parent = SectionOperations.GetParentSectionForLevel(currentElement, level);
      // 4. add new element after the current position
      var newElementOrderNr = 0;
      //var currentElementParent = currentElement;
      //var parent = currentElement;
      if (currentElement.Level != 2) {
        var currentElementParent = SectionOperations.GetParentSectionForLevel(currentElement, currentElement.Level);
        parent = SectionOperations.GetParentSectionForLevel(currentElement, level);
        // 4. add new element after the current position

        if (currentElement.Level == level)
          newElementOrderNr = currentElement.OrderNr + 1;
        else {
          var elementLevel = currentElementParent.Level;
          var currentOderNr = currentElementParent.OrderNr;
          var currentParent = currentElementParent;
          try {
            while (elementLevel != level) {
              currentParent = currentParent.Parent;
              elementLevel = currentParent.Level;
              currentOderNr = currentParent.OrderNr;
            }
            newElementOrderNr = currentOderNr + 1;
          } catch {
            newElementOrderNr = 1;
          }
        }
      } else
        newElementOrderNr = 1;
      //var newElementOrderNr = SectionOperations.GetCurrentOrderNr(flatChapterElements, remainingElements, level);
      var ns = new Section {
        Name = String.Format("Section Level {0}", level),
        Content = doc.SectionDefault.GetBytes(),
        Parent = parent,
        OrderNr = newElementOrderNr
      };

      // 6. reparent to current element
      //var reparentableElements = ReOrderOperations.GetReparentableElements(remainingElements, parent);
      /*List<Element> reparentableElements = new List<Element>();
      foreach (Element element in remainingElements)
      {
          if (element.Level == level && element is Section)
              break;
          else
              reparentableElements.Add(element);
      }*/
      remainingElements.ForEach(e => {
        e.Parent = ns;
        e.OrderNr = newOrderNr++;
      });

      // reorder remaining on same level
      //if (currentElement.Level != level || (currentElement is Section))
      //flatChapterElements.Where(e => ((e.Level == level) && e.OrderNr >= newElementOrderNr)).ForEach(e => e.OrderNr = ++newElementOrderNr);
      foreach (Element element in flatChapterElements) {
        if (element.Level == level && element.OrderNr >= newElementOrderNr) {
          element.OrderNr = ++newElementOrderNr;
        }
      }
      // Normalizing sections
      //newOrderNr = 1;
      //flatChapterElements.Where(e => e.Level == level).OrderBy(e => e.OrderNr).ForEach(e => e.OrderNr = newOrderNr++);
      //remainingElements.Where(e => e.Level >= level).ForEach(e => e.OrderNr = ++newElementOrderNr);
      /*foreach (var chapter in chapters)
      {
          chapter.Children
              .Where(e => (e.OrderNr >= (newElementOrderNr) && (e.Id != ns.Id)))
             .ForEach(e => e.OrderNr = ++newElementOrderNr);
          //.Where(e => e.Level >= level)
          //.ForEach(e => e.OrderNr = ++newElementOrderNr);
      }*/
      InsertElement(ns);
      return ns;
    }

    public Element CreateTableSnippet(int opusId, Element currentElement, string rows, string cols, string variation) {
      var doc = ProjectManager.Instance.GetOpusInternal(opusId);
      //var newOrderNr = (currentElement is Section) ? 1 : currentElement.OrderNr + 1;
      var newOrderNr = currentElement.OrderNr + 1;
      var ns = new TableSnippet {
        Name = "Table " + variation,
        Rows = UInt32.Parse(rows),
        Cols = UInt32.Parse(cols),
        TableType = variation,
        OrderNr = newOrderNr
      };
      ((TableSnippet)ns).GenerateTable();
      ns.Parent = SectionOperations.GetSection(currentElement);
      ReOrderOperations.ReorderLeafElementsAfterElement(ns, newOrderNr);
      InsertElement(ns);
      return ns;
    }

    public Snippet CreateTextSnippet(int opusId, Element currentElement) {
      var doc = ProjectManager.Instance.GetOpusInternal(opusId);
      //var chapters = doc.Children.OfType<Section>().OrderBy(c => c.OrderNr).ToList();
      var newOrderNr = (currentElement is Section) ? 1 : currentElement.OrderNr + 1;
      //var newOrderNr = currentElement.OrderNr + 1;
      var ns = new TextSnippet {
        Name = "Paragraph",
        Content = doc.TextSnippetDefault.GetBytes(),
        OrderNr = newOrderNr,
        Parent = SectionOperations.GetSection(currentElement)
      };
      ReOrderOperations.ReorderLeafElementsAfterElement(ns, newOrderNr, currentElement);
      /*foreach (var chapter in chapters)
      {
          chapter.Children
              .Where(e => (e.OrderNr >= (newOrderNr) && (e.Id != ns.Id)))
             .ForEach(e => e.OrderNr = ++newOrderNr);
          //.Where(e => e.Level >= level)
          //.ForEach(e => e.OrderNr = ++newElementOrderNr);
      }*/
      /*flatChapterElements
          .Where(e => e.OrderNr >= (newOrderNr))
         .ForEach(e => e.OrderNr = ++newOrderNr);*/
      InsertElement(ns);
      return ns;
    }

    public Snippet CreateSidebarSnippet(int opusId, Element currentElement) {
      var doc = ProjectManager.Instance.GetOpusInternal(opusId);
      //var newOrderNr = (currentElement is Section) ? 1 : currentElement.OrderNr + 1;
      var newOrderNr = currentElement.OrderNr + 1;
      const SidebarType type = SidebarType.Note;
      var ns = new SidebarSnippet {
        Name = "Sidebar",
        Content =
          String.Format("<header contenteditable='{1}'>{0}</header><aside>{2}</aside>",
                        type, type == SidebarType.Box, doc.TextSnippetDefault).GetBytes(),
        SidebarType = type,
        OrderNr = newOrderNr,
        Parent = SectionOperations.GetSection(currentElement)
      };
      ReOrderOperations.ReorderLeafElementsAfterElement(ns, newOrderNr);
      InsertElement(ns);
      return ns;
    }

    public Snippet CreateImageSnippet(int opusId, Element currentElement, ResourceFile res) {
      var newOrderNr = currentElement.OrderNr + 1;
      ImageSnippet ns;
      // assume this is an image!
      var guid = res.ResourceId;
      using (var blob = BlobFactory.GetBlobStorage(guid, BlobFactory.Container.Resources)) {
        using (var ms = new MemoryStream(blob.Content)) {
          switch (res.MimeType) {
            case "image/png":
            case "image/jpg":
            case "image/jpeg":
            case "image/gif":
              var ext = res.MimeType.Substring(res.MimeType.IndexOf("/", StringComparison.Ordinal) + 1);
              var img = System.Drawing.Image.FromStream(ms);
              using (var imgMs = new MemoryStream()) {
                // always use PNG
                img.Save(imgMs, ImageFormat.Png);
                ns = new ImageSnippet {
                  Name = res.Name, // remember the Name (will become the title once the user overwrites this
                  Title = res.Name.EndsWith("." + ext) ? res.Name.Substring(0, res.Name.Length - 4) : res.Name,
                  Content = imgMs.ToArray(), // hard copy of image
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
              break;
            case "image/svg+xml":
              ns = new ImageSnippet {
                Name = res.Name, // remember the Name (will become the title once the user overwrites this
                Title = res.Name.EndsWith(".svg") ? res.Name.Substring(0, res.Name.Length - 4) : res.Name,
                Content = blob.Content, // hard copy of image
                MimeType = res.MimeType,
                OrderNr = newOrderNr,
                Properties = System.Web.Helpers.Json.Encode(new ImageProperties {
                  // defaults from SVG editor TODO: we need to take this from the SVG to have right size handy?
                  OriginalWidth = 800,
                  OriginalHeight = 600,
                  ImageWidth = 800,
                  ImageHeight = 600
                }),
              };
              break;
            default:
              throw new NotSupportedException(res.MimeType + " is currently not supported");
          }
        }
      }
      ns.Parent = SectionOperations.GetSection(currentElement);
      InsertElement(ns);
      ReOrderOperations.ReorderLeafElementsAfterElement(ns, newOrderNr);
      SaveChanges();
      return ns;
    }

    public Snippet CreateListingSnippet(int opusId, Element currentElement) {
      var doc = ProjectManager.Instance.GetOpusInternal(opusId);
      //var newOrderNr = (currentElement is Section) ? 1 : currentElement.OrderNr + 1;
      var newOrderNr = currentElement.OrderNr + 1;
      var ns = new ListingSnippet {
        Name = "Listing",
        Content = doc.ListingSnippetDefault.GetBytes(),
        Parent = SectionOperations.GetSection(currentElement)
      };
      if (currentElement is TextSnippet || currentElement is ListingSnippet) {
        newOrderNr = currentElement.OrderNr + 1; // inserting always after so new element gets the position of current element
      } // else: keep 1 as default, as if not invoked from another textsnippet assume we're inserting after section
      ReOrderOperations.ReorderLeafElementsAfterElement(ns, newOrderNr);
      InsertElement(ns);
      return ns;
    }

    public List<Element> GetAllChildren(Element section) {
      return SectionOperations.GetAllSectionChildren(section);
    }

    public List<JsonBehavior> GetAllChildrenSnippets(Opus document, Snippet chapter, Element section) {
      return SectionOperations.GetAllSectionChildrenSnippets(document, chapter, section);
    }

    public List<JsonBehavior> ConvertElementToJsonSnippet(Opus document, Snippet chapter, List<Element> elementList) {
      var jsonList = new List<JsonBehavior>();
      foreach (Snippet el in elementList) {
        jsonList.Add((el).GetType().GetCustomAttributes(typeof(EditorServiceWrapperAttribute), true).OfType<EditorServiceWrapperAttribute>().Single().GetJson(document, chapter, el));
      }
      return jsonList;
    }

    # endregion

    internal static class SectionOperations {

      private static readonly Func<Element, Element> _seekParent;
      private static readonly Func<Element, Element> _seekChapter;
      private static readonly Func<Element, int, Element> _seekParentForLevel;

      static SectionOperations() {
        _seekParent = e => (e is Section ? e : _seekParent(e.Parent));
        _seekChapter = e => (e is Section && e.Level == 2 ? e : _seekChapter(e.Parent));
        _seekParentForLevel = (e, l) => (e is Section && e.Level == l - 1 ? e : _seekParentForLevel(e.Parent, l));
      }

      /// <summary>
      /// Take any element and seek until a section. Throws an exception if no section in the parent chain.
      /// </summary>
      /// <param name="e">Element</param>
      /// <returns>Section</returns>
      internal static Element GetSection(Element e) {
        return _seekParent(e);
      }

      /// <summary>
      /// Save way to get a section an element is in.
      /// </summary>
      /// <param name="e"></param>
      /// <returns></returns>
      internal static Element GetParentSection(Element e) {
        var p = _seekParent(e);
        return (p.Parent == null || p.Parent is Opus ? p : p.Parent);
      }

      /// <summary>
      /// Seek the elements parent up to the given level. Level is 1 for document, 2 for chapter, 3 for section 1.1 and so on.
      /// </summary>
      /// <param name="e"></param>
      /// <param name="level"></param>
      /// <returns></returns>
      internal static Element GetParentSectionForLevel(Element e, int level) {
        return e.Parent is Opus ? e : _seekParentForLevel(e, level);
      }

      /// <summary>
      /// Get the chapter for element
      /// </summary>
      /// <param name="e"></param>
      /// <returns></returns>
      internal static Element GetParentChapter(Element e) {
        return _seekChapter(e);
      }

      /// <summary>
      /// Takes an element and makes a flat list of this and all children in it, recursively. The list is ordered by OrderNr field.
      /// </summary>
      /// <param name="root">Root element that provides a Children collection.</param>
      /// <returns>Flat list</returns>
      internal static List<Element> GetFlattenElements(Element root) {
        var elements = new List<Element> { root };
        FlatElements(elements, root.Children.OrderBy(e => e.OrderNr));
        return elements;
      }

      private static void FlatElements(ICollection<Element> elements, IEnumerable<Element> children) {
        foreach (var item in children) {
          elements.Add(item);
          if (item.HasChildren()) {
            FlatElements(elements, item.Children.OrderBy(e => e.OrderNr));
          }
        }
      }

      /// <summary>
      /// Seeks the flatten list for an element's Id, skips this, and returns all elements of specified level after it.
      /// </summary>
      /// <param name="flatChapterElements"></param>
      /// <param name="currentId"></param>
      /// <param name="level"></param>
      /// <returns></returns>
      internal static List<Element> GetRemainingElementsOfLevel(ICollection<Element> flatChapterElements, int currentId, int level) {
        var currentElement = flatChapterElements.Where(e => e.Id == currentId).ToList()[0];
        var remainingElements = new List<Element>();
        remainingElements = flatChapterElements
              .SkipWhile(e => e.Id != currentId)             // everything before
              .Skip(1).ToList()        // and including (skip(1)) the current element
             .ToList(); // out of the formula and everything on same level (deeper remain unchanged)

        // When we are going to insert a section after section on the same level
        //if (currentElement.Level == level && currentElement is Section)
        //{
        //    remainingElements = flatChapterElements
        //      .SkipWhile(e => e.Id != currentId)             // everything before
        //      .Skip(1).ToList()        // and including (skip(1)) the current element
        //     .ToList(); // out of the formula and everything on same level (deeper remain unchanged)
        //}
        //else
        //    // When we are going to insert a sub section right after the parent section
        //    if (currentElement.Level < level && currentElement is Section)
        //    {
        //        remainingElements = flatChapterElements
        //      .SkipWhile(e => e.Id != currentId)             // everything before
        //      .Skip(1).ToList()        // and including (skip(1)) the current element
        //     .Where(e => e.Level >= level && e.Parent == currentElement).ToList(); // out of the formula and everything on same level (deeper remain unchanged)
        //    }
        //    else
        //    {
        //        // IF the current element was not a section, level could be any but should not be a section
        //        remainingElements = flatChapterElements
        //          .SkipWhile(e => e.Id != currentId)             // everything before
        //          .Skip(1).ToList()        // and including (skip(1)) the current element
        //         .Where(e => e.Level >= level && e.Parent == currentElement.Parent).ToList(); // out of the formula and everything on same level (deeper remain unchanged)
        //    }
        var remainingElementList = new List<Element>();
        // Populating the remainingElemnetList with all children and stop when the next element is a same level section
        foreach (Element element in remainingElements) {
          if (element.Level <= level && element is Section)
            break;
          // we have to pick all those elements which do not has any parent and need to be reparented
          if (!remainingElements.Contains(element.Parent))
            remainingElementList.Add(element);
        }
        return remainingElementList;
      }

      /// <summary>
      /// Create a flat list from a list of sections and limit the output to sections as well.
      /// </summary>
      /// <param name="sections"></param>
      /// <returns></returns>
      internal static List<Element> GetFlattenSections(List<Element> sections) {
        var elements = new List<Element>();
        FlatElements(elements, sections);
        return elements.OfType<Section>().ToList<Element>();
      }

      /// <summary>
      /// Seeks the list without remaining for specified level to get the last (highest) OrderNr.
      /// </summary>
      /// <param name="flatChapterElements">flat list</param>
      /// <param name="remainingElements">all after current</param>
      /// <param name="level">level we're looking for</param>
      /// <returns>The orderNr from that all later elements will be reordered</returns>
      internal static int GetCurrentOrderNr(List<Element> flatChapterElements, List<Element> remainingElements, int level) {
        var newElementOrderElements = flatChapterElements
          .Except(remainingElements)                // take everything before new element
            .Where(e => e.Level == level)             // only on same level
          .ToList();
        // take order no and calc next
        return newElementOrderElements.Any() ? newElementOrderElements.Max(e => e.OrderNr) + 1 : 1;
      }

      /// <summary>
      /// This method gets the last child of a parent with lowest order
      /// </summary>
      /// <param name="parent"> Parent which needs to be traversed for the children</param>
      /// <returns>Last child of a parent</returns>
      internal static List<Element> GetLastSectionByLevel(Element parent) {
        var elementList = new List<Element>();
        var allChildSections = parent.Children.OrderBy(e => e.OrderNr).Where(e => e is Section).ToList();
        if (allChildSections.Any()) {
          var currentElement = allChildSections.Last();
          while (currentElement.HasChildren()) {
            allChildSections = currentElement.Children.OrderBy(e => e.OrderNr).Where(e => e is Section).ToList();
            if (allChildSections.Any())
              currentElement = allChildSections.Last();
            else
              break;
          }
          elementList.Add(currentElement);
        } else {
          elementList.Add(parent);
        }
        return elementList;
      }

      /// <summary>
      /// This method will relevel all the sections according to difference from parent level
      /// </summary>
      /// <param name="reparentableSectionsOnly">list of sections to be relevel</param>
      /// <param name="levelDifference">The difference between parent section level and first child level</param>
      /// <returns>returns the element list back</returns>
      internal static List<Element> ReLevelSections(List<Element> reparentableSectionsOnly, int levelDifference) {
        var changedElements = new List<Element>();
        // Checking if the level difference is more than 1, because only then we have to relevel rest of the sections
        if (levelDifference > 1) {
          // Starting from level 4, because if it is section level 3 then we do not have to relevel
          reparentableSectionsOnly.ForEach(e => {
            e.Name = "Section Level " + (e.Level - (levelDifference - 1));
            changedElements.Add(e);
            if (e.HasChildren()) {
              // section level 5
              e.Children.Where(c => c is Section).ForEach(c => {
                c.Name = "Section Level " + (c.Level - (levelDifference - 1));
                changedElements.Add(c);
                if (c.HasChildren()) {
                  c.Children.Where(d => d is Section).ForEach(d => {
                    //section level 6
                    d.Name = "Section Level " + (d.Level - (levelDifference - 1));
                    changedElements.Add(d);
                  });
                }
              });
            }
          });
        }
        return changedElements;
      }

      /// <summary>
      /// This method will get all the children of the section on all the levels
      /// </summary>
      /// <param name="section">section to get the children</param>
      /// <returns>returns the element list back</returns>
      internal static List<Element> GetAllSectionChildren(Element section) {
        var changedElements = new List<Element>();
        changedElements.Add(section);
        section.Children.OrderBy(e => e.OrderNr).ForEach(e => {
          //section level 4
          changedElements.Add(e);
          if (e.HasChildren()) {
            e.Children.OrderBy(f => f.OrderNr).ForEach(c => {
              //section level 5
              changedElements.Add(c);
              if (c.HasChildren()) {
                c.Children.OrderBy(g => g.OrderNr).ForEach(d => {
                  //section level 6
                  changedElements.Add(d);
                });
              }
            });
          }
        });
        return changedElements;
      }

      /// <summary>
      /// This method will get all the children of the section on all the levels
      /// </summary>
      /// <param name="section">section to get the children</param>
      /// <returns>returns the snippet list back</returns>
      internal static List<JsonBehavior> GetAllSectionChildrenSnippets(Opus document, Snippet chapter, Element section) {
        var changedElements = new List<JsonBehavior>();
        Action<Snippet> getJson = null;
        getJson =
          e => {
            changedElements.Add(
            e.GetType()
            .GetCustomAttributes(typeof(EditorServiceWrapperAttribute), true)
            .OfType<EditorServiceWrapperAttribute>()
            .First()
            .GetJson(document, chapter, e));
            if (e.HasChildren()) {
              e.Children.OrderBy(g => g.OrderNr).ToList().ForEach(g => getJson(g as Snippet));
            }
          };
        getJson(section as Snippet);
        //changedElements.Add((section as Snippet).GetType().GetCustomAttributes(typeof(EditorServiceWrapper), true).OfType<EditorServiceWrapper>().First().GetJson(documentId, chapterId, section as Snippet));
        //section.Children.OrderBy(e => e.OrderNr).ForEach(e => {
        //  //section level 4
        //  changedElements.Add((e as Snippet).GetType().GetCustomAttributes(typeof(EditorServiceWrapper), true).OfType<EditorServiceWrapper>().First().GetJson(documentId, chapterId, e as Snippet));
        //  if (e.HasChildren()) {
        //    e.Children.OrderBy(f => f.OrderNr).ForEach(c => {
        //      //section level 5
        //      changedElements.Add((c as Snippet).GetType().GetCustomAttributes(typeof(EditorServiceWrapper), true).OfType<EditorServiceWrapper>().First().GetJson(documentId, chapterId, c as Snippet));
        //      if (c.HasChildren()) {
        //        c.Children.OrderBy(g => g.OrderNr).ForEach(d => {
        //          //section level 6
        //          changedElements.Add((d as Snippet).GetType().GetCustomAttributes(typeof(EditorServiceWrapper), true).OfType<EditorServiceWrapper>().First().GetJson(documentId, chapterId, d as Snippet));
        //        });
        //      }
        //    });
        //  }
        //});
        return changedElements;
      }

      //internal static List<Element> ResetSectionLevel(List<Element> sectionElementList,int level)
      //{
      //    List<Element> elementList = new List<Element>();
      //    sectionElementList.Where(e=> e is Section && e);
      //    foreach(Element element in sectionElementList)
      //    {

      //    }
      //    var sections=sectionElementList;
      //    var currentElement=sections.First();
      //    while (currentElement.HasChildren())
      //    {
      //    }
      //    return elementList;
      //}
    }

    internal static class ReOrderOperations {

      internal static List<Element> GetReparentableElements(List<Element> remainingElements, Element parent) {
        return remainingElements.Where(e => !(e is Section) /*&& e.Parent.Id == parent.Id*/).ToList();
      }

      /// <summary>
      /// After inserting an element we have to reorder all elements after the insertion point. We normalize to always begin with 1.
      /// </summary>
      /// <param name="ns"></param>
      /// <param name="newOrderNr"></param>
      internal static List<Element> ReorderLeafElementsAfterElement(Element ns, int newOrderNr) {
        var orderList = ns.Parent.Children.OrderBy(e => e.OrderNr).SkipWhile(e => e.Id == ns.Id && e.OrderNr < newOrderNr).ToList();
        if (orderList.Any()) {
          ns.OrderNr = newOrderNr;
          orderList.ForEach(e => e.OrderNr = ++newOrderNr);
        } else {
          ns.OrderNr = 1;
        }
        // normalize
        newOrderNr = 1;
        orderList.OrderBy(e => e.OrderNr).ForEach(e => e.OrderNr = newOrderNr++);
        return orderList;
      }

      /// <summary>
      /// After inserting an element we have to reorder all elements after the insertion point. We normalize to always begin with 1.
      /// </summary>
      /// <param name="ns"></param>
      /// <param name="newOrderNr"></param>
      /// <param name="currentElement"></param>
      internal static List<Element> ReorderLeafElementsAfterElement(Element ns, int newOrderNr, Element currentElement) {
        var orderList = ns.Parent.Children.OrderBy(e => e.OrderNr).SkipWhile(e => e.Id == ns.Id).ToList();

        // Normalize all items on the same level
        newOrderNr = 1;
        orderList.OrderBy(e => e.OrderNr).ForEach(e => e.OrderNr = newOrderNr++);

        if (orderList.Any()) {
          // Check if the top element is not a parent element, If the top element will be parent then the id wil not match and throw exception
          if (orderList.FirstOrDefault(e => e.Id == currentElement.Id) != null)
            ns.OrderNr = orderList.First(e => e.Id == currentElement.Id).OrderNr + 1;
          else
            ns.OrderNr = 1;

          newOrderNr = ns.OrderNr;
          orderList.Where(e => e.OrderNr >= newOrderNr).
              ForEach(e => e.OrderNr = ++newOrderNr);
        } else {
          ns.OrderNr = 1;
        }
        return orderList;
      }

      /// <summary>
      /// Used to remove holes in the order hierarchy, does not physically change the order.
      /// </summary>
      /// <remarks>Makes a sequence of 1 4 7 8 9 into 1 2 3 4 5</remarks>
      /// <param name="elements"></param>
      internal static void ReorderLeafElementsAll(IList<Element> elements) {
        var orderNr = 1;
        elements.OrderBy(e => e.OrderNr).ToList().ForEach(e => e.OrderNr = orderNr++);
      }

      internal static void RelocateChildElementsAfterElement(Element parent, Element ns, int newOrderNr) {
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

    }

    # region Section Operations

    public List<Element> MoveElementNext(Element movedElement, bool down) {
      var dropExchElement = GetSibling(movedElement, down);
      return MoveElement(movedElement, dropExchElement);
    }

    /// <summary>
    /// Takes an element and let the drop it wherever he likes, handle the moves smartly.
    /// </summary>
    /// <param name="movedElement"></param>
    /// <param name="dropExchElement"></param>
    public List<Element> MoveElement(Element movedElement, Element dropExchElement) {
      // 1. Check whether we can drop it, if not return false immediately
      var changedElements = new List<Element>();
      var movedElementParent = movedElement.Parent;
      if (movedElement is Section) {
        // only section need special treatment
        throw new NotImplementedException();
      } else {
        // we just use the Parent and OrderNr fields here
        if (dropExchElement is Section) {
          var newOderNr = 1;
          movedElement.Parent = dropExchElement;    // if the target is a section we become a child
          movedElement.OrderNr = newOderNr;           // we assume the element becomes the first if moved from top down
          //SaveChanges();
          changedElements.Add(movedElement);
          if (dropExchElement.HasChildren()) {
            foreach (Element e in dropExchElement.Children) {
              if (e.Id != movedElement.Id)
                e.OrderNr = e.OrderNr + 1;
            }
          }

          if (movedElementParent.HasChildren() && movedElementParent != dropExchElement) {
            newOderNr = 0;
            movedElementParent.Children.OrderBy(e => e.OrderNr).Where(e => e.Id != movedElement.Id).ForEach(e => {
              e.OrderNr = ++newOderNr;
              changedElements.Add(e);
            });
          }
          //changedElements = ReOrderOperations.ReorderLeafElementsAfterElement(movedElement, movedElement.OrderNr + 1);
        } else {
          //Not with in the same section
          if (movedElement.Parent != dropExchElement.Parent) {
            movedElement.Parent = dropExchElement.Parent;
          }
          int orderNr;
          if (dropExchElement.OrderNr - 1 == movedElement.OrderNr || dropExchElement.OrderNr + 1 == movedElement.OrderNr) {
            // just neighbors, either way
            orderNr = movedElement.OrderNr;
            movedElement.OrderNr = dropExchElement.OrderNr;
            dropExchElement.OrderNr = orderNr;
          } else {
            // some where down the slope
            orderNr = dropExchElement.OrderNr + 1; // become next
            movedElement.OrderNr = orderNr;
            movedElement.Parent.Children.Where(e => e.OrderNr >= orderNr && e != movedElement).ForEach(e => {
              e.OrderNr = ++orderNr;
            });
          }
          changedElements = new List<Element> { dropExchElement, movedElement };
          if (movedElementParent.HasChildren() && movedElementParent != dropExchElement.Parent) {
            var newOderNr = 0;
            movedElementParent.Children.OrderBy(e => e.OrderNr).Where(e => e.Id != movedElement.Id).ForEach(e => {
              e.OrderNr = ++newOderNr;
              changedElements.Add(e);
            });
          }
        }


        SaveChanges();
        // regular snippets can be dropped everywhere, so we return true always
        return changedElements;
      }
    }

    /// <summary>
    /// Takes an element and let the drop it wherever he likes, handle the moves smartly.
    /// </summary>
    /// <param name="movedElement"></param>
    /// <param name="dropExchElement"></param>
    public List<Element> MoveSections(Element movedElement, Element dropExchElement) {
      List<Element> changedElements = null;

      if (dropExchElement.Level < movedElement.Level) {
        changedElements = new List<Element>();
        List<Element> elementList = new List<Element>();
        elementList.Add(movedElement);
        // first we will have to see if the element after which the moved element is being dropped is a section
        if (dropExchElement is Section) {
          changedElements = SectionOperations.ReLevelSections(elementList, (movedElement.Level - dropExchElement.Level));
          movedElement.OrderNr = 1;

          movedElement.Parent = dropExchElement;

          var movedElementChildren = movedElement.Children.OrderBy(e => e.OrderNr).SkipWhile(e => e is Section).ToList();
          var lastSection = SectionOperations.GetLastSectionByLevel(movedElement);

          var reparentableSnippetsCount = 0;
          foreach (var element in dropExchElement.Children) {
            if (element.Id != movedElement.Id) {
              if (element is Section)
                element.OrderNr = (element.OrderNr + 1) - reparentableSnippetsCount;
              else {
                element.Parent = lastSection[0];
                if (lastSection[0].HasChildren()) {
                  element.OrderNr = lastSection[0].Children.OrderBy(e => e.OrderNr).Last().OrderNr + 1;
                } else
                  element.OrderNr = 1;
                reparentableSnippetsCount++;
              }
              //changedElements.Add(element);
            }
          }
          changedElements.Clear();
          changedElements = SectionOperations.GetAllSectionChildren(movedElement);
        }
          // if the element after which the moved element is being dropped is not a section
        else {
          movedElement.OrderNr = dropExchElement.OrderNr + 1;
          changedElements = SectionOperations.ReLevelSections(elementList, (movedElement.Level - dropExchElement.Parent.Level));

          movedElement.Parent = dropExchElement.Parent;

          var movedElementChildren = movedElement.Children.OrderBy(e => e.OrderNr).SkipWhile(e => e is Section).ToList();

          var lastSection = SectionOperations.GetLastSectionByLevel(movedElement);
          var reparentableSnippetsCount = 0;
          foreach (var element in dropExchElement.Parent.Children) {
            if (element.Id != movedElement.Id && element.Id > dropExchElement.Id) {
              if (element is Section)
                element.OrderNr = (element.OrderNr + 1) - reparentableSnippetsCount;
              else {
                element.Parent = lastSection[0];
                if (lastSection[0].HasChildren()) {
                  element.OrderNr = lastSection[0].Children.OrderBy(e => e.OrderNr).Last().OrderNr + 1;
                } else
                  element.OrderNr = 1;
                reparentableSnippetsCount++;
              }
              //changedElements.Add(element);
            }
          }
          changedElements.Clear();
          changedElements = SectionOperations.GetAllSectionChildren(movedElement);
        }
      } else {

      }

      //// 1. Check whether we can drop it, if not return false immediately

      //var movedElementParent = movedElement.Parent;
      //if (movedElement is Section)
      //{
      //    // only section need special treatment
      //    throw new NotImplementedException();
      //}
      //else
      //{
      //    // we just use the Parent and OrderNr fields here
      //    if (dropExchElement is Section)
      //    {
      //        var newOderNr = 1;
      //        movedElement.Parent = dropExchElement;    // if the target is a section we become a child
      //        movedElement.OrderNr = newOderNr;           // we assume the element becomes the first if moved from top down
      //        //SaveChanges();
      //        changedElements.Add(movedElement);
      //        if (dropExchElement.HasChildren())
      //        {
      //            foreach (Element e in dropExchElement.Children)
      //            {
      //                if (e.Id != movedElement.Id)
      //                    e.OrderNr = e.OrderNr + 1;
      //            }
      //        }

      //        if (movedElementParent.HasChildren() && movedElementParent != dropExchElement)
      //        {
      //            newOderNr = 0;
      //            movedElementParent.Children.OrderBy(e => e.OrderNr).Where(e => e.Id != movedElement.Id).ForEach(e =>
      //            {
      //                e.OrderNr = ++newOderNr;
      //                changedElements.Add(e);
      //            });
      //        }
      //        //changedElements = ReOrderOperations.ReorderLeafElementsAfterElement(movedElement, movedElement.OrderNr + 1);
      //    }
      //    else
      //    {
      //        //Not with in the same section
      //        if (movedElement.Parent != dropExchElement.Parent)
      //        {
      //            movedElement.Parent = dropExchElement.Parent;
      //        }
      //        var orderNr = dropExchElement.OrderNr + 1;
      //        movedElement.OrderNr = orderNr;
      //        movedElement.Parent.Children.Where(e => e.OrderNr >= orderNr && e != movedElement).ForEach(e =>
      //        {
      //            e.OrderNr = ++orderNr;
      //        });

      //        changedElements = new List<Element> { dropExchElement, movedElement };
      //        if (movedElementParent.HasChildren() && movedElementParent != dropExchElement.Parent)
      //        {
      //            var newOderNr = 0;
      //            movedElementParent.Children.OrderBy(e => e.OrderNr).Where(e => e.Id != movedElement.Id).ForEach(e =>
      //            {
      //                e.OrderNr = ++newOderNr;
      //                changedElements.Add(e);
      //            });
      //        }
      //    }


      SaveChanges();
      // regular snippets can be dropped everywhere, so we return true always

      //}
      return changedElements;
    }

    /// <summary>
    /// If there is a section before the parent of parent we can go a step deeper, all children move too (deep tree move).
    /// </summary>
    /// <param name="section"></param>
    /// <param name="withChildren"></param>
    public List<Element> DecreaseSectionLevel(Element section, bool withChildren) {
      var targetLevel = section.Level;                                    // manage ops on this (old) level
      var newOrderNr = 0;
      var chapter = SectionOperations.GetParentChapter(section);
      var allElements = SectionOperations.GetFlattenElements(chapter);
      var predecessor = allElements                                       // find all elements of the parent before current
        .OfType<Section>()
        .OrderBy(e => e.OrderNr)
        .LastOrDefault(e => e.Level == targetLevel && e.OrderNr < section.OrderNr && e.Parent == section.Parent);
      if (predecessor == null) return null;
      var prevElementOnNewLevel = predecessor.Children.OrderBy(e => e.OrderNr);
      if (prevElementOnNewLevel.Any())
        newOrderNr = prevElementOnNewLevel.Last().OrderNr + 1;
      else
        newOrderNr = 1;
      var changedElements = new List<Element>();
      //newOrderNr = predecessor == null ? 1 : predecessor.OrderNr + 1;
      section.Parent = predecessor; // make it a child element (e.g. decrease)
      section.OrderNr = newOrderNr; // insert at the position where it is
      section.Name = "Section Level " + section.Level;
      changedElements.Add(section);
      // Now also change the parent of its subordinate sections
      section.Children.Where(e => e is Section).ForEach(e => {
        e.OrderNr = ++newOrderNr;
        e.Parent = predecessor;
        //changedElements.Add(e);
      });

      //// all old siblings needs reorder
      //var successors = allElements                                        // all elements after current (after increase)
      //  .OfType<Section>()
      //  .OrderBy(e => e.OrderNr)
      //  .Where(e => e.Level == targetLevel && e.OrderNr >= newOrderNr)
      //  .ToList<Element>();
      //successors.ForEach(e => e.OrderNr = ++newOrderNr);
      SaveChanges();
      //successors.Insert(0, section);
      return changedElements;
    }

    /// <summary>
    /// If there is a section before the parent of parent we can go a step higher, all children move too (deep tree move).
    /// </summary>
    /// <param name="section"></param>
    /// <returns>List of elements which gets new order nr.</returns>
    public List<Element> IncreaseSectionLevel(Element section, bool withChildren) {
      var currentParent = section.Parent;                               // get current parent
      var newOrderNr = currentParent.OrderNr;                           // after increase this will be the new order nr
      var targetLevel = currentParent.Level;                            // limit ops to this (new) level
      var orderNrForReparentableElements = currentParent.OrderNr;
      var chapter = SectionOperations.GetParentChapter(section);
      var allElements = SectionOperations.GetFlattenElements(chapter);

      List<Element> changedElements = new List<Element>();
      //newOrderNr = predecessor == null ? 1 : predecessor.OrderNr + 1;
      changedElements.Add(section);
      var reparentableElements = currentParent.Children.OfType<Section>().OrderBy(e => e.OrderNr).Where(e => e.OrderNr > section.OrderNr);
      changedElements = SectionOperations.ReLevelSections(changedElements, 2);

      if (section.HasChildren())
        orderNrForReparentableElements = section.Children.OrderBy(e => e.OrderNr).Last().OrderNr;
      else
        orderNrForReparentableElements = 0;

      if (reparentableElements != null && reparentableElements.Any()) {
        reparentableElements.ForEach(e => {
          e.Parent = section;
          e.OrderNr = ++newOrderNr;
          changedElements.Add(e);
        });
      }
      section.Parent = currentParent.Parent;                                       // make it a child element (e.g. decrease)
      section.OrderNr = ++newOrderNr;                                      // insert at the position where it is
      section.Name = "Section Level " + section.Level;


      SaveChanges();
      return changedElements;
      //var successors = allElements                                      // all elements after current (after increase)
      //  .OfType<Section>()
      //  .OrderBy(e => e.OrderNr)
      //  .Where(e => e.Level == targetLevel && e.OrderNr > newOrderNr)
      //  .ToList<Element>();
      //var immediateChildren = SectionOperations.GetRemainingElementsOfLevel(allElements, section.Id, section.Level);
      //immediateChildren.ForEach(e => e.Parent = section);               // reparent siblings
      //section.Parent = currentParent.Parent;                            // reparent it self
      //section.OrderNr = ++newOrderNr;                                   // set order
      //// all new siblings need reorder
      //successors.ForEach(e => e.OrderNr = ++newOrderNr);                // re-order all others

      //successors.Insert(0, section);
      //return SectionOperations.GetFlattenSections(successors);
    }

    # endregion

    public bool SaveContent(int snippetId, System.Collections.Specialized.NameValueCollection frm, string content) {
      var res = GetElement(snippetId);
      if (res == null) return false; // freshly deleted objects may fire save a very last time 
      var refreshSection = false;
      var baseType = res.GetType().BaseType ?? res.GetType();
      switch (baseType.Name) {
        // images are saved through the Blob store 
        case "ImageSnippet":
          // images just save the figure's title here. All other operations are managed through the pane
          ((ImageSnippet)res).Title = frm["caption"].StripTags();
          ((ImageSnippet)res).Name = frm["caption"].StripTags();
          ((ImageSnippet)res).Properties = frm["properties"];
          break;
        case "ListingSnippet":
          ((ListingSnippet)res).Content = content.GetBytes();
          ((ListingSnippet)res).Title = frm["caption"].StripTags();
          ((ListingSnippet)res).Name = frm["caption"].StripTags();
          ((ListingSnippet)res).Language = frm["language"];
          bool b;
          ((ListingSnippet)res).LineNumbers = (Boolean.TryParse(frm["linenumbers"], out b) && b);
          ((ListingSnippet)res).SyntaxHighlight = (Boolean.TryParse(frm["syntaxhighlight"], out b) && b);
          break;
        case "TableSnippet":
          ((TableSnippet)res).Content = content.GetBytes();
          ((TableSnippet)res).Title = frm["caption"].StripTags();
          ((TableSnippet)res).Name = frm["caption"].StripTags();
          break;
        case "Section":
          ((Section)res).Content = content.StripTags().GetBytes();
          ((Section)res).Name = content.StripTags();
          break;
        case "SidebarSnippet":
          ((SidebarSnippet)res).Content = content.GetBytes();
          ((SidebarSnippet)res).SidebarType = (SidebarType)Int32.Parse(frm["sidebartype"].StripTags());
          ((SidebarSnippet) res).Name = ((SidebarSnippet) res).HeaderContent;
          break;
        default:
          if (res is Section && (res.Parent == null || res.Parent.Parent == null)) {
            res.Name = content;
            refreshSection = true;
          }
          res.Content = content.GetBytes();
          res.Name = content.StripTags().Ellipsis(30).ToString();
          break;
      }
      CreateHistory(snippetId);
      SaveChanges();
      return refreshSection;
    }

    public void CreateHistory(int snippetId) {
      try {
        var opus = ProjectManager.Instance.GetOpusFromSnippetId(snippetId);
        var history = ProjectManager.Instance.GetResourceFiles(opus, TypeOfResource.Project, "application/opus-xml")
          .OrderBy(r => r.CreatedAt).ToList();
        // save every then minutes if something has been changed
        if (history.LastOrDefault() == null || DateTime.Now.Subtract(history.Last().CreatedAt).Minutes > 10) {
          var ms = ProjectManager.Instance.CreateBackup(opus);
          var resId = Guid.NewGuid();
          var file = new ResourceFile {
            ResourceId = resId,
            Name = opus.Name + "-" + DateTime.Now.ToShortTimeString(),
            MimeType = "application/opus-xml",
            TypesOfResource = TypeOfResource.Project,
            Project = opus.Project
          };
          using (var blob = BlobFactory.GetBlobStorage(resId, BlobFactory.Container.Resources)) {
            blob.Content = ms.ToArray();
            blob.Save();
          }
          Ctx.Resources.Add(file);
          SaveChanges();
        }
        // remove elder steps (keep last 10 for now)
        if (history.Count() > 10) {
          foreach (var resourceFile in history.Take(history.Count() - 10)) {
            ResourceManager.Instance.Delete(resourceFile.ResourceId);
          }
        }
      } catch (Exception) {
        // TODO: notify caller, we ignore errors here to assure text get saved even if undo goes wrong
      }
    }

    public Element GetChapterForElement(Element element) {
      return SectionOperations.GetParentChapter(element);
    }

    /// <summary>
    /// Get a sibling snippet (ignore sections) before or after the given snippet.
    /// </summary>
    /// <param name="sn"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public Element GetSibling(Element sn, bool next) {
      if ((sn is Section)) {
        throw new ArgumentException("Element must be of type snippet", "sn");
      }
      // get the whole chapter
      // take the snippets only (we don't care about sections here)
      var flatList = SectionOperations.GetFlattenElements(SectionOperations.GetParentChapter(sn)).OfType<Snippet>().ToList();
      var idx = flatList.IndexOf(sn as Snippet);
      if (idx == -1) {
        throw new ArgumentException("snippet not found", "sn");
      }
      var lst = flatList.IndexOf(flatList.Last());
      if (idx == lst && next) {
        // if already the last there is no next
        return null;
      }
      if (!next && (idx == 0)) {
        // already first or second and previous (2 steps) is required
        return null;
      }
      return flatList[idx + (next ? 1 : -1)];
    }


    public List<Element> GetSectionsAfterLevel(Element section, int targetLevel) {
      var chapter = SectionOperations.GetParentChapter(section);
      var newOrderNr = section.OrderNr;
      var allElements = SectionOperations.GetFlattenElements(chapter);
      var successors = allElements                                      // all elements after current (after increase)
        .OfType<Section>()
        .OrderBy(e => e.OrderNr)
        .Where(e => e.Level == targetLevel && e.OrderNr > newOrderNr)
        .ToList<Element>();
      return SectionOperations.GetFlattenSections(successors);
    }

    public List<Element> GetAllSectionsAfterLevel(Element section, int targetLevel) {
      var chapter = SectionOperations.GetParentChapter(section);
      var newOrderNr = section.OrderNr;
      var allElements = SectionOperations.GetFlattenElements(chapter);
      var successors = allElements                                      // all elements after current (after increase)
        .OfType<Section>()
        .OrderBy(e => e.OrderNr)
        .Where(e => e.Level == targetLevel)
        .ToList<Element>();
      return SectionOperations.GetFlattenSections(successors);
    }

    public List<Element> MergeNextElement(Element sn) {
      var next = sn.Parent.Children
                   .OrderBy(e => e.OrderNr)
                   .SkipWhile(e => e.Id != sn.Id)
                   .Skip(1)
                   .OfType<TextSnippet>()
                   .FirstOrDefault();
      if (next != null) {
        sn.Content = sn.Content.Concat(next.Content).ToArray();
        Ctx.Elements.Remove(next);
        SaveChanges();
      }
      return new List<Element> { sn };
    }

    public Element InsertOrphanedElement(Element insert, int afterSnippet) {
      var after = GetElement(afterSnippet);
      Element snippet = null;
      var parent = after.WidgetName == "Section" ? after : after.Parent;
      var orderNr = after.WidgetName == "Section" ? 1 : after.OrderNr;
      switch (insert.WidgetName) {
        case "Text":
          snippet = new TextSnippet();
          break;
        case "Table":
          snippet = new TableSnippet();
          break;
        case "Sidebar":
          snippet = new SidebarSnippet();
          break;
        case "Listing":
          snippet = new ListingSnippet();
          ((ListingSnippet)snippet).Language = ((ListingSnippet)insert).Language;
          ((ListingSnippet)snippet).SyntaxHighlight = ((ListingSnippet)insert).SyntaxHighlight;
          ((ListingSnippet)snippet).LineNumbers = ((ListingSnippet)insert).LineNumbers;
          break;
        case "Image":
          snippet = new ImageSnippet();
          ((ImageSnippet)snippet).Properties = ((ImageSnippet)insert).Properties;
          break;
        case "Section":
          snippet = new Section();
          break;
      }
      if (snippet != null) {
        snippet.Parent = parent;
        snippet.OrderNr = orderNr;
        snippet.Content = insert.Content;
        snippet.LocaleId = insert.LocaleId;
        // re-order remaining elements
      }
      snippet = Ctx.Elements.Add(snippet);
      ReOrderOperations.ReorderLeafElementsAfterElement(after, orderNr + 1);
      SaveChanges();
      return snippet;
    }

    public string GetWordCount(int id) {
      var doc = ProjectManager.Instance.GetOpusInternal(id);
      var all = SectionOperations.GetFlattenElements(doc);
      var sum = all.Sum(e => ((e.WidgetName == "Text" || e.WidgetName == "Section") && e.Content.Length > 1) ? System.Text.Encoding.UTF8.GetString(e.Content).Split(' ').Length : 0);
      return sum.ToString(CultureInfo.InvariantCulture);
    }

    public string GetCharacterCount(int id) {
      var doc = ProjectManager.Instance.GetOpusInternal(id);
      var all = SectionOperations.GetFlattenElements(doc);
      var sum = all.Sum(e => e.WidgetName == "Text" || e.WidgetName == "Section" ? e.Content.Length : 0);
      return sum.ToString(CultureInfo.InvariantCulture);
    }

    public void SetDocumentProperties(int opusId, bool allowChapters, bool allowMetaData, string chapterDefault, string sectionDefault, string textSnippetDefault, string listingSnippetDefault, bool showFlowPane, bool showNaviPane, bool showNumberChain, string userName) {
      var doc = ProjectManager.Instance.GetOpusInternal(opusId);
      if (doc == null) return;
      var prj = ProjectManager.Instance.GetProject(doc.Project.Id, userName);
      doc.Project = prj;
      doc.AllowChapters = allowChapters;
      doc.AllowMetaData = allowMetaData;
      doc.ChapterDefault = chapterDefault;
      doc.SectionDefault = sectionDefault;
      doc.TextSnippetDefault = textSnippetDefault;
      doc.ListingSnippetDefault = listingSnippetDefault;
      doc.ShowFlowPane = showFlowPane;
      doc.ShowNaviPane = showNaviPane;
      doc.ShowNumberChain = showNumberChain;
      SaveChanges();
    }

    public List<TemplateGroup> GetTemplates(string name) {
      return Ctx.TemplateGroups.Where(t => t.Name == name).ToList();
    }

    public IEnumerable<WorkitemChat> GetComments(int sid, string part) {
      return Ctx.WorkitemChats.Where(w => w.Snippet.Id == sid && w.Name == part).OrderByDescending(e => e.OrderNr);
    }

    public void SaveComment(int? parentId, WorkitemChat chatItem) {
      // optionally add to a parent to create hierarchical thread
      var parent = Ctx.WorkitemChats.Find(parentId.GetValueOrDefault());
      if (parent == null) {
        // first element in the hierarchy or flat list chat
        Ctx.WorkitemChats.Add(chatItem);
      } else {
        if (parent.Children == null) {
          parent.Children = new List<WorkitemChat>();
        }
        parent.Children.Add(chatItem);
      }
      SaveChanges();
    }

    // tweak elements inserted from orphaned list
    public void SetElementContent(int newId, byte[] p) {
      var snippet = GetElement(newId);
      if (snippet == null) return;
      snippet.Content = p;
      SaveChanges();
    }

    # region Editor Service Support

    public int GetSnippetId(IList<SnippetDataModel> snippets, int snippetId, int direction, string value, int sd = 0) {
      var id = -1;
      IEnumerable<SnippetDataModel> result = null;
      switch (direction) {
        case -1:
          result = snippets.Where(x => x.CurrentSnippet.Id < snippetId).ToList();
          result = result.Where(d => d.CurrentSnippet.RawContent.StripTags().ToLower().Contains(value.ToLower())).ToList();
          id = sd == 0 ? result.Select(s => s.CurrentSnippet.Id).LastOrDefault() : result.Select(s => s.CurrentSnippet.Id).FirstOrDefault();
          break;
        case 1:
          result = snippets.Where(x => x.CurrentSnippet.Id > snippetId).ToList();
          result = result.Where(d => d.CurrentSnippet.RawContent.StripTags().ToLower().Contains(value.ToLower())).ToList();
          id = sd == 0 ? result.Select(s => s.CurrentSnippet.Id).FirstOrDefault() : result.Select(s => s.CurrentSnippet.Id).LastOrDefault();
          break;
      }
      if (result == null) return -1;
      if (id == 0 && sd != -direction)
        id = GetSnippetId(snippets, snippetId, -direction, value, direction);
      return id;
    }

    public string RemoteOnCreateImage(object sender, CreateImageArguments e) {
      // TODO: assume content is already in the element (becomes FrozenFagment later on)
      var path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/data/"), e.FileName);
      // file is being written from DB to disc for later retrieve from PDF generator
      using (var file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite)) {
        file.Write(e.Content, 0, e.Content.Length);
      }
      // tell the builder the full path (becomes <img src="c:\foo\bar\asdfgh.png" />)
      return "/data/" + e.FileName;
    }

    public string InstanceOnCreateImage(object sender, CreateImageArguments e) {
      // TODO: assume content is already in the element (becomes FrozenFagment later on)
      var path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/data/"), e.FileName);
      // file is being written from DB to disc for later retrieve from PDF generator
      using (var file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite)) {
        file.Write(e.Content, 0, e.Content.Length);
      }
      // tell the builder the full path (becomes <img src="c:\foo\bar\asdfgh.png" />)
      return path;
    }

    public byte[] ContentBuilder(Element e) {
      switch (e.GetType().Name) {
        case "Image":
          var properties = System.Web.Helpers.Json.Decode<ImageProperties>(e.Properties);
          var image = ImageUtil.ApplyImageProperties(e.Content, properties);
          using (var ms = new MemoryStream()) {
            ;
            image.Save(ms, ImageFormat.Png);
            return ms.ToArray();
          }
        default:
          return e.Content;
      }
    }

    /// <summary>
    /// Images are created by using a call to GetImage, such as <img src="AuthorPortal/Opus/GetImage/1" />
    /// </summary>
    /// <param name="id"></param>
    /// <param name="convertImage"></param>
    /// <returns></returns>
    public dynamic GetImage(int id, bool? convertImage) {
      var img = EditorManager.Instance.GetElement<ImageSnippet>(id);
      // with the img we get the id and can pull the real data from blob cache
      var properties = System.Web.Helpers.Json.Decode<ImageProperties>(img.Properties);
      //if (convertImage.HasValue && !convertImage.Value)
      //    return Controller.File(img.Content, "image/png");
      var image = ImageUtil.ApplyImageProperties(img.Content, properties);
      using (var ms = new MemoryStream()) {
        image.Save(ms, ImageFormat.Png);
        return ms;
        //return File(ms.ToArray(), "image/png");
      }
    }

    /// <summary>
    /// Returns all images from given project.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="height"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    public IList<RibbonImages> GetRibbonImagesListFromProject(int projectId, int height, int width) {
      var imagesList = new List<RibbonImages>();
      var resources = ProjectManager.Instance.GetResourceFiles(projectId, TypeOfResource.Content, "image", null).ToList();
      var folder = ProjectManager.Instance.GetResourceFolders(projectId, TypeOfResource.Content).ToList();
      if (resources.Any()) {
        imagesList.AddRange(resources.Select(res => new RibbonImages {
          id = res.Id,
          imageUrl = String.Format("/Editor/GetThumbnail/{0}?w={1}&h={2}&m={3}", res.Id, width, height, res.MimeType),
          name = res.Name,
          folder = folder.Any(f => f.Children.Any(c => c.ResourceId == res.ResourceId)) ? folder.First(f => f.Children.Any(c => c.ResourceId == res.ResourceId)).Name : ""
        }));
      }
      return imagesList;
    }

    /// <summary>
    ///  Returns all images from the project that contains the given opus.
    /// </summary>
    /// <param name="docId"></param>
    /// <param name="height"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    public IList<RibbonImages> GetRibbonImagesListFromOpus(int docId, int height, int width) {
      var imagesList = new List<RibbonImages>();
      var opus = ProjectManager.Instance.GetOpusInternal(docId);
      var resources = ProjectManager.Instance.GetResourceFiles(opus, TypeOfResource.Content, "image", null).ToList();
      var folder = ProjectManager.Instance.GetResourceFolders(opus, TypeOfResource.Content).ToList();
      if (resources.Any()) {
        imagesList.AddRange(resources.Select(res => new RibbonImages {
          id = res.Id,
          imageUrl = String.Format("/Editor/GetThumbnail/{0}?w={1}&h={2}", res.Id, width, height),
          name = res.Name,
          folder = folder.Any(f => f.Children.Any(c => c.ResourceId == res.ResourceId)) ? folder.First(f => f.Children.Any(c => c.ResourceId == res.ResourceId)).Name : ""
        }));
      }
      return imagesList;
    }

    # endregion

    # region Widget Helper

    public ChapterDataModel GetChapterModelForEdit(Opus doc, int? chapterId, Action<Element> saveNewChapter) {
      // we read chapter by chapter, which is the first level in the hierarchy by definition
      var chapters = doc.Children.OfType<Section>().OrderBy(c => c.OrderNr).ToList();
      Element currentChapter = null;
      Element prevChapter = null;
      Element nextChapter = null;
      IEnumerable<SnippetDataModel> run = null;
      if (chapterId.HasValue && chapters.Any()) {
        currentChapter = chapters.SingleOrDefault(c => c.Id == chapterId) ?? chapters.Last();
      }
      if (!chapterId.HasValue && chapters.Any()) {
        currentChapter = chapters.First();
      }
      if (currentChapter != null) {
        // take tha chapters content for view
        run = FlattenHierarchy(currentChapter, doc.ShowNumberChain);
        // take prev and next from index
        var idx = chapters.FindIndex(c => c.Id == currentChapter.Id);
        prevChapter = chapters.ElementAtOrDefault(idx - 1);
        nextChapter = chapters.ElementAtOrDefault(idx + 1);
      } else {
        // Create the first chapter on the fly and add to the DB immediately
        currentChapter = new Section { Parent = doc, Name = "Chapter 1", OrderNr = 1, Content = System.Text.Encoding.UTF8.GetBytes("Chapter 1") };
        saveNewChapter(currentChapter);
        run = new List<SnippetDataModel>(new[] { new SnippetDataModel { 
                  CurrentSnippet = currentChapter, 
                  ChapterId = currentChapter.Id, 
                  SnippetTitle = currentChapter.Name,
                  Decrease = false,
                  Increase = false
                } });
      }
      var rm = new ChapterDataModel {
        DocumentId = doc.Id,
        GenericChapterNumber = doc.ShowNumberChain ? currentChapter.OrderNr.ToString() : "#",
        CurrentChapter = currentChapter,
        ChapterElements = run,
        ChapterTitle = currentChapter.RawContent.Ellipsis(40).ToHtmlString(),
        PreviousChapter = prevChapter,
        PreviousChapterTitle = (prevChapter == null) ? "" : prevChapter.Name,
        NextChapter = nextChapter,
        NextChapterTitle = (nextChapter == null) ? "" : nextChapter.Name,
      };
      return rm;
    }

    /// <summary>
    /// This method is specially created to support multiple chapters
    /// </summary>
    /// <param name="doc">current document object</param>
    /// <param name="chapterId">current chapter id</param>
    /// <param name="saveNewChapter"></param>
    /// <returns>chapter data model</returns>
    public ChapterDataModel GetChapterModelForNewChapter(Opus doc, int? chapterId, Action<Element> saveNewChapter) {
      // we read chapter by chapter, which is the first level in the hierarchy by definition
      var chapters = doc.Children.OfType<Section>().OrderBy(c => c.OrderNr).ToList();
      Element currentChapter = null;
      Element prevChapter = null;
      Element nextChapter = null;
      IEnumerable<SnippetDataModel> run = null;
      IEnumerable<SnippetDataModel> reArrageRun = null;
      if (chapters.Any()) {
        currentChapter = chapters.SingleOrDefault(c => c.Id == chapterId) ?? chapters.First();
      }
      /*if (chapters.Any())
      {
          currentChapter = chapters.First();
      }*/
      if (currentChapter != null) {
        // take tha chapters content for view
        //reArrageRun = FlattenHierarchyForNewChapter(chapters, doc.ShowNumberChain);
        var chapterAsList = new List<Section> { currentChapter as Section };
        run = FlattenHierarchyForNewChapter(chapterAsList, doc.ShowNumberChain);
        // Rearrange all the items by order no
        // run = from x in reArrageRun
        //     orderby x.CurrentSnippet.OrderNr ascending
        //   select x;  
        // take prev and next from index
        var idx = chapters.FindIndex(c => c.Id == currentChapter.Id);
        prevChapter = chapters.ElementAtOrDefault(idx - 1);
        nextChapter = chapters.ElementAtOrDefault(idx + 1);
      } else {
        if (saveNewChapter == null) return null;
        // Create the first chapter on the fly and add to the DB immediately
        currentChapter = new Section { Parent = doc, Name = "Chapter 1", OrderNr = 1, Content = System.Text.Encoding.UTF8.GetBytes("Chapter 1") };
        saveNewChapter(currentChapter);
        run = new List<SnippetDataModel>(new[] { new SnippetDataModel { 
                  CurrentSnippet = currentChapter, 
                  ChapterId = currentChapter.Id, 
                  SnippetTitle = currentChapter.Name,
                  Decrease = false,
                  Increase = false
                } });
      }
      var rm = new ChapterDataModel {
        DocumentId = doc.Id,
        GenericChapterNumber = doc.ShowNumberChain ? currentChapter.OrderNr.ToString() : "#",
        CurrentChapter = currentChapter,
        ChapterElements = run,
        ChapterTitle = currentChapter.RawContent.Ellipsis(40).ToHtmlString(),
        PreviousChapter = prevChapter,
        PreviousChapterTitle = (prevChapter == null) ? "" : prevChapter.Name,
        NextChapter = nextChapter,
        NextChapterTitle = (nextChapter == null) ? "" : nextChapter.Name,
      };
      return rm;
    }

    public IEnumerable<SnippetDataModel> FlattenHierarchy(Element chapter, bool showChain) {
      var snippetList = new List<SnippetDataModel>();
      Func<List<Element>, List<SnippetDataModel>> helper = null;
      var section = chapter as Section;
      helper = nodes => {
        var ret = new List<SnippetDataModel>();
        foreach (var node in nodes) {
          var idx = nodes.OrderBy(e => e.OrderNr).ToList().FindIndex(n => n == node);
          ret.Add(new SnippetDataModel {
            ChapterId = chapter.Id,
            CurrentSnippet = node,
            SnippetTitle = node.Name,
            /*PrevSnippet = nodes.ElementAtOrDefault(idx - 1),
            NextSnippet = nodes.ElementAtOrDefault(idx + 1),*/
            PrevSnippet = Increase(node) ? nodes.ElementAtOrDefault(idx - 1) : null,
            NextSnippet = Decrease(node) ? nodes.ElementAtOrDefault(idx + 1) : null,
            SectionNumberChain = showChain ? node.GetSectionNumber(chapter.Id) : node.GetSectionLevel(chapter.Id),
            // Quick navi moves go only within a hierarchy
            Increase = Increase(node), // can go up always if not yet at chapter level
            Decrease = Decrease(node)
            /*Increase = node.Parent.Id != chapter.Id,                       // can go up always if not yet at chapter level
            Decrease = node.Parent.Children.OfType<Section>().OrderBy(se => se.OrderNr).Cast<Element>().ToList().IndexOf(node) > 0   // can go down if second in stack (can't be first, make 1. / 1.1 ==> 1. / 1.1.1 is not allowed)*/
          });
          if (node.HasChildren()) {
            ret.AddRange(helper(node.Children.OrderBy(c => c.OrderNr).ToList()));
          }
        }
        return ret;
      };
      // add chapter itself first, as it is editable
      snippetList.Add(new SnippetDataModel { ChapterId = chapter.Id, CurrentSnippet = chapter, SnippetTitle = chapter.Name });
      // add a flatten view of content
      if (chapter.HasChildren())
        snippetList.AddRange(helper(chapter.Children.OrderBy(c => c.OrderNr).ToList()));
      return snippetList;
    }

    /// <summary>
    /// This method is also created to support multiple chapters. This method tries to get all nodes with respect to their parents
    /// </summary>
    /// <param name="chapters">All chapters</param>
    /// <param name="showChain"></param>
    /// <returns>IEnumerable Snippet data model</returns>
    public IEnumerable<SnippetDataModel> FlattenHierarchyForNewChapter(List<Section> chapters, bool showChain) {
      var snippetList = new List<SnippetDataModel>();
      Func<List<Element>, List<SnippetDataModel>> helper = null;
      var currentIndex = 0;
      helper = nodes => {
        var ret = new List<SnippetDataModel>();

        foreach (var node in nodes) {
          var idx = nodes.OrderBy(e => e.OrderNr).ToList().FindIndex(n => n == node);
          ret.Add(new SnippetDataModel {
            ChapterId = chapters[currentIndex].Id,
            CurrentSnippet = node,
            SnippetTitle = node.Name,
            /*PrevSnippet = nodes.ElementAtOrDefault(idx - 1),
            NextSnippet = nodes.ElementAtOrDefault(idx + 1),*/
            PrevSnippet = Increase(node) ? nodes.ElementAtOrDefault(idx - 1) : null,
            NextSnippet = Decrease(node) ? nodes.ElementAtOrDefault(idx + 1) : null,
            SectionNumberChain = showChain ? node.GetSectionNumber(chapters[currentIndex].Id) : node.GetSectionLevel(chapters[currentIndex].Id),
            // Quick navi moves go only within a hierarchy
            Increase = Increase(node), // can go up always if not yet at chapter level
            Decrease = Decrease(node)
            /*Increase = node.Parent.Id != chapters[currentIndex].Id, // can go up always if not yet at chapter level
            Decrease = node.Parent.Children.OfType<Section>().OrderBy(se => se.OrderNr).Cast<Element>().ToList().IndexOf(node) > 0   // can go down if second in stack (can't be first, make 1. / 1.1 ==> 1. / 1.1.1 is not allowed)*/
          });
          if (node.HasChildren()) {
            ret.AddRange(helper(node.Children.OrderBy(c => c.OrderNr).ToList()));
          }
        }
        return ret;
      };
      // add chapter itself first, as it is editable
      foreach (var chapter in chapters) {
        snippetList.Add(new SnippetDataModel { ChapterId = chapter.Id, CurrentSnippet = chapter, SnippetTitle = chapter.Name });
        // add a flatten view of content
        if (chapter.HasChildren())
          snippetList.AddRange(helper(chapter.Children.OrderBy(c => c.OrderNr).ToList()));
        currentIndex++;
      }

      return snippetList;
    }

    public bool Decrease(Element node) {
      if (node is Section && node.Level >= 3 && node.Level < 6) {
        var lastSectionChild = node.Parent.Children.OrderBy(se => se.OrderNr).Where(e => e is Section && e.Parent == node.Parent && e.Level == node.Level && e.OrderNr < node.OrderNr);
        if (lastSectionChild.Any())
          return true;
      } else {
        var nodeParentLastChild = node.Parent.Children.OrderBy(se => se.OrderNr).Where(e => e.Level == node.Level && !(e is Section));
        if (!nodeParentLastChild.Any()) {
          return false;
        }
        if (nodeParentLastChild.Last().Id != node.Id)
          return true;
      }
      return false;
    }

    public bool Increase(Element node) {
      if (node is Section) {
        return node.Level > 3;
      }
      var nodeParentFirstChild = node.Parent.Children.First(e => e.Level == node.Level && !(e is Section));
      return nodeParentFirstChild.Id != node.Id;
    }

    public byte[] GetThumbnailImage(Stream image, int scaleToW, int scaleToH) {
      if (image == null) return null;
      try {
        using (var img = Image.FromStream(image)) {
          float w = img.Width;
          float h = img.Height;
          var scaleW = (w < scaleToW);
          var scaleH = (h > scaleToH);
          float scaleWFactor = scaleW ? (float)scaleToW / w : 1F;
          float scaleHFactor = scaleH ? (float)scaleToH / h : 1F;
          var scaleFactor = (scaleH && scaleW)
                              ? Math.Min(scaleHFactor, scaleWFactor)
                              : (scaleW ? scaleWFactor : scaleHFactor);
          var thmb = img.GetThumbnailImage(Convert.ToInt32(w * scaleFactor), Convert.ToInt32(h * scaleFactor), null,
                                           IntPtr.Zero);
          using (var newMs = new MemoryStream()) {
            thmb.Save(newMs, ImageFormat.Png);
            return newMs.ToArray();
          }
        }

      } catch {
        return GetStaticImage(HttpContext.Current.Server.MapPath("~/Content/icons/earth_network_48.png"));
      }
    }

    public byte[] GetThumbnailImage(byte[] image, int scaleToW, int scaleToH) {
      if (image == null) return null;
      using (var ms = new MemoryStream(image)) {
        return GetThumbnailImage(ms, scaleToW, scaleToH);
      }
    }

    public byte[] GetStaticImage(string fullPath) {
      using (var ms = new MemoryStream()) {
        var img = Image.FromFile(fullPath);
        img.Save(ms, ImageFormat.Png);
        var buffer = ms.ToArray();
        return buffer;
      }
    }

    /// <summary>
    /// Calculate the previous/ next position and return the current element, if there is no prev/next.
    /// </summary>
    /// <param name="opus"></param>
    /// <param name="chapterId"></param>
    /// <param name="previous"></param>
    /// <returns></returns>
    public Element GetPrevNextChapter(Opus opus, int chapterId, bool previous) {
      // keep order for subsequent operations
      var orderedChapters = opus.Children.OrderBy(e => e.OrderNr).ToList();
      var chapters = orderedChapters.Select(e => e.Id).ToList();
      if (chapterId == 0) {
        // assume it's the first chapter not yet set
        chapterId = chapters.First();
      }
      var currentPos = chapters.FindIndex(e => e == chapterId);
      if (currentPos == 0 && previous) return orderedChapters.First();
      if (currentPos == chapters.Count - 1 && !previous) return orderedChapters.Last();
      return orderedChapters.ElementAt(currentPos + (previous ? -1 : 1));
    }

    //public JsTreeModel GetOpusTree(int id) {
    //  var opus = Ctx.Opuses.Find(id);
    //  return new JsTreeModel {
    //    attr = new JsTreeAttribute{ id = opus.Id.ToString(), rel = "folder"},
    //    data = opus.Name,
    //    children = GetSnippetTreeModel(opus.Children)
    //  };
    //}

    //private JsTreeModel[] GetSnippetTreeModel(IList<Element> elements) {
    //  if (elements == null || !elements.Any()) return null;
    //  var ls = new List<JsTreeModel>();
    //  foreach (var n in elements) {
    //    ls.Add(new JsTreeModel {
    //      data = n.Name.Ellipsis(50).ToHtmlString(),
    //      attr = new JsTreeAttribute {
    //        id = n.Id.ToString(),
    //        rel = (n.Children != null && n.Children.Any()) ? "folder" : "file"
    //      },
    //      children = GetSnippetTreeModel(n.Children)
    //    });
    //  }
    //  return ls.ToArray();
    //}

    # endregion


    public void SaveReorganizedTree(int opusId, int source, int target, bool after) {
      var opus = Ctx.Opuses.Find(opusId);
      var src = Ctx.Elements.Find(source);
      var trg = Ctx.Elements.Find(target);
      var orderNr = 0;
      if (trg is Section && after) {
        // if target can have children, just change the parent and reorder        
        orderNr = 1;
        trg.Children.OrderBy(c => c.OrderNr).ForEach(c => c.OrderNr = orderNr++);
        src.Parent = trg;
        src.OrderNr = 0;
      } else if (trg is Section) {
        // this is a drop before a section element, we assume that this is not meant to target the section but add a leaf at the end

      }
      else {
        // if target is a leaf element, get its parent first and keep its order        
        orderNr = after ? trg.OrderNr : trg.OrderNr - 1;
        src.Parent = trg.Parent;
        src.OrderNr = orderNr++;
        trg.Children.Where(c => c.OrderNr >= orderNr).OrderBy(c => c.OrderNr).ForEach(c => c.OrderNr = orderNr++);
      }
      SaveChanges();
    }
  }
}