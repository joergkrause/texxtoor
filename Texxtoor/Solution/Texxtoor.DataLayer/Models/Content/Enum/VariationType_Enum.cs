using System.ComponentModel.DataAnnotations;

namespace Texxtoor.DataModels.Models.Content {

  /// <summary>
  /// The type of variation. This defines why a new variation of a leaf element exists. 
  /// </summary>
  public enum VariationType {
    /// <summary>
    /// The primary element, set automatically for all elements.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "VariationType_HeadRevision_HeadRevision", Description="VariationType_HeadRevision_HeadRevision_Helptext")]
    HeadRevision = 0,
    /// <summary>
    /// Just an alternative, the processor expects that it differs at least in level, or target, or both.  
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "VariationType_Alternative_Alternative", Description="VariationType_Alternative_Alternative_Helptext")]
    Alternative = 1,
    /// <summary>
    /// A translation of the snippet. The elements inherit from localized base so author can set the target culture.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "VariationType_Translation_Translation", Description="VariationType_Translation_Translation_Helptext")]
    Translation = 2,
    /// <summary>
    /// This continues a snippet. The processor tries to add all continuations together.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "VariationType_Continuation_Continuation", Description="VariationType_Continuation_Continuation_Helptext")]
    Continuation = 3,
    /// <summary>
    /// This is an replacing update. There is no specific reason for an update. 
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "VariationType_Update_Update", Description="VariationType_Update_Update_Helptext")]
    Update = 4,
    /// <summary>
    /// This is a correction, fix
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "VariationType_Correction_Correction", Description="VariationType_Correction_Correction_Helptext")]
    Correction = 5,
    /// <summary>
    /// Anything else, currently not used.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "VariationType_Other_Other", Description="VariationType_Other_Other_Helptext")]
    Other = 99
  }


}
