using System;
using System.Linq;
using System.Collections.Generic;
using Texxtoor.DataModels.Models;
using System.ComponentModel.DataAnnotations;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.ViewModels.Author {

  /// <summary>
  /// Controls the creation, editing, and confirmation of shares between contributors
  /// </summary>
  public class ContributorShares {

    [Required]
    public Opus Book { get; set; }

    public IList<TeamMember> TeamMembers { get; set; }

    public bool IsTeamLead(string userName) {
      return TeamMembers.Any(tm => tm.TeamLead && tm.Member.UserName == userName);
    }

    /// <summary>
    /// Checks for a user whether there is a ratio entry and the UseRatio field is not yet set.
    /// </summary>
    /// <param name="userName">User Logon Name</param>
    /// <returns>Return <c>true</c> if the user has a ratio and not yet confirmed. <c>False</c> means he has confirmed. <c>null</c> means there is nothing to confirm.</returns>
    public bool? HasPendingConfirmation(string userName) {
      var cr = GetContributorRatio(userName);
      var tm = TeamMembers.Single(t => t.Member.UserName == userName);
      if (cr != null) {
        return tm.Pending;
      } else {
        return null;
      }
    }

    public ContributorRatio GetContributorRatio(string userName) {
      var cr = Book.ContributorRatios.FirstOrDefault(c => c.Contributor.UserName == userName);
      return cr;
    }
  }
}
