using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Core.Logging;
using Texxtoor.BaseLibrary.Core.Utilities;
using Texxtoor.BaseLibrary.Core.Utilities.Imaging;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BusinessLayer.Repository.Marketing;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Marketing;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Functions;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.DataModels.ViewModels.Content;
using Texxtoor.ViewModels.Author;
using Texxtoor.ViewModels.Editor;
using Texxtoor.ViewModels.Users;
using Image = System.Drawing.Image;
using Texxtoor.BaseLibrary.Core.HtmlAgility.Pack;
using System.Xml;
using System.Web.Caching;
using Texxtoor.DataModels.Models;
using Texxtoor.BaseLibrary.Core.Utilities.Imaging.Barcode;
using System.Drawing;
using System.Diagnostics;
using System.Net;
using Texxtoor.ViewModels.Content;
using System.Security;

namespace Texxtoor.BusinessLayer {

  /// <summary>
  /// All BLL functions for projects.
  /// </summary>
  /// 
  public partial class ProjectManager {

    public UserRating RateMember(int id, string username, int projectId, int userId, string comment, int? fn, int? co, int? pu, int? qu, int? re) {
      var ratings = Ctx.UserProfiles
        .Include(u => u.Rating)
        .Include(u => u.Rating.Select(r => r.RelatedProject))
        .Single(up => up.User.Id == id)
        .Rating;
      var rating = ratings
        .FirstOrDefault(r => r.RelatedProject.Id == projectId);
      var prj = Instance.GetProject(projectId, username);
      if (rating == null) {
        rating = new UserRating {
          Friendlyness = 5,
          Communication = 5,
          Punctuality = 5,
          Quality = 5,
          Reliability = 5,
          Comment = comment,
          RelatedProject = prj
        };
        ratings.Add(rating);
      }
      rating.Friendlyness = fn ?? 0;
      rating.Communication = co ?? 0;
      rating.Punctuality = pu ?? 0;
      rating.Quality = qu ?? 0;
      rating.Reliability = re ?? 0;
      rating.RelatedProject = prj;
      ratings.Add(rating);
      SaveChanges();
      return rating;
    }

    public MarketingPackage SetMarketingPackage(Project prj, int marketingId, bool unassign) {
      return MarketingPackageRepository.Instance.SetMarketingPackage(prj, marketingId, unassign);
    }

    public IEnumerable<AverageUserRating> GetRatingsForUser(int userId) {
      var rating = Ctx.UserProfiles
        .Include(u => u.Rating)
        .First(u => u.User.Id == userId)
        .Rating
        .GroupBy(r => r.RelatedProject)
        .Select(ur => new AverageUserRating {
          ProjectName = ur.Key.Name,
          Friendlyness = ur.Average(s => s.Friendlyness),
          Punctuality = ur.Average(s => s.Punctuality),
          Communication = ur.Average(s => s.Communication),
          Quality = ur.Average(s => s.Quality),
          Reliability = ur.Average(s => s.Reliability)
        });
      return rating;
    }

    public IEnumerable<WorkitemChat> GetWorkItemsForSnippet(int snippetId) {
      return Ctx.WorkitemChats.Where(w => w.Snippet.Id == snippetId);
    }

    public Opus GetOpusFromSnippetId(int id) {
      var snippet = Ctx.Elements.Find(id);
      Func<Element, Element> findParent = null;
      findParent = e => (e.Parent is Opus) ? e.Parent : findParent(e.Parent);
      return findParent(snippet) as Opus;
    }

    public ManagerResult AddCommentForWorkitem(int id, string comment, string userName, bool @private) {
      var mr = new ManagerResult("ProjectManager");
      try {
        var wi = GetWorkItemsForSnippet(id).ToList();
        var orderNr = (!wi.Any()) ? 1 : wi.Max(w => w.OrderNr) + 1;
        var c = new WorkitemChat {
          Content = comment,
          Owner = GetCurrentUser(userName),
          Snippet = Ctx.Elements.Find(id) as Snippet,
          Closed = false,
          Private = @private,
          Subject = ControllerResources.ProjectManager_AddCommentForWorkitem_WorkItem_Comment,
          OrderNr = orderNr,
          Name = ControllerResources.ProjectManager_AddCommentForWorkitem_WorkItem_Name,
          GroupOnly = true
        };
        // TODO Return Comment View Model
        Ctx.WorkitemChats.Add(c);
        if (SaveChanges() == 1) {
          mr.SetInformation("OK");
        } else {
          mr.SetError("Not added", true);
        }
      } catch (Exception ex) {
        mr.InnerException = ex;
      }
      return mr;
    }

    public List<Work> GetPublishedWorks() {
      var works = Ctx.Works
        .Where(w => w.ExternalBook == null && w.Owner == null) // IsPrivate == false
        .ToList();
      return works;
    }

    internal Opus GetOpusInternal(int opusId, params Expression<Func<Opus, object>>[] loadProperties) {
      var opus = Ctx.Opuses.Find(opusId);
      return opus;
    }


    public void PrepareOpusForPublish(Opus opus) {
      // before we publish & freeze we re-arrange the order numbers as the editor might leave gaps
      PrepareOpusForPublishInternal(opus.Children.OfType<Section>());
    }

    private void PrepareOpusForPublishInternal(IEnumerable<Section> children) {
      // the overall order determines the appearance of sections
      children.OrderBy(c => c.OrderNr);
      var orderNr = 1;
      foreach (var child in children) {
        // we set a separate counter to simplify the section numbering
        child.Counter = orderNr++;
        if (child.HasChildren()) {
          PrepareOpusForPublishInternal(child.Children.OfType<Section>());
        }
      }
      SaveChanges();
    }

    public Opus GetOpus(int opusId, string userName, params Expression<Func<Opus, object>>[] loadProperties) {
      var opus = Ctx.Opuses
        .Include(o => o.Project)
        .Include(o => o.Project.Resources)
        .Include(o => o.Published)
        .Include(o => o.Project.Team)
        .Include(o => o.Project.Team.Members)
        .Include(o => o.Project.Team.Members.Select(m => m.Member))
        .Single(o => o.Id == opusId);
      var members = opus.Project.Team.Members.ToList();
      if (members.Any(m => m.Member.UserName.ToLower() == userName.ToLower())) {
        foreach (var expression in loadProperties) {
          Ctx.LoadProperty<Opus>(opus, expression);
        }
        return opus;
      } else {
        throw new ArgumentOutOfRangeException(userName, "is not team member, found: " + String.Join(", ", members.Select(m => m.Member.UserName).ToArray()));
      }
    }

    public ManagerResult SetSharesForOpus(int id, IList<string> user, IList<int> ratio, IList<bool> use, IList<ShareType> sharetype) {
      var mr = new ManagerResult("ProjectManager");
      // validate
      if (user.Count != ratio.Count) {
        mr.SetWarning("User Count is not Ratio Count");
        return mr;
      }
      var sum = ratio.Where((t, i) => sharetype[i] == ShareType.Ratio).Sum();
      if (sum != 100) {
        mr.SetWarning("The set ratios exceed 100% altogether. Fix you shares to have 100% totally.");
        return mr;
      }
      try {
        using (var scope = Ctx.BeginTransaction()) {
          // remove all 
          var opus = GetOpusInternal(id);
          var crs = Ctx.ContributorRatios.Where(c => c.Opus.Id == id).ToList();
          foreach (var cr in crs) {
            Ctx.ContributorRatios.Remove(cr);
          }
          SaveChanges();
          // rebuild all
          foreach (var name in user) {
            var idx = user.IndexOf(name);
            var cr = new ContributorRatio {
              Contributor = Ctx.Users.Single(u => u.UserName == name),
              Opus = opus,
              ShareType = sharetype[idx],
              ValueOrRatio = ratio[idx],
              Confirmed = use[idx]
            };
            opus.ContributorRatios.Add(cr);
            // only current user can set itself to confirmed in one single step
            if (UserName == name) {
              // if the user is still pending we force him to confirm the membership as well
              var pendingUser = opus.Project.Team.Members.Single(m => m.Member.UserName == name);
              Ctx.LoadProperty(pendingUser, p => p.Role);
              pendingUser.Pending = false;
            }
          }
          SaveChanges();
          mr.SetInformation(ControllerResources.MarketingController_Saved_Shares);
          scope.Commit();
        }
      } catch (Exception ex) {
        mr.InnerException = ex;
        mr.SetError(ControllerResources.MarketingController_Saved_Shares_Error, true);
      }
      return mr;
    }

    /// <summary>
    /// Load opus and includes the team for same, including Member and Role properties.
    /// </summary>
    /// <param name="opusId"></param>
    /// <returns></returns>
    public Opus GetOpusWithTeam(int opusId) {
      var opus = Ctx.Opuses
        .Include(p => p.Project.Team.Members.Select(m => m.Member))
        .Include(p => p.Project.Team.Members.Select(m => m.Role))
        .FirstOrDefault(o => o.Id == opusId);
      return opus;
    }

    public Published GetPublished(int id, string userName) {
      var pub = Ctx.Published
        .Include(p => p.SourceOpus)
        .Include(p => p.Marketing)
        .Include(p => p.ResourceFiles)
        .Include(p => p.SourceOpus.Project.Marketing)
        .Include(p => p.Owner)
        .Include(p => p.Catalogs)
        .SingleOrDefault(p => p.Id == id && p.Owner.UserName == userName);
      if (pub != null) {
        // we take this over to simplify the process
        pub.Marketing = Ctx.MarketingPackages.Find(pub.SourceOpus.Project.Marketing.Id);
        SaveChanges();
      }
      return pub;
    }

    public Published GetPublishedWithProps(int publishedId, string userName, params Expression<Func<Published, object>>[] properties) {
      var pub = Ctx.Published
        .Include(p => p.SourceOpus.Project.Marketing)
        .FirstOrDefault(p => p.Id == publishedId && p.Owner.UserName == userName);
      if (pub == null) return null;
      if (properties != null) {
        Ctx.LoadProperty(pub, properties);
      }
      if (pub.SourceOpus.Project.Marketing == null) {
        int[] pt = { (int)MarketingPackageType.CreativeCommon, (int)MarketingPackageType.RegularSale };
        var pckg = CreateMarketingPackage(new MarketingPackage {
          Name = ControllerResources.ProjectManager_GetPublishedAndLoad_Default_Package,
          Description = ControllerResources.ProjectManager_GetPublishedAndLoad_Generated_Package,
          AssignIsbn = false,
          AssignIsbnADOI = false,
          BasePrice = 2M,
          CreateRssFeed = true,
          CreateSocialPlatformInstances = false,
          PackageBasePrice = 0M,
          RegisterForLibraries = false,
          ShareContent = true
        }, pt, null, userName);
        pub.SourceOpus.Project.Marketing = pckg;
      }
      // we take this over to simplify the process
      pub.Marketing = Ctx.MarketingPackages.Find(pub.SourceOpus.Project.Marketing.Id);
      SaveChanges();
      return pub;
    }

    public Published GetPublished(int publishedId) {
      var pub = Ctx.Published
        .Include(p => p.SourceOpus)
        .Include(p => p.Owner)
        .Include(p => p.Catalogs)
        .FirstOrDefault(p => p.Id == publishedId);
      return pub;
    }

    public Published GetOrCreatePublished(int opusId, bool isArticle, string userName) {
      using (var scope = Ctx.BeginTransaction()) {
        Action<Published> setBackReferenceForPublish = (p) => {
          if (p.SourceOpus.Project.Published == null) {
            p.SourceOpus.Project.Published = new List<Published>();
          }
          if (!p.SourceOpus.Project.Published.Contains(p))
            p.SourceOpus.Project.Published.Add(p);
          SaveChanges();
        };
        var publ = Ctx.Published
          .Include(p => p.SourceOpus)
          .Include(p => p.SourceOpus.Project.Marketing)
          .FirstOrDefault(p => p.SourceOpus.Id == opusId);
        // check the back reference 
        if (publ != null) {
          setBackReferenceForPublish(publ);
          if (publ.Marketing != null) return publ;
          // in quick and simple mode the user has no way to assign the default package, hence we do it for him
          publ.Marketing = publ.SourceOpus.Project.Marketing;
          SaveChanges();
          scope.Commit();
          return publ;
        }
        var opus = GetOpusWithTeam(opusId);
        var teamLead = MemberIsTeamLead(opus.Project.Team.Id, userName);
        if (!teamLead) {
          throw new ArgumentOutOfRangeException("userName");
        }
        var prj = opus.Project;
        var user = GetCurrentUser(userName);
        // no Published Object, so we create one on the fly
        var articleName =
          String.Format(ControllerResources.ProjectManager_GetOrCreatePublished_Article_Title,
            opus.Name,
            String.Format("{0} {1} {2}", user.Profile.FirstName, user.Profile.MiddleName, user.Profile.LastName),
            DateTime.Now.ToShortDateString());
        //
        publ = new Published {
          SourceOpus = opus,
          Owner = user,
          Title = prj.Name,
          SubTitle = isArticle ? articleName : opus.Name,
          NavLevel = (int)(isArticle ? NavLevel.Document : NavLevel.Chapter),
          Marketing = prj.Marketing,
          Authors = prj.Team.Members.Select(tm => tm.Member).ToList()
        };
        Ctx.Published.Add(publ);
        SaveChanges();
        setBackReferenceForPublish(publ);
        scope.Commit();
        return publ;
      }
    }

