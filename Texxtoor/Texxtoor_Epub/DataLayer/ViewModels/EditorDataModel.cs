using System.Collections.Generic;
using Texxtoor.Models;

namespace Texxtoor.Editor.ViewModels {

  /// <summary>
  /// Prepares everything we need to handle chapters in the editor.
  /// </summary>
  public class ChapterDataModel {

    public int DocumentId { get; set; }
    public int GenericChapterNumber { get; set; }
    public Element CurrentChapter { get; set; }

    public string ChapterTitle { get; set; }
    public IEnumerable<SnippetDataModel> ChapterElements { get; set; }
    // the widget container sets this property to render in partial view
    public SnippetDataModel CurrentElement { get; set; }
    public string PreviousChapterTitle { get; set; }
    public Element PreviousChapter { get; set; }

    public string NextChapterTitle { get; set; }
    public Element NextChapter { get; set; }

    // count the elements on the current view dynamically, temporarily, database independent (just to improve viewing)
    private int _figureCount = 1;
    private int _videoCount = 1;
    private int _audioCount = 1;
    private int _tableCount = 1;
    private int _listingCount = 1;

    public int FigureCounter { get { return _figureCount++; } }
    public int VideoCounter { get { return _videoCount++; } }
    public int AudioCounter { get { return _audioCount++; } }
    public int TableCounter { get { return _tableCount++; } }
    public int ListingCounter { get { return _listingCount++; } }

  }

  /// <summary>
  /// Prepares everything we need to handle snippets and sections in the editor.
  /// </summary>
  public class SnippetDataModel {

    public int ChapterId { get; set; }
    public Element CurrentSnippet { get; set; }
    public string SnippetTitle { get; set; }
    public string SectionNumberChain { get; set; }    //2.2 or 4.3.1
    /// <summary>
    /// Precalculated predecessor in the same hierarchy level. Regular Up/Down operations never change hierarchy level.
    /// </summary>
    public Element PrevExchange { get; set; }
    /// <summary>
    /// Precalculated successor in the same hierarchy level. Regular Up/Down operations never change hierarchy level.
    /// </summary>
    public Element NextExchange { get; set; }

    public bool CanDown { get; set; }
    public bool CanUp { get; set; }

  }

}