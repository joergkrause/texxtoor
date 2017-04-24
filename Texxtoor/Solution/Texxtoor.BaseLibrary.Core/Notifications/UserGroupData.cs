using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Texxtoor.BaseLibrary.Core.Notifications {

  public class ConnectedUser {
    public int UserId { get; set; }
    public string UserName { get; set; }
    public bool IsConnected { get; set; }
  }
  
  public class UserGroupData {

    public UserGroupData() {
      Users = new Dictionary<string, List<ConnectedUser>>();
    }

    public Dictionary<string, List<ConnectedUser>> Users { get; set; }

    public int Total { get; set; }

    public int TotalOnline { get; set; }

    public string CurrentUser { get; set; }
    public int CurrentUserId { get; set; }

  }
}