    /// <summary>
    /// Seeks a package. If it does not exist create a new, default one and add and return this.
    /// </summary>
    /// <param name="prj"></param>
    /// <param name="marketingId">Provide 0 to force creation of new package.</param>
    /// <returns></returns>
    public MarketingPackage GetAndAddMarketingPackage(Project prj, int marketingId = 0) {
      var pckg = Ctx.MarketingPackages.Find(marketingId);
      if (pckg == null) {
        pckg = new MarketingPackage {
          AssignIsbn = false,
          AssignIsbnADOI = false,
          BasePrice = 0M,
          CreateRssFeed = true,
          CreateSocialPlatformInstances = true,
          Description = "Default Package",
          MarketingType = MarketingPackageType.CreativeCommon,
          Name = "Default Package",
          Owner = prj.Team.TeamLead,
          PackageBasePrice = 0M,
          RegisterForLibraries = false,
          ShareContent = true
        };
        Ctx.MarketingPackages.Add(pckg);
        SaveChanges();
      }
      prj.Marketing = pckg;
      SaveChanges();
      return pckg;
    }

    public IEnumerable<MarketingPackage> GetMarketingPackages(string userName) {
      return Ctx.MarketingPackages.Where(m => m.Owner.UserName == userName).OrderByDescending(p => p.CreatedAt);
    }

    public IEnumerable<Published> GetAllPublished(int projectId, string userName) {
      var publ = Ctx.Published
        .Include(p => p.Marketing)
        .Include(p => p.SourceOpus)
        .Include(p => p.FrozenFragments)
        .Where(p => p.SourceOpus.Project.Id == projectId && p.Owner.UserName == userName)
        .AsEnumerable();
      return publ;
    }

    public IList<Catalog> GetCatalogForLanguage(string currentCulture, string filter = "") {
      return Ctx.Catalog
        .Where(c => c.LocaleId == currentCulture && c.Parent == null)
        .OrderBy(c => c.Name)
        .ToList()
        .Where(c => c.Name.Contains(filter) || filter == "")
        .ToList();
    }

    public bool DeleteMilestone(int id, string userName) {
      var mstn = GetMilestone(id);
      if (mstn == null) return false;
      var opus = mstn.Opus;
      // only team lead can remove
      var teamLeader = GetTeamLeader(opus.Project.Team.Id);
      if (teamLeader.UserName != userName) return false;
      // remove dependencies before removing the milestone
      var dependend = Ctx.Milestones.Where((m => m.NextMilestone.Id == mstn.Id)).ToList();
      if (dependend.Any()) {
        dependend.ForEach(m => m.NextMilestone = null);
        SaveChanges();
      }
      // now we can savely remove the milestone
      Ctx.Milestones.Remove(mstn);
      SaveChanges();
      return true;
    }

    public void CreateMilestone(int opusId, Milestone mstn, int nextId, int ownerId) {
      var opus = GetOpusInternal(opusId);
      mstn.Opus = opus;
      mstn.DateAssigned = DateTime.Now;
      mstn.Owner = GetTeamMembersOfOpus(opus).Single(u => u.Id == ownerId);
      // TODO: Sort others accodingly
      mstn.NextMilestone = Ctx.Milestones.Find(nextId);
      Ctx.Milestones.Add(mstn);
      SaveChanges();
    }

    public void EditMilestone(int id, Milestone mstn, int nextId, int ownerId) {
      var ms = GetMilestone(id);
      mstn.CopyProperties<Milestone>(ms,
        m => m.Name,
        m => m.Description,
        m => m.DateDue
        );
      ms.NextMilestone = Ctx.Milestones.Find(nextId);
      var oldOwnerId = ms.Owner.Id;
      ms.Owner = GetTeamMembersOfOpus(ms.Opus).Single(u => u.Id == ownerId);
      if (oldOwnerId != ownerId) {
        ms.DateAssigned = DateTime.Now;
      }
      SaveChanges();
    }

    public ContributionProposal GetContributorProposal(int id) {
      var tm = Ctx.TeamMembers
        .Include(t => t.Team)
        .Single(t => t.Id == id);
      var opuses = tm.Team.Projects.SelectMany(p => p.Opuses).ToList();
      var cr = Ctx.ContributorRatios
        .ToList()
        .Where(c => c.Contributor.Id == tm.Member.Id && opuses.Any(o => o.Id == c.Opus.Id))
        .ToDictionary(c => c.Opus, c => c);
      var cp = new ContributionProposal {
        Member = tm,
        PendingProposals = cr
      };
      return cp;
    }

    /// <summary>
    /// Get the team only if the user is the team's lead
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public Team GetTeamForTeamLead(int id, string userName) {
      var team = Ctx.Teams
        .Include(t => t.Members.Select(r => r.Role))
        .Include(t => t.Members.Select(m => m.Member))
        .FirstOrDefault(t => t.Id == id && t.Members.Any(m => m.TeamLead && m.Member.UserName == userName));
      return team;
    }

    /// <summary>
    /// Return a list of users that match the given criteria.
    /// </summary>
    /// <param name="ct"></param>
    /// <param name="avail"></param>
    /// <param name="shared"></param>
    /// <param name="hourly"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public IList<User> GetContributorWithCriteria(ContributorRole ct, bool avail, bool shared, bool hourly, decimal from, decimal to, string order, bool dir) {
      Func<UserProfile, object> orderby = null;
      order = order.ToLowerInvariant();
      orderby = profile => {
        switch (order) {
          case "username":
            return profile.User.UserName;
          case "rating":
            return profile.GlobalRating;
          case "rate":
            return profile.MinHourlyRate;
        }
        return profile.User.UserName;
      };
      var query = Ctx.UserProfiles
        .Include(u => u.User)
        .Include(u => u.ContributorMatrix)
        .Include(u => u.Rating)
        .Where(up =>
          (!up.SharingAccepted.HasValue || up.SharingAccepted.Value == shared || shared == false)
          && (!up.Hidden.HasValue || up.Hidden.Value == false)
          && (!up.ContractAccepted.HasValue || up.ContractAccepted.Value == hourly || hourly == false)
          && (!up.MaxHourlyRate.HasValue || up.MaxHourlyRate <= to || to == 0)
          && (!up.MinHourlyRate.HasValue || up.MinHourlyRate >= from || from == 0)
          && (up.ContributorMatrix.Any(c => c.ContributorRole == ct))
        );
      var users = query
        .ToList()
        // only if check is provided, if none than fine, if any check thoroughly
        .Where(up => !avail || (up.FutureAvailabilities.Any() || !up.Availabilities.Any()))
        .Where(up => up.ContributorMatrix.Any())
        .ToList();
      return dir ? users.OrderByDescending(orderby).Select(up => up.User).ToList() : users.OrderBy(orderby).Select(up => up.User).ToList();
    }

    public Opus EditOpus(int id, string name, int version, string locale, VariationType vt, bool isBoilerplate, string requirements,
      string targetaudience, string proposedoutcome, int? requirementsResource, int? proposedOutcomeResource,
      bool useMilestones, IEnumerable<Milestone> setMilestones) {
      var opus = GetOpusInternal(id);
      var proj = Ctx.Projects.Find(opus.Project.Id);
      opus.Project = proj;
      opus.Name = name;
      opus.Version = version;
      opus.LocaleId = locale;
      opus.Variation = vt;
      opus.IsBoilerplate = isBoilerplate;
      opus.Requirements = requirements;
      opus.TargetAudience = targetaudience;
      opus.ProposedOutcome = proposedoutcome;
      opus.RequirementsResource = requirementsResource.HasValue ? ResourceManager.Instance.GetFile(requirementsResource.Value) : null;
      opus.ProposedOutcomeResource = proposedOutcomeResource.HasValue ? ResourceManager.Instance.GetFile(proposedOutcomeResource.Value) : null;
      SaveChanges();
      // first remove all
      if (!useMilestones && opus.Milestones != null) {
        foreach (var ms in opus.Milestones.ToList()) {
          Ctx.Milestones.Remove(ms);
        }
        opus.Milestones.RemoveAll(m => true);
        SaveChanges();
      }
      // add those we need
      if (useMilestones && setMilestones != null) {
        opus.Milestones = new List<Milestone>();
        foreach (var ms in setMilestones) {
          ms.Opus = opus; // assure proper back ref
          ms.Owner = GetTeamMember(ms.Owner.Id);
          opus.Milestones.Add(ms);
        }
        SaveChanges();
      }
      return opus;
    }

    public Opus EditOpus(int id, string name, int version, string locale, VariationType vt, bool isBoilerplate, bool useMilestones, IEnumerable<Milestone> setMilestones) {
      return EditOpus(id, name, version, locale, vt, isBoilerplate, null, null, null, null, null, useMilestones, setMilestones);
    }

    public Team SaveTeamImage(int id, HttpPostedFileBase file, string userName) {
      var team = Ctx.Teams.Find(id);
      if (team.TeamLead.UserName == userName) {
        if (file != null && file.InputStream != null) {
          team.Image = file.InputStream.ReadToEnd();
          SaveChanges();
        }
      }
      return team;
    }

    public Team RemoveTeamImage(int id, string userName) {
      var team = Ctx.Teams.Find(id);
      if (team.TeamLead.UserName == userName) {
        team.Image = null;
        SaveChanges();
      }
      return team;
    }


    public void SaveProjectImage(int id, HttpPostedFileBase file, string userName) {
      if (file != null && file.InputStream != null) {
        var prj = GetProject(id, userName);
        prj.Image = file.InputStream.ReadToEnd();
        SaveChanges();
      }
    }


    public void SaveProjectProperty(int id, string elementId, string value) {
      var prj = Ctx.Projects.FirstOrDefault(t => t.Id == id);
      if (prj == null) return;
      switch (elementId) {
        case "projectName":
          prj.Name = value;
          break;
        case "projectDescription":
          prj.Description = value;
          break;
        case "projectShort":
          prj.Short = value;
          break;
        case "projectTerms":
          prj.TermsAndConditions = value;
          // terms reset the approval list actively
          prj.MemberTermApprovals.Clear();
          break;
      }
      SaveChanges();
    }

    public void DeleteTermSet(int id) {
      using (var scope = Ctx.BeginTransaction()) {
        var ts = Ctx.TermSets.Find(id);
        if (ts.Terms.Any()) {
          ts.Terms.ForEach(t => Ctx.Terms.Remove(t));
          SaveChanges();
        }
        Ctx.TermSets.Remove(ts);
        SaveChanges();
        scope.Commit();
      }
    }

    public void DeleteTerm(int id) {
      var ts = Ctx.Terms.Find(id);
      Ctx.Terms.Remove(ts);
      SaveChanges();
    }

    public void EditMarketingPackage(MarketingPackage pckg, int[] package, string countryList, string userName) {
      var editpckg = Ctx.MarketingPackages.First(p => p.Id == pckg.Id && p.Owner.UserName == userName);
      pckg.CopyProperties<MarketingPackage>(editpckg,
        p => p.Name,
        p => p.BasePrice,
        p => p.Description,
        p => p.CreateSocialPlatformInstances,
        p => p.CreateRssFeed,
        p => p.AssignIsbn,
        p => p.AssignIsbnADOI,
        p => p.MarketingType,
        p => p.Owner,
        p => p.PackageBasePrice,
        p => p.RegisterForLibraries,
        p => p.ShareContent
        );
      editpckg.MarketingType = (MarketingPackageType)package.Sum();
      if (!String.IsNullOrEmpty(countryList)) {
        var countryIds = countryList.Split(',');
        editpckg.LimitCountries.Clear();
        foreach (var c in countryIds) {
          var country = Ctx.Countries.Find(Int32.Parse(c)).Name;
          editpckg.LimitCountries.Add(country);
        }
      }
      editpckg.Owner = GetCurrentUser(userName);
      SaveChanges();
    }

    public MarketingPackage GetMarketingPackage(int id, string userName) {
      return Ctx.MarketingPackages.FirstOrDefault(p => p.Id == id && p.Owner.UserName == userName);
    }

    public Opus CreateOpusFromTemplate(NameValueCollection values, string userName) {
      var pid = Int32.Parse(values["Project.Id"]);
      var prj = GetActiveProjectWithMembers(pid, userName);
      if (prj == null) {
        throw new SecurityException("Project not accessible");
      }
      var newOpus = new Opus {
        Project = prj,
        Version = Int32.Parse(values["Version"]),
        Name = values["Name"],
        Active = true,
        LocaleId = values["Language"]
      };
      // copy only active milestones (Opus.Milestones[1].Active)
      var lead = GetTeamLeaderAsMember(prj.Team.Id);
      var mstn = CreateDefaultMileStones(lead, newOpus);
      CreateMilestonesFromFormValues(newOpus, lead, mstn, values);
      Ctx.Opuses.Add(newOpus);
      SaveChanges();
      // use the template information 'tpl' to pre-create a number of chapters and sections
      CreateFromTemplate(newOpus, values);
      SaveChanges();
      return newOpus;
    }

