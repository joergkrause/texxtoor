using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;

namespace Texxtoor.ViewModels.Users {

  #region Models

  public class RetrievePassword {

    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "RetrievePassword_UserName_Please_provide_your_username_or_email")]
    [Display(ResourceType = typeof(ModelResources), Name = "RetrievePassword_UserName_User_name_or_E_Mail", Description="RetrievePassword_UserName_User_name_or_E_Mail_Helptext")]
    public string UserName { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "RetrievePassword_PasswordQuestion_Password_Question", Description="RetrievePassword_PasswordQuestion_Password_Question_Helptext")]
    [ReadOnly(true)]
    public string PasswordQuestion { get; set; }

    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "RetrievePassword_PasswordAnswer_The_answer_for_your_password_retrieval_question")]
    [Display(ResourceType = typeof(ModelResources), Name = "RetrievePassword_PasswordAnswer_Provide_Answer", Description="RetrievePassword_PasswordAnswer_Provide_Answer_Helptext")]
    [StringLength(250)]
    public string PasswordAnswer { get; set; }

  }
  #endregion

}
