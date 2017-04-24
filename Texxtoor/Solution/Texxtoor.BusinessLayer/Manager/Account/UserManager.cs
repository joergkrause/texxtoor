using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Xml.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Logging;
using Texxtoor.BaseLibrary.Services;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Marketing;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.ViewModels.Author;
using Texxtoor.ViewModels.Users;

namespace Texxtoor.BusinessLayer {

  public class UserManager : Manager<UserManager> {


    # region Register And Logon

    public int MinPasswordLength {
      get { return Int32.Parse(WebConfigurationManager.AppSettings["accounts:MinRequiredPasswordLength"]); }
    }

    public async Task<bool> ValidateUser(string userName, string password) {
      if (String.IsNullOrEmpty(userName)) throw new ArgumentException(ControllerResources.UserManager_ValidateUser_Value_cannot_be_null_or_empty_, "userName");
      if (String.IsNullOrEmpty(password)) throw new ArgumentException(ControllerResources.UserManager_ValidateUser_Value_cannot_be_null_or_empty_, "password");
      var user = GetUserByName(userName);
      if (user == null) return false;
      if (String.IsNullOrEmpty(user.PasswordHash)) {
        // assure backward compatibility to old password schema
        user.PasswordHash = user.Password;
      }
      return await Usermanager.CheckPasswordAsync(user, password);
    }

    private void AssignUserToApp(string userName, Application application) {
      var user = GetUserByName(userName);
      var app = Ctx.Applications.Single(a => a.Id == application.Id);
      app.Users.Add(user);
      user.LastActivityDate = DateTime.Now;
      SaveChanges();
    }

    public bool ChangePassword(string userName, string oldPassword, string newPassword) {
      // The underlying ChangePassword() will throw an exception rather
      // than return false in certain failure scenarios.
      try {
        var user = GetUserByName(userName);
        var oldPass = TexxtoorMembershipService.CreateHash(oldPassword, TexxtoorMembershipService.HashAlgorithmType);
        var newPass = TexxtoorMembershipService.CreateHash(newPassword, TexxtoorMembershipService.HashAlgorithmType);
        if (user.Password == oldPass) {
          user.Password = newPass;
          user.PasswordHash = newPass;
          return SaveChanges() == 1;
        }
      } catch (ArgumentException) {
      }
      return false;
    }

    public bool UserIsConfirmed(string userName) {
      return Ctx.Users.Any(u => u.UserName == userName && u.IsApproved);
    }

    public void LogSignIn(string userName, bool createPersistentCookie) {
      if (String.IsNullOrEmpty(userName)) throw new ArgumentException(ControllerResources.UserManager_ValidateUser_Value_cannot_be_null_or_empty_, "userName");
      var user = Ctx.Users.FirstOrDefault(u => u.UserName == userName);
      if (user == null) return;
      user.LastActivityDate = DateTime.Now;
      user.LastLoginDate = DateTime.Now;
      SaveChanges();
    }

    public void SignOut() {
      AuthManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
      // Invalidate the Cache on the Client Side
      HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
      HttpContext.Current.Response.Cache.SetNoStore();
    }

    public string GetPortalName() {
      // extract the subdomain and return as portal name hint
      var s = HttpContext.Current.Request.Url.Host;
      return s == "" ? "WWW" : s.ToUpper();
    }


    # endregion

    public Dictionary<string, List<ContributorMatrix>> GetContributorTypesByMembers(IEnumerable<TeamOverviewModel> members, User user) {
      var contribTypes = Ctx.UserProfiles
        .Include(u => u.User)
        .Include(u => u.ContributorMatrix)
        .ToList()
        .Where(p => members.Any(m => m.Members.Any(tm => tm.Id == user.Id)))
        .Select(p => p)
        .ToDictionary(p => p.User.UserName, p => p.ContributorMatrix);
      return contribTypes;
    }