    private void CreateMilestonesFromFormValues(Opus opus, TeamMember defaultResponsibleMember, IEnumerable<Milestone> mstn, NameValueCollection values) {
      if (values["useMilestones"] == null || !values["useMilestones"].ParseCheckValue()) return;
      opus.Milestones = new List<Milestone>();
      foreach (var ms in from ms in mstn let active = values[String.Format("Opus.Milestones[{0}].Active", ms.Id)] where !String.IsNullOrEmpty(active) && Boolean.Parse(active) select ms) {
        var teamMemberId = values[String.Format("Opus.Milestones[{0}].Owner", ms.Id)];
        if (teamMemberId != null) {
          // on AddProject the team might not yet exist  and hence the team's leader will own all milestones
          defaultResponsibleMember = Ctx.TeamMembers.Find(Int32.Parse(teamMemberId));
        }
        opus.Milestones.Add(new Milestone {
          DateDue = DateTime.Parse(values[String.Format("Opus.Milestones[{0}].DateDue", ms.Id)], System.Globalization.CultureInfo.GetCultureInfo(CurrentCulture)),
          Owner = defaultResponsibleMember,
          Progress = ms.Progress,
          Comment = ms.Comment,
          Description = ms.Description,
          NextMilestone = ms.NextMilestone,
          Name = ms.Name,
          Opus = opus,
          DateAssigned = DateTime.Now,
          CreatedAt = DateTime.Now
        });
      }
    }
    private void CreateFromTemplate(Opus newOpus, NameValueCollection values) {
      newOpus.Children = new List<Element>();
      var tpl = Int32.Parse(values["tpl"]);
      switch (tpl) {
        case 0:
          // Empty
          var c = new Section { Content = Encoding.UTF8.GetBytes(ControllerResources.ProjectManager_CreateFromTemplate_First_Chapter), Name = ControllerResources.ProjectManager_CreateFromTemplate_First_Chapter, OrderNr = 1, Parent = newOpus };
          newOpus.Children.Add(c);
          break;
        case 1:
          // Simple: 3 Chapters
          int? chapterNumbers = Int32.Parse(values["tpl-1-chapters"]);
          var c1 = new Section { Content = Encoding.UTF8.GetBytes(ControllerResources.ProjectManager_CreateFromTemplate_Preface), Name = ControllerResources.ProjectManager_CreateFromTemplate_Preface, OrderNr = 1, Parent = newOpus };
          AddSectionToChapter(c1);
          newOpus.Children.Add(c1);
          int nc = 0;
          for (int n = 1; n <= chapterNumbers.Value; n++) {
            nc = n + 1;
            var cn = new Section { Content = Encoding.UTF8.GetBytes(ControllerResources.ProjectManager_CreateFromTemplate_Chapter_ + nc), Name = ControllerResources.ProjectManager_CreateFromTemplate_Chapter_ + nc, OrderNr = nc, Parent = newOpus };
            newOpus.Children.Add(cn);
            AddSectionToChapter(cn);
          }
          nc++;
          var c3 = new Section { Content = Encoding.UTF8.GetBytes(ControllerResources.ProjectManager_CreateFromTemplate_Index), Name = ControllerResources.ProjectManager_CreateFromTemplate_Index, OrderNr = nc, Parent = newOpus };
          newOpus.Children.Add(c3);
          AddSectionToChapter(c3);
          break;
        case 2:
          // Advanced
          // 1.   Preface
          //1.1 Target Audience
          //1.2 Who Should Read This?
          //1.3 Examples and Feedback
          //1.4 Dedications
          //1.5 About the Authors
          //2.   Introduction  chapters
          //3.   Deep Dive  chapters
          //4.   Reference  chapters
          //5.   Appendixes
          //5.1 Resources Used
          //5.2 Glossary
          //5.3 Exercises
          //5.4 Solutions
          //N.  Index 
          int introNumber = Int32.Parse(values["tpl-2-chapters-1"]);
          int deepdNumber = Int32.Parse(values["tpl-2-chapters-2"]);
          int refrcNumber = Int32.Parse(values["tpl-2-chapters-3"]);
          var ch1 = new Section { Content = Encoding.UTF8.GetBytes("Preface"), Name = "Chapter", OrderNr = 1, Parent = newOpus };
          AddSectionToChapter(ch1, "Target Audience");
          AddSectionToChapter(ch1, "Who Should Read This?");
          AddSectionToChapter(ch1, "Examples and Feedback");
          AddSectionToChapter(ch1, "Dedications");
          AddSectionToChapter(ch1, "About the Authors");
          newOpus.Children.Add(ch1);
          var ch2 = new Section { Content = Encoding.UTF8.GetBytes("Introduction"), Name = "Chapter", OrderNr = 2, Parent = newOpus };
          for (var n = 1; n <= introNumber; n++) {
            AddSectionToChapter(ch2, "Introduction Part " + 1);
          }
          newOpus.Children.Add(ch2);
          var ch3 = new Section { Content = Encoding.UTF8.GetBytes("Deep Dive"), Name = "Chapter", OrderNr = 3, Parent = newOpus };
          for (var n = 1; n <= deepdNumber; n++) {
            AddSectionToChapter(ch3, "Deep Dive Part " + 1);
          }
          newOpus.Children.Add(ch3);
          var ch4 = new Section { Content = Encoding.UTF8.GetBytes("Reference Part"), Name = "Chapter", OrderNr = 4, Parent = newOpus };
          for (var n = 1; n <= refrcNumber; n++) {
            AddSectionToChapter(ch4, "Reference Part " + 1);
          }
          newOpus.Children.Add(ch4);
          var ch5 = new Section { Content = Encoding.UTF8.GetBytes("Appendixes"), Name = "Chapter", OrderNr = 5, Parent = newOpus };
          AddSectionToChapter(ch5, "Resources Used ");
          AddSectionToChapter(ch5, "Glossary ");
          AddSectionToChapter(ch5, "Excercises ");
          AddSectionToChapter(ch5, "Solutions ");
          newOpus.Children.Add(ch5);
          break;
        case 3:
          // BLOG ARTICLE
          var c4 = new Section { Content = Encoding.UTF8.GetBytes("Blog Article"), Name = "Blog Article", OrderNr = 1, Parent = newOpus };
          newOpus.Children.Add(c4);
          break;
      }
    }

    private void AddSectionToChapter(Section chapter, string content = null) {
      var maxOrderNr = chapter.HasChildren() ? chapter.Children.Max(c => c.OrderNr) + 1 : 1;
      const string bt = @"Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";
      var s1 = new Section { Content = Encoding.UTF8.GetBytes(content ?? "First Section"), Name = "Section", OrderNr = maxOrderNr, Parent = chapter };
      Ctx.Elements.Add(s1);
      var t1 = new TextSnippet { Content = Encoding.UTF8.GetBytes(bt), Name = "Paragraph", OrderNr = 1, Parent = s1 };
      Ctx.Elements.Add(t1);
      s1.Children = new List<Element> { t1 };
      chapter.Children = new List<Element> { s1 };
    }

    public MarketingPackage CreateMarketingPackage(MarketingPackage pckg, int[] package, string countryList, string userName) {
      pckg.MarketingType = (MarketingPackageType)package.Sum();
      pckg.Owner = GetCurrentUser(userName);
      if (!String.IsNullOrEmpty(countryList)) {
        var countryIds = countryList.Split(',');
        pckg.LimitCountries.Clear();
        foreach (var c in countryIds) {
          var country = Ctx.Countries.Find(Int32.Parse(c)).Name;
          pckg.LimitCountries.Add(country);
        }
      }
      Ctx.MarketingPackages.Add(pckg);
      SaveChanges();
      return pckg;
    }

    public Team GetTeam(int id) {
      return Ctx.Teams.Find(id);
    }

    public IEnumerable<Project> GetProjectForRoles(ContributorRole[] someRoles, string userName) {
      return Ctx.Projects
        .Include(p => p.Opuses)
        .Where(p => p.Team.Members.Any(m => m.Member.UserName == userName))
        .ToList()
        .Where(p => someRoles.Any(r => p.Team.Members.Any(t => t.Role.ContributorRoles == r)));
    }

    public IEnumerable<SalesTracking> GetSalesTracking(string userName) {
      return Ctx.SalesTracking.Where(s => s.Author.UserName == userName);
    }

    public bool DeleteMarketingPackage(MarketingPackage pckg, string userName) {
      var inuse = Ctx.Projects.Any(o => o.Marketing.Id == pckg.Id);
      if (!inuse) {
        Ctx.MarketingPackages.Remove(pckg);
        SaveChanges();
      }
      return inuse;
    }

    public IEnumerable<Team> GetTeamsWhereUserIsLead(string userName) {
      return Ctx.Teams.Where(t => t.Members.Any(m => m.Member.UserName == userName && m.TeamLead));
    }

    public void AddWorkroomMessage(WorkroomChat msg, int projectId, int? parentId, string userName) {
      msg.Owner = Manager<UserManager>.Instance.GetUserByName(userName);
      msg.Project = GetProject(projectId, userName);
      if (parentId.HasValue) {
        msg.Parent = Ctx.WorkroomChats.Find(parentId.Value);
      }
      Ctx.WorkroomChats.Add(msg);
      SaveChanges();
    }

    public IQueryable<WorkroomChat> GetTopWorkroomMessages(int projectId) {
      return Ctx.WorkroomChats
        .Include("Owner")
        .Where(w => w.Project.Id == projectId && w.Parent == null)
        .OrderByDescending(w => w.CreatedAt);
    }

    public IQueryable<ResourceFile> GetResourceFiles(int projectId, TypeOfResource type, string mimeType) {
      return GetResourceFiles(projectId, type, mimeType, "");
    }

    public IQueryable<ResourceFile> GetResourceFiles(int projectId, TypeOfResource type, string mimeType, string namePattern, bool excludebackups = false) {
      var res = Ctx.Resources
        .OfType<ResourceFile>()
        .Where(r => r.Project.Id == projectId && r.TypesOfResource == type)
        .AsQueryable();
      if (!String.IsNullOrEmpty(mimeType) && String.IsNullOrEmpty(namePattern)) {
        res = res.Where(r => r.MimeType.StartsWith(mimeType)).AsQueryable();
      }
      if (!String.IsNullOrEmpty(namePattern) && String.IsNullOrEmpty(mimeType)) {
        res = res.Where(r => r.Name.EndsWith(namePattern)).AsQueryable();
      }
      if (!String.IsNullOrEmpty(namePattern) && !String.IsNullOrEmpty(mimeType)) {
        res = res.Where(r => r.Name.EndsWith(namePattern) && r.MimeType.StartsWith(mimeType)).AsQueryable();
      }
      if (excludebackups) {
        res = res.Where(r => r.MimeType != "application/opus-xml");
      }
      res.ForEach(r => {
        var blob = BlobFactory.GetBlobStorage(r.ResourceId, BlobFactory.Container.Resources);
        r.FileSize = blob.Content != null ? blob.Content.Length : -1;
        r.Metadata = blob.MetaData;
      });
      return res;
    }

    public IQueryable<ResourceFolder> GetResourceFolders(Opus opus, TypeOfResource type) {
      var projectId = opus.Project.Id;
      var res = Ctx.Resources.OfType<ResourceFolder>().Where(r => r.Project.Id == projectId && r.TypesOfResource == type).AsQueryable();
      return res;
    }

    public IQueryable<ResourceFolder> GetResourceFolders(int projectId, TypeOfResource type) {
      var res = Ctx.Resources.OfType<ResourceFolder>().Where(r => r.Project.Id == projectId && r.TypesOfResource == type).AsQueryable();
      return res;
    }


    public IQueryable<ResourceFile> GetResourceFiles(Opus opus, TypeOfResource type, string mimeType) {
      return GetResourceFiles(opus.Project.Id, type, mimeType, null);
    }

    public IQueryable<ResourceFile> GetResourceFiles(Opus opus, TypeOfResource type, string mimeType, string namePattern, bool excludebackups = false) {
      return GetResourceFiles(opus.Project.Id, type, mimeType, namePattern, excludebackups);
    }

    public Resource GetResource(Guid guid) {
      return Ctx.Resources.FirstOrDefault(r => r.ResourceId == guid);
    }

    public Resource GetResource(int id) {
      return Ctx.Resources.FirstOrDefault(r => r.Id == id);
    }


    # region Publishing

    public void SavePublished(Published pub) {
      Ctx.Entry(pub).State = pub.Id == 0 ? EntityState.Added : EntityState.Modified;
      SaveChanges();
    }

    public void SavePublishedCommon(int publishedId,
      string title,
      string subtitle,
      int navlevel,
      string kindleLanguage,
      string keywords,
      string description,
      int[] contrib, int[] about, string userName) {
      var pub = Ctx.Published.FirstOrDefault(p => p.Id == publishedId && p.Owner.UserName == userName);
      if (pub == null) return;
      pub.Title = title;
      pub.SubTitle = subtitle;
      pub.NavLevel = navlevel;
      pub.Authors.Clear();
      if (contrib != null && contrib.Any()) {
        pub.Authors = Ctx.Users.Where(u => contrib.Any(c => c == u.Id)).ToList();
      }
      if (about != null && about.Any()) {
        pub.ExternalPublisher.AuthorIds = about;
      } else {
        pub.ExternalPublisher.Authors = String.Empty;
      }
      pub.ExternalPublisher.KindleLanguage = kindleLanguage;
      pub.ExternalPublisher.Description = description;
      pub.ExternalPublisher.Keywords = keywords;
      SaveChanges();
    }

