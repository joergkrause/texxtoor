using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using Texxtoor.Editor.Core;
using Texxtoor.Editor.Core.Extensions;
using Texxtoor.Editor.Utilities;
using Texxtoor.Editor.ViewModels;
using Texxtoor.Models;
using Image = System.Drawing.Image;
using Texxtoor.BaseLibrary.EPub;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.Editor.Models;
using Texxtoor.Editor.Utilities;
using Texxtoor.Editor.ViewModels;
using Texxtoor.Models;
using Texxtoor.Models.Attributes;

namespace Texxtoor.Editor.Controllers
{

    public class EditorController : Controller
    {

        public ActionResult Index()
        {
            var documents = ProjectManager.Instance.GetAllDocuments();
            return View(documents);
        }

        public ActionResult NewDocument()
        {
            var doc = ProjectManager.Instance.AddDocument();
            return RedirectToAction("Authoring", new { id = doc.Id });
        }

        public ActionResult ShowHtml(int id)
        {
            ProjectManager.Instance.ClearDataFolder(System.Web.HttpContext.Current.Server.MapPath("~/data/"));
            var doc = ProjectManager.Instance.GetDocument(id);
            ProjectManager.Instance.CreateImage += InstanceOnCreateImage;
            string path = ProjectManager.Instance.CreateHtml(doc);
            ProjectManager.Instance.CreateImage -= InstanceOnCreateImage;
            return View("ShowHtml", new Pair(doc.Name, path));
        }

        private string InstanceOnCreateImage(object sender, CreateImageArguments e)
        {
            var path = System.Web.HttpContext.Current.Server.MapPath("~/data/");
            using (var file = new FileStream(path + e.FileName, FileMode.Create, FileAccess.ReadWrite))
            {
                file.Write(e.Content, 0, e.Content.Length);
                file.Close();
            }

            // return Path.Combine(path, e.FileName);
            String BaseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            return BaseURL + "data/" + e.FileName;
        }

        public ActionResult ShowEpub(int id)
        {
            var doc = ProjectManager.Instance.GetDocument(id);
            return View(doc);
        }

        public ActionResult DownloadEpub(int id)
        {
            EpubBook book = ProductionManager.Instance.GenerateEpubMedia(id);
            byte[] epub = EBookFactory.SaveBook(book);
            MemoryStream ms = new MemoryStream(epub);
            return File(ms.ToArray(), "application/epub+zip", book.PackageData.MetaData.Title.Text + ".epub");
        }

