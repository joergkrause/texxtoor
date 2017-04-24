using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.ViewModels.Author {

  /// <summary>
  /// Handles all values for a proposal made by lead author
  /// </summary>
  public class ContributionProposal {

    public TeamMember Member { get; set; }

    public IDictionary<Opus, ContributorRatio> PendingProposals { get; set; }

    public IList<Opus> Memberships { get; set; }

  }
}