    public void SavePublishedCatalogue(int publishedId, int[] catalogItems, string userName) {
      var pub = Ctx.Published.FirstOrDefault(p => p.Id == publishedId && p.Owner.UserName == userName);
      if (pub == null) return;
      Ctx.LoadProperty(pub, p => p.Catalogs);
      Ctx.LoadProperty(pub.SourceOpus.Project, "Marketing");
      pub.Marketing = Ctx.MarketingPackages.Find(pub.SourceOpus.Project.Marketing.Id);
      if (pub.Catalogs.Any()) {
        pub.Catalogs.RemoveAll(c => true);
      }
      if (catalogItems != null) {
        pub.Catalogs = Ctx.Catalog.Where(c => catalogItems.Any(ci => ci == c.Id)).ToList();
      }
      SaveChanges();
    }

    /// <summary>
    /// Let the author decide the media, currently not used.
    /// </summary>
    /// <param name="publishedId"></param>
    /// <param name="mediaItems"></param>
    /// <param name="userName"></param>
    public void SaveSuportedMedia(int publishedId, int[] mediaItems, string userName) {
      var pub = Ctx.Published.FirstOrDefault(p => p.Id == publishedId && p.Owner.UserName == userName);
      if (pub == null) return;
      Ctx.LoadProperty(pub, p => p.SupportedMedia);
      pub.SupportedMedia = Ctx.OrderMedias.Where(m => mediaItems.Any(mi => mi == m.Id && m.Available)).ToList();
      SaveChanges();
    }

    public void SavePublishedCover(int publishedId, string foreColor, string backColor, string fontFamily, float fontSize, string backTemplate, bool useBackTemplate, string userName) {
      var pub = Ctx.Published.FirstOrDefault(p => p.Id == publishedId && p.Owner.UserName == userName);
      if (pub == null) return;
      if (pub.CoverImage == null) {
        pub.CoverImage = new Cover();
      }
      pub.CoverImage.ForeColor = foreColor;
      pub.CoverImage.BackColor = backColor;
      pub.CoverImage.FontFamily = fontFamily;
      pub.CoverImage.UseCoverBackgroundTemplate = useBackTemplate;
      pub.CoverImage.CoverBackgroundTemplate = backTemplate;
      pub.CoverImage.BaseFontSize = fontSize > 0 ? fontSize : 64F;
      SaveChanges();
    }

    public void SavePublishedCover(int publishedId, string userName) {
      var pub = Ctx.Published.FirstOrDefault(p => p.Id == publishedId && p.Owner.UserName == userName);
      if (pub == null) return;
      if (pub.CoverImage == null) {
        pub.CoverImage = new Cover();
      }
      pub.CoverImage.CoverImage = null;
      SaveChanges();
    }

    public void SavePublishedCover(int publishedId, byte[] cover, string userName) {
      var pub = Ctx.Published.FirstOrDefault(p => p.Id == publishedId && p.Owner.UserName == userName);
      if (pub == null) return;
      if (pub.CoverImage == null) {
        pub.CoverImage = new Cover();
      }
      pub.CoverImage.CoverImage = cover;
      SaveChanges();
    }

    public void SavePublishedMarketing(int publishedId, bool kindleDrmRequired, bool kindleCreativeCommon, bool shareContent, bool registerLib, bool createRss, bool assignIsbn, decimal basePrice, string userName) {
      var pub = Ctx.Published.FirstOrDefault(p => p.Id == publishedId && p.Owner.UserName == userName);
      if (pub == null) return;
      Ctx.LoadProperty(pub, p => p.Marketing);
      pub.Marketing.ShareContent = shareContent;
      pub.Marketing.RegisterForLibraries = registerLib;
      pub.Marketing.CreateRssFeed = createRss;
      pub.Marketing.BasePrice = basePrice;
      pub.Marketing.AssignIsbn = assignIsbn;
      if (assignIsbn) {
        ClaimIsbn(pub);
      }
      pub.ExternalPublisher.DrmRequired = kindleDrmRequired;
      pub.ExternalPublisher.CreativeCommon = kindleCreativeCommon;
      SaveChanges();
    }

    public void SavePublishedOptions(int publishedId, bool kindleDrmRequired, bool kindleCreativeCommon, string userName) {
      var pub = Ctx.Published.FirstOrDefault(p => p.Id == publishedId && p.Owner.UserName == userName);
      if (pub == null) return;
      pub.ExternalPublisher.DrmRequired = kindleDrmRequired;
      pub.ExternalPublisher.CreativeCommon = kindleCreativeCommon;
      SaveChanges();
    }

    public void SaveCoverImage(int id, HttpPostedFileBase file, string userName) {
      if (file == null || file.InputStream == null) return;
      SavePublishedCover(id, file.InputStream.ReadToEnd(), userName);
    }

    public void SaveCoverImage(int id, Image img, string userName) {
      var ms = new MemoryStream();
      img.Save(ms, ImageFormat.Png);
      SavePublishedCover(id, ms.ToArray(), userName);
    }

    public void SaveCoverImage(int id, string userName) {
      SavePublishedCover(id, userName);
    }


    public void SetPublished(int id, ExternalPublisherSettings kindle) {
      var publ = GetPublished(id);
      var ext = publ.ExternalPublisher;
      ext.Authors = kindle.Authors;
      ext.Categories = kindle.Categories;
      ext.CreativeCommon = kindle.CreativeCommon;
      ext.Description = kindle.Description;
      ext.DrmRequired = kindle.DrmRequired;
      ext.Isbn = kindle.Isbn;
      ext.Keywords = kindle.Keywords;
      ext.KindleLanguage = kindle.KindleLanguage;
      ext.Title = kindle.Title;
      SaveChanges();
    }

    # endregion

    public TermSet GetTermSet(int id) {
      return Ctx.TermSets.Find(id);
    }

    public IEnumerable<Term> GetTermSetForGrid(int id, bool search, string type, string key, string value, int r, int p, string sidx, string sord, out string tsName) {
      Func<Term, string> o = term => {
        switch (sidx) {
          case "value":
            return term.Content;
          case "key":
            return term.Text;
          case "type":
            return term.TermType.ToString();
        }
        return term.Text;
      };
      Func<Term, bool> w = term => {
        if (!String.IsNullOrEmpty(value)) {
          return !search || term.Content.Contains(value);
        }
        if (!String.IsNullOrEmpty(key)) {
          return !search || term.Text.Contains(key);
        }
        if (!String.IsNullOrEmpty(type)) {
          return !search || type == "99" || term.TermType == (TermType)Enum.Parse(typeof(TermType), type);
        }
        return true;
      };

      var s = (p - 1) * r;
      var ts = GetTermSet(id);
      tsName = ts.Name;
      var model = ts.Terms
        .Take(r)
        .Skip(s)
        .Where(w)
        .OrderBy(o, new TermOrderComparer(sord))
        .AsEnumerable();
      return model;
    }

    private class TermOrderComparer : IComparer<string> {

      private readonly string _order;

      public TermOrderComparer(string order) {
        _order = order;
      }

      public int Compare(string x, string y) {
        return _order == "desc" ? String.Compare(x, y, System.StringComparison.Ordinal) : String.Compare(y, x, System.StringComparison.Ordinal);
      }
    }

    public void PublishContentToFrozenState(int publishedId, int opusId, int level) {
      if (level > 2) {
        throw new NotSupportedException("Navigation Level 0 or 1 currently supported.");
      }
      using (var scope = Ctx.BeginTransaction()) {
        var opus = GetOpusInternal(opusId);
        // TODO: temporarily deactivate to prevent others from changing it during publishing phase?
        var publ = Ctx.Published.Find(publishedId);
        /******************************************************/
        // associate this Published entity with Opus and Project
        opus.Published = publ;
        if (opus.Project.Published == null) {
          opus.Project.Published = new List<Published>();
        }
        if (opus.Project.Published.All(p => p.Id != publishedId)) {
          opus.Project.Published.Add(publ);
        }
        SaveChanges();
        /******************************************************/
        // Assure that we don't have the same published object more than once, so delete same content first
        if (Ctx.FrozenFragments.Any(f => f.Published.Id == publishedId && f.Parent == null)) {
          foreach (var parentFf in Ctx.FrozenFragments.Where(f => f.Published.Id == publishedId && f.Parent == null)) {
            DeleteFrozenFragmentsRecursively(parentFf);
          }
          SaveChanges();
        }
        /******************************************************/
        // Copy the complete opus into frozen state, either in Level 0 (one fragment) or Level 1 (one per chapter) mode 
        // Apply the properties to the frozen state for images  
        // this call does copy the whole hierarchy
        publ.FrozenFragments = CopyFrozenFragment(opus, publ, level);
        // once we have the frozen fragments the readers sees the book in the catalog, on first request a copy is made into his private library (Work)
        // A Work entity has a identical list of WorkFragments, which has no content and relates n:1 to FrozenFragments
        SaveChanges();
        // If not shared unset the singularity token, this is relevant for matrix
        if (publ.Marketing != null && !publ.Marketing.ShareContent) {
          publ.FrozenFragments.ForEach(f => f.SingularEntity = false);
        }
        // Finally, at the last step, make this text visible
        publ.IsPublished = true;
        SaveChanges();
        scope.Commit();
      }
    }

    /// <summary>
    /// Create deep level copy of an opus and attach the result to the given Published entity.
    /// </summary>
    /// <param name="opus">Source</param>
    /// <param name="pub">Target</param>
    /// <param name="level"></param>
    /// <returns>Root element of fragments hierarchy as bound to Published entity.</returns>
    public static IList<FrozenFragment> CopyFrozenFragment(Opus opus, Published pub, int level) {
      opus.CreateImage = (sender, e) => {
        // caller applies properties, creates a temp file name, and let us handle the whole thing here
        var resff = new FrozenFragment {
          // explicitly set to null, because otherwise it would be part of published, and handled on document level, which is wrong
          // sub entities, such as images, are never allowed to be handled outside of their parent documents
          Published = null,
          // instead, we parenting this fragment to make clear where it belongs to
          Parent = e.TargetFragment,
          Title = ((TitledSnippet)e.SourceSnippet).Title,
          TypeOfFragment = FragmentType.Image,
          Content = e.Content,
          ItemHref = e.FileName
        };
        e.TargetFragment.Children.Add(resff);
        return e.FileName;
      };
      var frozenFragments = new List<FrozenFragment>();
      CopyToFrozen(opus, frozenFragments, pub, level);
      return frozenFragments;
    }

    private static void CopyToFrozen(Opus opus, ICollection<FrozenFragment> frozen, Published pub, int level) {
      // level 0 = one doc, 1 = chapters,
      switch (level) {
        case 2:
        case 1:
          var orderNr = 1;
          IEnumerable<Snippet> chapters = opus.Children.OfType<Snippet>();
          foreach (var chapter in chapters.OrderBy(c => c.OrderNr)) {
            var ff = new FrozenFragment {
              Name = chapter.Name,
              ItemHref = GetUniqueFragmentName(chapter),
              TypeOfFragment = chapter.ProposedFragmentType,
              Published = pub,
              OrderNr = orderNr++,
              Children = new List<FrozenFragment>()
            };
            // we use Pdf as the most verbose export format            
            var chapterBuilder = chapter.GetType()
              .GetCustomAttributes(typeof(SnippetBuilderAttribute), true)
              .OfType<SnippetBuilderAttribute>()
              .Single(a => a.Target == GroupKind.Pdf);
            ff.Content = Encoding.UTF8.GetBytes(chapterBuilder.BuildHtml(chapter, null, ff));
            frozen.Add(ff);
            // Get the final HTML, we use the "default" HTML 5 production instruction TemplateGroup.Html which gives the most basic level to freeze
          }
          break;
        case 0:
          var f = new FrozenFragment {
            Name = pub.Title,
            ItemHref = "opus-p-" + pub.Id,
            TypeOfFragment = FragmentType.Html,
            Published = pub,
          };
          // a single document consists just of the opus
          var opusBuilder = opus.GetType()
            .GetCustomAttributes(typeof(SnippetBuilderAttribute), true)
            .OfType<SnippetBuilderAttribute>()
            .Single(a => a.Target == GroupKind.Pdf);
          // we use Pdf as the most verbose export format
          opus.BuiltContent = CreateOpusHtmlInner(opus.Children.OfType<Snippet>().OrderBy(c => c.OrderNr), f, null, GroupKind.Pdf);
          var rawHtml = opusBuilder.BuildHtml(opus, null, f);
          var checkDoc = new HtmlDocument();
          checkDoc.LoadHtml(rawHtml);
          checkDoc.OptionOutputAsXml = true;
          checkDoc.OptionAutoCloseOnEnd = true;
          using (var tw = new StringWriter()) {
            using (var xw = new XmlTextWriter(tw)) {
              checkDoc.Save(xw);
              var toCheckForXml = tw.ToString();
              var xDoc = XDocument.Parse(toCheckForXml);
              f.Content = Encoding.UTF8.GetBytes(xDoc.ToString(SaveOptions.None));
            }
          }
          frozen.Add(f);
          break;
      }
    }

