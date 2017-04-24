using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Texxtoor.BaseLibrary.Core.Notifications;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.ViewModels.Author;
using Texxtoor.ViewModels.Users;

namespace Texxtoor.BusinessLayer {

  public class UserProfileManager : Manager<UserProfileManager> {

    # region Common Data

    public SelectList GetGenderList(UserProfile profile = null) {
      var c = HttpContext.Current.Cache;
      if (c["GenderList"] != null) {
        return ((SelectList)c["GenderList"]);
      }
      Func<string, string> localizeEnum = item => {
        var attr =
          typeof(Gender).GetField(item)
                         .GetCustomAttributes(typeof(DisplayAttribute), false)
                         .Cast<DisplayAttribute>()
                         .FirstOrDefault();
        return attr == null
                 ? item.ToString(CultureInfo.InvariantCulture)
                 : attr.ResourceType.GetProperty(attr.Name).GetValue(null).ToString();
      };
      var genders = (from item in Enum.GetNames(typeof(Gender))
                     let gender = (Gender)Enum.Parse(typeof(Gender), item)
                     select new SelectListItem {
                       Text = localizeEnum(item),
                       Value = ((int)Enum.Parse(typeof(Gender), item)).ToString(CultureInfo.InvariantCulture),
                       Selected = (profile != null && profile.Gender == gender)
                     }).ToList();
      var s = new SelectList(genders, "Value", "Text");
      c["GenderList"] = s;
      return s;
    }

    # endregion

    # region User Specific Data

    public ContributorMatrixHelper[] GetContributorTypesExceptCurrent(int userProfileId, string q) {
      var current = GetCurrentContributorMatrix(userProfileId);
      var result = Ctx.ContributorMatrix
        .ToList()
        .Except(current)
        .Where(c => c.Name.ToLower().Contains(q.ToLower()))
        .Select(c => new ContributorMatrixHelper {
          id = c.Id,
          name = String.Format("{0} ({1})", c.Name, ParentLanguageNameForCulture(c.LocaleId))
        }).ToArray();
      return result;
    }

    public ContributorMatrixHelper[] GetContributorMatrix(int userProfileId) {
      var result = GetCurrentContributorMatrix(userProfileId)
        .Select(c => new ContributorMatrixHelper {
          id = c.Id,
          name = String.Format("{0} ({1})", c.Name, ParentLanguageNameForCulture(c.LocaleId))
        }).ToArray();
      return result;
    }

    public ContributorMatrixHelper[] GetContributorMatrixByName(string name) {
      var result = Ctx.ContributorMatrix
        .Where(c => c.Name.Contains(name))
        .ToList()
        .Select(c => new ContributorMatrixHelper {
          id = c.Id,
          name = String.Format("{0} ({1})", c.Name, ParentLanguageNameForCulture(c.LocaleId))
        }).ToArray();
      return result;
    }

    public Dictionary<string, List<ContributorMatrix>> GetContributorMatrixOfProjectTeam(Project prj) {
      var teamMember = prj.Team.Members.Select(t => t.Member).ToList();
      var contribTypes = Ctx.UserProfiles
        .Include(p => p.User)
        .Include(p => p.ContributorMatrix)
        .ToList()
        .Where(p => teamMember.Any(m => m.Id == p.User.Id))
        .Select(p => p)
        .ToDictionary(p => p.User.UserName, p => p.ContributorMatrix);
      return contribTypes;
    }

    private IEnumerable<ContributorMatrix> GetCurrentContributorMatrix(int userProfileId) {
      return Ctx.UserProfiles
        .Include(p => p.ContributorMatrix)
        .First(c => c.Id == userProfileId)
        .ContributorMatrix
        .ToList();
    }

    private static string ParentLanguageNameForCulture(string localeId) {
      Func<CultureInfo, CultureInfo> p = null;
      p = ci => (!Equals(ci.Parent, CultureInfo.InvariantCulture)) ? p(ci.Parent) : ci;
      return p(CultureInfo.CreateSpecificCulture(localeId)).NativeName;
    }

