using System.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Attributes {
   
    /// <summary>
    /// A Text wrapper that creates the smallest possible JSON for each snippet
    /// </summary>
    [EditorServiceWrapper(typeof(ListingSnippet))]
    internal class ListingJsonBehavior : TitledJsonBehavior
    {
        #region Custom Properties

// ReSharper disable InconsistentNaming
        public string content { get; set; }
        public bool lineNumbers { get; set; }
        public bool syntaxHighlight { get; set; }
        public string listingLocalization { get; set; }
        public string language { get; set; }
// ReSharper restore InconsistentNaming

        #endregion

        #region Override, Properties

        #endregion

        #region Overriden Methods

        public override JsonBehavior GetJson(Opus doc, Snippet currentChapter, Snippet i)
        {
            var snippet = i as ListingSnippet;

            return new ListingJsonBehavior
            {
                documentId = doc.Id,
                chapterId = currentChapter.Id,
                snippetId = snippet.Id,
                listingLocalization = "Listing",
                syntaxHighlight = snippet.SyntaxHighlight,
                genericChapterNumber = doc.ShowNumberChain ? currentChapter.OrderNr.ToString() : "#",
                lineNumbers = snippet.LineNumbers,
                content = snippet.RawContent,
                widgetName = snippet.WidgetName,
                levelId = snippet.Level,
                parentId = snippet.Parent.Id,
                orderNr = snippet.OrderNr,
                snippetCounter = currentChapter.Children.FlattenHierarchy().OfType<ListingSnippet>().ToList().IndexOf(snippet) + 1,
                language = snippet.Language,
                isReadOnly = snippet.ReadOnly,
                title = snippet.Title
            };
        }

        #endregion
    }


}
