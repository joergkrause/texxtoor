using System;
using System.Data;
using System.Linq.Expressions;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.DataModels.Context {


  public interface IUnitOfWork {

    int Save();

    void ApplyValues<T>(T entity) where T : class;

    void Detach(object entity);

    void Attach(object entity);

    object Clone(object entity);

    void LoadProperty(object entity, string navProperty);

    void LoadProperty<T>(T entity, Expression<Func<T, object>> expression) where T : EntityBase;

    void LoadProperty<T>(T entity, params Expression<Func<T, object>>[] expressions) where T : EntityBase;

    void UndoChanges();

    void DeleteObject(object entity);

    IDbTransaction BeginTransaction();

  }


}