    # endregion

    /// <summary>
    /// Get profile for user.
    /// </summary>
    /// <param name="userId">User Id</param>
    /// <returns></returns>
    public UserProfile GetProfileByUser(int userId) {
      return Ctx.UserProfiles
        .Include(u => u.Addresses)
        .FirstOrDefault(p => p.User.Id == userId);
    }

    public UserProfile GetProfileByUser(string userName) {
      var up = Ctx.UserProfiles
        .Include(u => u.Addresses)
        .Include(u => u.User)
        .SingleOrDefault(p => p.User.UserName == userName);
      if (up == null) {
        var user = GetCurrentUser(userName);
        if (user != null) {
          var rc = HttpContext.Current.Session["RunControl"] as RunControl;
          up = new UserProfile {Addresses = new List<AddressBook>(), User = user, PayPalUserId = user.Email};
          up.RunControl = new RunControl {
            Complexity = rc.Complexity,
            Favorites = rc.Favorites,
            RunMode = rc.RunMode,
            UiLanguage = rc.UiLanguage
          };
          Ctx.UserProfiles.Add(up);
          SaveChanges();
        }
        else {
          throw new ArgumentException("Invalid user name or account state: " + userName, "userName");
        }
      }
      return up;
    }

    public UserProfile GetProfile(int profileId, params Expression<Func<UserProfile, object>>[] expression) {
      var up = Ctx.UserProfiles
        .Include(u => u.User)
        .SingleOrDefault(u => u.Id == profileId);
      if (up == null) {
        var userName = GetCurrentUserName();
        var user = GetCurrentUser(userName);
        if (user != null) {
          up = new UserProfile { Addresses = new List<AddressBook>(), User = user, PayPalUserId = user.Email };
          Ctx.UserProfiles.Add(up);
          SaveChanges();
        } else {
          throw new ArgumentException("Invalid user name or account state: " + userName, "userName");
        }
      }
      if (String.IsNullOrEmpty(up.Walltext)) {
        up.Walltext = ControllerResources.UserManager_GetProfile_User_has_no_wall_profile;
      }
      if (expression != null) {
        foreach (var expr in expression) {
          Ctx.LoadProperty(up, expr);
        }
      }
      return up;
    }

    public void AddConsumerMatrixToProfile(int profileId) {
      var p = GetProfile(profileId);
      p.ConsumerMatrix.Add(new ConsumerMatrix {
        Keyword = ControllerResources.UserManager_AddConsumerMatrixToProfile_New,
        Stage = StageType.Beginner,
        Target = TargetType.Professional,
        Temporary = false,
        Profile = p
      });
      SaveChanges();
    }

    public void AddContributorMatrixToProfile(int profileId) {
      var p = GetProfile(profileId);
      var localizedName = typeof(ContributorRole).GetField(ContributorRole.Author.ToString())
                                  .GetCustomAttributes(typeof(DisplayAttribute), true)
                                  .Cast<DisplayAttribute>()
                                  .Single()
                                  .GetName();
      p.ContributorMatrix.Add(new ContributorMatrix {
        Name = localizedName,
        Description = ControllerResources.UserManager_AddContributorMatrixToProfile_I_can_write,
        MinimumRole = UserRole.Contributor,
        LocaleId = CurrentCulture,
        ContributorRole = ContributorRole.Author,
        Profile = p
      });
      SaveChanges();
    }

    public void AddLanguageMatrixToProfile(int profileId) {
      var p = GetProfile(profileId);
      p.LanguageMatrix.Add(new LanguageMatrix { LocaleId = CurrentCulture, LanguageLevel = LanguageLevel.Native, Profile = p });
      SaveChanges();
    }

    public void RemoveLanguageMatrixToProfile(int matrixId) {
      var p = Ctx.LanguageMatrix.Find(matrixId);
      Ctx.LanguageMatrix.Remove(p);
      SaveChanges();
    }

