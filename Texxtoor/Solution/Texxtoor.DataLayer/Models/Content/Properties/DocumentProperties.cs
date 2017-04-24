using System;

namespace Texxtoor.DataModels.Properties {

  /// <summary>
  /// Properties mostly to store settings for the editor on a per document base.
  /// </summary>
  public class DocumentProperties {

    public bool ShowNumberChain { get; set; }
    public int LastChapterId { get; set; }
    public int LastSnippetId { get; set; }
    public bool AllowMetaData { get; set; }
    public bool AllowChapters { get; set; }
    public bool ShowNaviPane { get; set; }
    public bool ShowFlowPane { get; set; }

    public string ListingSnippetDefault { get; set; }
    public string TextSnippetDefault { get; set; }
    public string ChapterDefault { get; set; }
    public string SectionDefault { get; set; }


    public bool IsBoilerplate { get; set; }

  }
}
