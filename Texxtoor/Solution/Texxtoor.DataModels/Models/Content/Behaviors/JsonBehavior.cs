using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Attributes {

  public abstract class JsonBehavior {
    #region Abstract Properties

// ReSharper disable InconsistentNaming
    public int documentId { get; set; }
    public int chapterId { get; set; }
    public int snippetId { get; set; }
    public string widgetName { get; set; }
    public int levelId { get; set; }
    public int parentId { get; set; }
    public int orderNr { get; set; }
    public bool isReadOnly { get; set; }
// ReSharper restore InconsistentNaming

    #endregion Abstract Properties

    #region Virtual Methods

    public virtual JsonBehavior GetJson(Opus doc, Snippet currentChapter, Snippet i) { return null; }

    #endregion
  }
}
