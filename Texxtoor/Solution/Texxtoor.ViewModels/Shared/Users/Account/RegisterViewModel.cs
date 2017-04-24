using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;

namespace Texxtoor.ViewModels.Users {

  #region Models

  public class RegisterViewModel {

    [StringLength(200)]
    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "Register_UserName_You_must_provide_a_username")]
    [Display(ResourceType = typeof(ModelResources), Name = "LogOn_UserName_User_Name", Description="LogOn_UserName_User_Name_Helptext")]
    public string UserName { get; set; }

    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "Register_Email_You_must_provide_an_email_address")]
    [DataType(DataType.EmailAddress)]
    [StringLength(200)]
    [EmailAddress]
    [Display(ResourceType = typeof(ModelResources), Name = "Register_Email_E_Mail", Description="Register_Email_E_Mail_Helptext")]
    public string Email { get; set; }

    [Required(ErrorMessageResourceType = typeof(ModelResources), ErrorMessageResourceName = "Register_Password_You_must_provide_a_password")]
    [ValidatePasswordLength]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(ModelResources), Name = "LogOn_Password_Password", Description="LogOn_Password_Password_Helptext")]
    public string Password { get; set; }

    [StringLength(200)]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(ModelResources), Name = "Register_ConfirmPassword_Confirm_password", Description="Register_ConfirmPassword_Confirm_password_Helptext")]
    [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "Register_ConfirmPassword_The_password_given_does_not_match_the_password_typed_above")]
    public string ConfirmPassword { get; set; }

    [StringLength(250)]
    [ScaffoldColumn(false)]
    [Display(ResourceType = typeof(ModelResources), Name = "Register_PasswordQuestion_Question_asked_to_retrieve_password", Description="Register_PasswordQuestion_Question_asked_to_retrieve_password_Helptext")]
    public string PasswordQuestion { get; set; }

    [StringLength(250)]
    [ScaffoldColumn(false)]
    [Display(ResourceType = typeof(ModelResources), Name = "Register_PasswordAnswer_Answer_to_get_password_back", Description = "Register_PasswordAnswer_Answer_to_get_password_back_Helptext")]
    public string PasswordAnswer { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "QuickForm_FavoriteRole_Favorite_Role", Description = "QuickForm_FavoriteRole_Favorite_Role_Helptext")]
    [UIHint("FavoriteRole")]
    public ContributorRole FavoriteRole { get; set; }

    [StringLength(60)]
    [RegularExpression("[0-9-/]{5,60}", ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "RegisterViewModel_Phone_FormatError")]
    [Display(ResourceType = typeof(ModelResources), Name = "QuickForm_Phone_Phone", Description = "QuickForm_Phone_Phone_Helptext")]
    public string Phone { get; set; }

    [ScaffoldColumn(false)]    
    public List<string> ProviderList  { get; set; }
  }
  #endregion

  }