    public ManagerResults<IEnumerable<EditUser>> SearchUsersInTeam(int teamId, string q)
    {
      var mr = new ManagerResults<IEnumerable<EditUser>>("UserManager");
      var team = Ctx.Teams.Find(teamId);
      if (team == null)
      {
        mr.SetError("Team not found", true);
        return mr;
      }
      var users = team.Members
        .Select(m => m.Member.Profile)
        .Where(up => up.ContributorMatrix.Any() && (up.User.UserName.Contains(q) || up.User.Email.Contains(q)))
        .Select(u => new EditUser {
          id = u.User.Id,
          name = u.User.UserName
        });
      mr.ViewModel = users;
      return mr;
    }

    /// <summary>
    /// Search users that are NOT already part of the given team.
    /// </summary>
    /// <param name="teamId"></param>
    /// <param name="q"></param>
    /// <returns></returns>
    public ManagerResults<IEnumerable<EditUser>> SearchUsersForTeam(int teamId, string q) {
      var mr = new ManagerResults<IEnumerable<EditUser>>("UserManager");
      var team = Ctx.Teams.Find(teamId);
      if (team == null) {
        mr.SetError("Team not found", true);
        return mr;
      }
      var users = Ctx.UserProfiles
        .Include(up => up.ContributorMatrix)
        .Where(up => up.ContributorMatrix.Any() && (up.User.UserName.Contains(q) || up.User.Email.Contains(q)))
        .ToList()
        .Where(up => team.Members.All(t => t.Member.UserName != up.User.UserName))
        .Select(u => new EditUser {
          id = u.User.Id,
          name = u.User.UserName
        });
      mr.ViewModel = users;
      return mr;
    }

    /// <summary>
    /// Return all users with a given minimum reputation.
    /// </summary>
    /// <param name="q">Search query</param>
    /// <param name="minRep">minimum CreatorScore</param>
    /// <returns></returns>
    public IEnumerable<EditUser> SearchUsersForReputation(string q, int minRep) {
      var users = Ctx.UserProfiles
        .Where(up => up.User.UserName.Contains(q) || up.User.Email.Contains(q))
        .ToList()
        .Where(up => up.CreatorScore > minRep)
        .Select(u => new EditUser {
          id = u.User.Id,
          name = u.User.UserName
        });
      return users;
    }

    public IEnumerable<EditUser> SearchUsersTopForReputation(int minRep, int take) {
      var users = Ctx.UserProfiles
        .Include(up => up.User)
        .Include(up => up.User.Published)
        .Where(up => up.User.Published.Any())
        .ToList()
        .Where(up => up.CreatorScore > minRep)
        .OrderByDescending(up => up.CreatorScore)
        .Take(take)
        .Select(u => new EditUser {
          id = u.User.Id,
          name = u.User.UserName
        });
      return users;
    }

    /// <summary>
    /// Get all members in the portal without those already member of the given group.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="q"></param>
    /// <returns></returns>
    public IEnumerable<EditUser> SearchUsersForGroup(int groupId, string q) {
      var group = Ctx.ReaderGroups.Find(groupId);
      var groupMembers = group == null || group.Members == null ? new List<User>() : group.Members;
      var users = Ctx.Users
        .Where(up => up.UserName.Contains(q) || up.Email.Contains(q))
        .ToList()
        .Except(groupMembers)
        .Select(u => new EditUser {
          id = u.Id,
          name = u.UserName
        });
      return users;
    }


    public List<User> GetUsersByRole(UserRole role) {
      var users = Ctx.UserProfiles
        .Where(u => u.ContributorMatrix.Any(ct => ct.MinimumRole == role))
        .Select(u => u.User);
      return users.ToList();
    }

    public IEnumerable<User> GetUsersByName(string q, string userName) {
      var user = GetCurrentUser(userName);
      return Ctx.Users
        .ToList()
        .Where(u => u.LoweredUserName.Contains(q.ToLower()))
        .Except(new[] { user });
    }

    /// <summary>
    /// Return exactly one user.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public User GetUserByName(string userName) {
      userName = userName.ToLower();
      return Ctx.Users.SingleOrDefault(u => u.UserName.ToLower() == userName); // no provider == texxtoor
    }


