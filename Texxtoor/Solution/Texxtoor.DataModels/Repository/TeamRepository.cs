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
using Texxtoor.DataModels.Models.Author;

namespace Texxtoor.DataModels.Repository {


  /// <summary>
  /// These are low level functions to handle safe database calls. They are used from business layer only.
  /// </summary>
  /// <remarks>
  /// Do not expose these functions to the UI or any other layer but business layer.
  /// </remarks>
  public class TeamRepository : BaseRepository<TeamRepository> {
    public TeamRepository() {

    }

    public Team Get(int id) {
      return Ctx.Teams.Find(id);
    }

    public Team GetForMember(int id, string userName) {
      return Ctx.Teams
        .SingleOrDefault(p => p.Id == id && p.Members.Any(m => m.Member.UserName == userName));
    }

    public bool Insert(Team p) {
      return Insert<Team>(p);
    }

    public bool Delete(Team p) {
      return Delete<Team>(p);
    }

    public bool Update(Team p) {
      return Update<Team>(p);
    }

    public bool InsertOrUpdate(Team p) {
      return InsertOrUpdate<Team>(p);
    }



  }
}
