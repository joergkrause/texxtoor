using System.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.DataModels.Attributes
{
  
    /// <summary>
    /// A Image wrapper that creates the smallest possible JSON for each snippet
    /// </summary>
    [EditorServiceWrapper(typeof(ImageSnippet))]
    internal class ImageJsonBehavior : TitledJsonBehavior
    {
        #region Custom Properties

// ReSharper disable InconsistentNaming
        public string imageUrl { get; set; }
        public string imageLocalization { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int originalwidth { get; set; }
        public int originalheight { get; set; }
        public string content { get; set; }
        public string properties { get; set; }

        public string mimetype { get; set; }
// ReSharper restore InconsistentNaming

        #endregion

        #region Override, Properties

        #endregion

        #region Overriden Methods

        public override JsonBehavior GetJson(Opus doc, Snippet currentChapter, Snippet i)
        {
            var snippet = i as ImageSnippet;

            return new ImageJsonBehavior
            {
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
                genericChapterNumber = doc.ShowNumberChain ? currentChapter.OrderNr.ToString() : "#",
                snippetCounter = currentChapter.Children.FlattenHierarchy().OfType<ImageSnippet>().ToList().IndexOf(snippet) + 1,
                levelId = snippet.Level,
                parentId = snippet.Parent.Id,
                isReadOnly = snippet.ReadOnly,
                originalheight = snippet.ImageProperties.OriginalHeight,
                originalwidth = snippet.ImageProperties.OriginalWidth,
                mimetype = snippet.MimeType,
                properties = snippet.Properties
            };
        }

        #endregion
    }


}