    private static string CreateOpusHtmlInner(IEnumerable<Snippet> source, FrozenFragment targetFragment, IDictionary<string, NumberingSchema> numbering, GroupKind target) {
      // each snippet has its very own numbering schema
      var flatContent = new StringBuilder();
      foreach (var elm in source) {
        // Get the final HTML
        var builder = elm.GetType().GetCustomAttributes(typeof(SnippetBuilderAttribute), true).OfType<SnippetBuilderAttribute>().Single(sb => sb.Target == target);
        var html = builder.BuildHtml(elm, numbering, targetFragment);
        flatContent.Append(html);
        if (numbering == null) continue;
        // add chapter and reset all other counters
        numbering["Section1"].Major = numbering["Section1"].Major + 1;
        numbering["Section2"].Major = numbering["Section1"].Major;
        numbering["Section3"].Major = numbering["Section1"].Major;
        numbering["Section4"].Major = numbering["Section1"].Major;
        numbering["ImageSnippet"].Major = numbering["Section1"].Major;
        numbering["TableSnippet"].Major = numbering["Section1"].Major;
        numbering["ListingSnippet"].Major = numbering["Section1"].Major;
        numbering["Section1"].Minor = 1;
        numbering["Section2"].Minor = 1;
        numbering["Section3"].Minor = 1;
        numbering["Section4"].Minor = 1;
        numbering["ImageSnippet"].Minor = 1;
        numbering["TableSnippet"].Minor = 1;
        numbering["ListingSnippet"].Minor = 1;
      }
      return flatContent.ToString();
    }


    private static string GetUniqueFragmentName(Element elm) {
      var n = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
      return String.Format("{2}-{1}-{0}", n, elm.Level, elm.GetType().BaseType.Name);
    }

    /// <summary>
    /// This is a helper method used internally to remove accidentially published content multiple times
    /// </summary>
    /// <remarks>ATTENTION! NEVER EVER USE THIS METHOD TO REMOVE FRAGMENTS FROM ACTIVE CONTENT</remarks>
    /// <param name="parentFf"></param>
    private void DeleteFrozenFragmentsRecursively(FrozenFragment parentFf) {
      Ctx.LoadProperty(parentFf, p => p.Children);
      if (parentFf.HasChildren()) {
        foreach (var child in parentFf.Children.ToList()) {
          DeleteFrozenFragmentsRecursively(child);
        }
      }
      // if we come here we got a leaf element
      Ctx.FrozenFragments.Remove(parentFf);
    }

    public ManagerResult SetBasePrice(int id, decimal price, string userName) {
      var mr = new ManagerResult("ProjectManager");
      var prj = GetProject(id, userName, p => p.Marketing);
      if (prj != null) {
        prj.Marketing.BasePrice = price;
        SaveChanges();
        mr.SetInformation(ControllerResources.MarketingController_Saved_BasePrice);
      } else {
        mr.SetError(ControllerResources.MarketingController_Saved_BasePrice_Error);
      }
      return mr;
    }

    public IEnumerable<WizardWorkflow> GetUserWorkflows(string userName) {
      var wfs = Ctx.WizardWorkflows.Where(w => w.Owner.UserName == userName);
      return wfs;
    }


    public void DeleteUserWorkflows(int id, string userName) {
      var wf = Ctx.WizardWorkflows.FirstOrDefault(w => w.Owner.UserName == userName);
      if (wf == null) return;
      Ctx.WizardWorkflows.Remove(wf);
      SaveChanges();
    }

    public WizardWorkflow GetUserWorkflow(int id, string userName) {
      var wf = Ctx.WizardWorkflows.FirstOrDefault(w => w.Id == id && w.Owner.UserName == userName);
      return wf;
    }

    public List<PeerReview> GetReviewsForOpus(int opusId) {
      var revs = Ctx.Opuses
        .Include(p => p.Published)
        .Include(p => p.Published.Reviews)
        .Single(p => p.Id == opusId)
        .Published
        .Reviews
        .OfType<PeerReview>()
        .ToList();
      return revs;
    }

    public List<T> GetReviewsForPublished<T>(int publishedId)
      where T : Review {
      var revs = Ctx.Published
        .Include(p => p.Reviews)
        .Single(p => p.Id == publishedId);
      return revs.Reviews.OfType<T>().ToList();
    }

    public T GetReview<T>(int id) where T : Review {
      return Ctx.Reviews.Find(id) as T;
    }

    public void EditReview(int id, Review review) {
      Ctx.Reviews.Attach(review);
      Ctx.LoadProperty(review, m => m.Reviewer, m => m.PublishedWork);
      Ctx.Entry(review).State = EntityState.Modified;
      SaveChanges();
    }

    public void ApproveReview(int id, bool approved, string userName) {
      var review = Ctx.Reviews.Find(id);
      if (review != null) {
        // only team lead is allowed to approve
        Ctx.LoadProperty(review, m => m.PublishedWork);
        var opus = review.PublishedWork.SourceOpus;
        if (UserIsTeamLead(opus.Id, userName)) {
          review.Approved = approved;
          SaveChanges();
        }
      }
    }


    public void DeleteReview(int id, string userName) {
      var rev = Ctx.Reviews.FirstOrDefault(r => r.Id == id && r.Reviewer.UserName == userName);
      if (rev != null) {
        Ctx.Reviews.Remove(rev);
        SaveChanges();
      }
    }

    public void AddReviewForPublished(int id, Review review, string userName) {
      var pub = Ctx.Published.Find(id);
      var usr = GetCurrentUser(userName);
      review.Reviewer = usr;
      review.PublishedWork = pub;
      review.Approved = false;
      Ctx.Entry(review).State = EntityState.Added;
      SaveChanges();
    }

    public Project GetProjectForOpus(int opusId, string userName) {
      var prj = GetOpus(opusId, userName).Project;
      return prj;
    }

    public Guid SaveImportFile(int projectId, HttpPostedFileBase file, string folder, string userName) {
      var fileName = Path.GetFileName(file.FileName);
      file.InputStream.Position = 0;
      var fileData = file.InputStream.ReadToEnd();
      return SaveImportFile(projectId, fileData, fileName, folder, userName);
    }

    public Guid SaveImportFile(int projectId, byte[] fileData, string fileName, string folder, string userName) {
      var resId = Guid.NewGuid();
      var prj = GetProject(projectId, userName);
      // Get folder and create if necessary
      ResourceFolder parentFolder = null;
      if (!String.IsNullOrEmpty(folder)) {
        parentFolder = Ctx.Resources
          .OfType<ResourceFolder>()
          .FirstOrDefault(r => r.Name == folder && r.Project.Id == projectId && r.TypesOfResource == TypeOfResource.Import) ??
                       new ResourceFolder {
                         Name = folder,
                         Owner = GetCurrentUser(userName),
                         Project = prj,
                         TypesOfResource = TypeOfResource.Import
                       };
      }
      var res = new ResourceFile {
        Name = fileName,
        MimeType = MimeTypeHelper.GetFromExtension(Path.GetExtension(fileName)),
        Owner = GetCurrentUser(userName),
        ResourceId = resId,
        Project = prj,
        Parent = parentFolder,
        TypesOfResource = TypeOfResource.Import
      };
      using (var blob = BlobFactory.GetBlobStorage(resId, BlobFactory.Container.Resources)) {
        blob.Content = fileData;
        blob["FullName"] = fileName;
        blob["FileName"] = Path.GetFileNameWithoutExtension(fileName);
        blob.Save();
      }
      Ctx.Resources.Add(res);
      SaveChanges();
      return resId;
    }

    public TeamMember GetTeamMember(int id) {
      return Ctx.TeamMembers
        .Include(t => t.Team)
        .First(t => t.Id == id);
    }

    public void SetTeamMemberRoles(int teamMemberId, int roles) {
      var tm = GetTeamMember(teamMemberId);
      tm.Role.ContributorRoles = (ContributorRole)roles;
      SaveChanges();
    }

    //    /// <summary>
    //    /// Create deep level copy of an opus
    //    /// </summary>
    //    /// <param name="document"></param>
    //    /// <param name="withNumbers"> </param>
    //    /// <returns></returns>
    //    public string CreateHtml(Opus document, bool withNumbers = true) {
    //      var final = CreateFinalHtml(document, withNumbers);
    //      var path = Path.Combine(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "data\\"), document.Name + ".html");
    //      //var path = Path.Combine(HttpContext.Current.Server.MapPath("~/data"), document.Name + ".html");
    //      using (var file = new StreamWriter(path, false, Encoding.UTF8)) {
    //        file.Write(final);
    //        file.Close();
    //      }
    //      return Path.Combine("/data", document.Name + ".html");
    //    }

    //    public string CreateFinalHtml(Opus document, bool withNumbers = true) {
    //      var flatContent = new StringBuilder();
    //      flatContent.Append(CreateHtmlInner(document.Children.OfType<Snippet>(), withNumbers));
    //      var html = flatContent.ToString();


    //      var final = String.Format(@"<!DOCTYPE HTML>
    //                <html>
    //                  <head>
    //                    <link rel=""stylesheet"" href=""document.css"" />
    //                  </head>
    //                  <body>
    //                    {0}
    //                  </body>
    //                </html>
    //                        ",
    //        html);
    //      return final;
    //    }

    //    private string CreateHtmlInner(IEnumerable<Snippet> source, bool withNumbers) {
    //      // each snippet has its very own numbering schema
    //      var numbering = new Dictionary<string, NumberingSchema> {        
    //        {"Section1", new NumberingSchema { Major = 1, Separator = '.', Divider = "", Label = "&nbsp;&nbsp;"}}, 
    //        {"Section2", new NumberingSchema { Major = 1, Minor = 1, Separator = '.', Divider = "", Label = "&nbsp;&nbsp;", IncludeParent = true}},
    //        {"Section3", new NumberingSchema { Major = 1, Minor = 1, Separator = '.', Divider = "", Label = "&nbsp;&nbsp;", IncludeParent = true}},
    //        {"Section4", new NumberingSchema { Major = 1, Minor = 1, Separator = '.', Divider = "", Label = "&nbsp;&nbsp;", IncludeParent = true}},
    //        {"ImageSnippet", new NumberingSchema { Major = 1, Minor = 1, Separator = '-', Divider = ": ", Label = "Figure " }}, 
    //        {"TableSnippet", new NumberingSchema { Major = 1, Minor = 1, Separator = '-', Divider = ": ", Label = "Table " }},
    //        {"ListingSnippet", new NumberingSchema { Major = 1, Minor = 1, Separator = '-', Divider = ": ", Label = "Listing " }}
    //      };
    //      var flatContent = new StringBuilder();
    //      foreach (var elm in source) {
    //        // Get the final HTML
    //        var builder = elm.GetType().GetCustomAttributes(typeof(SnippetBuilderAttribute), true).OfType<SnippetBuilderAttribute>().Single();
    //        var html = builder.BuildHtml(elm, numbering);
    //        // Resource based elements get the resource's content as child element
    //        if (builder.CreateResource) {
    //          throw new NotImplementedException();
    //        }
    //        flatContent.Append(html);
    //        if (elm.HasChildren()) {
    //          flatContent.Append(CreateHtmlInnerRecursively(elm.Children.OfType<Snippet>(), numbering));
    //        }
    //        // add chapter and reset all other counters
    //        numbering["Section1"].Major = numbering["Section1"].Major + 1;
    //        numbering["Section2"].Major = numbering["Section1"].Major;
    //        numbering["Section3"].Major = numbering["Section1"].Major;
    //        numbering["Section4"].Major = numbering["Section1"].Major;
    //        numbering["ImageSnippet"].Major = numbering["Section1"].Major;
    //        numbering["TableSnippet"].Major = numbering["Section1"].Major;
    //        numbering["ListingSnippet"].Major = numbering["Section1"].Major;
    //        numbering["Section1"].Minor = 1;
    //        numbering["Section2"].Minor = 1;
    //        numbering["Section3"].Minor = 1;
    //        numbering["Section4"].Minor = 1;
    //        numbering["ImageSnippet"].Minor = 1;
    //        numbering["TableSnippet"].Minor = 1;
    //        numbering["ListingSnippet"].Minor = 1;
    //      }
    //      return flatContent.ToString();
    //    }

    //    public event CreateImageHandler CreateImage;

    //    private string CreateHtmlInnerRecursively(IEnumerable<Snippet> snippets, IDictionary<string, NumberingSchema> numbering) {
    //      var flatContent = new StringBuilder();
    //      Action<Snippet> convert = null;
    //      convert = e => {
    //        var builder = e.GetType().GetCustomAttributes(typeof(SnippetBuilderAttribute), true).OfType<SnippetBuilderAttribute>().Single();
    //        if (builder.CreateResource) {
    //          string itemHref = String.Empty;
    //          // BaseType because it's a proxy element; if database context changes behavior, change this
    //          switch (e.GetType().BaseType.Name) {
    //            case "ImageSnippet":
    //              // pull resource from BLOB storage
    //              if (e.Content != null) {
    //                itemHref = ((ImageSnippet)e).ItemHref;
    //                var properties = System.Web.Helpers.Json.Decode<ImageProperties>(((ImageSnippet)e).Properties);
    //                var image = ImageUtil.ApplyImageProperties(e.Content, properties);
    //                if (image != null) {
    //                  using (var ms = new MemoryStream()) {
    //                    image.Save(ms, ImageFormat.Png);
    //                    byte[] bytes = ms.ToArray();
    //                    ((ImageSnippet)e).ItemHref = CreateImage(this, new CreateImageArguments { Content = bytes, FileName = itemHref });
    //                  }
    //                } else {
    //                  // create a placeholder image
    //                  var fs = File.ReadAllBytes(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "data\\images\\bullet.png"));

