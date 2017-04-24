using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Texxtoor.BaseLibrary;
using Texxtoor.BaseLibrary.Core.Notifications;
using System.Diagnostics;
using System.Threading.Tasks;
using Texxtoor.BusinessLayer;

namespace Texxtoor.Portal.Hubs {
  public class NotificationService : INotificationService {

    private readonly static Lazy<NotificationService> instance =
        new Lazy<NotificationService>(() => new NotificationService(GlobalHost.ConnectionManager.GetHubContext<NotificationHub>().Clients));

    private NotificationService(IHubConnectionContext clients) {
      this.Clients = clients;
    }

    public static NotificationService Instance { get { return instance.Value; } }

    private IHubConnectionContext Clients { get; set; }

    public NotificationHub Hub { get; set; }

    public HubCallerContext Context {
      get {
        return Hub.Context;
      }
    }

    # region Functions we can call on the client's machines

    /// <summary>
    /// Notify all
    /// </summary>
    /// <param name="message"></param>
    public void Notify(string message) {
      Clients.All.notify(message);
    }

    /// <summary>
    /// Refresh all tiles
    /// </summary>
    /// <param name="target"></param>
    /// <param name="value"></param>
      public void ApplyTileValue(string target, int value) {
        Clients.All.applyTileValue(new NavigationTileData(){
          Target = target, 
          Value = value
        });
      }

    /// <summary>
    /// Reset all clients' tiles
    /// </summary>
    public void Reset() {
      Clients.All.reset();
    }

    /// <summary>
    /// Refresh the chat's global user display
    /// </summary>
    public void SetUserOnline() {
      if (Hub == null) return;
      // we broadcast but each clients gets its very own package
      var connections = Hub.ConnectionMapper.ToArray();
      foreach (var usermap in connections) {
        Debug.WriteLine(usermap.Key);
        var users = Manager<UserProfileManager>.Instance.GetUserRelations(usermap.Key);
        if (users != null) {
          Clients.Client(usermap.Value).setUserOnline(users);
        }
      }
    }

    /// <summary>
    /// An individual message for just one user
    /// </summary>
    /// <param name="percent"></param>
    /// <param name="message"></param>
    public void SetProductionProgress(int percent, string message) {
      if (Hub == null) return;
      var user = HttpContext.Current.User.Identity.Name;
      var userId = Hub.ConnectionMapper[user];
      // send to exactly this user only
      Clients.Client(userId).setProductionProgress(percent, message);
# if DEBUG
      Thread.Sleep(500);
# else 
      Thread.Sleep(50);
# endif
    }

    /// <summary>
    /// Send a chat message to the user
    /// </summary>
    /// <param name="receivers">All receivers, including the sender for reference</param>
    /// <param name="message"></param>
    public void ChatMessage(string[] receivers, string sender, string message) {
      if (Hub == null) return;
      foreach (var receiver in receivers) {
        var userId = Hub.ConnectionMapper[receiver];
        var senderId = Manager<UserManager>.Instance.GetUserByName(sender).Id;
        Clients.Client(userId).chatMessage(senderId, sender, message);
      }
    }

    /// <summary>
    /// Send a chat message to a group
    /// </summary>
    /// <param name="group"></param>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    public void GroupMessage(string group, string sender, string message) {
      if (Hub == null) return;
      var user = HttpContext.Current.User.Identity.Name;
      Clients.Group(group).chatMessage(sender, message);
    }

    # endregion

  }
}