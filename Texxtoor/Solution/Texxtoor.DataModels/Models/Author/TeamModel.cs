using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Models.Author {

  /// <summary>
  /// Defines a team. A team can run multiple projects and can have any number of members.
  /// </summary>
  [Table("Teams", Schema = "Author")]
  public class Team : EntityBase {
    
    [StringLength(150)]
    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "Team_Name_Team_s_Name", Description="Team_Name_Team_s_Name_Helptext")]
    [Watermark(typeof(ModelResources), "Team_Watermark_Name")]
    [FilterUIHint("StringFilter", "MVC")]
    public string Name { get; set; }

    [StringLength(2048)]
    [Display(ResourceType = typeof(ModelResources), Name = "Team_Description_Description", Description="Team_Description_Description_Helptext")]
    [UIHint("TextArea")]
    [AdditionalMetadata("Rows", 3)]
    [AdditionalMetadata("Cols", 55)]
    [Watermark(typeof(ModelResources), "Team_Watermark_Description")]
    [FilterUIHint("StringFilter", "MVC")]
    public string Description { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Team_Active_Team_is_active", Description="Team_Active_Team_is_active_Helptext")]
    [ScaffoldColumn(false)]
    public bool Active { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Team_Image_Image_for_homepage", Description="Team_Image_Image_for_homepage_Helptext")]
    [ScaffoldColumn(false)]
    public byte[] Image { get; set; }

    // relations
    [Display(ResourceType = typeof(ModelResources), Name = "Team_Members_Team_s_Members", Description="Team_Members_Team_s_Members_Helptext")]
    public virtual IList<TeamMember> Members { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "Team_Projects_Related_Projects", Description="Team_Projects_Related_Projects_Helptext")]
    public virtual IList<Project> Projects { get; set; }

    [NotMapped]
    public User TeamLead {
      get {
        try {
          return Members.Single(m => m.TeamLead).Member;
        } catch (Exception) {
          return null;
        }
      }
    }
  }

  /// <summary>
  /// A member of team must have a role and must be part of team. He has always a relation to a regular registered user.
  /// </summary>
  [Table("TeamMembers", Schema = "Author")]
  public class TeamMember : EntityBase {

    public TeamMember() {
      Pending = true;
    }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "TeamMember_Member_Team_Member", Description="TeamMember_Member_Team_Member_Helptext")]
    public virtual User Member { get; set; }

    [Required]
    [Display(ResourceType = typeof(ModelResources), Name = "TeamMember_Role_Role_in_Team", Description="TeamMember_Role_Role_in_Team_Helptext")]
    public virtual TeamRole Role { get; set; }

    [Display(ResourceType = typeof(ModelResources), Name = "TeamMember_Team_Related_Team", Description="TeamMember_Team_Related_Team_Helptext")]
    public Team Team { get; set; }
    /// <summary>
    /// User must confirm its membership, as long as its not confirmed the membership is "pending".
    /// </summary>
    /// <remarks>
    /// Contributors offering their work can be selected by an author. Once selected they become part of a team, however, the membership remains pending.
    /// </remarks>    
    [Display(ResourceType = typeof(ModelResources), Name = "TeamMember_Pending_Membership_pending", Description="TeamMember_Pending_Membership_pending_Helptext")]
    public bool Pending { get; set; }

    /// <summary>
    /// This member is teamlead. Only one member can be the team's lead.
    /// </summary>
    [Display(ResourceType = typeof(ModelResources), Name = "TeamMember_TeamLead_Team_Lead", Description="TeamMember_TeamLead_Team_Lead_Helptext")]
    public bool TeamLead { get; set; }


    public IEnumerable<string> GetLocalizedContributorRoles() {
      if (Role == null) {
        return null;
      }
      var localizedRoles = new List<string>();
      foreach (var role in Role.ContributorRoles.ToEnumerable()) {
        var rs = Enum.GetValues(typeof(ContributorRole)).Cast<ContributorRole>();
        localizedRoles.AddRange(from r in rs where role.HasFlag(r) select typeof(ContributorRole).GetField(r.ToString()).GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().Single() into attr select attr.GetName());
      }
      return localizedRoles;
    }


  }

  /// <summary>
  /// A role for a member in a specific team. Roles can be pre-defined and assigned to multiple members.
  /// </summary>
  [Table("TeamRoles", Schema="Author")]
  public class TeamRole : EntityBase {

    [Required]
    [Display(Name = "Team")]
    public Team Team { get; set; }

    /// <summary>
    /// A flagged collection of roles.
    /// </summary>
    [Required]
    [Display(Name = "Roles")]
    public ContributorRole ContributorRoles { get; set; }

    [Display(Name = "Role Members")]
    public virtual List<TeamMember> Members { get; set; }

  }

}
