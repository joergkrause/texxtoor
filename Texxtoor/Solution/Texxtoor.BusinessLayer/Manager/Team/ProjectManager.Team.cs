using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Texxtoor.BaseLibrary.Core.Logging;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.ViewModels.Author;

namespace Texxtoor.BusinessLayer {

  /// <summary>
  /// All BLL functions for projects.
  /// </summary>
  public partial class ProjectManager {

    # region Team Functions


    public void ChangeTeamInfo(int id, string elementId, string value, string userName) {
      var team = Ctx.Teams.Find(id);
      if (team.TeamLead.UserName == userName)
      {
        switch (elementId)
        {
          case "teamName":
            team.Name = value;
            break;
          case "teamDescription":
            team.Description = value;
            break;
        }
        SaveChanges();
      }
    }

    /// <summary>
    /// Check whether a user is the specified team's lead.
    /// </summary>
    /// <param name="teamId">Team Id</param>
    /// <param name="username">User Name</param>
    /// <returns>Returns <c>true</c> if the user is the team's lead.</returns>
    public bool MemberIsTeamLead(int teamId, string username) {
      var team = LoadTeamWithMembers(teamId);
      return team.Members.Any(m => m.TeamLead && m.Member.UserName == username);
    }

    /// <summary>
    /// Check whether a user is a specified team's member.
    /// </summary>
    /// <param name="teamId">Team Id</param>
    /// <param name="username">User Name</param>
    /// <returns>Returns <c>true</c> if the user is a member of the team.</returns>
    public bool MemberIsTeamMember(int teamId, string username) {
      var user = GetCurrentUser(username);
      var team = LoadTeamWithMembers(teamId);
      return team.Members.Any(m => !m.TeamLead && m.Member.Id == user.Id);
    }

    /// <summary>
    /// Retrieve a team and load all navigation properties.
    /// </summary>
    /// <param name="teamId"></param>
    /// <returns></returns>
    private Team LoadTeamWithMembers(int teamId) {
      var team = Ctx.Teams
        .Include(t => t.Members)
        .Include(t => t.Members.Select(m => m.Member))
        .Include(t => t.Members.Select(m => m.Role))
        .FirstOrDefault(t => t.Id == teamId);
      return team;
    }

    /// <summary>
    /// Get all members of a team, optionally filtered by a project, including all role data.
    /// </summary>
    /// <param name="teamId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public IEnumerable<TeamMemberModel> GetTeamMembersWithRoles(int teamId, int? projectId) {
      try {
        var members = Ctx.TeamMembers
                         .Include(t => t.Member.Roles)
                         .Where(m => m.Team.Id == teamId)
                         .GroupBy(m => m.Member)
                         .OrderBy(m => m.Key.UserName)
                         .ToList()
                         .Select(g => new TeamMemberModel {
                           Member = g.Key,
                           TeamMember = g.FirstOrDefault(),
                           Projects = GetProjectsWhereUserIsMember(g.Key.UserName).ToList(),
                           Roles = g.Select(x => x.Role),
                           Profile = g.Key.Profile
                         })
                         .ToList();
        members.ForEach(m => m.Profile = Ctx.UserProfiles
          .Include(u => u.Rating)
          .Include(c => c.ContributorMatrix)
          .FirstOrDefault(up => up.User.Id == m.Member.Id));
        return members;
      } catch (Exception ex) {
        Logger.Error(ex, "ProjectManager(GetTeamMembersWithRoles)", teamId);
        throw;
      }
    }

    public TeamMemberModel GetTeamMemberWithRoles(int id) {
      var m = Ctx.TeamMembers
        .Single(t => t.Id == id);
      var tm = new TeamMemberModel {
          Member = m.Member,
          TeamMember = m,
          Projects = null,
          Roles = new List<TeamRole>(new [] { m.Role }),
          Profile = m.Member.Profile
        };
      return tm;
    }

    /// <summary>
    /// Add the member with <paramref name="userId"/> to the specified team.
    /// </summary>
    /// <param name="teamId"></param>
    /// <param name="userId"> </param>
    /// <returns></returns>
    public TeamMember AddMemberToTeam(int teamId, int userId) {
      var user = Manager<UserManager>.Instance.GetUser(userId);
      try {
        var team = Ctx.Teams.Find(teamId);
        if (team.Members.Any(m => m.Member.Id == userId)) {
          // we do not add an existing member again
          return null;
        }
        var newMember = new TeamMember {
          Member = user,
          Team = team,
          Role = null
        };
        // The role of the new member has not been added to the role table
        if (newMember.Role == null) {
          // create a new role in the team, that matches the minimum access role with the contributor roles
          var newRole = new TeamRole {
            Team = team,
            ContributorRoles = ContributorRole.Other
          };
          // add new team role
          Ctx.TeamRoles.Add(newRole);
          // and assign to the new member (this might raise the members rights at all, but its fine if the team lead makes such a decision)
          newMember.Role = newRole;
        }
        Ctx.TeamMembers.Add(newMember);
        SaveChanges();
        return newMember;
      } catch (Exception ex) {
        Logger.Error(ex, "ProjectManager(AddMemberToTeam)", teamId, userId);
        throw;
      }
    }

