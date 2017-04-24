using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Attributes {
    
    /// <summary>
    /// A Text wrapper that creates the smallest possible JSON for each snippet
    /// </summary>
    [EditorServiceWrapper(typeof(TextSnippet))]
    internal class TextJsonBehavior : JsonBehavior
    {
        #region Custom Properties

        public string content { get; set; }

        #endregion

        #region Override, Properties

        #endregion

        #region Overriden Methods

        public override JsonBehavior GetJson(Opus doc, Snippet currentChapter, Snippet i)
        {
            var snippet = i as TextSnippet;

            return new TextJsonBehavior
            {
                documentId = doc.Id,
                chapterId = currentChapter.Id,
                snippetId = snippet.Id,
                content = snippet.RawContent,
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
