using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Attributes {

  /// <summary>
  /// A Image wrapper that creates the smallest possible JSON for each snippet
  /// </summary>
  internal class TitledJsonBehavior : JsonBehavior {
    #region Custom Properties

    // ReSharper disable InconsistentNaming
    public string genericChapterNumber { get; set; }
    public int snippetCounter { get; set; }
    public string title { get; set; }
    // ReSharper restore InconsistentNaming
    #endregion

    #region Override, Properties

    #endregion

    #region Overriden Mthods

    public override JsonBehavior GetJson(Opus doc, Snippet currentChapter, Snippet i) {
      var snippet = i as ImageSnippet;
      //var rawContent = String.Format("<div class=\"img\" style='width:{1};'><img src='{0}?{2}' /></div>", Url.Action("GetImage", new { id = Model.Id }),'+
      //    '(String.IsNullOrEmpty(Model.Properties) ? 300 : Json.Decode<ImageProperties>(Model.Properties).ImageWidth), '+
      //    '(String.IsNullOrEmpty(Model.Properties) ? 300 : Json.Decode<ImageProperties>(Model.Properties).ImageHeight),'+
      //    'DateTime.Now.Ticks);
      return new ImageJsonBehavior {
        documentId = doc.Id,
        chapterId = currentChapter.Id,
        snippetId = snippet.Id,
        width = snippet.Width,
        height = snippet.Height,
        imageUrl = snippet.ItemHref,
        title = snippet.Title,
        widgetName = snippet.WidgetName,
        imageLocalization = "Figure",
        orderNr = snippet.OrderNr,
        levelId = snippet.Level,
        parentId = snippet.Parent.Id,
        isReadOnly = snippet.ReadOnly,
        originalheight = snippet.ImageProperties.OriginalHeight,
        originalwidth = snippet.ImageProperties.OriginalWidth,
        properties = snippet.Properties
      };
    }

    #endregion
  }

}
