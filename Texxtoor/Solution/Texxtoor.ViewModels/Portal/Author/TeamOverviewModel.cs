using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.ViewModels.Author {
  
  public class TeamOverviewModel {

    public Project Project { get; set; }
    public Team Team { get; set; }
    public IEnumerable<TeamRole> Roles { get; set; }
    public bool CurrentUserIsLead { get; set; }
    public IEnumerable<User> Members { get; set; }
    
    [FilterUIHint("StringFilter", "MVC")]
    [Display(ResourceType = typeof (ModelResources), Name = "TeamOverviewModel_TeamName_Team_Name")]
    public string TeamName {
      get {
        return Team.Name;
      } 
    }

    public Dictionary<string, List<ContributorMatrix>> AssignedRoleMatrix { get; set; }
    public Dictionary<string, List<ContributorMatrix>> AllUserRoleMatrix { get; set; }
    public Dictionary<string, List<ContributorMatrix>> RemovedRoleMatrix { get; set; }
    public Dictionary<string, List<ContributorMatrix>> AvailableRoleMatrix { get; set; }
  }

  public class TeamMemberModel {

    public TeamMemberModel() {
      Projects = new List<Project>();
    }

    public User Member { get; set; }
    public TeamMember TeamMember { get; set; }
    public UserProfile Profile { get; set; }
    public IEnumerable<TeamRole> Roles { get; set; }
    public List<Project> Projects { get; set; }

    public IEnumerable<object> GetInterSectProjects() {

      return null;
    }
  }
}
