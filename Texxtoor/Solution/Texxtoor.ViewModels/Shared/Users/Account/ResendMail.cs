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

  
  public class ResendMail {

    [Required(ErrorMessageResourceType = typeof(ModelResources), ErrorMessageResourceName = "LogOn_UserName_You_must_provide_your_username_or_email")]
    [Display(ResourceType = typeof(ModelResources), Name = "LogOn_UserName_User_Name", Description = "LogOn_UserName_User_Name_Helptext")]
    public string UserName { get; set; }

  }

  #endregion


}
