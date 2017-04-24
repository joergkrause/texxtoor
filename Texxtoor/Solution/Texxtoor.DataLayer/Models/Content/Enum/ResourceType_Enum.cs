using System.ComponentModel.DataAnnotations;
using Texxtoor.BaseLibrary.Core;
using System;

namespace Texxtoor.DataModels.Models.Content {

  /// <summary>
  /// Used to manage icons in the resource manager.
  /// </summary>
  public enum TypeOfResource {
    /// <summary>
    /// Part of the project that supports the members, such as zip files or PDFs
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "TypeOfResource_Project_Project")]
    Project = 1,
    /// <summary>
    /// Actual content, used to create fragments
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "TypeOfResource_Content_Content")]
    Content = 2,
    /// <summary>
    /// If the project was imported from Word the import's raw material goes here
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "TypeOfResource_Import_Import")]
    Import = 3,
    /// <summary>
    /// Content that was deleted goes to trash. If trash is deleted it's gone forever
    /// </summary>
    [Display(ResourceType = typeof (ModelResources), Name = "TypeOfResource_Trash_Trash")]
    Trash = 99
  }

}