    public User GetUserByName(string userName, string provider, string oAuthProviderUserId) {
      userName = String.Format("{0}|{1}", userName.ToLower(), provider);
      return Ctx.Users.SingleOrDefault(u => u.UserName.ToLower() == userName && u.OAuthProvider == provider && u.OAuthUserId == oAuthProviderUserId);
    }


    public User GetUser(int cid) {
      return Ctx.Users.Find(cid);
    }

    # region Mail

    public void SendExternalMail(User from, User to, string template, bool withExternalMail, IDictionary<string, object> data) {
      if (from == null) {
        // System Mail
        from = Ctx.Users.Single(u => u.UserName == "Admin");
      }
      var profile = UserProfileManager.Instance.GetProfileByUser(to.Id);
      var l1 = profile.RunControl.NativeName;
      var l2 = profile.RunControl.TwoLetterISO;
      var l3 = "en";
      // Load Mail Template Group
      var tpl = Ctx.TemplateGroups.FirstOrDefault(t => t.Group == GroupKind.Email && (t.LocaleId == l1 || t.LocaleId == l2 || t.LocaleId == l3));
      if (tpl == null) throw new ArgumentOutOfRangeException("template");
      // Get specific template
      var tp = tpl.Templates.Single(t => t.InternalName == template);
      // Replace all variables
      var content = data.Aggregate(Encoding.UTF8.GetString(tp.Content), (current, o) => current.Replace(String.Concat("{", o.Key, "}"), o.Value.ToString()));
      // make an XML
      using (var sr = new StringReader(String.Format("<mail>{0}</mail>", content))) {
        var mail = XDocument.Load(sr);
        var body = mail.Root.Element("body").Value;
        var subject = mail.Root.Element("subject").Value;
        // either forced by caller or user want's all notifications by email
        if (withExternalMail || profile.Newsletter.GetValueOrDefault()) {
          // Send
          try {
            var message = new MailMessage();
            message.To.Add(to.Email);
            message.Subject = subject;
            message.From = new MailAddress(from.Email);
            message.Body = body;
            var smtp = new SmtpClient();
            smtp.Send(message);
          } catch (Exception ex) {
            Trace.TraceError(ex.Message);
          }
        }
        // always make it an internal message
        Ctx.Messages.Add(new Message {
          Sender = from,
          Receiver = to,
          Subject = subject,
          Body = body
        });
      }
      SaveChanges();
    }

    public void AddAwardState(int userId, int awardValue) {
      Ctx.Users.Find(userId).Profile.AwardState += awardValue;
      SaveChanges();
    }

    /// <summary>
    /// Adds a message for a number of recepients
    /// </summary>
    /// <param name="receiverIds">Recipients' id from User table), as comma separated list</param>
    /// <param name="msg">Message body and subject, receiver and sender can be empty</param>
    /// <param name="senderName">Sender (as userName)</param>
    public void AddMessage(IEnumerable<int> receiverIds, Message msg, string senderName) {
      var user = GetCurrentUser(senderName);
      receiverIds.ForEach(id => {
        var message = new Message {
          Sender = user,
          Receiver = Ctx.Users.FirstOrDefault(u => u.Id == id),
          Subject = msg.Subject,
          Body = msg.Body
        };
        Ctx.Messages.Add(message);
        // TODO: Send External Mail if required
      });
      Ctx.SaveChanges();
    }

    public Message GetMessageForUser(int id, string userName) {
      return Ctx.Messages // null check for system replies
        .FirstOrDefault(m => m.Id == id && (m.Receiver == null || m.Receiver.UserName == userName || m.Sender == null || m.Sender.UserName == userName));
    }

    public bool DeleteMessage(int id, string userName) {
      var msg = Ctx.Messages.Find(id);
      // as we currently don't copy sent messages receiver or sender can delete. That means that if a sender deletes a message it will magically disappear for the receiver as well.
      if (msg != null && (msg.Receiver.UserName == userName || msg.Sender.UserName == userName)) {
        Ctx.Messages.Remove(msg);
        return SaveChanges() >= 1;
      }
      return false;
    }

