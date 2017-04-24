using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;

namespace Texxtoor.ViewModels.Users {

  #region Models

  public class LogOn {

    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "LogOn_UserName_You_must_provide_your_username_or_email")]
    [Display(ResourceType = typeof(ModelResources), Name = "LogOn_UserName_User_Name", Description="LogOn_UserName_User_Name_Helptext")]
    [StringLength(200)]
    public string UserName { get; set; }

    [Required(ErrorMessageResourceType = typeof (ModelResources), ErrorMessageResourceName = "LogOn_Password_You_must_provide_a_password")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(ModelResources), Name = "LogOn_Password_Password", Description="LogOn_Password_Password_Helptext")]
    [AdditionalMetadata("Length", 25)]
    [StringLength(30)]
    public string Password { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "LogOn_RememberMe_Remember_Me", Description="LogOn_RememberMe_Remember_Me_Helptext")]
    [UIHint("Boolean_NotNull")]
    public bool RememberMe { get; set; }
  }
  #endregion

  
}
