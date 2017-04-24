using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Attributes {
  /// <summary>
    /// A Text wrapper that creates the smallest possible JSON for each snippet
    /// </summary>
    [EditorServiceWrapper(typeof(SidebarSnippet))]
    internal class SidebarJsonBehavior : JsonBehavior
    {
        #region Custom Properties

        public string content { get; set; }
        public string asideContent { get; set; }
        public string headerContent { get; set; }
        public int sidebarType { get; set; }
        public bool isEditableContent { get; set; }

        #endregion

        #region Override, Properties

        #endregion

        #region Overriden Methods

        public override JsonBehavior GetJson(Opus doc, Snippet currentChapter, Snippet i)
        {
            var snippet = i as SidebarSnippet;

            return new SidebarJsonBehavior
            {
                documentId = doc.Id,
                chapterId = currentChapter.Id,
                snippetId = snippet.Id,
                content = snippet.RawContent,
                isEditableContent = (!snippet.ReadOnly && snippet.SidebarType == SidebarType.Custom),
                sidebarType = (int)snippet.SidebarType,
                asideContent = snippet.AsideContent,
                headerContent = snippet.HeaderContent,
                widgetName = snippet.WidgetName,
                levelId = snippet.Level,
                parentId = snippet.Parent.Id,
                orderNr = snippet.OrderNr,
                isReadOnly = snippet.ReadOnly
            };
        }

        #endregion
    }
}