    /// <summary>
    /// Remove a team's member and optionally send email to user and team lead.
    /// </summary>
    /// <param name="memberId"></param>
    /// <param name="userName"></param>
    /// <param name="force"></param>
    /// <param name="sendMessages"></param>
    public bool RemoveMemberFromTeam(int memberId, string userName, bool force, bool sendMessages = true) {
      var teamLead = GetCurrentUser(userName);
      // check whether the lead is removing a member
      var member = GetTeamMember(memberId);
      // cannot remove lead
      if (member.TeamLead) return false;
      Ctx.LoadProperty(member, teamMember => teamMember.Team);
      var teamName = member.Team.Name;
      var leadofMember = GetTeamLeader(member.Team.Id);
      // only lead can remove
      if (leadofMember.UserName != teamLead.UserName) return false;
      try {
        // get and remove immediately
        var sel = Ctx.TeamMembers
                    .FirstOrDefault(m => m.Id == memberId && (m.Pending || force));
        if (sel == null) return false; // not found, ignore
        var userId = sel.Member.Id;
        Ctx.TeamMembers.Remove(sel);
        SaveChanges();
        if (!sendMessages) return true;
        // send a message to both, member and team leader
        var user = Ctx.Users.Single(u => u.Id == userId);
        // to user
        UserManager.Instance.SendExternalMail(teamLead, user, "RemovedFromTeam", true, new Dictionary<string, object> {
          {"UserName", user.UserName },
          {"TeamName", teamName}
        });
        // to teamleader
        UserManager.Instance.SendExternalMail(teamLead, user, "RemovedFromTeamLead", true, new Dictionary<string, object> {
          {"UserName", teamLead.UserName },
          {"TeamName", teamName},
          {"ContributorName", user.UserName}
        });
        SaveChanges();
      } catch (Exception ex) {
        Logger.Error(ex, "ProjectManager(RemoveMemberFromTeam)");
        throw;
      }
      return true;
    }

    /// <summary>
    /// Create a new team with default values.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="teamLeader"></param>
    /// <param name="sendMessages"></param>
    public Team CreateTeam(string name, string description, User teamLeader, bool sendMessages = true) {
      try {
        // assure right context
        teamLeader = Ctx.Users.Single(u => u.UserName == teamLeader.UserName);

        var team = new Team {
          Active = true,
          Name = name,
          Description = description
        };

        var lead = new TeamRole {
          Team = team,
          ContributorRoles = ContributorRole.Author
        };

        Ctx.TeamRoles.Add(lead);
        Ctx.Teams.Add(team);
        Ctx.TeamMembers.Add(new TeamMember {
          Member = teamLeader,
          Team = team,
          Role = lead,
          TeamLead = true
        });

        SaveChanges();
        if (sendMessages) {
          Ctx.Messages.Add(new Message {
            Sender = null,    // Null is the system itself
            Receiver = teamLeader,
            Subject = String.Format("You have created the new team {0}", team.Name),
            Body = String.Format("This message confirms that you have successfully created a team with the name {0}.",
            team.Name)
          });
          SaveChanges();
        }
        return team;
      } catch (Exception ex) {
        Logger.Error(ex, "ProjectManager(CreateTeam)", name, description);
        throw;
      }
    }

    public IEnumerable<User> GetUserNotAlreadyInTeam(string userName, int teamId) {
      var users = Ctx.Users
          .Where(u => u.UserName.Contains(userName) && !u.Teams.Any(t => t.Member.Id == u.Id && t.Team.Id == teamId));
      return users;
    }

    public IEnumerable<TeamOverviewModel> GetTeamMembersOfAllTeamsForUser(User user) {
      var members = Ctx.TeamMembers
        .Include(t => t.Team.Projects)
        .Where(m => m.Member.Id == user.Id)
        .GroupBy(m => m.Role.Team)
        .Select(g => new TeamOverviewModel {
          Team = g.Key,
          Members = g.Select(m => m.Member),
          Roles = g.Select(x => x.Role),
          CurrentUserIsLead = g.Key.Members.Any(m => m.TeamLead && m.Member.Id == user.Id)
        })
        .OrderByDescending(m => m.Team.CreatedAt);
      return members;
    }

    public IEnumerable<TeamMember> GetTeamMembersOfOpus(Opus opus) {
      var teamMembers = Ctx.Teams
        .Include(m => m.Members)
        .Include(m => m.Members.Select(r => r.Member))
        .Include(m => m.Members.Select(r => r.Role))
        .Include(m => m.Members.Select(r => r.Member).Select(r => r.Roles))
        .First(tr => tr.Id == opus.Project.Team.Id)
        .Members;
      return teamMembers;
    }