    public void RemoveConsumerMatrixToProfile(int id, int matrixId) {
      var cm = Ctx.ConsumerMatrix.FirstOrDefault(m => m.Profile.Id == id && m.Id == matrixId);
      if (cm != null) {
        Ctx.ConsumerMatrix.Remove(cm);
        SaveChanges();
      }
    }

    public void RemoveContributorMatrixToProfile(int matrixId) {
      var p = Ctx.ContributorMatrix.Find(matrixId);
      Ctx.ContributorMatrix.Remove(p);
      SaveChanges();
    }

    /// <summary>
    /// Get all addresses for user
    /// </summary>
    /// <returns></returns>
    public IList<AddressBook> GetAllAddressedForUser(int userId) {
      var adr = GetProfileByUser(userId).Addresses.ToList().Where(a => a.GetType() == typeof(AddressBook)); // base only, inherited types are non-changable by user
      return adr.ToList();
    }

    public AddressBook GetAddress(int addressId) {
      return Ctx.AddressBook.Find(addressId);
    }

    public void DeleteAddress(int id, string userName) {
      var addr = Ctx.AddressBook.FirstOrDefault(a => a.Id == id && a.Profile.User.UserName == userName);
      if (addr == null) return;
      Ctx.AddressBook.Remove(addr);
      SaveChanges();
    }

    public AddressBook GetInvoiceAddressForUser(string userName) {
      var profile = GetProfileByUser(userName);
      var address = profile.Addresses.FirstOrDefault(a => a.Invoice);
      if (address == null) {
        // no invoice address, so we make the first to those
        address = profile.Addresses.FirstOrDefault();
        if (address != null) {
          address.Invoice = true;
          SaveChanges();
        }
      }
      return address;
    }

    public AddressBook SetOrAddAddressForUser(AddressBook newAddress, int countryId, string region, string userName) {
      var address = Ctx.AddressBook.FirstOrDefault(a => a.Profile.User.UserName == userName && a.Id == newAddress.Id);
      var cntry = Ctx.Countries.SingleOrDefault(c => c.Id == countryId);
      var profile = Ctx.UserProfiles.Include(p => p.Addresses).Single(p => p.User.UserName == userName);
      if (address == null) {
        newAddress.Profile = profile;
        newAddress.Country = cntry.Name;
        newAddress.Region = region;
        Ctx.AddressBook.Add(newAddress);
        address = newAddress;
      } else {
        newAddress.CopyProperties<AddressBook>(address,
          a => a.Name,
          a => a.Default,
          a => a.Invoice,
          a => a.City,
          a => a.Region,
          a => a.StreetNumber,
          a => a.Zip);
        address.Country = cntry.Name;
        // we can have only one address as invoice and only one as default
        if (profile.Addresses != null && profile.Addresses.Any()) {
          if (newAddress.Invoice) {
            profile.Addresses.Except(new[] {address}).ForEach(a => a.Invoice = false);
          } else {
            // if no invoice take first as invoice
            if (!profile.Addresses.Any(a => a.Invoice)) {
              profile.Addresses.First().Invoice = true;
            }
          }
          if (newAddress.Default) {
            profile.Addresses.Except(new[] {address}).ForEach(a => a.Default = false);
          }
          else {
            // if no default take first as default
            if (!profile.Addresses.Any(a => a.Default)) {
              profile.Addresses.First().Default = true;
            }
          }
        }
      }
      SaveChanges();
      return address;
    }