    public IEnumerable<Message> GetMessagesByFilter(string filter, string userName) {
      IEnumerable<Message> msgs = null;
      var user = GetCurrentUser(userName);
      switch (filter) {
        case "In":
          msgs = Ctx.Messages
            .Where(msg => msg.Receiver == null || msg.Receiver.Id == user.Id)
            .OrderByDescending(msg => msg.CreatedAt)
            .ToList();
          break;
        case "Out":
          msgs = Ctx.Messages
            .Where(msg => msg.Receiver.Id != user.Id && msg.Sender.Id == user.Id)
            .OrderByDescending(msg => msg.CreatedAt)
            .ToList();
          break;
      }
      return msgs;
    }


    # endregion

    public void ClickCount(int clickValue, ClickSourceType sourceType, string action, string controller) {
      var userName = GetCurrentUserName();
      var user = GetCurrentUser(userName);
      user.LastActivityDate = DateTime.Now;
      var click = new ClickCount {
        User = user,
        SourceType = sourceType,
        Value = clickValue,
        Action = action,
        Controller = controller
      };
      Ctx.ClickCount.Add(click);
      SaveChanges();
    }

    public void GatherUserActivity(int operationValue, string operationReason, string userName) {
      var user = GetCurrentUser(userName);
      var activity = new UserActivity {
        ActivityUser = user,
        OperationReason = operationReason,
        OperationValue = operationValue
      };
      Ctx.UserActivities.Add(activity);
      SaveChanges();
    }

    public bool ResendActivationMail(ResendMail model) {
      var user = GetUserByName(model.UserName) ?? Ctx.Users.SingleOrDefault(u => u.Email == model.UserName);
      if (user != null) {
        user.LastActivityDate = DateTime.Now;
        SaveChanges();
        SendExternalMail(null, user, "WelcomeUser",
          true,
          new Dictionary<string, object>() {
          {"UserName", user.UserName},
          {"ConfirmLink", String.Format("http://{1}/Account/ConfirmMail/{0}", user.PasswordSalt, HttpContext.Current.Request.Url.Authority)}
        });
        return true;
      }
      return false;
    }

    public bool ConfirmMail(string registerId) {
      // Check in Database for id
      // if exists and user is not already activate then activate
      var user = Ctx.Users.FirstOrDefault(u => u.PasswordSalt == registerId);
      if (user == null) return false;
      user.IsApproved = true;
      user.IsAnonymous = false;
      user.LastActivityDate = DateTime.Now;
      SaveChanges();
      LogSignIn(user.UserName, false);
      return true;
    }

    public void RegisterFailedAttempt(string userName) {
      var user = Ctx.Users.FirstOrDefault(u => u.UserName == userName);
      if (user == null) return;
      if (user.FailedPasswordAttemptWindowStart.AddMinutes(60) >= DateTime.Now) {
        user.FailedPasswordAttemptCount = 0;
      } else {
        user.FailedPasswordAttemptCount = user.FailedPasswordAttemptCount + 1;
      }
      user.FailedPasswordAttemptWindowStart = DateTime.Now;
      user.LastActivityDate = DateTime.Now;
      SaveChanges();
    }

    public void RegisterFailedAnswer(string userName) {
      var user = Ctx.Users.FirstOrDefault(u => u.UserName == userName);
      if (user == null) return;
      if (user.FailedPasswordAnswerAttemptWindowStart.AddMinutes(60) >= DateTime.Now) {
        user.FailedPasswordAnswerAttemptCount = 0;
      } else {
        user.FailedPasswordAnswerAttemptCount = user.FailedPasswordAnswerAttemptCount + 1;
      }
      user.FailedPasswordAnswerAttemptWindowStart = DateTime.Now;
      user.LastActivityDate = DateTime.Now;
      SaveChanges();
    }


    public bool RetrievePassword(string userName, string answer) {
      var user = Ctx.Users.SingleOrDefault(u => u.UserName == userName);
      if (user == null) {
        user = Ctx.Users.SingleOrDefault(u => u.Email == userName);
        if (user == null) return false;
      }
      if (user.PasswordAnswer != null && user.PasswordAnswer != answer.Trim()) return false;
      var password = new Random().Next(100000).ToString();
      user.Password = TexxtoorMembershipService.CreateHash(password, TexxtoorMembershipService.HashAlgorithmType);
      var data = new Dictionary<string, object> { { "UserName", user.UserName }, { "Password", password } };
      SendExternalMail(null, user, "RetrievePassword", true, data);
      SaveChanges();
      return true;
    }


