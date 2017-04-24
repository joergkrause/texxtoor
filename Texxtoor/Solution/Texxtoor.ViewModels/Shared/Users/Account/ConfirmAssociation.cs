using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels;

namespace Texxtoor.ViewModels.Users {

  public class ConfirmAssociation {
  
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "ConfirmAssociation_LinkCode_Code")]
    public string Email { get; set; }

    [Required]
    [ScaffoldColumn(false)]
    public string LinkCode { get; set; }
  }
}