    public bool SaveProfile(UserProfile profile, int? gender, string userName) {
      var user = Ctx.Users.FirstOrDefault(u => u.UserName == userName);
      if (user == null) return false;
      profile.User = user;
      var newprofile = Ctx.UserProfiles
        .Include(u => u.ConsumerMatrix)
        .Include(u => u.ContributorMatrix)
        .Include(u => u.LanguageMatrix)
        .FirstOrDefault(p => p.User.UserName == userName);
      if (newprofile == null) {
        newprofile = new UserProfile { User = user };
        Ctx.UserProfiles.Add(newprofile);
      }
      profile.CopyProperties<UserProfile>(newprofile,
          m => m.FirstName,
          m => m.MiddleName,
          m => m.LastName,
          m => m.Description,
        // do not copy walltext here as we use editorprofile view for same
          m => m.BirthDay,
          m => m.XingProfile,
          m => m.FacebookProfile,
          m => m.OtherProfile,

          m => m.StatementOfTaste,
          m => m.Notes,
          m => m.Newsletter,

          m => m.ShowWalltextOnApplication,
          m => m.Application,
          m => m.Hidden,
          m => m.AvailabilityContainer,

          m => m.ContractAccepted,
          m => m.SharingAccepted,
          m => m.MinHourlyRate,
          m => m.MaxHourlyRate,
          m => m.PayPalUserId
          );
      newprofile.RunControl = new RunControl {
        UiLanguage = profile.RunControl.UiLanguage,
        Complexity = profile.RunControl.Complexity,
        RunMode = profile.RunControl.RunMode
      };
      newprofile.Gender = (gender.HasValue ? (Gender)gender : Gender.Unknown);
      newprofile.Status = AccountStatus.Active;
      SaveChanges();
      return true;
    }

    public void SaveProfileWalltext(int id, string content, string userName) {
      var model = GetProfileByUser(userName);
      model.Walltext = content;
      // add a bonus for making a profile
      if (model.AwardState < 25) {
        model.AwardState += 20;
      }
      SaveChanges();
    }

    public void AddContactFormRequest(ContactModel model, string userName) {
      var user = UserManager.Instance.GetUserByName(userName) ?? UserManager.Instance.GetUserByName("Admin");
      var cfr = new ContactFormRequest {
        Sender = user,
        Name = model.Name,
        EMail = model.EMail,
        Subject = model.Subject,
        Message = model.Message
      };
      Ctx.ContactFormRequests.Add(cfr);
      SaveChanges();
    }

    public void EditConsumerMatrixToProfile(int id, int matrixId, string keyWord, int targets, int stages) {
      if (String.IsNullOrEmpty(keyWord)) {
        throw new ArgumentOutOfRangeException();
      }
      var matrix = Ctx.ConsumerMatrix
        .Include(u => u.Profile)
        .FirstOrDefault(m => m.Profile.Id == id && m.Id == matrixId);
      if (matrix != null) {
        matrix.Keyword = keyWord;
        matrix.Target = (TargetType)targets;
        matrix.Stage = (StageType)stages;
      }
      SaveChanges();
    }

    public void EditLanguageMatrixToProfile(int id, int matrixId, string language, int levels) {
      var matrix = Ctx.LanguageMatrix
        .Include(u => u.Profile)
        .FirstOrDefault(m => m.Profile.Id == id && m.Id == matrixId);
      if (matrix != null) {
        matrix.LocaleId = language;
        matrix.LanguageLevel = (LanguageLevel)levels;
      }
      SaveChanges();
    }

    public void EditContributorMatrixToProfile(int id, int matrixId, string language, ContributorRole role, string information) {
      var matrix = Ctx.ContributorMatrix
        .Include(u => u.Profile)
        .FirstOrDefault(m => m.Profile.Id == id && m.Id == matrixId);
      if (matrix != null) {
        matrix.LocaleId = language;
        matrix.ContributorRole = role;
        matrix.Description = information;
        matrix.MinimumRole = UserRole.Contributor;
        matrix.Name =
          typeof(ContributorRole).GetField(Enum.GetName(typeof(ContributorRole), role))
                                  .GetCustomAttributes(typeof(DisplayAttribute), true)
                                  .Cast<DisplayAttribute>()
                                  .Single()
                                  .GetName();
      }
      SaveChanges();
    }

