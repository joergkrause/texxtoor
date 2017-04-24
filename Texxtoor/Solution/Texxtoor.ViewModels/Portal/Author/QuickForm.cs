using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.ViewModels.Author {
  public class QuickForm {

    [Required]
    [StringLength(100)]
    [Display(ResourceType = typeof(ModelResources), Name = "QuickForm_Name_Your_name", Description="QuickForm_Name_Your_name_Helptext")]
    public string Name { get; set; }

    [Required]
    [StringLength(100)]
    [EmailAddress]
    [Display(ResourceType = typeof(ModelResources), Name = "QuickForm_Email_Your_email", Description="QuickForm_Email_Your_email_Helptext")]
    public string Email { get; set; }

    [StringLength(200)]
    [Display(ResourceType = typeof(ModelResources), Name = "QuickForm_ProjectName_Project_proposal_name", Description="QuickForm_ProjectName_Project_proposal_name_Helptext")]
    [ScaffoldColumn(false)]
    public string ProjectName { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "QuickForm_FavoriteRole_Favorite_Role", Description = "QuickForm_FavoriteRole_Favorite_Role_Helptext")]
    [UIHint("FavoriteRole")]
    public ContributorRole FavoriteRole { get; set; }

    [StringLength(200)]
    [Display(ResourceType = typeof(ModelResources), Name = "QuickForm_Phone_Phone", Description="QuickForm_Phone_Phone_Helptext")]
    public string Phone { get; set; }

  }
}