    public void DeactivateUser(int userId) {
      var user = Ctx.Users.Find(userId);
      if (user != null) {
        user.IsLockedOut = true;
        SaveChanges();
      }
    }

    /// <summary>
    /// Raise or lower the complexity level of the UI by partially removing menu items. It does not shrink the ability to do things.
    /// </summary>
    /// <param name="raise"></param>
    /// <param name="userName"></param>
    public void RaiseModeToFull(bool raise, string userName) {
      var user = GetUserByName(userName);
      var profile = user.Profile;
      if (profile == null) {
        profile = new UserProfile {
          RunControl = new RunControl {
            Complexity = Complexity.Full,
            RunMode = RunMode.Texxtoor,
            UiLanguage = CurrentCulture
          }
        };
        user.Profile = profile;
        SaveChanges();
      }
      Ctx.Entry(profile).Property("RunControl.Complexity").CurrentValue = (raise ? Complexity.Full : Complexity.Simple);
      Ctx.Entry(profile).Property("RunControl.Complexity").IsModified = true;
      SaveChanges();
    }

    public bool AskReputationResponse(int id) {
      return false;
    }


    public IQueryable<User> GetActiveUsers() {
      var now = DateTime.Now;
      return Ctx.Users
        .Where(u => u.LastActivityDate != DateTime.MinValue)
        .Where(u => EntityFunctions.DiffMinutes(u.LastActivityDate, now) < 3);
    }

    public User GetUserByEmail(string email) {
      email = email.ToLower();
      return Ctx.Users.FirstOrDefault(u => u.Email.ToLower() == email);
    }

    public bool IsExistingEmail(string email) {
      var user = Ctx.Users.Any(u => u.Email.ToLower() == email);
      return user;
    }

    public IQueryable<User> GetExternalUsers(string email) {

      return Ctx.Users
        .Where(u => u.UserName.Contains(email));

    }


    private void SendAssociationMail(User user) {
      SendExternalMail(null, user, "ConfirmAssociation",
        true,
        new Dictionary<string, object>() {
          {"UserName", user.UserName},
          {"ConfirmCode", user.LinkSalt}
        });
    }

    public void DisconnectUser(string providerId, string provider) {
      var user = Ctx.Users.FirstOrDefault(u => u.OAuthUserId == providerId && u.OAuthProvider == provider);
      if (user != null) {
        user.LeadingAccountId = null;
        SaveChanges();
      }
    }


    public bool IsConnectedUser(string userName) {
      return Ctx.Users.Any(u => u.UserName.Equals(userName) && (u.OAuthUserId == null || u.OAuthUserId == "" || u.LeadingAccountId != null));
    }

    public bool CheckEncryptedPassword(string transferHash) {
      transferHash = String.Join("", transferHash.Reverse());
      return Ctx.Users.SingleOrDefault(u => u.Password == transferHash) != null;
    }

    public IAuthenticationManager AuthManager {
      get {
        return HttpContext.Current.GetOwinContext().Authentication;
      }
    }

    public async Task SignInAsync(User user, bool isPersistent) {
      AuthManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
      ClaimsIdentity identity = null;
      try {
        identity = await Usermanager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);

      } catch (Exception) {
      }
      if (identity != null)
      {
        AuthManager.SignIn(new AuthenticationProperties() {IsPersistent = isPersistent}, identity);
      }
    }



    public IList<string> GetRolesForUser(IIdentity identity) {
      var userName = identity.GetUserName();
      var user = Usermanager.Users.SingleOrDefault(u => u.UserName == userName);
      if (user != null) {
        var userRoles = user.Roles;
        return Rolemanager.Roles
          .ToList()
          .Where(r => userRoles.Any(ur => ur.RoleId == r.Id))
          .Select(r => r.Name).ToList();
      }
      return new List<string>();
    }
  }
}