    public IEnumerable<Country> GetCountryList() {
      return Ctx.Countries.OrderBy(c => c.LocalName);
    }

    public SelectList GetCountryListForSelect(string model) {
      Func<string, string> encoder = s => {
        var iso = Encoding.GetEncoding("ISO-8859-1");
        var utfBytes = Encoding.UTF8.GetBytes(s);
        var isoBytes = Encoding.Convert(Encoding.UTF8, iso, utfBytes);
        return Encoding.UTF8.GetString(isoBytes);
      };
      var countries = Ctx.Countries.OrderBy(s => s.LocalName).ToList().Select(c => new {
        Id = c.Id,
        LocalName = HttpContext.Current.Server.HtmlDecode(encoder(c.LocalName)),
        Name = c.Name
      });
      // get the selected from name
      var selectedCountry = Ctx.Countries.SingleOrDefault(c => c.Name == model);
      // make this the top item
      countries = countries.OrderBy(s => s.LocalName != "Deutschland");
      return selectedCountry != null ? new SelectList(countries, "Id", "LocalName", selectedCountry.Id) : new SelectList(countries, "Id", "LocalName");
    }

    public void SaveAddress(AddressBook adr, int countryId) {
      var oldAdr = Ctx.AddressBook.Find(adr.Id);
      adr.CopyProperties<AddressBook>(oldAdr,
        a => a.Invoice,
        a => a.Name,
        a => a.Region,
        a => a.StreetNumber,
        a => a.City,
        a => a.Zip,
        a => a.Default);
      oldAdr.Country = Ctx.Countries.Single(c => c.Id == countryId).Name;
      SaveChanges();
    }

    public Gender RemoveProfileImage(int id, string userName) {
      var p = GetProfile(id);
      if (p == null || p.User.UserName != userName) return Gender.Unknown;
      p.Image = null;
      SaveChanges();
      // without an image we create one according to given gender settings
      return p.Gender.GetValueOrDefault();
    }

    public void SaveProfileImage(int id, HttpPostedFileBase file, string userName) {
      var p = GetProfile(id);
      if (p == null || p.User.UserName != userName) return;
      p.Image = file.InputStream.ReadToEnd();
      SaveChanges();
    }

    public HomeScreenInfo GetHomeScreenInfo(string userName) {
      var key = String.Format("HomeScreenInfo-{0}", userName);
      HomeScreenInfo hsi;
      if (HttpContext.Current.Cache["key"] == null) {
        var prjs = ProjectManager.Instance.GetProjectsWhereUserIsMember(userName).Where(p => p.Active).ToList();
        var opus = ProjectManager.Instance.GetAllOpusForUser(userName, false).ToList();
        hsi = new HomeScreenInfo {
          ProjectsCount = prjs.Count(),
          ProjectsEditable = prjs.OrderByDescending(p => p.CreatedAt).Take(10).ToList(),
          BooksEditable = opus.OrderByDescending(p => p.CreatedAt).Take(10).ToList(),
          Authors = Rolemanager.Roles.Any() ? Rolemanager.Roles.ToList().Single(r => r.UserRole == UserRole.Author).Users.Count() : 0, // don't count Admins
          Variations = Ctx.Published.SelectMany(p => p.FrozenFragments).Count(),
          Groups = Ctx.ReaderGroups.Count(),
          Items = Ctx.Bookmarks.Count() + Ctx.Comments.Count() + Ctx.Works.Count(),
          ProjectPublished = prjs.Count(p => p.Opuses.Any(o => o.IsPublished)),
          TeamsLeading = ProjectManager.Instance.GetTeamsWhereUserIsLead(userName).Count(),
          Products = ReaderManager.Instance.GetAllProducts(userName).Count(),
          Works = ReaderManager.Instance.GetWorksForUser(userName).Count(),
          Editables = opus.Count(),
          Memberships = ProjectManager.Instance.GetTeamMemberships(userName).Count(),
          ProfileExists = Ctx.UserProfiles.FirstOrDefault(u => u.User.UserName == userName) == null ? ControllerResources.MenuService_CreatePropertyBag_No_Profile : ControllerResources.MenuService_CreatePropertyBag_Profile_Exists,
          MessageCount = Ctx.Messages.Count(m => m.Receiver.UserName == userName),
          ArchiveCount = Ctx.UserFiles.Count(m => m.Owner.UserName == userName && !m.Deleted),
          OrderCount = Ctx.OrderProducts.Count(m => m.Owner.UserName == userName),
          ProjectsAsLeader = ProjectManager.Instance.GetProjectsTeamLeader(prjs).Count(d => d.Value.UserName == userName),
        };
        hsi.TeamsContributing = ProjectManager.Instance.GetTeamMemberships(userName).Count() - hsi.TeamsLeading;
        hsi.TextsPublishable = Ctx.Published.Include(p => p.FrozenFragments).Count(p => p.Owner.UserName == userName && !p.FrozenFragments.Any());
        hsi.TextsPublished = Ctx.Catalog.Count(c => c.Published.Any(p => p.FrozenFragments.Any() && p.ExternalPublisherProperties != null));
        HttpContext.Current.Cache.Add(key, hsi, null, System.Web.Caching.Cache.NoAbsoluteExpiration,
          TimeSpan.FromMinutes(15), System.Web.Caching.CacheItemPriority.BelowNormal, null);
      } else {
        hsi = (HomeScreenInfo)HttpContext.Current.Cache["key"];
      }
      return hsi;
    }

