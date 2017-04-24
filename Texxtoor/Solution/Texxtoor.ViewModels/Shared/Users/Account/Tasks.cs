using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.ViewModels.Shared.Users.Account {
  /// <summary>
  /// Handle tasks for user to show items that need immediate action
  /// </summary>
  public class Tasks {

    public IList<Opus> Texts { get; set; }

    public IList<TeamMember> TeamMemberPending { get; set; }

    public IList<Milestone> MilestonesDue { get; set; }

    public IList<Milestone> MilestonesNotDone { get; set; }

    public IList<Invoice> Invoices { get; set; }

    public IList<Project> Projects { get; set; }


  }
}
