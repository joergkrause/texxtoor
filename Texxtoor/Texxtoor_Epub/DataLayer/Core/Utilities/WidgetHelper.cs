using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using Texxtoor.Editor.Core.Extensions;
using Texxtoor.Editor.ViewModels;
using Texxtoor.Models;

namespace Texxtoor.Editor.Utilities {
  public static class WidgetHelper {

    public static ChapterDataModel GetChapterModelForEdit(Document opus, int? chapterId, Action<Element> saveNewChapter) {     
      // we read chapter by chapter, which is the first level in the hierarchy by definition
      var chapters = opus.Children.OfType<Section>().OrderBy(c => c.OrderNr).ToList();
      Element currentChapter = null;
      Element prevChapter = null;
      Element nextChapter = null;
      IEnumerable<SnippetDataModel> run = null;
      if (chapterId.HasValue && chapters.Any()) {
          currentChapter = chapters.SingleOrDefault(c => c.Id == chapterId);
          if (currentChapter == null) // If chapter was deleted.
              currentChapter = chapters.Last();
      }
      if (!chapterId.HasValue && chapters.Any()) {
        currentChapter = chapters.First();
      }
      if (currentChapter != null) {
        // take tha chapters content for view
        run = FlattenHierarchy(currentChapter);
        // take prev and next from index
        var idx = chapters.FindIndex(c => c.Id == currentChapter.Id);
        prevChapter = chapters.ElementAtOrDefault(idx - 1);
        nextChapter = chapters.ElementAtOrDefault(idx + 1);
      } else {
        // Create the first chapter on the fly and add to the DB immediately
        currentChapter = new Section { Parent = opus, Name = "Chapter 1", OrderNr = 1, Content = System.Text.Encoding.UTF8.GetBytes("Chapter 1") };
        saveNewChapter(currentChapter);
        run = new List<SnippetDataModel>(new[] { new SnippetDataModel { 
          CurrentSnippet = currentChapter, 
          ChapterId = currentChapter.Id, 
          SnippetTitle = currentChapter.Name,
          CanDown = false,
          CanUp = false
        } });
      }
      var rm = new ChapterDataModel {
        DocumentId = opus.Id,
        GenericChapterNumber = currentChapter.OrderNr,
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

    public static IEnumerable<SnippetDataModel> FlattenHierarchy(Element chapter) {
      var snippetList = new List<SnippetDataModel>();
      Func<List<Element>, List<SnippetDataModel>> helper = null;
      helper = nodes => {
        var ret = new List<SnippetDataModel>();
        foreach (var node in nodes) {
          int idx = nodes.FindIndex(n => n == node);
          ret.Add(new SnippetDataModel {
            ChapterId = chapter.Id,
            CurrentSnippet = node,
            SnippetTitle = node.Name,
            PrevExchange = nodes.ElementAtOrDefault(idx - 1),
            NextExchange = nodes.ElementAtOrDefault(idx + 1),
            SectionNumberChain = GetSectionNumber(node, chapter.Id),
            CanUp = node.Parent.Id != chapter.Id,                       // can go up always if not yet at chapter level
            CanDown = node.Parent.Children.OfType<Section>().OrderBy(se => se.OrderNr).Cast<Element>().ToList().IndexOf(node) > 0   // can go down if second in stack (can't be first, make 1. / 1.1 ==> 1. / 1.1.1 is not allowed)
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

    public static string GetSectionNumber(Element e, int stopAtId) {
      // we calculate sections only
      var s = e as Section;
      if (s == null) return null;
      // first, get the position in the stack of sections (ignoring other widget types)
      var res = new List<string>();
      do {
        var stack = e.Parent.Children.OfType<Section>().OrderBy(se => se.OrderNr).Cast<Element>().ToList();
        var pos = stack.IndexOf(e);
        res.Add((pos + 1).ToString());
        e = e.Parent;
      } while (e.Parent != null && e.Id != stopAtId);
      res.Reverse();
      return String.Concat(".", String.Join(".", res.ToArray()));
    }

    public static byte[] GetThumbnailImage(Stream image, int scaleToW, int scaleToH) {
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

    public static byte[] GetThumbnailImage(byte[] image, int scaleToW, int scaleToH) {
      if (image == null) return null;
      using (var ms = new MemoryStream(image)) {
        return GetThumbnailImage(ms, scaleToW, scaleToH);
      }
    }

    public static byte[] GetStaticImage(string fullPath) {
      using (var ms = new MemoryStream()) {
        var img = Image.FromFile(fullPath);
        img.Save(ms, ImageFormat.Png);
        var buffer = ms.ToArray();
        return buffer;
      }
    }

  }
}