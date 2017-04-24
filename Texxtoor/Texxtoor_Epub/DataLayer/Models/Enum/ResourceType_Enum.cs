using System;

namespace Texxtoor.Models {

  /// <summary>
  /// Used to manage icons in the resource manager.
  /// </summary>
  public enum TypeOfResource {
    /// <summary>
    /// Part of the project that supports the members, such as zip files or PDFs
    /// </summary>
    Project = 1,
    /// <summary>
    /// Actual content, used to create fragments
    /// </summary>
    Content = 2,
    /// <summary>
    /// If the project was imported from Word the import's raw material goes here
    /// </summary>
    Import = 3,
    /// <summary>
    /// Content that was deleted goes to trash. If trash is deleted it's gone forever
    /// </summary>
    Trash = 99
  }

}
