using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Texxtoor.DataModels.Models.Author {

  /// <summary>
  /// The roles a user can have in a team, or a user can propose in his or her profile.
  /// </summary>
  /// <remarks>
  /// This role can be assigned multiple times, that means, a user can have as many roles as he or she likes.
  /// </remarks>
  [Flags]
  [DefaultValue("ContributorRole_Default")] // defined in ModelResource.resx
  public enum ContributorRole {

    [Display(ResourceType = typeof(ModelResources), Name = "ContributorRole_Author_Author", Description="ContributorRole_Author_Author_Helptext")]
    Author = 1,
    [Display(ResourceType = typeof(ModelResources), Name = "ContributorRole_Designer_Designer", Description="ContributorRole_Designer_Designer_Helptext")]
    Designer = 2,
    [Display(ResourceType = typeof(ModelResources), Name = "ContributorRole_Illustrator_Illustrator", Description="ContributorRole_Illustrator_Illustrator_Helptext")]
    Illustrator = 4,
    [Display(ResourceType = typeof(ModelResources), Name = "ContributorRole_CopyEditor_Copy_Editor", Description="ContributorRole_CopyEditor_Copy_Editor_Helptext")]
    CopyEditor = 8,
    [Display(ResourceType = typeof(ModelResources), Name = "ContributorRole_Editor_Editor", Description="ContributorRole_Editor_Editor_Helptext")]
    Editor = 16,
    [Display(ResourceType = typeof(ModelResources), Name = "ContributorRole_Reviewer_Reviewer", Description="ContributorRole_Reviewer_Reviewer_Helptext")]
    Reviewer = 32,
    [Display(ResourceType = typeof(ModelResources), Name = "ContributorRole_Translator_Translator", Description="ContributorRole_Translator_Translator_Helptext")]
    Translator = 64,
    [Display(ResourceType = typeof(ModelResources), Name = "ContributorRole_Critic_Critic", Description="ContributorRole_Critic_Critic_Helptext")]
    Critic = 128,
    [Display(ResourceType = typeof(ModelResources), Name = "ContributorRole_TechnicalReviewer_Technical_Reviewer", Description="ContributorRole_TechnicalReviewer_Technical_Reviewer_Helptext")]
    TechnicalReviewer = 256,
    [Display(ResourceType = typeof(ModelResources), Name = "ContributorRole_Other_Other", Description="ContributorRole_Other_Other_Helptext")]
    Other = 1024    

  }

}