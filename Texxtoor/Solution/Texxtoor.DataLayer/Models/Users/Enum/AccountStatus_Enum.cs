using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core;

namespace Texxtoor.DataModels.Models {

  public enum AccountStatus {
    Created = 1,
    Active = 2,
    Deleted = 3,
    Closed = 4,
    Blocked = 5,
    Suspicious = 6
  }



}