    //                  //var fs = File.ReadAllBytes(HttpContext.Current.Server.MapPath("~/data/images/bullet.png"));
    //                  ((ImageSnippet)e).ItemHref = CreateImage(this, new CreateImageArguments { Content = fs, FileName = itemHref });
    //                }
    //              }
    //              break;
    //          }
    //          flatContent.Append(builder.BuildHtml(e, numbering));
    //        } else {
    //          flatContent.Append(builder.BuildHtml(e, numbering));
    //          // if resource do not add again
    //          if (e.HasChildren()) {
    //            e.Children.OfType<Snippet>().ToList().ForEach(c => convert(c));
    //          }
    //        }
    //      };
    //      snippets.ToList().ForEach(c => convert(c));
    //      return flatContent.ToString();
    //    }

    public IEnumerable<PackageCountryHelper> GetCountries(string searchTerm) {
      var cntry = PermanentCache("Countries", () => Ctx.Countries.ToList());
      return cntry
        .Where(c => c.Name.ToLower().Contains(searchTerm.ToLower()))
        .Select(c => new PackageCountryHelper { id = c.Id, name = c.Name });
    }

    public IEnumerable<PackageCountryHelper> GetCountries(IList<string> searchTerms) {
      var cntry = PermanentCache("Countries", () => Ctx.Countries.ToList());
      return cntry
        .Where(c => searchTerms.Any(s => s == c.Name))
        .Select(c => new PackageCountryHelper { id = c.Id, name = c.Name });
    }

    public Country GetCountry(string searchTerm) {
      var cntry = PermanentCache("Countries", () => Ctx.Countries.ToList());
      return cntry.Single(c => c.Name == searchTerm);
    }


    public IEnumerable<WorkitemChat> GetTeamComments(int snippetId, string userName) {
      return Ctx.WorkitemChats.Where(w => w.Snippet.Id == snippetId && !w.Private);
    }

    public IEnumerable<WorkitemChat> GetAuthorComments(int snippetId, string userName) {
      return Ctx.WorkitemChats.Where(w => w.Snippet.Id == snippetId && w.Private && w.Owner.UserName == userName);
    }

    public IEnumerable<Comment> GetReaderComments(int snippetId) {
      // TODO: Take snippet, get opus, search published, look to works derived from published, 
      // TODO: get comments from work, filter for those with same frozenfragment, return  
      return null;
    }

    public void SaveComment(int id, string comment, string type, string userName) {
      WorkitemChat chat = null;
      var user = GetCurrentUser(userName);
      switch (type) {
        case "Reader":
          throw new NotImplementedException();
        case "Author":
          chat = new WorkitemChat {
            Content = comment,
            Private = true,
            Owner = user
          };
          break;
        case "Contributor":
          chat = new WorkitemChat {
            Content = comment,
            Private = false,
            Owner = user
          };
          break;
      }
      if (chat != null) {
        Ctx.WorkitemChats.Add(chat);
        SaveChanges();
      }
    }

    public Team GetTeambyName(string tn) {
      return Ctx.Teams.FirstOrDefault(t => t.Name == tn);
    }

    private void DeleteOpusContentRecursively(Element parentElement) {
      if (parentElement.HasChildren()) {
        foreach (var child in parentElement.Children.ToList()) {
          DeleteOpusContentRecursively(child);
        }
      }
      // if we come here we got a leaf element
      Ctx.Elements.Remove(parentElement);
    }

    private void SaveOpusContentRecursively(Element parentElement, List<Element> saveElements) {
      if (parentElement.HasChildren()) {
        foreach (var child in parentElement.Children.ToList()) {
          parentElement.Children.Add(child);
          SaveOpusContentRecursively(child, saveElements);
        }
      }
      // if we come here we got a leaf element
      saveElements.Add(parentElement);
    }

    public void RestoreOpusFromFile(int id, XDocument xDoc, string userName) {
      var user = GetCurrentUser(userName);
      var opus = GetOpusWithTeam(id);
      var saveElements = new List<Element>();
      opus.Children.ToList().ForEach(e => SaveOpusContentRecursively(e, saveElements));
      opus.Children.ToList().ForEach(DeleteOpusContentRecursively);
      SaveChanges();
      Func<IEnumerable<XElement>, Element, List<Element>> helper = null;
      var currentChapter = opus.Name;
      var chapterOrder = 1;
      helper = (nodes, parent) => {
        var ret = new List<Element>();
        var orderNr = 1;
        foreach (var elm in nodes) {
          Element newElm = null;
          # region Detect Element Type
          switch (elm.Attribute("Type").Value.ToLower()) {
            case "opus":
              # region OPUS
              // do nothing as this import runs on opus level already, simply assign current as start
              opus.Name = elm.Attribute("Name").NullSafeString();
              ((Opus)opus).Version = ((Opus)opus).Version + 1;
              break;
              # endregion
            case "section":
              # region SECTION
              if (elm.FirstNode != null && elm.FirstNode.NodeType == System.Xml.XmlNodeType.Text) {
                newElm = new Section {
                  Content = System.Text.Encoding.UTF8.GetBytes(((XText)elm.FirstNode).Value.Trim())
                };
              } else {
                newElm = new Section {
                  Content = System.Text.Encoding.UTF8.GetBytes("Empty Section")
                };
              }
              if (elm.Attribute("Name") == null || String.IsNullOrEmpty(elm.Attribute("Name").Value)) {
                newElm.Name = System.Text.Encoding.UTF8.GetString(newElm.Content);
              } else {
                newElm.Name = elm.Attribute("Name").Value;
                if (elm.FirstNode == null) {
                  newElm.Content = System.Text.Encoding.UTF8.GetBytes(elm.Attribute("Name").Value);
                }
              }
              // only if import has an opus/parent part
              if (elm.Parent != null && elm.Parent.Name == "Content") {
                newElm.Parent = opus;
                currentChapter = elm.Attribute("Name").NullSafeString();
              }
              currentChapter = currentChapter ?? "Import Files";
              break;
              # endregion
            case "text":
              # region TEXT
              newElm = new TextSnippet {
                Content = Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Value.CleanUpString(15)
              };
              break;
              # endregion
            case "image":
              # region IMAGE
              var imgType = "png";
              var error = false;
              byte[] content = null;
              switch (elm.Attribute("Method").NullSafeString()) {
                case "Base64":
                  // assume the image is stored internally as Base64
                  try {
                    content = Convert.FromBase64String(elm.Value.Trim());
                  } catch (Exception ex) {
                    Trace.TraceError(ex.Message);
                    error = true;
                  }
                  break;
                case "ServerResource":
                  // assume the content is a PK from Res table that points to a resource on this server's "Resources" blob store
                  try {
                    var resId = Int32.Parse(elm.Value.Trim());
                    var resData = Ctx.Resources.Find(resId);
                    if (resData != null) {
                      using (var blob = BlobFactory.GetBlobStorage(resData.ResourceId, BlobFactory.Container.Resources)) {
                        content = blob.Content;
                      }
                    }
                  } catch (Exception ex) {
                    Trace.TraceError(ex.Message);
                    error = true;
                  }
                  break;
                case "Url":
                  // assume the content is a URL somewhere else
                  try {
                    var uri = elm.Value.Trim();
                    var fileName = Path.GetFileName(uri);
                    var wc = new WebClient();
                    var fp = HttpContext.Current.Server.MapPath("~/Download/" + fileName);
                    wc.DownloadFile(elm.Value.Trim(), fp);
                    if (File.Exists(fp)) {
                      content = File.ReadAllBytes(fp);
                    }
                  } catch (Exception ex) {
                    Trace.TraceError(ex.Message);
                    error = true;
                  }
                  break;
                default:
                  var imgpath = elm.Value.Trim();
                  if (!String.IsNullOrEmpty(imgpath)) {
                    imgType = Path.GetExtension(imgpath).Substring(1); // kick the leading "."
                    try {
                      var fp = HttpContext.Current.Server.MapPath("~/Download/" + imgpath);
                      if (File.Exists(fp)) {
                        content = File.ReadAllBytes(fp);
                      }
                    } catch (Exception) {
                      error = true;
                    }
                  }
                  break;
              }
              if (error) break;
              // volume root
              var currentChapterResFolder = Ctx.Resources
                .OfType<ResourceFolder>()
                .SingleOrDefault(rf => rf.TypesOfResource == TypeOfResource.Content && rf.Project.Id == opus.Project.Id && rf.Parent == null);
              if (currentChapterResFolder == null) {
                // HACK: Usually the folder should be there, if not we create one on the fly, but that's not the proper way
                var localizeAttribute = typeof(TypeOfResource).GetField(TypeOfResource.Content.ToString()).GetCustomAttribute(typeof(DisplayAttribute), true) as DisplayAttribute;
                var locName = (localizeAttribute != null) ? localizeAttribute.GetName() : "Content";
                currentChapterResFolder = ResourceManager.Instance.AddResourceFolder(opus.Project, locName, TypeOfResource.Content, null);
              }
              // create folder per chapter
              var currentChapterFolder = Ctx.Resources
                .OfType<ResourceFolder>()
                .SingleOrDefault(rf => rf.Name == currentChapter && rf.Project.Id == opus.Project.Id);
              if (currentChapterFolder == null) {
                currentChapterFolder = new ResourceFolder {
                  ResourceId = Guid.NewGuid(), // we need an id even if there is no file relation to support the finder javascript (Guid == Hash)
                  Name = currentChapter,
                  Owner = user,
                  Project = opus.Project,
                  Parent = currentChapterResFolder,
                  TypesOfResource = TypeOfResource.Content,
                  OrderNr = chapterOrder++
                };
                Ctx.Resources.Add(currentChapterFolder);
                Ctx.SaveChanges();
              }
              var imageName = elm.Attribute("Name").Value.Trim();
              // try to find a resource at that location that already exists. In a restore we overwrite always
              var res = Ctx.Resources.OfType<ResourceFile>().FirstOrDefault(r => r.Name == imageName && r.Parent.Id == currentChapterFolder.Id && r.Project.Id == opus.Project.Id);
              // not there?
              if (res == null) {
                res = new ResourceFile {
                  Owner = user,
                  Name = imageName,
                  Project = opus.Project,
                  Parent = currentChapterFolder,
                  ResourceId = Guid.NewGuid(),
                  TypesOfResource = TypeOfResource.Content,
                  MimeType = "image/" + imgType
                };
              }
              // in any case write to blob store
              using (var blobImg = BlobFactory.GetBlobStorage(res.ResourceId, BlobFactory.Container.Resources)) {
                if (content != null && content.Length > 16) {
                  blobImg.Content = content;
                  blobImg.Save(() => Ctx.Resources.Add(res));
                }
                // adjust document 
                // create anyway, even if there is no physical image (we need to protect the document's structure, user can fix missing images later easily)
                newElm = new ImageSnippet {
                  // images used in the active content are referenced in the blob storage but stored in the elements tree independently
                  Content = content,
                  Name = res.Name,
                  Title = res.Name,
                  MimeType = res.MimeType
                };
                // get the image's properties, if not in the source doc, try to get from image itself
                var imgprops = new ImageProperties();
                if (elm.Attribute("Width") == null || elm.Attribute("Height") == null) {
                  using (var img = Image.FromStream(new MemoryStream(blobImg.Content) { Position = 0 })) {
                    if (img != null) {
                      imgprops.ImageWidth = imgprops.OriginalWidth = img.Width;
                      imgprops.ImageHeight = imgprops.OriginalHeight = img.Height;
                    } else {
                      imgprops.ImageWidth = imgprops.OriginalWidth = 100;
                      imgprops.ImageHeight = imgprops.OriginalHeight = 100;
                    }
                  }
                } else {
                  imgprops.ImageWidth = imgprops.OriginalWidth = Convert.ToInt32(elm.Attribute("Width").NullSafeString());
                  imgprops.ImageHeight = imgprops.OriginalHeight = Convert.ToInt32(elm.Attribute("Height").NullSafeString());
                }
                ((ImageSnippet)newElm).Properties = new JavaScriptSerializer().Serialize(imgprops);
              }
              break;
              # endregion
            case "listing":
              # region LISTING
              newElm = new ListingSnippet {
                Content = Encoding.UTF8.GetBytes(elm.Value.Trim()), //.Replace("\n", " "); - Causes problems with Listing widget. All data is displayed in one line
                Name = elm.Attribute("Name") == null ? "Listing" : elm.Attribute("Name").Value,
                Title = elm.Attribute("Name") == null ? "Listing" : elm.Attribute("Name").Value,
                Language = elm.Attribute("Language") == null ? "" : elm.Attribute("Language").Value,
                SyntaxHighlight = elm.Attribute("Highlight") == null || Boolean.Parse(elm.Attribute("Highlight").Value),
                LineNumbers = elm.Attribute("LineNumbers") == null || Boolean.Parse(elm.Attribute("LineNumbers").Value)
              };
              # endregion
              break;
            case "table":
              # region TABLE
              newElm = new TableSnippet {
                Content = System.Text.Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Attribute("Name") == null ? "Table" : elm.Attribute("Name").Value,
                Title = elm.Attribute("Name") == null ? "Table" : elm.Attribute("Name").Value,
                RepeatHeadRow = elm.Attribute("RepeatHeadRow") == null || Boolean.Parse(elm.Attribute("RepeatHeadRow").Value)
              };
              # endregion
              break;
            case "sidebar":
              # region SIDEBAR
              newElm = new SidebarSnippet {
                Content = Encoding.UTF8.GetBytes(elm.GetInnerXml()),
                Name = elm.Attribute("Name") == null ? "Sidebar" : elm.Attribute("Name").Value,
                SidebarType = elm.GetEnumAttribute<SidebarType>("SidebarType")
              };
              # endregion
              break;
            default:
              throw new NotSupportedException("Unknown snippet type found in source XML: " + elm.Attribute("Type").NullSafeString());
          }
          # endregion

          // take opus as existent, add else
          if (newElm == null) {
            opus.Children = helper(elm.Elements("Element"), opus);
          } else {
            if (elm.Elements("Element").Any()) {
              newElm.Children = helper(elm.Elements("Element"), newElm);
            }
            if (parent == null) {
              throw new InvalidOperationException("Parent must exist");
            }
            newElm.OrderNr = orderNr++;
            newElm.Parent = parent;
            ret.Add(newElm);
          }
        }
        return ret;
      };
      // invoke Content loader (assume each xml contains one Opus)
      try {
        if (xDoc.Root != null) {
          var restore = helper(from o in xDoc.Root.Elements("Element") select o, opus);
          restore.ForEach(o => Ctx.Elements.Add(o));
          SaveChanges();
        }
      } catch (Exception ex) {
        Trace.TraceError(ex.Message);
        // Restore the old state 
        opus.Children.AddRange(saveElements);
        SaveChanges();
      }
    }


