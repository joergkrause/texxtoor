using System.ComponentModel.DataAnnotations;

namespace LinqDemo.Models {

  /// <summary>
  /// The type of variation. This defines why a new variation of a leaf element exists. 
  /// </summary>
  public enum VariationType {
    /// <summary>
    /// The primary element, set automatically for all elements.
    /// </summary>
    HeadRevision = 0,
    /// <summary>
    /// Just an alternative, the processor expects that it differs at least in level, or target, or both.  
    /// </summary>
    Alternative = 1,
    /// <summary>
    /// A translation of the snippet. The elements inherit from localized base so author can set the target culture.
    /// </summary>
    Translation = 2,
    /// <summary>
    /// This continues a snippet. The processor tries to add all continuations together.
    /// </summary>
    Continuation = 3,
    /// <summary>
    /// This is an replacing update. There is no specific reason for an update. 
    /// </summary>
    Update = 4,
    /// <summary>
    /// This is a correction, fix
    /// </summary>
    Correction = 5,
    /// <summary>
    /// Anything else, currently not used.
    /// </summary>
    Other = 99
  }


}
