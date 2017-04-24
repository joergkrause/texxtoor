using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Metadata.Edm;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Linq.Expressions;
using Texxtoor.Editor.Models;
using Texxtoor.Models;
using Texxtoor.Models.BaseEntities;

namespace Texxtoor.Editor.Context {
  public class EditorContext : DbContext {

    public EditorContext()
      : base("EditorContext") { }

    public EditorContext(string cnt)
      : base(cnt) { }

    public DbSet<Element> Elements { get; set; }

    public DbSet<Resource> Resources { get; set; }

    public DbSet<Document> Documents { get; set; }

    public DbSet<Term> Terms { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder) {
      modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
      base.OnModelCreating(modelBuilder);
    }

    # region API

    public void ApplyValues<T>(T entity) where T : class {
      var ctx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext;
      var eset = ctx.MetadataWorkspace.GetEntityContainer(ctx.DefaultContainerName, DataSpace.CSpace)
        .BaseEntitySets.Single(es => es.ElementType.Name == typeof(T).Name);
      var key = ctx.CreateEntityKey(eset.Name, entity);
      ctx.GetObjectByKey(key);
      ctx.ApplyCurrentValues<T>(eset.Name, entity);
    }

    public void Detach(object entity) {
      ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.Detach(entity);
    }

    public void Attach(object entity) {
      ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.Attach(entity as IEntityWithKey);
    }

    public void LoadProperty(object entity, string navProperty) {
      ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.LoadProperty(entity, navProperty);
    }

    public void LoadProperty<T>(T entity, Expression<Func<T, object>> expression) where T: EntityBase {
      Expression exp = expression.Body;
      string name = null;
      if (exp.NodeType == ExpressionType.MemberAccess) {
        name = ((MemberExpression)exp).Member.Name;
      }
      if (exp.NodeType == ExpressionType.Convert) {
        name = ((MemberExpression)((UnaryExpression)exp).Operand).Member.Name;

      }
      if (!String.IsNullOrEmpty(name)) {
        LoadProperty(entity, name);
      }
    }
    # endregion
  }

}