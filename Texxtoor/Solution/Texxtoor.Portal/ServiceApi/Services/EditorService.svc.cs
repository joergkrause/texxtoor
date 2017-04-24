using System.Diagnostics;
using System.ServiceModel.Activation;
using System.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;
using System.Web.Script.Serialization;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Utilities.Imaging;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.ViewModels.Common;
using Texxtoor.ViewModels.Editor;
using Texxtoor.Portal.Services;
using Texxtoor.DataModels.Attributes;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;

namespace Texxtoor.Portal.ServiceApi.Services {

  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
  [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
  [DataContract(Namespace = "http://www.texxtoor.de")]
  public class EditorService : IEditorService {

    private const string Key = "__orphanes";

    public dynamic GetSnippet(int id) {
      var i = EditorManager.Instance.GetElement(id) as Snippet;
      if (i != null) {
        var chapter = EditorManager.Instance.GetChapterForElement(EditorManager.Instance.GetElement(id)) as Snippet;
        var doc = chapter.GetOpus();
        var json =
          i.GetType()
           .GetCustomAttributes(typeof(EditorServiceWrapperAttribute), true)
           .OfType<EditorServiceWrapperAttribute>()
           .First()
           .GetJson(doc, chapter, i);
        var objSerializer = new JavaScriptSerializer();
        return objSerializer.Serialize(json);
      }
      return null;
    }

    public dynamic InsertSnippet(int documentId, int chapterId, int id, string type, string variation, string data) {
      var level = 0;
      var opus = ProjectManager.Instance.GetOpus(documentId, HttpContext.Current.User.Identity.Name);
      var chapter = EditorManager.Instance.GetElement(chapterId) as Snippet;
      var currentElement = EditorManager.Instance.GetElement(id > 0 ? id : chapterId);
      JsonBehavior json;
      var changeElements = new List<JsonBehavior>();
      Snippet ns;
      switch (type.ToLowerInvariant()) {
        case "chapter":
          ns = EditorManager.Instance.CreateChapterSnippet(documentId, currentElement);
          json = ns.GetType().GetCustomAttributes(typeof(EditorServiceWrapperAttribute), true).OfType<EditorServiceWrapperAttribute>().Single().GetJson(opus, chapter, ns as Snippet);
          break;
        case "section":
          level = 3;  // opus == 1, chapter == 2
          goto case "insertsection";
        case "subsection":
          level = 4;
          goto case "insertsection";
        case "subsubsection":
          level = 5;
          goto case "insertsection";
        case "subsubsubsection":
          level = 6;
          goto case "insertsection";
        case "insertsection":
          ns = EditorManager.Instance.CreateSectionSnippet(documentId, currentElement, level);
          //json = (EditorServiceWrapper)Attribute.("Section", typeof(EditorServiceWrapper));
          //typeof(EditorServiceWrapper).GetType().GetCustomAttributes(typeof(EditorServiceWrapper), true).OfType.Where(e=>e.
          //var snippet = ns as Snippet;
          json = (ns).GetType().GetCustomAttributes(typeof(EditorServiceWrapperAttribute), true).OfType<EditorServiceWrapperAttribute>().Single().GetJson(opus, chapter, ns);
          changeElements = EditorManager.Instance.ConvertElementToJsonSnippet(opus, chapter, EditorManager.Instance.GetSectionsAfterLevel(ns, level));
          break;
        case "text":
          ns = EditorManager.Instance.CreateTextSnippet(documentId, currentElement);
          json = (ns).GetType().GetCustomAttributes(typeof(EditorServiceWrapperAttribute), true).OfType<EditorServiceWrapperAttribute>().First().GetJson(opus, chapter, ns);
          break;
        case "img":
          # region Image
          var resId = Convert.ToInt32(data); // resId is from resources, we need to pull this from blob storage first and save to element
          if (resId != 0) {
            // 1. Retrieve Res, 2. read blob, 3. put in content (hard copy of everything)
            var res = ProjectManager.Instance.GetResource(resId) as ResourceFile;
            if (res != null) {
              ns = EditorManager.Instance.CreateImageSnippet(documentId, currentElement, res);
              json = (ns).GetType().GetCustomAttributes(typeof(EditorServiceWrapperAttribute), true).OfType<EditorServiceWrapperAttribute>().First().GetJson(opus, chapter, ns);
            } else {
              ns = new ImageSnippet();
              json = null;
            }
          } else {
            ns = new ImageSnippet();
            json = null;
          }

          # endregion
          break;
        case "table":
          # region Table
          string rows = "", cols = "";
          if (!String.IsNullOrEmpty(data) && data.Contains(",")) {
            rows = data.Split(",".ToCharArray())[0];
            cols = data.Split(",".ToCharArray())[1];
          }
          if (String.IsNullOrEmpty(variation)) {
            variation = "simple";
          }
          ns = EditorManager.Instance.CreateTableSnippet(documentId, currentElement, rows, cols, variation) as Snippet;
          json = (ns).GetType().GetCustomAttributes(typeof(EditorServiceWrapperAttribute), true).OfType<EditorServiceWrapperAttribute>().First().GetJson(opus, chapter, ns);
          # endregion
          break;
        case "listing":
          ns = EditorManager.Instance.CreateListingSnippet(documentId, currentElement);
          json = (ns).GetType().GetCustomAttributes(typeof(EditorServiceWrapperAttribute), true).OfType<EditorServiceWrapperAttribute>().First().GetJson(opus, chapter, ns);
          break;
        case "sidebar":
          ns = EditorManager.Instance.CreateSidebarSnippet(documentId, currentElement);
          json = (ns).GetType().GetCustomAttributes(typeof(EditorServiceWrapperAttribute), true).OfType<EditorServiceWrapperAttribute>().First().GetJson(opus, chapter, ns);
          break;
        default:
          throw new ArgumentOutOfRangeException(type + " is an unknown command");
      }
      var redirectId = (type == "chapter" ? ns.Id : -1);
      return JsonConvert.SerializeObject(new { snippet = json, id = ns.Id, msg = ns.GetType().Name, relocateTo = redirectId, snippetsData = changeElements });
    }

    #region -= Tree Helper =-

    public dynamic GetTreeData(int documentId) {
      var doc = ProjectManager.Instance.GetOpus(documentId, HttpContext.Current.User.Identity.Name);

      var tree = (new List<Opus> { doc }).RecursiveSelect<Element, JsTreeModel>(
        c => c.Children.OrderBy(e => e.OrderNr),
        (e, c) => new JsTreeModel {
          data = e.Name,
          attr = new JsTreeAttribute {
            id = e.Id.ToString(),
            rel = (e is Snippet) ? "snippet" : ((e is Section) ? "section" : "opus")
          },
          children = c.ToArray()
        });
      var objSerializer = new JavaScriptSerializer();
      return objSerializer.Serialize(tree);
    }

    #endregion

    public dynamic InsertOrphanedSnippet(int documentId, int chapterId, int id, int afterSnippet) {
      if (HttpContext.Current.Session[Key] == null) {
        HttpContext.Current.Session[Key] = new List<Element>();
      }
      var or = (List<Element>)HttpContext.Current.Session[Key];
      var insert = or.Single(e => e.Id == id);
      var inserted = InsertSnippet(documentId, chapterId, afterSnippet, insert.WidgetName, null, null); // EditorManager.Instance.InsertOrphanedElement(insert, afterSnippet);
      var newId = (int)inserted.id;
      inserted.snippet.content = Encoding.UTF8.GetString(insert.Content);
      // for sync with DB
      EditorManager.Instance.SetElementContent(newId, insert.Content);
      return inserted;
    }

    public dynamic GetContentStructure(int id, int chapterId) {

      var opus = ProjectManager.Instance.GetOpus(id, HttpContext.Current.User.Identity.Name);
      var rm = EditorManager.Instance.GetChapterModelForEdit(opus, chapterId, null);
      var structure = rm.ChapterElements.Select(x => new { x.CurrentSnippet.Id, x.CurrentSnippet.RawContent.Length });
      var objSerializer = new JavaScriptSerializer();
      dynamic result = objSerializer.Serialize(structure);
      return result;
    }

    /// <summary>
    /// GetContentStructureForNewChapter is to handler new created chapters
    /// </summary>
    /// <param name="id">document ID</param>
    /// <param name="chapterId">Newly added chapter id</param>
    /// <returns>JSON Response</returns>
    public dynamic GetContentStructureForNewChapter(int id, int chapterId) {
      var opus = ProjectManager.Instance.GetOpus(id, HttpContext.Current.User.Identity.Name);
      var rm = EditorManager.Instance.GetChapterModelForNewChapter(opus, chapterId, null);
      var structure = rm.ChapterElements.Select(x => new { x.CurrentSnippet.Id, x.CurrentSnippet.RawContent.Length });
      var objSerializer = new JavaScriptSerializer();
      return objSerializer.Serialize(structure);
    }

    public dynamic SearchSnippetId(int id, int chapterId, int snippetId, string value, int direction) {
      var opus = ProjectManager.Instance.GetOpus(id, HttpContext.Current.User.Identity.Name);
      var rm = EditorManager.Instance.GetChapterModelForEdit(opus, chapterId, null);
      if (direction < -1) direction = -1;
      if (direction > 1) direction = 1;
      if (direction == 0) direction = 1;
      var json = new ResultJsonBehavior { snippetId = EditorManager.Instance.GetSnippetId(rm.ChapterElements.ToList(), snippetId, direction, value) };
      var objSerializer = new JavaScriptSerializer();
      return objSerializer.Serialize(json);
    }

    public dynamic Move(int id, int chapterId, int sectionId, int dropId, string move, bool withChildren) {
      var document = ProjectManager.Instance.GetOpus(id, HttpContext.Current.User.Identity.Name);
      var chapter = EditorManager.Instance.GetElement(chapterId) as Snippet;
      var objSerializer = new JavaScriptSerializer();
      if (!String.IsNullOrEmpty(move) && new[] { "d", "u", "r", "l", "m", "g", "s" }.Contains(move) && sectionId > 0) {
        // moving is always an exchange operation, two element simply change their order        
        var sn = EditorManager.Instance.GetElement(sectionId);
        var result = false;
        var changedElements = new List<JsonBehavior>();
        //var changedElements = new List<Element>();
        switch (move) {
          case "r":
            // decrease (e.g. 1.1 ==> 1.1.1)
            //changeElements = EditorManager.Instance.ConvertElementToJsonSnippet(documentId, chapterId, EditorManager.Instance.GetSectionsAfterLevel(ns, level));
            changedElements = EditorManager.Instance.ConvertElementToJsonSnippet(document, chapter, EditorManager.Instance.DecreaseSectionLevel(sn, withChildren));
            result = true;
            break;
          case "l":
            // increase (e.g. 1.1.1 ==> 1.1)
            changedElements = EditorManager.Instance.ConvertElementToJsonSnippet(document, chapter, EditorManager.Instance.IncreaseSectionLevel(sn, withChildren));
            result = true;
            break;
          case "m":
            // move anywhere using drag drop
            if (dropId > 0) {
              var drop = EditorManager.Instance.GetElement(dropId);
              changedElements = EditorManager.Instance.ConvertElementToJsonSnippet(document, chapter, EditorManager.Instance.MoveElement(sn, drop));
              result = true;
            }
            break;
          case "d":
          case "u":
            // move one element up or down
            changedElements = EditorManager.Instance.ConvertElementToJsonSnippet(document, chapter, EditorManager.Instance.MoveElementNext(sn, move == "d"));
            result = changedElements.Any();
            break;
          case "g":
            // merge to text elements
            changedElements = EditorManager.Instance.ConvertElementToJsonSnippet(document, chapter, EditorManager.Instance.MergeNextElement(sn));
            break;
          case "s":
            // sections are moved using drag and drop
            if (dropId > 0) {
              var drop = EditorManager.Instance.GetElement(dropId);
              changedElements = EditorManager.Instance.ConvertElementToJsonSnippet(document, chapter, EditorManager.Instance.MoveSections(sn, drop));
              result = true;
            }
            break;
        }
        if (changedElements == null)
          return JsonConvert.SerializeObject(new { msg = "Warning: this move is not allowed" });
        else
          return JsonConvert.SerializeObject(new { success = result, snippetsData = changedElements });
      }
      return objSerializer.Serialize(null);
    }

    public dynamic OrphanSnippet(int id, bool withChildren) {
      var sn = EditorManager.Instance.GetElement(id);
      var result = DeleteSnippet(id, withChildren);
      if (HttpContext.Current.Session[Key] == null) {
        HttpContext.Current.Session[Key] = new List<Element>();
      }
      var or = (List<Element>)HttpContext.Current.Session[Key];
      if (or.All(e => e.Id != id)) {
        or.Add(sn);
      }
      HttpContext.Current.Session[Key] = or;
      return result;
    }

    public dynamic DeleteSnippet(int id, bool delChildren) {
      try {
        var sn = EditorManager.Instance.GetElement(id);
        var refreshId = id;
        var children = new List<int>();
        var thisChapter = EditorManager.Instance.GetChapterForElement(sn);
        if (sn is Section) {
          // check if the deleting element is a chapter
          if (sn.Id == thisChapter.Id)
            // check if that chapter is the first and default chapter of the document
            if (thisChapter.OrderNr == 1)
              return null;

          if (delChildren) {
            // client needs list if recursively removed children to update UI
            EditorManager.Instance.DeleteElementWithChildren(sn.Children, children);
          } else {
            if (sn.HasChildren()) {
              var orderNr = sn.OrderNr;
              //sn.Children.ForEach(c =>
              //{
              //    c.Parent = sn.Parent;
              //    c.OrderNr = orderNr++;  // first child replaces the deleted snippet, hence we increase afterwards
              //    children.Add(c.Id);
              //});
            }
          }
        }
        var changedElements = EditorManager.Instance.DeleteElement(sn);
        var json = new ResultJsonBehavior { snippetId = id, msg = "Snippet removed and reordered", relocateTo = refreshId, children = children, snippetsData = changedElements.Select(e => e.Id) };
        var objSerializer = new JavaScriptSerializer();
        return objSerializer.Serialize(json);
        //return new JsonResult { Data = new { id, msg = "Snippet removed and reordered", relocateTo = refreshId, children, snippetsData = changedElements.Select(e => e.Id) } };
      } catch (Exception) {
        return null;
      }
    }

    public dynamic OrphanedSnippets() {
      if (HttpContext.Current.Session[Key] == null) {
        HttpContext.Current.Session[Key] = new List<Element>();
      }
      var or = (List<Element>)HttpContext.Current.Session[Key];
      HttpContext.Current.Session[Key] = or;
      var objSerializer = new JavaScriptSerializer();
      return objSerializer.Serialize(or.Select(e => new {
        e.Id,
        Name = String.Format("{0}: {1}", e.WidgetName, e.RawContent.Ellipsis(20).ToHtmlString().StripTags())
      }));
    }

    /// <summary>
    /// For document id move an element source within the hierarchy to element target, so it becomes a child if target.
    /// If the target element is a leaf element that cannot have children, the element is attached to the leaf's parent and get's the order position AFTER the selected leaf.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="position">'after' or 'before'</param>
    /// <returns></returns>
    public dynamic SaveReorganizedTree(int id, int source, int target, string position) {
      EditorManager.Instance.SaveReorganizedTree(id, source, target, position == "after");
      var objSerializer = new JavaScriptSerializer();
      return objSerializer.Serialize(
      new {
        msg = "OK"
      });
    }

    public dynamic SaveContent(int id, int documentId, string content, string form) {
      var objSerializer = new JavaScriptSerializer();
      try {
        //string[] data = form.Split(',');
        // some additional fields either from toolset or from might be added
        var frm = HttpUtility.ParseQueryString(form);
        var refreshSection = EditorManager.Instance.SaveContent(id, frm, content);
        return objSerializer.Serialize(refreshSection ? new { msg = String.Format("Successfully saved !"), sectionRefresh = (refreshSection ? content : null) } : new { msg = String.Format("Nothing to save"), sectionRefresh = "" });
      } catch (Exception ex) {
        var json = new ResultJsonBehavior { msg = String.Format("Error ! {0}", ex.Message) };
        return objSerializer.Serialize(json);
      }
    }

    public dynamic GetSidebarType() {
      var objSerializer = new JavaScriptSerializer();
      Func<string, string> sideBarLocalizer = s => {
        var da = typeof(SidebarType).GetField(s)
                                     .GetCustomAttributes(typeof(DisplayAttribute), true)
                                     .Cast<DisplayAttribute>()
                                     .SingleOrDefault();
        return da == null ? s : da.GetName();
      };
      var json = Enum.GetValues(typeof(SidebarType)).Cast<int>().Select(t => new {
        Id = (int)t,
        Name = sideBarLocalizer(Enum.GetName(typeof(SidebarType), t))
      });
      return objSerializer.Serialize(json);
    }

    public dynamic GetThumbnails(int id, string w, string h) {
      var width = String.IsNullOrEmpty(w) ? null : new int?(Int32.Parse(w));
      var height = String.IsNullOrEmpty(h) ? null : new int?(Int32.Parse(h));
      var ribbonImages = EditorManager.Instance.GetRibbonImagesListFromOpus(id, width ?? 32, height ?? 32);
      return JsonConvert.SerializeObject(new { ribbonImages, ribbonTitle = "Insert Image" });
    }

    public dynamic SemanticLists(int id, string type) {
      var sl = ProjectManager.Instance.GetSemanticListForDocument(id, (TermType)Enum.Parse(typeof(TermType), type, true));
      var objSerializer = new JavaScriptSerializer();
      return objSerializer.Serialize(sl);
    }

    public List<int> GetAllChapterIds(int id) {
      var opus = ProjectManager.Instance.GetOpus(id, HttpContext.Current.User.Identity.Name);
      return opus.Children.OrderBy(e => e.OrderNr).Select(e => e.Id).ToList();
    }

    public dynamic LoadChapter(int id, int chapterId) {
      var opus = ProjectManager.Instance.GetOpus(id, HttpContext.Current.User.Identity.Name);
      var chapter = opus.Children.OfType<Section>().OrderBy(c => c.OrderNr).Single(e => e.Id == chapterId);
      var allChapterSnippets = new List<JsonBehavior>();
      allChapterSnippets.AddRange(EditorManager.Instance.GetAllChildrenSnippets(opus, chapter, chapter));
      return JsonConvert.SerializeObject(new { snippets = allChapterSnippets });
    }

    public dynamic GetNextChapter(int id, int chapterId, string dir) {
      var opus = ProjectManager.Instance.GetOpus(id, HttpContext.Current.User.Identity.Name);
      var chapter = EditorManager.Instance.GetPrevNextChapter(opus, chapterId, dir == "prev") as Snippet;
      var allChapterSnippets = new List<JsonBehavior>();

      allChapterSnippets.AddRange(EditorManager.Instance.GetAllChildrenSnippets(opus, chapter, chapter));
      return JsonConvert.SerializeObject(new { snippets = allChapterSnippets });
    }

    #region Partial View Methods

    public dynamic Toc(int id) {
      var doc = ProjectManager.Instance.GetOpus(id, HttpContext.Current.User.Identity.Name);
      var chapters = doc.Children.OfType<Section>().OrderBy(c => c.OrderNr).Select(c => new {
        c.Name,
        c.Id,
        HasChildren = c.HasChildren(),
        c.OrderNr,
        c.ReadOnly
      });
      return JsonConvert.SerializeObject(chapters.ToArray());
    }

    #endregion

    public Stream GetImage(int id, bool c, string m) {
      var img = EditorManager.Instance.GetElement<ImageSnippet>(id);
      // with the img we get the id and can pull the real data from blob cache
      var properties = System.Web.Helpers.Json.Decode<ImageProperties>(img.Properties);
      WebOperationContext.Current.OutgoingResponse.ContentType = img.MimeType;
      if (!c || m.Equals("image/svg+xml")) {
        return new MemoryStream(img.Content) { Position = 0 };
      }
      var image = ImageUtil.ApplyImageProperties(img.Content, properties);
      var ms = new MemoryStream();
      image.Save(ms, ImageFormat.Png);
      ms.Position = 0;
      return ms;
    }

    public dynamic GetDialogData(string id, string dialog, string additionalData) {
      var iid = 0;
      Int32.TryParse(id, out iid);
      var opus = ProjectManager.Instance.GetOpus(iid, HttpContext.Current.User.Identity.Name);
      switch (dialog) {
        case "reorganize":
          // deliver a true object hierarchy which we convert to a <ul><li><dl><dt><dd></dl><& structure on the client
          Func<IEnumerable<Snippet>, JsTreeModel[]> convertToTree = null;
          convertToTree = (a) => a.Select(e => new JsTreeModel {
            data = Encoding.UTF8.GetString(e.Content),
            attr = new JsTreeAttribute {
              id = String.Format("il-{0}", e.Id),
              rel = e.WidgetName,
              dataitem = e.Id.ToString(),
              datatext = e.Name
            },
            children =
              (e.HasChildren() ? convertToTree(e.Children.OrderBy(s => s.OrderNr).OfType<Snippet>()) : null)
          }).ToArray();
          return
            JsonConvert.SerializeObject(new {
              options = new {
                linkTree = convertToTree(opus.Children.OrderBy(s => s.OrderNr).OfType<Snippet>())
              }
            });
          break;
        case "internal":
          Func<Element, IDictionary<string, int>, string> localizeWidget = (s, c) => {
            switch (s.WidgetName) {
              case "Text":
                return "Text";
              case "Listing":
                c["Listing"] = c["Listing"] + 1;
                return String.Format("Listing {0}", c["Listing"]);
              case "Sidebar":
                return "Sidebar";
              case "Image":
                c["Image"] = c["Image"] + 1;
                return String.Format("Figure {0}", c["Image"]);
              case "Table":
                c["Table"] = c["Table"] + 1;
                return String.Format("Table {0}", c["Table"]);
              case "Section":
                return s.Level == 1 ? "Chapter" : "Section";
            }
            return "Other";
          };
          var counter = new Dictionary<string, int> { { "Listing", 0 }, { "Image", 0 }, { "Table", 0 } };
          Func<Snippet, string> titleOrName = snippet => {
            if (snippet is TitledSnippet) {
              if (!String.IsNullOrEmpty(((TitledSnippet)snippet).Title)) {
                return ((TitledSnippet)snippet).Title;
              }
            }
            return snippet.Name;
          };
          Func<IEnumerable<Snippet>, bool, JsTreeModel[]> convertToModel = null;
          convertToModel = (a, b) => {
            if (b) {
              counter["Listing"] = 0;
              counter["Image"] = 0;
              counter["Table"] = 0;
            }
            return a.Select(e => new JsTreeModel {
              data = String.Format("[{1}] {0}", titleOrName(e), localizeWidget(e, counter)),
              attr = new JsTreeAttribute {
                id = String.Format("il-{0}", e.Id),
                rel = e.WidgetName,
                dataitem = e.Id.ToString(),
                datatext = e.Name
              },
              children =
                (e.HasChildren() ? convertToModel(e.Children.OrderBy(s => s.OrderNr).OfType<Snippet>(), false) : null)
            }).ToArray();
          };
          return
            JsonConvert.SerializeObject(new {
              options = new {
                linkTree = convertToModel(opus.Children.OrderBy(s => s.OrderNr).OfType<Snippet>(), true)
              }
            });
        case "external":
          return
            JsonConvert.SerializeObject(new {
              options = new {
                linkTree = 0
              }
            });
        case "imagecrop":
          var imageToCrop = EditorManager.Instance.GetElement(Int32.Parse(additionalData)) as ImageSnippet;
          return
            JsonConvert.SerializeObject(new {
              options = new {
                noRatio = true,
                keepRatio = false,
                setRatio = false,
                cropX = imageToCrop.ImageProperties.Crop.XOffset,
                cropY = imageToCrop.ImageProperties.Crop.YOffset,
                cropWidth = imageToCrop.ImageProperties.Crop.CropWidth,
                cropHeight = imageToCrop.ImageProperties.Crop.CropHeight,
                ratioWidth = imageToCrop.ImageProperties.ImageWidth,
                ratioHeight = imageToCrop.ImageProperties.ImageHeight
              }
            });
        case "imagecolors":
          var imageToColor = EditorManager.Instance.GetElement(Int32.Parse(additionalData)) as ImageSnippet;
          return
            JsonConvert.SerializeObject(new {
              options = new {
                transparent = imageToColor.ImageProperties.Colors.TransparentColor,
                brightness = imageToColor.ImageProperties.Colors.Brightness,
                contrast = imageToColor.ImageProperties.Colors.Contrast,
                hue = imageToColor.ImageProperties.Colors.Hue,
                saturation = imageToColor.ImageProperties.Colors.Saturation
              }
            });
        case "imageupload":
          return
            JsonConvert.SerializeObject(new { options = new { noptions = true } });
        case "figurepicker":
          var w = Int32.Parse(additionalData.Split('x')[0]);
          var h = Int32.Parse(additionalData.Split('x')[1]);
          return
            JsonConvert.SerializeObject(new {
              options = new {
                images = EditorManager.Instance.GetRibbonImagesListFromOpus(iid, h, w)
              }
            });
        case "properties":
          return
            JsonConvert.SerializeObject(new {
              options = new {
                allowChapters = opus.AllowChapters,
                allowMetaData = opus.AllowMetaData,
                chapterDefault = opus.ChapterDefault,
                sectionDefault = opus.SectionDefault,
                textSnippetDefault = opus.TextSnippetDefault,
                listingSnippetDefault = opus.ListingSnippetDefault,
                showNaviPane = opus.ShowNaviPane,
                showFlowPane = opus.ShowFlowPane,
                showNumberChain = opus.ShowNumberChain
              }
            });
        case "comments":
          return
            JsonConvert.SerializeObject(new { options = new { } });
      }
      throw new NotImplementedException();
    }

    public dynamic SaveDialogData(string id, string dialog, Dictionary<string, string> form) {
      var userName = HttpContext.Current.User.Identity.Name;
      switch (dialog) {
        case "properties":
          EditorManager.Instance.SetDocumentProperties(Int32.Parse(id),
                                         form.ContainsKey("allowChapters"),
                                         form.ContainsKey("allowMetaData"),
                                         form["chapterDefault"],
                                         form["sectionDefault"],
                                         form["textSnippetDefault"],
                                         form["listingSnippetDefault"],
                                         form.ContainsKey("showFlowPane"),
                                         form.ContainsKey("showNaviPane"),
                                         form.ContainsKey("showNumberChain"),
                                         userName);
          break;
      }
      var json = new ResultJsonBehavior { msg = "OK" };
      var objSerializer = new JavaScriptSerializer();
      return objSerializer.Serialize(json);
    }

    // This method is not in use
    public dynamic UploadImage(string id, string title, Stream fileContents) {
      const int bufferSize = 8 * 1024 * 2;
      byte[] buffer = new byte[bufferSize];
      MemoryStream ms = new MemoryStream();
      int bytesRead, totalBytesRead = 0;
      do {
        bytesRead = fileContents.Read(buffer, 0, buffer.Length);
        totalBytesRead += bytesRead;

        ms.Write(buffer, 0, bytesRead);
      } while (bytesRead > 0);
      using (var fileStream = File.Create(@"D:\AllTxtFiles.jpg")) {
        ms.CopyTo(fileStream);
      }
      var json = new ResultJsonBehavior { msg = "OK" };
      var objSerializer = new JavaScriptSerializer();
      return objSerializer.Serialize(json);

    }

    public dynamic LoadComments(int id, int snippetId, string target) {
      var user = HttpContext.Current.User.Identity.Name;
      var comments = new {
        Target = target,
        Snippet = snippetId,
        Comments = EditorManager.Instance.GetComments(snippetId, target)
                                .Select(c => new {
                                  c.Subject,
                                  Text = c.Content,
                                  // TODO: Fake 
                                  Date = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(),
                                  UserName = user
                                }).ToArray()
      };
      var objSerializer = new JavaScriptSerializer();
      return objSerializer.Serialize(comments);
    }

    public dynamic SaveComment(int id, int snippetId, string target, string subject, string comment, bool closed) {
      var noTagWord = new List<string> { "the", "that", "if", "then", "where" };
      var currentComments = EditorManager.Instance.GetComments(snippetId, target).ToList();
      var user = Manager<UserManager>.Instance.GetUserByName(HttpContext.Current.User.Identity.Name);
      var element = EditorManager.Instance.GetElement(snippetId);
      // currently, we do not allow comments on text level, so we go to first element (chapter) instead
      var item = new WorkitemChat {
        Content = comment,
        Owner = user, // TODO: Replace with User
        Closed = closed,
        Mood = 2,
        Private = target == "me",
        GroupOnly = target == "team",
        Snippet = (Element)element,
        Subject = subject,
        Name = target,
        Tags = String.Join(",", comment.Split(' ', '.', ',').Where(e => !noTagWord.Contains(e)).ToArray()),
        OrderNr = currentComments.Any() ? currentComments.Max(e => e.OrderNr) + 1 : 1
      };
      EditorManager.Instance.SaveComment(null, item);
      // avoid another DB call to simply get a fast response and refresh UI
      currentComments.Insert(0, item);
      // return the new set immediately to keep the display current
      var comments = new {
        Target = target,
        Snippet = snippetId,
        Comments = currentComments
                                .Select(c => new {
                                  c.Subject,
                                  Text = c.Content,
                                  // TODO: Fake 
                                  Date = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(),
                                  user.UserName
                                }).ToArray()
      };
      var objSerializer = new JavaScriptSerializer();
      return objSerializer.Serialize(comments);
    }

    public dynamic GetDocumentProperties(int id) {
      var objSerializer = new JavaScriptSerializer();
      var docProperties = new {
        wordCount = EditorManager.Instance.GetWordCount(id),
        charCount = EditorManager.Instance.GetCharacterCount(id)
      };
      return objSerializer.Serialize(docProperties);
    }

    public string GetTranslation(int id, Translators engine, string fromLanguage, string toLanguage) {
      var element = EditorManager.Instance.GetElement(id);
      if (element == null) return String.Empty;
      var text = Encoding.UTF8.GetString(element.Content);
      if (String.IsNullOrEmpty(text)) return String.Empty;
      var t = TranslatorFactory.GetTranslator(engine);
      return t.Translate(text, fromLanguage, toLanguage);
    }

    # region SVG Edit

    public SvgData LoadSvg(int resourceId) {
      var rm = ResourceManager.Instance;
      var re = rm.GetFile(resourceId);
      var res = rm.GetFileData(resourceId, BlobFactory.Container.Resources);
      var objSerializer = new JavaScriptSerializer();
      var resProperties = new SvgData {
        fileName = re.Name,
        svg = Encoding.ASCII.GetString(res),
        exportImageId = re.Metadata.Any(m => m.Key == "img") ? Convert.ToInt32(re.Metadata.Single(m => m.Key == "img").Value) : 0
      };
      return resProperties;
    }

    [DataContract]
    public class SvgData {
      [DataMember]
      public string fileName { get; set; }
      [DataMember]
      public string svg { get; set; }
      [DataMember]
      public int exportImageId { get; set; }
    }

    public dynamic SaveSvg(int id, int imgId, int projectId, string svg, string filename) {
      var rm = ResourceManager.Instance;
      var userName = HttpContext.Current.User.Identity.Name;
      filename = String.IsNullOrEmpty(filename) ? String.Format("Saved {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString()) : filename;
      if (id == 0) {
        // Create new
        id = rm.AddResource(projectId, TypeOfResource.Content, "SVG Drawings", filename, "image/svg+xml", svg, userName);
      } else {
        rm.SaveResource(id, filename, svg, userName);
      }
      // we store this as a back reference to the IMG that was has the saved version
      rm.AddMetaDataToResource(id, "img", imgId);
      var objSerializer = new JavaScriptSerializer();
      var resProperties = new {
        fileName = filename,
        resourceId = id
      };
      return objSerializer.Serialize(resProperties);
    }

    public dynamic SaveImage(int id, int svgId, int projectId, string img, string mimeType, string filename) {
      var rm = ResourceManager.Instance;
      ResourceFile res;
      var userName = HttpContext.Current.User.Identity.Name;
      filename = String.IsNullOrEmpty(filename) ? String.Format("Exported {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString()) : filename;
      var content = Convert.FromBase64String(img.StartsWith(String.Format("data:{0};base64,", mimeType)) ? img.Substring(img.IndexOf("base64,", StringComparison.InvariantCulture) + 7) : img);
      if (id == 0) {
        // Create new
        id = rm.AddResource(projectId, TypeOfResource.Content, "SVG Exports", filename, mimeType, content, userName);
      } else {
        rm.SaveResource(id, filename, content, userName);
      }
      // we store this as a reference to the SVG that was the base for this image
      rm.AddMetaDataToResource(id, "svg", svgId);
      var objSerializer = new JavaScriptSerializer();
      var resProperties = new {
        fileName = filename,
        resourceId = id
      };
      return objSerializer.Serialize(resProperties);
    }


    public dynamic ProjectLibrary(int id, string type) {
      var ribbonImages = EditorManager.Instance.GetRibbonImagesListFromProject(id, 100, 100);
      Func<RibbonImages, bool> clause;
      // we filter the list of images here because the caller must treat SVG different from IMG 
      switch (type) {
        case "img":
          clause = r => r.folder != "SVG Drawings" && r.folder != "SVG Exports";  // strings are consts used in Save methods
          break;
        case "svg":
          clause = r => r.folder == "SVG Drawings";
          break;
        default:
          clause = r => true;
          break;
      }
      var filteredList = ribbonImages.Where(clause).ToList();
      return JsonConvert.SerializeObject(new { filteredList });
    }

    # endregion

  }
}
