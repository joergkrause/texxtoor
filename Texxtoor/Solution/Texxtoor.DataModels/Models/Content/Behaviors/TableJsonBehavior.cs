
using System.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Attributes {

  /// <summary>
  /// A Text wrapper that creates the smallest possible JSON for each snippet
  /// </summary>
  [EditorServiceWrapper(typeof(TableSnippet))]
  internal class TableJsonBehavior : TitledJsonBehavior {
    #region Custom Properties

    // ReSharper disable InconsistentNaming
    public string content { get; set; }
    public string defaultContent { get; set; }
    public string tableLocalization { get; set; }
    public uint rows { get; set; }
    public uint columns { get; set; }
    public bool repeatHeadRow { get; set; }
    public int tableNumber { get; set; }
    // ReSharper restore InconsistentNaming

    #endregion

    #region Override, Properties

    #endregion

    #region Overriden Methods

    public override JsonBehavior GetJson(Opus doc, Snippet currentChapter, Snippet i) {
      var snippet = i as TableSnippet;

      return new TableJsonBehavior {
        documentId = doc.Id,
        chapterId = currentChapter.Id,
        snippetId = snippet.Id,
        content = snippet.RawContent,
        genericChapterNumber = doc.ShowNumberChain ? currentChapter.OrderNr.ToString() : "#",
        tableLocalization = "Table",
        tableNumber = 0,
        rows = snippet.Rows,
        columns = snippet.Cols,
        defaultContent = snippet.DefaultContent,
        repeatHeadRow = snippet.RepeatHeadRow,
        title = snippet.Title,
        widgetName = snippet.WidgetName,
        levelId = snippet.Level,
        parentId = snippet.Parent.Id,
        orderNr = snippet.OrderNr,
        snippetCounter = currentChapter.Children.FlattenHierarchy().OfType<TableSnippet>().ToList().IndexOf(snippet) + 1,
        isReadOnly = snippet.ReadOnly
      };
    }

    #endregion
  }

}