        public ActionResult ShowPdf(int id)
        {
            String BaseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"] + "data/";

            String ApplicationPath = System.Web.HttpContext.Current.Server.MapPath("~/data/");

            ProjectManager.Instance.ClearDataFolder(System.Web.HttpContext.Current.Server.MapPath("~/data/"));

            var doc = ProjectManager.Instance.GetDocument(id);

            ProjectManager.Instance.CreateImage += InstanceOnCreateImage;
            ProjectManager.Instance.CreateHtml(doc);
            var Htmlpath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/data"), doc.Name + ".html");

            byte[] coverImage = null;
            FileStream fs = new FileStream(System.Web.HttpContext.Current.Server.MapPath("~/Images/") + "Cover.jpg", FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(fs);

            byte[] image = br.ReadBytes((int)fs.Length);

            br.Close();

            fs.Close();
            coverImage = image;
            PdfConvertor.PDFPath = ApplicationPath + Path.GetFileNameWithoutExtension(Htmlpath) + ".pdf";
            PdfConvertor.Instance.GeneratePDF(BaseURL + Path.GetFileNameWithoutExtension(Htmlpath) + ".HTML",
               System.Web.HttpContext.Current.Server.MapPath("~/temp/") + "Cover.html");

            var imgs = new List<Bitmap>();

            var path = System.Web.HttpContext.Current.Server.MapPath("~/data/");
            ProjectManager.Instance.CreateImage += (sender, e) =>
            {
                var ms = new MemoryStream(e.Content);
                var img = Image.FromStream(ms) as Bitmap;
                imgs.Add(img);
                using (var file = new FileStream(path + e.FileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    file.Write(e.Content, 0, e.Content.Length);
                    file.Close();
                }
                return path + e.FileName;
            };
            //string html = ProjectManager.Instance.CreateFinalHtml(doc, true);
            /* var pdf = Create PDF here and store as "ProjectName.pdf in folder /data/ ; */
            return View("ShowPdf", new Pair(doc.Name, "/data/" + doc.Name + ".pdf"));
        }


        #region -= Tree Helper =-
        [HttpPost]
        public JsonResult GetTreeData(int documentId)
        {
            var doc = ProjectManager.Instance.GetDocument(documentId);

            var tree = (new List<Document> { doc }).RecursiveSelect<Element, JsTreeModel>(
              c => c.Children.OrderBy(e => e.OrderNr),
              (e, c) => new JsTreeModel
              {
                  data = e.Name,
                  attr = new JsTreeAttribute
                  {
                      id = e.Id.ToString(),
                      rel = (e is Snippet) ? "snippet" : ((e is Section) ? "section" : "opus")
                  },
                  children = c.ToArray()
              });

            return Json(tree);
        }

        #endregion

        # region -= Editor Helper =-

        public ActionResult AvailableToolSet(int documentId, int? id)
        {
            // take the current snippet Id and show what's available after the current snippet
            var ts = new ToolSetModel();
            // start with an empty editor
            if (!id.HasValue)
            {
                // we can only insert headers first
                ts.CurrentSnippetElements.AddRange(ts.GetAllSnippetElements());
                ts.CurrentInlineElements.Clear();
                ts.CurrentListElements.Clear();
            }
            else
            {
                ts.CurrentInlineElements.AddRange(ts.GetAllInlineElements());
                ts.CurrentListElements.AddRange(ts.GetAllListElements());
                ts.CurrentSnippetElements.AddRange(ts.GetAllSnippetElements());
            }
            ViewBag.DocumentId = documentId;
            return PartialView("_ToolSet", ts);
        }

        public ActionResult UpdateWidgetContainer(int id, int chapterId)
        {
            var rms = HttpContext.Cache[GetAuthorRoomCacheId(id)] as List<ChapterDataModel>;
            ChapterDataModel rm;
            if (rms == null)
            {
                var opus = ProjectManager.Instance.GetDocument(id);
                rm = WidgetHelper.GetChapterModelForEdit(opus, chapterId, null);
            }
            else
            {
                rm = rms.Single(r => r.CurrentChapter.Id == chapterId);
            }
            return PartialView("_WidgetContainer", rm);
        }

        // designed to insert a new snippet quite fast and let the 
        public ActionResult InsertWidget(int id, int chapterId, int snippetId)
        {
            var rms = HttpContext.Cache[GetAuthorRoomCacheId(id)] as List<ChapterDataModel>;
            ChapterDataModel rm;
            if (rms == null)
            {
                var opus = ProjectManager.Instance.GetDocument(id);
                rm = WidgetHelper.GetChapterModelForEdit(opus, chapterId, null);
            }
            else
            {
                rm = rms.Single(r => r.CurrentChapter.Id == chapterId);
            }
            rm.CurrentElement = rm.ChapterElements.FirstOrDefault(e => e.CurrentSnippet.Id == snippetId);
            return PartialView("_SingleWidget", rm);
        }

        public ActionResult GetWidget(int id, int chapterId, int snippetId)
        {
            var rms = HttpContext.Cache[GetAuthorRoomCacheId(id)] as List<ChapterDataModel>;
            ChapterDataModel rm;
            if (rms == null)
            {
                var opus = ProjectManager.Instance.GetDocument(id);
                rm = WidgetHelper.GetChapterModelForEdit(opus, chapterId, null);
            }
            else
            {
                rm = rms.Single(r => r.CurrentChapter.Id == chapterId);
            }
            rm.CurrentElement = rm.ChapterElements.FirstOrDefault(e => e.CurrentSnippet.Id == snippetId);
            return PartialView("_SingleWidget", rm);
        }

        public ActionResult LoadContent(int id, int chapterId)
        {
            var rms = HttpContext.Cache[GetAuthorRoomCacheId(id)] as List<ChapterDataModel>;
            ChapterDataModel rm;
            if (rms == null)
            {
                var opus = ProjectManager.Instance.GetDocument(id);
                rm = WidgetHelper.GetChapterModelForEdit(opus, chapterId, null);
            }
            else
            {
                rm = rms.Single(r => r.CurrentChapter.Id == chapterId);
            }
            return PartialView("_WidgetContainer", rm);
        }

        public JsonResult GetContentStructure(int id, int chapterId)
        {
            return Json(GetSnippetsData(id, chapterId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SearchSnippetId(int id, int chapterId, int snippetId, string value, int direction)
        {
            var opus = ProjectManager.Instance.GetDocument(id);
            var rm = WidgetHelper.GetChapterModelForEdit(opus, chapterId, null);
            if (direction < -1) direction = -1;
            if (direction > 1) direction = 1;
            if (direction == 0) direction = 1;

            return Json(new { Id = GetSnippetId(rm.ChapterElements, snippetId, direction, value) }, JsonRequestBehavior.AllowGet);
        }
        public int GetSnippetId(IEnumerable<SnippetDataModel> snippets, int snippetId, int direction, string value, int sd = 0)
        {
            var id = -1;
            IEnumerable<SnippetDataModel> result = null;
            switch (direction)
            {
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

        /// <summary>
        /// Check whether a specific insert operation is available, check available tools.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="chapterId"> </param>
        /// <param name="snippetId"> </param>
        /// <returns></returns>    
        [HttpGet]
        public JsonResult UpdateWidgetTools(int id, int chapterId, int snippetId)
        {
            try
            {
                var sn = EditorManager.Instance.GetElement(snippetId);
                var opus = ProjectManager.Instance.GetDocument(id);
                var deep = ((sn is Section) ? ((Section)sn).Deep : ((Section)EditorManager.Instance.GetSection(sn)).Deep);
                var rm = WidgetHelper.GetChapterModelForEdit(opus, chapterId, (s) => EditorManager.Instance.AddElement(s));
                rm.CurrentElement = rm.ChapterElements.First(e => e.CurrentSnippet.Id == snippetId);
                var data = new
                {
                    S1 = (deep == 2 || deep == 1),
                    S2 = (deep == 3 || deep == 2),
                    S3 = (deep == 4 || deep == 3),
                    S4 = (deep == 5 || deep == 4),
                    Increase = rm.CurrentElement.CanUp,
                    Decrease = rm.CurrentElement.CanDown,
                    Up = rm.CurrentElement.PrevExchange != null,
                    Down = rm.CurrentElement.NextExchange != null,
                    Bold = !(sn is Section),
                    Italic = !(sn is Section),
                    Sub = !(sn is Section),
                    Sup = !(sn is Section),
                    Underline = !(sn is Section),
                    Ul = !(sn is Section),
                    Ol = !(sn is Section),
                    Code = true,
                    P = true,
                    Aside = true
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Images are created by using a call to GetImage, such as <img src="AuthorPortal/Opus/GetImage/1" />
        public ActionResult GetImage(int id, bool? convertImage)
        {
            var img = EditorManager.Instance.GetElement<ImageSnippet>(id);
            // with the img we get the id and can pull the real data from blob cache
            var properties = System.Web.Helpers.Json.Decode<ImageProperties>(img.Properties);
            if (convertImage.HasValue && !convertImage.Value)
                return File(img.Content, "image/png");
            var image = ImageUtil.ApplyImageProperties(img.Content, properties);
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                return File(ms.ToArray(), "image/png");
            }
        }
        public ActionResult ImageColors()
        {
            return PartialView("Widgets/Tools/_ImageColors", new ImageEffects().Effects);
        }

        private string GetAuthorRoomCacheId(int key)
        {
            return User.Identity.Name + "-" + key;
        }

        public ActionResult Authoring(int id, int? chapterId)
        {
            var opus = ProjectManager.Instance.GetDocument(id);
            // build all chapter, assume that first level are chapters
            var rms = opus.Children.Select(chapter => WidgetHelper.GetChapterModelForEdit(opus, chapter.Id, (s) => EditorManager.Instance.AddElement(s))).ToList();
            var rm = chapterId.HasValue ? rms.First(r => r.CurrentChapter.Id == chapterId.Value) : rms.FirstOrDefault();
            HttpContext.Cache.Remove(GetAuthorRoomCacheId(id));
            HttpContext.Cache.Add(GetAuthorRoomCacheId(id), rms, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            // regular data retrieval
            ViewBag.WorkspaceName = opus.Name;
            ViewBag.CreatedAtDate = String.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
            ViewBag.BaseUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, "/Editor/");
            return View(rm);
        }

        public JsonResult Move(int id, int? chapterId, int? sectionId, string move)
        {
            if (!String.IsNullOrEmpty(move) && new[] { "d", "u", "r", "l" }.Contains(move))
            {
                HandleMoves(move, sectionId);
                var rms = HttpContext.Cache[GetAuthorRoomCacheId(id)] as List<ChapterDataModel>;
                var opus = ProjectManager.Instance.GetDocument(id);
                rms.Remove(rms.Single(r => r.CurrentChapter.Id == chapterId));
                rms.Add(WidgetHelper.GetChapterModelForEdit(opus, chapterId, null));
                HttpContext.Cache[GetAuthorRoomCacheId(id)] = rms;
                return Json(GetSnippetsData(id, chapterId.Value), JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public object GetSnippetsData(int id, int chapterId)
        {
            var opus = ProjectManager.Instance.GetDocument(id);
            ChapterDataModel rm = WidgetHelper.GetChapterModelForEdit(opus, chapterId, null);
            return rm.ChapterElements.Select(x => new { Id = x.CurrentSnippet.Id, Length = x.CurrentSnippet.RawContent.Length });
        }

        public ActionResult SemanticList(int id, string type)
        {
            var sl = ProjectManager.Instance.GetSemanticListForDocument(id, type);
            ViewBag.Type = type;
            switch (type)
            {
                case "abbreviation":
                    ViewBag.TargetElement = "abbr";
                    break;
                case "cite":
                    ViewBag.TargetElement = "cite";
                    break;
                case "definition":
                    ViewBag.TargetElement = "def";
                    break;
                case "idiom":
                    ViewBag.TargetElement = "i";
                    break;
                case "variable":
                    ViewBag.TargetElement = "var";
                    break;
                case "link":
                    ViewBag.TargetElement = "a";
                    break;
            }
            return PartialView("Widgets/Tools/_SemanticList", sl);
        }

        private void HandleMoves(string move, int? sectionId)
        {
            // if the call delivers a section and move operation we make this first
            if (sectionId.HasValue && !String.IsNullOrEmpty(move))
            {
                // moving is always an exchange operation, two element simply change their order        
                var sn = EditorManager.Instance.GetElement(sectionId.Value);
                switch (move)
                {
                    case "u":
                        // move up within parent section
                        EditorManager.Instance.MoveElementUp(sn);
                        break;
                    case "d":
                        // move down within parent section
                        EditorManager.Instance.MoveElementDown(sn);
                        break;
                    case "r":
                        EditorManager.Instance.DecreaseSectionLevel(sn);
                        break;
                    case "l":
                        EditorManager.Instance.IncreaseSectionLevel(sn);
                        break;
                }
            }
        }

        public ActionResult Toc(int id)
        {
            var opus = ProjectManager.Instance.GetDocument(id);
            return PartialView("Panes/_Toc", opus);
        }

        /// <summary>
        /// Insert new snippet at the end of the current element's parent section.
        /// </summary>
        /// <param name="opusId"> </param>
        /// <param name="id">Current Snippet Id</param>
        /// <param name="chapterId">Chapter</param>
        /// <param name="type">type</param>
        /// <param name="variation"> </param>
        /// <param name="data"> </param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertSnippet(int documentId, int? id, int chapterId, string type, string variation, string data)
        {
            Element sn;
            int redirectId = -1;
            if (id.HasValue)
            {
                sn = EditorManager.Instance.GetElement(id.Value);
            }
            else
            {
                // an empty book starts with just one chapter, so we take the first element as the current
                sn = EditorManager.Instance.GetElement(chapterId);
            }
            Element ns = null;
            Element parent = null;
            int newOrderNr = 1;
            switch (type)
            {
                case "chapter":
                    ns = new Section { Name = "Chapter", Content = "New Chapter".GetBytes() };
                    var prevChapter = EditorManager.Instance.GetParentChapter(sn);
                    parent = prevChapter.Parent;
                    if (parent is Document)
                    {
                        ns.Parent = parent;
                        ns.Name = "Inserted " + DateTime.Now.ToShortTimeString();
                        ns.OrderNr = parent.Children.Max(x => x.OrderNr) + 1;
                        EditorManager.Instance.ReorderLeafElementsAfterOrderNr(ns.OrderNr, parent.Id);
                    }
                    break;
                case "section":
                    // section is always on the same level as the current selection (if we're on 1.2, this inserts 1.3)
                    ns = new Section { Name = "Section", Content = "Enter text".GetBytes() };
                    // if current element (sn) is not a section than take the next parent 
                    parent = EditorManager.Instance.GetSection(sn);
                    ns.Parent = parent;
                    if (sn is Section && ((Section)sn).Deep == 2)
                    {
                        ns.Parent = parent.Parent;
                        newOrderNr = parent.OrderNr + 1;
                    }
                    else if (sn is Section && ((Section)sn).Deep == 3)
                    {
                        ns.Parent = parent.Parent.Parent;
                        newOrderNr = parent.Parent.OrderNr + 1;
                    }
                    else if (sn is Section && ((Section)sn).Deep == 4)
                    {
                        ns.Parent = parent.Parent.Parent.Parent;
                        newOrderNr = parent.Parent.Parent.OrderNr + 1;
                    }
                    else if (sn is Section && ((Section)sn).Deep == 5)
                    {
                        ns.Parent = parent.Parent.Parent.Parent.Parent;
                        newOrderNr = parent.Parent.Parent.Parent.OrderNr + 1;
                    }
                    EditorManager.Instance.ReorderLeafElementsAfterElement(ns, newOrderNr);
                    break;
                case "subsection":
                    // subsection is always on level deeper as the current selection (if we're on 1.2, and the last subsection is 1.2.4 this inserts 1.2.5)
                    ns = new Section { Name = "SubSection", Content = "Level 2 Section".GetBytes() };
                    parent = EditorManager.Instance.GetSection(sn);
                    ns.Parent = parent;
                    if (sn is Section && ((Section)sn).Deep == 3)
                    {
                        ns.Parent = parent.Parent;
                        newOrderNr = sn.OrderNr + 1;
                    }
                    EditorManager.Instance.ReorderLeafElementsAfterElement(ns, newOrderNr);
                    break;
                case "subsubsection":
                    // subsection is always on level deeper as the current selection (if we're on 1.2, and the last subsection is 1.2.4 this inserts 1.2.5)
                    ns = new Section { Name = "SubSubSection", Content = "Level 3 Section".GetBytes() };
                    parent = EditorManager.Instance.GetSection(sn);
                    ns.Parent = parent;
                    if (sn is Section && ((Section)sn).Deep == 4)
                    {
                        ns.Parent = parent.Parent;
                        newOrderNr = sn.OrderNr + 1;
                    }
                    EditorManager.Instance.ReorderLeafElementsAfterElement(ns, newOrderNr);
                    break;
                case "subsubsubsection":
                    // subsection is always on level deeper as the current selection (if we're on 1.2, and the last subsection is 1.2.4 this inserts 1.2.5)
                    ns = new Section { Name = "SubSubSubSection", Content = "Level 4 Section".GetBytes() };
                    parent = EditorManager.Instance.GetSection(sn);
                    ns.Parent = parent;
                    if (sn is Section && ((Section)sn).Deep == 5)
                    {
                        ns.Parent = parent.Parent;
                        newOrderNr = sn.OrderNr + 1;
                    }
                    EditorManager.Instance.ReorderLeafElementsAfterElement(ns, newOrderNr);
                    break;
                case "text":
                    ns = EditorManager.Instance.CreateTextSnippet(sn);
                    break;
                case "img":
                    # region Image
                    var resId = Convert.ToInt32(data); // resId is from resources, we need to pull this from blob storage first and save to element
                    // 1. Retrieve Res, 2. read blob, 3. put in content (hard copy of everything)
                    var res = ProjectManager.Instance.GetResource(resId);
                    ns = EditorManager.Instance.CreateImageSnippet(sn, res);
                    # endregion
                    break;
                case "table":
                    # region Table
                    string rows = "", cols = "";
                    if (!String.IsNullOrEmpty(data) && data.Contains(","))
                    {
                        rows = data.Split(",".ToCharArray())[0];
                        cols = data.Split(",".ToCharArray())[1];
                    }
                    if (String.IsNullOrEmpty(variation))
                    {
                        variation = "simple";
                    }
                    ns = new TableSnippet
                    {
                        Name = "Table " + variation,
                        Rows = UInt32.Parse(rows),
                        Cols = UInt32.Parse(cols),
                        TableType = variation,
                        TableStyle = new Style() { Width = Unit.Percentage(100D) },
                        RowStyle = new Style() { BackColor = Color.White },
                        CellStyle = new Style() { ForeColor = Color.Black },
                        HeadRowStyle = new Style() { ForeColor = Color.Black },
                        HeadCellStyle = new Style() { ForeColor = Color.Black },
                        LeadCellStyle = new Style() { ForeColor = Color.Black, BackColor = Color.FromArgb(79, 129, 189) },
                        OrderNr = newOrderNr + 1
                    };
                    ((TableSnippet)ns).GenerateTable();
                    ns.Parent = EditorManager.Instance.GetSection(sn);
                    EditorManager.Instance.ReorderLeafElementsAfterElement(ns, newOrderNr + 1);
                    # endregion
                    break;
                case "listing":
                    ns = EditorManager.Instance.CreateListingSnippet(sn);
                    break;
                case "sidebar":
                    ns = EditorManager.Instance.CreateSidebarSnippet(sn);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(type + " is an unknown command");
            }
            // save
            EditorManager.Instance.InsertElement(ns, true);
            redirectId = (type == "chapter" ? ns.Id : -1);
            // refresh cache
            var rms = HttpContext.Cache[GetAuthorRoomCacheId(documentId)] as List<ChapterDataModel>;
            var opus = ProjectManager.Instance.GetDocument(documentId);
            if (type == "chapter")
            {
                rms.Add(WidgetHelper.GetChapterModelForEdit(opus, ns.Id, null));
            }
            else
            {
                rms.Remove(rms.Single(r => r.CurrentChapter.Id == chapterId));
                rms.Add(WidgetHelper.GetChapterModelForEdit(opus, chapterId, null));
            }
            HttpContext.Cache[GetAuthorRoomCacheId(documentId)] = rms;
            return new JsonResult { Data = new { id = ns.Id, msg = ns.GetType().Name, relocateTo = redirectId, snippetsData = GetSnippetsData(documentId, chapterId) } };
        }

        public ActionResult InternalLink(int id)
        {
            var opus = ProjectManager.Instance.GetDocument(id);
            return PartialView("Widgets/Tools/_InternalLink", opus.Children);
        }

        private void FlatElements(ICollection<Element> elements, IEnumerable<Element> children)
        {
            foreach (var item in children)
            {
                elements.Add(item);
                if (item.HasChildren())
                {
                    FlatElements(elements, item.Children);
                }
            }
        }

        public ActionResult Images(int id)
        {
            var opus = ProjectManager.Instance.GetDocument(id);
            ViewBag.ProjectId = opus.Id;
            var res = ProjectManager.Instance.GetResourceFiles(opus, TypeOfResource.Content, "image");
            return PartialView("Panes/_Images", res);
        }

        public ActionResult Tables(int id)
        {
            return PartialView("Panes/_Tables");
        }

        public ActionResult Equations(int id)
        {
            return PartialView("Panes/_Equations");
        }

        public ActionResult GetThumbnail(int id, int w, int h)
        {
            var res = ProjectManager.Instance.GetResource(id);
            if (res != null)
            {
                return File(WidgetHelper.GetThumbnailImage(res.Content, w, h), "image/png");
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public JsonResult DeleteSnippet(int? opusId, int? chapterId, int id, bool? delChildren)
        {
            try
            {
                var sn = EditorManager.Instance.GetElement(id);
                var refreshId = id;
                var parentId = sn.Parent.Id;
                if (sn is Section)
                {
                    if (delChildren.HasValue && delChildren.Value)
                    {
                        // all children might be removed recursively
                        EditorManager.Instance.DeleteChildrenResursively(sn.Children);
                    }
                    else
                    {
                        // all child elements become a new parent, if the removed section is the last we move the level up, otherwise we attach the element to the previous
                        EditorManager.Instance.RelocateChildElementsToParent(sn);
                    }
                }
                var children = new List<int>();
                if (sn.HasChildren())
                {
                    GetChildrenId(ref children, sn);
                }
                EditorManager.Instance.DeleteElement(sn);
                // reorder
                var downList = EditorManager.Instance.GetElementsForParent(parentId);

                EditorManager.Instance.ReorderLeafElementsAll(downList, true);
                if (sn.Parent is Document)
                {
                    // assume the element being deleted is a chapter, hence we need to relocate the whole editor to another chapter
                    var newChapter = sn.Parent.Children.OfType<Section>().FirstOrDefault();
                    if (newChapter != null)
                    {
                        refreshId = newChapter.Id;
                    }
                    else
                    {
                        // TODO: if we come here the user has removed the very last chapter and we need to create at least one defaul chapter an move there
                        refreshId = -1;
                    }
                }
                return new JsonResult { Data = new { id = id, msg = "Snippet removed and reordered", relocateTo = refreshId, children = children, snippetsData = GetSnippetsData(opusId.Value, chapterId.Value) } };
            }
            catch (Exception)
            {
                return new JsonResult();
            }
        }
        public void GetChildrenId(ref List<int> list, Element el)
        {
            foreach (var item in el.Children)
            {
                list.Add(item.Id);
                if (item.HasChildren())
                {
                    GetChildrenId(ref list, item);
                }
            }
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult SaveContent(int id, int documentId, string content, string[] form)
        {
            try
            {
                // some additional fields either from toolset or from might be added
                var frm = System.Web.HttpUtility.ParseQueryString(form[0]);
                bool refreshSection = EditorManager.Instance.SaveContent(id, frm, content);

                // refresh cache
                var rms = HttpContext.Cache[GetAuthorRoomCacheId(documentId)] as List<ChapterDataModel>;
                var doc = ProjectManager.Instance.GetDocument(documentId);
                var chapter = EditorManager.Instance.GetParentChapter(EditorManager.Instance.GetElement(id));
                rms.Remove(rms.Single(r => r.CurrentChapter.Id == chapter.Id));
                rms.Add(WidgetHelper.GetChapterModelForEdit(doc, chapter.Id, null));
                HttpContext.Cache[GetAuthorRoomCacheId(documentId)] = rms;

                return Json(new { msg = String.Format("Successfully saved !"), sectionRefresh = (refreshSection ? content : null) });
            }
            catch (Exception ex)
            {
                return Json(new { msg = String.Format("Error ! {0}", ex.Message) });
            }
        }

        private void SaveMetaData(NameValueCollection frm, Element snippet)
        {
            // metadata-@ts.Key.ToLower()-@ViewBag.SnippetId
            var metadata = frm.AllKeys.Where(key => key.StartsWith("metadata") && key.EndsWith(snippet.Id.ToString()));


        }

        # endregion
    }
}
