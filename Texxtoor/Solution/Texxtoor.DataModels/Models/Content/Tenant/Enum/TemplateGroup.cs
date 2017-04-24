
using System;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.DataModels.Models.Content {

  /// <summary>
  /// Manage the template types.
  /// </summary>
  public enum GroupKind {
    /// <summary>
    /// Use these templates to create a PDF.
    /// </summary>
    [FileExtensions(Extensions = "pdf")]
    [Display(Name ="PDF")]
    [UIHint("Publishable")]
    Pdf,
    /// <summary>
    /// These template contain serialized import functions.
    /// </summary>
    [FileExtensions(Extensions = "dotx")]
    Word,
    /// <summary>
    /// CSS and HTML for EPUBs.
    /// </summary>
    [FileExtensions(Extensions = "epub")]
    [Display(Name = "EPUB")]
    [UIHint("Publishable")]
    Epub,
    /// <summary>
    /// CSS and HTML or XAML for Apps. 
    /// </summary>
    App,
    /// <summary>
    /// Used to modify the editor's behavior  
    /// </summary>
    Editor,
    /// <summary>
    /// Identifies templates for sending mail
    /// </summary>
    Email,
    /// <summary>
    /// Identifies templates for plain HTML output
    /// </summary>
    [FileExtensions(Extensions = "html")]
    [Display(Name = "HTML")]
    [UIHint("Publishable")]
    Html,
    /// <summary>
    /// Makes well looking RSS feeds
    /// </summary>
    Rss
  }
}
