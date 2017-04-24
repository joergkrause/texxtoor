
using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels;

namespace Texxtoor.ViewModels.Users {
  
  public class ExternalLoginConfirmationViewModel {

    [Required]
    [Display(ResourceType = typeof (ModelResources), Name = "ExternalLoginConfirmationViewModel_UserName_User_name")]
    public string UserName { get; set; }

    [Display(Name="Association Password", Description="If you have already a local texxtoor account, you can associate your external logon with this internal one. Please provide the password for your local account. It you want to use the external account only, leave the field empty.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

  }


}
