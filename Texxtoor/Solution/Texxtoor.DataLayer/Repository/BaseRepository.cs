using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.DataModels.Context;

namespace Texxtoor.DataModels.Repository {

  /// <summary>
  /// The repository provides low level database access functions to keep the business layer clean and sleak.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class BaseRepository<T> : Singleton<T> 
    where T : BaseRepository<T>, new() {

    private IUnitOfWork _ctx;

    public BaseRepository() {

    }

    protected BaseRepository(IUnitOfWork unitOfWork) {
      _ctx = unitOfWork as PortalContext;
    }

    protected PortalContext Ctx {
      get {
        return (_ctx ?? UnitOfWorkFactory.GetIUnitOfWorkContext<PortalContext>()) as PortalContext;
      }
    }

    protected bool Insert<TEntity>(TEntity p)
      where TEntity : EntityBase {
      Ctx.Entry(p).State = EntityState.Added;
      return Ctx.SaveChanges() == 1;
    }

    protected bool Delete<TEntity>(TEntity p)
      where TEntity : EntityBase {
      Ctx.Entry(p).State = EntityState.Deleted;
      return Ctx.SaveChanges() == 1;
    }

    protected bool Update<TEntity>(TEntity p)
      where TEntity : EntityBase {
      Ctx.Entry(p).State = EntityState.Modified;
      return Ctx.SaveChanges() == 1;
    }

    protected bool InsertOrUpdate<TEntity>(TEntity p)
    where TEntity : EntityBase {
      Ctx.Entry(p).State = p.Id == 0 ? EntityState.Added : EntityState.Modified;
      return Ctx.SaveChanges() == 1;
    }

  }
}
