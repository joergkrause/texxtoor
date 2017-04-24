using System;
using System.Collections.Generic;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.ViewModels.Editor {

  /// <summary>
  /// Prepares everything we need to handle chapters in the editor.
  /// </summary>
  /// <summary>
  /// Prepares everything we need to handle chapters in the editor.
  /// </summary>
  public class
    ChapterDataModel {

    public int DocumentId { get; set; }
    public int ProjectId { get; set; }
    public string GenericChapterNumber { get; set; }
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


}