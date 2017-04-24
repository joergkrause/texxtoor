using System;
using System.Collections.Generic;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.ViewModels.Editor {

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
    public Element PrevSnippet { get; set; }
    /// <summary>
    /// Precalculated successor in the same hierarchy level. Regular Up/Down operations never change hierarchy level.
    /// </summary>
    public Element NextSnippet { get; set; }

    public bool Decrease { get; set; }
    public bool Increase { get; set; }

    public int DocumentId { get; set; } // Opus, in Designer we don't use this, because SVG images are assigned to project
    public int SnippetId { get; set; }  // Snippet, if already assigned to a particular Opus
    public int ProjectId { get; set; }  // related project
    public int ResourceId { get; set; } // source Id, a project resource

    public string Referer { get; set; }

  }

}