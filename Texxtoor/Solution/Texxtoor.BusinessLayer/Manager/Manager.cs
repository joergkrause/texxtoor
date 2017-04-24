using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.BaseLibrary.Core.Notifications;
using Texxtoor.BusinessLayer.Properties;
using Texxtoor.BaseLibrary.Repository;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Users;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Texxtoor.BusinessLayer {


  /// <summary>
  /// Base class for all BLL classes
  /// </summary>
  [DebuggerStepThrough]
  public abstract class Manager<T> : Singleton<T>, IManager, IDisposable where T : new() {

    /// <summary>
    /// For global security trimming
    /// </summary>
    public string UserName { get; set; }

    public PortalContext Ctx {
      get {
        return UnitOfWorkFactory.GetIUnitOfWorkContext<PortalContext>();
      }
    }

    # region AspNet User Management

    public UserManager<User, int> Usermanager;
    public RoleManager<Role, int> Rolemanager;

    # region Custom User Store

    public interface ITexxtoorUserStore<TUser> : IUserStore<TUser, int>, IDisposable where TUser : IdentityUser<int, TexxtoorLogin, TexxtoorUserRole, TexxtoorUserClaim> { //}Microsoft.AspNet.Identity.IUser<int> {
    }


    public class TexxtoorUserStore<TUser> :
      IUserRoleStore<TUser, int>,
      IQueryableUserStore<TUser, int>,
      IUserPasswordStore<TUser, int>,
      IUserSecurityStampStore<TUser, int>,
      ITexxtoorUserStore<TUser>,
      IUserLoginStore<TUser, int> where TUser : User { //}IdentityUser<int, TexxtoorLogin, TexxtoorUserRole, TexxtoorUserClaim>  {

      private readonly PortalContext _ctx;
      private UserStore<IdentityUser> _store;
      private readonly object _lock = new object();

      public TexxtoorUserStore() {
      }

      public TexxtoorUserStore(PortalContext ctx) {
        _ctx = ctx;
        _store = new UserStore<IdentityUser>(_ctx);
      }

      public async System.Threading.Tasks.Task CreateAsync(TUser user) {
        var context = _store.Context as PortalContext;
        user.SecurityStamp = Guid.NewGuid().ToString();
        context.Users.Add(user);
        user.CreatedAt = DateTime.Now;
        user.ModifiedAt = DateTime.Now;
        var profile = await context.UserProfiles.SingleOrDefaultAsync(u => u.User.UserName == user.UserName);
        if (profile == null) {
          // create default profile and take current runcontrol settings
          var rc = HttpContext.Current != null ? HttpContext.Current.Session["RunControl"] as RunControl : new RunControl() {
            Complexity = Complexity.Full,
            UiLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName
          };
          var up = new UserProfile { Addresses = new List<AddressBook>(), User = user, PayPalUserId = user.Email };
          up.RunControl = new RunControl {
            Complexity = rc.Complexity,
            Favorites = rc.Favorites,
            RunMode = rc.RunMode,
            UiLanguage = rc.UiLanguage
          };
          context.UserProfiles.Add(up);
        }
        await context.SaveChangesAsync();
      }

      public async System.Threading.Tasks.Task DeleteAsync(TUser user) {
        var context = _store.Context as PortalContext;
        _ctx.Users.Remove(user);
        await context.SaveChangesAsync();
      }

      public async System.Threading.Tasks.Task<TUser> FindByIdAsync(int userId) {
        var context = _store.Context as PortalContext;
        var result = await context.Users.SingleOrDefaultAsync(u => u.Id == userId);
        return result as TUser;
      }

      public async System.Threading.Tasks.Task<TUser> FindByNameAsync(string userName) {
        var context = _store.Context as PortalContext;
        var result = await context.Users.SingleOrDefaultAsync(u => u.UserName == userName);
        return result as TUser;
      }

      public async System.Threading.Tasks.Task UpdateAsync(TUser user) {
        var context = _store.Context as PortalContext;
        user.ModifiedAt = DateTime.Now;
        context.Entry(user).State = EntityState.Modified;
        await context.SaveChangesAsync();
      }

      public void Dispose() {
        var context = _store.Context as PortalContext;
        context.Dispose();
      }

      private readonly SemaphoreSlim semaphoreGetPasswordHash = new SemaphoreSlim(1, 10);

      public async System.Threading.Tasks.Task<string> GetPasswordHashAsync(TUser user) {
        await semaphoreGetPasswordHash.WaitAsync();
        try {
          var context = _store.Context as PortalContext;
          var result = await context.Users.SingleOrDefaultAsync(u => u.UserName == user.UserName);
          return result != null ? result.Password : String.Empty;
        } finally {
          semaphoreGetPasswordHash.Release();
        }
      }

      public async System.Threading.Tasks.Task<bool> HasPasswordAsync(TUser user) {
        var context = _store.Context as PortalContext;
        var result = await context.Users.SingleOrDefaultAsync(u => u.UserName == user.UserName);
        return result != null && !String.IsNullOrEmpty(result.Password);
      }

      public async System.Threading.Tasks.Task SetPasswordHashAsync(TUser user, string passwordHash) {
        var context = _store.Context as PortalContext;
        var result = await context.Users.SingleOrDefaultAsync(u => u.UserName == user.UserName);
        if (result != null) {
          // we write both as we want to be downwards compatible with the user base created with old membership provider
          result.Password = passwordHash;
          result.PasswordHash = passwordHash;
          await context.SaveChangesAsync();
        }
      }

      public async System.Threading.Tasks.Task<string> GetSecurityStampAsync(TUser user) {
        var context = _store.Context as PortalContext;
        var result = await context.Users.SingleOrDefaultAsync(u => u.UserName == user.UserName);
        return result != null ? result.SecurityStamp : String.Empty;
      }

      public async System.Threading.Tasks.Task SetSecurityStampAsync(TUser user, string stamp) {
        var context = _store.Context as PortalContext;
        var result = await context.Users.SingleOrDefaultAsync(u => u.UserName == user.UserName);
        if (result != null) {
          result.SecurityStamp = stamp;
          result.ModifiedAt = DateTime.Now;
          await context.SaveChangesAsync();
        }
      }

      public IQueryable<TUser> Users {
        get {
          var context = _store.Context as PortalContext;
          return context.Users.Cast<TUser>().AsQueryable();
        }
      }

      public async System.Threading.Tasks.Task AddToRoleAsync(TUser user, string roleName) {
        var context = _store.Context as PortalContext;
        var usr = await context.Users.SingleOrDefaultAsync(u => u.Id == user.Id);
        var rl = await context.Roles.SingleOrDefaultAsync(r => r.Name == roleName && r.Users.Any(u => u.UserId == user.Id));
        if (rl == null) {
          // not yet assigned          
          rl = await context.Roles.SingleOrDefaultAsync(r => r.Name == roleName);
          if (rl != null) {
            // role exists
            rl.Users.Add(new TexxtoorUserRole { RoleId = rl.Id, UserId = usr.Id });
            rl.ModifiedAt = DateTime.Now;
            await context.SaveChangesAsync();
          }
        }
      }

      public async System.Threading.Tasks.Task<System.Collections.Generic.IList<string>> GetRolesAsync(TUser user) {
        var context = _store.Context as PortalContext;
        return await context.Roles
          .Where(r => r.Users.Any(u => u.UserId == user.Id))
          .Select(r => r.Name)
          .ToListAsync();
      }

      public async System.Threading.Tasks.Task<bool> IsInRoleAsync(TUser user, string roleName) {
        var context = _store.Context as PortalContext;
        var usr = context.Users.SingleOrDefault(u => u.Id == user.Id);
        if (usr != null) {
          return await context.Roles.AnyAsync(r => r.Name == roleName && r.Users.Any(u => u.UserId == user.Id));
        }
        return false;
      }

      public async System.Threading.Tasks.Task RemoveFromRoleAsync(TUser user, string roleName) {
        var context = _store.Context as PortalContext;
        var usr = await context.Users.SingleOrDefaultAsync(u => u.Id == user.Id);
        var rl = await context.Roles.SingleOrDefaultAsync(r => r.Name == roleName && r.Users.Any(u => u.UserId == user.Id));
        if (rl != null) {
          // is assigned          
          rl = await context.Roles.SingleOrDefaultAsync(r => r.Name == roleName);
          if (rl != null) {
            // role exists
            rl.Users.Remove(new TexxtoorUserRole { RoleId = rl.Id, UserId = usr.Id });
            rl.ModifiedAt = DateTime.Now;
            await context.SaveChangesAsync();
          }
        }
      }

      public async System.Threading.Tasks.Task AddLoginAsync(TUser user, UserLoginInfo login) {
        var context = _store.Context as PortalContext;
        var provider = login.LoginProvider;
        var providerKey = login.ProviderKey;
        var contextUser = (TUser)context.Users.Find(user.Id);
        if (contextUser != null) {
          var texxtoorLogin = new TexxtoorLogin {
            LoginProvider = provider,
            ProviderKey = providerKey
          };
          contextUser.Logins.Add(texxtoorLogin);
          await context.SaveChangesAsync();
        }
      }

      public System.Threading.Tasks.Task<TUser> FindAsync(UserLoginInfo login) {
        var context = _store.Context as PortalContext;
        var user = context.Users.SingleOrDefault(u => u.Logins.Any(l => l.ProviderKey == login.ProviderKey && l.LoginProvider == login.LoginProvider)) as TUser;
        return Task.FromResult<TUser>(user);
      }

      public System.Threading.Tasks.Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user) {
        var context = _store.Context as PortalContext;
        var contextUser = (TUser)context.Users.Find(user.Id);
        var logins = contextUser.Logins.Select(l => new UserLoginInfo(l.ProviderKey, l.LoginProvider));
        return Task.FromResult<IList<UserLoginInfo>>(logins.ToList());
      }

      public async System.Threading.Tasks.Task RemoveLoginAsync(TUser user, UserLoginInfo login) {
        var context = _store.Context as PortalContext;
        var provider = login.LoginProvider;
        var providerKey = login.ProviderKey;
        var contextUser = (TUser)context.Users.Find(user.Id);
        var texxtoorLogin = new TexxtoorLogin {
          LoginProvider = provider,
          ProviderKey = providerKey
        };
        contextUser.Logins.Remove(texxtoorLogin);
        await context.SaveChangesAsync();
      }

    }

    # endregion Custom User Store

    # region Custom Role Store

    public class TexxtoorRoleStore<TRole> : RoleStore<Role, int, TexxtoorUserRole> {

      public TexxtoorRoleStore(PortalContext ctx)
        : base(ctx) {
      }


    }

    # endregion Custom Role Store

    protected Manager() {
      Usermanager = new UserManager<User, int>(new TexxtoorUserStore<User>(Ctx));
      Usermanager.PasswordHasher = new TexxtoorPasswordHasher("SHA1");
      Rolemanager = new RoleManager<Role, int>(new TexxtoorRoleStore<Role>(Ctx));
    }


    # endregion

    public IUnitOfWork AsUnitOfWork() {
      return Ctx;
    }

    public int SaveChanges() {
      return Ctx.SaveChanges();
    }

    public void UndoChanges() {
      Ctx.UndoChanges();
    }

    public TP PermanentCache<TP>(string cacheName, Func<TP> loadCache) where TP : class {
      if (HttpContext.Current.Cache[cacheName] != null) {
        return HttpContext.Current.Cache[cacheName] as TP;
      }
      var obj = loadCache();
      HttpContext.Current.Cache.Add("Countries", obj, null, Cache.NoAbsoluteExpiration, TimeSpan.FromDays(5),
                                    CacheItemPriority.Normal, null);
      return obj;
    }

    public User GetCurrentUser(string username) {
      return Ctx.Users.FirstOrDefault(u => u.UserName.Equals(username));
    }

    public string GetCurrentUserName() {
      return HttpContext.Current.User.Identity.Name;
    }

    protected void ClearNavCache(string username) {
      var key = String.Format(Settings.Default.NavCache, username);
      var c = HttpContext.Current.Cache;
      c.Remove(key);
    }

    public string CurrentCulture {
      get {
        var cult = System.Threading.Thread.CurrentThread.CurrentUICulture;
        return (cult.Parent == CultureInfo.InvariantCulture) ? cult.TwoLetterISOLanguageName : cult.Parent.TwoLetterISOLanguageName;
      }
    }

    public void Dispose() { }

  }
}
