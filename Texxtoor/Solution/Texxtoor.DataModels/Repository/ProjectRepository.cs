using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texxtoor.BaseLibrary.Core.Logging;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Repository {


  /// <summary>
  /// These are low level functions to handle safe database calls. They are used from business layer only.
  /// </summary>
  /// <remarks>
  /// Do not expose these functions to the UI or any other layer but business layer.
  /// </remarks>
  public class ProjectRepository : BaseRepository<ProjectRepository> {
    public ProjectRepository() {

    }

    public Project Get(int id) {
      return Ctx.Projects.Find(id);
    }

    public Project GetForOwner(int id, string userName) {
      return Ctx.Projects
        .Include(p => p.Team)
        .Include(p => p.Team.Members)
        .SingleOrDefault(p => p.Id == id && p.Team.Members.Any(m => m.Member.UserName == userName && m.TeamLead));
    }

    public Project GetForMember(int id, string userName) {
      return Ctx.Projects
        .Include(p => p.Team)
        .Include(p => p.Team.Members)
        .SingleOrDefault(p => p.Id == id && p.Team.Members.Any(t => t.Member.UserName == userName));
    }

    public bool Insert(Project p) {
      return Insert<Project>(p);
    }

    public bool Delete(Project p) {
      return Delete<Project>(p);
    }

    public bool Update(Project p) {
      return Update<Project>(p);
    }

    public bool InsertOrUpdate(Project p) {
      return InsertOrUpdate<Project>(p);
    }



  }
}