    public MemoryStream CreateBackup(Opus opus) {
      Func<Element, XElement> helper = null;
      helper = element => {
        XElement currentTarget;
        var content = element.Content != null ? Encoding.UTF8.GetString(element.Content) : "";
        switch (element.WidgetName) {
          case "Section":
            currentTarget = new XElement("Element", new XAttribute("Type", "Section"), new XAttribute("Name", element.Name),
             content);
            break;
          case "Text":
            currentTarget = new XElement("Element", new XAttribute("Type", "Text"), new XAttribute("Name", element.Name),
              new XAttribute("Properties", element.Properties ?? ""),
              new XCData(content));
            break;
          case "Image":
            byte[] imgContent = null;
            if (element.Content != null) {
              if (element.Content.Length == 16) {
                using (var blob = BlobFactory.GetBlobStorage(new Guid(element.Content), BlobFactory.Container.Resources)) {
                  imgContent = blob.Content;
                }
              } else {
                imgContent = element.Content;
              }
            }
            currentTarget = new XElement("Element", new XAttribute("Type", "Image"), new XAttribute("Name", ((ImageSnippet)element).Title ?? "Image"),
              new XAttribute("Width", ((ImageSnippet)element).Width),
              new XAttribute("Height", ((ImageSnippet)element).Height),
              new XAttribute("Properties", element.Properties ?? ""),
              new XAttribute("Method", "Base64"), // currently the only method
              imgContent == null ? new XCData("") : new XCData(Convert.ToBase64String(imgContent)));
            break;
          case "Table":
            currentTarget = new XElement("Element", new XAttribute("Type", "Table"), new XAttribute("Name", element.Name),
              new XAttribute("Properties", element.Properties ?? ""),
              new XCData(content));
            break;
          case "Sidebar":
            currentTarget = new XElement("Element", new XAttribute("Type", "Sidebar"), new XAttribute("Name", ((SidebarSnippet)element).HeaderContent),
              new XAttribute("SidebarType", ((SidebarSnippet)element).SidebarType),
              new XAttribute("Properties", element.Properties ?? ""),
              new XCData(content));
            break;
          case "Listing":
            currentTarget = new XElement("Element", new XAttribute("Type", "Listing"), new XAttribute("Name", ((ListingSnippet)element).Title ?? "Listing"),
              new XAttribute("Properties", element.Properties ?? ""),
              new XCData(content));
            break;
          default:
            throw new NotImplementedException("Snippet of type " + element.WidgetName + " is currently not implemented for backup.");
        }
        if (element.HasChildren()) {
          currentTarget.Add(element.Children.Select(child => helper(child)));
        }
        return currentTarget;
      };
      var xDoc = new XDocument(new XElement("Content",
          new XAttribute("Type", "Opus"),
          new XAttribute("Name", opus.Name),
          new XAttribute("Version", opus.Version),
          new XAttribute("Properties", opus.Properties),
          new XAttribute("Variation", opus.Variation),
          opus.Children.Select(c => helper(c))));
      var ms = new MemoryStream();
      xDoc.Save(ms);
      return ms;
    }


    public Isbn ClaimIsbn(Published published) {
      if (published.Marketing == null) {
        Ctx.LoadProperty(published, p => p.Marketing);
      }
      if (published.Marketing.AssignIsbn) {
        // again
        var isbn = Ctx.Isbns.FirstOrDefault(p => p.AssignedTo.Id == published.Id) ??
                         Ctx.Isbns.FirstOrDefault(p => !p.Isbn.Claimed);
        if (isbn == null) {
          // TODO: Inform Admin
          return null;
        }
        isbn.Isbn.Claimed = false;
        isbn.AssignedTo = published;
        SaveChanges();
        return isbn.Isbn;
      }
      return null;
    }

    public void FreezeIsbn(Published published) {
      var isbn = Ctx.Isbns.FirstOrDefault(p => p.AssignedTo.Id == published.Id);
      if (isbn != null) {
        isbn.Isbn.Claimed = true;
        SaveChanges();
      }
    }

    public void MergeChaptersToOpus(int id, int[] fragmentIds, string userName) {
      var targetOpus = GetOpus(id, userName);
      var sourceOpusesChapters = targetOpus.Project.Opuses.SelectMany(o => o.Children).ToList();
      // we just change the parent and hang the element into the new position in the elements' tree
      var order = 1;
      foreach (var sourceChapter in fragmentIds.Select(i => sourceOpusesChapters.Single(c => c.Id == i))) {
        sourceChapter.Parent = targetOpus;
        sourceChapter.OrderNr = order++;
      }
      // deactivate empty opuses
      targetOpus.Project.Opuses.Where(o => !o.Children.Any()).ForEach(o => o.Active = false);
      // clean the order
      order = 1;
      targetOpus.Children.ForEach(c => c.OrderNr = order++);
      SaveChanges();
    }

    public bool CopyChaptersToOpus(int id, List<int> fragmentIds, string userName) {
      bool result = false;
      try {
        var targetOpus = GetOpus(id, userName);
        // extract what's not already in
        var newFragmentIds = fragmentIds.Except(targetOpus.Children.OfType<Section>().Select(s => s.Id));
        // that's what we need to copy, result is true if all of the copies are done
        result = newFragmentIds
          .Select(i => Ctx.Elements
            .OfType<Section>()
            .Single(e => e.Id == i))
          .Aggregate(result, (current, startElement) => current & CopyElementTree<Section>(startElement, targetOpus) != null);
        // after copy is done, we need to set the Order according to fragmentIds
        // however, any element might be a reference, so we resolve these first
        var normalizedIdsInOrder = (from i in fragmentIds
                                    let section = Ctx.Elements.OfType<Section>().Single(e => e.Id == i)
                                    select section.Parent.Id == targetOpus.Id ? section : Ctx.Elements.OfType<Section>().Single(s => s.Predecessor.Id == i)).ToList();
        var newOrder = 1;
        foreach (var i in normalizedIdsInOrder) {
          i.OrderNr = newOrder++;
        }
        result = true;
      } catch (Exception ex) {
        Logger.Error(ex, "ProjectManager.CopyChaptersToOpus");
      }
      return result;
    }


    public bool AssignToLeadAuthor(int projectId, bool keep, string userName, int newUserId) {
      // get current project
      var project = GetProject(projectId, userName);
      // get the role data for current user to take this over to new lead
      // condition assures that only teamlead can throw itself out
      try {
        var oldLead = project.Team.Members.Single(t => t.TeamLead && t.Member.UserName == userName);
        var role = oldLead.Role;
        using (var scope = Ctx.BeginTransaction()) {
          // assign new
          if (project.Team.Members.All(t => t.Member.Id != newUserId)) {
            // user is not in the team yet
            var newMember = new TeamMember {
              Member = Manager<UserManager>.Instance.GetUser(newUserId),
              Team = project.Team,
              TeamLead = true,
              Role = role,
              Pending = false
            };
            project.Team.Members.Add(newMember);
          } else {
            // user is already in the team, just skip the roles
            var oldMember = project.Team.Members.Single(t => t.Member.Id == newUserId);
            oldMember.Role = role;
            oldMember.Pending = false;
            oldMember.TeamLead = false;
          }
          // keep or remove old          
          if (keep) {
            oldLead.TeamLead = false;
            oldLead.Role = new TeamRole {
              Team = project.Team,
              ContributorRoles = ContributorRole.Author
            };
          } else {
            project.Team.Members.Remove(oldLead);
          }
          SaveChanges();
          scope.Commit();
        }
      } catch {
        return false;
      }
      // If the new owner is not in the team, add to team
      return true;
    }

    public void AddResourceFilesToPublished(Published published, int[] targetIds) {
      var publ = Ctx.Published.Find(published.Id); // get new instance here
      if (targetIds == null) {
        foreach (var rf in publ.ResourceFiles.ToList()) {
          publ.ResourceFiles.Remove(rf);
        }
      } else {
        publ.ResourceFiles = Ctx.Resources
          .OfType<ResourceFile>()
          .Where(r => targetIds.Any(t => t == r.Id))
          .ToList();
      }
      SaveChanges();
    }

    public void AddResourceFilesToPublished(Published published, Opus opus) {
      var publ = Ctx.Published.Find(published.Id); // get new instance here
      publ.ResourceFiles = opus
        .Project
        .Resources
        .Where(r => r.TypesOfResource == TypeOfResource.Project)
        .OfType<ResourceFile>()
        .ToList();
      SaveChanges();
    }

    public void UnsetSingularFragments(int id, int[] isSingular, string userName) {
      var publ = GetPublished(id, userName);
      foreach (var f in publ.FrozenFragments) {
        f.SingularEntity = isSingular.Contains(f.Id);
      }
      SaveChanges();
    }

    public FeedPreview QuickPublish(int id, string userName) {
      var publ = GetOrCreatePublished(id, true, userName);
      // make some assumptions to shorten the procedure
      PublishContentToFrozenState(publ.Id, id, publ.NavLevel.GetValueOrDefault()); // 0 = whole document
      var feed = new FeedPreview {
        Published = publ,
        User = publ.Owner
      };
      return feed;
    }

    public IEnumerable<Opus> GetPublishables(List<int> ids, string userName, out List<KeyValuePair<Opus, string>> missReason) {
      var miss = new List<KeyValuePair<Opus, string>>();
      var projects = Ctx.Projects.Where(p => ids.Any(i => i == p.Id)).ToList();
      var publishables = projects
        .SelectMany(p => p.GetPublishableOpuses(ref miss))
        .ToList();
      missReason = new List<KeyValuePair<Opus, string>>(miss);
      return publishables;
    }

    public ProjectManager() {
    }

    /// <summary>
    /// returns image as thumbnail
    /// </summary>
    /// <param name="id">primary key of object table</param>
    /// <param name="c">name of object</param>
    /// <param name="res">resolution wwwwxhhh, e.g. 80x150</param>
    /// <param name="userName">user's name</param>
    /// <param name="nc">suppress cache if true</param>
    /// <param name="href">Only for epub the href of manifest item (all other types ignore this)</param>
    /// <returns></returns>
    public byte[] GetImage(int id, string c, string res, string userName, bool nc = false, string href = "") {
      byte[] bytes = null;
      var ctx = System.Web.HttpContext.Current;
      if (ctx.Cache[GetThumbnailCacheId(id, c, res)] == null || nc) {
        int w, h;
        res.GetIntPair(out w, out h);
        Published publ = null;
        var img = "";
        switch (c.ToLowerInvariant()) {
          case "profilebyname":
            // assume user is logged on
            var p0 = UserManager.Instance.GetUserByName(userName).Profile;
            img += p0.Gender == Gender.Male ? "-m" : "-f";
            bytes = p0.Image;
            break;
          case "profile":
            var p = UserProfileManager.Instance.GetProfile(id);
            bytes = p.Image;
            img += p.Gender == Gender.Male ? "-m" : "-f";
            break;
          case "home":
            publ = ReaderManager.Instance.GetPublishedById(id);
            bytes = publ.CoverImage.GetFinalCoverBytes(publ);
            break;
          case "epub":
            var work = ReaderManager.Instance.GetWork(id, true);
            var item = work.ExternalBook.PackageData.Manifest.Items.FirstOrDefault(i => i.Href == href);
            bytes = item != null ? item.Data : null;
            break;
          case "reader":
            publ = ReaderManager.Instance.GetPublishedById(id);
            bytes = publ.CoverImage.GetFinalCoverBytes(publ);
            break;
          case "project":
            bytes = GetProject(id, userName).Image;
            break;
          case "projectcover":
            publ = ReaderManager.Instance.GetPublishedById(id, pbl => pbl.Authors.Select(a => a.Roles));
            bytes = publ.CoverImage.GetFinalCoverBytes(publ);
            break;
          case "team":
            bytes = GetTeam(id).Image;
            break;
          case "memberthumbnail":
            var p2 = UserProfileManager.Instance.GetProfile(id);
            bytes = p2.Image;
            img += p2.Gender == Gender.Male ? "-m" : "-f";
            break;
          case "finderresource":
            w = w == 0 ? 50 : w;
            h = h == 0 ? 50 : h;
            c = "editorresource";
            goto case "editorresource";
          case "editor":
          case "editorresource":
            var resource = ProjectManager.Instance.GetResource(id) as ResourceFile;
            if (resource != null) {
              using (var blob = BlobFactory.GetBlobStorage(resource.ResourceId, BlobFactory.Container.Resources)) {
                switch (resource.MimeType) {
                  case "image/svg+xml":
                    // from meta data we look for the related export
                    if (blob.MetaData.ContainsKey("img")) {
                      var imgId = Convert.ToInt32(blob.MetaData["img"]);
                      resource = ProjectManager.Instance.GetResource(imgId) as ResourceFile;
                      if (resource != null) {
                        using (var blobImg = BlobFactory.GetBlobStorage(resource.ResourceId, BlobFactory.Container.Resources)) {
                          bytes = blobImg.Content;
                        }
                      }
                    }
                    break;
                  default:
                    bytes = blob.Content;
                    break;
                }
              }
            }
            break;
          case "barcode":
            bytes = GetBarCode(href, w, h, href);
            w = h = 0; // suppress shrink
            break;
          case "imprintlogo":
            var imprint = Ctx.Imprints.Find(id);
            bytes = imprint.CompanyLogo;
            break;
        }
        var staticImage = false; // do not cache static image
        if (bytes == null) {
          staticImage = true;
          // nothing in the DB, so we pull a placeholder
          bytes = ImageUtil.GetStaticImage(ctx.Server.MapPath(
            String.Format("~/Content/icons/{0}/{1}{2}.png", c, "nopic", img)));
        }
        // shrink only if requested
        if (w > 0 && h > 0) {
          bytes = ImageUtil.GetThumbnailImage(bytes, w, h);
        }
        if (!nc && bytes != null && !staticImage) {
          ctx.Cache.Add(GetThumbnailCacheId(id, c, res), bytes, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(60), CacheItemPriority.Normal, null);
        }
      } else {
        bytes = (byte[])ctx.Cache[GetThumbnailCacheId(id, c, res)];
      }
      return bytes;
    }