    /// <summary>
    /// The internal chat and messaging system uses this to refresh the member list.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public UserGroupData GetUserRelations(string userName) {
      if (String.IsNullOrEmpty(userName)) return null;
      var activeUsers = UserManager.Instance.GetActiveUsers().ToList();
      var user = UserManager.Instance.GetUserByName(userName);
      var userList = new List<User>(new[] { user });
      var data = new UserGroupData();
      data.Users.Add("All", new List<ConnectedUser>(activeUsers.Select(u => new ConnectedUser {
        UserId = u.Id,
        UserName = u.UserName,
        IsConnected = true
      })));
      var groups = ReaderManager.Instance.GetGroups(userName);
      foreach (var g in groups) {
        var list = new List<ConnectedUser>(g.Members.Except(userList).Select(u => new ConnectedUser {
          UserId = u.Id,
          UserName = u.UserName,
          IsConnected = activeUsers.Any(a => a.UserName == u.UserName)
        })).ToList();
        if (list.Any()) {
          data.Users.Add(String.Format("Group|{0}|{1}", g.Name, g.Id), list);
        }
      }
      var teams = ProjectManager.Instance.GetTeamsWhereUserIsLead(userName);
      foreach (var t in teams) {
        var list = new List<ConnectedUser>(t.Members.Where(m => userList.All(l => m.Member.UserName != l.UserName)).Select(u => new ConnectedUser {
          UserId = u.Member.Id,
          UserName = u.Member.UserName,
          IsConnected = activeUsers.Any(a => a.UserName == u.Member.UserName)
        })).ToList();
        if (list.Any()) {
          data.Users.Add(String.Format("Team|{0}|{1}", t.Name, t.Id), list);
        }
      }
      data.CurrentUser = userName;
      data.CurrentUserId = user.Id;
      data.Total = data.Users.SelectMany(u => u.Value).Select(u => u.UserName).Distinct().Count();
      data.TotalOnline = data.Users.SelectMany(u => u.Value).Where(u => u.IsConnected).Select(u => u.UserName).Distinct().Count();
      return data;
    }

    public bool AskReputationResponse(int id) {
      return false;
    }


    public void SaveRunControl(int userProfileId, RunControl rm) {
      var profile = GetProfile(userProfileId);
      profile.RunControl = rm;
      SaveChanges();
    }
  }
}
