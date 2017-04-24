using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Texxtoor.BaseLibrary;
using System.Threading.Tasks;
using Texxtoor.BaseLibrary.Core.Notifications;
using Texxtoor.BusinessLayer;

namespace Texxtoor.Portal.Hubs {


  /// <summary>
  /// Define methods the client can call.
  /// </summary>
  [HubName("notifications")]
  public class NotificationHub : Hub {

    # region Service Reference

    // map connection id to username
    private static readonly IDictionary<string, string> connectionMapper = new Dictionary<string, string>();

    public IDictionary<string, string> ConnectionMapper {
      get {
        return NotificationHub.connectionMapper;
      }
    }

    private readonly NotificationService notification;

    public NotificationHub()
      : this(NotificationService.Instance) {
    }

    public NotificationHub(NotificationService notification) {
      notification.Hub = this;
      this.notification = notification;      
    }

    # endregion

    # region Server Methods

    /// <summary>
    /// Initial values for clients calling the very first time.
    /// </summary>
    /// <returns></returns>
    public NavigationTileData GetTileValues() {
      return new NavigationTileData();

    }

    public UserGroupData GetUserOnline() {
      var users = Manager<UserProfileManager>.Instance.GetUserRelations(HttpContext.Current.User.Identity.Name);
      return users;
    }

    public void SendChatMessageToUser(int user, string message) {
      var sender = HttpContext.Current.User.Identity.Name;
      var receiver = Manager<UserManager>.Instance.GetUser(user).UserName;
      notification.ChatMessage(new[] { receiver, sender }, sender, message);
    }

    public void SendChatMessageToGroup(int group, string message) {
      var sender = HttpContext.Current.User.Identity.Name;
      var groupId = Manager<ReaderManager>.Instance.GetGroup(group).Id;
      var groupName = String.Format("Group~{0}", groupId);
      notification.GroupMessage(groupName, sender, message);
    }

    public void SendChatMessageToTeam(int team, string message) {
      var sender = HttpContext.Current.User.Identity.Name;
      var teamId = Manager<ProjectManager>.Instance.GetTeam(team).Id;      
      var teamName = String.Format("Team~{0}", teamId);
      notification.GroupMessage(teamName, sender, message);
    }

    public void JoinGroup(int groupId) {
      var groupName = String.Format("Group~{0}", groupId);
      Groups.Add(Context.ConnectionId, groupName);
    }

    public void JoinTeam(int teamId) {
      var teamName = String.Format("Team~{0}", teamId);
      Groups.Add(Context.ConnectionId, teamName);
    }

    # endregion

    public override Task OnConnected() {
      var user = Context.User.Identity.Name;
      if (!connectionMapper.ContainsKey(user)) {
        connectionMapper.Add(user, Context.ConnectionId);
      }
      return base.OnConnected();
    }

    public override Task OnReconnected() {
      var user = Context.User.Identity.Name;
      if (!connectionMapper.ContainsKey(user)) {
        connectionMapper.Add(user, Context.ConnectionId);
      } else {
        connectionMapper[user] = Context.ConnectionId;
      }
      return base.OnReconnected();
    }

    public override Task OnDisconnected() {
      var user = Context.User.Identity.Name;
      if (connectionMapper.ContainsKey(user)) {
        connectionMapper.Remove(user);
      }
      notification.SetUserOnline();
      return base.OnDisconnected();
    }

  }
}