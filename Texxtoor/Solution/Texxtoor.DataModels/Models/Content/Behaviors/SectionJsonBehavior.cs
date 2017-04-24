using System.Linq;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Attributes {

  [EditorServiceWrapper(typeof(Section))]
  internal class SectionJsonBehavior : JsonBehavior {
    #region Custom Properties

    public string sectionNumberChain { get; set; }
    public string genericChapterNumber { get; set; }
    public string content { get; set; }
    public int childCount { get; set; }
    public bool hasChildren { get; set; }

    #endregion

    #region Override, Properties

    #endregion

    /// <summary>
    ///  This method will return a section snippet, with all desired attributes
    /// </summary>
    /// <param name="documentId">Current document id</param>
    /// <param name="chapterId">Current chapter id</param>
    /// <param name="i">This variable respresent to the section</param>
    /// <returns>Section json attributes</returns>
    public JsonBehavior GetJson(Opus doc, Snippet currentChapter, Section currentElement, string numberChain) {
      var sectionNumberChain = string.Empty;
      if (currentElement.Parent.Id != doc.Id) {
        sectionNumberChain = numberChain;
      }
      return new SectionJsonBehavior {
        documentId = doc.Id,
        chapterId = currentChapter.Id,
        snippetId = currentElement.Id,
        content = currentElement.RawContent,
        widgetName = currentElement.WidgetName,
        sectionNumberChain = sectionNumberChain,
        genericChapterNumber = doc.ShowNumberChain ? currentChapter.OrderNr.ToString() : "#",
        levelId = currentElement.Level,
        parentId = currentElement.Parent.Id,
        orderNr = currentElement.OrderNr,
        childCount = (currentElement.HasChildren() ? currentElement.Children.Count() : 0),
        hasChildren = currentElement.HasChildren(),
        isReadOnly = currentElement.ReadOnly
      };
    }
  }
}
