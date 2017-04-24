using System;
using System.Collections.Generic;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.ViewModels.Author {
  public class MarketingSummary {
    // Number of projects as a lead author
    public int ProjectsAsLeader { get; set; }

    // Number of projects as a member
    public int ProjectsAsMember { get; set; }

    // the next few milestones of all active projects
    public List<DateTime> Milestones { get; set; }

    // all projects for further reference
    public List<Project> Projects { get; set; }

  }
}
