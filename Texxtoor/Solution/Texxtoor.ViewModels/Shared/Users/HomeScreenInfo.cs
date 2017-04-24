
using System.Collections.Generic;
using Texxtoor.DataModels.Models.Content;

namespace Texxtoor.ViewModels.Users {

  /// <summary>
  /// Info Block on index / Home
  /// </summary>
  public class HomeScreenInfo {

    public int Authors { get; set; }
    public int Variations { get; set; }
    public int Groups { get; set; }
    public int Items { get; set; }

    public int ProjectPublished { get; set; }
    public int TeamsLeading { get; set; }
    public int TeamsContributing { get; set; }
    public int TextsPublishable { get; set; }
    public int TextsPublished { get; set; }

    public int Products { get; set; }
    public int Works { get; set; }
    public int Editables { get; set; }
    public int Memberships { get; set; }
    public string ProfileExists { get; set; }
    public int MessageCount { get; set; }
    public int ArchiveCount { get; set; }
    public int OrderCount { get; set; }

    public int ProjectsCount { get; set; }
    public IEnumerable<Project> ProjectsEditable { get; set; }
    public IEnumerable<Opus> BooksEditable { get; set; }

    public int ProjectsAsLeader { get; set; }
  }
}