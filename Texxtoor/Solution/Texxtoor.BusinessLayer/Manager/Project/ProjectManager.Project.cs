using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BusinessLayer.Attributes;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Marketing;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.BusinessLayer {

  /// <summary>
  /// All BLL functions for projects.
  /// </summary>
  public partial class ProjectManager {

    # region Project Functions

    public Project GetProject(int projectId, string userName) {
      return GetProject(projectId, userName, null);
    }

    public Project GetProject(int projectId, string userName, params Expression<Func<Project, object>>[] loadProperties) {
      var prj = GetUsersProjectWithMembers(projectId, userName);
      if (loadProperties == null) return prj;
      foreach (var expression in loadProperties) {
        Ctx.LoadProperty<Project>(prj, expression);
      }
      return prj;
    }

    public IQueryable<Project> GetProjectsWhereUserIsMember(string userName) {
      var prjs = Ctx.Projects
        .Include(p => p.Team)
        .Include(p => p.Team.Members)
        .Include(p => p.Team.Members.Select(m => m.Member))
        .Where(p => p.Team.Members.Any(m => m.Member.UserName == userName));
      return prjs;
    }

    public IQueryable<Opus> GetOpuses(int projectId, string userName, bool activeOnly) {
      var prj = GetUsersProjectWithMembers(projectId, userName);
      if (prj == null) return new List<Opus>().AsQueryable(); // return empty list to allow concenations in Linq
      var ops = prj.Opuses
        .Where(o => activeOnly && o.Active || !activeOnly).AsQueryable();
      return ops;
    }

    private Project GetUsersProjectWithMembers(int projectId, string username) {
      var prj = Ctx.Projects
        .Include(p => p.Opuses)
        .Include(p => p.Team)
        .Include(p => p.Team.Members)
        .Include(p => p.Team.Members.Select(m => m.Member))
        .Include(p => p.Team.Members.Select(m => m.Role))
        .Include(p => p.Marketing)
        .Include(p => p.Published) // for traffic light icon
        .FirstOrDefault(p => p.Id == projectId);
      if (prj == null) return null;
      return prj.Team.Members.Any(m => m.Member.UserName == username) ? prj : null;
    }

    public IEnumerable<Project> GetUsersProjectsWithMembers(string userName, bool activeOnly) {
      var user = GetCurrentUser(userName);
      var projects = Ctx.Projects
        .Include(p => p.Opuses)
        .Include(p => p.Team)
        .Include(p => p.Team.Members)
        .Include(p => p.Team.Members.Select(m => m.Member))
        .Include(p => p.Team.Members.Select(m => m.Role))
        .Include(p => p.Marketing)
        .Include(p => p.Published) // for traffic light icon
        .ToList()
        .Where(p => p.Active == activeOnly)
        .Where(p => p.Team.Members.Any(m => m.Member.Id == user.Id))
        .OrderByDescending(x => x.CreatedAt)
        .ThenBy(x => x.Name);
      return projects;
    }

    public Dictionary<int, User> GetProjectsTeamLeader(IEnumerable<Project> projects) {
      return projects.ToDictionary(prj => prj.Id, prj => GetTeamLeader(prj.Team.Id));
    }

    public Project GetActiveProjectWithMembers(int id, string userName) {
      var prj = Ctx.Projects
        .Include(p => p.Team)
        .Include(p => p.Team.Members)
        .Include(p => p.Team.Members.Select(m => m.Member))
        .Include(p => p.Team.Members.Select(m => m.Role))
        .Include(p => p.Marketing)
        .Include(p => p.Resources)
        .SingleOrDefault(e => e.Id == id && e.Active);
      if (prj == null) return null;
      if (prj.Team.Members.All(m => m.Member.UserName != UserName)) return null;
      return prj;
    }

    [UserActivityAttribute("Create Project", 50)]
    public Project CreateProject(string userName, string name, string @short, string description, string terms, bool approveTerms, int teamId, NameValueCollection values, bool withMileStones = false) {
      var user = GetCurrentUser(userName);
      var team = Ctx.Teams.SingleOrDefault(t => t.Id == teamId);
      Project project = null;
      using (var scope = AsUnitOfWork().BeginTransaction()) {
        TeamMember owner;
        if (team == null) {
          // create a new team on the fly for this project
          team = new Team {
            Active = true,
            Name = String.Format(ControllerResources.ProjectManager_CreateProject__0__Team, name),
            Description = description,
          };
          // set the creator as team lead, each project must have at least the TeamLead Role
          var lead = new TeamMember {
            Role = new TeamRole {
              ContributorRoles = ContributorRole.Author,
              Team = team
            },
            TeamLead = true,
            Member = user,
            Team = team,
            Pending = false // the team's lead must not confirm itself
          };
          // the owner of all automatic created tasks is the lead by default. The lead can use the delegate function to assign other members.
          owner = lead;
          Ctx.TeamMembers.Add(lead);
        } else {
          // there is a team already, so we pull the lead
          owner = team.Members.First(m => m.TeamLead);
        }

        // create the project
        project = new Project {
          Name = name,
          Short = @short,
          Description = description,
          TermsAndConditions = terms,
          ApproveTerms = approveTerms,
          Team = team,
          Active = true
        };

        // create the default version of the associated opus
        var opus = new Opus {
          Name = name,
          Project = project,
          Version = 1
        };
        // use template for first opus, template data are in Collection
        if (values != null) {
          CreateFromTemplate(opus, values);
        } else {
          var c = new Section { Content = System.Text.Encoding.UTF8.GetBytes(ControllerResources.ProjectManager_CreateProject_Chapter), Name = "Chapter", OrderNr = 1, Parent = opus };
          opus.Children.Add(c);
        }
        if (withMileStones && values != null) {
          var mstn = CreateDefaultMileStones(owner, opus);
          CreateMilestonesFromFormValues(opus, owner, mstn, values);
        }
        // create a default marketing package (for convenience)
        var mp = new MarketingPackage {
          AssignIsbn = false,
          AssignIsbnADOI = false,
          CreateRssFeed = true,
          CreateSocialPlatformInstances = true,
          Description = String.Format(ControllerResources.ProjectManager_CreateProject_Primary_Marketing_Package_for_project_, project.Name),
          BasePrice = 0,
          MarketingType = MarketingPackageType.CreativeCommon,
          RegisterForLibraries = false,
          Owner = owner.Member,
          PackageBasePrice = 0,
          ShareContent = true,
          Name = String.Format("{0} {1}", ControllerResources.ProjectManager_CreateProject__Package, project.Name)
        };
        Ctx.MarketingPackages.Add(mp);
        project.Marketing = mp;
        // save all changes
        Ctx.Elements.Add(opus);
        Ctx.Projects.Add(project);
        Ctx.SaveChanges();
        scope.Commit();
      }
      var homeScreen = UserProfileManager.Instance.GetHomeScreenInfo(userName);
      return project;
    }

    public bool DeactivateProject(int id, string userName) {
      var prj = Ctx.Projects
        .Include(p => p.Opuses)
        .Single(e => e.Id == id);
      var tl = GetTeamLeader(prj.Team.Id);
      if (tl.UserName == userName) {
        prj.Active = false;
        prj.Opuses.ToList().ForEach(o => o.Active = false);
        Ctx.SaveChanges();
        var homeScreen = UserProfileManager.Instance.GetHomeScreenInfo(userName);
        return true;
      }
      return false;
    }

    public bool ReactivateProject(int id, string userName) {
      var prj = Ctx.Projects
        .Include(p => p.Opuses)
        .Single(e => e.Id == id);
      var tl = GetTeamLeader(prj.Team.Id);
      if (tl.UserName == userName) {
        prj.Active = true;
        prj.Opuses.ToList().ForEach(o => o.Active = true);
        Ctx.SaveChanges();
        return true;
      }
      return false;
    }


    public bool DeleteProject(int id, string userName) {
      var prj = Ctx.Projects
        .Include(p => p.Opuses)
        .Single(e => e.Id == id);
      var tl = GetTeamLeader(prj.Team.Id);
      if (tl.UserName == userName) {
        prj.Opuses.ToList().ForEach(o => Ctx.Opuses.Remove(o));
        Ctx.Projects.Remove(prj);
        Ctx.SaveChanges();
        return true;
      }
      return false;
    }


    public void DeleteProjectDeep(int projectId) {
      // user internally for testing purposes, user functions should not delete anthing
      // we go back from elements up to project to delete in the right order
      var prj = Ctx.Projects
        .Include(p => p.Team)
        .Include(p => p.Team.Members)
        .FirstOrDefault(p => p.Id == projectId);
      if (prj != null) {
        var opuses = Ctx.Opuses.Where(o => o.Project.Id == projectId);
        foreach (var opus in opuses) {
          if (opus == null) continue;
          if (opus.HasChildren()) {
            RemoveElementsRecursivelyBackOrder(opus.Children);
          }
          Ctx.Opuses.Remove(opus);
        }
        Ctx.SaveChanges();
        // remove team but leave members intact
        if (prj.Team != null) {
          if (prj.Team.Members.Any()) {
            prj.Team.Members.ForEach(tm => Ctx.TeamMembers.Remove(tm));
          }
          Ctx.Teams.Remove(prj.Team);
        }
        Ctx.Projects.Remove(prj);
        // remove resources associated with project
        var res = Ctx.Resources.Where(r => r.Project.Id == projectId);
        res.ForEach(r => Ctx.Resources.Remove(r));
      }
      SaveChanges();
    }

    private void RemoveElementsRecursivelyBackOrder(IEnumerable<Element> elements) {
      foreach (var element in elements.ToList()) {
        if (element.HasChildren()) {
          RemoveElementsRecursivelyBackOrder(element.Children);
        }
        // reached leaf element
        Ctx.Elements.Remove(element);
      }
    }

    public List<Milestone> CreateDefaultMileStones(TeamMember owner, Opus opus) {
      // this is an easy way to implement a workflow
      // TODO: Make this configurable
      var ms = new List<Milestone>();
      var m1 = new Milestone { Id = 1, Name = ControllerResources.ProjectManager_CreateDefaultMileStones_Writing, Description = ControllerResources.ProjectManager_CreateDefaultMileStones_Create_Content, DateDue = DateTime.Now.AddDays(30), DateAssigned = DateTime.Now, Owner = owner, Progress = 0, Opus = opus };
      var m2 = new Milestone { Id = 2, Name = ControllerResources.ProjectManager_CreateDefaultMileStones_Illustrations, Description = ControllerResources.ProjectManager_CreateDefaultMileStones_Create_Illustrations, DateDue = DateTime.Now.AddDays(40), DateAssigned = DateTime.Now, Owner = owner, Progress = 0, Opus = opus };
      var m3 = new Milestone { Id = 3, Name = ControllerResources.ProjectManager_CreateDefaultMileStones_CopyEditing, Description = ControllerResources.ProjectManager_CreateDefaultMileStones_Copy_Editing, DateDue = DateTime.Now.AddDays(50), DateAssigned = DateTime.Now, Owner = owner, Progress = 0, Opus = opus };
      m2.NextMilestone = m3;
      var m4 = new Milestone { Id = 4, Name = ControllerResources.ProjectManager_CreateDefaultMileStones_Proof, Description = ControllerResources.ProjectManager_CreateDefaultMileStones_Proof_Read, DateDue = DateTime.Now.AddDays(55), DateAssigned = DateTime.Now, Owner = owner, Progress = 0, Opus = opus };
      m3.NextMilestone = m4;
      var m5 = new Milestone { Id = 5, Name = ControllerResources.ProjectManager_CreateDefaultMileStones_Marketing, Description = ControllerResources.ProjectManager_CreateDefaultMileStones_Create_Marketing_Package, DateDue = DateTime.Now.AddDays(56), DateAssigned = DateTime.Now, Owner = owner, Progress = 0, Opus = opus };
      ms.AddRange(new Milestone[] { m1, m2, m3, m4, m5 });
      return ms;
    }

    # endregion

    /// <summary>
    /// Get default 
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    public IEnumerable<TemplateGroup> GetTemplatesForTenant(GroupKind group) {
      return Ctx.TemplateGroups.Where(t => t.Group == group);
    }

    public IEnumerable<TemplateGroup> GetTemplatesForTenant(Opus opus, GroupKind? group = null) {
      var teamMembers = GetTeamMembersOfOpus(opus).Select(m => m.Member);
      var tenant = Ctx.Tenants
        .Include(t => t.Users)
        .ToList()
        .FirstOrDefault(t => teamMembers.Intersect(t.Users).Any());
      if (group == null) {
        return tenant == null
                 ? Ctx.TemplateGroups.Where(t => t.Owner == null &&
                  (t.Group == GroupKind.Epub || t.Group == GroupKind.Html || t.Group == GroupKind.Pdf || t.Group == GroupKind.Rss))
                 : Ctx.TemplateGroups.Where(t => t.Owner.Id == tenant.Id &&
                 (t.Group == GroupKind.Epub || t.Group == GroupKind.Html || t.Group == GroupKind.Pdf || t.Group == GroupKind.Rss));
      } else {
        return tenant == null
                 ? Ctx.TemplateGroups.Where(t => t.Owner == null && t.Group == group)
                 : Ctx.TemplateGroups.Where(t => t.Owner.Id == tenant.Id && t.Group == group);
      }
    }

    public Project CreateSampleProject(string userName) {
      throw new NotImplementedException();
    }
  }
}