    public User GetTeamLeader(int teamId) {
      var leader = GetTeamLeaderAsMember(teamId);
      return ((leader == null) ? null : leader.Member);
    }

    /// <summary>
    /// Return all teams where the user is member (regardless of role).
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public IEnumerable<Team> GetTeamMemberships(string userName) {
      return Ctx.Teams.Where(t => t.Members.Any(m => m.Member.UserName == userName));
    }

    public bool UserIsTeamLead(int opusId, string userName) {
      try {
        var opus = GetOpus(opusId, userName);
        return opus.Project.Team.TeamLead.UserName == userName;
      } catch (Exception) {
        // not even member
        return false;
      }
    }

    public TeamMember GetTeamLeaderAsMember(int teamId) {
      //var team = Ctx.Teams
      //  .Include(m => m.Members.Select(r => r.Member))
      //  .Include(m => m.Members.Select(r => r.Role))
      //  .FirstOrDefault(t => t.Id == teamId);
      //if (team == null) return null;
      //var leaderRole = Ctx.TeamRoles
      //  .Include(m => m.Members)
      //  .Include(m => m.Members.Select(r => r.Member))
      //  .Include(m => m.Members.Select(r => r.Role))
      //  .First(tr => tr.Members.Any(m => m.TeamLead) && tr.Team.Id == team.Id);
      //var leader = team.Members
      //  .FirstOrDefault(t => t.Role == leaderRole && t.TeamLead);
      var leader = Ctx.Teams.Find(teamId).Members.First(m => m.TeamLead);
      return leader;
    }

    public Dictionary<string, List<ContributorMatrix>> GetContributorMatrixAssigned(int projectId) {
      var roles = Ctx.Projects
        .Include(m => m.Team.Members.Select(t => t.Member))
        .Include(m => m.Team.Members.Select(t => t.Member.Profile))
        .Include(m => m.Team.Members.Select(t => t.Member.Profile.ContributorMatrix))
        .First(p => p.Id == projectId)
        .Team
        .Members
        .Select(m => m)
        // TODO: v.Member.Profile.ContributorMatrix is wrong! it must be the collection of flags from v.Role.ContributorRoles!
        .ToDictionary(k => k.Member.UserName, v => v.Member.Profile.ContributorMatrix);
      return roles;
    }

    public ManagerResult DeleteTeam(int id, string userName) {
      var team = Ctx.Teams.Find(id);
      var mr = new ManagerResult("ProjectManager");
      if (team != null) {
        if (team.Projects.Any()) {
          mr.Text = ControllerResources.ProjectManager_DeleteTeam_Cannot_delete__team_runs_active_projects;
        }
        if (GetTeamLeader(id).UserName != userName) {
          mr.Text = ControllerResources.ProjectManager_DeleteTeam_Only_the_team_leader_can_delete_a_team;
        }
        Ctx.Teams.Remove(team);
        SaveChanges();
        mr.Text = ControllerResources.ProjectManager_DeleteTeam_Team_deleted;
      } else {
        mr.Text = ControllerResources.ProjectManager_DeleteTeam_Team_not_found;
      }
      return mr;
    }

    public ManagerResult DeactivateTeam(int id, string userName) {
      var team = Ctx.Teams.Find(id);
      var mr = new ManagerResult("ProjectManager");
      if (team != null) {
        if (team.Projects.Any()) {
          mr.Text = ControllerResources.ProjectManager_DeleteTeam_Cannot_delete__team_runs_active_projects;
        }
        if (GetTeamLeader(id).UserName != userName) {
          mr.Text = ControllerResources.ProjectManager_DeleteTeam_Only_the_team_leader_can_delete_a_team;
        }
        team.Active = false;
        SaveChanges();
        mr.Text = ControllerResources.ProjectManager_DeleteTeam_Team_deleted;
      } else {
        mr.Text = ControllerResources.ProjectManager_DeleteTeam_Team_not_found;
      }
      return mr;
    }

    public void ConfirmProposal(int id, int opusId, bool confirm, string userName) {
      var t = Ctx.TeamMembers
        .Include(m => m.Role)
        .Include(m => m.Team)
        .Include(m => m.Team.Members)
        .FirstOrDefault(tm => tm.Id == id && tm.Member.UserName == userName);
      if (t != null) {
        t.Pending = !confirm;
        var opus = GetOpusInternal(opusId);
        var ratioForUser = opus.ContributorRatios.SingleOrDefault(r => r.Contributor.UserName == t.Member.UserName);
        if (ratioForUser != null) {
          ratioForUser.Confirmed = confirm;
          SaveChanges();
        }
      }
    }

    # endregion

  }
}