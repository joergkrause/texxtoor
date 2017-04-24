using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels;
using Texxtoor.BaseLibrary.Core;

namespace Texxtoor.ViewModels.Users {

  public class RegisterExternalLogin {
    [Required]
    [Display(ResourceType = typeof (ModelResources), Name = "RegisterExternalLogin_UserName_User_name")]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    [Display(ResourceType = typeof(ModelResources), Name = "RegisterExternalLogin_Email_Email")]
    public string Email { get; set; }

    [Required(ErrorMessageResourceType = typeof(ModelResources), ErrorMessageResourceName = "Register_Password_You_must_provide_a_password")]
    [ValidatePasswordLength]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(ModelResources), Name = "LogOn_Password_Password", Description = "LogOn_Password_Password_Helptext")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(ModelResources), Name = "Register_ConfirmPassword_Confirm_password", Description = "Register_ConfirmPassword_Confirm_password_Helptext")]
    [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessageResourceType = typeof(ModelResources), ErrorMessageResourceName = "Register_ConfirmPassword_The_password_given_does_not_match_the_password_typed_above")]
    public string ConfirmPassword { get; set; }

    //[ScaffoldColumn(false)]
    //public string ExternalLoginData { get; set; }

    //[Display(ResourceType = typeof(ModelResources), Name = "RegisterExternalLogin_InternalUserName_Internal_User_Name")]
    //[ScaffoldColumn(false)]
    //public string InternalUserName { get; set; }
  }

}
