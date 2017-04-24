using System;
using Texxtoor.BaseLibrary.Core.Notifications;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.BusinessLayer {

  /// <summary>
  /// Unified structure of business layer entities.
  /// </summary>
  public interface IManager {

    /// <summary>
    /// Global security trimming
    /// </summary>
    string UserName { get; set; }

    /// <summary>
    /// Allows the caller to override the internal context. This is used internally for setup. Should remain <c>null</c>.
    /// </summary>
    PortalContext Ctx { get; }
    /// <summary>
    /// Convenient access to Context.SaveChanges with error handling and logging.
    /// </summary>
    /// <returns></returns>
    int SaveChanges();
    /// <summary>
    /// Used to secure all data accuess
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    User GetCurrentUser(string username);

  }
}
