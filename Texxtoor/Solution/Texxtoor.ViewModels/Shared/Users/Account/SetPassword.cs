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

  /// <summary>
  /// Used after retrieve to reset in one single step.
  /// </summary>
  public class SetPassword {

    [Required(ErrorMessageResourceType = typeof(ModelResources), ErrorMessageResourceName = "LogOn_UserName_You_must_provide_your_username_or_email")]
    [Display(ResourceType = typeof(ModelResources), Name = "LogOn_UserName_User_Name", Description = "LogOn_UserName_User_Name_Helptext")]
    [StringLength(200)]
    public string UserName { get; set; }


    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "ChangePassword_OldPassword_You_must_provide_the_current_password")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(ModelResources), Name = "ChangePassword_OldPassword_Current_password", Description="ChangePassword_OldPassword_Current_password_Helptext")]
    public string OldPassword { get; set; }

    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "ChangePassword_NewPassword_You_must_provide_a_new_password")]
    [ValidatePasswordLength]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(ModelResources), Name = "ChangePassword_NewPassword_New_password", Description="ChangePassword_NewPassword_New_password_Helptext")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(ModelResources), Name = "ChangePassword_ConfirmPassword_Confirm_new_password", Description="ChangePassword_ConfirmPassword_Confirm_new_password_Helptext")]
    [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "ChangePassword_ConfirmPassword_The_new_password_and_confirmation_password_do_not_match_")]
    public string ConfirmPassword { get; set; }
  }
  
  #endregion

}