    private static string GetThumbnailCacheId(int id, string c, string res) {
      return String.Format("ThumbnailCache-", id, c, res);
    }

    private static byte[] GetBarCode(string code, int w, int h, string title) {
      var bcl = new BarcodeLib(code, BarCodeType.ISBN) {
        Width = w,
        Height = h,
        ForeColor = Color.Black,
        BackColor = Color.White,
        ImageFormat = ImageFormat.Png,
        IncludeLabel = true,
        LabelPosition = LabelPositions.TOPCENTER,
        RawData = title
      };
      bcl.Encode();
      return bcl.GetImageData(SaveTypes.PNG);
    }


    public MarketingPackage GetAssignedMarketingPackage(int projectId) {
      return Ctx.Projects.Find(projectId).Marketing;
    }

    /// <summary>
    ///  Each user can have one imprint, which is accessible while publishing a project as lead author.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public ImprintAddress GetImprintForUser(string userName) {
      var imprint = Ctx.Imprints
        .Include(i => i.Owner)
        .Include(i => i.Address)
        .Include(i => i.Address.Country)
        .SingleOrDefault(i => i.Owner.UserName == userName);
      if (imprint == null) return new ImprintAddress();
      var country = UserProfileManager.Instance.GetCountryList().Single(c => c.Name == imprint.Address.Country);
      var viewModel = new ImprintAddress {
        Name = imprint.Name,
        Description = imprint.Description,
        AboutUs = imprint.AboutUs,
        OwnerId = imprint.Owner.Id,
        Firm = imprint.Firm,
        Url = imprint.Url,
        CompanyLogo = imprint.CompanyLogo,
        StreetNumber = imprint.Address.StreetNumber,
        Zip = imprint.Address.Zip,
        City = imprint.Address.City,
        Region = imprint.Address.Region,
        Country = country,
        CountryId = country.Id,
        ImprintId = imprint.Id,
        AddressId = imprint.Address.Id
      };
      return viewModel;
    }

    private Imprint GetImprint(Texxtoor.ViewModels.Content.ImprintAddress viewModel, User owner) {
      var imprint = new Imprint {
        Id = viewModel.ImprintId,
        Name = viewModel.Name,
        Description = viewModel.Description,
        AboutUs = viewModel.AboutUs,
        Firm = viewModel.Firm,
        Url = viewModel.Url,
        CompanyLogo = viewModel.CompanyLogo,
        Owner = owner
      };
      return imprint;
    }

    private AddressBook GetAddressBook(ImprintAddress viewModel) {
      var address = new AddressBook {
        Id = viewModel.AddressId,
        Name = viewModel.Name,
        City = viewModel.City,
        Country = viewModel.Country.Name,
        Region = viewModel.Region,
        StreetNumber = viewModel.StreetNumber,
        Zip = viewModel.Zip,
        Invoice = true,
        Default = true
      };
      return address;
    }

    public bool AddOrUpdateAddressForImprint(int id, AddressBook address) {
      var imprint = Ctx.Imprints.Find(id);
      if (imprint != null) {
        if (address.Id != 0) {
          var oldAddress = Ctx.AddressBook.Find(address.Id);
          if (oldAddress != null) {
            address.CopyProperties<AddressBook>(oldAddress,
              a => a.Name,
              a => a.City,
              a => a.Country,
              a => a.Region,
              a => a.StreetNumber,
              a => a.Zip
              );
            SaveChanges();
            return true;
          }
        }
        using (var scope = Ctx.BeginTransaction()) {
          address = Ctx.AddressBook.Add(address);
          SaveChanges();
          imprint.Address = address;
          SaveChanges();
          scope.Commit();
        }
      }
      return false;
    }

    public ImprintAddress AddOrUpdateImprint(ImprintAddress imprintaddress, string userName) {
      try {
        var owner = UserManager.Instance.GetUserByName(userName);
        var imprint = GetImprint(imprintaddress, owner);
        var addressbook = GetAddressBook(imprintaddress);
        if (imprint.Id == 0) {
          if (!AddOrUpdateAddressForImprint(imprint.Id, addressbook)) {
            // no address because imprint is new
            addressbook = new AddressBook {
              Name = String.Format("Address for Imprint {0}", imprintaddress.Name),
              City = imprintaddress.City,
              Country = imprintaddress.Country.Name,
              Region = imprintaddress.Region,
              StreetNumber = imprintaddress.StreetNumber,
              Zip = imprintaddress.Zip
            };
            var country = Ctx.Countries.Find(imprintaddress.CountryId);
            addressbook.Country = country.Name;
            Ctx.AddressBook.Add(addressbook);
          }
          Ctx.Imprints.Add(imprint);
          imprint.Address = addressbook;
          SaveChanges();
        } else {
          var country = Ctx.Countries.Find(imprintaddress.CountryId);
          addressbook.Country = country.Name;
          Ctx.Entry(addressbook).State = EntityState.Modified;
          SaveChanges();
          imprint.Address = addressbook;
          Ctx.Entry(imprint).State = EntityState.Modified;
          SaveChanges();
        }
        return GetImprintForUser(userName);
      } catch (Exception ex) {
        Trace.WriteLine(ex.Message);
        return null;
      }
    }

    public Tuple<int, int> SaveImprintLogo(int id, HttpPostedFileBase file) {
      var imprint = Ctx.Imprints
        .Include(i => i.Owner)
        .Include(i => i.Address)
        .Include(i => i.Address.Country)
        .SingleOrDefault(i => i.Id == id);
      if (imprint != null && file != null && file.InputStream != null) {
        try {
          file.InputStream.Seek(0, SeekOrigin.Begin);
          var img = Image.FromStream(file.InputStream, false, true);
          imprint.Id = id;
          file.InputStream.Seek(0, SeekOrigin.Begin);
          imprint.CompanyLogo = file.InputStream.ReadToEnd();
          Ctx.Entry(imprint).Property(i => i.CompanyLogo).IsModified = true;
          SaveChanges();
          return new Tuple<int, int>(img.Width, img.Height);
        } catch (ArgumentException e) {
          // cannot read image file
        }
      }
      return null;
    }


    public IEnumerable<IsbnStore> GetIsbnForImprint(int id, string userName) {
      try {
        return Ctx.Imprints
          .Include(i => i.Isbns)
          .Single(i => i.Id == id && i.Owner.UserName == userName)
          .Isbns;
      } catch (Exception ex) {
        Trace.TraceError(ex.Message);
        return null;
      }
    }

    public bool SaveIsbnToImprint(int id, string isbnList, string userName) {
      var imprint = Ctx.Imprints
        .Include(i => i.Isbns)
        .SingleOrDefault(i => i.Id == id && i.Owner.UserName == userName);
      if (imprint != null) {
        var list = isbnList.Split('\n').ToList();
        using (var scope = Ctx.BeginTransaction()) {
          if (imprint.Isbns != null) {
            imprint.Isbns.RemoveAll(i => !i.Isbn.Claimed);
            SaveChanges();
          }
          var validator = new ISBN13();
          list.ForEach(i => {
            validator.ISBN = i;
            // remove invalid
            if (validator.IsValid) {
              // remove duplicates
              if (imprint.Isbns.All(ei => ei.Isbn.Isbn13 != i)) {
                imprint.Isbns.Add(new IsbnStore {
                  Isbn = new Isbn() { Isbn13 = i }
                });
              }
            }
          });
          scope.Commit();
        }
        return true;
      }
      return false;
    }

    public Imprint GetImprint(string userName) {
      return Ctx.Imprints.SingleOrDefault(i => i.Owner.UserName == userName);
    }

    public void SetPublishingTarget(int id, bool publisher, bool isGobal, string userName) {
      var publ = GetPublished(id, userName);
      publ.ExternalPublisher.RequestsPublishing = isGobal;
      var imprint = GetImprint(userName);
      if (publisher && imprint != null) {
        // use and connect user's Imprint        
        imprint.Published = publ;
        publ.Imprint = imprint;
        publ.Publisher = imprint.Name;
      } else {
        publ.Publisher = "texxtoor";
        publ.Imprint = null;
        if (imprint != null) {
          imprint.Published = null;
        }
      }
      SaveChanges();
    }

    public bool SavePublishedTemplate(int id, int[] templateGroupId, string userName) {
      var publ = GetPublished(id, userName);
      var templates = Ctx.TemplateGroups.Where(g => templateGroupId.Any(t => t == g.Id));
      // we check the name and write if exists, target field is not typed yet
      if (publ.PreferredTemplateGroup != null && publ.PreferredTemplateGroup.Any()) {
        publ.PreferredTemplateGroup.Clear();
      }
      publ.PreferredTemplateGroup = templates.ToList();
      return SaveChanges() > 0;
    }

    public Team GetTeamLeadForOpus(int id, string userName) {
      var opus = GetOpus(id, userName);
      return opus.Project.Team;
    }

    public TemplateGroup GetTemplateGroup(int templateGroupId) {
      return Ctx.TemplateGroups.Find(templateGroupId);
    }

    public TemplateGroup GetTemplateGroup(string localeId) {
      if (String.IsNullOrEmpty(localeId)) {
        localeId = CurrentCulture;
      }
      return Ctx.TemplateGroups.Where(t => t.LocaleId == localeId).ToList().Single(t => t.IsCommonTemplate);
    }


    public bool MoveOpusToProject(int id, int projectId, bool copyResources, string userName) {
      var opus = GetOpus(id, userName);
      var project = GetProject(projectId, userName);
      if (opus != null && project != null) {
        opus.Project = project;
        if (copyResources) {
          // TODO: Copy resources
        }
        return SaveChanges() == 1;
      }
      return false;
    }



    public void AddMilestoneToOpus(int opusId, Milestone newMilestone) {
      var opus = GetOpusInternal(opusId, o => o.Milestones);
      if (opus.Milestones == null) {
        opus.Milestones = new List<Milestone>();
      }
      // we re-request the relations to assure proper context
      newMilestone.Opus = opus;
      newMilestone.Owner = GetTeamMember(newMilestone.Owner.Id);
      opus.Milestones.Add(newMilestone);
      SaveChanges();
    }

    public Opus RepairEmptyDocument(int opusId) {
      var opus = GetOpusInternal(opusId);
      if (!opus.Children.Any()) {
        var c = new Section { Content = Encoding.UTF8.GetBytes(ControllerResources.ProjectManager_CreateFromTemplate_First_Chapter), Name = ControllerResources.ProjectManager_CreateFromTemplate_First_Chapter, OrderNr = 1, Parent = opus };
        opus.Children.Add(c);
        SaveChanges();
      }
      return opus;
    }

    public bool SaveProject(Project project, string userName) {
      var prj = GetProject(project.Id, userName);
      if (prj == null) return false;
      project.CopyProperties<Project>(prj, p => p.Name, p => p.Short, p => p.Description);
      return SaveChanges() > 0;
    }

    public void RemoveProjectImage(int id, string userName) {
      var prj = GetProject(id, userName);
      prj.Image = null;
      SaveChanges();
    }


    public bool ChangeOpusVariation(int opusId, VariationType variation, string userName) {
      var opus = GetOpus(opusId, userName);
      opus.Variation = variation;
      return SaveChanges() == 1;
    }
  }
}